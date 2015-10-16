using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hans.CodeGen.Core.Domains;
using Hans.CodeGen.Core.Commons;
using System.IO;
using Hans.CodeGen.Core.Utils;

namespace Hans.CodeGen.App
{
    public static class DbFile
    {
        public static void Read(DatabaseInfo db)
        {
            var schemas = new List<Schema>();
            var relations = new List<Relation>();
            var constraints = new List<Constraint>();

            var textPath = string.Format(@"{0}\{1}", db.OutputDirectory, db.InputAndOutputDbFile);
            var textReader = File.ReadAllText(textPath);
            var tables = textReader.Split(new string[] { "-" }, StringSplitOptions.None);

            foreach (var t in tables)
            {
                var lines = t.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                var tableName = string.Empty;

                tableName = GetTableName(lines, tableName);

                GetRelationsAndConstraints(schemas, relations, constraints, lines, tableName);
            }
            db.Schemas = schemas;
            db.Relations = relations;
            db.Constraints = constraints;
        }

        private static string GetTableName(string[] lines, string tableName)
        {
            var singularization = new Singularization();

            foreach (var line in lines)
            {
                var words = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (line.Contains(DbLineType.Table))
                {
                    if (words.Count() > 1)
                    {
                        //tableName = singularization.Singularize(words[1].ToString());
                        tableName = words[1].ToString();
                        break;
                    }
                }
            }
            return tableName;
        }

        private static void GetRelationsAndConstraints(List<Schema> schemas, List<Relation> relations, List<Constraint> constraints, string[] lines, string tableName)
        {
            foreach (var line in lines)
            {
                var schema = new Schema();
                var relation = new Relation();
                var constraint = new Constraint();
                var words = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                schema.Table = tableName;
                constraint.Table = tableName;

                if (line.Contains(DbLineType.Column))
                {
                    var wordCount = words.Count();

                    if (wordCount == 5)
                    {
                        schema.Column = words[1].ToString();
                        schema.DataType = words[2].ToString();
                        schema.IsNullable = words[3].ToString();
                        schema.MaxLength = string.Empty;
                        schema.ColumnType = words[4].ToString();

                        constraint.Column = words[1].ToString();
                        constraint.Name = string.Empty;
                    }

                    if (wordCount == 6)
                    {
                        schema.Column = words[1].ToString();
                        schema.DataType = words[2].ToString();
                        schema.IsNullable = words[3].ToString();
                        schema.MaxLength = words[4].ToString();
                        schema.ColumnType = words[5].ToString();

                        constraint.Column = words[1].ToString();
                        constraint.Name = string.Empty;
                    }
                }

                if (line.Contains(DbLineType.HasMany))
                {
                    relation.ChildTable = words[1].ToString();
                    relation.ParentTable = tableName;
                    relation.FkConstraintColumn = string.Format("{0}", words[2].ToString());
                    relation.PkConstraintColumn = string.Format("{0}", words[2].ToString());
                }

                if (line.Contains(DbLineType.BelongTo))
                {
                    relation.ChildTable = tableName;
                    relation.ParentTable = words[1].ToString();
                    relation.FkConstraintColumn = words[2].ToString();
                    relation.PkConstraintColumn = words[2].ToString();
                }

                if (!string.IsNullOrEmpty(schema.Table) &&
                    !string.IsNullOrEmpty(schema.Column))
                {
                    if (schemas.FirstOrDefault(x => x.Table == schema.Table && x.Column == schema.Column) == null)
                        schemas.Add(schema);

                    constraints.Add(constraint);
                }

                if (!string.IsNullOrEmpty(relation.ChildTable) &&
                    !string.IsNullOrEmpty(relation.ParentTable) &&
                    relations.FirstOrDefault(x => x.ChildTable.Contains(relation.ChildTable) &&
                        x.ParentTable.Contains(relation.ParentTable)) == null)
                {
                    relations.Add(relation);
                }
            }
        }
    }
}
