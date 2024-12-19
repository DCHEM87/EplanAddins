using Eplan.EplApi.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSnA.EplAddin.BackupProjectOnLoad
{
    internal record ProjectInfo(string ProjectName, string ProjectDirectoryPath, string ProjectLinkFilePath);

	internal static class ProjectToInfoExtensions
	{
		public static ProjectInfo ToInfo(this Project project) =>
			new(project.ProjectName, project.ProjectDirectoryPath, project.ProjectLinkFilePath);
	}
}
