using Eplan.EplApi.DataModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSnA.EplAddin.AutoDocumentationReferences
{
    internal class DocumentsReader
    {
        public static DocumentInfo[] GetDocuments(Page[] pages)
        {
            return pages
                .GroupBy(x => new { x.Properties.DESIGNATION_FULLPLANT, x.Properties.DESIGNATION_USERDEFINED_DESCR })
                .Select(x =>
                {
                    var pageProperties = x.First().Properties;
                    var formats = x.Select(
                        x => SheetFormatFromTemplateName(x.Properties.PAGE_FORMPLOT.AsPropertyString())
                    )
                    .Distinct()
                    .OrderByDescending(x => x)
                    .ToArray();

                    string sheetFormat = string.Empty;
                    string description = string.Empty;
                    if (formats.Length > 1)
                    {
                        sheetFormat = "*)";
                        description = "*) " + string.Join(", ", formats);
                    }
                    else
                    {
                        sheetFormat = formats[0];
                    }

                    return new DocumentInfo(
                        pageProperties.DESIGNATION_USERDEFINED_DESCR.AsPropertyString(),
                        sheetFormat,
                        pageProperties.DESIGNATION_FULLPLANT.AsPropertyString(),
                        pageProperties.PAGE_NOMINATIOMN.AsPropertyString(),
                        pageProperties["EPLAN.Page.UserSupplementaryField29"].AsPropertyString(),
                        description
                    );
                })
                .ToArray();
        }

        private static string SheetFormatFromTemplateName(string templateName) =>
            Regex.Match(templateName, "_(?<format>A.*?)_").Groups["format"].Captures[0].Value;
    }

    public record DocumentInfo(string Type, string SheetFormat, string Designation, string Naming, string Applicability, string Description);
}
