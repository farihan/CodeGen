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
            var path1 = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.AngularModels);
            var path2 = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.KOModels);
            var path3 = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.RazorModels);

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

                WriterForModel(path1, tableName, className, db);
                WriterForModel(path2, tableName, className, db);
                WriterForModel(path3, tableName, className, db);

                WriterForReadModel(path1, tableName, className, db);
                WriterForReadModel(path2, tableName, className, db);
                WriterForReadModel(path3, tableName, className, db);

                WriterForCreateModel(path1, tableName, className, db);
                WriterForCreateModel(path2, tableName, className, db);
                WriterForCreateModel(path3, tableName, className, db);

                WriterForUpdateModel(path1, tableName, className, db);
                WriterForUpdateModel(path2, tableName, className, db);
                WriterForUpdateModel(path3, tableName, className, db);
            }

            Console.WriteLine();
        }

        private static void WriterForModel(string path, string tableName, string className, DatabaseInfo db)
        {
            var singularization = new Singularization();
            var pluralization = new Pluralization();
            var property = "{ get; set; }";
            var textPath = string.Format(@"{0}\{1}ViewModel.cs", path, className);

            var outFile = File.CreateText(textPath);

            //outFile.WriteLine("using System;");
            //outFile.WriteLine("using System.Collections.Generic;"); ;
            //outFile.WriteLine("using System.Linq;");
            //outFile.WriteLine("using System.Text;");
            //outFile.WriteLine("using CFF.Models;");
            //outFile.WriteLine("using System.ComponentModel.DataAnnotations;");
            outFile.WriteLine();
            outFile.WriteLine("namespace {0}.ViewModels", db.ApplicationName);
            outFile.WriteLine("{");
            outFile.WriteLine("    public class {0}ViewModel", className);
            outFile.WriteLine("    {");

            //foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            //{
            //    var columnName = s.Column;
            //    outFile.WriteLine("        public {0} {1} {2}", s.MsSqlDataType(), columnName, property);
            //    outFile.WriteLine();
            //}

            //foreach (var r in db.Relations.Where(p => p.ChildTable == tableName).OrderBy(p => p.ParentTable))
            //{
            //    if (r.ParentTable == tableName)
            //        break;
            //    outFile.WriteLine("        public {0} {0} {1}", singularization.Singularize(r.ParentTable.UpperedFirstChar()), property);
            //}

            //foreach (var r in db.Relations.Where(p => p.ParentTable == tableName).OrderBy(p => p.ChildTable))
            //{
            //    outFile.WriteLine("        public IList<{0}> {1} {2}", 
            //        singularization.Singularize(r.ChildTable.UpperedFirstChar()),
            //        pluralization.Pluralize(r.ChildTable.UpperedFirstChar()), 
            //        property);
            //}

            outFile.WriteLine("        public int TotalPages {0}", property);
            outFile.WriteLine("        public int CurrentPage {0}", property);
            outFile.WriteLine("        public int PageSize {0}", property);
            outFile.WriteLine("        public int PageIndex {0}", property);
            outFile.WriteLine("        public string Sort {0}", property);
            outFile.WriteLine("        public bool IsAsc {0}", property);
            outFile.WriteLine("        public IList<{0}ReadViewModel> {1} {2}", className, pluralization.Pluralize(className), property);
            outFile.WriteLine();
            outFile.WriteLine("        public {0}ViewModel()", className);
            outFile.WriteLine("        {");
            outFile.WriteLine("            TotalPages = 0;");
            outFile.WriteLine("            CurrentPage = 0;");
            outFile.WriteLine("            PageSize = 0;");
            outFile.WriteLine("            PageIndex = 0;");
            outFile.WriteLine("            Sort = \"\";");
            outFile.WriteLine("            {0} = new List<{1}ReadViewModel>();", pluralization.Pluralize(className), className);
            outFile.WriteLine("        }");
            outFile.WriteLine("    }");
            outFile.WriteLine("}");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForReadModel(string path, string tableName, string className, DatabaseInfo db)
        {
            var singularization = new Singularization();
            var pluralization = new Pluralization();
            var property = "{ get; set; }";
            var textPath = string.Format(@"{0}\{1}ReadViewModel.cs", path, className);

            var outFile = File.CreateText(textPath);

            //outFile.WriteLine("using System;");
            //outFile.WriteLine("using System.Collections.Generic;"); ;
            //outFile.WriteLine("using System.Linq;");
            //outFile.WriteLine("using System.Text;");

            //outFile.WriteLine("using CFF.Helpers.Common;");
            outFile.WriteLine("using System.ComponentModel.DataAnnotations;");
            outFile.WriteLine();
            outFile.WriteLine("namespace {0}.ViewModels", db.ApplicationName);
            outFile.WriteLine("{");
            outFile.WriteLine("    public class {0}ReadViewModel", className);
            outFile.WriteLine("    {");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                var columnName = s.Column;

                WriteReadProperty(outFile, s, columnName, property);
                outFile.WriteLine();
            }

            foreach (var r in db.Relations.Where(p => p.ChildTable == tableName).OrderBy(p => p.ParentTable))
            {
                if (r.ParentTable == tableName)
                    break;
                outFile.WriteLine("        public {0} {0} {1}", r.ParentTable, property);
            }

            foreach (var r in db.Relations.Where(p => p.ParentTable == tableName).OrderBy(p => p.ChildTable))
            {
                outFile.WriteLine("        public IList<{0}> {1} {2}",
                    r.ChildTable,
                    pluralization.Pluralize(r.ChildTable.UpperedFirstChar()),
                    property);
            }

            outFile.WriteLine("    }");
            outFile.WriteLine("}");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForCreateModel(string path, string tableName, string className, DatabaseInfo db)
        {
            var unusedFields = db.RemoveFields.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            var singularization = new Singularization();
            var pluralization = new Pluralization();
            var property = "{ get; set; }";
            var textPath = string.Format(@"{0}\{1}CreateViewModel.cs", path, className);

            var outFile = File.CreateText(textPath);

            //outFile.WriteLine("using System;");
            //outFile.WriteLine("using System.Collections.Generic;"); ;
            //outFile.WriteLine("using System.Linq;");
            //outFile.WriteLine("using System.Text;");
            //outFile.WriteLine("using CFF.Models;");
            //outFile.WriteLine("using CFF.Helpers.Common;");
            //outFile.WriteLine("using CFF.Models;");
            outFile.WriteLine("using System.ComponentModel.DataAnnotations;");
            outFile.WriteLine();
            outFile.WriteLine("namespace {0}.ViewModels", db.ApplicationName);
            outFile.WriteLine("{");
            outFile.WriteLine("    public class {0}CreateViewModel", className);
            outFile.WriteLine("    {");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                var columnName = s.Column;

                if (columnName.ToLower() == "id")
                    continue;

                if (!unusedFields.Any(x => x == s.Column))
                {
                    if (s.ColumnType == "FK")
                    {
                        outFile.WriteLine("        [Required]");
                        //outFile.WriteLine("        [Required(ErrorMessage = \"The field is required\")]", columnName);
                    }

                    if (s.ColumnType == "NONE")
                    {
                        outFile.WriteLine("        [Required]");
                        //outFile.WriteLine("        [Required(ErrorMessage = \"The field is required\")]", columnName);

                        //if (s.MsSqlDataType() == "int")
                        //{
                        //    outFile.WriteLine("        [RegularExpression(@\"^[0-9]+$\", ErrorMessage=\"Enter numbers only\")]", columnName);
                        //}

                        //if (s.MsSqlDataType() == "double")//for money usage
                        //{
                        //    outFile.WriteLine("        [RegularExpression(@\"^[0-9]+(\\.[0-9][0-9]?)?$\", ErrorMessage=\"Enter numbers with two decimal points only\")]", columnName);
                        //}

                        //if (s.MsSqlDataType() == "decimal")
                        //{
                        //    outFile.WriteLine("        [RegularExpression(@\"^[0-9]+(\\.[0-9]+)?$\", ErrorMessage=\"Enter decimal numbers only\")]", columnName);
                        //}

                        //if (s.MsSqlDataType() == "float")
                        //{
                        //    outFile.WriteLine("        [RegularExpression(@\"^[0-9]+(\\.[0-9]+)?$\", ErrorMessage=\"Enter decimal numbers only\")]", columnName);
                        //}
                    }
                }
                //outFile.WriteLine("        public {0} {1} {2} {3}", s.MsSqlDataType(), columnName, property, s.MsSqlDataType() == MsSqlDataType.String2 ? "= string.Empty;" : "");
                WriteProperty(outFile, s, columnName, property);
                outFile.WriteLine();
            }

            foreach (var r in db.Relations.Where(p => p.ChildTable == tableName).OrderBy(p => p.ParentTable))
            {
                if (r.ParentTable == tableName)
                    break;
                outFile.WriteLine("        public {0} {0} {1}", r.ParentTable, property);
            }

            foreach (var r in db.Relations.Where(p => p.ParentTable == tableName).OrderBy(p => p.ChildTable))
            {
                outFile.WriteLine("        public IList<{0}> {1} {2}",
                    r.ChildTable,
                    pluralization.Pluralize(r.ChildTable.UpperedFirstChar()),
                    property);
            }

            outFile.WriteLine("    }");
            outFile.WriteLine("}");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForUpdateModel(string path, string tableName, string className, DatabaseInfo db)
        {
            var unusedFields = db.RemoveFields.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            var singularization = new Singularization();
            var pluralization = new Pluralization();
            var property = "{ get; set; }";
            var textPath = string.Format(@"{0}\{1}UpdateViewModel.cs", path, className);

            var outFile = File.CreateText(textPath);

            //outFile.WriteLine("using System;");
            //outFile.WriteLine("using System.Collections.Generic;"); ;
            //outFile.WriteLine("using System.Linq;");
            //outFile.WriteLine("using System.Text;");
            //outFile.WriteLine("using CFF.Helpers.Common;");
            outFile.WriteLine("using System.ComponentModel.DataAnnotations;");
            outFile.WriteLine();
            outFile.WriteLine("namespace {0}.ViewModels", db.ApplicationName);
            outFile.WriteLine("{");
            outFile.WriteLine("    public class {0}UpdateViewModel", className);
            outFile.WriteLine("    {");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                var columnName = s.Column;
                if (!unusedFields.Any(x => x == s.Column))
                {
                    if (s.ColumnType == "FK")
                    {
                        outFile.WriteLine("        [Required]");
                    }

                    if (s.ColumnType == "NONE")
                    {
                        if (s.Column != "Attachment" && s.Column != "Image")
                        {
                            outFile.WriteLine("        [Required]");
                        }
                    }
                }

                WriteProperty(outFile, s, columnName, property, true);
                outFile.WriteLine();
            }

            foreach (var r in db.Relations.Where(p => p.ChildTable == tableName).OrderBy(p => p.ParentTable))
            {
                if (r.ParentTable == tableName)
                    break;
                outFile.WriteLine("        public {0} {0} {1}", singularization.Singularize(r.ParentTable.UpperedFirstChar()), property);
            }

            foreach (var r in db.Relations.Where(p => p.ParentTable == tableName).OrderBy(p => p.ChildTable))
            {
                outFile.WriteLine("        public IList<{0}> {1} {2}",
                    r.ChildTable,
                    pluralization.Pluralize(r.ChildTable.UpperedFirstChar()),
                    property);
            }

            outFile.WriteLine("    }");
            outFile.WriteLine("}");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriteProperty(StreamWriter outFile, Schema schema, string columnName, string property, bool isUpdate = false)
        {
            if (columnName == "Id" || columnName == "CreatedAt" || columnName == "UpdatedAt")
            {
                outFile.WriteLine("        [Display(Name = \"{0}\")]", columnName.SplitCamelCase());
                outFile.WriteLine("        public {0} {1} {2} {3}", schema.MsSqlDataType(), columnName, property, schema.MsSqlDataType() == MsSqlDataType.String2 ? "= string.Empty;" : "");
            }
            else if (columnName == "Image")
            {
                if (isUpdate)
                {
                    outFile.WriteLine("        [Display(Name = \"Image\")]", columnName.SplitCamelCase());
                    outFile.WriteLine("        public string? Image { get; set; } = string.Empty;");
                    outFile.WriteLine();
                    outFile.WriteLine("        [Display(Name = \"ImageUpdate\")]", columnName.SplitCamelCase());
                    outFile.WriteLine("        public IFormFile? ImageUpdate { get; set; }");
                }
                else
                {
                    outFile.WriteLine("        [Display(Name = \"Image\")]", columnName.SplitCamelCase());
                    outFile.WriteLine("        public IFormFile? Image { get; set; }");
                }
            }
            else if (columnName == "Attachment")
            {

                if (isUpdate)
                {
                    outFile.WriteLine("        [Display(Name = \"Attachment\")]", columnName.SplitCamelCase());
                    outFile.WriteLine("        public string? Attachment { get; set; } = string.Empty;");
                    outFile.WriteLine();
                    outFile.WriteLine("        [Display(Name = \"AttachmentUpdate\")]", columnName.SplitCamelCase());
                    outFile.WriteLine("        public IFormFile? AttachmentUpdate { get; set; }");
                }
                else
                {
                    outFile.WriteLine("        [Display(Name = \"Attachment\")]", columnName.SplitCamelCase());
                    outFile.WriteLine("        public IFormFile? Attachment { get; set; }");
                }
            }
            else if (columnName == "Status")
            {
                outFile.WriteLine("        public StatusType Status { get; set; }");
            }
            else if (columnName == "Featured")
            {
                outFile.WriteLine("        public bool Featured { get; set; }");
            }
            else
            {
                outFile.WriteLine("        [Display(Name = \"{0}\")]", columnName.SplitCamelCase());
                outFile.WriteLine("        public {0}? {1} {2} {3}", schema.MsSqlDataType(), columnName, property, schema.MsSqlDataType() == MsSqlDataType.String2 ? "= string.Empty;" : "");
            }
        }

        private static void WriteReadProperty(StreamWriter outFile, Schema schema, string columnName, string property, bool isUpdate = false)
        {
            if (columnName == "Id" || columnName == "CreatedAt" || columnName == "UpdatedAt")
            {
                outFile.WriteLine("        [Display(Name = \"{0}\")]", columnName.SplitCamelCase());
                outFile.WriteLine("        public {0} {1} {2} {3}", schema.MsSqlDataType(), columnName, property, schema.MsSqlDataType() == MsSqlDataType.String ? "= string.Empty;" : "");
            }
            else if (columnName == "Image")
            {
                outFile.WriteLine("        [Display(Name = \"Image\")]", columnName.SplitCamelCase());
                outFile.WriteLine("        public string? Image { get; set; } = string.Empty;");
            }
            else if (columnName == "Attachment")
            {
                outFile.WriteLine("        public string? Attachment { get; set; } = string.Empty;");
            }
            else if (columnName == "Status")
            {
                outFile.WriteLine("        public StatusType Status { get; set; }");
            }
            else if (columnName == "Featured")
            {
                outFile.WriteLine("        public bool Featured { get; set; }");
            }
            else
            {
                outFile.WriteLine("        [Display(Name = \"{0}\")]", columnName.SplitCamelCase());
                outFile.WriteLine("        public {0}? {1} {2} {3}", schema.MsSqlDataType(), columnName, property, schema.MsSqlDataType() == MsSqlDataType.String ? "= string.Empty;" : "");
            }
        }
    }
}
