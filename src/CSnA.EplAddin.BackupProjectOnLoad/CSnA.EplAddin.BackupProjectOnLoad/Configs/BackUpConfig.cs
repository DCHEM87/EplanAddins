using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSnA.EplAddin.BackupProjectOnLoad.Configs
{
    public class BackUpConfig
    {
        public string BackUpBaseFolder { get; init; } = @"C:\Users\%username%\Downloads\backups\";
        public TimeSpan TimeDifferenceForBackUp { get; init; } = TimeSpan.FromMinutes(1);
        public int MaxSavesPerProject { get; init; } = 3;
    }
}
