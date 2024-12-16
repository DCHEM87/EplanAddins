using Eplan.EplApi.DataModel;
using System;
using System.Linq;

namespace CSnA.EplAddin.AutoDocumentationReferences
{
    internal class ArticleReferenceWriter(WriteLogger logger)
    {
        private readonly WriteLogger _logger = logger;

        public void WriteDocuments(DocumentInfo[] documents, Function function)
        {
            WriteAssemblyReferences(documents.Where(x => string.IsNullOrWhiteSpace(x.Type) == false).ToArray(), function, (key, documentReference, asmReference) =>
            {
                documentReference.Properties["EPLAN.PartRef.UserSupplementaryField2"] = asmReference.Naming;
                documentReference.Properties["EPLAN.PartRef.UserSupplementaryField8"] = asmReference.Description;
                documentReference.Properties["EPLAN.PartRef.UserSupplementaryField17"] = key;
                documentReference.Properties["EPLAN.PartRef.UserSupplementaryField18"] = asmReference.SheetFormat;
                documentReference.Properties["ЕСКД.Раздел"] = "Документация";
                documentReference.Properties["ЕСКД.Сортировка_раздел"] = "01_Документация";
            },
            r => r.Designation + r.Type);
        }

        public void WriteVirtualAssemblyReferences(VirtualAssemblyReference[] references, Function function)
        {
            WriteAssemblyReferences(references, function, (key, documentReference, asmReference) =>
            {
                documentReference.Properties["EPLAN.PartRef.UserSupplementaryField2"] = asmReference.Naming;
                documentReference.Properties["EPLAN.PartRef.UserSupplementaryField17"] = asmReference.ReferenceToSubAssembly;
                documentReference.Properties["EPLAN.PartRef.UserSupplementaryField18"] = "А4";
                documentReference.Properties["ЕСКД.Раздел"] = "Сборочные единицы";
                documentReference.Properties["ЕСКД.Сортировка_раздел"] = "02_Сборочные единицы";
                documentReference.Properties["Teamcenter.TeamcenterExportChild"] = asmReference.ReferenceToSubAssembly;
            },
            r => r.ReferenceToSubAssembly);
        }

        private void WriteAssemblyReferences<T>(T[] references,
                                                    Function function,
                                                    Action<string, ArticleReference, T> writeCallback,
                                                    Func<T, string> referenceKeySelector)
        {
            foreach (var reference in references)
            {
                string key = referenceKeySelector.Invoke(reference);
                ArticleReference articleReference;
                if (function.CrossReferencedObjectsAll
                        .SelectMany(x => (x as Function)?.ArticleReferences ?? [])
                        .FirstOrDefault(x => x.PartNr == key)
                        is ArticleReference article)
                {
                    articleReference = article;
                    _logger.LogUpdate(key);
                }
                else
                {
                    articleReference = function.AddArticleReference(key, "1", 1);
                    _logger.LogCreate(key);
                }

                writeCallback.Invoke(key, articleReference, reference);
                articleReference.StoreToObject();
            }
        }
    }
}
