using System;
using System.Collections.Generic;
using System.Text;

namespace CSnA.EplAddin.CustomItemNumbers.Infos
{
    public class ArrangeInfo
    {
        public string ChapterPropertyName { get; set; } = string.Empty;
        public string SubChapterPropertyName { get; set; } = string.Empty;
        public string PrimarySortName { get; set; } = string.Empty;
        public string SecondarySortName { get; set; } = string.Empty;
        public int StartValue { get; set; }
        public int Increment { get; set; }
        public int FieldWidth { get; set; }
        public OrderPair[] ChapterSortOrderPairs { get; set; } = [];
        public OrderPair[] SubChapterSortOrderPairs { get; set; } = [];
        public string[] ChaptersToIgnore { get; set; } = [];
    }
}
