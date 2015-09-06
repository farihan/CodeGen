using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using Hans.CodeGen.Core.Domains;

namespace Hans.CodeGen.Core.DataProvider
{
    public class MsSqlRepository : IRepository
    {
        private string connectionString = string.Empty;

        public MsSqlRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<Schema> GetSchemas()
        {
            using (var conn = new SqlConnection(connectionString))
            {
                var sql = "SELECT * from INFORMATION_SCHEMA.COLUMNS";
//                var sql = @"SELECT * FROM INFORMATION_SCHEMA.COLUMNS AS tab1 
//                            INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS as tab2
//                            ON tab1.[TABLE_NAME] = tab2.[TABLE_NAME];";
                var cmd = new SqlCommand(sql, conn);
                cmd.CommandType = CommandType.Text;

                conn.Open();

                IDataReader reader = cmd.ExecuteReader();

                var schemas = new List<Schema>();

                while (reader.Read())
                {
                    var s = new Schema();
                    s.Table = reader["table_name"].ToString();
                    s.Column = reader["column_name"].ToString();
                    s.DataType = reader["data_type"].ToString();
                    s.MaxLength = string.IsNullOrEmpty(reader["character_maximum_length"].ToString()) ? "" : reader["character_maximum_length"].ToString();
                    s.IsNullable = reader["is_nullable"].ToString();
                    //s.ConstraintType = reader["constraint_type"].ToString();
                    schemas.Add(s);
                }

                return schemas;
            }
        }

        public List<Schema> GetSchemasBy(string tableName)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                var sql = "SELECT * from INFORMATION_SCHEMA.COLUMNS WHERE [TABLE_NAME] = @Table_Name";

                var cmd = new SqlCommand(sql, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@Table_Name", SqlDbType.NVarChar, 255).Value = tableName;

                conn.Open();

                IDataReader reader = cmd.ExecuteReader();

                var schemas = new List<Schema>();

                while (reader.Read())
                {
                    var s = new Schema();
                    s.Table = reader["table_name"].ToString();
                    s.Column = reader["column_name"].ToString();
                    s.DataType = reader["data_type"].ToString();
                    s.MaxLength = string.IsNullOrEmpty(reader["character_maximum_length"].ToString()) ? "" : reader["character_maximum_length"].ToString();
                    s.IsNullable = reader["is_nullable"].ToString();

                    schemas.Add(s);
                }

                return schemas;
            }
        }

        public List<Schema> GetSchemasForBaseTable()
        {
            using (var conn = new SqlConnection(connectionString))
            {
                var sql = @"SELECT c.*
                            FROM INFORMATION_SCHEMA.COLUMNS c JOIN INFORMATION_SCHEMA.TABLES t
                            ON c.TABLE_NAME = t.TABLE_NAME
                            WHERE t.TABLE_TYPE = 'base table'";

                var cmd = new SqlCommand(sql, conn);
                cmd.CommandType = CommandType.Text;

                conn.Open();

                IDataReader reader = cmd.ExecuteReader();

                var schemas = new List<Schema>();

                while (reader.Read())
                {
                    var s = new Schema();
                    s.Table = reader["table_name"].ToString();
                    s.Column = reader["column_name"].ToString();
                    s.DataType = reader["data_type"].ToString();
                    s.MaxLength = string.IsNullOrEmpty(reader["character_maximum_length"].ToString()) ? "" : reader["character_maximum_length"].ToString();
                    s.IsNullable = reader["is_nullable"].ToString();

                    schemas.Add(s);
                }

                return schemas;
            }
        }

        public List<Schema> GetSchemasForBaseTableBy(string tableName)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                var sql = @"SELECT c.*
                            FROM INFORMATION_SCHEMA.COLUMNS c JOIN INFORMATION_SCHEMA.TABLES t
                            ON c.TABLE_NAME = t.TABLE_NAME
                            WHERE t.TABLE_TYPE = 'base table' AND t.TABLE_NAME = @Table_Name";

                var cmd = new SqlCommand(sql, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@Table_Name", SqlDbType.NVarChar, 255).Value = tableName;

                conn.Open();

                IDataReader reader = cmd.ExecuteReader();

                var schemas = new List<Schema>();

                while (reader.Read())
                {
                    var s = new Schema();
                    s.Table = reader["table_name"].ToString();
                    s.Column = reader["column_name"].ToString();
                    s.DataType = reader["data_type"].ToString();
                    s.MaxLength = string.IsNullOrEmpty(reader["character_maximum_length"].ToString()) ? "" : reader["character_maximum_length"].ToString();
                    s.IsNullable = reader["is_nullable"].ToString();

                    schemas.Add(s);
                }

                return schemas;
            }
        }

        public List<Domains.Constraint> GetConstraints(string constraintType)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                var sql = string.Empty;

                if (constraintType.ToLower() == "pk")
                {
                    sql = @"SELECT *
                            FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                            WHERE CONSTRAINT_NAME LIKE 'PK_%';";
                }

                if (constraintType.ToLower() == "fk")
                {
                    sql = @"SELECT *
                            FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                            WHERE CONSTRAINT_NAME LIKE 'FK_%';";
                }

                var cmd = new SqlCommand(sql, conn);
                cmd.CommandType = CommandType.Text;

                conn.Open();

                IDataReader reader = cmd.ExecuteReader();

                var constraints = new List<Domains.Constraint>();

                while (reader.Read())
                {
                    var c = new Domains.Constraint();
                    if (constraintType.ToLower() == "pk")
                    {
                        c.Table = reader["table_name"].ToString();
                        c.Column = reader["column_name"].ToString();
                        c.Name = reader["constraint_name"].ToString();
                        c.ConstraintType = constraintType.ToUpper();
                    }

                    if (constraintType.ToLower() == "fk")
                    {
                        c.Table = reader["table_name"].ToString();
                        c.Column = reader["column_name"].ToString();
                        c.Name = reader["constraint_name"].ToString();
                        c.ConstraintType = constraintType.ToUpper();
                    }
                    
                    constraints.Add(c);
                }

                return constraints;
            }
        }

        public List<Relation> GetRelations()
        {
            using (var conn = new SqlConnection(connectionString))
            {
                var sql = @"SELECT
                            tc.TABLE_SCHEMA,
                            tc.TABLE_NAME AS CHILD_TABLE_NAME,
                            rc.CONSTRAINT_NAME,
                            rc.DELETE_RULE,
                            STUFF(
                            (
                            SELECT
                            ',' + kcu.COLUMN_NAME
                            FROM
                            INFORMATION_SCHEMA.KEY_COLUMN_USAGE  AS kcu
                            WHERE
                            kcu.TABLE_CATALOG = tc.TABLE_CATALOG
                            AND kcu.TABLE_SCHEMA = tc.TABLE_SCHEMA
                            AND kcu.TABLE_NAME = tc.TABLE_NAME
                            AND kcu.CONSTRAINT_CATALOG = tc.CONSTRAINT_CATALOG
                            AND kcu.CONSTRAINT_SCHEMA = tc.CONSTRAINT_SCHEMA
                            AND kcu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
                            ORDER BY
                            kcu.ORDINAL_POSITION
                            FOR XML PATH('')
                            ), 1, 1, '') AS [FK_CONSTRAINT_COLUMNS],
                            --pc.TABLE_SCHEMA AS PARENT_TABLE_SCHEMA,
                            pc.TABLE_NAME AS PARENT_TABLE_NAME,
                            rc.UNIQUE_CONSTRAINT_NAME,
                            STUFF(
                            (
                            SELECT
                            ',' + kcu.COLUMN_NAME
                            FROM
                            INFORMATION_SCHEMA.KEY_COLUMN_USAGE  AS kcu
                            WHERE
                            kcu.TABLE_CATALOG = pc.TABLE_CATALOG
                            AND kcu.TABLE_SCHEMA = pc.TABLE_SCHEMA
                            AND kcu.TABLE_NAME = pc.TABLE_NAME
                            AND kcu.CONSTRAINT_CATALOG = pc.CONSTRAINT_CATALOG
                            AND kcu.CONSTRAINT_SCHEMA = pc.CONSTRAINT_SCHEMA
                            AND kcu.CONSTRAINT_NAME = pc.CONSTRAINT_NAME
                            ORDER BY
                            kcu.ORDINAL_POSITION
                            FOR XML PATH('')
                            ), 1, 1, '') AS [PK_UQ_CONSTRAINT_COLUMNS]
                            FROM
                            INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS  AS rc
                            INNER JOIN
                            INFORMATION_SCHEMA.CONSTRAINT_TABLE_USAGE AS tc
                            ON rc.CONSTRAINT_CATALOG = tc.CONSTRAINT_CATALOG
                            AND rc.CONSTRAINT_SCHEMA = tc.CONSTRAINT_SCHEMA
                            AND rc.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
                            INNER JOIN
                            INFORMATION_SCHEMA.CONSTRAINT_TABLE_USAGE AS pc
                            ON pc.CONSTRAINT_CATALOG = rc.UNIQUE_CONSTRAINT_CATALOG
                            AND pc.CONSTRAINT_SCHEMA = rc.UNIQUE_CONSTRAINT_SCHEMA
                            AND pc.CONSTRAINT_NAME = rc.UNIQUE_CONSTRAINT_NAME";

                var cmd = new SqlCommand(sql, conn);
                cmd.CommandType = CommandType.Text;

                conn.Open();

                IDataReader reader = cmd.ExecuteReader();

                var relations = new List<Relation>();

                while (reader.Read())
                {
                    var r = new Relation();
                    r.ChildTable = reader["child_table_name"].ToString();
                    r.Constraint = reader["constraint_name"].ToString();
                    r.FkConstraintColumn = reader["fk_constraint_columns"].ToString();
                    r.ParentTable = reader["parent_table_name"].ToString();
                    r.PkConstraintColumn = reader["pk_uq_constraint_columns"].ToString();
                    r.DeleteRule = reader["delete_rule"].ToString();

                    if (relations.FirstOrDefault(x => x.Constraint == r.Constraint) == null)
                        relations.Add(r);
                }

                return relations;
            }
        }

        public List<string> GetBaseTableNames()
        {
            using (var conn = new SqlConnection(connectionString))
            {
                var sql = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
                var cmd = new SqlCommand(sql, conn);
                cmd.CommandType = CommandType.Text;

                conn.Open();

                IDataReader reader = cmd.ExecuteReader();

                var list = new List<string>();

                while (reader.Read())
                {
                    var s = reader["table_name"].ToString();
                    list.Add(s);
                }

                return list;
            }
        }
    }
}
