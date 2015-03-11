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
    public class DbFileToKnockoutUI
    {
        public static void Generate(DatabaseInfo db)
        {
            var path = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.KOViews);

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

                // create views
                // index
                WriterForIndex(path, tableName, className, db);
                // details
                WriterForDetails(path, tableName, className, db);
                // create
                WriterForCreate(path, tableName, className, db);
                // edit
                WriterForEdit(path, tableName, className, db);
                // delete
                WriterForDelete(path, tableName, className, db);
                // grid
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
            outFile.WriteLine("    @Styles.Render(\"~/Content/css\")");
            outFile.WriteLine("    @Scripts.Render(\"~/bundles/modernizr\")");
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
            outFile.WriteLine("    @Scripts.Render(\"~/bundles/jquery\")");
            outFile.WriteLine("    @Scripts.Render(\"~/bundles/bootstrap\")");
            outFile.WriteLine("    @Scripts.Render(\"~/bundles/knockout\")");
            outFile.WriteLine("    @Scripts.Render(\"~/bundles/knockoutvalidation\")");
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
            outFile.WriteLine("<div class=\"row-fluid\">");
            outFile.WriteLine("    <div data-bind=\"visible: currentView() == 'List'\">");
            outFile.WriteLine("        @Html.Partial(\"_Grid\")");
            outFile.WriteLine("    </div>");
            outFile.WriteLine("    <div data-bind=\"visible: currentView() == 'Details'\">");
            outFile.WriteLine("        @Html.Partial(\"_Details\")");
            outFile.WriteLine("    </div>");
            outFile.WriteLine("    <div data-bind=\"visible: currentView() == 'Create'\">");
            outFile.WriteLine("        @Html.Partial(\"_Create\")");
            outFile.WriteLine("    </div>");
            outFile.WriteLine("    <div data-bind=\"visible: currentView() == 'Edit'\">");
            outFile.WriteLine("        @Html.Partial(\"_Edit\")");
            outFile.WriteLine("    </div>");
            outFile.WriteLine("    <div data-bind=\"visible: currentView() == 'Delete'\">");
            outFile.WriteLine("        @Html.Partial(\"_Delete\")");
            outFile.WriteLine("    </div>");
            outFile.WriteLine("</div>");
            outFile.WriteLine("@Scripts.Render(\"~/bundles/{0}\")", className.ToLower());

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForDetails(string path, string tableName, string className, DatabaseInfo db)
        {
            Folder.Create(string.Format(@"{0}\{1}", path, className.UpperedFirstChar()));

            var textPath = string.Format(@"{0}\{1}\_{2}.cshtml", path, className.UpperedFirstChar(), CreationType.Details);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("<h2>@ViewBag.Title Details</h2>");
            outFile.WriteLine("<!-- ko with: model -->");
            outFile.WriteLine("<div class=\"form-horizontal\">");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                outFile.WriteLine("    <div class=\"form-group\">");
                outFile.WriteLine("        <div class=\"col-md-2 control-label\">");
                outFile.WriteLine("            {0}", s.Column);
                outFile.WriteLine("        </div>");
                outFile.WriteLine("        <div class=\"col-md-10\">");
                outFile.WriteLine("            <div data-bind=\"value: {0}\" class=\"form-control input-sm\" ></div>", s.Column);
                outFile.WriteLine("        </div>");
                outFile.WriteLine("    </div>");
            }

            foreach (var r in db.Relations.Where(p => p.ChildTable == tableName).OrderBy(p => p.ParentTable))
            {
                outFile.WriteLine("    <div class=\"form-group\">");
                outFile.WriteLine("        <div class=\"col-md-2 control-label\">");
                outFile.WriteLine("            {0}", r.ParentTable.UpperedFirstChar());
                outFile.WriteLine("        </div>");
                outFile.WriteLine("        <div class=\"col-md-10\">");
                outFile.WriteLine("            <select data-bind=\"options: $root.available{0}List, optionsText: 'Name', optionsValue: 'ID', optionsCaption: 'Choose...', value: {1}\" class=\"form-control\" disabled></select>", r.ParentTable.UpperedFirstChar(), r.FkConstraintColumn);
                outFile.WriteLine("        </div>");
                outFile.WriteLine("    </div>");
            }

            outFile.WriteLine("</div>");
            outFile.WriteLine("<!-- /ko -->");
            outFile.WriteLine("<div class=\"form-group\">");
            outFile.WriteLine("    <a href=\"#\" class=\"btn btn-default btn-sm\" data-bind=\"click: showList\">Back to List</a>&nbsp;");
            outFile.WriteLine("    <a href=\"#\" class=\"btn btn-default btn-sm\" data-bind=\"text: 'Edit', click: $root.showEdit\"></a>");
            outFile.WriteLine("</div>");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForCreate(string path, string tableName, string className, DatabaseInfo db)
        {
            Folder.Create(string.Format(@"{0}\{1}", path, className.UpperedFirstChar()));

            var textPath = string.Format(@"{0}\{1}\_{2}.cshtml", path, className.UpperedFirstChar(), CreationType.Create);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("<h2>New @ViewBag.Title</h2>");
            outFile.WriteLine("<!-- ko with: model -->");
            outFile.WriteLine("<div class=\"form-horizontal\">");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                outFile.WriteLine("    <div class=\"form-group\"  data-bind=\"validationElement: {0}\">", s.Column);
                outFile.WriteLine("        <div class=\"col-md-2 control-label\">");
                outFile.WriteLine("            {0}", s.Column);
                outFile.WriteLine("        </div>");
                outFile.WriteLine("        <div class=\"col-md-10\">");
                outFile.WriteLine("            <input data-bind=\"value: {0}\" class=\"form-control input-sm\" />", s.Column);
                outFile.WriteLine("        </div>");
                outFile.WriteLine("    </div>");
            }
            
            foreach (var r in db.Relations.Where(p => p.ChildTable == tableName).OrderBy(p => p.ParentTable))
            {
                outFile.WriteLine("    <div class=\"form-group\"  data-bind=\"validationElement: {0}\">", r.FkConstraintColumn);
                outFile.WriteLine("        <div class=\"col-md-2 control-label\">");
                outFile.WriteLine("            {0}", r.ParentTable.UpperedFirstChar());
                outFile.WriteLine("        </div>");
                outFile.WriteLine("        <div class=\"col-md-10\">");
                outFile.WriteLine("            <select data-bind=\"options: $root.available{0}List, optionsText: 'Name', optionsValue: 'ID', optionsCaption: 'Choose...', value: {1}\" class=\"form-control\" ></select>", r.ParentTable.UpperedFirstChar(), r.FkConstraintColumn);
                outFile.WriteLine("        </div>");
                outFile.WriteLine("    </div>");
            }

            outFile.WriteLine("</div>");
            outFile.WriteLine("<!-- /ko -->");
            outFile.WriteLine("<div class=\"form-group\">");
            outFile.WriteLine("    <a href=\"#\" class=\"btn btn-default btn-sm\" data-bind=\"click: showList\">Back to List</a>&nbsp;");
            outFile.WriteLine("    <a href=\"#\" class=\"btn btn-primary btn-sm\" data-bind=\"text: 'Create', click: $root.clickCreate\"></a>");
            outFile.WriteLine("</div>");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForEdit(string path, string tableName, string className, DatabaseInfo db)
        {
            Folder.Create(string.Format(@"{0}\{1}", path, className.UpperedFirstChar()));

            var textPath = string.Format(@"{0}\{1}\_{2}.cshtml", path, className.UpperedFirstChar(), CreationType.Edit);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("<h2>Edit @ViewBag.Title</h2>");
            outFile.WriteLine("<!-- ko with: model -->");
            outFile.WriteLine("<div class=\"form-horizontal\">");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                if (s.ColumnType == "PK")
                    continue;

                outFile.WriteLine("    <div class=\"form-group\">");
                outFile.WriteLine("        <div class=\"col-md-2 control-label\">");
                outFile.WriteLine("            {0}", s.Column);
                outFile.WriteLine("        </div>");
                outFile.WriteLine("        <div class=\"col-md-10\">");
                outFile.WriteLine("            <input data-bind=\"value: {0}\" class=\"form-control input-sm\" />", s.Column);
                outFile.WriteLine("        </div>");
                outFile.WriteLine("    </div>");
            }

            foreach (var r in db.Relations.Where(p => p.ChildTable == tableName).OrderBy(p => p.ParentTable))
            {
                outFile.WriteLine("    <div class=\"form-group\">");
                outFile.WriteLine("        <div class=\"col-md-2 control-label\">");
                outFile.WriteLine("            {0}", r.ParentTable.UpperedFirstChar());
                outFile.WriteLine("        </div>");
                outFile.WriteLine("        <div class=\"col-md-10\">");
                outFile.WriteLine("            <select data-bind=\"options: $root.available{0}List, optionsText: 'Name', optionsValue: 'ID', optionsCaption: 'Choose...', value: {1}\" class=\"form-control\" ></select>", r.ParentTable.UpperedFirstChar(), r.FkConstraintColumn);
                outFile.WriteLine("        </div>");
                outFile.WriteLine("    </div>");
            }

            outFile.WriteLine("</div>");
            outFile.WriteLine("<!-- /ko -->");
            outFile.WriteLine("<div class=\"form-group\">");
            outFile.WriteLine("    <a href=\"#\" class=\"btn btn-default btn-sm\" data-bind=\"click: showList\">Back to List</a>&nbsp;");
            outFile.WriteLine("    <a href=\"#\" class=\"btn btn-primary btn-sm\" data-bind=\"text: 'Edit', click: $root.clickEdit\"></a>");
            outFile.WriteLine("</div>");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForDelete(string path, string tableName, string className, DatabaseInfo db)
        {
            Folder.Create(string.Format(@"{0}\{1}", path, className.UpperedFirstChar()));

            var textPath = string.Format(@"{0}\{1}\_{2}.cshtml", path, className.UpperedFirstChar(), CreationType.Delete);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("<h2>Delete @ViewBag.Title</h2>");
            outFile.WriteLine("<h3>Are you sure you want to delete this {0}?</h3>", className.UpperedFirstChar());
            outFile.WriteLine("<!-- ko with: model -->");
            outFile.WriteLine("<div class=\"form-horizontal\">");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                outFile.WriteLine("    <div class=\"form-group\">");
                outFile.WriteLine("        <div class=\"col-md-2 control-label\">");
                outFile.WriteLine("            {0}", s.Column);
                outFile.WriteLine("        </div>");
                outFile.WriteLine("        <div class=\"col-md-10\">");
                outFile.WriteLine("            <div data-bind=\"value: {0}\" class=\"form-control input-sm\" ></div>", s.Column);
                outFile.WriteLine("        </div>");
                outFile.WriteLine("    </div>");
            }

            foreach (var r in db.Relations.Where(p => p.ChildTable == tableName).OrderBy(p => p.ParentTable))
            {
                outFile.WriteLine("    <div class=\"form-group\">");
                outFile.WriteLine("        <div class=\"col-md-2 control-label\">");
                outFile.WriteLine("            {0}", r.ParentTable.UpperedFirstChar());
                outFile.WriteLine("        </div>");
                outFile.WriteLine("        <div class=\"col-md-10\">");
                outFile.WriteLine("            <select data-bind=\"options: $root.available{0}List, optionsText: 'Name', optionsValue: 'ID', optionsCaption: 'Choose...', value: {1}\" class=\"form-control\" disabled></select>", r.ParentTable.UpperedFirstChar(), r.FkConstraintColumn);
                outFile.WriteLine("        </div>");
                outFile.WriteLine("    </div>");
            }

            outFile.WriteLine("</div>");
            outFile.WriteLine("<!-- /ko -->");
            outFile.WriteLine("<div class=\"form-group\">");
            outFile.WriteLine("    <a href=\"#\" class=\"btn btn-default btn-sm\" data-bind=\"click: showList\">Back to List</a>&nbsp;");
            outFile.WriteLine("    <a href=\"#\" class=\"btn btn-danger btn-sm\" data-bind=\"text: 'Delete', click: $root.clickDelete\"></a>");
            outFile.WriteLine("</div>");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForGrid(string path, string tableName, string className, DatabaseInfo db)
        {
            Folder.Create(string.Format(@"{0}\{1}", path, className.UpperedFirstChar()));

            var textPath = string.Format(@"{0}\{1}\_{2}.cshtml", path, className.UpperedFirstChar(), CreationType.Grid);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("<h2>@ViewBag.Title</h2>");
            outFile.WriteLine("<div class=\"span12 text-right\">");
            outFile.WriteLine("    <p><a href=\"#\" class=\"btn btn-default btn-sm\" data-bind=\"click: showCreate\">Create New {0}</a></p>", className.UpperedFirstChar());
            outFile.WriteLine("</div>");
            outFile.WriteLine("<table class=\"table table-striped table-condensed\">");
            outFile.WriteLine("    <thead>");
            outFile.WriteLine("        <tr>");
            outFile.WriteLine("            <th>No.</th>");
            
            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                outFile.WriteLine("            <th>{0}</th>", s.Column);
            }
            
            outFile.WriteLine("            <th>Discontinued</th>");
            outFile.WriteLine("            <th></th>");
            outFile.WriteLine("            <th></th>");
            outFile.WriteLine("        </tr>");
            outFile.WriteLine("    </thead>");
            outFile.WriteLine("    <tbody data-bind=\"foreach: pagedList\">");
            outFile.WriteLine("        <tr>");
            outFile.WriteLine("            <td data-bind=\"text: $root.itemNumber($index())\"></td>");
            
            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                outFile.WriteLine("            <td data-bind=\"text: {0}\"></td>", s.Column);
            }
            
            outFile.WriteLine("            <td><a href=\"#\" data-bind=\"text: 'Details', click: $root.showDetails\" class=\"btn btn-default btn-sm\"></a></td>");
            outFile.WriteLine("            <td><a href=\"#\" data-bind=\"text: 'Delete', click: $root.showDelete\" class=\"btn btn-danger btn-sm\"></a></td>");
            outFile.WriteLine("        </tr>");
            outFile.WriteLine("    </tbody>");
            outFile.WriteLine("</table>");
            outFile.WriteLine("<ul class=\"pagination\">");
            outFile.WriteLine("    <li data-bind=\"css: { disabled: pageIndex() === 0 }\"><a href=\"#\" data-bind=\"click: previousPage\">Previous</a></li>");
            outFile.WriteLine("</ul>");
            outFile.WriteLine("<ul class=\"pagination\" data-bind=\"foreach: allPages\">");
            outFile.WriteLine("    <li data-bind=\"css: { active: $data.pageNumber === ($root.pageIndex() + 1) }\"><a href=\"#\" data-bind=\"text: $data.pageNumber, click: function() { $root.moveToPage($data.pageNumber-1); }\"></a></li>");
            outFile.WriteLine("</ul>");
            outFile.WriteLine("<ul class=\"pagination\">");
            outFile.WriteLine("    <li data-bind=\"css: { disabled: pageIndex() === maxPageIndex() }\"><a href=\"#\" data-bind=\"click: nextPage\">Next</a></li>");
            outFile.WriteLine("</ul>");

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
