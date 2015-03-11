using Hans.CodeGen.Core.Commons;
using Hans.CodeGen.Core.Domains;
using Hans.CodeGen.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hans.CodeGen.App
{
    public class DbFileToGlobal
    {
        public static void Generate(DatabaseInfo db)
        {
            var path1 = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.AngularWeb);
            var path2 = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.KOWeb);
            var path3 = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.RazorWeb);

            if (string.IsNullOrEmpty(path1) || string.IsNullOrEmpty(path2) || string.IsNullOrEmpty(path3))
            {
                throw new Exception("Output Directory does not exist");
            }

            Folder.Create(path1);
            Folder.Create(path2);
            Folder.Create(path3);

            WriterForGlobal(path1, db);
            WriterForGlobal(path2, db);
            WriterForGlobal(path3, db);

            Console.WriteLine();
        }

        private static void WriterForGlobal(string path, DatabaseInfo db)
        {
            var textPath = string.Format(@"{0}\Global.asax.cs", path);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("using System;");
            outFile.WriteLine("using System.Collections.Generic;");
            outFile.WriteLine("using System.Linq;");
            outFile.WriteLine("using System.Web;");
            outFile.WriteLine("using System.Web.Http;");
            outFile.WriteLine("using System.Web.Mvc;");
            outFile.WriteLine("using System.Web.Optimization;");
            outFile.WriteLine("using System.Web.Routing;");
            outFile.WriteLine("");
            outFile.WriteLine("namespace {0}", db.NamespaceCs);
            outFile.WriteLine("{");
            outFile.WriteLine("    // Note: For instructions on enabling IIS6 or IIS7 classic mode,");
            outFile.WriteLine("    // visit http://go.microsoft.com/?LinkId=9394801");
            outFile.WriteLine("");
            outFile.WriteLine("    public class MvcApplication : System.Web.HttpApplication");
            outFile.WriteLine("    {");
            outFile.WriteLine("        // [to be added]");
            outFile.WriteLine("        protected void Application_Error(object sender, EventArgs e)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            Exception exception = Server.GetLastError();");
            outFile.WriteLine("            HttpException httpException = exception as HttpException;");
            outFile.WriteLine("            // Log this exception with your logger");
            outFile.WriteLine("        }");
            outFile.WriteLine("    }");
            outFile.WriteLine("}");
            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }
    }
}
