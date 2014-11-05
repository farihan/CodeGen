using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hans.CodeGen.Core.Domains;
using Hans.CodeGen.Core.Commons;
using Hans.CodeGen.Core.Utils;
using System.Configuration;
using Hans.CodeGen.Core.DataProvider;

namespace Hans.CodeGen.App
{
    public static class DbToDbFile
    {
        public static void Generate(DatabaseInfo db)
        {
            if (db.ReadFromDb.ToLower() == SelectType.True)
            {
                if (db.DatabaseType.ToLower() == DbType.MsSql)
                {
                    var repo = new MsSqlRepository(ConfigurationManager.ConnectionStrings["0"].ConnectionString);

                    db.Schemas = repo.GetSchemas();
                    db.Constraints = repo.GetConstraints();
                    db.Relations = repo.GetRelations();
                }
                else if (db.DatabaseType.ToLower() == DbType.Oracle)
                {
                    //var repo = new OracleRepository(ConfigurationManager.ConnectionStrings["0"].ConnectionString, db.Owner);

                    //db.Schemas = repo.GetSchemas();
                    //db.Constraints = repo.GetConstraints();
                    //db.Relations = repo.GetRelations();
                }

                var value = string.Empty;
                var fileName = string.Format(@"{0}\{1}", db.OutputDirectory, db.InputAndOutputDbFile);
                Folder.Create(db.OutputDirectory);
                var outFile = System.IO.File.CreateText(fileName);
                var list = db.Schemas.Select(x => x.Table)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                foreach (var tableName in list)
                {
                    outFile.WriteLine("{0} {1}", DbLineType.Table, tableName);

                    if (db.DatabaseType.ToLower() == DbType.Oracle)
                    {
                        foreach (var s in db.Schemas.Where(p => p.Table == tableName))
                        {
                            outFile.WriteLine("{0} {1} {2}{3}",
                                DbLineType.Column,
                                s.Column,
                                s.DataType,
                                SetNullable(s.IsNullable));
                        }
                    }
                    else if (db.DatabaseType.ToLower() == DbType.MsSql)
                    {
                        foreach (var s in db.Schemas.Where(p => p.Table == tableName))
                        {
                            outFile.WriteLine("{0} {1} {2}{3}{4} {5}",
                                DbLineType.Column,
                                s.Column,
                                s.DataType,
                                string.IsNullOrEmpty(s.MaxLength) ? "" : " " + s.MaxLength,
                                SetNullable(s.IsNullable),
                                SetPk(s.Column, s.IsNullable, db.Relations.Where(x => x.ChildTable == tableName).ToList()));
                        }
                    }

                    foreach (var r in db.Relations.Where(p => p.ParentTable == tableName).OrderBy(p => p.ChildTable))
                    {
                        outFile.WriteLine("hasmany: {0} {1}", r.ChildTable, r.PkConstraintColumn);
                    }

                    foreach (var r in db.Relations.Where(p => p.ChildTable == tableName).OrderBy(p => p.ParentTable))
                    {
                        if (r.ParentTable == tableName)
                            break;
                        outFile.WriteLine("belongto: {0} {1}", r.ParentTable, r.FkConstraintColumn);
                    }

                    outFile.WriteLine("-");
                }

                outFile.Close();

                value = string.Format("\n{0} created", fileName);

                Console.Write(value);
            }
            Console.WriteLine();
        }

        private static string SetNullable(string nullable)
        {
            if (nullable == "NO")
                return " NOT_NULL";
            else
                return " NULL";
        }
        private static string SetPk(string column, string nullable, List<Relation> relations)
        {
            if (column.ToUpper().Contains("ID") && nullable == "NO" && (relations.FirstOrDefault(x => x.FkConstraintColumn == column) == null))
                return "PK";
            else if (relations.FirstOrDefault(x => x.FkConstraintColumn == column) != null)
                return "FK";
            else
                return "NONE";
        }

        private static string SetConstraint(string constraintType)
        {
            if (!string.IsNullOrEmpty(constraintType))
                return string.Format(" {0}", constraintType);
            else
                return " NULL";
        }

        private static string SetConstraint(List<Constraint> constraints, string tableName, string columnName)
        {
            var list = constraints.Where(x => x.Table.Equals(tableName) &&
                x.Column.Equals(columnName));

            if (list.Count() > 0)
            {
                var counter = 0;
                var s = string.Empty;
                foreach (var constraint in list)
                {
                    counter++;

                    if (counter == list.Count())
                        s += string.Format("{0}", ProcessConstraint(constraint.Name));
                    else
                        s += string.Format("{0},", ProcessConstraint(constraint.Name));
                }

                return string.Format(" {0}", s);
            }
            else
            {
                return " NULL";
            }
        }

        private static string ProcessConstraint(string constraintName)
        {
            var value = string.Empty;

            if (constraintName.ToUpper().StartsWith("PK_"))
                value = "PK";
            else if (constraintName.ToUpper().StartsWith("FK_"))
                value = "FK";
            else if (constraintName.ToUpper().StartsWith("IX_"))
                value = "IX";
            else
                value = "NULL";

            return value;
        }
    }
}
