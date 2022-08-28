using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hans.CodeGen.Core.Domains
{
    public class DatabaseInfo
    {
        public string NamespaceCs { get; set; }
        public string DatabaseType { get; set; }
        public string InputDirectory { get; set; }
        public string InputAndOutputDbFile { get; set; }
        public string InputUIFile { get; set; }
        public string OutputDirectory { get; set; }
        public string Owner { get; set; }
        public string ReadFromDb { get; set; }
        public string ReadFromDbFile { get; set; }
        public string RemoveChars { get; set; }
        public string ApplicationName { get; set; }
        public string RemoveFields { get; set; }
        public string EnableAuditTrail { get; set; }

        public List<Schema> Schemas { get; set; }
        public List<Relation> Relations { get; set; }
        public List<Constraint> Constraints { get; set; }

        public List<string> TableNames { get; set; }
    }
}
