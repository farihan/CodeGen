using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hans.CodeGen.Core.Domains
{
    public class Relation
    {
        public string Constraint { get; set; }
        public string ParentTable { get; set; }
        public string ChildTable { get; set; }
        public string PkConstraintColumn { get; set; }
        public string FkConstraintColumn { get; set; }
        public string DeleteRule { get; set; }
        public string RelationType { get; set; }
    }
}
