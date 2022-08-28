using Hans.CodeGen.Core.Commons;
using Hans.CodeGen.Core.Domains;
using Hans.CodeGen.Core.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hans.CodeGen.App
{
    class Program
    {
        static void Main(string[] args)
        {
            //var rand1 = Math.andom().toString(36).substr(2, 6).toUpperCase();
            //var rand2 = Math.random().toString(36).substr(2, 6).toUpperCase();
            //SubmissionNo = Random + Random2
            //L0G4EZ7U8QSE

            //var random = new Random();
            //var rand1 = random.Next().ToString().Substring(2, 6).ToUpper();
            //var rand2 = random.Next().ToString().Substring(2, 6).ToUpper();
            //var submissionNo1 = rand1 + rand2;

            //var rand3 = GenerateRandom(36).Substring(2, 6).ToUpper();
            //var rand4 = GenerateRandom(36).Substring(2, 6).ToUpper();
            //var submissionNo2 = rand3 + rand4;

            //Console.WriteLine(rand1);
            //Console.WriteLine(rand2);
            //Console.WriteLine(submissionNo1);

            //Console.WriteLine(rand3);
            //Console.WriteLine(rand4);
            //Console.WriteLine(submissionNo2);
            //Console.ReadKey();

            //var list = new List<int>() { 10, 11, 12, 13, 14, 15, 16 };
            //var check = 10;
            //var exit = true;
            //do
            //{
            //    // get submission number
            //    check++; 
            //    // check duplicate
            //    var exist = list.Any(x => x == check);
            //    // if not exist.. set exit to false and proceed to save
            //    if (!exist)
            //        exit = false;

            //    Console.WriteLine("submission number:" + check);
            //    Console.WriteLine("exist in db      :" + exist);
            //} while (exit);

            //Console.ReadKey();

            var db = Setup();
            Console.WriteLine(DateTime.Now.AddDays(30).ToString("d MMMM yyyy", new CultureInfo("ms-MY")));

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
            Console.Write("Remove Field(s)         : {0}\n", db.EnableAuditTrail);
            Console.Write("Remove Field(s)         : {0}\n", db);

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
                    // razor
                    DbFileToController.Generate(db);
                    DbFileToUI.Generate(db);
                    // ko
                    DbFileToKnockoutApi.Generate(db);
                    DbFileToKnockoutJS.Generate(db);
                    DbFileToKnockoutUI.Generate(db);
                    // angular
                    DbFileToAngularApi.Generate(db);
                    DbFileToAngularJS.Generate(db);
                    DbFileToAngularUI.Generate(db);
                    
                    DbFileToErrorHandling.Generate(db);
                    DbFileToGlobal.Generate(db);
                }
            }

            Console.WriteLine();
            Console.Write("\nPress any key to exit...");
            Console.ReadKey();
        }

        private static DatabaseInfo Setup()
        {
            var db = new DatabaseInfo();

            db.ApplicationName = ConfigurationManager.AppSettings[KeyType.ApplicationName].ToString();
            db.NamespaceCs = string.Format(ConfigurationManager.AppSettings[KeyType.NamespaceCs].ToString(), db.ApplicationName);
            db.DatabaseType = ConfigurationManager.AppSettings[KeyType.DatabaseType].ToString();
            db.InputDirectory = ConfigurationManager.AppSettings[KeyType.InputDirectory].ToString();
            db.InputAndOutputDbFile = string.Format(ConfigurationManager.AppSettings[KeyType.InputAndOutputDbFile].ToString(), db.ApplicationName);
            db.InputUIFile = string.Format(ConfigurationManager.AppSettings[KeyType.InputUIFile].ToString(), db.ApplicationName);
            db.OutputDirectory = ConfigurationManager.AppSettings[KeyType.OutputDirectory].ToString();
            db.Owner = ConfigurationManager.AppSettings[KeyType.Owner].ToString();
            db.ReadFromDb = ConfigurationManager.AppSettings[KeyType.ReadFromDb].ToString();
            db.ReadFromDbFile = ConfigurationManager.AppSettings[KeyType.ReadFromDbFile].ToString();
            db.RemoveChars = ConfigurationManager.AppSettings[KeyType.RemoveChars].ToString();
            db.RemoveFields = ConfigurationManager.AppSettings[KeyType.RemoveFields].ToString();
            db.EnableAuditTrail = ConfigurationManager.AppSettings[KeyType.EnableAuditTrail].ToString();

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

        private static string GenerateRandom(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
