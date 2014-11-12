using Hans.CodeGen.Core.Commons;
using Hans.CodeGen.Core.Domains;
using Hans.CodeGen.Core.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hans.CodeGen.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var db = Setup();

            Console.WriteLine("Code Generator");
            Console.WriteLine("==============");
            Console.Write("Namespace               : {0}\n", db.NamespaceCs);
            Console.Write("Database Type           : {0}\n", db.DatabaseType);
            Console.Write("Input Directory         : {0}\n", db.InputDirectory);
            Console.Write("Input and Output Db File: {0}\n", db.InputAndOutputDbFile);
            Console.Write("Input UI File           : {0}\n", db.InputUIFile);
            Console.Write("Output Directory        : {0}\n", db.OutputDirectory);
            Console.Write("Owner                   : {0}\n", db.Owner);
            Console.Write("Read From Db            : {0}\n", db.ReadFromDb);
            Console.Write("Read From Db File       : {0}\n", db.ReadFromDbFile);
            Console.Write("Database Context        : {0}\n", db.ApplicationName);

            Console.Write("\nPress any key to continue...");
            Console.ReadKey();

            using(var timer = new TimedLog("Generation time"))
            {
                DbToDbFile.Generate(db);

                if (db.ReadFromDbFile.ToLower() ==  SelectType.True)
                {
                    DbFile.Read(db);
                    CheckDatabaseInfo(db);
                    DbFileToRepository.Generate(db);
                    DbFileToDomain.Generate(db);
                    DbFileToModel.Generate(db);
                    DbFileToController.Generate(db);
                    DbFileToApiController.Generate(db);
                    DbFileToErrorHandling.Generate(db);
                    DbFileToUI.Generate(db);
                    DbFileToKnockoutJS.Generate(db);
                    DbFileToKnockoutUI.Generate(db);
                }
            }

            Console.WriteLine();
            Console.Write("\nPress any key to exit...");
            Console.ReadKey();
        }

        private static DatabaseInfo Setup()
        {
            var db = new DatabaseInfo();

            db.NamespaceCs = ConfigurationManager.AppSettings[KeyType.NamespaceCs].ToString();
            db.DatabaseType = ConfigurationManager.AppSettings[KeyType.DatabaseType].ToString();
            db.InputDirectory = ConfigurationManager.AppSettings[KeyType.InputDirectory].ToString();
            db.InputAndOutputDbFile = ConfigurationManager.AppSettings[KeyType.InputAndOutputDbFile].ToString();
            db.InputUIFile = ConfigurationManager.AppSettings[KeyType.InputUIFile].ToString();
            db.OutputDirectory = ConfigurationManager.AppSettings[KeyType.OutputDirectory].ToString();
            db.Owner = ConfigurationManager.AppSettings[KeyType.Owner].ToString();
            db.ReadFromDb = ConfigurationManager.AppSettings[KeyType.ReadFromDb].ToString();
            db.ReadFromDbFile = ConfigurationManager.AppSettings[KeyType.ReadFromDbFile].ToString();
            db.RemoveChars = ConfigurationManager.AppSettings[KeyType.RemoveChars].ToString();
            db.ApplicationName = ConfigurationManager.AppSettings[KeyType.ApplicationName].ToString();

            return db;
        }

        private static void CheckDatabaseInfo(DatabaseInfo db)
        {
            if (db.Schemas == null)
            {
                throw new Exception("Schemas are NULL");
            }
            else if (db.Relations == null)
            {
                throw new Exception("Relations are NULL");
            }
            else if (db.Constraints == null)
            {
                throw new Exception("Constraints are NULL");
            }
        }

        // [todo]
        // plan
        // code for controller ~ crud
        // code for ui ~ List/Create/Edit/Delete/Details
        // code for ui knockout ~ List/Crate/Edit/Delete/Details
        // references
        // 'getting started with ef 6 code first using mvc 5'
    }
}
