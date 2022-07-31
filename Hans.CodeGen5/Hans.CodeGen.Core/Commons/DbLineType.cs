using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hans.CodeGen.Core.Commons
{
    public class DbLineType
    {
        public const string Table = "table:";
        public const string Column = "column:";
        public const string HasMany = "hasmany:";
        public const string HasOne = "hasone:";
        public const string BelongTo = "belongto:";

        public const string Space = " ";
        public const string Nullable = "?";
    }
}
