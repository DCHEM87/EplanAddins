using System.Diagnostics;

namespace CSnA.EplAddin.CustomItemNumbers
{
    [DebuggerDisplay("ArrangeableItem PositionNumber = {PositionNumber}, Name = {Name}")]
    internal class ArrangeableItem : IArrangeableItem
    {
        public int PositionNumber { get; set; }
        public string Identifier { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Chapter { get; set; }
        public int SubChapter { get; set; }
        public object? Source { get; private set; }

        public ArrangeableItem(object source) => Source = source;
        public ArrangeableItem() { }
    }
}
