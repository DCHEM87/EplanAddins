using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CSnA.EplAddin.CustomItemNumbers
{
    [DebuggerDisplay($$"""{{nameof(CompositeArrangeableItem)}} PositionNumber = {PositionNumber}, Name = {Name}""")]
    internal class CompositeArrangeableItem : IArrangeableItem
    {
        public int PositionNumber { get => Source.First().PositionNumber; set => Source.Select(x => x.PositionNumber = value).ToArray(); }
        public string Identifier { get => Source.First().Identifier; } 
        public string Name { get => Source.First().Name; } 
        public int Chapter { get => Source.First().Chapter; }
        public int SubChapter { get => Source.First().SubChapter; }
        public IEnumerable<IArrangeableItem> Source { get; }

        public CompositeArrangeableItem(IEnumerable<IArrangeableItem> source) => Source = source;
    }
}
