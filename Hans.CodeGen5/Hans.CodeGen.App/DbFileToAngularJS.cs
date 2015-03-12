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
            var path = string.Format(@"{0}\{1}", db.OutputDirectory, DirectoryType.AngularScripts);

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

                WriterForControllerJS(path, tableName, className, db);
            }

            WriterForAppJS(path, db);

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
            var specificPath = path;
            var textPath = string.Format(@"{0}\{1}{2}.js", specificPath, className.LoweredFirstChar(), CreationType.Controller);

            var singularization = new Singularization();
            var pluralization = new Pluralization();

            var id = db.Schemas.Where(p => p.Table == tableName).FirstOrDefault().Column;
            var lastSchema = db.Schemas.Where(p => p.Table == tableName).Last();

            if (db.Schemas.Where(p => p.Table == tableName).FirstOrDefault(c => c.ColumnType == "PK") != null)
                id = db.Schemas.Where(p => p.Table == tableName).FirstOrDefault(c => c.ColumnType == "PK").Column;

            Folder.Create(specificPath);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("//'use strict';");
            outFile.WriteLine("");
            outFile.WriteLine("app.controller('{0}Controller', function ($scope, $http, $modal, toaster) {{", className);
            outFile.WriteLine("    $scope.pagingInfo = {");
            outFile.WriteLine("        page: 1,");
            outFile.WriteLine("        pageSize: 10,");
            outFile.WriteLine("        sortBy: '{0}',", id);
            outFile.WriteLine("        isAsc: true,");
            outFile.WriteLine("        totalItems: 0");
            outFile.WriteLine("    };");
            outFile.WriteLine("");
            outFile.WriteLine("    $scope.{0} = null;", pluralization.Pluralize(className.ToLower()));
            outFile.WriteLine("");
            outFile.WriteLine("    $scope.gridIndex = function (index) {");
            outFile.WriteLine("        return (index + 1) + (($scope.pagingInfo.page - 1) * $scope.pagingInfo.pageSize);");
            outFile.WriteLine("    };");
            outFile.WriteLine("");
            outFile.WriteLine("    $scope.setPage = function (pageNo) {");
            outFile.WriteLine("        $scope.pagingInfo.page = pageNo;");
            outFile.WriteLine("    };");
            outFile.WriteLine("");
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
            outFile.WriteLine("    $scope.openModal = function (id, html) {");
            outFile.WriteLine("        var modalInstance = $modal.open({");
            outFile.WriteLine("            templateUrl: html,");
            outFile.WriteLine("            controller: 'ModalController',");
            outFile.WriteLine("            size: '',");
            outFile.WriteLine("            resolve: {");
            outFile.WriteLine("                selectedID: function () {");
            outFile.WriteLine("                    return id;");
            outFile.WriteLine("                },");
            outFile.WriteLine("                selectedPagingInfo: function () {");
            outFile.WriteLine("                    return $scope.pagingInfo;");
            outFile.WriteLine("                }");
            outFile.WriteLine("            }");
            outFile.WriteLine("        });");
            outFile.WriteLine("");
            outFile.WriteLine("        modalInstance.result.then(function (refreshProducts) {");
            outFile.WriteLine("            $scope.products = refreshProducts;");
            outFile.WriteLine("        }, function () {");
            outFile.WriteLine("            //$log.info('Modal dismissed at: ' + new Date());");
            outFile.WriteLine("        });");
            outFile.WriteLine("    };");
            outFile.WriteLine("");
            outFile.WriteLine("    function loadTotalItems() {");
            outFile.WriteLine("        $http.get('/api/product/getsize')");
            outFile.WriteLine("        .success(function (data, status, headers, config) {");
            outFile.WriteLine("            $scope.pagingInfo.totalItems = data;");
            outFile.WriteLine("        })");
            outFile.WriteLine("        .error(function (data, status, headers, config) {");
            outFile.WriteLine("            $scope.error = 'Error has occured!\';");
            outFile.WriteLine("            toaster.pop('error', $scope.error);");
            outFile.WriteLine("        });");
            outFile.WriteLine("    }");
            outFile.WriteLine("");
            outFile.WriteLine("    function loadProducts() {");
            outFile.WriteLine("        $http.get('/api/product/getallby', { params: $scope.pagingInfo })");
            outFile.WriteLine("        .success(function (data, status, headers, config) {");
            outFile.WriteLine("            $scope.products = data;");
            outFile.WriteLine("        })");
            outFile.WriteLine("        .error(function (data, status, headers, config) {");
            outFile.WriteLine("            $scope.error = 'Error has occured!';");
            outFile.WriteLine("            toaster.pop('error', $scope.error);");
            outFile.WriteLine("        });");
            outFile.WriteLine("    }  ");
            outFile.WriteLine("");
            outFile.WriteLine("    function pageInit() {");
            outFile.WriteLine("        loadTotalItems();");
            outFile.WriteLine("        loadProducts();");
            outFile.WriteLine("        toaster.pop('info', 'Load page ' + $scope.pagingInfo.page + ' and sort by ' + $scope.pagingInfo.sortBy);");
            outFile.WriteLine("    }");
            outFile.WriteLine("");
            outFile.WriteLine("    pageInit();");
            outFile.WriteLine("});");
            outFile.WriteLine("");
            outFile.WriteLine("app.controller('ModalController', function ($scope, $modalInstance, $http, selectedID, selectedPagingInfo, toaster) {{");
            outFile.WriteLine("    $scope.selectedID = selectedID;");
            outFile.WriteLine("    $scope.selectedPagingInfo = selectedPagingInfo;");
            outFile.WriteLine("");
            outFile.WriteLine("    $scope.ok = function () {");
            outFile.WriteLine("        closeAndRefreshRepeater();");
            outFile.WriteLine("    };");
            outFile.WriteLine("");
            outFile.WriteLine("    $scope.cancel = function () {");
            outFile.WriteLine("        closeAndRefreshRepeater();");
            outFile.WriteLine("    };");
            outFile.WriteLine("");
            outFile.WriteLine("    $scope.create = function ({0}) {{", className.ToLower());
            outFile.WriteLine("        if ($scope.createForm.$valid) {");
            outFile.WriteLine("            $scope.isProcessing = true;");
            outFile.WriteLine("            $http({");
            outFile.WriteLine("                method: 'POST',");
            outFile.WriteLine("                url: '/api/{0}/create',", className.ToLower());
            outFile.WriteLine("                data: {");
            
            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                if (s.ColumnType == ColumnType.None)
                {
                    outFile.WriteLine("                    '{0}': {1}.{0},", s.Column, className.ToLower());
                }
            }
            
            outFile.WriteLine("                },");
            outFile.WriteLine("                headers: { 'Content-Type': 'application/json' }");
            outFile.WriteLine("            })");
            outFile.WriteLine("            .success(function (data, status, headers, config) {");
            outFile.WriteLine("                $scope.isProcessing = false;");
            outFile.WriteLine("                toaster.pop('success', 'Create successful...');");
            outFile.WriteLine("                closeAndRefreshRepeater();");
            outFile.WriteLine("            })");
            outFile.WriteLine("            .error(function(data, status, headers, config) {");
            outFile.WriteLine("                $scope.error = 'Error has occured!';");
            outFile.WriteLine("                toaster.pop('error', $scope.error);");
            outFile.WriteLine("            });");
            outFile.WriteLine("        }");
            outFile.WriteLine("    };");
            outFile.WriteLine("");
            outFile.WriteLine("    $scope.update = function (product) {");
            outFile.WriteLine("        if ($scope.editForm.$valid) {");
            outFile.WriteLine("            $scope.isProcessing = true;");
            outFile.WriteLine("            $http({");
            outFile.WriteLine("                method: 'PUT',");
            outFile.WriteLine("                url: '/api/{0}/edit/' + {0}.{1} ,", className.ToLower(), id);
            outFile.WriteLine("                data: {");

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                outFile.WriteLine("                    '{0}': {1}.{0},", s.Column, className.ToLower());
            }

            outFile.WriteLine("                },");
            outFile.WriteLine("                headers: { 'Content-Type': 'application/json' }");
            outFile.WriteLine("            })");
            outFile.WriteLine("            .success(function (data, status, headers, config) {");
            outFile.WriteLine("                $scope.isProcessing = false;");
            outFile.WriteLine("                toaster.pop('success', 'Update successful...');");
            outFile.WriteLine("                closeAndRefreshRepeater();");
            outFile.WriteLine("            })");
            outFile.WriteLine("            .error(function(data, status, headers, config) {");
            outFile.WriteLine("                $scope.error = 'Error has occured!';");
            outFile.WriteLine("                toaster.pop('error', $scope.error);");
            outFile.WriteLine("            });");
            outFile.WriteLine("        }");
            outFile.WriteLine("    };");
            outFile.WriteLine("");
            outFile.WriteLine("    $scope.delete = function (id) {");
            outFile.WriteLine("        $scope.isProcessing = true;");
            outFile.WriteLine("        $http.delete('/api/{0}/delete', {{", className.ToLower());
            outFile.WriteLine("            params: { 'id': id }");
            outFile.WriteLine("        })");
            outFile.WriteLine("        .success(function (data, status, headers, config) {");
            outFile.WriteLine("            $scope.isProcessing = false;");
            outFile.WriteLine("            toaster.pop('success', 'Delete successful...');");
            outFile.WriteLine("            closeAndRefreshRepeater();");
            outFile.WriteLine("        })");
            outFile.WriteLine("        .error(function (data, status, headers, config) {");
            outFile.WriteLine("            $scope.error = 'Error has occured!';");
            outFile.WriteLine("            toaster.pop('error', $scope.error);");
            outFile.WriteLine("        });");
            outFile.WriteLine("    };");
            outFile.WriteLine("");

            foreach (var r in db.Relations.Where(p => p.ChildTable == tableName).OrderBy(p => p.ParentTable))
            {
                outFile.WriteLine("    function getAvailable{0}() {{", r.ParentTable.UpperedFirstChar());
                outFile.WriteLine("        $scope.available{0} = [];", r.ParentTable.UpperedFirstChar());
                outFile.WriteLine("        $http.get('/api/{0}/getall')", singularization.Singularize(r.ParentTable.ToLower()));
                outFile.WriteLine("        .success(function (data, status, headers, config) {");
                outFile.WriteLine("            $.each(data, function (index, item) {");
                outFile.WriteLine("                $scope.available{0}.push({{ ID: item.{1}, Name: item.{1} }});", 
                    r.ParentTable.UpperedFirstChar(), r.PkConstraintColumn);
                outFile.WriteLine("            });");
                outFile.WriteLine("        })");
                outFile.WriteLine("        .error(function (data, status, headers, config) {");
                outFile.WriteLine("            $scope.error = 'Error has occured!';");
                outFile.WriteLine("            toaster.pop('error', $scope.error);");
                outFile.WriteLine("        });");
                outFile.WriteLine("    }");
            }

            outFile.WriteLine("");
            outFile.WriteLine("    function load{0}() {{", className);
            outFile.WriteLine("        $scope.{0} = null;", className.ToLower());
            outFile.WriteLine("        $http.get('/api/{0}/get', {{ params: {{ id: $scope.selectedID }} }})", className.ToLower());
            outFile.WriteLine("        .success(function (data, status, headers, config) {");
            outFile.WriteLine("            $scope.{0} = data;", className.ToLower());
            outFile.WriteLine("        })");
            outFile.WriteLine("        .error(function (data, status, headers, config) {");
            outFile.WriteLine("            $scope.error = 'Error has occured!';");
            outFile.WriteLine("            toaster.pop('error', $scope.error);");
            outFile.WriteLine("        });");
            outFile.WriteLine("    }");
            outFile.WriteLine("");
            outFile.WriteLine("    function loadTotalItems() {");
            outFile.WriteLine("        $http.get('/api/{0}/getsize')", className.ToLower());
            outFile.WriteLine("        .success(function (data, status, headers, config) {");
            outFile.WriteLine("            $scope.selectedPagingInfo.totalItems = data;");
            outFile.WriteLine("        })");
            outFile.WriteLine("        .error(function (data, status, headers, config) {");
            outFile.WriteLine("            $scope.error = 'Error has occured!';");
            outFile.WriteLine("            toaster.pop('error', $scope.error);");
            outFile.WriteLine("        });");
            outFile.WriteLine("    }");
            outFile.WriteLine("");
            outFile.WriteLine("    function load{0}() {{", pluralization.Pluralize(className));
            outFile.WriteLine("        $http.get('/api/{0}/getallby', {{ params: $scope.selectedPagingInfo }})", className.ToLower());
            outFile.WriteLine("        .success(function (data, status, headers, config) {");
            outFile.WriteLine("            $scope.refresh{0} = data;", pluralization.Pluralize(className));
            outFile.WriteLine("            $modalInstance.close(data);");
            outFile.WriteLine("        })");
            outFile.WriteLine("        .error(function (data, status, headers, config) {");
            outFile.WriteLine("            $scope.error = 'Error has occured!';");
            outFile.WriteLine("            toaster.pop('error', $scope.error);");
            outFile.WriteLine("        });");
            outFile.WriteLine("    }");
            outFile.WriteLine("");
            outFile.WriteLine("    function closeAndRefreshRepeater() {");
            outFile.WriteLine("        toaster.pop('info', 'Unload modal...');");
            outFile.WriteLine("        loadTotalItems();");
            outFile.WriteLine("        load{0}();", pluralization.Pluralize(className));
            outFile.WriteLine("    }");
            outFile.WriteLine("");
            outFile.WriteLine("    function pageInit() {");
            outFile.WriteLine("        toaster.pop('info', 'Load modal...');");

            foreach (var r in db.Relations.Where(p => p.ChildTable == tableName).OrderBy(p => p.ParentTable))
            {
                outFile.WriteLine("        getAvailable{0}();", pluralization.Pluralize(r.ParentTable.UpperedFirstChar()));
            }

            outFile.WriteLine("        if ($scope.selectedID != '')");
            outFile.WriteLine("            load{0}();", className);
            outFile.WriteLine("    }");
            outFile.WriteLine("");
            outFile.WriteLine("    pageInit();");
            outFile.WriteLine("});");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForAppJS(string path, DatabaseInfo db)
        {
            var specificPath = path;
            var textPath = string.Format(@"{0}\app.js", specificPath);

            Folder.Create(specificPath);
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("//'use strict';");
            outFile.WriteLine("var app = angular.module('{0}', ['ui.bootstrap', 'toaster', 'angular-loading-bar']);", db.ApplicationName);

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }
    }
}
