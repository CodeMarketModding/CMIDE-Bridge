using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using JavaMarket.App;
using JavaMarket.UI;

namespace CodeMarket.LiveIDE
{
    // Filter the visual tree only. The underlying project and its scripts remain intact.
    internal static class HiddenFolderView
    {
        private static readonly MethodInfo AddRows = AccessTools.Method(typeof(CodeExplorer), "AddRows");
        private static readonly MethodInfo CreateRow = AccessTools.Method(typeof(CodeExplorer), "CreateRow");
        private static readonly FieldInfo Rows = AccessTools.Field(typeof(CodeExplorer), "rows");

        public static void Install()
        {
            if (AddRows == null || CreateRow == null || Rows == null) throw new MissingMemberException("CodeExplorer row methods changed.");
            new Harmony("local.codemarket.liveide.hiddenfolders").Patch(AddRows, prefix: new HarmonyMethod(AccessTools.Method(typeof(HiddenFolderView), nameof(Prefix))));
        }

        private static bool Prefix(CodeExplorer __instance, CodeFolder folder)
        {
            var rows = (List<ExplorerRow>)Rows.GetValue(__instance);
            foreach (var child in folder.Children)
            {
                if (child is CodeFolder hidden && hidden.Name.StartsWith("_", StringComparison.Ordinal)) continue;
                rows.Add((ExplorerRow)CreateRow.Invoke(__instance, new object[] { child, rows.Count }));
                if (child is CodeFolder nested && nested.Expanded) Prefix(__instance, nested);
            }
            return false;
        }
    }
}
