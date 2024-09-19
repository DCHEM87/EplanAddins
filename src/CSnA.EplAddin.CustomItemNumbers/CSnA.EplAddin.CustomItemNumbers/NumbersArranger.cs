using System.Collections.Generic;
using System.Linq;

namespace CSnA.EplAddin.CustomItemNumbers
{
    internal class NumbersArranger
    {
        public int StartValue { get; set; } = 1;
        public int Increment { get; set; } = 1;
        public int FieldWidth { get; set; } = 10;

        public void Arrange(IEnumerable<IArrangeableItem> items)
        {
            var sortedItems = items
                .GroupBy(x => x.Identifier)
                .Select(x => new CompositeArrangeableItem(x))
                .OrderBy(x => x.SubChapter)
                .ThenBy(x => x.Name)
                .GroupBy(x => x.Chapter)
                .OrderBy(x => x.Key)
                .ToArray();

            Dictionary<string, IArrangeableItem> setItems = new();
            int currentPosition = StartValue;

            foreach (var chapter in sortedItems)
            {
                foreach (var item in chapter)
                {
                    if (setItems.ContainsKey(item.Identifier))
                    {
                        item.PositionNumber = setItems[item.Identifier].PositionNumber;
                    }
                    else
                    {
                        item.PositionNumber = currentPosition;
                        currentPosition += Increment;
                        setItems.Add(item.Identifier, item);
                    }
                }
                currentPosition += FieldWidth;
            }
        }
    }
}
