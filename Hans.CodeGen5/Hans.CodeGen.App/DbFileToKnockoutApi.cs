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
    public class DbFileToKnockoutApi
    {
        public static void Generate(DatabaseInfo db)
        {
            var path = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.KOController);

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

                WriterForApiController(path, tableName, className, db);
            }

            WriterForErrorController(path, db);

            Console.WriteLine();
        }

        private static void WriterForApiController(string path, string tableName, string className, DatabaseInfo db)
        {
            var textPath = string.Format(@"{0}\{1}{2}.cs", path, className, CreationType.Controller);
            var id = db.Schemas.Where(p => p.Table == tableName).FirstOrDefault().Column;
            var lastSchema = db.Schemas.Where(p => p.Table == tableName).Last();
            if (db.Schemas.Where(p => p.Table == tableName).FirstOrDefault(c => c.ColumnType == "PK") != null)
                id = db.Schemas.Where(p => p.Table == tableName).FirstOrDefault(c => c.ColumnType == "PK").Column;

            var outFile = File.CreateText(textPath);

            outFile.WriteLine("using System;");
            outFile.WriteLine("using System.Collections.Generic;");
            outFile.WriteLine("using System.Linq;");
            outFile.WriteLine("using System.Net;");
            outFile.WriteLine("using System.Net.Http;");
            outFile.WriteLine("using System.Web.Http;");
            outFile.WriteLine("using System.Threading.Tasks");
            outFile.WriteLine();
            outFile.WriteLine("namespace {0}.Controllers", db.NamespaceCs);
            outFile.WriteLine("{");
            outFile.WriteLine("    public class {0}Controller : ApiController", className);
            outFile.WriteLine("    {");
            outFile.WriteLine("        private IRepository<{0}> {0}Repository = new Repository<{0}>();", className);
            outFile.WriteLine();
            outFile.WriteLine("        // GET: api/{0}", className);
            outFile.WriteLine("        public int GetSize()", className);
            outFile.WriteLine("        {");
            outFile.WriteLine("            return {0}Repository.FindAll().Count();", className);
            outFile.WriteLine("        }");
            outFile.WriteLine();
            outFile.WriteLine("        // GET: api/{0}", className);
            outFile.WriteLine("        public IQueryable<{0}Model> GetAll()", className);
            outFile.WriteLine("        {");
            outFile.WriteLine("            return {0}Repository.FindAll().Select(x => new {0}Model", className);
            outFile.WriteLine("            {");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                var columnName = s.Column;
                if (s.Equals(lastSchema))
                {
                    outFile.WriteLine("                {0} = x.{0}", columnName);
                }
                else
                {
                    outFile.WriteLine("                {0} = x.{0},", columnName);
                }
            }

            outFile.WriteLine("            });");
            outFile.WriteLine("        }");
            outFile.WriteLine();
            outFile.WriteLine("        // GET: api/{0}", className);
            outFile.WriteLine("        public IQueryable<{0}Model> GetAllBy(int page, int pageSize)", className);
            outFile.WriteLine("        {");
            outFile.WriteLine("            return {0}Repository.FindAll().Select(x => new {0}Model", className);
            outFile.WriteLine("            {");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                var columnName = s.Column;
                if (s.Equals(lastSchema))
                {
                    outFile.WriteLine("                {0} = x.{0}", columnName);
                }
                else
                {
                    outFile.WriteLine("                {0} = x.{0},", columnName);
                }
            }

            outFile.WriteLine("            }).Skip(page * pageSize).Take(pageSize);");
            outFile.WriteLine("        }");
            outFile.WriteLine();
            outFile.WriteLine("        // GET: api/{0}/5", className);
            outFile.WriteLine("        [ResponseType(typeof({0}))]", className);
            outFile.WriteLine("        public async Task<IHttpActionResult> Get(int id)", className);
            outFile.WriteLine("        {");
            outFile.WriteLine("            var {0} = await {1}Repository.FindOneByAsync(x => x.{2} == id);", className.ToLower(), className, id);
            outFile.WriteLine("            if ({0} == null)", className.ToLower());
            outFile.WriteLine("            {");
            outFile.WriteLine("                return NotFound();");
            outFile.WriteLine("            }");
            outFile.WriteLine("");
            outFile.WriteLine("            return Ok({0});", className.ToLower());
            outFile.WriteLine("        }");
            outFile.WriteLine();
            outFile.WriteLine("        // POST: api/{0}", className);
            outFile.WriteLine("        [ResponseType(typeof({0}))]", className);
            outFile.WriteLine("        public async Task<IHttpActionResult> Create({0} {1})", className, className.ToLower());
            outFile.WriteLine("        {");
            outFile.WriteLine("            if (!ModelState.IsValid)");
            outFile.WriteLine("            {");
            outFile.WriteLine("                return BadRequest(ModelState);");
            outFile.WriteLine("            }");
            outFile.WriteLine("");
            outFile.WriteLine("            var model = new {0}()", className);
            outFile.WriteLine("            {");
            
            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                var columnName = s.Column;
                if (s.ColumnType != "PK")
                {
                    if (s.Equals(lastSchema))
                    {
                        outFile.WriteLine("                {0} = {1}.{0}", columnName, className.ToLower());
                    }
                    else
                    {
                        outFile.WriteLine("                {0} = {1}.{0},", columnName, className.ToLower());
                    }
                }
            }

            outFile.WriteLine("            };");
            outFile.WriteLine("");
            outFile.WriteLine("            await {0}Repository.SaveAsync(model);", className, className.ToLower());
            outFile.WriteLine("");
            outFile.WriteLine("            return CreatedAtRoute(\"DefaultApi\", new {{ id = {0}.{1} }}, {0});", className.ToLower(), id);
            outFile.WriteLine("        }");
            outFile.WriteLine();
            outFile.WriteLine("        // DELETE: api/{0}/5", className);
            outFile.WriteLine("        [ResponseType(typeof({0}))]", className);
            outFile.WriteLine("        public async Task<IHttpActionResult> Delete(int id)", className);
            outFile.WriteLine("        {");
            outFile.WriteLine("            var {0} = {1}Repository.FindOneBy(x => x.{2} == id);", className.ToLower(), className, id);
            outFile.WriteLine("            if ({0} == null)", className.ToLower());
            outFile.WriteLine("            {");
            outFile.WriteLine("                return NotFound();");
            outFile.WriteLine("            }");
            outFile.WriteLine("");
            outFile.WriteLine("            await {0}Repository.DeleteAsync({1});", className, className.ToLower());
            outFile.WriteLine("");
            outFile.WriteLine("            return Ok({0});", className.ToLower());
            outFile.WriteLine("        }");
            outFile.WriteLine();
            outFile.WriteLine("        // PUT: api/{0}/5", className);
            outFile.WriteLine("        [ResponseType(typeof(void))]");
            outFile.WriteLine("        public async Task<IHttpActionResult> Edit(int id, {0} {1})", className, className.ToLower());
            outFile.WriteLine("        {");
            outFile.WriteLine("            if (!ModelState.IsValid)");
            outFile.WriteLine("            {");
            outFile.WriteLine("                return BadRequest(ModelState);");
            outFile.WriteLine("            }");
            outFile.WriteLine("");
            outFile.WriteLine("            if (id != {0}.{1})", className.ToLower(), id);
            outFile.WriteLine("            {");
            outFile.WriteLine("                return BadRequest();");
            outFile.WriteLine("            }");
            outFile.WriteLine("");
            outFile.WriteLine("            try");
            outFile.WriteLine("            {");
            outFile.WriteLine("                var model = {1}Repository.FindOneBy(x => x.{2} == id);", className.ToLower(), className, id);
            outFile.WriteLine("");
            outFile.WriteLine("                if (model != null)");
            outFile.WriteLine("                {");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                var columnName = s.Column;
                if (s.ColumnType != "PK")
                {
                    if (s.Equals(lastSchema))
                    {
                        outFile.WriteLine("                    model.{0} = {1}.{0};", columnName, className.ToLower());
                    }
                    else
                    {
                        outFile.WriteLine("                    model.{0} = {1}.{0};", columnName, className.ToLower());
                    }
                }
            }

            outFile.WriteLine("                }");
            outFile.WriteLine("");
            outFile.WriteLine("                await {0}Repository.UpdateAsync({1});", className, className.ToLower());
            outFile.WriteLine("            }");
            outFile.WriteLine("            catch (DbUpdateConcurrencyException)");
            outFile.WriteLine("            {");
            outFile.WriteLine("                var {0}ToEdit = {1}Repository.FindOneBy(x => x.{2} == id);", className.ToLower(), className, id);
            outFile.WriteLine("");
            outFile.WriteLine("                if ({0}ToEdit != null)", className.ToLower());
            outFile.WriteLine("                {");
            outFile.WriteLine("                    return NotFound();");
            outFile.WriteLine("                }");
            outFile.WriteLine("                else");
            outFile.WriteLine("                {");
            outFile.WriteLine("                    throw;");
            outFile.WriteLine("                }");
            outFile.WriteLine("            }");
            outFile.WriteLine("");
            outFile.WriteLine("            return StatusCode(HttpStatusCode.NoContent);");
            outFile.WriteLine("        }");
            outFile.WriteLine("    }");
            outFile.WriteLine("}");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
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
