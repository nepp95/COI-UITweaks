using System;
using System.Reflection;

using UITweaks.Layout;
using UITweaks.UI;

using HarmonyLib;
using Mafi;
using Mafi.Core.Mods;
using Mafi.Unity.Ui.Hud;

namespace UITweaks.Patches;

// These private member references are checked against the installed game at startup.
// No game method bodies are copied or replaced on disk.
internal sealed class HudPatch : IDisposable
{
    private const string PatchId = "local.uitweaks.resourcecolumns";
    private static HudPatch _active;
    private readonly Harmony _harmony = new Harmony(PatchId);
    private readonly ModJsonConfig _config;
    private readonly FieldInfo _list;
    private readonly FieldInfo _switchCount;
    private readonly FieldInfo _dragging;
    private readonly PropertyInfo _count;
    private readonly PropertyInfo _columns;
    private readonly MethodInfo _setColumns;
    private readonly MethodInfo _alternate;
    private readonly MethodInfo _validate;
    private readonly MethodInfo _endReorder;
    private readonly FieldInfo _managerPanel;
    private HudLayout _layout;
    private LayoutController _controller;
    private ColumnControls _controls;
    private bool _failed;
    private bool _installed;

    internal HudPatch(ModJsonConfig config)
    {
        _config = config;

        var hud = typeof(PinnedProductsHud);
        _list = Required(AccessTools.Field(hud, "m_productsColumn"));
        _switchCount = Required(AccessTools.Field(hud, "m_countAtDualColumnSwitch"));
        _validate = Required(AccessTools.DeclaredMethod(hud, "validateLayout", Type.EmptyTypes));
        _alternate = Required(AccessTools.DeclaredMethod(hud, "alternateChildrenBackground", Type.EmptyTypes));

        var listType = _list.FieldType;
        _dragging = Required(AccessTools.Field(listType, "m_isReordering"));
        _count = Required(AccessTools.Property(listType, "Count"));
        _columns = Required(AccessTools.Property(listType, "Columns"));
        _setColumns = Required(AccessTools.DeclaredMethod(listType, "SetColumns", new[] { typeof(int) }));
        _endReorder = Required(AccessTools.DeclaredMethod(listType, "endReorder"));
        _managerPanel = Required(AccessTools.Field(typeof(PinnedProductsHudManager), "m_pinnedProductsPanel"));

        if (_switchCount.FieldType != typeof(int) || _dragging.FieldType != typeof(bool)
            || _count.PropertyType != typeof(int) || _columns.PropertyType != typeof(int)
            || _validate.ReturnType != typeof(void) || _setColumns.ReturnType != typeof(void))
        {
            throw new NotSupportedException("The resource panel API has changed.");
        }
    }

    private static T Required<T>(T member) where T : MemberInfo
    {
        return member ?? throw new MissingMemberException("The installed game has an unsupported resource panel API.");
    }

    internal void Install(PinnedProductsHudManager manager)
    {
        if (_active != null)
        {
            throw new InvalidOperationException("UITweaks is already active.");
        }

        _active = this;
        _installed = true; // Also permits cleanup if the second patch fails.
        _harmony.Patch(
            _validate,
            prefix: new HarmonyMethod(typeof(HudPatch), nameof(BeforeValidate)),
            postfix: new HarmonyMethod(typeof(HudPatch), nameof(AfterValidate)));
        _harmony.Patch(_endReorder, postfix: new HarmonyMethod(typeof(HudPatch), nameof(AfterReorder)));
        Track((PinnedProductsHud)_managerPanel.GetValue(manager));
        Refresh();
    }

    private void Track(PinnedProductsHud hud)
    {
        if (_layout != null && ReferenceEquals(_layout.Hud, hud))
        {
            return;
        }

        _controls?.Dispose();
        _layout = new HudLayout(this, hud, _list.GetValue(hud));
        _controller = new LayoutController();
        _controls = new ColumnControls(
            hud,
            () => AdjustColumns(-1),
            () => AdjustColumns(1),
            RestoreAutomaticColumns);
    }

    private static bool BeforeValidate(PinnedProductsHud __instance)
    {
        var patch = _active;
        if (patch == null || patch._failed)
        {
            return true;
        }

        try
        {
            patch.Track(__instance);
            int desired = LayoutPolicy.GetColumns(
                patch._config.GetBool("enabled", true),
                patch._config.GetInt("fixed_columns", 0),
                patch._config.GetInt("rows_before_split", LayoutPolicy.DefaultRowsBeforeSplit),
                (int)patch._count.GetValue(patch._layout.List));
            return patch._controller.Apply(patch._layout, desired);
        }
        catch (Exception error)
        {
            patch.Fail(error);
            return true;
        }
    }

    private static void AfterValidate(PinnedProductsHud __instance)
    {
        var patch = _active;
        if (patch == null || patch._failed || patch._layout == null
            || !ReferenceEquals(patch._layout.Hud, __instance))
        {
            return;
        }

        try
        {
            bool manualOverride = patch._config.GetBool("enabled", true)
                && patch._config.GetInt("fixed_columns", 0) != 0;
            patch._controls.Refresh(patch._layout.Columns, manualOverride, patch._layout.IsDragging);
        }
        catch (Exception error)
        {
            patch.Fail(error);
        }
    }

    private void AdjustColumns(int direction)
    {
        if (_failed || _layout == null || _layout.IsDragging)
        {
            return;
        }

        SetColumnOverride(LayoutPolicy.AdjustColumns(_layout.Columns, direction));
    }

    private void RestoreAutomaticColumns()
    {
        if (_failed || _layout == null || _layout.IsDragging)
        {
            return;
        }

        SetColumnOverride(0);
    }

    private void SetColumnOverride(int columns)
    {
        if (!_config.TrySetValue("fixed_columns", columns, out var error)
            || !_config.TrySetValue("enabled", true, out error))
        {
            Log.Error("UITweaks: could not update resource columns. " + error);
            return;
        }

        Refresh();
    }

    private static void AfterReorder(object __instance)
    {
        var patch = _active;
        if (patch?._layout != null && ReferenceEquals(patch._layout.List, __instance))
        {
            patch.Refresh();
        }
    }

    internal void Refresh()
    {
        if (_failed || _layout == null)
        {
            return;
        }

        try
        {
            _validate.Invoke(_layout.Hud, null);
        }
        catch (Exception error)
        {
            Fail(error);
        }
    }

    private void Fail(Exception error)
    {
        _failed = true;

        try
        {
            _controls?.Dispose();
            _controls = null;
            _controller?.Restore(_layout);
        }
        catch (Exception restoreError)
        {
            Log.Error("UITweaks: restore failed. " + restoreError);
        }

        Log.Error("UITweaks: disabled for this session after a layout error. " + error);
    }

    public void Dispose()
    {
        if (!_installed)
        {
            return;
        }

        _installed = false;

        if (ReferenceEquals(_active, this))
        {
            _active = null;
        }

        _harmony.UnpatchAll(PatchId);
        _controls?.Dispose();
        _controls = null;

        try
        {
            if (_layout != null)
            {
                _controller.Restore(_layout);
                _validate.Invoke(_layout.Hud, null);
            }
        }
        catch (Exception error)
        {
            Log.Error("UITweaks: UI cleanup failed. " + error);
        }

        _layout = null;
    }

    private sealed class HudLayout : IColumnLayout
    {
        private readonly HudPatch _owner;
        internal readonly PinnedProductsHud Hud;
        internal readonly object List;

        internal HudLayout(HudPatch owner, PinnedProductsHud hud, object list)
        {
            _owner = owner;
            Hud = hud;
            List = list;
        }

        public bool IsDragging => (bool)_owner._dragging.GetValue(List);
        public int Columns => (int)_owner._columns.GetValue(List);

        public void SetColumns(int columns)
        {
            _owner._setColumns.Invoke(List, new object[] { columns });
            _owner._alternate.Invoke(Hud, null);
        }

        public void ResetAutomaticLayout()
        {
            SetColumns(1);
            _owner._switchCount.SetValue(Hud, -1);
        }
    }
}
