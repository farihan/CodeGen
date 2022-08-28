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
    public class DbFileToController
    {
        public static void Generate(DatabaseInfo db)
        {
            var path = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.RazorController);
            var path1 = string.Format(@"{0}\{1}\EF", db.OutputDirectory, DirectoryType.RazorController);
            var path2 = string.Format(@"{0}\{1}\NH", db.OutputDirectory, DirectoryType.RazorController);

            if (string.IsNullOrEmpty(path1) || string.IsNullOrEmpty(path2))
            {
                throw new Exception("Output Directory does not exist");
            }

            Folder.Create(path1);
            Folder.Create(path2);

            foreach (var tableName in db.Schemas
                .Select(x => x.Table)
                .Distinct())
            {
                var className = tableName.UpperedFirstChar();

                WriterForController(path1, tableName, className, db, "EF");
                WriterForController(path2, tableName, className, db, "NH");
            }

            WriterForErrorController(path, db);

            Console.WriteLine();
        }

        private static void WriterForController(string path, string tableName, string className, DatabaseInfo db, string repo)
        {
            var textPath = string.Format(@"{0}\{1}{2}.cs", path, className, CreationType.Controller);
            var defaultSortColumn = db.Schemas.Where(p => p.Table == tableName).Select(x => x.Column).FirstOrDefault();

            var list = db.Schemas.Where(p => p.Table == tableName).Select(x => string.Format("{0}", x.Column)).ToList();
            var items = new List<string>();
            foreach(var item in list)
            {
                items.Add(item);
                if (item == "Image")
                {
                    items.Add("ImageUpdate");
                }
                else if (item == "Attachment")
                {
                    items.Add("AttachmentUpdate");
                }
            }
            //if (list.Any(x => x == "Image"))
            //    list.Add("ImageUpdate");

            //if (list.Any(x => x == "Attachment"))
            //    list.Add("AttachmentUpdate");

            var includeColumns = string.Join(", ", items.ToArray());

            var outFile = File.CreateText(textPath);

            //outFile.WriteLine("using System;");
            //outFile.WriteLine("using System.Collections.Generic;");
            //outFile.WriteLine("using System.Data;");
            //outFile.WriteLine("using System.Data.Entity;");
            //outFile.WriteLine("using System.Linq;");
            //outFile.WriteLine("using System.Net;");
            //outFile.WriteLine("using System.Web;");
            //outFile.WriteLine("using System.Web.Mvc;");
            //outFile.WriteLine("using System.Configuration");
            outFile.WriteLine("using CFF.DataAccess;");
            outFile.WriteLine("using CFF.Models;");
            outFile.WriteLine("using CFF.Helpers;");
            outFile.WriteLine("using CFF.Helpers.Common;");
            outFile.WriteLine("using CFF.ViewModels;");
            outFile.WriteLine("using Microsoft.AspNetCore.Mvc;");
            outFile.WriteLine("using Microsoft.EntityFrameworkCore;");
            outFile.WriteLine();
            outFile.WriteLine("namespace {0}.Controllers", db.ApplicationName);
            outFile.WriteLine("{");
            outFile.WriteLine("    public class {0}Controller : Controller", className);
            outFile.WriteLine("    {");
            if (repo == "EF")
            {
                outFile.WriteLine("        private readonly {0}Context _context;", db.ApplicationName);
                if (db.EnableAuditTrail == "false")
                    outFile.WriteLine("        private readonly string _username;");
                outFile.WriteLine("        private IWebHostEnvironment _environment;");
                outFile.WriteLine("        private const string _folderType = \"{0}\";", tableName);
                //outFile.WriteLine("        //public IRepository<{0}> {0}Repository {{ get; set; }}", className);
                outFile.WriteLine();
                outFile.WriteLine("        public {0}Controller({1}Context context, IWebHostEnvironment environment)", className, db.ApplicationName);
                outFile.WriteLine("        {");
                outFile.WriteLine("            _context = context;");
                if (db.EnableAuditTrail == "false")
                    outFile.WriteLine("            _username = \"User1\";");
                outFile.WriteLine("            _environment = environment;");
                outFile.WriteLine("        }");
            }
            if (repo == "NH")
            {
                outFile.WriteLine("        private readonly IRepository<{0}> {0}Repository;", className);
                outFile.WriteLine();
                outFile.WriteLine("        public {0}Controller(ISession session)", className);
                outFile.WriteLine("        {");
                outFile.WriteLine("            {0}Repository = new Repository<{0}>(session);", className);
                outFile.WriteLine("        }");
                outFile.WriteLine();
            }
            WriterForIndex(tableName, className, db, outFile, defaultSortColumn, repo);
            WriterForDetails(tableName, className, outFile, db, repo);
            WriterForCreate(className, outFile, db, includeColumns, repo);
            WriterForEdit(className, outFile, db, includeColumns);
            WriterForDelete(className, db, outFile);
            WriterForMapper(className, outFile, db);
            //outFile.WriteLine("        protected override void Dispose(bool disposing)");
            //outFile.WriteLine("        {");
            //outFile.WriteLine("            if (disposing)");
            //outFile.WriteLine("            {");
            //outFile.WriteLine("                db.Dispose();");
            //outFile.WriteLine("                //{0}Repository.Dispose();", className);
            //outFile.WriteLine("            }");
            //outFile.WriteLine();
            //outFile.WriteLine("            base.Dispose(disposing);");
            //outFile.WriteLine("        }");
            outFile.WriteLine("    }");
            outFile.WriteLine("}");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForDelete(string className, DatabaseInfo db, StreamWriter outFile)
        {
            var hasStatus = db.Schemas.Any(p => p.Table == className && p.Column == "Status");
            var hasDeletedAt = db.Schemas.Any(p => p.Table == className && p.Column == "DeletedAt");
            var hasDeletedBy = db.Schemas.Any(p => p.Table == className && p.Column == "DeletedBy");

            outFile.WriteLine("        // GET: /{0}/Delete/5", className);
            outFile.WriteLine("        public async Task<ActionResult> Delete(int? id)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            if (id == null)");
            outFile.WriteLine("            {");
            outFile.WriteLine("                return new BadRequestResult();");
            outFile.WriteLine("            }");
            outFile.WriteLine();
            outFile.WriteLine("            var {0} = await _context.{1}.FindAsync(id);", className.LoweredFirstChar(), className);
            outFile.WriteLine();
            outFile.WriteLine("            if ({0} == null)", className.LoweredFirstChar());
            outFile.WriteLine("            {");
            outFile.WriteLine("                return new NotFoundResult();");
            outFile.WriteLine("            }");
            outFile.WriteLine();
            outFile.WriteLine("            var model = To{0}ReadViewModel({1});", className, className.LoweredFirstChar());
            outFile.WriteLine();
            outFile.WriteLine("            return View(model);", className.LoweredFirstChar());
            outFile.WriteLine("        }");
            outFile.WriteLine();
            outFile.WriteLine("        // POST: /{0}/Delete/5", className);
            outFile.WriteLine("        [HttpPost, ActionName(\"Delete\")]");
            outFile.WriteLine("        [ValidateAntiForgeryToken]");
            outFile.WriteLine("        public async Task<ActionResult> DeleteConfirmed(int? id)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            if (id == null)");
            outFile.WriteLine("            {");
            outFile.WriteLine("                return new BadRequestResult();");
            outFile.WriteLine("            }");
            outFile.WriteLine();
            outFile.WriteLine("            var {0} = await _context.{1}.FirstOrDefaultAsync(x => x.Id == id);", className.LoweredFirstChar(), className);
            outFile.WriteLine();
            outFile.WriteLine("            if ({0} == null)", className.LoweredFirstChar());
            outFile.WriteLine("            {");
            outFile.WriteLine("                return new NotFoundResult();");
            outFile.WriteLine("            }");
            outFile.WriteLine();

            if (hasDeletedAt)
            {
                outFile.WriteLine("            {0}.UpdatedAt = DateTime.Now;", className.LoweredFirstChar());
                outFile.WriteLine("            {0}.UpdatedBy = _username;", className.LoweredFirstChar());
                outFile.WriteLine("            {0}.DeletedAt = DateTime.Now;", className.LoweredFirstChar());
                outFile.WriteLine("            {0}.DeletedBy = _username;", className.LoweredFirstChar());
                outFile.WriteLine();
                outFile.WriteLine("            _context.Entry({0}).State = EntityState.Modified;", className.LoweredFirstChar());
            }
            else
            {
                outFile.WriteLine("            _context.{0}.Remove({1});", className, className.LoweredFirstChar());
            }

            outFile.WriteLine("            await _context.SaveChangesAsync();");
            outFile.WriteLine();
            outFile.WriteLine("            return RedirectToAction(\"Index\");");
            outFile.WriteLine("        }");
            outFile.WriteLine();
        }

        private static void WriterForEdit(string className, StreamWriter outFile, DatabaseInfo db, string includeColumns)
        {
            var unusedFields = db.RemoveFields.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            outFile.WriteLine("        // GET: /{0}/Edit/5", className);
            outFile.WriteLine("        public async Task<ActionResult> Edit(int? id)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            if (id == null)");
            outFile.WriteLine("            {");
            outFile.WriteLine("                return new BadRequestResult();");
            outFile.WriteLine("            }");
            outFile.WriteLine();
            outFile.WriteLine("            var {0} = await _context.{1}.FindAsync(id);", className.LoweredFirstChar(), className);
            outFile.WriteLine();
            outFile.WriteLine("            if ({0} == null)", className.LoweredFirstChar());
            outFile.WriteLine("            {");
            outFile.WriteLine("                return new NotFoundResult();");
            outFile.WriteLine("            }");
            outFile.WriteLine();
            outFile.WriteLine("            var model = new {0}UpdateViewModel()", className);
            outFile.WriteLine("            {");

            var lastSchema = db.Schemas.Where(p => p.Table == className).Last();
            foreach (var s in db.Schemas.Where(p => p.Table == className))
            {
                var columnName = s.Column;

                if (s.Equals(lastSchema))
                {
                    if (columnName == "Status")
                    {
                        outFile.WriteLine("                Status = (StatusType){0}.Status", className.LoweredFirstChar());
                    }
                    else if (columnName == "Featured")
                    {
                        //outFile.WriteLine("                Featured = {0}.Featured ? 1 : 0", className.LoweredFirstChar());
                        outFile.WriteLine("                Featured = {0}.Featured == 1 ? true : false", className.LoweredFirstChar());
                    }
                    else
                    {
                        outFile.WriteLine("                {0} = {1}.{0}", columnName, className.LoweredFirstChar());
                    }
                }
                else
                {
                    if (columnName == "Status")
                    {
                        outFile.WriteLine("                Status = (StatusType){0}.Status,", className.LoweredFirstChar());
                    }
                    else if (columnName == "Featured")
                    {
                        //outFile.WriteLine("                Featured = {0}.Featured ? 1 : 0,", className.LoweredFirstChar());
                        outFile.WriteLine("                Featured = {0}.Featured == 1 ? true : false,", className.LoweredFirstChar());
                    }
                    else
                    {
                        outFile.WriteLine("                {0} = {1}.{0},", columnName, className.LoweredFirstChar());
                    }
                }
            }

            outFile.WriteLine("            };");
            outFile.WriteLine();

            outFile.WriteLine("            return View(model);", className.LoweredFirstChar());
            outFile.WriteLine("        }");
            outFile.WriteLine();

            outFile.WriteLine("        // POST: /{0}/Edit", className);
            outFile.WriteLine("        // To protect from overposting attacks, please enable the specific properties you want to bind to, for ");
            outFile.WriteLine("        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.");
            outFile.WriteLine("        [HttpPost]");
            outFile.WriteLine("        [ValidateAntiForgeryToken]");
            outFile.WriteLine("        public async Task<ActionResult> Edit([Bind(\"{0}\")]{1}UpdateViewModel model)", includeColumns, className);
            outFile.WriteLine("        {");
            outFile.WriteLine("            try");
            outFile.WriteLine("            {");
            outFile.WriteLine("                if (ModelState.IsValid)");
            outFile.WriteLine("                {");
            outFile.WriteLine("                    var {0} = await _context.{1}.FindAsync(model.Id);", className.LoweredFirstChar(), className);
            outFile.WriteLine();
            outFile.WriteLine("                    if ({0} == null)", className.LoweredFirstChar());
            outFile.WriteLine("                    {");
            outFile.WriteLine("                        return new NotFoundResult();");
            outFile.WriteLine("                    }");
            outFile.WriteLine();

            foreach (var s in db.Schemas.Where(p => p.Table == className))
            {
                if (unusedFields.Any(x => x == s.Column))
                    continue;

                var columnName = s.Column;
                if (s.ColumnType != "PK")
                {
                    if (columnName == "Status")
                    {
                        outFile.WriteLine("                    {0}.Status = (int)model.Status;", className.LoweredFirstChar());
                    }
                    else if (columnName == "Image")
                    {
                        outFile.WriteLine("                    if (model.ImageUpdate is not null)", className.LoweredFirstChar());
                        outFile.WriteLine("                    {");
                        outFile.WriteLine("                        {0}.Image = model.ImageUpdate?.FileName;", className.LoweredFirstChar());
                        outFile.WriteLine("                    }");
                    }
                    else if (columnName == "Attachment")
                    {
                        outFile.WriteLine("                    if (model.AttachmentUpdate is not null)", className.LoweredFirstChar());
                        outFile.WriteLine("                    {");
                        outFile.WriteLine("                        {0}.Attachment = model.AttachmentUpdate?.FileName;", className.LoweredFirstChar());
                        outFile.WriteLine("                    }");
                    }
                    else if (columnName == "Featured")
                    {
                        outFile.WriteLine("                    {0}.Featured = model.Featured ? 1 : 0;", className.LoweredFirstChar());
                    }
                    else
                    {
                        if (s.FieldType == "Select")
                        {
                            outFile.WriteLine("                    {1}.{0} = (int)model.{0},", columnName, className.LoweredFirstChar());
                        }
                        else
                        {
                            outFile.WriteLine("                    {1}.{0} = model.{0};", columnName, className.LoweredFirstChar());
                        }
                    }
                }
            }
            outFile.WriteLine();
            outFile.WriteLine("                    _context.Entry({0}).State = EntityState.Modified;", className.LoweredFirstChar());
            outFile.WriteLine("                    await _context.SaveChangesAsync();");
            outFile.WriteLine();

            var hasImage = db.Schemas.Any(p => p.Table == className && p.Column == "Image");
            if (hasImage)
            {
                outFile.WriteLine("                    if (model.ImageUpdate is not null)");
                outFile.WriteLine("                    {");
                outFile.WriteLine("                        Uploader.UploadImage(_environment.WebRootPath, _folderType, {0}.Id, model.ImageUpdate);", className.LoweredFirstChar());
                outFile.WriteLine("                    }");
                outFile.WriteLine();
            }

            var hasAttachment = db.Schemas.Any(p => p.Table == className && p.Column == "Attachment");
            if (hasAttachment)
            {
                outFile.WriteLine("                    if (model.AttachmentUpdate is not null)");
                outFile.WriteLine("                    {");
                outFile.WriteLine("                        Uploader.UploadAttachment(_environment.WebRootPath, _folderType, {0}.Id, model.AttachmentUpdate);", className.LoweredFirstChar());
                outFile.WriteLine("                    }");
                outFile.WriteLine();
            }
            outFile.WriteLine("                    return RedirectToAction(\"Index\");");
            outFile.WriteLine("                }");
            outFile.WriteLine("            }");
            //outFile.WriteLine("            catch (RetryLimitExceededException /* ex */)");
            outFile.WriteLine("            catch (Exception ex)");
            outFile.WriteLine("            {");
            outFile.WriteLine("                //Log the error (uncomment ex variable name and add a line here to write a log.");
            outFile.WriteLine("                ModelState.AddModelError(ex.Message, \"Unable to save changes. Try again, and if the problem persists see your system administrator.\");");
            outFile.WriteLine("            }");
            outFile.WriteLine();
            outFile.WriteLine("            return View(model);", className.LoweredFirstChar());
            outFile.WriteLine("        }");
            outFile.WriteLine();
        }

        private static void WriterForCreate(string className, StreamWriter outFile, DatabaseInfo db, string includeColumns, string repo)
        {
            var unusedFields = db.RemoveFields.Split(new[] { "," } , StringSplitOptions.RemoveEmptyEntries);

            outFile.WriteLine("        // GET: /{0}/Create", className);
            outFile.WriteLine("        public ActionResult Create()");
            outFile.WriteLine("        {");
            outFile.WriteLine("            var model = new {0}CreateViewModel();", className);
            outFile.WriteLine();
            outFile.WriteLine("            return View(model);");
            outFile.WriteLine("        }");
            outFile.WriteLine();

            outFile.WriteLine("        // POST: /{0}/Create", className);
            outFile.WriteLine("        // To protect from overposting attacks, please enable the specific properties you want to bind to, for ");
            outFile.WriteLine("        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.");
            outFile.WriteLine("        [HttpPost]");
            outFile.WriteLine("        [ValidateAntiForgeryToken]");
            outFile.WriteLine("        public async Task<ActionResult> Create([Bind(\"{0}\")]{1}CreateViewModel model)", includeColumns, className);
            outFile.WriteLine("        {");
            outFile.WriteLine("            try");
            outFile.WriteLine("            {");
            outFile.WriteLine("                if (ModelState.IsValid)");
            outFile.WriteLine("                {");
            outFile.WriteLine("                    var {0} = new {1}()", className.LoweredFirstChar(), className);
            outFile.WriteLine("                    {");

            var lastSchema = db.Schemas.Where(p => p.Table == className).Last();
            foreach (var s in db.Schemas.Where(p => p.Table == className))
            {
                if (unusedFields.Any(x => x == s.Column))
                    continue;

                var columnName = s.Column;
                if (s.ColumnType != "PK")
                {
                    if (s.Equals(lastSchema))
                    {
                        if (columnName == "Status")
                        {
                            outFile.WriteLine("                        Status = (int)model.Status");
                        }
                        else if (columnName == "Image")
                        {
                            outFile.WriteLine("                        Image = model.Image?.FileName");
                        }
                        else if (columnName == "Attachment")
                        {
                            outFile.WriteLine("                        Attachment = model.Attachment?.FileName");
                        }
                        else if (columnName == "Featured")
                        {
                            outFile.WriteLine("                        Featured = model.Featured ? 1 : 0");
                        }
                        else
                        {
                            outFile.WriteLine("                        {0} = model.{0}", columnName, className.LoweredFirstChar());
                        }
                    }
                    else
                    {
                        if (columnName == "Status")
                        {
                            outFile.WriteLine("                        Status = (int)model.Status,");
                        }
                        else if (columnName == "Image")
                        {
                            outFile.WriteLine("                        Image = model.Image?.FileName,");
                        }
                        else if (columnName == "Attachment")
                        {
                            outFile.WriteLine("                        Attachment = model.Attachment?.FileName,");
                        }
                        else if (columnName == "Featured")
                        {
                            outFile.WriteLine("                        Featured = model.Featured ? 1 : 0,");
                        }
                        else
                        {
                            if (s.FieldType == "Select")
                            {
                                outFile.WriteLine("                        {0} = (int)model.{0},", columnName, className.LoweredFirstChar());
                            }
                            else
                            {
                                outFile.WriteLine("                        {0} = model.{0},", columnName, className.LoweredFirstChar());
                            }
                        }
                    }
                }
            }
            outFile.WriteLine("                    };");
            outFile.WriteLine();

            if (repo == "EF")
            {
                outFile.WriteLine("                    _context.{0}.Add({1});", className, className.LoweredFirstChar());
                outFile.WriteLine("                    await _context.SaveChangesAsync();");
                outFile.WriteLine();

                var hasImage = db.Schemas.Any(p => p.Table == className && p.Column == "Image");
                if (hasImage)
                {
                    outFile.WriteLine("                    if (model.Image is not null)", className);
                    outFile.WriteLine("                    {");
                    outFile.WriteLine("                        Uploader.UploadImage(_environment.WebRootPath, _folderType, {0}.Id, model.Image);", className.LoweredFirstChar());
                    outFile.WriteLine("                    }");
                    outFile.WriteLine();
                }

                var hasAttachment = db.Schemas.Any(p => p.Table == className && p.Column == "Attachment");
                if (hasAttachment)
                {
                    outFile.WriteLine("                    if (model.Attachment is not null)");
                    outFile.WriteLine("                    {");
                    outFile.WriteLine("                        Uploader.UploadAttachment(_environment.WebRootPath, _folderType, {0}.Id, model.Attachment);", className.LoweredFirstChar());
                    outFile.WriteLine("                    }");
                    outFile.WriteLine();
                }

                outFile.WriteLine("                    return RedirectToAction(\"Index\");");
            }

            if (repo == "NH")
            {
                outFile.WriteLine("                    {0}Repository.Save({1});", className, className.LoweredFirstChar());
                outFile.WriteLine("                    return RedirectToAction(\"Index\");");
            }

            outFile.WriteLine("                }");
            outFile.WriteLine("            }");
            //outFile.WriteLine("            catch (RetryLimitExceededException /* ex */)");
            outFile.WriteLine("            catch (Exception ex)");
            outFile.WriteLine("            {");
            outFile.WriteLine("                //Log the error (uncomment ex variable name and add a line here to write a log.");
            outFile.WriteLine("                ModelState.AddModelError(ex.Message, \"Unable to save changes. Try again, and if the problem persists see your system administrator.\");");
            outFile.WriteLine("            }");
            outFile.WriteLine();
            outFile.WriteLine("            return View(model);", className.LoweredFirstChar());
            outFile.WriteLine("        }");
            outFile.WriteLine();
        }

        private static void WriterForDetails(string tableName, string className, StreamWriter outFile, DatabaseInfo db, string repo)
        {
            //var id = db.Schemas.Where(p => p.Table == tableName && p.ConstraintType == "PK").FirstOrDefault().Column;
            var id = db.Schemas.Where(p => p.Table == tableName).FirstOrDefault().Column;
            var lastSchema = db.Schemas.Where(p => p.Table == tableName).Last();

            outFile.WriteLine("        // GET: /{0}/Details/5", className);
            outFile.WriteLine("        public async Task<ActionResult> Details(int? id)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            if (id == null)");
            outFile.WriteLine("            {");
            outFile.WriteLine("                return new BadRequestResult();");
            outFile.WriteLine("            }");
            outFile.WriteLine();
            if (repo == "EF")
            {
                outFile.WriteLine("            var {0} = await _context.{1}.FindAsync(id);", className.LoweredFirstChar(), className);
            }
            if (repo == "NH")
            {
                outFile.WriteLine("            var {0} = {1}Repository.FindOneBy(p => p.{2} == id);", className.LoweredFirstChar(), className, id);
            }
            outFile.WriteLine();
            outFile.WriteLine("            if ({0} == null)", className.LoweredFirstChar());
            outFile.WriteLine("            {");
            outFile.WriteLine("                return new NotFoundResult();");
            outFile.WriteLine("            }");
            outFile.WriteLine();
            outFile.WriteLine("            var model = To{0}ReadViewModel({1});", className, className.LoweredFirstChar());
            outFile.WriteLine();
            outFile.WriteLine("            return View(model);");
            outFile.WriteLine("        }");
            outFile.WriteLine();
        }

        private static void WriterForIndex(string tableName, string className, DatabaseInfo db, StreamWriter outFile, string defaultSortColumn, string repo)
        {
            var pluralization = new Pluralization();
            var firstSchema = db.Schemas.Where(p => p.Table == tableName).First();
            var lastSchema = db.Schemas.Where(p => p.Table == tableName).Last();
            var hasStatus = db.Schemas.Any(p => p.Table == tableName && p.Column == "Status");
            var hasDeletedBy = db.Schemas.Any(p => p.Table == tableName && p.Column == "DeletedBy");

            outFile.WriteLine("        // GET: /{0}/", className);
            outFile.WriteLine("        public async Task<ActionResult> Index(int? page, string sort = \"{0}\", bool asc = true, string query = \"\")", defaultSortColumn.ToLower());
            outFile.WriteLine("        {");
            if (repo == "EF")
            {
                if (hasDeletedBy)
                {
                    outFile.WriteLine("            var list = _context.{1}.Where(x => x.DeletedBy == null).OrderBy(x => x.{2});", className.ToLower(), className, defaultSortColumn);
                    //outFile.WriteLine("            var list = _context.{1}.OrderBy(x => x.{2}).Where(x => x.Status == 1);", className.ToLower(), className, defaultSortColumn);
                }
                else
                {
                    outFile.WriteLine("            var list = _context.{1}.OrderBy(x => x.{2});", className.ToLower(), className, defaultSortColumn);
                    //outFile.WriteLine("            //var list = _context.{1}.OrderBy(x => x.{2}).Where(x => x.Status == 1);", className.ToLower(), className, defaultSortColumn);
                }
            }
            if (repo == "NH")
            {
                outFile.WriteLine("            var list = {0}Repository.FindAll();", className.LoweredFirstChar());
                outFile.WriteLine();
                outFile.WriteLine("            if (!string.IsNullOrEmpty(query))");
                outFile.WriteLine("            {");
                outFile.WriteLine("                ViewBag.CurrentQuery = query;", className.LoweredFirstChar());
                outFile.WriteLine();
                outFile.WriteLine("                list = list.Where(p => ");

                foreach (var s in db.Schemas.Where(p => p.Table == tableName))
                {
                    var columnName = s.Column;

                    if (s.Equals(firstSchema))
                        outFile.WriteLine("                    p.{0}.ToUpper().Contains(query.ToUpper())", columnName);
                    else if (s.Equals(lastSchema))
                        outFile.WriteLine("                    || p.{0}.ToUpper().Contains(query.ToUpper());", columnName);
                    else
                        outFile.WriteLine("                    || p.{0}.ToUpper().Contains(query.ToUpper())", columnName);
                }

                outFile.WriteLine("            }");
            }
            outFile.WriteLine();
            outFile.WriteLine("            switch (sort.ToLower())");
            outFile.WriteLine("            {");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                var columnName = s.Column;

                outFile.WriteLine("                case \"{0}\":", columnName.ToLower());
                outFile.WriteLine("                    list = asc ? list.OrderBy(p => p.{1}) : list.OrderByDescending(p => p.{1});", className.ToLower(), columnName);
                outFile.WriteLine("                    break;");
            }

            outFile.WriteLine("            }");
            outFile.WriteLine();
            outFile.WriteLine("            var model = new {0}ViewModel();", className);
            outFile.WriteLine("            model.PageSize = 10; //int.Parse(ConfigurationManager.AppSettings[\"PageSize\"]);");
            if (repo == "EF")
            {
                //outFile.WriteLine("            model.TotalPages = (int)Math.Ceiling((double)_context.{0}.Count() / model.PageSize);", className);
                outFile.WriteLine("            model.TotalPages = (int)Math.Ceiling((double)list.Count() / model.PageSize);", className);
            }

            if (repo == "NH")
            {
                outFile.WriteLine("            model.TotalPages = (int)Math.Ceiling((double)list.Count() / model.PageSize);", className);
            }

            outFile.WriteLine("            model.CurrentPage = page ?? 1;");
            outFile.WriteLine();
            outFile.WriteLine("            var models = await list");
            outFile.WriteLine("                .Skip((model.CurrentPage - 1) * model.PageSize)");
            outFile.WriteLine("                .Take(model.PageSize)  ");
            outFile.WriteLine("                .ToListAsync();");
            outFile.WriteLine();

            var pluralWord = pluralization.Pluralize(className);
            outFile.WriteLine("            model.{0} = To{1}ReadViewModels(models);", pluralWord, className);
            outFile.WriteLine("            model.PageIndex = model.PageSize * (model.CurrentPage - 1);");
            outFile.WriteLine("            model.Sort = sort;");
            outFile.WriteLine("            model.IsAsc = asc;");
            outFile.WriteLine();
            outFile.WriteLine("            return View(model);");
            outFile.WriteLine("        }");
            outFile.WriteLine();
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
            
            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForMapper(string className, StreamWriter outFile, DatabaseInfo db)
        {
            var pluralization = new Pluralization();

            outFile.WriteLine("        private FileResult Download(int id, string downloadType, string attachment)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            var path = Path.Combine(_environment.WebRootPath, _folderType, string.Format($\"{id}\"), string.Format($\"{downloadType}\")) + \"\\\\\" + attachment;");
            outFile.WriteLine("            var bytes = System.IO.File.ReadAllBytes(path);");
            outFile.WriteLine();
            outFile.WriteLine("            return File(bytes, \"application/octet-stream\", attachment);");
            outFile.WriteLine("        }");
            outFile.WriteLine();

            outFile.WriteLine("        private static {0}ReadViewModel To{0}ReadViewModel({0} {1})", className, className.LoweredFirstChar());
            outFile.WriteLine("        {");
            outFile.WriteLine("            var model = new {0}ReadViewModel()", className);
            outFile.WriteLine("            {");

            var lastSchema = db.Schemas.Where(p => p.Table == className).Last();
            foreach (var s in db.Schemas.Where(p => p.Table == className))
            {
                //outFile.WriteLine("                {0} = {1}.{0},", s.Column, className.LoweredFirstChar());

                var columnName = s.Column;

                if (s.Equals(lastSchema))
                {
                    if (columnName == "Status")
                    {
                        outFile.WriteLine("                Status = (StatusType){0}.Status", className.LoweredFirstChar());
                    }
                    else if (columnName == "Image")
                    {
                        outFile.WriteLine("                Image = {0}.Image", className.LoweredFirstChar());
                    }
                    else if (columnName == "Attachment")
                    {
                        outFile.WriteLine("                Attachment = {0}.Attachment", className.LoweredFirstChar());
                    }
                    else if (columnName == "Featured")
                    {
                        outFile.WriteLine("                Featured = {0}.Featured == 1 ? true : false", className.LoweredFirstChar());
                    }
                    else
                    {
                        if (s.FieldType == "Select")
                        {
                            outFile.WriteLine("                {0} = ({2}){1}.{0}", columnName, className.LoweredFirstChar(), s.FieldTypeName);
                        }
                        else
                        {
                            outFile.WriteLine("                {0} = {1}.{0}", columnName, className.LoweredFirstChar());
                        }
                    }
                }
                else
                {
                    if (columnName == "Status")
                    {
                        outFile.WriteLine("                Status = (StatusType){0}.Status,", className.LoweredFirstChar());
                    }
                    else if (columnName == "Image")
                    {
                        outFile.WriteLine("                Image = {0}.Image,", className.LoweredFirstChar());
                    }
                    else if (columnName == "Attachment")
                    {
                        outFile.WriteLine("                Attachment = {0}.Attachment,", className.LoweredFirstChar());
                    }
                    else if (columnName == "Featured")
                    {
                        outFile.WriteLine("                Featured = {0}.Featured == 1 ? true : false,", className.LoweredFirstChar());
                    }
                    else
                    {
                        if (s.FieldType == "Select")
                        {
                            outFile.WriteLine("                {0} = ({2}){1}.{0},", columnName, className.LoweredFirstChar(), s.FieldTypeName);
                        }
                        else
                        {
                            outFile.WriteLine("                {0} = {1}.{0},", columnName, className.LoweredFirstChar());
                        }
                    }
                }
            }
            outFile.WriteLine("            };");
            outFile.WriteLine();
            outFile.WriteLine("            return model;");
            outFile.WriteLine("        }");
            outFile.WriteLine();

            var pluralWord = pluralization.Pluralize(className.LoweredFirstChar());
            outFile.WriteLine("        private static List<{0}ReadViewModel> To{0}ReadViewModels(List<{0}> {1})", className, pluralWord);
            outFile.WriteLine("        {");
            outFile.WriteLine("            var list = new List<{0}ReadViewModel>();", className);
            outFile.WriteLine();
            outFile.WriteLine("            foreach (var item in {0})", pluralWord);
            outFile.WriteLine("            {");
            outFile.WriteLine("                list.Add(To{0}ReadViewModel(item));", className);
            outFile.WriteLine("            }");
            outFile.WriteLine();
            outFile.WriteLine("            return list;");
            outFile.WriteLine("        }");
        }
    }
}
