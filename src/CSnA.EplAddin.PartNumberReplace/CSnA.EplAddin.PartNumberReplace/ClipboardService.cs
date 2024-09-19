using CSnA.EplAddin.PartNumberReplace.Models;
using GeKtvi.Toolkit.Clipboard;
using System.Linq;

namespace CSnA.EplAddin.PartNumberReplace
{
    internal class ClipboardService(ClipboardHelper clipboard)
    {
        private readonly ClipboardHelper _clipboard = clipboard;

        public PartNumberModel[] GetData()
        {
            var data = _clipboard.ParseClipboardData();

            if (data.All(x => x.Length <= 2) == false)
                return [];

            return data.Select(x => new PartNumberModel() { NewValue = x[0], OldValue = x[1] }).ToArray();
        }
    }
}
