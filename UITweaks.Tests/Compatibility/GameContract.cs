using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace UITweaks.Tests.Compatibility;

// Reads metadata only. Never loads game assemblies into the runtime or starts Unity.
internal static class GameContract
{
    internal static void Verify(string managedPath)
    {
        using var stream = File.OpenRead(Path.Combine(managedPath, "Mafi.Unity.dll"));
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();
        var types = metadata.TypeDefinitions.Select(metadata.GetTypeDefinition).ToArray();

        TypeDefinition Find(string name) => types.Single(t => metadata.GetString(t.Name) == name);

        void Field(TypeDefinition type, string name)
        {
            if (!type.GetFields().Select(metadata.GetFieldDefinition).Any(f => metadata.GetString(f.Name) == name))
            {
                throw new Exception($"Missing game field: {name}");
            }
        }

        void Method(TypeDefinition type, string name, int parameters)
        {
            var method = type.GetMethods().Select(metadata.GetMethodDefinition)
                .Single(m => metadata.GetString(m.Name) == name);
            int actual = method.GetParameters().Select(metadata.GetParameter).Count(p => p.SequenceNumber > 0);
            if (actual != parameters || method.RelativeVirtualAddress == 0 || (method.Attributes & MethodAttributes.Static) != 0)
            {
                throw new Exception($"Unsupported game method: {name}");
            }
        }

        var hud = Find("PinnedProductsHud");
        Field(hud, "m_productsColumn");
        Field(hud, "m_countAtDualColumnSwitch");
        Method(hud, "validateLayout", 0);
        Method(hud, "alternateChildrenBackground", 0);
        var columns = Find("ReorderableMultiColumns");

        if (columns.GetDeclaringType().IsNil || metadata.GetString(metadata.GetTypeDefinition(columns.GetDeclaringType()).Name) != "PinnedProductsHud")
        {
            throw new Exception("Unexpected nested column type.");
        }

        Field(columns, "m_isReordering");
        Method(columns, "get_Count", 0);
        Method(columns, "get_Columns", 0);
        Method(columns, "SetColumns", 1);
        Method(columns, "endReorder", 2);
        Field(Find("PinnedProductsHudManager"), "m_pinnedProductsPanel");
        Console.WriteLine($"PASS: installed game UI member contract ({metadata.GetAssemblyDefinition().Version}); metadata only.");
    }
}
