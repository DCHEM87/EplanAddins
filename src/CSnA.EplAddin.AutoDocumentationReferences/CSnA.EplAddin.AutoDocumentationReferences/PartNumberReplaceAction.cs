using Eplan.EplApi.ApplicationFramework;
using Eplan.EplApi.Base;
using Eplan.EplApi.DataModel;
using Eplan.EplApi.DataModel.Graphics;
using Eplan.EplApi.DataModel.MasterData;
using Eplan.EplApi.HEServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSnA.EplAddin.AutoDocumentationReferences
{
    public class AddDocumentationReferencesAction : IEplAction
    {
        public const string CommandName = "AddDocumentsAsReferences";

        public bool Execute(ActionCallingContext ctx)
        {
            var project = new SelectionSet().GetCurrentProject(true);

            var dbg = new SelectionSet();

            var pagesByProject = new SelectionSet().SelectionRecursive
                                    .Where(x => x is Page)
                                    .Cast<Page>()
                                    .GroupBy(x => x.Project);

            WriteLogger logger = new();
            ArticleReferenceWriter referenceWriter = new(logger);

            List<string> addedComponents = [];

            using UndoStep step = new UndoManager().CreateUndoStep();

            foreach (var pageByProject in pagesByProject)
            {
                var documents = DocumentsReader.GetDocuments([.. pageByProject]);
                var refs = VirtualAssemblyReferences.GetReferences(project, documents);

                var functions = GetOrCreateFunction(pageByProject.Key, "DOCS", 10);
                foreach (var group in documents.GroupBy(x => x.Designation))
                {
                    referenceWriter.WriteDocuments([.. group], functions[group.Key]);
                }

                var virtualAssemblyReferences = GetOrCreateFunction(pageByProject.Key, "SUBASM", 20);
                foreach (var group in refs.GroupBy(x => x.PlacementDesignation).Where(x => virtualAssemblyReferences.ContainsKey(x.Key)))
                {
                    referenceWriter.WriteVirtualAssemblyReferences([.. group], virtualAssemblyReferences[group.Key]);
                }
            }

            new Decider().Decide(EnumDecisionType.eOkDecision,
                                 logger.ToString(),
                                 "Компоненты",
                                 EnumDecisionReturn.eOK,
                                 EnumDecisionReturn.eOK,
                                 "AddDocumentationReferencesActionOkResult",
                                 true,
                                 EnumDecisionIcon.eINFORMATION);

            return true;
        }

        private static Dictionary<string, Function> GetOrCreateFunction(Project project, string identifier, int offset)
        {
            var targetPages = project.Pages
                .GroupBy(x => x.Properties.DESIGNATION_FULLPLANT.AsPropertyString())
                .Select(x => new { x.Key, Page = x.First(x => x.Properties.DESIGNATION_USERDEFINED_DESCR.AsPropertyString() == "Э3") });

            Dictionary<string, Function> designatorToFunction = [];
            foreach (var targetPage in targetPages)
            {
                string funcName = GetNameForFunction(targetPage.Page) + "-" + identifier;

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
                function.SmartLock();
                function.Location = new PointD(0, -offset);
                function.Name = funcName;

                if (function.GetGraphics() is GraphicalPlacement placement)
                    placement.IsVisible = false;

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
