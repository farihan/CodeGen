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
    public class DbFileToModel
    {
        public static void Generate(DatabaseInfo db)
        {
            var path = string.Format(@"{0}\{1}", db.OutputDirectory, CreationType.Models);

            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("Output Directory does not exist");
            }

            Folder.Create(path);

            var orders = db.Schemas.Where(x => x.Table == "Products").ToList();

            foreach (var tableName in db.Schemas
                .Select(x => x.Table)
                .Distinct())
            {
                var className = tableName.UpperedFirstChar();

                // create model class
                WriterForModel(path, tableName, className, db);
            }

            Console.WriteLine();
        }

        private static void WriterForModel(string path, string tableName, string className, DatabaseInfo db)
        {
            var property = "{ get; set; }";
            var textPath = string.Format(@"{0}\{1}Model.cs", path, className);

            var outFile = File.CreateText(textPath);

            outFile.WriteLine("using System;");
            outFile.WriteLine("using System.Collections.Generic;"); ;
            outFile.WriteLine("using System.Linq;");
            outFile.WriteLine("using System.Text;");
            outFile.WriteLine();
            outFile.WriteLine("namespace {0}.Models", db.NamespaceCs);
            outFile.WriteLine("{");
            outFile.WriteLine("    public class {0}Model", className);
            outFile.WriteLine("    {");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                var columnName = s.Column;
                
                if (s.ColumnType == "FK")
                {
                    outFile.WriteLine("        [Required(ErrorMessage = \"The field is required\")]", columnName);
                }
                
                if (s.ColumnType == "NONE")
                {
                    outFile.WriteLine("        [Required(ErrorMessage = \"The field is required\")]", columnName);

                    if (s.MsSqlDataType() == "int")
                    {
                        outFile.WriteLine("        [RegularExpression(@\"^[0-9]+$\", ErrorMessage=\"Enter numbers only\")]", columnName);
                    }

                    if (s.MsSqlDataType() == "double")//for money usage
                    {
                        outFile.WriteLine("        [RegularExpression(@\"^[0-9]+(\\.[0-9][0-9]?)?$\", ErrorMessage=\"Enter numbers with two decimal points only\")]", columnName);
                    }

                    if (s.MsSqlDataType() == "decimal")
                    {
                        outFile.WriteLine("        [RegularExpression(@\"^[0-9]+(\\.[0-9]+)?$\", ErrorMessage=\"Enter decimal numbers only\")]", columnName);
                    }

                    if (s.MsSqlDataType() == "float")
                    {
                        outFile.WriteLine("        [RegularExpression(@\"^[0-9]+(\\.[0-9]+)?$\", ErrorMessage=\"Enter decimal numbers only\")]", columnName);
                    }
                }

                outFile.WriteLine("        public {0} {1} {2}", s.MsSqlDataType(), columnName, property);
                outFile.WriteLine();
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

            outFile.WriteLine("        public int TotalPages {0}", property);
            outFile.WriteLine("        public int CurrentPage {0}", property);
            outFile.WriteLine("        public int PageSize {0}", property);
            outFile.WriteLine("        public int PageIndex {0}", property);
            outFile.WriteLine("        public string Sort {0}", property);
            outFile.WriteLine("        public bool IsAsc {0}", property);
            outFile.WriteLine("        public IList<{0}> {0}s {1}", className, property);
            outFile.WriteLine();
            outFile.WriteLine("        public {0}Model()", className);
            outFile.WriteLine("        {");
            outFile.WriteLine("            TotalPages = 0;");
            outFile.WriteLine("            CurrentPage = 0;");
            outFile.WriteLine("            PageSize = 0;");
            outFile.WriteLine("            PageIndex = 0;");
            outFile.WriteLine("            {0}s = new List<{0}>();", className);
            outFile.WriteLine("        }");
            outFile.WriteLine("    }");
            outFile.WriteLine("}");

            outFile.Close();

            Console.Write(string.Format("\n{0}Model.cs created", className));
        }
    }
}
