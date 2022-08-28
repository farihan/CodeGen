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
            var path4 = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.RazorConfigurations);
            var path5 = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.RazorCore);
            var path6 = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.RazorPdf);

            if (string.IsNullOrEmpty(path1) || string.IsNullOrEmpty(path2) || string.IsNullOrEmpty(path3))
            {
                throw new Exception("Output Directory does not exist");
            }

            Folder.Create(path1);
            Folder.Create(path2);
            Folder.Create(path3);

            Folder.Create(path4);
            Folder.Create(path5);
            Folder.Create(path6);

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
                WriterForConfiguration(path4, tableName, className, db);
                WriterForPdf(path6, tableName, className, db);
            }
            
            WriterForContext(path5, db);

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
            var ns = string.Format("{0}.Models", db.ApplicationName);
            var property = "{ get; set; }";
            var textPath = string.Format(@"{0}\{1}.cs", path, className);

            var outFile = File.CreateText(textPath);

            //outFile.WriteLine("using System;");
            //outFile.WriteLine("using System.Collections.Generic;"); ;
            //outFile.WriteLine("using System.Linq;");
            //outFile.WriteLine("using System.Text;");
            //outFile.WriteLine();
            outFile.WriteLine("namespace {0}", ns);
            outFile.WriteLine("{");
            outFile.WriteLine("    public class {0}", className);
            outFile.WriteLine("    {");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                var columnName = s.Column;
                var constraintType = s.IsNullable == "NULL" ? "?" : "";
                if (s.ColumnType == "PK")
                {
                    outFile.WriteLine("        public {0}{1} {2} {3}", s.MsSqlDataType(), constraintType, columnName, property);
                }
                else if (s.ColumnType == "FK")
                {
                    outFile.WriteLine("        public {0}{1} {2} {3}", s.MsSqlDataType(), constraintType, columnName, property);
                }
                else
                {
                    if (s.MsSqlDataType() == "string?")
                    {
                        outFile.WriteLine("        public {0} {2} {3}", s.MsSqlDataType(), constraintType, columnName, property);
                    }
                    else if (s.DataType.ToLower() == "datetime2")
                    {
                        outFile.WriteLine("        public {0} {2} {3}", s.MsSqlDataType(), constraintType, columnName, property);
                    }
                    else
                    {
                        outFile.WriteLine("        public {0}{1} {2} {3}", s.MsSqlDataType(), constraintType, columnName, property);
                    }
                }
            }

            var pluralize = new Pluralization();
            foreach (var r in db.Relations.Where(p => p.ChildTable == tableName).OrderBy(p => p.ParentTable))
            {
                if (r.ParentTable == tableName)
                    break;
                outFile.WriteLine("        public {0}? {0} {1}", r.ParentTable.UpperedFirstChar(), property);
            }

            foreach (var r in db.Relations.Where(p => p.ParentTable == tableName).OrderBy(p => p.ChildTable))
            {
                if (r.RelationType == RelationType.OneToMany)
                    outFile.WriteLine("        public IList<{0}>? {1} {2}", r.ChildTable.UpperedFirstChar(), pluralize.Pluralize(r.ChildTable.UpperedFirstChar()), property);
                else if (r.RelationType == RelationType.OneToOne)
                    outFile.WriteLine("        public {0}? {1} {2}", r.ChildTable.UpperedFirstChar(), r.ChildTable.UpperedFirstChar(), property);
            }

            outFile.WriteLine("    }");
            outFile.WriteLine("}");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForConfiguration(string path, string tableName, string className, DatabaseInfo db)
        {
            var ns = string.Format("{0}.DataAccess.Configurations", db.ApplicationName);
            var textPath = string.Format(@"{0}\{1}Configuration.cs", path, className);

            var outFile = File.CreateText(textPath);

            outFile.WriteLine("using CFF.Models;");
            outFile.WriteLine("using Microsoft.EntityFrameworkCore;");
            outFile.WriteLine("using Microsoft.EntityFrameworkCore.Metadata.Builders;");
            outFile.WriteLine();
            outFile.WriteLine("namespace {0}", ns);
            outFile.WriteLine("{");
            outFile.WriteLine("    public class {0}Configuration : IEntityTypeConfiguration<{0}>", className);
            outFile.WriteLine("    {");
            outFile.WriteLine("        public void Configure(EntityTypeBuilder<{0}> builder)", className);
            outFile.WriteLine("        {");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                var columnName = s.Column;

                if (s.ColumnType == "PK")
                {
                    outFile.WriteLine("            builder.HasKey(x => x.{0});", s.Column);
                    continue;
                }

                if (s.ColumnType == "FK")
                {
                    outFile.WriteLine();
                    outFile.WriteLine("            builder.HasIndex(x => x.{0}).IsUnique(false);", s.Column);
                    continue;
                }

                if (s.DataType.ToLower() == "nvarchar" && s.MaxLength != "MAX")
                {
                    outFile.WriteLine();
                    outFile.WriteLine("            builder.Property(x => x.{0})", s.Column);
                    outFile.WriteLine("                .HasMaxLength({0});", s.MaxLength);
                }
                else if (s.DataType.ToLower() == "decimal")
                {
                    outFile.WriteLine();
                    outFile.WriteLine("            builder.Property(x => x.{0})", s.Column);
                    outFile.WriteLine("                .HasPrecision(18, 2);", s.MaxLength);
                }
                else if (s.DataType.ToLower() == "datetime2")
                {
                    outFile.WriteLine();
                    outFile.WriteLine("            builder.Property(x => x.{0})", s.Column);
                    outFile.WriteLine("                .HasPrecision(7);", s.MaxLength);
                }
                else
                {
                    outFile.WriteLine();
                    outFile.WriteLine("            builder.Property(x => x.{0});", s.Column);
                }
            }

            var pluralize = new Pluralization();

            if (tableName == "LookupComplainReason")
            {
                var a = tableName;
            }

            foreach (var r in db.Relations.Where(p => p.ParentTable == tableName).OrderBy(p => p.ChildTable))
            {
                if (r.RelationType == RelationType.OneToMany)
                {
                    outFile.WriteLine();
                    outFile.WriteLine("            builder.HasMany(x => x.{0})", pluralize.Pluralize(r.ChildTable.UpperedFirstChar()));
                    outFile.WriteLine("                .WithOne(x => x.{0})", r.ParentTable.UpperedFirstChar());
                    outFile.WriteLine("                .HasForeignKey(x => x.{0}Id)", r.ParentTable.UpperedFirstChar());
                    outFile.WriteLine("                .OnDelete(DeleteBehavior.NoAction);");
                }
                
                if (r.RelationType == RelationType.OneToOne)
                {
                    outFile.WriteLine();
                    outFile.WriteLine("            builder.HasOne(x => x.{0})", r.ChildTable.UpperedFirstChar());
                    outFile.WriteLine("                .WithOne(x => x.{0})", r.ParentTable.UpperedFirstChar());
                    outFile.WriteLine("                .HasForeignKey<{0}>(x => x.{1}Id)", r.ChildTable.UpperedFirstChar(), r.ParentTable.UpperedFirstChar());
                    outFile.WriteLine("                .OnDelete(DeleteBehavior.NoAction);");
                }
            }

            outFile.WriteLine("        }");
            outFile.WriteLine("    }");
            outFile.WriteLine("}");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForContext(string path, DatabaseInfo db)
        {
            var ns = string.Format("{0}.DataAccess", db.ApplicationName);

            var textPath = string.Format(@"{0}\{1}Context.cs", path, db.ApplicationName);

            var outFile = File.CreateText(textPath);

            outFile.WriteLine("using Microsoft.EntityFrameworkCore;");
            outFile.WriteLine();
            outFile.WriteLine("namespace {0}", ns);
            outFile.WriteLine("{");
            outFile.WriteLine("    public class {0}Context : DbContext", db.ApplicationName);
            outFile.WriteLine("    {");

            var setCounter = 1;

            foreach (var table in db.Schemas
                .Select(x => x.Table)
                .Distinct())
            {
                outFile.WriteLine("        // {0}", setCounter);
                outFile.WriteLine("        public DbSet<{0}> {0} => Set<{0}>();", table);
                setCounter++;
            }

            outFile.WriteLine();
            outFile.WriteLine("        public BillingContext(DbContextOptions<BillingContext> options) : base(options) { }");

            outFile.WriteLine();
            outFile.WriteLine("        protected override void OnModelCreating(ModelBuilder modelBuilder)");
            outFile.WriteLine("        {");

            var builderCounter = 1;

            foreach (var table in db.Schemas
                .Select(x => x.Table)
                .Distinct())
            {
                outFile.WriteLine("            // {0}", builderCounter);
                outFile.WriteLine("            modelBuilder.ApplyConfiguration(new {0}Configuration());", table);
                builderCounter++;
            }

            outFile.WriteLine("        }");

            outFile.WriteLine("    }");
            outFile.WriteLine("}");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForPdf(string path, string tableName, string className, DatabaseInfo db)
        {
            var ns = string.Format("{0}.Models", db.ApplicationName);
            var textPath = string.Format(@"{0}\{1}.cs", path, className);

            var outFile = File.CreateText(textPath);

            outFile.WriteLine("namespace {0}", ns);
            outFile.WriteLine("{");
            outFile.WriteLine("    public class {0}", className);
            outFile.WriteLine("    {");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                outFile.WriteLine("                    if ({0}.{1} is not null)", tableName.LoweredFirstChar(), s.Column);
                outFile.WriteLine("                    {");
                outFile.WriteLine("                        column.Item().Row(row =>");
                outFile.WriteLine("                        {");
                outFile.WriteLine("                            row.RelativeItem().ValueCell().PaddingLeft(5).Text($\"{0}\").Bold();", s.Column);
                outFile.WriteLine("                            row.RelativeItem(2).ValueCell().PaddingLeft(5).Text($\"{{{0}.{1}}}\");", tableName.LoweredFirstChar(), s.Column);
                outFile.WriteLine("                        });");
                outFile.WriteLine("                    }");
            }

            outFile.WriteLine("    }");
            outFile.WriteLine("}");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }
    }
}
