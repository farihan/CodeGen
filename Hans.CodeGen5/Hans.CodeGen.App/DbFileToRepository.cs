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
    public class DbFileToRepository
    {
        public static void Generate(DatabaseInfo db)
        {
            var path1 = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.AngularRepository);
            var path2 = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.KORepository);
            var path3 = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.RazorRepository);

            if (string.IsNullOrEmpty(path1) || string.IsNullOrEmpty(path2) || string.IsNullOrEmpty(path3))
            {
                throw new Exception("Output Directory does not exist");
            }

            Folder.Create(path1);
            Folder.Create(path2);
            Folder.Create(path3);

            foreach (var tableName in db.Schemas
                .Select(x => x.Table)
                .Distinct())
            {
                var className = tableName.UpperedFirstChar();

            }

            WriterForInterface(path1, db);
            WriterForImplementation(path1, db);

            WriterForInterface(path2, db);
            WriterForImplementation(path2, db);

            WriterForInterface(path3, db);
            WriterForImplementation(path3, db);

            Console.WriteLine();
        }

        private static void WriterForInterface(string path, DatabaseInfo db)
        {
            var textPath = string.Format(@"{0}\IRepository.cs", path);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("using System;");
            outFile.WriteLine("using System.Collections.Generic;");
            outFile.WriteLine("using System.Linq;");
            outFile.WriteLine("using System.Linq.Expressions;");
            outFile.WriteLine("using System.Text;");
            outFile.WriteLine("using System.Threading.Tasks;");
            outFile.WriteLine("");
            outFile.WriteLine("namespace {0}.Core.Repositories", db.NamespaceCs);
            outFile.WriteLine("{");
            outFile.WriteLine("    public interface IRepository<TModel> where TModel : class");
            outFile.WriteLine("    {");
            outFile.WriteLine("        void Save(TModel instance);");
            outFile.WriteLine("        void Update(TModel instance);");
            outFile.WriteLine("        void Delete(TModel instance);");
            outFile.WriteLine("        Task SaveAsync(TModel instance);");
            outFile.WriteLine("        Task UpdateAsync(TModel instance);");
            outFile.WriteLine("        Task DeleteAsync(TModel instance);");
            outFile.WriteLine("");
            outFile.WriteLine("        IQueryable<TModel> FindAll();");
            outFile.WriteLine("        IQueryable<TModel> FindAllBy(System.Linq.Expressions.Expression<Func<TModel, bool>> where);");
            outFile.WriteLine("        TModel FindOneBy(System.Linq.Expressions.Expression<Func<TModel, bool>> where);");
            outFile.WriteLine("");
            outFile.WriteLine("        Task<IQueryable<TModel>> FindAllAsync();");
            outFile.WriteLine("        Task<IQueryable<TModel>> FindAllByAsync(System.Linq.Expressions.Expression<Func<TModel, bool>> where);");
            outFile.WriteLine("        Task<TModel> FindOneByAsync(System.Linq.Expressions.Expression<Func<TModel, bool>> where);");
            outFile.WriteLine("    }");
            outFile.WriteLine("}");
            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForImplementation(string path, DatabaseInfo db)
        {
            var textPath = string.Format(@"{0}\Repository.cs", path);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("using System;");
            outFile.WriteLine("using System.Collections.Generic;");
            outFile.WriteLine("using System.Linq;");
            outFile.WriteLine("using System.Text;");
            outFile.WriteLine("using System.Threading.Tasks;");
            outFile.WriteLine("");
            outFile.WriteLine("namespace {0}.Core.Repositories", db.NamespaceCs);
            outFile.WriteLine("{");
            outFile.WriteLine("    public class Repository<TModel> : IRepository<TModel> where TModel : class");
            outFile.WriteLine("    {");
            outFile.WriteLine("        private {0}Context Context {{ get; set; }}", db.ApplicationName);
            outFile.WriteLine("");
            outFile.WriteLine("        public Repository()");
            outFile.WriteLine("        {");
            outFile.WriteLine("            this.Context = new {0}Context();", db.ApplicationName);
            outFile.WriteLine("        }");
            outFile.WriteLine("");
            outFile.WriteLine("        public void Save(TModel instance)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            Context.Set<TModel>().Add(instance);");
            outFile.WriteLine("            Context.SaveChanges();");
            outFile.WriteLine("        }");
            outFile.WriteLine("");
            outFile.WriteLine("        public void Update(TModel instance)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            Context.Set<TModel>().Attach(instance);");
            outFile.WriteLine("            Context.Entry(instance).State = System.Data.Entity.EntityState.Modified;");
            outFile.WriteLine("            Context.SaveChanges();");
            outFile.WriteLine("        }");
            outFile.WriteLine("");
            outFile.WriteLine("        public void Delete(TModel instance)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            Context.Set<TModel>().Remove(instance);");
            outFile.WriteLine("            Context.SaveChanges();");
            outFile.WriteLine("        }");
            outFile.WriteLine("");
            outFile.WriteLine("        public async Task SaveAsync(TModel instance)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            Context.Set<TModel>().Add(instance);");
            outFile.WriteLine("            await Context.SaveChangesAsync();");
            outFile.WriteLine("        }");
            outFile.WriteLine("");
            outFile.WriteLine("        public async Task UpdateAsync(TModel instance)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            Context.Set<TModel>().Attach(instance);");
            outFile.WriteLine("            Context.Entry(instance).State = System.Data.Entity.EntityState.Modified;");
            outFile.WriteLine("            await Context.SaveChangesAsync();");
            outFile.WriteLine("        }");
            outFile.WriteLine("");
            outFile.WriteLine("        public async Task DeleteAsync(TModel instance)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            Context.Set<TModel>().Remove(instance);");
            outFile.WriteLine("            await Context.SaveChangesAsync();");
            outFile.WriteLine("        }");
            outFile.WriteLine("");
            outFile.WriteLine("        public IQueryable<TModel> FindAll()");
            outFile.WriteLine("        {");
            outFile.WriteLine("            return Context.Set<TModel>();");
            outFile.WriteLine("        }");
            outFile.WriteLine("");
            outFile.WriteLine("        public IQueryable<TModel> FindAllBy(System.Linq.Expressions.Expression<Func<TModel, bool>> where)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            return Context.Set<TModel>().Where(where.Compile()).AsQueryable();");
            outFile.WriteLine("        }");
            outFile.WriteLine("");
            outFile.WriteLine("        public TModel FindOneBy(System.Linq.Expressions.Expression<Func<TModel, bool>> where)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            return Context.Set<TModel>().FirstOrDefault(where.Compile());");
            outFile.WriteLine("        }");
            outFile.WriteLine("");
            outFile.WriteLine("        public Task<IQueryable<TModel>> FindAllAsync()");
            outFile.WriteLine("        {");
            outFile.WriteLine("            return Task.Run<IQueryable<TModel>>(() =>");
            outFile.WriteLine("            {");
            outFile.WriteLine("                return Context.Set<TModel>().AsParallel().AsQueryable();");
            outFile.WriteLine("            });");
            outFile.WriteLine("        }");
            outFile.WriteLine("");
            outFile.WriteLine("        public Task<IQueryable<TModel>> FindAllByAsync(System.Linq.Expressions.Expression<Func<TModel, bool>> where)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            return Task.Run<IQueryable<TModel>>(() =>");
            outFile.WriteLine("            {");
            outFile.WriteLine("                return FindAllBy(where);");
            outFile.WriteLine("            });");
            outFile.WriteLine("        }");
            outFile.WriteLine("");
            outFile.WriteLine("        public Task<TModel> FindOneByAsync(System.Linq.Expressions.Expression<Func<TModel, bool>> where)");
            outFile.WriteLine("        {");
            outFile.WriteLine("            return Task.Run<TModel>(() =>");
            outFile.WriteLine("            {");
            outFile.WriteLine("                return FindOneBy(where);");
            outFile.WriteLine("            });");
            outFile.WriteLine("        }");
            outFile.WriteLine("    }");
            outFile.WriteLine("}");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }
    }
}
