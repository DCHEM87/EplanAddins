using Eplan.EplApi.DataModel;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace CSnA.EplAddin.BackupProjectOnLoad
{
    internal class ProjectBackUp
    {
        public void CreateBackUp(ProjectInfo project, string destinationFile)
        {
            using TempFolder folder = new($"{project.ProjectName}_{Guid.NewGuid()}");

            var dirName = Path.GetFileName(project.ProjectDirectoryPath);
            var projectDirectory = folder.DirectoryInfo.CreateSubdirectory(dirName);
            
            File.Copy(
                project.ProjectLinkFilePath,
                Path.Combine(folder.Path, Path.GetFileName(project.ProjectLinkFilePath))
            );

            CopyRecursively(new(project.ProjectDirectoryPath), projectDirectory);

            ZipFile.CreateFromDirectory(folder.Path, destinationFile);
        }

        private void CopyRecursively(DirectoryInfo source, DirectoryInfo destination)
        {
            foreach (var dir in source.GetDirectories())
                if (dir.Name != "TeamcenterExportDOC" && dir.Name != "DOC")
                    CopyRecursively(dir, destination.CreateSubdirectory(dir.Name));

            foreach (var file in source.GetFiles())
                try
                {
                    file.CopyTo(Path.Combine(destination.FullName, file.Name));
                }
                catch (IOException e) when (e.HResult == -2147024863) // file locked by other process
                {
                    Debug.WriteLine(e);
                }
        }

        private class TempFolder : IDisposable
        {
            public string Path { get; private set; }
            public DirectoryInfo DirectoryInfo => new(Path);

            public TempFolder(string name)
            {
                Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), name);

                // Clear directory if garbage is left
                if (Directory.Exists(Path))
                    Directory.Delete(Path);

                Directory.CreateDirectory(Path);
            }

            public void Dispose()
            {
                if (Directory.Exists(Path))
                    Directory.Delete(Path, true);
            }
        }
    }
}
