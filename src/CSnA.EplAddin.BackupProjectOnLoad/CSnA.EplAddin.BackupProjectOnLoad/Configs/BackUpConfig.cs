using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CSnA.EplAddin.BackupProjectOnLoad.Configs
{
    public class BackUpConfig
    {
        public string BackUpBaseFolder { get; init; } = @"C:\Users\%username%\Downloads\backups\";

        [XmlIgnore]
        public TimeSpan TimeDifferenceForBackUp { get; private set; } = TimeSpan.FromMinutes(1);

        [Browsable(false)]
        [XmlElement(ElementName = nameof(TimeDifferenceForBackUp))]
        public string TimeDifferenceForBackUpString { get => TimeDifferenceForBackUp.ToString(); init => TimeDifferenceForBackUp = TimeSpan.Parse(value); }


        public int MaxSavesPerProject { get; init; } = 3;
    }
}
