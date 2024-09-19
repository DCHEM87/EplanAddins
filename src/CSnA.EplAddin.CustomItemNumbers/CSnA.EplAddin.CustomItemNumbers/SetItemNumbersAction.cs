using CSnA.EplAddin.CustomItemNumbers.Infos;
using Eplan.EplApi.ApplicationFramework;
using Eplan.EplApi.Base;
using Eplan.EplApi.DataModel;
using Eplan.EplApi.HEServices;
using GeKtvi.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSnA.EplAddin.CustomItemNumbers
{
    public class SetItemNumbersAction : IEplAction
    {

        public const string CommandName = "SetItemNumbers";
        private ArticleReference[]? _selectedArticleReferences;
        private NumbersArranger _numbersArranger = new();
        private ArrangeInfo? _arrangeInfo;

        public bool Execute(ActionCallingContext ctx)
        {
            _arrangeInfo = AppConfigHelper.LoadConfig<ArrangeInfo>("Addin.config", AddInModule.AddinPath + "\\Configs");

            _numbersArranger.StartValue = _arrangeInfo.StartValue;
            _numbersArranger.Increment = _arrangeInfo.Increment;
            _numbersArranger.FieldWidth = _arrangeInfo.FieldWidth;

            _selectedArticleReferences = new SelectionSet() { LockSelectionByDefault = true, LockProjectByDefault = true }.Selection
                .Where(x => x is MergedArticleReference)
                .Cast<MergedArticleReference>()
                .SelectMany(x => x.GetArticleReferences())
                .ToArray();

            var items = GetArrangeableItems();
            _numbersArranger.Arrange(items);
            SetArrangeableItems(items);

            return true;
        }

        public bool OnRegister(ref string Name, ref int Ordinal)
        {
            Name = CommandName;
            Ordinal = 20;
            return true;
        }

        public void GetActionProperties(ref ActionProperties actionProperties) { }

        private ArrangeableItem[] GetArrangeableItems() =>
            _selectedArticleReferences
                .Select(CreateArrangeInfo)
                .OfType<ArrangeableItem>()
                .ToArray();

        private ArrangeableItem? CreateArrangeInfo(ArticleReference articleReference)
        {
            if (_arrangeInfo == null)
                throw new InvalidOperationException("Arrange info is null");

            if (articleReference.Properties.ARTICLEREF_SUPPRESSINPARTSLIST.IsEmpty == false && articleReference.Properties.ARTICLEREF_SUPPRESSINPARTSLIST)
                return null;

            string chapter;
            string subChapter;
            try
            {
                chapter = GetPropertyValueAsString(articleReference.Properties[_arrangeInfo.ChapterPropertyName]);
                subChapter = GetPropertyValueAsString(articleReference.Properties[_arrangeInfo.SubChapterPropertyName]);
            }
            catch
            {
                new Decider().Decide(EnumDecisionType.eOkDecision, $"Problem with: {GetDesignation(articleReference)}", "", EnumDecisionReturn.eOK, EnumDecisionReturn.eOK);
                throw;
            }

            if (_arrangeInfo.ChaptersToIgnore.Contains(chapter) || string.IsNullOrEmpty(chapter))
                return null;

            var chapterPair = _arrangeInfo.ChapterSortOrderPairs.FirstOrDefault(x => x.Value == chapter);
            var SubChapterPair = _arrangeInfo.SubChapterSortOrderPairs.FirstOrDefault(x => x.Value == subChapter);

            int order = chapterPair is null ? -1 : chapterPair.SortOrder;
            int subOrder = SubChapterPair is null ? -1 : SubChapterPair.SortOrder;

            return new ArrangeableItem(articleReference)
            {
                Name = GetDesignation(articleReference),
                Identifier = GetPartNumber(articleReference),
                Chapter = order,
                SubChapter = subOrder
            };
        }

        private void SetArrangeableItems(IEnumerable<ArrangeableItem> items)
        {
            foreach (var item in items)
            {

                if (item.Source is ArticleReference itemSource)
                    SetPositionNumber(item, itemSource);
                else if (item.Source is CompositeArrangeableItem compositeItemSource)
                    foreach (var compositeItem in compositeItemSource.Source)
                        if (compositeItem is ArrangeableItem arrangeable)
                            if (arrangeable.Source is ArticleReference reference)
                                SetPositionNumber(arrangeable, reference);
                            else
                                ThrowInvalidItemSource();
                        else
                            ThrowInvalidItemSource();
                else
                    ThrowInvalidItemSource();

            }
        }

        private static void ThrowInvalidItemSource()
        {
            throw new InvalidArgumentException($"Item's source is not {nameof(ArticleReference)}");
        }

        private static void SetPositionNumber(ArrangeableItem item, ArticleReference itemSource)
        {
            itemSource.Properties.ARTICLEREF_POSNR = item.PositionNumber.ToString();
            itemSource.StoreToObject();
        }

        private string GetPartNumber(ArticleReference articleReference)
        {
            if (_arrangeInfo == null)
                throw new InvalidOperationException("Arrange info is null");

            string partNumber = articleReference.PartNr;
            return partNumber;
        }

        private string GetDesignation(ArticleReference articleReference)
        {
            if (_arrangeInfo == null)
                throw new InvalidOperationException("Arrange info is null");

            if (!articleReference.Properties[_arrangeInfo.PrimarySortName].IsEmpty)
                return GetPropertyValueAsString(articleReference.Properties[_arrangeInfo.PrimarySortName]);

            if (!articleReference.Properties[_arrangeInfo.SecondarySortName].IsEmpty)
                return GetPropertyValueAsString(articleReference.Properties[_arrangeInfo.SecondarySortName]);

            return string.Empty;
        }

        private string GetPropertyValueAsString(PropertyValue value)
        {
            if (value.IsEmpty)
                return string.Empty;
            if (value.Definition.Type == PropertyDefinition.PropertyType.String)
                return value;
            if (value.Definition.Type != PropertyDefinition.PropertyType.MultilangString)
                return string.Empty;

            MultiLangString val = value;
            string str = val.GetStringToDisplay(ISOCode.Language.L_ru_RU);
            if (string.IsNullOrEmpty(str))
                str = val.GetString(ISOCode.Language.L___);

            return str;
        }
    }
}
