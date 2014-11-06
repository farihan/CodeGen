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
    public class DbFileToApiController
    {
        public static void Generate(DatabaseInfo db)
        {
            var path = string.Format(@"{0}\{1}", db.OutputDirectory, CreationType.ApiControllers);

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
                // index
                // details
                // create
                // edit
                // delete
            }

            Console.WriteLine();
        }

        private static void WriterForApiController(string path, string tableName, string className, DatabaseInfo db)
        {
            var textPath = string.Format(@"{0}\{1}{2}.cs", path, className, CreationType.Controller);
            var id = db.Schemas.Where(p => p.Table == tableName).FirstOrDefault().Column;

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
            outFile.WriteLine("        public IQueryable<{0}> Get{0}List()", className);
            outFile.WriteLine("        {");
            outFile.WriteLine("            return {0}Repository.FindAll();", className);
            outFile.WriteLine("        }");
            outFile.WriteLine();
            outFile.WriteLine("        // GET: api/{0}", className);
            outFile.WriteLine("        public int Get{0}ListSize()", className);
            outFile.WriteLine("        {");
            outFile.WriteLine("            return {0}Repository.FindAll().Count();", className);
            outFile.WriteLine("        }");
            outFile.WriteLine();
            outFile.WriteLine("        // GET: api/{0}", className);
            outFile.WriteLine("        public IQueryable<{0}> Get{0}List(int page, int pageSize)", className);
            outFile.WriteLine("        {");
            outFile.WriteLine("            return {0}Repository.FindAll().Skip(page * pageSize).Take(pageSize);", className);
            outFile.WriteLine("        }");
            outFile.WriteLine();
            outFile.WriteLine("        // GET: api/{0}/5", className);
            outFile.WriteLine("        [ResponseType(typeof({0}))]", className);
            outFile.WriteLine("        public async Task<IHttpActionResult> Get{0}(int id)", className);
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
            outFile.WriteLine("        public async Task<IHttpActionResult> Create{0}({0} {1})", className, className.ToLower());
            outFile.WriteLine("        {");
            outFile.WriteLine("            if (!ModelState.IsValid)");
            outFile.WriteLine("            {");
            outFile.WriteLine("                return BadRequest(ModelState);");
            outFile.WriteLine("            }");
            outFile.WriteLine("");
            outFile.WriteLine("            await {0}Repository.SaveAsync({0});", className, className.ToLower());
            outFile.WriteLine("");
            outFile.WriteLine("            return CreatedAtRoute(\"DefaultApi\", new {{ id = {0}.{1} }}, {0});", className.ToLower(), id);
            outFile.WriteLine("        }");
            outFile.WriteLine();
            outFile.WriteLine("        // DELETE: api/{0}/5", className);
            outFile.WriteLine("        [ResponseType(typeof({0}))]", className);
            outFile.WriteLine("        public async Task<IHttpActionResult> Delete{0}(int id)", className);
            outFile.WriteLine("        {");
            outFile.WriteLine("            var {0} = await {1}Repository.FindOneBy(x => x.{2} == id);", className.ToLower(), className, id);
            outFile.WriteLine("            if ({0} == null)", className.ToLower());
            outFile.WriteLine("            {");
            outFile.WriteLine("                return NotFound();");
            outFile.WriteLine("            }");
            outFile.WriteLine("");
            outFile.WriteLine("            await {0}Repository.DeleteAsync({0});", className, className.ToLower());
            outFile.WriteLine("");
            outFile.WriteLine("            return Ok({0});", className.ToLower());
            outFile.WriteLine("        }");
            outFile.WriteLine();
            outFile.WriteLine("        // PUT: api/{0}/5", className);
            outFile.WriteLine("        [ResponseType(typeof(void))]");
            outFile.WriteLine("        public async Task<IHttpActionResult> Edit{0}(int id, {0} {1})", className, className.ToLower());
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
            outFile.WriteLine("                await {0}Repository.UpdateAsync({0});", className, className.ToLower());
            outFile.WriteLine("            }");
            outFile.WriteLine("            catch (DbUpdateConcurrencyException)");
            outFile.WriteLine("            {");
            outFile.WriteLine("                var {0} = {1}Repository.FindOneBy(x => x.{2} == id);", className.ToLower(), className, id);
            outFile.WriteLine("                if ({0} != null)", className);
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
            outFile.WriteLine();
            outFile.WriteLine("        protected override void Dispose(bool disposing)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            if (disposing)");
            outFile.WriteLine("            {");
            outFile.WriteLine("                {0}Repository.Dispose();", className);
            outFile.WriteLine("            }");
            outFile.WriteLine("");
            outFile.WriteLine("            base.Dispose(disposing);");
            outFile.WriteLine("        }");
            outFile.WriteLine();
            outFile.WriteLine("    }");
            outFile.WriteLine("}");

            outFile.Close();

            Console.Write(string.Format("\n{0}ApiController.cs created", className));
        }
    }
}
