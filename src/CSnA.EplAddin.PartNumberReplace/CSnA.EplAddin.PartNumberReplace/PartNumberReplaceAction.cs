using CSnA.EplAddin.PartNumberReplace.ViewModels;
using CSnA.EplAddin.PartNumberReplace.Views;
using Eplan.EplApi.ApplicationFramework;
using Eplan.EplSDK.WPF.ThemingManager;
using GeKtvi.Toolkit.WpfKit.Clipboard;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

namespace CSnA.EplAddin.CustomItemNumbers
{
    public class PartNumberReplaceAction : IEplAction
    {
        public const string CommandName = "PartNumberReplace";

        public bool Execute(ActionCallingContext ctx)
        {
            var viewModel = new MainViewModel(
                new PartNumberReplace.Models.MainModel(),
                new PartNumberReplace.ClipboardService(new ClipboardHelperWpf())
            );

            var view = new MainView()
            {
                DataContext = viewModel
            };

            //var factory = new DialogFactory(CommandName + "Dialog", view);
            //new DialogManager().StartDialogModal(CommandName + "Dialog");

            WPFThemingManager.Instance.RegisterControl(view);
            var window = new Window
            {
                Title = "Замена номера детали",
                Content = view,
              //  ShowActivated = true,
                Width = 500,
            };

            //new WindowInteropHelper(window).Owner = Process.GetCurrentProcess().MainWindowHandle;

            window.ShowDialog();

            return true;
        }

        public bool OnRegister(ref string Name, ref int Ordinal)
        {
            Name = CommandName;
            Ordinal = 20;
            return true;
        }

        public void GetActionProperties(ref ActionProperties actionProperties) { }
    }
}
