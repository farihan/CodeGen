using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hans.CodeGen.Core.Domains
{
    public class Constraint
    {
        public string Table { get; set; }
        public string Column { get; set; }
        public string Name { get; set; }
        public string ConstraintType { get; set; }
    }
}
