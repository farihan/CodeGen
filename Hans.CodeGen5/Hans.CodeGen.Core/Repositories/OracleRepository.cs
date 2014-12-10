using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OracleClient;

namespace Hans.CodeGen.Core.DataProvider
{
    public class OracleRepository : IRepository
    {
        private string connectionString = string.Empty;
        private string owner = string.Empty;

        public OracleRepository(string connectionString, string owner)
        {
            this.connectionString = connectionString;
            this.owner = owner;
        }

        public List<Domains.Schema> GetSchemas()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                var sql = "select tc.table_name as table_name, " +
                          "tc.column_name as column_name, " +
                          "tc.data_type as data_type, " +
                          "tc.nullable as nullable, " +
                          "nvl(c.constraint_type,c.constraint_type) as constraint_type, " +
                          "data_length as data_length " +
                          "from all_tab_columns tc " +
                          "left outer join ( " +
                          "    all_cons_columns cc join all_constraints c on ( " +
                          "	   c.owner = cc.owner " +
                          "    and c.constraint_name = cc.constraint_name " +
                          "	   )) " +
                          "	   on (tc.owner = cc.owner and cc.owner = :OWNER " +
                          "       and tc.table_name = cc.table_name " +
                          "       and tc.column_name = cc.column_name) " +
                          "where tc.owner = :OWNER " +
                          "order by tc.table_name, cc.position nulls last, tc.column_id";

                var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.Add(new OracleParameter("OWNER", OracleType.VarChar)).Value = owner;

                conn.Open();

                var reader = cmd.ExecuteReader();

                var schemas = new List<Domains.Schema>();

                while (reader.Read())
                {
                    var s = new Domains.Schema();
                    s.Table = reader["table_name"].ToString();
                    s.Column = reader["column_name"].ToString();
                    s.DataType = reader["data_type"].ToString();
                    s.IsNullable = reader["nullable"].ToString();
                    s.MaxLength = reader["data_length"].ToString();
                    s.ConstraintType = reader["constraint_type"] == null ? string.Empty : reader["constraint_type"].ToString();

                    schemas.Add(s);
                }

                return schemas;
            }
        }

        public List<Domains.Schema> GetSchemasBy(string tableName)
        {
            throw new NotImplementedException();
        }

        public List<Domains.Schema> GetSchemasForBaseTable()
        {
            throw new NotImplementedException();
        }

        public List<Domains.Schema> GetSchemasForBaseTableBy(string tableName)
        {
            throw new NotImplementedException();
        }

        public List<Domains.Constraint> GetConstraints(string constraintType)
        {
            using (var conn = new OracleConnection(connectionString))
            {
                var sql = "select cc_r.owner as owner, cc_r.table_name as table_name, " +
                          "cc_r.column_name as column_name, " +
                          "cc_r.constraint_name as constraint_name, " +
                          "c.constraint_type as constraint_type " +
                          "from all_constraints c, " +
                          "all_cons_columns cc, " +
                          "all_cons_columns cc_r " +
                          "where c.owner = :OWNER " +
                          "and cc.constraint_name = c.constraint_name " +
                          "and cc.table_name = c.table_name " +
                          "and cc_r.constraint_name = c.r_constraint_name";

                var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.Add(new OracleParameter("OWNER", OracleType.VarChar)).Value = owner;

                conn.Open();

                var reader = cmd.ExecuteReader();

                var constraints = new List<Domains.Constraint>();

                while (reader.Read())
                {
                    var c = new Domains.Constraint();
                    c.Table = reader["table_name"].ToString();
                    c.Column = reader["column_name"].ToString();
                    c.Name = reader["constraint_name"].ToString();

                    constraints.Add(c);
                }

                return constraints;
            }
        }

        public List<Domains.Relation> GetRelations()
        {
            using (var conn = new OracleConnection(connectionString))
            {
                var sql = "select distinct cc_r.owner as r_owner, cc_r.table_name as r_table_name, " +
                          "cc_r.column_name as r_column_name, " +
                          "cc_r.constraint_name as r_constraint_name, " +
                          "c.owner, c.constraint_name, cc.table_name, cc.column_name, c.delete_rule " +
                          "from all_constraints c, " +
                          "all_cons_columns cc, " +
                          "all_cons_columns cc_r " +
                          "where c.owner = :OWNER " + 
                          "and cc.constraint_name = c.constraint_name " +
                          "and cc.table_name = c.table_name " +
                          "and cc_r.constraint_name = c.r_constraint_name";

                var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.Add(new OracleParameter("OWNER", OracleType.VarChar)).Value = owner;

                conn.Open();

                var reader = cmd.ExecuteReader();

                var relations = new List<Domains.Relation>();

                while (reader.Read())
                {
                    var r = new Domains.Relation();
                    r.ChildTable = reader["table_name"].ToString();
                    r.Constraint = reader["constraint_name"].ToString();
                    r.FkConstraintColumn = reader["column_name"].ToString();
                    r.ParentTable = reader["r_table_name"].ToString();
                    r.PkConstraintColumn = reader["r_column_name"].ToString();
                    r.DeleteRule = reader["delete_rule"].ToString();

                    if (relations.FirstOrDefault(x => x.Constraint == r.Constraint) == null)
                        relations.Add(r);
                }

                return relations;
            }
        }
    }
}
