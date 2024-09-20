using CSnA.EplAddin.PartNumberReplace.Models;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Windows.Input;

namespace CSnA.EplAddin.PartNumberReplace.ViewModels
{
    internal class MainViewModel : ReactiveObject
    {
        public ICommand Paste => _paste;
        public ICommand Replace => _replace;

        [Reactive] public string ErrorMessage { get; private set; } = string.Empty;

        public bool IsAutoReplaceEnabled { get => _mainModel.IsAutoReplaceEnabled; set => _mainModel.IsAutoReplaceEnabled = value; }

        public ObservableCollection<PartNumberModel> PartNumbers { get; } = [];

        private ReactiveCommand<Unit, Unit> _paste;
        private ReactiveCommand<Unit, Unit> _replace;
        private readonly MainModel _mainModel;
        private readonly ClipboardService _clipboardService;

        public MainViewModel(MainModel mainModel, ClipboardService clipboardService)
        {
            _mainModel = mainModel;
            _clipboardService = clipboardService;

            _mainModel.IsAutoReplaceEnabled = true;

            _paste = ReactiveCommand.Create(() => SetPartNumbers(clipboardService.GetData()));
            _paste.ThrownExceptions.Subscribe(e => ErrorMessage = e.Message);

            _mainModel.Replaced.Subscribe(x => PartNumbers.Remove(x));

            _replace = ReactiveCommand.Create(() => 
            {
                _mainModel.Replace(PartNumbers.ToArray());
                ErrorMessage = string.Empty;
            });
            _replace.ThrownExceptions.Subscribe(e => ErrorMessage = e.Message);
        }

        private void SetPartNumbers(PartNumberModel[] partNumberViewModels)
        {
            PartNumbers.Clear();
            PartNumbers.AddRange(partNumberViewModels);
        }
    }
}
