using Eplan.EplApi.ApplicationFramework;
using Eplan.EplApi.Base;
using Eplan.EplApi.Gui;
using System.IO;

namespace CSnA.EplAddin.AutoDocumentationReferences
{
    public class AddInModule : IEplAddIn, IEplAddInShadowCopy
    {
        private static ISOCode.Language _guiLanguage = new Languages().GuiLanguage.GetNumber();
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
            commandNameMultiString.AddString(ISOCode.Language.L_ru_RU, "Добавить документацию как изделия");
            commandNameMultiString.AddString(ISOCode.Language.L_en_US, "Add docs as references");
            string commandName = commandNameMultiString.GetStringToDisplay(_guiLanguage);
            if (string.IsNullOrEmpty(commandName))
                commandName = commandNameMultiString.GetString(ISOCode.Language.L_ru_RU);

            ContextMenuLocation oCtxLoc = new()
            {
                DialogName = "PmPageObjectTreeDialog",
                ContextMenuName = "1007"
            };

            new ContextMenu().AddMenuItem(oCtxLoc, commandName, AddDocumentationReferencesAction.CommandName, true, false);

            return true;
        }

        public void OnBeforeInit(string strOriginalAssemblyPath) =>
            AddinPath = Path.GetDirectoryName(strOriginalAssemblyPath);
    }
}
