using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CSnA.EplAddin.PartNumberReplace.Models
{
    internal class PartNumberModel : ReactiveObject
    {
       [Reactive] public string OldValue { get; set; } = string.Empty;
       [Reactive] public string NewValue { get; set; } = string.Empty;
    }
}
