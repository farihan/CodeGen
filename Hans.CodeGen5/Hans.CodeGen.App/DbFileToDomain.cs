using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hans.CodeGen.Core.Domains;
using Hans.CodeGen.Core.Commons;
using Hans.CodeGen.Core.Utils;
using System.IO;

namespace Hans.CodeGen.App
{
    public static class DbFileToDomain
    {
        public static void Generate(DatabaseInfo db)
        {
            var path1 = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.AngularDomains);
            var path2 = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.KODomains);
            var path3 = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.RazorDomains);

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

                // create domain class
                //WriterForNh(path, tableName, className.UpperedFirstChar(), db);
                //WriterForAdoNet(path, tableName, className.UpperedFirstChar(), db);
                WriterForDomain(path1, tableName, className, db);
                WriterForDomain(path2, tableName, className, db);
                WriterForDomain(path3, tableName, className, db);
            }

            Console.WriteLine();
        }

        private static void WriterForNh(string path, string tableName, string className, DatabaseInfo db)
        {
            var property = "{ get; set; }";
            var textPath = string.Format(@"{0}\{1}\{2}.cs", path, CreationType.NHibernate, className);

            Folder.Create(string.Format(@"{0}\{1}", path, CreationType.NHibernate));

            var outFile = File.CreateText(textPath);

            outFile.WriteLine("using System;");
            outFile.WriteLine("using System.Collections.Generic;"); ;
            outFile.WriteLine("using System.Linq;");
            outFile.WriteLine("using System.Text;");
            outFile.WriteLine();
            outFile.WriteLine("namespace {0}.Domain", db.NamespaceCs);
            outFile.WriteLine("{");
            outFile.WriteLine("    public class {0}", className);
            outFile.WriteLine("    {");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                var columnName = s.Column;

                outFile.WriteLine("        public virtual {0} {1} {2}", s.MsSqlDataType(), columnName, property);
            }

            var filteredChildTableName = db.Relations
                .Where(p => p.ParentTable == tableName)
                .OrderBy(p => p.ChildTable)
                .ToList();

            foreach (var r in filteredChildTableName)
            {
                var childTableName = r.ChildTable;

                outFile.WriteLine();
                outFile.WriteLine("        //has many {0}", r.ChildTable);
                outFile.WriteLine("        public virtual IList<{0}> {0}List {1}", childTableName, property);
                outFile.WriteLine();
                outFile.WriteLine("        public virtual void Add{0}List({0} {1})", childTableName, childTableName.LoweredFirstChar());
                outFile.WriteLine("        {");
                outFile.WriteLine("            if({0}List == null)", childTableName);
                outFile.WriteLine("                {0}List = new List<{0}>();", childTableName);
                outFile.WriteLine("            {0}List.Add({1});", childTableName, childTableName.LoweredFirstChar());
                outFile.WriteLine("        }");
            }

            var filteredParentTableName = db.Relations
                .Where(p => p.ChildTable == tableName)
                .OrderBy(p => p.ParentTable)
                .ToList();

            foreach (var r in filteredParentTableName)
            {
                var parentTableName = r.ParentTable;

                if (r.ParentTable == tableName)
                    break;

                outFile.WriteLine();
                outFile.WriteLine("        //belong to {0}", r.ParentTable);
                outFile.WriteLine("        public virtual {0} {0} {1}", parentTableName, property);
            }

            outFile.WriteLine("    }");
            outFile.WriteLine("}");

            outFile.Close();
        }

        private static void WriterForAdoNet(string path, string tableName, string className, DatabaseInfo db)
        {
            var property = "{ get; set; }";            
            var textPath = string.Format(@"{0}\{1}.cs", path, className);
                        var outFile = File.CreateText(textPath);

            outFile.WriteLine("using System;");
            outFile.WriteLine("using System.Collections.Generic;"); ;
            outFile.WriteLine("using System.Linq;");
            outFile.WriteLine("using System.Text;");
            outFile.WriteLine();
            outFile.WriteLine("namespace {0}.Domains", db.NamespaceCs);
            outFile.WriteLine("{");
            outFile.WriteLine("    public class {0}", className);
            outFile.WriteLine("    {");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                var columnName = s.Column;

                outFile.WriteLine("        public {0} {1} {2}", s.MsSqlDataType(), columnName, property);
            }

            outFile.WriteLine("    }");
            outFile.WriteLine("}");

            outFile.Close();
        }
        
        private static void WriterForDomain(string path, string tableName, string className, DatabaseInfo db)
        {
            var property = "{ get; set; }";
            var textPath = string.Format(@"{0}\{1}.cs", path, className);

            var outFile = File.CreateText(textPath);

            outFile.WriteLine("using System;");
            outFile.WriteLine("using System.Collections.Generic;"); ;
            outFile.WriteLine("using System.Linq;");
            outFile.WriteLine("using System.Text;");
            outFile.WriteLine();
            outFile.WriteLine("namespace {0}.Domains", db.NamespaceCs);
            outFile.WriteLine("{");
            outFile.WriteLine("    public class {0}", className);
            outFile.WriteLine("    {");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                var columnName = s.Column;

                outFile.WriteLine("        public {0} {1} {2}", s.MsSqlDataType(), columnName, property);
            }

            foreach (var r in db.Relations.Where(p => p.ChildTable == tableName).OrderBy(p => p.ParentTable))
            {
                if (r.ParentTable == tableName)
                    break;
                outFile.WriteLine("        public {0} {0} {1}", r.ParentTable.UpperedFirstChar(), property);
            }

            foreach (var r in db.Relations.Where(p => p.ParentTable == tableName).OrderBy(p => p.ChildTable))
            {
                outFile.WriteLine("        public IList<{0}> {0}List {1}", r.ChildTable.UpperedFirstChar(), property);
            }

            outFile.WriteLine("    }");
            outFile.WriteLine("}");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }
    }
}
