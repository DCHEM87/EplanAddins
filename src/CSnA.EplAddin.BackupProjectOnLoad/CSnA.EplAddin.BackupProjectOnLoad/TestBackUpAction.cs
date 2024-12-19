using CSnA.EplAddin.BackupProjectOnLoad.Configs;
using Eplan.EplApi.ApplicationFramework;
using Eplan.EplApi.DataModel;
using Eplan.EplApi.HEServices;

namespace CSnA.EplAddin.BackupProjectOnLoad
{
#if DEBUG
    public class TestBackUpAction : IEplAction
    {
        public const string CommandName = "TestBackUp";

        public bool Execute(ActionCallingContext oActionCallingContext)
        {
            var project = new SelectionSet().GetCurrentProject(true);
            new BackUpManager(new BackUpConfig()).BackUpIfNeeded(project.ToInfo());
            return true;
        }

        public void GetActionProperties(ref ActionProperties actionProperties) { }

        public bool OnRegister(ref string Name, ref int Ordinal)
        {
            Name = CommandName;
            Ordinal = 20;
            return true;
        }
    }
#endif
}
