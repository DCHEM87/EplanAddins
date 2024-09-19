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

        public IObservable<Exception?> Finished => _finished;

        [Reactive] public string ErrorMessage { get; private set; } = string.Empty;

        private Subject<Exception?> _finished = new();

        public ObservableCollection<PartNumberModel> PartNumbers { get; } = [];

        private ReactiveCommand<Unit, Unit> _paste;
        private ReactiveCommand<Unit, Unit> _replace;
        private readonly ClipboardService _clipboardService;

        public MainViewModel(MainModel mainModel, ClipboardService clipboardService)
        {
            _clipboardService = clipboardService;

            _paste = ReactiveCommand.Create(() => SetPartNumbers(clipboardService.GetData()));
            _paste.ThrownExceptions.Subscribe(e => ErrorMessage = e.Message);

            mainModel.Replaced.Subscribe(x => PartNumbers.Remove(x));

            _replace = ReactiveCommand.Create(() => 
            {
                mainModel.Replace(PartNumbers.ToArray());
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
