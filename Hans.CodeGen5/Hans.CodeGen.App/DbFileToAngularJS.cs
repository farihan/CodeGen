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
    public class DbFileToAngularJS
    {
        public static void Generate(DatabaseInfo db)
        {
            var path = string.Format(@"{0}\{1}", db.OutputDirectory, CreationType.Angular);

            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("Output Directory does not exist");
            }

            Folder.Create(path);

            foreach (var tableName in db.Schemas
                .Select(x => x.Table)
                .Distinct())
            {
                var className = tableName.UpperedFirstChar();

                // create js
                //WriterForServiceJS(path, tableName, className, db);
                WriterForControllerJS(path, tableName, className, db);
            }

            Console.WriteLine();
        }

        private static void WriterForServiceJS(string path, string tableName, string className, DatabaseInfo db)
        {
            var specificPath = string.Format(@"{0}\{1}\{2}", path, CreationType.AngularJS, className.UpperedFirstChar());
            var textPath = string.Format(@"{0}\{1}{2}.js", specificPath, className.LoweredFirstChar(), CreationType.Service);

            var singularization = new Singularization();
            var pluralization = new Pluralization();

            var id = db.Schemas.Where(p => p.Table == tableName).FirstOrDefault().Column;
            var lastSchema = db.Schemas.Where(p => p.Table == tableName).Last();

            if (db.Schemas.Where(p => p.Table == tableName).FirstOrDefault(c => c.ColumnType == "PK") != null)
                id = db.Schemas.Where(p => p.Table == tableName).FirstOrDefault(c => c.ColumnType == "PK").Column;

            Folder.Create(specificPath);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("'use strict';");
            outFile.WriteLine("");
            outFile.WriteLine("angular.module('{0}')", singularization.Singularize(className));
            outFile.WriteLine("");
            outFile.WriteLine(".factory('{0}Service', ['$http',", className);
            outFile.WriteLine("    function ($http) {");
            outFile.WriteLine("        var service = {};");
            outFile.WriteLine("");
            outFile.WriteLine("        service.Get{0} = function (pagingInfo) {{", pluralization.Pluralize(className));
            outFile.WriteLine("            return $http.get('/api/{0}', {{ params: pagingInfo }});", singularization.Singularize(className.ToLower()));
            outFile.WriteLine("        };");
            outFile.WriteLine("");
            outFile.WriteLine("        return service;");
            outFile.WriteLine("    }]);");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForControllerJS(string path, string tableName, string className, DatabaseInfo db)
        {
            var specificPath = string.Format(@"{0}\{1}\{2}", path, CreationType.AngularJS, className.UpperedFirstChar());
            var textPath = string.Format(@"{0}\{1}{2}.js", specificPath, className.LoweredFirstChar(), CreationType.Controller);

            var singularization = new Singularization();
            var pluralization = new Pluralization();

            var id = db.Schemas.Where(p => p.Table == tableName).FirstOrDefault().Column;
            var lastSchema = db.Schemas.Where(p => p.Table == tableName).Last();

            if (db.Schemas.Where(p => p.Table == tableName).FirstOrDefault(c => c.ColumnType == "PK") != null)
                id = db.Schemas.Where(p => p.Table == tableName).FirstOrDefault(c => c.ColumnType == "PK").Column;

            Folder.Create(specificPath);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("'use strict';");
            outFile.WriteLine("angular.module('{0}', ['ui.bootstrap']);", db.ApplicationName);
            outFile.WriteLine("");
            outFile.WriteLine("angular.module('{0}').controller('{1}Controller', function ($scope, $http) {{", db.ApplicationName, className);
            outFile.WriteLine("    $scope.pagingInfo = {");
            outFile.WriteLine("        page: 1,");
            outFile.WriteLine("        pageSize: 10,");
            outFile.WriteLine("        sortBy: '{0}',", id);
            outFile.WriteLine("        isAsc: true,");
            outFile.WriteLine("        totalItems: 0");
            outFile.WriteLine("    };");
            outFile.WriteLine("    $scope.setPage = function (pageNo) {");
            outFile.WriteLine("        $scope.pagingInfo.page = pageNo;");
            outFile.WriteLine("    };");
            outFile.WriteLine("    $scope.pageChanged = function () {");
            outFile.WriteLine("        pageInit();");
            outFile.WriteLine("    };");
            outFile.WriteLine("");
            outFile.WriteLine("    $scope.sort = function (sortBy) {");
            outFile.WriteLine("        if (sortBy === $scope.pagingInfo.sortBy) {");
            outFile.WriteLine("            $scope.pagingInfo.isAsc = !$scope.pagingInfo.isAsc;");
            outFile.WriteLine("        } else {");
            outFile.WriteLine("            $scope.pagingInfo.sortBy = sortBy;");
            outFile.WriteLine("            $scope.pagingInfo.isAsc = false;");
            outFile.WriteLine("        }");
            outFile.WriteLine("        pageInit();");
            outFile.WriteLine("    };");
            outFile.WriteLine("");
            outFile.WriteLine("    function loadTotalItems() {");
            outFile.WriteLine("        $http.get('/api/{0}/getsize').success(function (data) {{", className.ToLower());
            outFile.WriteLine("            $scope.pagingInfo.totalItems = data;");
            outFile.WriteLine("        })");
            outFile.WriteLine("        .error(function () {");
            outFile.WriteLine("            $scope.error = \"Error has occured!\";");
            outFile.WriteLine("        });");
            outFile.WriteLine("    }");
            outFile.WriteLine("");
            outFile.WriteLine("    function load{0}() {{", pluralization.Pluralize(className));
            outFile.WriteLine("        $scope.{0} = null;", pluralization.Pluralize(className.ToLower()));
            outFile.WriteLine("        $http.get('/api/{0}/getallby', {{ params: $scope.pagingInfo }}).success(function (data) {{", className.ToLower());
            outFile.WriteLine("            $scope.{0} = data;", pluralization.Pluralize(className.ToLower()));
            outFile.WriteLine("        })");
            outFile.WriteLine("        .error(function () {");
            outFile.WriteLine("            $scope.error = \"Error has occured!\";");
            outFile.WriteLine("        });");
            outFile.WriteLine("    }");    
            outFile.WriteLine("");
            outFile.WriteLine("    function pageInit() {");
            outFile.WriteLine("        loadTotalItems();");
            outFile.WriteLine("        load{0}() {{", pluralization.Pluralize(className));
            outFile.WriteLine("    }");
            outFile.WriteLine("");
            outFile.WriteLine("    pageInit();");
            outFile.WriteLine("});");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }
    }
}
