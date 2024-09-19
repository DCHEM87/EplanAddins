using CSnA.EplAddin.CustomItemNumbers;
using Eplan.EplApi.ApplicationFramework;
using Eplan.EplApi.Base;
using Eplan.EplApi.Gui;
using System;
using System.IO;
using System.Reflection;

namespace CSnA.EplAddin.PartNumberReplace
{
    public class AddInModule : IEplAddIn, IEplAddInShadowCopy
    {
        private static ISOCode.Language global_GuiLanguage = new Languages().GuiLanguage.GetNumber();
        public static string AddinPath { get; private set; } = string.Empty;

        public bool OnRegister(ref bool LoadOnStart)
        {
            LoadOnStart = true;
            return true;
        }

        public bool OnUnregister()
        {
            CreateContextMenu();
            return true;
        }

        public bool OnInit() => true;

        public bool OnInitGui() => CreateContextMenu();

        public bool OnExit() => true;

        private bool CreateContextMenu()
        {
            MultiLangString commandNameMultiString = new();
            commandNameMultiString.AddString(ISOCode.Language.L_ru_RU, "Test");
            commandNameMultiString.AddString(ISOCode.Language.L_en_US, "Test");
            string commandName = commandNameMultiString.GetStringToDisplay(global_GuiLanguage);
            if (string.IsNullOrEmpty(commandName))
                commandName = commandNameMultiString.GetString(ISOCode.Language.L_ru_RU);

            ContextMenuLocation oCtxLoc = new ContextMenuLocation
            {
                DialogName = "XPalTabTree",
                ContextMenuName = "1018"
            };

            new ContextMenu().AddMenuItem(oCtxLoc, commandName, PartNumberReplaceAction.CommandName, true, false);



            return true;
        }
        public void OnBeforeInit(string strOriginalAssemblyPath) =>
            AddinPath = Path.GetDirectoryName(strOriginalAssemblyPath);

    }
}
