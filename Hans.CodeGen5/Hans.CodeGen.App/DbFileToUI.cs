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
    public class DbFileToUI
    {
        public static void Generate(DatabaseInfo db)
        {
            var path = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.RazorViews);

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

                WriterForIndex2(path, tableName, className, db);
                WriterForDetails2(path, tableName, className, db);
                WriterForCreate2(path, tableName, className, db);
                WriterForEdit2(path, tableName, className, db);
                WriterForDelete2(path, tableName, className, db);
            }

            WriterForErrorCshtml(path, db);
            WriterForFailCshtml(path, db);

            WriterForLayout(path, db);

            Console.WriteLine();
        }

        private static void WriterForHeader(StreamWriter outFile, string applicationName, string className, string actionType = "")
        {
            outFile.WriteLine("@using {0}.Helpers", applicationName);
            outFile.WriteLine("@model {0}.ViewModels.{1}{2}ViewModel", applicationName, className, actionType);
        }

        private static void WriterForFooter(StreamWriter outFile)
        {
            outFile.WriteLine("@section Scripts {");
            outFile.WriteLine("    <script src=\"~/lib/jquery/dist/jquery.min.js\"></script>");
            outFile.WriteLine("    <script src=\"~/lib/jquery-validation/dist/jquery.validate.min.js\"></script>");
            outFile.WriteLine("    <script src=\"~/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js\"></script>");
            outFile.WriteLine("}");
        }

        private static void WriterForFooterScriptPartial(StreamWriter outFile)
        {
            outFile.WriteLine("@section Scripts {");
            outFile.WriteLine("    @{await Html.RenderPartialAsync(\"_ValidationScriptsPartial\");}");
            outFile.WriteLine("}");
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
            outFile.WriteLine("    @RenderSection(\"scripts\", required: false)");
            outFile.WriteLine("</body>");
            outFile.WriteLine("</html>");
            outFile.Close();

            Console.Write(string.Format("\n{0} created", "_Layout.cshtml"));
        }

        private static void WriterForIndex(string path, string tableName, string className, DatabaseInfo db)
        {
            var id = db.Schemas.Where(p => p.Table == tableName).FirstOrDefault().Column;
            Folder.Create(string.Format(@"{0}\{1}", path, className.UpperedFirstChar()));

            var textPath = string.Format(@"{0}\{1}\{2}.cshtml", path, className.UpperedFirstChar(), CreationType.Index);
            var outFile = File.CreateText(textPath);

            //outFile.WriteLine("@model {0}.ViewModels.{1}ViewModel", db.ApplicationName, className);
            //outFile.WriteLine("@using {0}.Helpers", db.ApplicationName);
            WriterForHeader(outFile, db.ApplicationName, className);

            outFile.WriteLine("");
            outFile.WriteLine("@{");
            outFile.WriteLine("    ViewBag.Title = \"{0}\";", className);
            outFile.WriteLine("    Layout = \"~/Views/Shared/_Layout.cshtml\";");
            outFile.WriteLine("}");
            outFile.WriteLine("");
            outFile.WriteLine("<div class=\"page-header\">");
            outFile.WriteLine("    <h2>{0} <small>Manage {1}</small></h2>", className, className.LoweredFirstChar());
            outFile.WriteLine("</div>");
            outFile.WriteLine("");
            outFile.WriteLine("<div class=\"clearfix\"></div>");
            outFile.WriteLine("");
            outFile.WriteLine("@using (Html.BeginForm(\"Index\", \"{0}\", FormMethod.Get))", className);
            outFile.WriteLine("{");
            outFile.WriteLine("    <div class=\"search-content form-inline\">");
            outFile.WriteLine("        <div class=\"form-group\">");
            outFile.WriteLine("            <label>Search by:</label>");
            outFile.WriteLine("        </div>");
            outFile.WriteLine("        <div class=\"form-group\">");
            outFile.WriteLine("            @Html.TextBox(\"query\", ViewBag.CurrentQuery as string, new { @class = \"form-control input-sm\", style=\"width: 200px\" })");
            outFile.WriteLine("        </div>");
            outFile.WriteLine("        <button type=\"submit\" class=\"btn btn-sm btn-default\">Search</button>");
            outFile.WriteLine("    </div>");
            outFile.WriteLine("}");
            outFile.WriteLine("");
            outFile.WriteLine("<div class=\"clearfix\"></div>");
            outFile.WriteLine("");
            outFile.WriteLine("<table class=\"table table-bordered table-striped\">");
            outFile.WriteLine("    <thead>");
            outFile.WriteLine("        <tr>");
            outFile.WriteLine("            <th>No.</th>");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                outFile.WriteLine("            <th>@Html.SortLinks(\"{0}\", \"{0}\", Model.Sort, Model.CurrentPage, Model.IsAsc, ViewBag.CurrentQuery as string)</th>", s.Column);
            }

            outFile.WriteLine("        </tr>");
            outFile.WriteLine("    </thead>");
            outFile.WriteLine("    <tbody>");
            outFile.WriteLine("        @foreach (var item in Model.{0}s)", className);
            outFile.WriteLine("        {");

            outFile.WriteLine("            Model.PageIndex++;");
            outFile.WriteLine("            <tr>");
            outFile.WriteLine("                <td>@Model.PageIndex</td>");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                outFile.WriteLine("                <td>@item.{0}</td>", s.Column);
            }

            outFile.WriteLine("                <td>");
            outFile.WriteLine("                    <div class=\"btn-group\">");
            outFile.WriteLine("                        <a href=\"#\" class=\"btn btn-xs btn-default\">Action</a>");
            outFile.WriteLine("                        <a href=\"#\" class=\"btn btn-xs btn-default dropdown-toggle\" data-toggle=\"dropdown\" aria-expanded=\"false\"><span class=\"caret\"></span></a>");
            outFile.WriteLine("                        <ul class=\"dropdown-menu\">");
            outFile.WriteLine("                            <li>@Html.ActionLink(\"Details\", \"Details\", new {{ id = item.{0} }})</li>", id);
            outFile.WriteLine("                            <li>@Html.ActionLink(\"Edit\", \"Edit\", new {{ id = item.{0} }})</li>", id);
            outFile.WriteLine("                            <li>@Html.ActionLink(\"Delete\", \"Delete\", new {{ id = item.{0} }})</li>", id);
            outFile.WriteLine("                        </ul>");
            outFile.WriteLine("                    </div>");
            outFile.WriteLine("                </td>");
            outFile.WriteLine("            </tr>");
            outFile.WriteLine("        }");
            outFile.WriteLine("    </tbody>");
            outFile.WriteLine("</table>");
            outFile.WriteLine();
            outFile.WriteLine("@Html.PageLinks(Model.PageSize, Model.TotalPages, Model.CurrentPage, ViewBag.CurrentQuery as string)");
            outFile.WriteLine();
            outFile.WriteLine("<div class=\"clearfix\"></div>");
            outFile.WriteLine();
            outFile.WriteLine("@Html.ActionLink(\"Create New\", \"Create\", \"{0}\", null, new {{ @class = \"btn btn-sm btn-primary\" }})", className);

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForIndex2(string path, string tableName, string className, DatabaseInfo db)
        {
            var id = db.Schemas.Where(p => p.Table == tableName).FirstOrDefault().Column;
            Folder.Create(string.Format(@"{0}\{1}", path, className.UpperedFirstChar()));

            var textPath = string.Format(@"{0}\{1}\{2}.cshtml", path, className.UpperedFirstChar(), CreationType.Index);
            var outFile = File.CreateText(textPath);

            WriterForHeader(outFile, db.ApplicationName, className);
            outFile.WriteLine();

            outFile.WriteLine("@{");
            outFile.WriteLine("    ViewData[\"Title\"] = \"Index\";");
            outFile.WriteLine("}");
            outFile.WriteLine();

            outFile.WriteLine("<div>");
            //outFile.WriteLine("    <a asp-action=\"Create\">Create New</a>");
            outFile.WriteLine("    @Html.ActionLink(\"Create New\", \"Create\", \"{0}\", null, new {{ @class = \"btn btn-sm btn-primary\" }})", className);
            outFile.WriteLine("</div>");
            outFile.WriteLine();

            outFile.WriteLine("<div class=\"clearfix\"></div>");
            outFile.WriteLine();

            //outFile.WriteLine("@using (Html.BeginForm(\"Index\", \"{0}\", FormMethod.Get))", className);
            //outFile.WriteLine("{");
            //outFile.WriteLine("    <div class=\"search-content form-inline\">");
            //outFile.WriteLine("        <div class=\"form-group\">");
            //outFile.WriteLine("            <label>Search by:</label>");
            //outFile.WriteLine("        </div>");
            //outFile.WriteLine("        <div class=\"form-group\">");
            //outFile.WriteLine("            @Html.TextBox(\"query\", ViewBag.CurrentQuery as string, new { @class = \"form-control input-sm\", style=\"width: 200px\" })");
            //outFile.WriteLine("        </div>");
            //outFile.WriteLine("        <button type=\"submit\" class=\"btn btn-sm btn-default\">Search</button>");
            //outFile.WriteLine("    </div>");
            //outFile.WriteLine("}");
            //outFile.WriteLine("");
            //outFile.WriteLine("<div class=\"clearfix\"></div>");
            //outFile.WriteLine();

            outFile.WriteLine("<table class=\"table table-bordered table-striped\">");
            outFile.WriteLine("    <thead>");
            outFile.WriteLine("        <tr>");
            outFile.WriteLine("            <th>No.</th>");

            var unusedFields = db.RemoveFields.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                if (!unusedFields.Any(x => x == s.Column))
                {
                    outFile.WriteLine("            <th>@Html.SortLinks(\"{0}\", \"{0}\", Model.Sort, Model.CurrentPage, Model.IsAsc, ViewBag.CurrentQuery as string)</th>", s.Column);
                }
            }

            outFile.WriteLine("        </tr>");
            outFile.WriteLine("    </thead>");
            outFile.WriteLine("    <tbody>");
            outFile.WriteLine("    @foreach (var item in Model.{0}s)", className);
            outFile.WriteLine("    {");

            outFile.WriteLine("        Model.PageIndex++;");
            outFile.WriteLine("        <tr>");
            outFile.WriteLine("            <td>@Model.PageIndex</td>");
            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                if (!unusedFields.Any(x => x == s.Column))
                {
                    if (s.Column == "Featured")
                    {
                        outFile.WriteLine("            <td><input class=\"form-check-input\" type=\"checkbox\" checked=\"@item.{0}\" /></td>", s.Column);
                    }
                    else
                    {
                        outFile.WriteLine("            <td>@item.{0}</td>", s.Column);
                    }
                }
            }

            outFile.WriteLine("            <td>");
            outFile.WriteLine("                <a asp-action=\"Details\" asp-route-id=\"@item.{0}\">Details</a>", id);
            outFile.WriteLine("                <a asp-action=\"Edit\" asp-route-id=\"@item.{0}\">Edit</a>", id);
            outFile.WriteLine("                <a asp-action=\"Delete\" asp-route-id=\"@item.{0}\">Delete</a>", id);
            outFile.WriteLine("            </td>");
            outFile.WriteLine("        </tr>");
            outFile.WriteLine("    }");
            outFile.WriteLine("    </tbody>");
            outFile.WriteLine("</table>");
            outFile.WriteLine();

            outFile.WriteLine("@Html.PageLinks(Model.PageSize, Model.TotalPages, Model.CurrentPage, ViewBag.CurrentQuery as string)");
            outFile.WriteLine();

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForCreate(string path, string tableName, string className, DatabaseInfo db)
        {
            Folder.Create(string.Format(@"{0}\{1}", path, className.UpperedFirstChar()));

            var textPath = string.Format(@"{0}\{1}\{2}.cshtml", path, className.UpperedFirstChar(), CreationType.Create);
            var outFile = File.CreateText(textPath);

            WriterForHeader(outFile, db.ApplicationName, className, "Read");
            outFile.WriteLine("");
            outFile.WriteLine("@{");
            outFile.WriteLine("    ViewBag.Title = \"{0}\";", className);
            outFile.WriteLine("    Layout = \"~/Views/Shared/_Layout.cshtml\";");
            outFile.WriteLine("}");
            outFile.WriteLine("");
            outFile.WriteLine("<div class=\"page-header\">");
            outFile.WriteLine("    <h2>{0} <small>Create {1}</small></h2>", className, className.LoweredFirstChar());
            outFile.WriteLine("</div>");
            outFile.WriteLine("");
            outFile.WriteLine("<div class=\"clearfix\"></div>");
            outFile.WriteLine("");

            outFile.WriteLine("@using (Html.BeginForm())");
            outFile.WriteLine("{");
            outFile.WriteLine("    @Html.AntiForgeryToken()");
            outFile.WriteLine("    <div class=\"form-horizontal\">");
            outFile.WriteLine("        @Html.ValidationSummary(true)");
            outFile.WriteLine("");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                if (s.Column.ToLower() == "id")
                    continue;

                outFile.WriteLine("        <div class=\"form-group\">");
                outFile.WriteLine("            @Html.LabelFor(model => model.{0}, new {{ @class = \"control-label col-md-2\" }})", s.Column);
                outFile.WriteLine("            <div class=\"col-md-10\">");
                outFile.WriteLine("                @Html.TextBoxFor(model => model.{0}, new {{ @class = \"form-control input-sm\" }})", s.Column);
                outFile.WriteLine("                @Html.ValidationMessageFor(model => model.{0})", s.Column);
                outFile.WriteLine("            </div>");
                outFile.WriteLine("        </div>");
                outFile.WriteLine("");
            }
            outFile.WriteLine("    </div>");
            outFile.WriteLine("    <div class=\"form-group\">");
            outFile.WriteLine("        <input type=\"submit\" value=\"Create\" class=\"btn btn-sm btn-primary\" />");
            outFile.WriteLine("        @Html.ActionLink(\"Back to List\", \"Index\", \"{0}\", null, new {{ @class = \"btn btn-sm btn-primary\" }})", className);
            outFile.WriteLine("    </div>");
            outFile.WriteLine("}");
            outFile.WriteLine("");
            //outFile.WriteLine("@section Scripts {");
            //outFile.WriteLine("    @*TODO : Until script helpers are available, adding script references manually*@");
            //outFile.WriteLine("    @*@Scripts.Render(\"~/ bundles / jqueryval\")*@");
            //outFile.WriteLine("    <script src=\"@Url.Content(\"~/Scripts/jquery.validate.js\")\"></script>");
            //outFile.WriteLine("    <script src=\"@Url.Content(\"~/Scripts/jquery.validate.unobtrusive.js\")\"></script>");
            //outFile.WriteLine("}");
            WriterForFooter(outFile);

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForCreate2(string path, string tableName, string className, DatabaseInfo db)
        {
            var unusedFields = db.RemoveFields.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            Folder.Create(string.Format(@"{0}\{1}", path, className.UpperedFirstChar()));

            var textPath = string.Format(@"{0}\{1}\{2}.cshtml", path, className.UpperedFirstChar(), CreationType.Create);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("@using CFF.Helpers.Common");
            outFile.WriteLine("@model {0}.ViewModels.{1}{2}ViewModel", db.ApplicationName, className, "Create");
            outFile.WriteLine();

            outFile.WriteLine("@{");
            outFile.WriteLine("    ViewData[\"Title\"] = \"Create\";");
            outFile.WriteLine("}");
            outFile.WriteLine();

            outFile.WriteLine("<h1>Create</h1>");
            outFile.WriteLine("<h4>{0}</h4>", className);
            outFile.WriteLine();
            outFile.WriteLine("<div class=\"clearfix\"></div>");
            outFile.WriteLine();

            outFile.WriteLine("<div class=\"row\">");
            outFile.WriteLine("    <div class=\"col-md-4\">");
            outFile.WriteLine("        <form asp-action=\"Create\" enctype=\"multipart/form-data\">");
            outFile.WriteLine("            <div asp-validation-summary=\"ModelOnly\" class=\"text-danger\"></div>");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                if (s.Column.ToLower() == "id")
                    continue;

                if (unusedFields.Any(x => x == s.Column))
                    continue;

                if (s.Column == "Status")
                {
                    outFile.WriteLine("            <div class=\"form-group\">");
                    outFile.WriteLine("                <label asp-for=\"{0}\" class=\"control-label\"></label>", s.Column);
                    outFile.WriteLine("                <br />");
                    outFile.WriteLine("                @Html.DropDownListFor(x => ");
                    outFile.WriteLine("                    x.Status,");
                    outFile.WriteLine("                    new SelectList(Enum.GetValues(typeof(StatusType))),");
                    outFile.WriteLine("                    \"Select..\",");
                    outFile.WriteLine("                    new { @class = \"form-control\" })");
                    outFile.WriteLine("                <span asp-validation-for=\"Status\" class=\"text-danger\"></span>");
                    outFile.WriteLine("            </div>");
                }
                else if (s.Column == "Featured")
                {
                    outFile.WriteLine("            <div class=\"form-group\">");
                    outFile.WriteLine("                <label asp-for=\"{0}\" class=\"control-label\"></label>", s.Column);
                    outFile.WriteLine("                <br />");
                    outFile.WriteLine("                <input class=\"form-check-input\" type=\"checkbox\" asp-for=\"Featured\" />");
                    outFile.WriteLine("            </div>");
                }
                else
                {
                    outFile.WriteLine("            <div class=\"form-group\">");
                    outFile.WriteLine("                <label asp-for=\"{0}\" class=\"control-label\"></label>", s.Column);
                    outFile.WriteLine("                <input asp-for=\"{0}\" class=\"form-control\" />", s.Column);
                    outFile.WriteLine("                <span asp-validation-for=\"{0}\" class=\"text-danger\"></span>", s.Column);
                    outFile.WriteLine("            </div>");
                }
            }

            outFile.WriteLine("            <div class=\"form-group\">");
            outFile.WriteLine("                <input type=\"submit\" value=\"Create\" class=\"btn btn-sm btn-primary\" />");
            //outFile.WriteLine("                @Html.ActionLink(\"Back to List\", \"Index\", \"{0}\", null, new {{ @class = \"btn btn-sm btn-secondary\" }})", className);
            outFile.WriteLine("                {0}", BackToListActionLink(className, "secondary"));
            outFile.WriteLine("            </div>");
            outFile.WriteLine("        </form>");
            outFile.WriteLine("    </div>");
            outFile.WriteLine("</div>");

            outFile.WriteLine();

            WriterForFooterScriptPartial(outFile);

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForDetails(string path, string tableName, string className, DatabaseInfo db)
        {
            Folder.Create(string.Format(@"{0}\{1}", path, className.UpperedFirstChar()));
            var id = db.Schemas.Where(p => p.Table == tableName).FirstOrDefault().Column;
            var textPath = string.Format(@"{0}\{1}\{2}.cshtml", path, className.UpperedFirstChar(), CreationType.Details);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("@model {0}.Web.Models.{1}Model", db.NamespaceCs, className);
            outFile.WriteLine("");
            outFile.WriteLine("@{");
            outFile.WriteLine("    ViewBag.Title = \"{0}\";", className);
            outFile.WriteLine("    Layout = \"~/Views/Shared/_Layout.cshtml\";");
            outFile.WriteLine("}");
            outFile.WriteLine("");
            outFile.WriteLine("<div class=\"page-header\">");
            outFile.WriteLine("    <h2>{0} <small>{0} details</small></h2>", className);
            outFile.WriteLine("</div>");
            outFile.WriteLine("");
            outFile.WriteLine("<div class=\"clearfix\"></div>");
            outFile.WriteLine("");
            outFile.WriteLine("<div class=\"form-horizontal\">");
            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                outFile.WriteLine("    <div class=\"form-group\">");
                outFile.WriteLine("        @Html.LabelFor(model => model.{0}, new {{ @class = \"control-label col-md-2\" }})", s.Column);
                outFile.WriteLine("        <div class=\"col-md-10\">");
                outFile.WriteLine("            @Html.LabelFor(model => model.{0})", s.Column);
                if (s.MsSqlDataType() == MsSqlDataType.DateTime)
                {
                    outFile.WriteLine("            @Html.TextBoxFor(model => model.{0}, \"{{0:dd/MM/yyyy}}\", new { @class = \"form-control input-sm\" })", s.Column);
                }
                else
                {
                    outFile.WriteLine("            @Html.TextBoxFor(model => model.{0}, new {{ @class = \"form-control input-sm\" }})", s.Column);
                }
                outFile.WriteLine("        </div>");
                outFile.WriteLine("    </div>");
                outFile.WriteLine("");
            }
            outFile.WriteLine("</div>");
            outFile.WriteLine("");
            outFile.WriteLine("<div class=\"form-group\">");
            outFile.WriteLine("    @Html.ActionLink(\"Edit\", \"Edit\", \"Customer\", null, new {{ id = Model.{0}, @class = \"btn btn-sm btn-primary\" }})", id);
            outFile.WriteLine("    @Html.ActionLink(\"Back to List\", \"Index\", \"Customer\", null, new {{ @class = \"btn btn-sm btn-primary\" }})");
            outFile.WriteLine("</div>");
            outFile.WriteLine("");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForDetails2(string path, string tableName, string className, DatabaseInfo db)
        {
            Folder.Create(string.Format(@"{0}\{1}", path, className.UpperedFirstChar()));
            var id = db.Schemas.Where(p => p.Table == tableName).FirstOrDefault().Column;
            var textPath = string.Format(@"{0}\{1}\{2}.cshtml", path, className.UpperedFirstChar(), CreationType.Details);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("@model {0}.ViewModels.{1}{2}ViewModel", db.ApplicationName, className, "Read");
            outFile.WriteLine();

            outFile.WriteLine("@{");
            outFile.WriteLine("    ViewData[\"Title\"] = \"Details\";");
            outFile.WriteLine("}");
            outFile.WriteLine();

            outFile.WriteLine("<h1>Details</h1>");
            outFile.WriteLine("<h4>{0}</h4>", className);
            outFile.WriteLine();
            outFile.WriteLine("<div class=\"clearfix\"></div>");
            outFile.WriteLine();

            outFile.WriteLine("<div>");
            outFile.WriteLine("    <dl class=\"row\">");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                outFile.WriteLine("        <dt class = \"col-sm-2\">");
                outFile.WriteLine("            @Html.DisplayNameFor(model => model.{0})", s.Column);
                outFile.WriteLine("        </dt>");

                //if (s.Column == "Status")
                //{
                //    outFile.WriteLine("        <dd class = \"col-sm-10\">");
                //    outFile.WriteLine("            @if (@Model.Status == 1)", s.Column);
                //    outFile.WriteLine("            {");
                //    outFile.WriteLine("                <input class=\"form-check-input\" type=\"checkbox\" value=\"\" checked />");
                //    outFile.WriteLine("            }");
                //    outFile.WriteLine("            else");
                //    outFile.WriteLine("            {");
                //    outFile.WriteLine("                <input class=\"form-check-input\" type=\"checkbox\" value=\"\" />");
                //    outFile.WriteLine("            }");
                //    outFile.WriteLine("        </dd>");
                //}
                //else
                //{
                //    outFile.WriteLine("        <dd class = \"col-sm-10\">");
                //    outFile.WriteLine("            @Html.DisplayFor(model => model.{0})", s.Column);
                //    outFile.WriteLine("        </dd>");
                //}
                WriteReadProperty(tableName, outFile, s);
            }

            outFile.WriteLine("        <div class=\"form-group\">");
            outFile.WriteLine("           <a asp-action=\"Edit\" asp-route-id=\"@Model.{0}\" class=\"btn btn-sm btn-primary\">Edit</a>", id);
            //outFile.WriteLine("           @Html.ActionLink(\"Back to List\", \"Index\", \"{0}\", null, new {{ @class = \"btn btn-sm btn-secondary\" }})", className);
            outFile.WriteLine("           {0}", BackToListActionLink(className, "secondary"));
            outFile.WriteLine("        </div>");
            outFile.WriteLine("    </dl>");
            outFile.WriteLine("</div>");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForEdit(string path, string tableName, string className, DatabaseInfo db)
        {
            Folder.Create(string.Format(@"{0}\{1}", path, className.UpperedFirstChar()));
            var id = db.Schemas.Where(p => p.Table == tableName).FirstOrDefault().Column;
            var textPath = string.Format(@"{0}\{1}\{2}.cshtml", path, className.UpperedFirstChar(), CreationType.Edit);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("@model {0}.Models.{1}Model", db.NamespaceCs, className);
            outFile.WriteLine("");
            outFile.WriteLine("@{");
            outFile.WriteLine("    ViewBag.Title = \"Edit - {0}\";", className);
            outFile.WriteLine("}");
            outFile.WriteLine("");
            outFile.WriteLine("<h2>@ViewBag.Title</h2>");
            outFile.WriteLine("");
            outFile.WriteLine("@using (Html.BeginForm())");
            outFile.WriteLine("{");
            outFile.WriteLine("    @Html.AntiForgeryToken()");

            outFile.WriteLine("    <div class=\"form-horizontal\">");
            outFile.WriteLine("        @Html.ValidationSummary(true)");
            outFile.WriteLine("        @Html.HiddenFor(model => model.{0})", id);
            outFile.WriteLine("");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                outFile.WriteLine("        <div class=\"form-group\">");
                outFile.WriteLine("            @Html.LabelFor(model => model.{0}, new {{ @class = \"control-label col-md-2\" }})", s.Column);
                outFile.WriteLine("            <div class=\"col-md-10\">");
                outFile.WriteLine("                @Html.EditorFor(model => model.{0})", s.Column);
                outFile.WriteLine("                @Html.ValidationMessageFor(model => model.{0})", s.Column);
                outFile.WriteLine("            </div>");
                outFile.WriteLine("        </div>");
                outFile.WriteLine("");
            }

            outFile.WriteLine("        <div class=\"form-group\">");
            outFile.WriteLine("            <div class=\"col-md-offset-2 col-md-10\">");
            outFile.WriteLine("                @Html.ActionLink(\"Back to List\", \"Index\") | <input type=\"submit\" value=\"Save\" class=\"btn btn-primary\" />");
            outFile.WriteLine("            </div>");
            outFile.WriteLine("        </div>");
            outFile.WriteLine("    </div>");
            outFile.WriteLine("}");
            outFile.WriteLine("");
            outFile.WriteLine("@section Scripts {");
            outFile.WriteLine("    @Scripts.Render(\"~/bundles/jqueryval\")");
            outFile.WriteLine("}");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForEdit2(string path, string tableName, string className, DatabaseInfo db)
        {
            var unusedFields = db.RemoveFields.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            Folder.Create(string.Format(@"{0}\{1}", path, className.UpperedFirstChar()));
            var id = db.Schemas.Where(p => p.Table == tableName).FirstOrDefault().Column;
            var textPath = string.Format(@"{0}\{1}\{2}.cshtml", path, className.UpperedFirstChar(), CreationType.Edit);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("@using {0}.Helpers.Common", db.ApplicationName);
            outFile.WriteLine("@model {0}.ViewModels.{1}{2}ViewModel", db.ApplicationName, className, "Update");
            outFile.WriteLine();

            outFile.WriteLine("@{");
            outFile.WriteLine("    ViewData[\"Title\"] = \"Edit\";");
            outFile.WriteLine("}");
            outFile.WriteLine();

            outFile.WriteLine("<h1>Edit</h1>");
            outFile.WriteLine("<h4>{0}</h4>", className);
            outFile.WriteLine();
            outFile.WriteLine("<div class=\"clearfix\"></div>");
            outFile.WriteLine();

            outFile.WriteLine("<div class=\"row\">");
            outFile.WriteLine("    <div class=\"col-md-4\">");
            outFile.WriteLine("        <form asp-action=\"Edit\" enctype=\"multipart/form-data\" >");
            outFile.WriteLine("            <div asp-validation-summary=\"ModelOnly\" class=\"text-danger\"></div>");
            outFile.WriteLine("            <input type=\"hidden\" asp-for=\"{0}\" />", id);

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                if (s.Column.ToLower() == "id")
                    continue;

                if (unusedFields.Any(x => x == s.Column))
                    continue;

                //outFile.WriteLine("            <div class=\"form-group\">");
                //outFile.WriteLine("                <label asp-for=\"{0}\" class=\"control-label\"></label>", s.Column);
                //outFile.WriteLine("                <input asp-for=\"{0}\" class=\"form-control\" />", s.Column);
                //outFile.WriteLine("                <span asp-validation-for=\"{0}\" class=\"text-danger\"></span>", s.Column);
                //outFile.WriteLine("            </div>");
                WriteProperty(tableName, outFile, s, true);
            }

            outFile.WriteLine("            <div class=\"form-group\">");
            outFile.WriteLine("                <input type=\"submit\" value=\"Save\" class=\"btn btn-sm btn-primary\" />");
            //outFile.WriteLine("                @Html.ActionLink(\"Back to List\", \"Index\", \"{0}\", null, new {{ @class = \"btn btn-sm btn-secondary\" }})", className);
            outFile.WriteLine("                {0}", BackToListActionLink(className, "secondary"));
            outFile.WriteLine("            </div>");
            outFile.WriteLine("        </form>");
            outFile.WriteLine("    </div>");
            outFile.WriteLine("</div>");

            outFile.WriteLine();

            WriterForFooterScriptPartial(outFile);
            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForDelete(string path, string tableName, string className, DatabaseInfo db)
        {
            Folder.Create(string.Format(@"{0}\{1}", path, className.UpperedFirstChar()));

            var textPath = string.Format(@"{0}\{1}\{2}.cshtml", path, className.UpperedFirstChar(), CreationType.Delete);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("@model {0}.Models.{1}Model", db.NamespaceCs, className);
            outFile.WriteLine("");
            outFile.WriteLine("@{");
            outFile.WriteLine("    ViewBag.Title = \"Delete - {0}\";", className);
            outFile.WriteLine("}");
            outFile.WriteLine("");
            outFile.WriteLine("<h2>@ViewBag.Title</h2>");
            outFile.WriteLine("<h3>Are you sure you want to delete this?</h3>");
            outFile.WriteLine("");
            outFile.WriteLine("<div class=\"form-horizontal\">");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                outFile.WriteLine("    <div class=\"form-group\">");
                outFile.WriteLine("        @Html.LabelFor(model => model.{0}, new {{ @class = \"control-label col-md-2\" }})", s.Column);
                outFile.WriteLine("        <div class=\"col-md-10\">");
                outFile.WriteLine("            @Html.LabelFor(model => model.{0})", s.Column);
                outFile.WriteLine("        </div>");
                outFile.WriteLine("    </div>");
                outFile.WriteLine("");
            }

            outFile.WriteLine("    @using (Html.BeginForm()) {");
            outFile.WriteLine("        @Html.AntiForgeryToken()");
            outFile.WriteLine("        <div class=\"form-group\">");
            outFile.WriteLine("            <div class=\"col-md-offset-2 col-md-10\">");
            outFile.WriteLine("                @Html.ActionLink(\"Back to List\", \"Index\") | <input type=\"submit\" value=\"Delete\" class=\"btn btn-danger\" /> />");
            outFile.WriteLine("            </div>");
            outFile.WriteLine("        </div>");
            outFile.WriteLine("    }");
            outFile.WriteLine("</div>");
            outFile.WriteLine("");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForDelete2(string path, string tableName, string className, DatabaseInfo db)
        {
            var hasStatus = db.Schemas.Any(p => p.Table == className && p.Column == "Status");

            var id = db.Schemas.Where(p => p.Table == tableName).FirstOrDefault().Column;
            Folder.Create(string.Format(@"{0}\{1}", path, className.UpperedFirstChar()));

            var textPath = string.Format(@"{0}\{1}\{2}.cshtml", path, className.UpperedFirstChar(), CreationType.Delete);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("@model {0}.ViewModels.{1}{2}ViewModel", db.ApplicationName, className, "Read");
            outFile.WriteLine();

            outFile.WriteLine("@{");
            outFile.WriteLine("    ViewData[\"Title\"] = \"Delete\";");
            outFile.WriteLine("}");
            outFile.WriteLine();

            outFile.WriteLine("<h1>Delete</h1>");
            outFile.WriteLine("<h4>Are you sure you want to delete this?</h4>", className);
            outFile.WriteLine();
            outFile.WriteLine("<div class=\"clearfix\"></div>");
            outFile.WriteLine();

            outFile.WriteLine("<div>");
            outFile.WriteLine("    <dl class=\"row\">");
            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                outFile.WriteLine("        <dt class = \"col-sm-2\">");
                outFile.WriteLine("            @Html.DisplayNameFor(model => model.{0})", s.Column);
                outFile.WriteLine("        </dt>");
                WriteReadProperty(tableName, outFile, s);
            }
            outFile.WriteLine("    </dl>");

            outFile.WriteLine("    <form asp-action=\"Delete\">");
            outFile.WriteLine("        <input type=\"hidden\" asp-for=\"{0}\" />", id);
            outFile.WriteLine("        <input type=\"submit\" value=\"Delete\" class=\"btn btn-sm btn-danger\" />");
            //outFile.WriteLine("        @Html.ActionLink(\"Back to List\", \"Index\", \"{0}\", null, new {{ @class = \"btn btn-sm btn-secondary\" }})", className);
            outFile.WriteLine("        {0}", BackToListActionLink(className, "secondary"));
            outFile.WriteLine("    </form>");
            outFile.WriteLine("</div>");

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

        private static string BackToListActionLink(string className, string buttonType)
        {
            //var s = string.Format("@Html.ActionLink(\"Back to List\", \"Index\", \"{0}\", null, new {{ @class = \"btn btn-sm btn-{1}\" }})", className, buttonType);
            var s = string.Format("<a asp-action=\"Index\" asp-controller=\"{0}\" class=\"btn btn-sm btn-{1}\">Back to List</a>", className, buttonType);

            return s;
        }

        private static void WriteProperty(string tableName, StreamWriter outFile, Schema schema, bool isUpdate = false)
        {

            if (schema.Column == "Image")
            {
                outFile.WriteLine("            <div class=\"form-group\">");
                outFile.WriteLine("                <label asp-for=\"{0}\" class=\"control-label\"></label>", schema.Column);
                outFile.WriteLine("                <br />");
                outFile.WriteLine("                @if (!string.IsNullOrWhiteSpace(@Model.Image))");
                outFile.WriteLine("                {");
                outFile.WriteLine("                    <img src=\"/{0}/@Model.Id/Image/@Model.Image\" alt=\"\" style=\"width:150px;height:150px;\">", tableName);
                outFile.WriteLine("                }");
                outFile.WriteLine("            </div>");

                if (isUpdate)
                {
                    outFile.WriteLine("            <div class=\"form-group\">");
                    outFile.WriteLine("                <input asp-for=\"ImageUpdate\" class=\"form-control\" />");
                    outFile.WriteLine("            </div>");
                }
            }
            else if (schema.Column == "Attachment")
            {
                outFile.WriteLine("            <div class=\"form-group\">");
                outFile.WriteLine("                <label asp-for=\"{0}\" class=\"control-label\"></label>", schema.Column);
                outFile.WriteLine("                <br />");
                outFile.WriteLine("                @if (!string.IsNullOrWhiteSpace(@Model.Attachment))");
                outFile.WriteLine("                {");
                outFile.WriteLine("                    @Html.ActionLink(");
                outFile.WriteLine("                        @Model.Attachment,");
                outFile.WriteLine("                        \"Download\",");
                outFile.WriteLine("                        new { Id = @Model.Id, DownloadType = \"Attachment\", Attachment = @Model.Attachment })");
                outFile.WriteLine("                }");
                outFile.WriteLine("            </div>");

                if (isUpdate)
                {
                    outFile.WriteLine("            <div class=\"form-group\">");
                    outFile.WriteLine("                <input asp-for=\"AttachmentUpdate\" class=\"form-control\" />");
                    outFile.WriteLine("            </div>");
                }
            }
            else if (schema.Column == "Status")
            {
                outFile.WriteLine("            <div class=\"form-group\">");
                outFile.WriteLine("                <label asp-for=\"{0}\" class=\"control-label\"></label>", schema.Column);
                outFile.WriteLine("                <br />");
                outFile.WriteLine("                @Html.DropDownListFor(x => ");
                outFile.WriteLine("                    x.Status,");
                outFile.WriteLine("                    new SelectList(Enum.GetValues(typeof(StatusType))),");
                outFile.WriteLine("                    \"Select..\",");
                outFile.WriteLine("                    new { @class = \"form-control\" })");
                outFile.WriteLine("                <span asp-validation-for=\"Status\" class=\"text-danger\"></span>");
                outFile.WriteLine("            </div>");
            }
            else if (schema.Column == "Featured")
            {
                outFile.WriteLine("            <div class=\"form-group\">");
                outFile.WriteLine("                <label asp-for=\"{0}\" class=\"control-label\"></label>", schema.Column);
                outFile.WriteLine("                <br />");
                outFile.WriteLine("                <input class=\"form-check-input\" type=\"checkbox\" asp-for=\"Featured\" />");
                outFile.WriteLine("            </div>");
            }
            else
            {
                outFile.WriteLine("            <div class=\"form-group\">");
                outFile.WriteLine("                <label asp-for=\"{0}\" class=\"control-label\"></label>", schema.Column);
                outFile.WriteLine("                <input asp-for=\"{0}\" class=\"form-control\" />", schema.Column);
                outFile.WriteLine("                <span asp-validation-for=\"{0}\" class=\"text-danger\"></span>", schema.Column);
                outFile.WriteLine("            </div>");
            }
        }

        private static void WriteReadProperty(string tableName, StreamWriter outFile, Schema schema)
        {


            if (schema.Column == "Image")
            {
                outFile.WriteLine("        <dd class = \"col-sm-10\">");
                outFile.WriteLine("            @if (!string.IsNullOrWhiteSpace(@Model.Image))");
                outFile.WriteLine("            {");
                outFile.WriteLine("                 <img src=\"/{0}/@Model.Id/Image/@Model.Image\" alt=\"\" style=\"width:150px;height:150px;\">", tableName);
                outFile.WriteLine("            }");
                outFile.WriteLine("        </dd>");
            }
            else if (schema.Column == "Attachment")
            {
                outFile.WriteLine("        <dd class = \"col-sm-10\">");
                outFile.WriteLine("            @if (!string.IsNullOrWhiteSpace(@Model.Attachment))");
                outFile.WriteLine("            {");
                outFile.WriteLine("                 @Html.ActionLink(@Model.Attachment, \"Download\", new { Id = @Model.Id, DownloadType = \"Attachment\", Attachment = @Model.Attachment })");
                outFile.WriteLine("            }");
                outFile.WriteLine("        </dd>");
            }
            else if (schema.Column == "Featured")
            {
                outFile.WriteLine("        <dd class = \"col-sm-10\">");
                outFile.WriteLine("            <input class=\"form-check-input\" type=\"checkbox\" checked=\"@Model.Featured\" disabled />");
                outFile.WriteLine("        </dd>");
            }
            else
            {
                outFile.WriteLine("        <dd class = \"col-sm-10\">");
                outFile.WriteLine("            @Html.DisplayFor(model => model.{0})", schema.Column);
                outFile.WriteLine("        </dd>");
            }
        }
    }
}
