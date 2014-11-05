using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Hans.CodeGen.Core.Utils
{
    public static class Folder
    {
        public static void Create(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
