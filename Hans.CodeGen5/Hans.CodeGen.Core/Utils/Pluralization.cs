using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hans.CodeGen.Core.Utils
{
    public class Pluralization
    {
        private readonly IDictionary<string, string> Pluralizations = new Dictionary<string, string>
        {
            // Start with the rarest cases, and move to the most common
            { "person", "people" },
            { "ox", "oxen" },
            { "child", "children" },
            { "foot", "feet" },
            { "tooth", "teeth" },
            { "goose", "geese" },
            { "(.*)info", @"$1infos" },
            // And now the more standard rules.
            { "(.*)fe?", "$1ves" },         // ie, wolf, wife
            { "(.*)man$", "$1men" },
            { "(.+[aeiou]y)$", "$1s" },
            { "(.+[^aeiou])y$", "$1ies" },
            { "(.+z)$", "$1zes" },
            { "([m|l])ouse$", "$1ice" },
            { "(.+)(e|i)x$", @"$1ices"},    // ie, Matrix, Index
            { "(octop|vir)us$", "$1i"},
            { "(.+(s|x|sh|ch))$", @"$1es"},
            { "(.+)", @"$1s" }
        };

        public string Pluralize(string word)
        {
            foreach (var pluralization in Pluralizations)
            {
                if (Regex.IsMatch(word, pluralization.Key))
                {
                    return Regex.Replace(word, pluralization.Key, pluralization.Value);
                }
            }

            return word;
        }
    }
}
