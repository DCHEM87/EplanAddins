using CSnA.EplAddin.BackupProjectOnLoad.Configs;
using Eplan.EplApi.ApplicationFramework;
using Eplan.EplApi.Base;
using Eplan.EplApi.DataModel;
using Eplan.EplApi.Gui;
using GeKtvi.Toolkit;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CSnA.EplAddin.BackupProjectOnLoad
{
    public class AddInModule : IEplAddIn, IEplAddInShadowCopy
    {
        private static ISOCode.Language _guiLanguage = new Languages().GuiLanguage.GetNumber();
        private Eplan.EplApi.ApplicationFramework.EventHandler? _eventHandler;

        public static string AddinPath { get; private set; } = string.Empty;
        private static Task? _backUpTask;
        private static object _lock = new();

        public bool OnRegister(ref bool LoadOnStart)
        {
            LoadOnStart = true;
            return true;
        }

        public bool OnUnregister()
        {
            return true;
        }

        public bool OnInit()
        {
            AddHandlers();
            return true;
        }

        public bool OnInitGui() => true;

        public bool OnExit() => true;

        private bool AddHandlers()
        {
            _eventHandler = new Eplan.EplApi.ApplicationFramework.EventHandler("onActionEnd.String.XPrjActionProjectOpen");
            _eventHandler.EplanNameEvent += delegate (IEventParameter parameter, string strNameOfEvent)
            {
                Debug.WriteLine("Project opened");
                _ = BackUpIfNeededAsync();
            };

            SystemEvents.PowerModeChanged += (s, e) =>
            {
                if (e.Mode == PowerModes.Resume)
                {
                    Debug.WriteLine("Woke up");
                    _ = BackUpIfNeededAsync();
                }
            };
#if DEBUG
            MultiLangString commandNameMultiString = new();
            commandNameMultiString.AddString(ISOCode.Language.L_ru_RU, TestBackUpAction.CommandName);
            commandNameMultiString.AddString(ISOCode.Language.L_en_US, TestBackUpAction.CommandName);
            string commandName = commandNameMultiString.GetStringToDisplay(_guiLanguage);
            if (string.IsNullOrEmpty(commandName))
                commandName = commandNameMultiString.GetString(ISOCode.Language.L_ru_RU);

            ContextMenuLocation oCtxLoc = new()
            {
                DialogName = "PmPageObjectTreeDialog",
                ContextMenuName = "1007"
            };

            bool isAdded = new ContextMenu().AddMenuItem(oCtxLoc, commandName, TestBackUpAction.CommandName, true, false);
            Debug.WriteLine($"Debug command added: {isAdded}");
#endif
            return true;
        }

        public void OnBeforeInit(string strOriginalAssemblyPath) =>
            AddinPath = Path.GetDirectoryName(strOriginalAssemblyPath);

        private async Task BackUpIfNeededAsync()
        {
            IEnumerable<ProjectInfo> currentProjects = new ProjectManager().OpenProjects.Select(x => x.ToInfo());
            Debug.WriteLine(nameof(BackUpIfNeededAsync));
            Task? oldTask;
            Task newTask;
            lock (_lock)
            {
                oldTask = _backUpTask;
                newTask = new(() => BackUpIfNeeded(currentProjects));
                _backUpTask = newTask;
            }

            if (oldTask is not null)
            {
                Debug.WriteLine("Awaiting old task");
                await oldTask;
                Debug.WriteLine("Old task finish");
            }

            Debug.WriteLine("Start new task");
            newTask.Start();
        }

        private void BackUpIfNeeded(IEnumerable<ProjectInfo> currentProjects)
        {
            Debug.WriteLine(nameof(BackUpIfNeededAsync));

            foreach (var project in currentProjects)
            {
                try
                {
                    new BackUpManager(AppConfigHelper.LoadConfig<BackUpConfig>("BackUpConfig.config")).BackUpIfNeeded(project);
                }
                catch (Exception e)
                {
                    LogError(e);
                }
                LogMessage($"{project.ProjectName} обработано резервирование");
            }

            Debug.WriteLine("End of: " + nameof(BackUpIfNeededAsync));
        }

        private void LogMessage(string message)
        {
            BaseException exc = new(message, MessageLevel.Message);
            exc.FixMessage();
        }

        private void LogError(Exception exception)
        {
            BaseException exc = new(exception.Message, MessageLevel.Error);
            exc.FixMessage();
        }

    }
}
