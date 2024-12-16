using Eplan.EplApi.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSnA.EplAddin.AutoDocumentationReferences
{
    internal class VirtualAssemblyReferences
    {
        public static VirtualAssemblyReference[] GetReferences(Project project, DocumentInfo[] documents)
        {
            DMObjectsFinder finder = new(project);
            var projectArticleReferences = finder.GetArticleReferences(new());

            //var specifications = projectArticleReferences.Where(x => 
            //                        string.IsNullOrWhiteSpace(x.Properties.DESIGNATION_USERDEFINED_DESCR.AsPropertyString()))
            //                     .ToDictionary(x => x.Properties.DESIGNATION_FULLPLANT.AsPropertyString());

            var refsByPlant = projectArticleReferences  
                                .GroupBy(x => 
                                    x.Properties.DESIGNATION_FULLPLANT.AsPropertyString())
                                .ToDictionary(x => x.Key);


            var refs = documents
                .GroupBy(x => x.Designation)
                .Select(x => x.First())
                .Where(x => x.Type == string.Empty /* Spec */)
                .Select(item =>
                {
                    if (refsByPlant.TryGetValue(item.Designation, out var value))
                        return new VirtualAssemblyReference(
                            item.Applicability,
                            item.Naming,
                            item.Designation
                        );
                    else
                        return null;
                })
                .Where(x => x != null)
                .Cast<VirtualAssemblyReference>()
                .ToArray();

            return refs;
        }

    }

    public record VirtualAssemblyReference(string PlacementDesignation, string Naming, string ReferenceToSubAssembly);
}
