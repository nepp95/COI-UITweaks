using System;

using UITweaks.Patches;

using Mafi;
using Mafi.Collections;
using Mafi.Core.Game;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Mafi.Unity.Ui.Hud;

namespace UITweaks;

public sealed class UITweaksMod : IMod
{
    private HudPatch _patch;
    public ModManifest Manifest { get; }
    public bool IsUiOnly => true;
    public Option<IConfig> ModConfig => Option<IConfig>.None;
    public ModJsonConfig JsonConfig { get; }

    public UITweaksMod(ModManifest manifest)
    {
        Manifest = manifest;
        JsonConfig = new ModJsonConfig(this);
    }

    void IMod.RegisterPrototypes(ProtoRegistrator registrator)
    {
    }

    void IMod.RegisterDependencies(
        DependencyResolverBuilder builder,
        ProtosDb protos,
        bool loaded)
    {
    }

    void IMod.EarlyInit(DependencyResolver resolver)
    {
    }

    void IMod.MigrateJsonConfig(VersionSlim version, Dict<string, object> values)
    {
    }

    void IMod.Initialize(DependencyResolver resolver, bool loaded)
    {
        try
        {
            _patch = new HudPatch(JsonConfig);
            _patch.Install(resolver.Resolve<PinnedProductsHudManager>());
            JsonConfig.OnValueChanged += OnConfigChanged;
            Log.Info("UITweaks: initialized (UI only).");
        }
        catch (Exception error)
        {
            _patch?.Dispose();
            _patch = null;
            Log.Error("UITweaks: could not initialize; " + error);
        }
    }

    private void OnConfigChanged(string name)
    {
        _patch?.Refresh();
    }

    public void Dispose()
    {
        JsonConfig.OnValueChanged -= OnConfigChanged;
        _patch?.Dispose();
        _patch = null;
    }
}
