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
    public class DbFileToAngularUI
    {
        public static void Generate(DatabaseInfo db)
        {
            var path = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.AngularViews);

            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("Output Directory does not exist");
            }

            Folder.Create(path);

            foreach (var tableName in db.Schemas
                .Select(x => x.Table)
                .Distinct())
            {
                var className = tableName.UpperedFirstChar();

                WriterForIndex(path, tableName, className, db);
                WriterForDetails(path, tableName, className, db);
                WriterForCreate(path, tableName, className, db);
                WriterForEdit(path, tableName, className, db);
                WriterForDelete(path, tableName, className, db);
                WriterForGrid(path, tableName, className, db);
            }

            WriterForErrorCshtml(path, db);
            WriterForFailCshtml(path, db);

            // layout
            WriterForLayout(path, db);

            Console.WriteLine();
        }

        private static void WriterForLayout(string path, DatabaseInfo db)
        {
            Folder.Create(string.Format(@"{0}\{1}", path, CreationType.Shared));

            var textPath = string.Format(@"{0}\{1}\_Layout.cshtml", path, CreationType.Shared);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("<!DOCTYPE html>");
            outFile.WriteLine("<html>");
            outFile.WriteLine("<head>");
            outFile.WriteLine("    <!--Default setup-->");
            outFile.WriteLine("    <meta charset=\"utf-8\">");
            outFile.WriteLine("    <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">");
            outFile.WriteLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
            outFile.WriteLine("    <title>@ViewBag.Title - {0}</title>", db.ApplicationName);
            outFile.WriteLine("    <!-- Bootstrap -->");
            outFile.WriteLine("    @Scripts.Render(\"~/bundles/jquery\")");
            outFile.WriteLine("    @Scripts.Render(\"~/bundles/modernizr\")");
            outFile.WriteLine("    @Scripts.Render(\"~/bundles/angular\")");
            outFile.WriteLine("    @Styles.Render(\"~/Content/css\")");
            outFile.WriteLine("    <meta name=\"description\" content=\"The description of my {0} application\" />", db.ApplicationName);
            outFile.WriteLine("    <!--[if lt IE 9]>");
            outFile.WriteLine("        script type=\"text/javascript\" src=\"Scripts/html5shiv.js\"></script>");
            outFile.WriteLine("        <script type=\"text/javascript\" src=\"Scripts/respond.min.js\"></script>");
            outFile.WriteLine("    <![endif]-->");
            outFile.WriteLine("</head>");
            outFile.WriteLine("<body>");
            outFile.WriteLine("    <div class=\"navbar navbar-inverse navbar-fixed-top\">");
            outFile.WriteLine("        <div class=\"container\">");
            outFile.WriteLine("            <div class=\"navbar-header\">");
            outFile.WriteLine("                <button type=\"button\" class=\"navbar-toggle\" data-toggle=\"collapse\" data-target=\"navbar-collapse\">");
            outFile.WriteLine("                    <span class=\"icon-bar\"></span>");
            outFile.WriteLine("                    <span class=\"icon-bar\"></span>");
            outFile.WriteLine("                    <span class=\"icon-bar\"></span>");
            outFile.WriteLine("                </button>");
            var cssProperty = "new { area = \"\" }, new { @class = \"navbar-brand\" }";
            outFile.WriteLine("                @Html.ActionLink(\"{0}\", \"Index\", \"Home\", {1}", db.ApplicationName, cssProperty);
            outFile.WriteLine("            </div>");
            outFile.WriteLine("            <div class=\"navbar-collapse collapse\">");
            outFile.WriteLine("                <ul class=\"nav navbar-nav\">");
            outFile.WriteLine("                    <li>@Html.ActionLink(\"Home\", \"Index\", \"Home\")</li>");
            outFile.WriteLine("                    <li>@Html.ActionLink(\"About\", \"About\", \"Home\")</li>");
            outFile.WriteLine("                    <li>@Html.ActionLink(\"Contact\", \"Contact\", \"Home\")</li>");

            foreach (var className in db.Schemas
                .Select(x => x.Table)
                .Distinct())
            {
                outFile.WriteLine("                    <li>@Html.ActionLink(\"{0}\", \"Index\", \"{0}\")</li>", className.UpperedFirstChar());
            }

            outFile.WriteLine("                </ul>");
            outFile.WriteLine("            </div>");
            outFile.WriteLine("        </div>");
            outFile.WriteLine("    </div>");
            outFile.WriteLine("    <div class=\"container body-content\">");
            outFile.WriteLine("        @RenderBody()");
            outFile.WriteLine("        <hr />");
            outFile.WriteLine("        <footer>");
            outFile.WriteLine("            <p>&copy; @DateTime.Now.Year - {0}</p>", db.ApplicationName);
            outFile.WriteLine("        </footer>");
            outFile.WriteLine("    </div>");
            outFile.WriteLine("");
            outFile.WriteLine("    @RenderSection(\"scripts\", required: false)");
            outFile.WriteLine("</body>");
            outFile.WriteLine("</html>");
            outFile.Close();

            Console.Write(string.Format("\n{0} created", "_Layout.cshtml"));
        }

        private static void WriterForIndex(string path, string tableName, string className, DatabaseInfo db)
        {
            Folder.Create(string.Format(@"{0}\{1}", path, className.UpperedFirstChar()));

            var textPath = string.Format(@"{0}\{1}\{2}.cshtml", path, className.UpperedFirstChar(), CreationType.Index);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("@{");
            outFile.WriteLine("    ViewBag.Title = \"Index - {0};\"", className);
            outFile.WriteLine("    Layout = \"~/Views/Shared/_Layout.cshtml\";");
            outFile.WriteLine("}");
            outFile.WriteLine("");
            outFile.WriteLine("<h2>@ViewBag.Title</h2>");
            outFile.WriteLine("<div ng-controller=\"ProductController\">");
            outFile.WriteLine("    @Html.Partial(\"~/Views/Product/_Grid.cshtml\")");
            outFile.WriteLine("    @Html.Partial(\"~/Views/Product/_Create.cshtml\")");
            outFile.WriteLine("    @Html.Partial(\"~/Views/Product/_Delete.cshtml\")");
            outFile.WriteLine("    @Html.Partial(\"~/Views/Product/_Edit.cshtml\")");
            outFile.WriteLine("    @Html.Partial(\"~/Views/Product/_Details.cshtml\")");
            outFile.WriteLine("</div>");
            outFile.WriteLine("<script src=\"~/Scripts/App/productController.js\"></script>");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForDetails(string path, string tableName, string className, DatabaseInfo db)
        {
            Folder.Create(string.Format(@"{0}\{1}", path, className.UpperedFirstChar()));

            var textPath = string.Format(@"{0}\{1}\_{2}.cshtml", path, className.UpperedFirstChar(), CreationType.Details);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("<script type=\"text/ng-template\" id=\"detailsModal.html\">");
            outFile.WriteLine("    <div class=\"modal-header\">");
            outFile.WriteLine("        <h3 class=\"modal-title\">@ViewBag.Title Details</h3>");
            outFile.WriteLine("    </div>");
            outFile.WriteLine("    <div class=\"modal-body\">");
            outFile.WriteLine("        <div class=\"form-horizontal\">");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                if (s.ColumnType == ColumnType.None)
                {
                    outFile.WriteLine("            <div class=\"form-group\">");
                    outFile.WriteLine("                <div class=\"col-md-3 control-label\">");
                    outFile.WriteLine("                    {0}", s.Column);
                    outFile.WriteLine("                </div>");
                    outFile.WriteLine("                <div class=\"col-md-9\">");
                    outFile.WriteLine("                    <input type=\"text\" name=\"{0}\" ng-model=\"{1}.{0}\" placeholder=\"\" class=\"form-control input-sm\" />", s.Column, className.LoweredFirstChar());
                    outFile.WriteLine("                </div>");
                    outFile.WriteLine("            </div>");
                }
            }

            foreach (var r in db.Relations.Where(p => p.ChildTable == tableName).OrderBy(p => p.ParentTable))
            {
                outFile.WriteLine("            <div class=\"form-group\">");
                outFile.WriteLine("                <div class=\"col-md-3 control-label\">");
                outFile.WriteLine("                    {0}", r.ParentTable.UpperedFirstChar());
                outFile.WriteLine("                </div>");
                outFile.WriteLine("                <div class=\"col-md-9\">");
                outFile.WriteLine("                    <select class=\"form-control\" ng-model=\"{0}.{1}\" ng-options=\"item.ID as item.Name for item in available{2}\">",
                    className.LoweredFirstChar(), r.PkConstraintColumn, r.ParentTable.UpperedFirstChar());
                outFile.WriteLine("                </div>");
                outFile.WriteLine("            </div>");
            }

            outFile.WriteLine("        </div>");
            outFile.WriteLine("    </div>");
            outFile.WriteLine("	   <div class=\"modal-footer\">");
            outFile.WriteLine("        <button class=\"btn btn-default btn-sm\" ng-click=\"ok()\">Cancel</button>");
            outFile.WriteLine("    </div>");
            outFile.WriteLine("</script>");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForCreate(string path, string tableName, string className, DatabaseInfo db)
        {
            Folder.Create(string.Format(@"{0}\{1}", path, className.UpperedFirstChar()));

            var textPath = string.Format(@"{0}\{1}\_{2}.cshtml", path, className.UpperedFirstChar(), CreationType.Create);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("<script type=\"text/ng-template\" id=\"createModal.html\">");
            outFile.WriteLine("    <div class=\"modal-header\">");
            outFile.WriteLine("        <h3 class=\"modal-title\">Create @ViewBag.Title</h3>");
            outFile.WriteLine("    </div>");
            outFile.WriteLine("    <form name=\"createForm\" novalidate ng-submit=\"create({0})\">", className.LoweredFirstChar());
            outFile.WriteLine("        <div class=\"modal-body\">");
            outFile.WriteLine("            <div class=\"form-horizontal\">");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                if (s.ColumnType == ColumnType.None)
                {
                    outFile.WriteLine("                <div class=\"form-group\" ng-class=\"{{ 'has-error' : createForm.{0}.$invalid && (createForm.{0}.$dirty || submitted)}}\">", s.Column);
                    outFile.WriteLine("                    <div class=\"col-md-3 control-label\">");
                    outFile.WriteLine("                        {0}", s.Column);
                    outFile.WriteLine("                    </div>");
                    outFile.WriteLine("                    <div class=\"col-md-9\">");
                    outFile.WriteLine("                        <input type=\"text\" name=\"{0}\" ng-model=\"{1}.{0}\" placeholder=\"\" class=\"form-control input-sm\" ng-required=\"true\" />", s.Column, className.LoweredFirstChar());
                    outFile.WriteLine("                        <div ng-show=\"createForm.{0}.$invalid && (createForm.{0}.$dirty || submitted)\">", s.Column);
                    outFile.WriteLine("                            <div ng-show=\"createForm.{0}.$error.required\" class=\"help-block\">Required</div>", s.Column);
                    outFile.WriteLine("                        </div>");
                    outFile.WriteLine("                    </div>");
                    outFile.WriteLine("                </div>");
                }
            }

            var singularize = new Singularization();

            foreach (var r in db.Relations.Where(p => p.ChildTable == tableName).OrderBy(p => p.ParentTable))
            {
                var parent = singularize.Singularize(r.ParentTable.UpperedFirstChar());

                outFile.WriteLine("                <div class=\"form-group\" ng-class=\"{{ 'has-error' : createForm.Selected{0}.$invalid && (createForm.Selected{0}.$dirty || submitted)}}\">", parent);
                outFile.WriteLine("                    <div class=\"col-md-3 control-label\">");
                outFile.WriteLine("                        {0}", parent);
                outFile.WriteLine("                    </div>");
                outFile.WriteLine("                    <div class=\"col-md-9\">");
                outFile.WriteLine("                        <select class=\"form-control\" ng-model=\"{0}.{1}\" ng-options=\"item.ID as item.Name for item in available{2}\" >",
                    className.LoweredFirstChar(), r.PkConstraintColumn, r.ParentTable.UpperedFirstChar());
                outFile.WriteLine("                            <option value=\"\">Select...</option>");
                outFile.WriteLine("                        </select>");
                outFile.WriteLine("                        <div ng-show=\"createForm.Selected{0}.$invalid && (createForm.Selected{0}.$dirty || submitted)\">", parent);
                outFile.WriteLine("                            <div ng-show=\"createForm.Selected{0}.$error.required\" class=\"help-block\">Required</div>", parent);
                outFile.WriteLine("                        </div>");
                outFile.WriteLine("                    </div>");
                outFile.WriteLine("                </div>");
            }

            outFile.WriteLine("            </div>");
            outFile.WriteLine("        </div>");
            outFile.WriteLine("        <div class=\"modal-footer\">");
            outFile.WriteLine("            <input class=\"btn btn-default btn-sm\" type=\"button\" ng-click=\"cancel()\" value=\"Cancel\" />");
            outFile.WriteLine("            <input class=\"btn btn-primary btn-sm\" type=\"submit\" ng-click=\"submitted=true\" value=\"Create\" />");
            outFile.WriteLine("        </div>");
            outFile.WriteLine("    </form>");
            outFile.WriteLine("</script>");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }
                
        private static void WriterForEdit(string path, string tableName, string className, DatabaseInfo db)
        {
            Folder.Create(string.Format(@"{0}\{1}", path, className.UpperedFirstChar()));

            var textPath = string.Format(@"{0}\{1}\_{2}.cshtml", path, className.UpperedFirstChar(), CreationType.Edit);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("<script type=\"text/ng-template\" id=\"editModal.html\">");
            outFile.WriteLine("    <div class=\"modal-header\">");
            outFile.WriteLine("        <h3 class=\"modal-title\">Edit @ViewBag.Title</h3>");
            outFile.WriteLine("    </div>");
            outFile.WriteLine("    <form name=\"editForm\" novalidate ng-submit=\"update({0})\">", className.LoweredFirstChar());
            outFile.WriteLine("        <div class=\"modal-body\">");
            outFile.WriteLine("            <div class=\"form-horizontal\">");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                if (s.ColumnType == ColumnType.None)
                {
                    outFile.WriteLine("                <div class=\"form-group\" ng-class=\"{{ 'has-error' : editForm.{0}.$invalid && (editForm.{0}.$dirty || submitted)}}\">", s.Column);
                    outFile.WriteLine("                    <div class=\"col-md-3 control-label\">");
                    outFile.WriteLine("                        {0}", s.Column);
                    outFile.WriteLine("                    </div>");
                    outFile.WriteLine("                    <div class=\"col-md-9\">");
                    outFile.WriteLine("                        <input type=\"text\" name=\"{0}\" ng-model=\"{1}.{0}\" placeholder=\"\" class=\"form-control input-sm\" ng-required=\"true\" />", s.Column, className.LoweredFirstChar());
                    outFile.WriteLine("                        <div ng-show=\"editForm.{0}.$invalid && (createForm.{0}.$dirty || submitted)\">", s.Column);
                    outFile.WriteLine("                            <div ng-show=\"editForm.{0}.$error.required\" class=\"help-block\">Required</div>", s.Column);
                    outFile.WriteLine("                        </div>");
                    outFile.WriteLine("                    </div>");
                    outFile.WriteLine("                </div>");
                }
            }

            var singularize = new Singularization();

            foreach (var r in db.Relations.Where(p => p.ChildTable == tableName).OrderBy(p => p.ParentTable))
            {
                var parent = singularize.Singularize(r.ParentTable.UpperedFirstChar());

                outFile.WriteLine("                <div class=\"form-group\" ng-class=\"{{ 'has-error' : editForm.Selected{0}.$invalid && (editForm.Selected{0}.$dirty || submitted)}}\">", parent);
                outFile.WriteLine("                    <div class=\"col-md-3 control-label\">");
                outFile.WriteLine("                        {0}", parent);
                outFile.WriteLine("                    </div>");
                outFile.WriteLine("                    <div class=\"col-md-9\">");
                outFile.WriteLine("                        <select class=\"form-control\" ng-model=\"{0}.{1}\" ng-options=\"item.ID as item.Name for item in available{2}\" >",
                    className.LoweredFirstChar(), r.PkConstraintColumn, r.ParentTable.UpperedFirstChar());
                outFile.WriteLine("                            <option value=\"\">Select...</option>");
                outFile.WriteLine("                        </select>");
                outFile.WriteLine("                        <div ng-show=\"editForm.Selected{0}.$invalid && (editForm.Selected{0}.$dirty || submitted)\">", parent);
                outFile.WriteLine("                            <div ng-show=\"editForm.Selected{0}.$error.required\" class=\"help-block\">Required</div>", parent);
                outFile.WriteLine("                        </div>");
                outFile.WriteLine("                    </div>");
                outFile.WriteLine("                </div>");
            }

            outFile.WriteLine("            </div>");
            outFile.WriteLine("        </div>");
            outFile.WriteLine("        <div class=\"modal-footer\">");
            outFile.WriteLine("            <input class=\"btn btn-default btn-sm\" type=\"button\" ng-click=\"cancel()\" value=\"Cancel\" />");
            outFile.WriteLine("            <input class=\"btn btn-primary btn-sm\" type=\"submit\" ng-click=\"submitted=true\" value=\"Update\" />");
            outFile.WriteLine("        </div>");
            outFile.WriteLine("    </form>");
            outFile.WriteLine("</script>");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForDelete(string path, string tableName, string className, DatabaseInfo db)
        {
            var id = db.Schemas.FirstOrDefault(p => p.Table == tableName && p.ColumnType == ColumnType.PK);

            Folder.Create(string.Format(@"{0}\{1}", path, className.UpperedFirstChar()));

            var textPath = string.Format(@"{0}\{1}\_{2}.cshtml", path, className.UpperedFirstChar(), CreationType.Delete);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("<script type=\"text/ng-template\" id=\"deleteModal.html\">");
            outFile.WriteLine("    <div class=\"modal-header\">");
            outFile.WriteLine("        <h3 class=\"modal-title\">Delete this @ViewBag.Title?</h3>");
            outFile.WriteLine("    </div>");
            outFile.WriteLine("    <div class=\"modal-body\">");
            outFile.WriteLine("        <div class=\"form-horizontal\">");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                if (s.ColumnType == ColumnType.None)
                {
                    outFile.WriteLine("            <div class=\"form-group\">");
                    outFile.WriteLine("                <div class=\"col-md-3 control-label\">");
                    outFile.WriteLine("                    {0}", s.Column);
                    outFile.WriteLine("                </div>");
                    outFile.WriteLine("                <div class=\"col-md-9\">");
                    outFile.WriteLine("                    <input type=\"text\" name=\"{0}\" ng-model=\"{1}.{0}\" placeholder=\"\" class=\"form-control input-sm\" />", s.Column, className.LoweredFirstChar());
                    outFile.WriteLine("                </div>");
                    outFile.WriteLine("            </div>");
                }
            }

            foreach (var r in db.Relations.Where(p => p.ChildTable == tableName).OrderBy(p => p.ParentTable))
            {
                outFile.WriteLine("            <div class=\"form-group\">");
                outFile.WriteLine("                <div class=\"col-md-3 control-label\">");
                outFile.WriteLine("                    {0}", r.ParentTable.UpperedFirstChar());
                outFile.WriteLine("                </div>");
                outFile.WriteLine("                <div class=\"col-md-9\">");
                outFile.WriteLine("                    <select class=\"form-control\" ng-model=\"{0}.{1}\" ng-options=\"item.ID as item.Name for item in available{2}\">",
                    className.LoweredFirstChar(), r.PkConstraintColumn, r.ParentTable.UpperedFirstChar());
                outFile.WriteLine("                </div>");
                outFile.WriteLine("            </div>");
            }

            outFile.WriteLine("        </div>");
            outFile.WriteLine("    </div>");
            outFile.WriteLine("	   <div class=\"modal-footer\">");
            outFile.WriteLine("        <button class=\"btn btn-default btn-sm\" ng-click=\"ok()\">Cancel</button>");
            outFile.WriteLine("        <button class=\"btn btn-danger btn-sm\" ng-click=\"delete({0}.{1})\">Delete</button>", className.LoweredFirstChar(), id.Column);
            outFile.WriteLine("    </div>");
            outFile.WriteLine("</script>");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForGrid(string path, string tableName, string className, DatabaseInfo db)
        {
            var pluralize = new Pluralization();
            var id = db.Schemas.FirstOrDefault(p => p.Table == tableName && p.ColumnType == ColumnType.PK);

            Folder.Create(string.Format(@"{0}\{1}", path, className.UpperedFirstChar()));

            var textPath = string.Format(@"{0}\{1}\_{2}.cshtml", path, className.UpperedFirstChar(), CreationType.Grid);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("<div class=\"span12 text-right\">");
            outFile.WriteLine("    <p><button class=\"btn btn-default btn-sm\" ng-click=\"openModal('', 'createModal.html')\">Create {0}</button></p>", className);
            outFile.WriteLine("</div>");
            outFile.WriteLine("");
            outFile.WriteLine("<table class=\"table table-striped table-condensed\">");
            outFile.WriteLine("    <thead>");
            outFile.WriteLine("        <tr>");
            outFile.WriteLine("            <th>No.</th>");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                outFile.WriteLine("            <th><a href=\"\" ng-click=\"sort('{0}')\">{0}</a></th>", s.Column);
            }

            outFile.WriteLine("            <th></th>");
            outFile.WriteLine("        </tr>");
            outFile.WriteLine("    </thead>");
            outFile.WriteLine("    <tbody>");
            outFile.WriteLine("        <tr ng-repeat=\"{0} in {1}\">",className.LoweredFirstChar(), pluralize.Pluralize(className.LoweredFirstChar()));
            
            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                outFile.WriteLine("            <td>{{{{{0}.{1}}}}}</td>", className.LoweredFirstChar(), s.Column);
            }

            outFile.WriteLine("            <td>");
            outFile.WriteLine("                <button class=\"btn btn-default btn-sm\" ng-click=\"openModal({0}.{1}, 'detailsModal.html')\">Details</button>", className.LoweredFirstChar(), id.Column);
            outFile.WriteLine("                <button class=\"btn btn-default btn-sm\" ng-click=\"openModal(product.ProductID, 'editModal.html')\">Edit</button>", className.LoweredFirstChar(), id.Column);
            outFile.WriteLine("                <button class=\"btn btn-default btn-sm\" ng-click=\"openModal(product.ProductID, 'deleteModal.html')\">Delete</button>", className.LoweredFirstChar(), id.Column);
            outFile.WriteLine("            </td>");
            outFile.WriteLine("        </tr>");
            outFile.WriteLine("    </tbody>");
            outFile.WriteLine("</table>");
            outFile.WriteLine("<pagination direction-links=\"false\" boundary-links=\"true\" total-items=\"pagingInfo.totalItems\" ng-model=\"pagingInfo.page\" ng-change=\"pageChanged(page)\"></pagination>");
            
            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
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

            Console.Write(string.Format("\n{0} created", textPath));
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

            Console.Write(string.Format("\n{0} created", textPath));
        }
    }
}
