using Eplan.EplApi.DataModel;
using System;
using System.Linq;

namespace CSnA.EplAddin.AutoDocumentationReferences
{
    internal static class ArticleReferenceWriter
    {
        public static void WriteDocuments(DocumentInfo[] documents, Function function)
        {
            WriteAssemblyReferences(documents, function, (key, documentReference, asmReference) =>
            {
                documentReference.Properties["EPLAN.PartRef.UserSupplementaryField2"] = asmReference.Naming;
                documentReference.Properties["EPLAN.PartRef.UserSupplementaryField8"] = asmReference.Description;
                documentReference.Properties["EPLAN.PartRef.UserSupplementaryField17"] = key;
                documentReference.Properties["EPLAN.PartRef.UserSupplementaryField18"] = asmReference.SheetFormat;
                documentReference.Properties["ЕСКД.Раздел"] = "Документация";
                documentReference.Properties["ЕСКД.Сортировка_раздел"] = "01_Документация";
            },
            reference => reference.Designation + reference.Type);
        }

        public static void WriteVirtualAssemblyReferences(VirtualAssemblyReference[] references, Function function)
        {
            WriteAssemblyReferences(references, function, (key, documentReference, asmReference) =>
            {
                documentReference.Properties["EPLAN.PartRef.UserSupplementaryField2"] = asmReference.Naming;
                documentReference.Properties["EPLAN.PartRef.UserSupplementaryField17"] = asmReference.ReferenceToSubAssembly;
                documentReference.Properties["ЕСКД.Раздел"] = "Сборочные единицы";
                documentReference.Properties["ЕСКД.Сортировка_раздел"] = "02_Сборочные единицы";
                documentReference.Properties["Teamcenter.TeamcenterExportChild"] = asmReference.ReferenceToSubAssembly;
            },
            reference => reference.ReferenceToSubAssembly);
        }

        private static void WriteAssemblyReferences<T>(T[] references,
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
                    articleReference = article;
                else
                    articleReference = function.AddArticleReference(key, "1", 1);

                writeCallback.Invoke(key, articleReference, reference);
                articleReference.StoreToObject();
            }
        }
    }
}
