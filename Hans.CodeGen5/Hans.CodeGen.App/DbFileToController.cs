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
            var includeColumns = string.Join(", ", db.Schemas.Where(p => p.Table == tableName).Select(x => x.Column).ToArray());

            var outFile = File.CreateText(textPath);

            outFile.WriteLine("using System;");
            outFile.WriteLine("using System.Collections.Generic;");
            outFile.WriteLine("using System.Data;");
            outFile.WriteLine("using System.Data.Entity;");
            outFile.WriteLine("using System.Linq;");
            outFile.WriteLine("using System.Net;");
            outFile.WriteLine("using System.Web;");
            outFile.WriteLine("using System.Web.Mvc;");
            outFile.WriteLine("using System.Configuration");
            outFile.WriteLine();
            outFile.WriteLine("namespace {0}.Controllers", db.NamespaceCs);
            outFile.WriteLine("{");
            outFile.WriteLine("    public class {0}Controller : Controller", className);
            outFile.WriteLine("    {");
            if (repo == "EF")
            {
                outFile.WriteLine("        private {0}Context db = new {0}Context();", db.ApplicationName);
                outFile.WriteLine("        //public IRepository<{0}> {0}Repository {{ get; set; }}", className);
                outFile.WriteLine();
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
            WriterForCreate(className, outFile, includeColumns);
            WriterForEdit(className, outFile, includeColumns);
            WriterForDelete(className, outFile);

            outFile.WriteLine("        protected override void Dispose(bool disposing)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            if (disposing)");
            outFile.WriteLine("            {");
            outFile.WriteLine("                db.Dispose();");
            outFile.WriteLine("                //{0}Repository.Dispose();", className);
            outFile.WriteLine("            }");
            outFile.WriteLine();
            outFile.WriteLine("            base.Dispose(disposing);");
            outFile.WriteLine("        }");
            outFile.WriteLine("    }");
            outFile.WriteLine("}");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForDelete(string className, StreamWriter outFile)
        {
            outFile.WriteLine("        // GET: /{0}/Delete/5", className);
            outFile.WriteLine("        public async Task<ActionResult> Delete(int? id, bool? saveChangesError = false)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            if (id == null)");
            outFile.WriteLine("            {");
            outFile.WriteLine("                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);");
            outFile.WriteLine("            }");
            outFile.WriteLine();
            outFile.WriteLine("            if (saveChangesError.GetValueOrDefault())");
            outFile.WriteLine("            {");
            outFile.WriteLine("                ViewBag.ErrorMessage = \"Delete failed. Try again, and if the problem persists see your system administrator.\";");
            outFile.WriteLine("            }");
            outFile.WriteLine();
            outFile.WriteLine("            var {0} = db.{1}s.FindAsync(id);", className.ToLower(), className);
            outFile.WriteLine();
            outFile.WriteLine("            if ({0} == null)", className.ToLower());
            outFile.WriteLine("            {");
            outFile.WriteLine("                return HttpNotFound();");
            outFile.WriteLine("            }");
            outFile.WriteLine();
            outFile.WriteLine("            return View({0});", className.ToLower());
            outFile.WriteLine("        }");
            outFile.WriteLine();

            outFile.WriteLine("        // POST: /{0}/Delete/5", className);
            outFile.WriteLine("        [HttpPost]");
            outFile.WriteLine("        [ValidateAntiForgeryToken]");
            outFile.WriteLine("        public async Task<ActionResult> Delete(int id)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            try");
            outFile.WriteLine("            {");
            outFile.WriteLine("                db.Entry({0}).State = EntityState.Modified;", className.ToLower());
            outFile.WriteLine("                await db.SaveChangesAsync()");
            outFile.WriteLine("                return RedirectToAction(\"Index\");");
            outFile.WriteLine("            }");
            outFile.WriteLine("            catch (RetryLimitExceededException /* ex */)");
            outFile.WriteLine("            {");
            outFile.WriteLine("                //Log the error (uncomment ex variable name and add a line here to write a log.");
            outFile.WriteLine("                ModelState.AddModelError(\"\", \"Unable to save changes. Try again, and if the problem persists see your system administrator.\");");
            outFile.WriteLine("            }");
            outFile.WriteLine();
            outFile.WriteLine("            return View({0});", className.ToLower());
            outFile.WriteLine("        }");
            outFile.WriteLine();
        }

        private static void WriterForEdit(string className, StreamWriter outFile, string includeColumns)
        {
            outFile.WriteLine("        // GET: /{0}/Edit/5", className);
            outFile.WriteLine("        public async Task<ActionResult> Edit(int? id)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            if (id == null)");
            outFile.WriteLine("            {");
            outFile.WriteLine("                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);");
            outFile.WriteLine("            }");
            outFile.WriteLine();
            outFile.WriteLine("            var {0} = db.{1}s.FindAsync(id);", className.ToLower(), className);
            outFile.WriteLine();
            outFile.WriteLine("            if ({0} == null)", className.ToLower());
            outFile.WriteLine("            {");
            outFile.WriteLine("                return HttpNotFound();");
            outFile.WriteLine("            }");
            outFile.WriteLine();
            outFile.WriteLine("            return View({0});", className.ToLower());
            outFile.WriteLine("        }");
            outFile.WriteLine();

            outFile.WriteLine("        // POST: /{0}/Edit", className);
            outFile.WriteLine("        // To protect from overposting attacks, please enable the specific properties you want to bind to, for ");
            outFile.WriteLine("        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.");
            outFile.WriteLine("        [HttpPost]");
            outFile.WriteLine("        [ValidateAntiForgeryToken]");
            outFile.WriteLine("        public async Task<ActionResult> Edit([Bind(Include = \"{0}\")]{1} {2}", includeColumns, className, className.ToLower());
            outFile.WriteLine("        {");
            outFile.WriteLine("            try");
            outFile.WriteLine("            {");
            outFile.WriteLine("                if (ModelState.IsValid)");
            outFile.WriteLine("                {");
            outFile.WriteLine("                    db.Entry({0}).State = EntityState.Modified;", className.ToLower());
            outFile.WriteLine("                    await db.SaveChangesAsync()");
            outFile.WriteLine("                    return RedirectToAction(\"Index\");");
            outFile.WriteLine("                }");
            outFile.WriteLine("            }");
            outFile.WriteLine("            catch (RetryLimitExceededException /* ex */)");
            outFile.WriteLine("            {");
            outFile.WriteLine("                //Log the error (uncomment ex variable name and add a line here to write a log.");
            outFile.WriteLine("                ModelState.AddModelError(\"\", \"Unable to save changes. Try again, and if the problem persists see your system administrator.\");");
            outFile.WriteLine("            }");
            outFile.WriteLine();
            outFile.WriteLine("            return View({0});", className.ToLower());
            outFile.WriteLine("        }");
            outFile.WriteLine();
        }

        private static void WriterForCreate(string className, StreamWriter outFile, string includeColumns)
        {
            outFile.WriteLine("        // GET: /{0}/Create", className);
            outFile.WriteLine("        public ActionResult Create()");
            outFile.WriteLine("        {");
            outFile.WriteLine("            return View();");
            outFile.WriteLine("        }");
            outFile.WriteLine();

            outFile.WriteLine("        // POST: /{0}/Create", className);
            outFile.WriteLine("        // To protect from overposting attacks, please enable the specific properties you want to bind to, for ");
            outFile.WriteLine("        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.");
            outFile.WriteLine("        [HttpPost]");
            outFile.WriteLine("        [ValidateAntiForgeryToken]");
            outFile.WriteLine("        public async Task<ActionResult> Create([Bind(Include = \"{0}\")]{1} {2}", includeColumns, className, className.ToLower());
            outFile.WriteLine("        {");
            outFile.WriteLine("            try");
            outFile.WriteLine("            {");
            outFile.WriteLine("                if (ModelState.IsValid)");
            outFile.WriteLine("                {");
            outFile.WriteLine("                    db.{0}s.Add({1});", className, className.ToLower());
            outFile.WriteLine("                    await db.SaveChangesAsync()");
            outFile.WriteLine("                    return RedirectToAction(\"Index\");");
            outFile.WriteLine("                }");
            outFile.WriteLine("            }");
            outFile.WriteLine("            catch (RetryLimitExceededException /* ex */)");
            outFile.WriteLine("            {");
            outFile.WriteLine("                //Log the error (uncomment ex variable name and add a line here to write a log.");
            outFile.WriteLine("                ModelState.AddModelError(\"\", \"Unable to save changes. Try again, and if the problem persists see your system administrator.\");");
            outFile.WriteLine("            }");
            outFile.WriteLine();
            outFile.WriteLine("            return View({0});", className.ToLower());
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
            outFile.WriteLine("                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);");
            outFile.WriteLine("            }");
            outFile.WriteLine();
            if (repo == "EF")
            {
                outFile.WriteLine("            var {0} = db.{1}s.FindAsync(id);", className.LoweredFirstChar(), className);
            }
            if (repo == "NH")
            {
                outFile.WriteLine("            var {0} = {1}Repository.FindOneBy(p => p.{2} == id);", className.LoweredFirstChar(), className, id);
            }
            outFile.WriteLine();
            outFile.WriteLine("            if ({0} == null)", className.LoweredFirstChar());
            outFile.WriteLine("            {");
            outFile.WriteLine("                return HttpNotFound();");
            outFile.WriteLine("            }");
            outFile.WriteLine();
            outFile.WriteLine("            var model = new {0}()", className);
            outFile.WriteLine("            {");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                var columnName = s.Column;
                if (s.ColumnType != "PK")
                {
                    if (s.Equals(lastSchema))
                    {
                        outFile.WriteLine("                {0} = model.{0}", columnName);
                    }
                    else
                    {
                        outFile.WriteLine("                {0} = model.{0},", columnName);
                    }
                }
            }

            outFile.WriteLine("            };");
            outFile.WriteLine();
            outFile.WriteLine("            return View(model);");
            outFile.WriteLine("        }");
            outFile.WriteLine();
        }

        private static void WriterForIndex(string tableName, string className, DatabaseInfo db, StreamWriter outFile, string defaultSortColumn, string repo)
        {
            var firstSchema = db.Schemas.Where(p => p.Table == tableName).First();
            var lastSchema = db.Schemas.Where(p => p.Table == tableName).Last();

            outFile.WriteLine("        // GET: /{0}/", className);
            outFile.WriteLine("        public async Task<ActionResult> Index(int? page, string sort = \"{0}\", bool asc = true, string query = \"\")", defaultSortColumn.ToLower());
            outFile.WriteLine("        {");
            if (repo == "EF")
            {
                outFile.WriteLine("            var list = db.{1}.OrderBy(x => x.{2});", className.ToLower(), className, defaultSortColumn);
            }
            if (repo == "NH")
            {
                outFile.WriteLine("            var list = {0}Repository.FindAll();", className.ToLower());
                outFile.WriteLine();
                outFile.WriteLine("            if (!string.IsNullOrEmpty(query))");
                outFile.WriteLine("            {");
                outFile.WriteLine("                ViewBag.CurrentQuery = query;", className.ToLower());
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
            outFile.WriteLine("            var model = new {0}Model();", className);
            outFile.WriteLine("            model.PageSize = int.Parse(ConfigurationManager.AppSettings[\"PageSize\"]);");
            if (repo == "EF")
            {
                outFile.WriteLine("            model.TotalPages = (int)Math.Ceiling((double)db.{0}.Count() / model.PageSize);", className);
            }

            if (repo == "NH")
            {
                outFile.WriteLine("            model.TotalPages = (int)Math.Ceiling((double)list.Count() / model.PageSize);", className);
            }

            outFile.WriteLine("            model.CurrentPage = page ?? 1;");
            outFile.WriteLine();
            outFile.WriteLine("            model.{0}s = await list", className.ToLower(), className);
            outFile.WriteLine("                .Skip((model.CurrentPage - 1) * model.PageSize)");
            outFile.WriteLine("                .Take(model.PageSize)  ");
            outFile.WriteLine("                .ToListAsync();");
            outFile.WriteLine();
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
    }
}
