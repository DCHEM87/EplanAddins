using Eplan.EplApi.Base;
using Eplan.EplApi.DataModel;
using Eplan.EplApi.MasterData;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Forms;

namespace CSnA.EplAddin.PartNumberReplace.Models
{
    internal class MainModel
    {
        public bool IsAutoReplaceEnabled { get; set; }
        public IObservable<PartNumberModel> Replaced => _replaced.AsObservable();
        private Subject<PartNumberModel> _replaced = new();

        public void Replace(PartNumberModel[] partNumberModels)
        {
            var project = new ProjectManager().CurrentProject;

            MDPartsManagement oPartsManagement = new MDPartsManagement();
            MDPartsDatabase partsDatabase = GetDataBase(oPartsManagement);

            foreach (var prop in partNumberModels)
            {
                if(IsAutoReplaceEnabled)
                    AutoReplace(project, partsDatabase, prop);
                else
                    SimpleReplace(project, partsDatabase, prop);
            }
        }

        private void AutoReplace(Project project, MDPartsDatabase partsDatabase, PartNumberModel prop)
        {
            DMObjectsFinder finder = new(project);

            var projectArticleReferences = finder.GetArticleReferences(new());

            var references = projectArticleReferences.Where(x => x.PartNr == prop.OldValue).ToArray();

            bool isChanged = false;

            if (references.Length != 0)
            {
                ReplaceInReference(prop, references);
                isChanged = true;
            }    

            var part = partsDatabase.GetPart(prop.OldValue);

            if(part is not null)
            {
                part.PartNr = prop.NewValue;
                isChanged = true;
            }

            if(isChanged)
                _replaced.OnNext(prop);
        }

        private void SimpleReplace(Project project, MDPartsDatabase partsDatabase, PartNumberModel prop)
        {
            DMObjectsFinder finder = new(project);

            var projectArticleReferences = finder.GetArticleReferences(new());

            var part = partsDatabase.GetPart(prop.OldValue)
                ?? throw new BaseException($"Part \"{prop.OldValue}\" not found in database", MessageLevel.Error);

            var references = projectArticleReferences.Where(x => x.PartNr == prop.OldValue).ToArray();

            if (references.Length == 0)
                throw new BaseException($"References \"{prop.OldValue}\" not found in project", MessageLevel.Error);

            part.PartNr = prop.NewValue;

            ReplaceInReference(prop, references);

            _replaced.OnNext(prop);
        }

        private static MDPartsDatabase GetDataBase(MDPartsManagement oPartsManagement)
        {
            MDPartsDatabase partsDatabase = oPartsManagement.OpenDatabase();
            if (partsDatabase.IsReadOnly)
                throw new Exception("Database is readonly");
            return partsDatabase;
        }

        private static void ReplaceInReference(PartNumberModel prop, ArticleReference[] references)
        {
            foreach (var reference in references)
                reference.PartNr = prop.NewValue;
            foreach (var reference in references)
                reference.StoreToObject();
        }
    }
}
