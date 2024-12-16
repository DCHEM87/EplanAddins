using System;
using System.Collections.Generic;

namespace CSnA.EplAddin.AutoDocumentationReferences
{
    internal class WriteLogger
    {
        private readonly List<string> _creations = [];
        private readonly List<string> _updates = [];

        public void LogCreate(string componentName) => _creations.Add(componentName);
        public void LogUpdate(string componentName) => _updates.Add(componentName);

        
        public override string ToString()
        {
            string ret = string.Empty;

            if (_creations.Count != 0)
                ret += "Созданы компоненты: " + Environment.NewLine
                    + string.Join(Environment.NewLine, _creations) + Environment.NewLine;

            if (_updates.Count != 0)
                ret += "Обновлены компоненты: " + Environment.NewLine
                    + string.Join(Environment.NewLine, _updates) + Environment.NewLine;

            return ret;
        }
    }
}
