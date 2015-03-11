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
    public class DbFileToErrorHandling
    {
        public static void Generate(DatabaseInfo db)
        {
            var path1 = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.AngularAppStart);
            var path2 = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.KOAppStart);
            var path3 = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.RazorAppStart);

            if (string.IsNullOrEmpty(path1) || string.IsNullOrEmpty(path2) || string.IsNullOrEmpty(path3))
            {
                throw new Exception("Output Directory does not exist");
            }

            Folder.Create(path1);
            Folder.Create(path2);
            Folder.Create(path3);

            WriterForFilterConfig(path1, db);
            WriterForActionFilter(path1, db);
            WriterForRouteConfig(path1, db);

            WriterForFilterConfig(path2, db);
            WriterForActionFilter(path2, db);
            WriterForRouteConfig(path2, db);

            WriterForFilterConfig(path3, db);
            WriterForActionFilter(path3, db);
            WriterForRouteConfig(path3, db);

            Console.WriteLine();
        }

        private static void WriterForFilterConfig(string path, DatabaseInfo db)
        {
            var textPath = string.Format(@"{0}\FilterConfig.cs", path);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("using System.Web;");
            outFile.WriteLine("using System.Web.Mvc;");
            outFile.WriteLine("");
            outFile.WriteLine("namespace {0}", db.NamespaceCs);
            outFile.WriteLine("{");
            outFile.WriteLine("    public class FilterConfig");
            outFile.WriteLine("    {");
            outFile.WriteLine("        // [to be added]");
            outFile.WriteLine("        public static void RegisterGlobalFilters(GlobalFilterCollection filters)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            filters.Add(new HandleAndLogErrorAttribute());");
            outFile.WriteLine("        }");
            outFile.WriteLine("    }");
            outFile.WriteLine("}");
            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForActionFilter(string path, DatabaseInfo db)
        {
            var textPath = string.Format(@"{0}\HandleAndLogErrorAttribute.cs", path);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("using System.Web;");
            outFile.WriteLine("using System.Web.Mvc;");
            outFile.WriteLine("");
            outFile.WriteLine("namespace {0}.ActionFilters", db.NamespaceCs);
            outFile.WriteLine("{");
            outFile.WriteLine("    public class HandleAndLogErrorAttribute : HandleErrorAttribute");
            outFile.WriteLine("    {");
            outFile.WriteLine("        // [to be added]");
            outFile.WriteLine("        public override void OnException(ExceptionContext filterContext)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            // Log the filterContext.Exception details");
            outFile.WriteLine("            base.OnException(filterContext);");
            outFile.WriteLine("        }");
            outFile.WriteLine("    }");
            outFile.WriteLine("}");
            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForRouteConfig(string path, DatabaseInfo db)
        {
            var textPath = string.Format(@"{0}\RouteConfig.cs", path);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("using System;");
            outFile.WriteLine("using System.Collections.Generic;");
            outFile.WriteLine("using System.Linq;");
            outFile.WriteLine("using System.Web;");
            outFile.WriteLine("using System.Web.Http;");
            outFile.WriteLine("using System.Web.Mvc;");
            outFile.WriteLine("using System.Web.Routing;");
            outFile.WriteLine("");
            outFile.WriteLine("namespace Mvc4HandleErrors");
            outFile.WriteLine("{");
            outFile.WriteLine("    public class RouteConfig");
            outFile.WriteLine("    {");
            outFile.WriteLine("        public static void RegisterRoutes(RouteCollection routes)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            routes.IgnoreRoute(\"{resource}.axd/{*pathInfo}\");");
            outFile.WriteLine("");
            outFile.WriteLine("            // [to be added]");
            outFile.WriteLine("            routes.MapRoute(");
            outFile.WriteLine("                name: \"Fail\",");
            outFile.WriteLine("                url: \"Fail/{action}/{id}\",");
            outFile.WriteLine("                defaults: new {"); 
            outFile.WriteLine("                    controller = \"Error\","); 
            outFile.WriteLine("                    action = \"Fail\","); 
            outFile.WriteLine("                    id = UrlParameter.Optional }");
            outFile.WriteLine("            );");
            outFile.WriteLine("        }");
            outFile.WriteLine("    }");
            outFile.WriteLine("}");
            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }
    }
}
