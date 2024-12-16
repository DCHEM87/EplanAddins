using Eplan.EplApi.ApplicationFramework;
using Eplan.EplApi.Base;
using Eplan.EplApi.DataModel;
using Eplan.EplApi.DataModel.MasterData;
using Eplan.EplApi.HEServices;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CSnA.EplAddin.AutoDocumentationReferences
{
    public class AddDocumentationReferencesAction : IEplAction
    {
        public const string CommandName = "AddDocumentsAsReferences";

        public bool Execute(ActionCallingContext ctx)
        {
            //var dbg1 = new SelectionSet();
            var project = new SelectionSet().GetCurrentProject(true);
            var documents = Documents.GetDocuments(project);
            var refs = VirtualAssemblyReferences.GetReferences(project, documents);

            var functions = GetOrCreateFunction(project, "DOCS", 10);
            foreach (var group in documents.GroupBy(x => x.Designation))
                ArticleReferenceWriter.WriteDocuments([.. group], functions[group.Key]);

            var virtualAssemblyReferences = GetOrCreateFunction(project, "SUBASM", 20);
            foreach (var group in refs.GroupBy(x => x.PlacementDesignation).Where(x => virtualAssemblyReferences.ContainsKey(x.Key)))
                ArticleReferenceWriter.WriteVirtualAssemblyReferences([.. group], virtualAssemblyReferences[group.Key]);


            //var viewModel = new MainViewModel(
            //    new PartNumberReplace.Models.MainModel(),
            //    new PartNumberReplace.ClipboardService(new ClipboardHelperWpf())
            //);

            //var view = new MainView()
            //{<EPLAN.Page.UserSupplementaryField29> 29_Перв.примен.
            //    DataContext = viewModel
            //};

            //var factory = new DialogFactory(CommandName + "Dialog", view);
            //new DialogManager().StartDialogModal(CommandName + "Dialog");

            //WPFThemingManager.Instance.RegisterControl(view);
            //var window = new Window
            //{
            //    Title = "Замена номера детали",
            //    Content = view,
            //    ShowActivated = true,
            //    Width = 500,
            //};

            //new WindowInteropHelper(window).Owner = Process.GetCurrentProcess().MainWindowHandle;

            //window.ShowDialog();

            return true;
        }

        //private static void WriteDocuments(DocumentInfo[] documents, Function function)
        //{
        //    foreach (var document in documents)
        //    {
        //        string documentNumber = document.Designation + document.Type;

        //        ArticleReference documentReference;
        //        if (function.ArticleReferences.FirstOrDefault(x => x.PartNr == documentNumber) is ArticleReference article)
        //            documentReference = article;
        //        else
        //            documentReference = function.AddArticleReference(documentNumber, "1", 1);

        //        documentReference.Properties["EPLAN.PartRef.UserSupplementaryField2"] = document.Naming;
        //        documentReference.Properties["EPLAN.PartRef.UserSupplementaryField8"] = document.Description;
        //        documentReference.Properties["EPLAN.PartRef.UserSupplementaryField17"] = documentNumber;
        //        documentReference.Properties["EPLAN.PartRef.UserSupplementaryField18"] = document.SheetFormat;
        //        documentReference.Properties["ЕСКД.Раздел"] = "Документация";
        //        documentReference.Properties["ЕСКД.Сортировка_раздел"] = "01_Документация";
        //        documentReference.StoreToObject();
        //    }
        //}

        //private static void WriteVirtualAssemblyReferences(VirtualAssemblyReference[] references, Function function)
        //{
        //    foreach (var reference in references)
        //    {
        //        ArticleReference documentReference;
        //        if (function.CrossReferencedObjectsAll
        //                .SelectMany(x => (x as Function)?.ArticleReferences ?? [])
        //                .FirstOrDefault(x => x.PartNr == reference.ReferenceToSubAssembly) 
        //                is ArticleReference article)
        //            documentReference = article;
        //        else
        //            documentReference = function.AddArticleReference(reference.ReferenceToSubAssembly, "1", 1);

        //        documentReference.Properties["EPLAN.PartRef.UserSupplementaryField2"] = reference.Naming;
        //        documentReference.Properties["EPLAN.PartRef.UserSupplementaryField17"] = reference.ReferenceToSubAssembly;
        //        documentReference.Properties["ЕСКД.Раздел"] = "Сборочные единицы";
        //        documentReference.Properties["ЕСКД.Сортировка_раздел"] = "02_Сборочные единицы";
        //        documentReference.Properties["Teamcenter.TeamcenterExportChild"] = reference.ReferenceToSubAssembly;
        //        documentReference.StoreToObject();
        //    }
        //}

        private static Dictionary<string, Function> GetOrCreateFunction(Project project, string identifier, int offset)
        {
            var targetPages = project.Pages
                .GroupBy(x => x.Properties.DESIGNATION_FULLPLANT.AsPropertyString())
                .Select(x => new { x.Key, Page = x.First(x => x.Properties.DESIGNATION_USERDEFINED_DESCR.AsPropertyString() == "Э3") });

            Dictionary<string, Function> designatorToFunction = [];
            foreach (var targetPage in targetPages)
            {
                //var dbg = targetPage.Page.Properties.INSTALLATIONSPACE_FULLNAME;
                string funcName = GetNameForFunction(targetPage.Page) + "-" + identifier; 
                //targetPage.Page.IdentifyingName.Substring(0, targetPage.Page.IdentifyingName.IndexOf('/')) + "-" + identifier;

                if (targetPage.Page.Functions.FirstOrDefault(x => x.Name == funcName) is Function func)
                {
                    designatorToFunction[targetPage.Key] = func;
                    continue;
                }

                Function function = new();

                SymbolLibrary oSymbolLibrary = new(project, "SPECIAL");
                Symbol oSymbol = new(oSymbolLibrary, "PAN");
                SymbolVariant oSymbolVariant = new(oSymbol, 0);

                function.Create(targetPage.Page, oSymbolVariant);
                function.Location = new PointD(0, -offset);
                function.Name = funcName;
                function.VisibleName = identifier;

                designatorToFunction[targetPage.Key] = function;
            }

            return designatorToFunction;
        }

        private static string GetNameForFunction(Page page) => 
            $"={page.Properties.DESIGNATION_FULLPLANT}+{page.Properties.DESIGNATION_FULLLOCATION}#{page.Properties.DESIGNATION_FULLUSERDEFINED}";

        public bool OnRegister(ref string Name, ref int Ordinal)
        {
            Name = CommandName;
            Ordinal = 20;
            return true;
        }

        public void GetActionProperties(ref ActionProperties actionProperties) { }
    }
}
