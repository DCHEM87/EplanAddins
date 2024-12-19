using CSnA.EplAddin.BackupProjectOnLoad.Configs;
using Eplan.EplApi.DataModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSnA.EplAddin.BackupProjectOnLoad
{
    internal class BackUpManager(BackUpConfig backUpConfig)
    {
        private readonly BackUpConfig _backUpConfig = backUpConfig;
        private const string _timeFormat = "MM.dd.yy H-mm-ss";

        // TODO: replace Project to readonly record for thread safety
        public void BackUpIfNeeded(ProjectInfo project)
        {
            var backUpName =
                $"{project.ProjectName}_{DateTime.Now.ToString(_timeFormat)}_{System.Security.Principal.WindowsIdentity.GetCurrent().Name.Replace("\\", string.Empty)}";

            string backUpFolder = _backUpConfig.BackUpBaseFolder + project.ProjectName;
            backUpFolder = Environment.ExpandEnvironmentVariables(backUpFolder);
            string backUpPath = backUpFolder + Path.DirectorySeparatorChar + backUpName + ".zip";

            Directory.CreateDirectory(backUpFolder);

            var existingBackups = GetExistingBackUps(backUpFolder, project.ProjectName);

            BackUpInfo? latestBackup = existingBackups.OrderByDescending(x => x.DateTime).FirstOrDefault();

            if(latestBackup is null || (DateTime.Now - latestBackup.DateTime) > _backUpConfig.TimeDifferenceForBackUp)
            {
                RemoveOldBackUps(existingBackups);
                new ProjectBackUp().CreateBackUp(project, backUpPath);
            }
        }

        private void RemoveOldBackUps(List<BackUpInfo> existingBackups)
        {
            int backUpsCountDeference = existingBackups.Count - _backUpConfig.MaxSavesPerProject;

            if (backUpsCountDeference > 0)
                foreach (var item in existingBackups.OrderBy(x => x.DateTime).Take(backUpsCountDeference))
                    item.File.Delete();
        }

        private static List<BackUpInfo> GetExistingBackUps(string backUpFolder, string projectName)
        {
            List<BackUpInfo> backUpConfigs = [];
            Regex fileRegex = new(@"(?<name>.*?)_(?<date>.*?)_(?<userName>.*?)\..*");
            foreach (FileInfo item in new DirectoryInfo(backUpFolder).GetFiles())
            {
                string itemName = Path.GetFileNameWithoutExtension(item.Name);
                Match match = fileRegex.Match(item.Name);

                try
                {
                    if (match.Captures.Count == 0)
                        continue;

                    if (match.Groups["name"].Captures[0].Value != projectName)
                        continue;

                    backUpConfigs.Add(new BackUpInfo(item,
                                                     DateTime.ParseExact(match.Groups["date"].Captures[0].Value, _timeFormat, CultureInfo.CurrentCulture),
                                                     match.Groups["userName"].Captures[0].Value));
                }
                catch (Exception)
                {
                    continue;
                }

            }
            return backUpConfigs;
        }

        private record BackUpInfo(FileInfo File, DateTime DateTime, string UserName);
    }
}
