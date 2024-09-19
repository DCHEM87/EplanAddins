namespace CSnA.EplAddin.CustomItemNumbers
{
    internal interface IArrangeableItem
    {
        int PositionNumber { get; set; }
        string Identifier { get; }
        string Name { get; }
        int Chapter { get; }
        int SubChapter { get; }
    }
}