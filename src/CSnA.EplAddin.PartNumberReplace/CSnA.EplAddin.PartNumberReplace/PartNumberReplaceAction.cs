using CSnA.EplAddin.PartNumberReplace.ViewModels;
using CSnA.EplAddin.PartNumberReplace.Views;
using Eplan.EplApi.ApplicationFramework;
using Eplan.EplSDK.WPF;
using GeKtvi.Toolkit.WpfKit.Clipboard;
using System;

namespace CSnA.EplAddin.CustomItemNumbers
{
    public class PartNumberReplaceAction : IEplAction
    {
        public const string CommandName = "PartNumberReplace";

        public bool Execute(ActionCallingContext ctx)
        {
            Exception? result = null;

            var viewModel = new MainViewModel(
                new PartNumberReplace.Models.MainModel(),
                new PartNumberReplace.ClipboardService(new ClipboardHelperWpf())
            );

            var view = new MainView()
            {
                DataContext = viewModel
            };

            viewModel.Finished.Subscribe(e =>
            {
                result = e;
            });

            var factory = new DialogFactory(CommandName + "Dialog", view);

            new DialogManager().StartDialogModal(CommandName + "Dialog");

            if (result is not null)
                throw result;

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
