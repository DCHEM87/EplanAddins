using Eplan.EplApi.Base;
using Eplan.EplApi.DataModel;
using System;
using System.Linq;
using System.Text;

namespace CSnA.EplAddin.AutoDocumentationReferences
{
    internal static class PropertyExtensions
    {
        public static string AsPropertyString(this PropertyValue value)
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

        public static PropertyValue GetProperty(this UniversalPropertyList list, string id)
        {
            try
            {
                PropertyValue ret;

                int leftBraced = id.IndexOf('[');
                int rightBraced = id.IndexOf(']');
                if (leftBraced != -1 && rightBraced != -1)
                {
                    if (leftBraced >= rightBraced)
                        throw new InvalidOperationException("Property id contains invalid braces order");

                    string indS = id.Substring(leftBraced + 1, rightBraced - leftBraced - 1);

                    if (int.TryParse(indS, out int ind))
                    {
                        string propName = id.Substring(0, rightBraced - leftBraced + 1);
                        AnyPropertyId property = list.ExistingIds.FirstOrDefault(x => x.Definition.Id.AsText.ToString() == propName);
                        ret = list[property, ind];
                    }
                    else
                    {
                        throw new InvalidOperationException("Cant parse int from index");
                    }
                }
                else
                {
                    ret = list[id];
                }

                return ret;
            }
            catch (PropertyNotFoundException)
            {
                string? parent = GetParentName(list);

                throw new Exception();
            }
        }

        public static PropertyValue? TryGetProperty(this UniversalPropertyList list, string id)
        {
            try
            {
                return list.GetProperty(id);
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        private static string? GetParentName(UniversalPropertyList list)
        {
            return list.Parent switch
            {
                ArticleReference => (list.Parent as ArticleReference)!.PartNr,
                Article => (list.Parent as Article)!.PartNr,
                _ => null
            };
        }
    }
}
