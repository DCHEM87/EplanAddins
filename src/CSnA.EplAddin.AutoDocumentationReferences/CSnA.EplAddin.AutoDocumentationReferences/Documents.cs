using Eplan.EplApi.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CSnA.EplAddin.AutoDocumentationReferences
{
    internal class Documents
    {
        public static DocumentInfo[] GetDocuments(Project project)
        {
            var pages = project.Pages;

            return pages
                .GroupBy(x =>  new { x.Properties.DESIGNATION_FULLPLANT, x.Properties.DESIGNATION_USERDEFINED_DESCR })
                //.Where(x => string.IsNullOrWhiteSpace(x.First().Properties.DESIGNATION_USERDEFINED_DESCR.AsPropertyString()) == false)
                .Select(x =>
                {
                    var pageProperties = x.First().Properties;
                    var formats = x.Select(
                        x => SheetFormatFromTemplateName(x.Properties.PAGE_FORMPLOT.AsPropertyString())
                    )
                    .Distinct()
                    .ToArray();

                    string sheetFormat = string.Empty;
                    string description = string.Empty;
                    if (formats.Length > 1)
                    {
                        sheetFormat = "*)";
                        description = "*)" + string.Join(", ", formats);
                    }
                    else
                    {
                        sheetFormat = formats[0];
                    }

                    return new DocumentInfo(
                        pageProperties.DESIGNATION_USERDEFINED_DESCR.AsPropertyString() /*x.Key.PAGE_ADDITIONALPAGE*/,
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
