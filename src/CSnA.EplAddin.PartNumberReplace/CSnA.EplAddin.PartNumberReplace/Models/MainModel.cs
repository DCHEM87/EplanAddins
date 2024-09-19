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
        public IObservable<PartNumberModel> Replaced => _replaced.AsObservable();
        private Subject<PartNumberModel> _replaced = new();

        public void Replace(PartNumberModel[] partNumberModels)
        {
            var project = new ProjectManager().CurrentProject;

            MDPartsManagement oPartsManagement = new MDPartsManagement();
            MDPartsDatabase partsDatabase = oPartsManagement.OpenDatabase();

            foreach (var prop in partNumberModels)
            {
                DMObjectsFinder finder = new(project);

                var projectArticleReferences = finder.GetArticleReferences(new());

                var part = partsDatabase.GetPart(prop.OldValue)
                    ?? throw new BaseException($"Part \"{prop.OldValue}\" not found in database", MessageLevel.Error);

                var reference = projectArticleReferences.FirstOrDefault(x => x.PartNr == prop.OldValue) 
                    ?? throw new BaseException($"Reference \"{prop.OldValue}\" not found in project", MessageLevel.Error);

                part.PartNr = prop.NewValue;

                reference.PartNr = prop.NewValue;
                reference.StoreToObject();

                _replaced.OnNext(prop);
            }
        }
    }
}
