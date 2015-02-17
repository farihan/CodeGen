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
                    db.Constraints = new List<Constraint>();
                    db.Constraints.AddRange(repo.GetConstraints("pk"));
                    db.Constraints.AddRange(repo.GetConstraints("fk"));
                    db.Relations = repo.GetRelations();
                    db.TableNames = repo.GetBaseTableNames();
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

                foreach (var tableName in db.TableNames.OrderBy(tableName => tableName))
                {
                    outFile.WriteLine("{0} {1}", DbLineType.Table, tableName.Replace(" ", "_"));

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
                                SetKey(s.Column, s.IsNullable, db.Constraints.Where(x => x.Table == tableName).ToList()));
                        }
                    }

                    foreach (var r in db.Relations.Where(p => p.ParentTable == tableName).OrderBy(p => p.ChildTable))
                    {
                        outFile.WriteLine("hasmany: {0} {1}", r.ChildTable.Replace(" ", "_"), r.PkConstraintColumn);
                    }

                    foreach (var r in db.Relations.Where(p => p.ChildTable == tableName).OrderBy(p => p.ParentTable))
                    {
                        if (r.ParentTable == tableName)
                            break;
                        outFile.WriteLine("belongto: {0} {1}", r.ParentTable.Replace(" ", "_"), r.FkConstraintColumn);
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

        private static string SetKey(string column, string nullable, List<Constraint> constraints)
        {
            if (constraints.FirstOrDefault(x => x.Column == column && x.ConstraintType == "PK") != null)
                return "PK";
            else if (constraints.FirstOrDefault(x => x.Column == column && x.ConstraintType == "FK") != null)
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
