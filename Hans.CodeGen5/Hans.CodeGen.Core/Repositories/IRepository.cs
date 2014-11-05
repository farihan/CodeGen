using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hans.CodeGen.Core.Domains;

namespace Hans.CodeGen.Core.DataProvider
{
    public interface IRepository
    {
        List<Schema> GetSchemas();
        List<Schema> GetSchemasBy(string tableName);
        List<Schema> GetSchemasForBaseTable();
        List<Schema> GetSchemasForBaseTableBy(string tableName);
        List<Constraint> GetConstraints();
        List<Relation> GetRelations();
    }
}
