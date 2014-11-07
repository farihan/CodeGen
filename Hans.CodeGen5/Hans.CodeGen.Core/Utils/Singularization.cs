using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hans.CodeGen.Core.Utils
{
    public class Singularization
    {
        private readonly IDictionary<string, string> Singularizations = new Dictionary<string, string>
        {
            // Start with the rarest cases, and move to the most common
            {"people", "person"},
            {"oxen", "ox"},
            {"children", "child"},
            {"feet", "foot"},
            {"teeth", "tooth"},
            {"geese", "goose"},
            // And now the more standard rules.
            {"(.*)ives?", "$1ife"},
            {"(.*)ves?", "$1f"},
            // ie, wolf, wife
            {"(.*)men$", "$1man"},
            {"(.+[aeiou])ys$", "$1y"},
            {"(.+[^aeiou])ies$", "$1y"},
            {"(.+)zes$", "$1"},
            {"([m|l])ice$", "$1ouse"},
            {"matrices", @"matrix"},
            {"indices", @"index"},
            {"(.+[^aeiou])ices$","$1ice"},
            {"(.*)ices", @"$1ex"},
            // ie, Matrix, Index
            {"(octop|vir)i$", "$1us"},
            {"(.+(s|x|sh|ch))es$", @"$1"},
            {"(.+)s", @"$1"}
        };

        public string Singularize(string word)
        {
            foreach (var singularization in Singularizations)
            {
                if (Regex.IsMatch(word, singularization.Key))
                {
                    return Regex.Replace(word, singularization.Key, singularization.Value);
                }
            }

            return word;
        }
    }
}
