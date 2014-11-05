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
            var path = string.Format(@"{0}\{1}", db.OutputDirectory, CreationType.Error);

            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("Output Directory does not exist");
            }

            Folder.Create(path);

            WriterForFilterConfig(path, db);
            WriterForActionFilter(path, db);
            WriterForRouteConfig(path, db);
            WriterForGlobal(path, db);
            WriterForErrorController(path, db);
            WriterForErrorCshtml(path, db);
            WriterForFailCshtml(path, db);

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

            Console.Write(textPath);
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

            Console.Write(textPath);
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

            Console.Write(textPath);
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

            Console.Write(textPath);
        }

        private static void WriterForErrorController(string path, DatabaseInfo db)
        {
            var textPath = string.Format(@"{0}\ErrorController.cs", path);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("using System;");
            outFile.WriteLine("using System.Collections.Generic;");
            outFile.WriteLine("using System.Linq;");
            outFile.WriteLine("using System.Web;");
            outFile.WriteLine("using System.Web.Mvc;");
            outFile.WriteLine("");
            outFile.WriteLine("namespace {0}.Controllers", db.NamespaceCs);
            outFile.WriteLine("{");
            outFile.WriteLine("    public class ErrorController : Controller");
            outFile.WriteLine("    {");
            outFile.WriteLine("        // The 404 action handler");
            outFile.WriteLine("        // Get: /Fail/");
            outFile.WriteLine("        public ActionResult Fail()");
            outFile.WriteLine("        {");
            outFile.WriteLine("            Response.StatusCode = 404;");
            outFile.WriteLine("            Response.TrySkipIisCustomErrors = true;");
            outFile.WriteLine("            return View();");
            outFile.WriteLine("        }");
            outFile.WriteLine("");
            outFile.WriteLine("    }");
            outFile.WriteLine("}");
            outFile.Close();

            Console.Write(textPath);
        }

        private static void WriterForErrorCshtml(string path, DatabaseInfo db)
        {
            var textPath = string.Format(@"{0}\Error.cshtml", path);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("@model System.Web.Mvc.HandleErrorInfo");
            outFile.WriteLine("@{");
            outFile.WriteLine("    ViewBag.Title = \"Error\";");
            outFile.WriteLine("}");
            outFile.WriteLine("<h1 class=\"error\">");
            outFile.WriteLine("    Error.</h1>");
            outFile.WriteLine("<h2 class=\"error\">");
            outFile.WriteLine("    An error occurred while processing your request.</h2>");
            outFile.WriteLine("@{");
            outFile.WriteLine("    if (Request.IsLocal)");
            outFile.WriteLine("    {");
            outFile.WriteLine("        if (@Model != null && @Model.Exception != null)");
            outFile.WriteLine("        {");
            outFile.WriteLine("    <h3>");
            outFile.WriteLine("        Exception was : @Model.Exception.Message</h3>");
            outFile.WriteLine("    <h4>");
            outFile.WriteLine("        Stack Trace: @Model.Exception.StackTrace</h4>");
            outFile.WriteLine("        }");
            outFile.WriteLine("        else");
            outFile.WriteLine("        {");
            outFile.WriteLine("    <h3>");
            outFile.WriteLine("        Exception was null</h3>");
            outFile.WriteLine("        }");
            outFile.WriteLine("    }");
            outFile.WriteLine("    else");
            outFile.WriteLine("    {");
            outFile.WriteLine("    <h4>");
            outFile.WriteLine("        The Error has been reported to the Administrator</h4>");
            outFile.WriteLine("    }");
            outFile.WriteLine("}");
            outFile.Close();

            Console.Write(textPath);
        }

        private static void WriterForFailCshtml(string path, DatabaseInfo db)
        {
            var textPath = string.Format(@"{0}\Fail.cshtml", path);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("@model System.Web.Mvc.HandleErrorInfo");
            outFile.WriteLine("@{");
            outFile.WriteLine("    ViewBag.Title = \"Fail\";");
            outFile.WriteLine("}");
            outFile.WriteLine("");
            outFile.WriteLine("<h1 class=\"error\">");
            outFile.WriteLine("    Fail</h1>");
            outFile.WriteLine("");
            outFile.WriteLine("@if (@Model != null)");
            outFile.WriteLine("{");
            outFile.WriteLine("");
            outFile.WriteLine("    <h2 class=\"error\">@Model.Exception</h2>");
            outFile.WriteLine("}");
            outFile.WriteLine("else");
            outFile.WriteLine("{");
            outFile.WriteLine("    <h2>");
            outFile.WriteLine("        Oops! Server Returned a 404 (Page Not Found)!");
            outFile.WriteLine("        <br />");
            outFile.WriteLine("        The Error has been reported to the Administrator");
            outFile.WriteLine("    </h2>");
            outFile.WriteLine("}");
            outFile.Close();

            Console.Write(textPath);
        }
    }
}
