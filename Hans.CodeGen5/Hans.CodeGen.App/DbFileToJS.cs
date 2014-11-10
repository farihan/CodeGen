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
    public class DbFileToJS
    {
        public static void Generate(DatabaseInfo db)
        {
            var path = string.Format(@"{0}\{1}", db.OutputDirectory, CreationType.Javascripts);

            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("Output Directory does not exist");
            }

            Folder.Create(path);

            //ajax.js
            WriterForAjax(path, db);

            foreach (var tableName in db.Schemas
                .Select(x => x.Table)
                .Distinct())
            {
                var className = tableName.UpperedFirstChar();
                //model.js
                WriterForModel(path, tableName, className, db);
                //viewmodel.js
                WriterForViewModel(path, tableName, className, db);
            }

            Console.WriteLine();
        }

        private static void WriterForAjax(string path, DatabaseInfo db)
        {
            var textPath = string.Format(@"{0}\{1}-{2}.js", path, db.ApplicationName.ToLower(), "ajax");

            var outFile = File.CreateText(textPath);

            outFile.WriteLine("var sendRequest = function (url, verb, data, successCallback, errorCallback, options) {");
            outFile.WriteLine("");
            outFile.WriteLine("    var requestOptions = options || {};");
            outFile.WriteLine("    requestOptions.type = verb;");
            outFile.WriteLine("    requestOptions.success = successCallback;");
            outFile.WriteLine("    requestOptions.error = errorCallback;");
            outFile.WriteLine("");
            outFile.WriteLine("    if (!url || !verb) {");
            outFile.WriteLine("        errorCallback(401, \"URL and HTTP verb required\");");
            outFile.WriteLine("    }");
            outFile.WriteLine("");
            outFile.WriteLine("    if (data) {");
            outFile.WriteLine("        requestOptions.data = data;");
            outFile.WriteLine("    }");
            outFile.WriteLine("    $.ajax(url, requestOptions);");
            outFile.WriteLine("}");
            outFile.WriteLine("");
            outFile.WriteLine("var setDefaultCallbacks = function (successCallback, errorCallback) {");
            outFile.WriteLine("    $.ajaxSetup({");
            outFile.WriteLine("        complete: function (jqXHR, status) {");
            outFile.WriteLine("            if (jqXHR.status >= 200 && jqXHR.status < 300) {");
            outFile.WriteLine("                successCallback(jqXHR.responseJSON);");
            outFile.WriteLine("            } else {");
            outFile.WriteLine("                errorCallback(jqXHR.status, jqXHR.statusText);");
            outFile.WriteLine("            }");
            outFile.WriteLine("        }");
            outFile.WriteLine("    });");
            outFile.WriteLine("}");
            outFile.WriteLine("");
            outFile.WriteLine("var setAjaxHeaders = function (requestHeaders) {");
            outFile.WriteLine("    $.ajaxSetup({ headers: requestHeaders });");
            outFile.WriteLine("}");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForModel(string path, string tableName, string className, DatabaseInfo db)
        {
            Folder.Create(string.Format(@"{0}\{1}", path, className.UpperedFirstChar()));

            var textPath = string.Format(@"{0}\{1}\{2}-model.js", path, className.UpperedFirstChar(), className.ToLower());
            var id = db.Schemas.Where(p => p.Table == tableName).FirstOrDefault().Column;

            var outFile = File.CreateText(textPath);
            outFile.WriteLine("ko.validation.rules.pattern.message = 'Invalid.';");
            outFile.WriteLine("");
            outFile.WriteLine("ko.validation.configure({");
            outFile.WriteLine("    registerExtenders: true,");
            outFile.WriteLine("    messagesOnModified: true,");
            outFile.WriteLine("    insertMessages: true,");
            outFile.WriteLine("    parseInputAttributes: true,");
            outFile.WriteLine("    messageTemplate: null,");
            outFile.WriteLine("    errorElementClass: 'has-error',");
            outFile.WriteLine("    errorMessageClass: 'help-block',");
            outFile.WriteLine("    decorateElement: true");
            outFile.WriteLine("});");
            outFile.WriteLine("");
            outFile.WriteLine("function {0}Model(data) {{", className.LoweredFirstChar());

            foreach (var s in db.Schemas.Where(p => p.Table == tableName))
            {
                var columnName = s.Column;
                //outFile.WriteLine("    this.{0} = ko.observable(data.{1});", columnName.LoweredFirstChar(), columnName);
                
                if (s.ColumnType == "FK")
                {
                    outFile.WriteLine("        this.{0} = ko.observable(data.{1}).extend({{ required: true }});", columnName.LoweredFirstChar(), columnName);
                }

                if (s.ColumnType == "NONE")
                {
                    if (s.MsSqlDataType() == "int")
                    {
                        outFile.WriteLine("        this.{0} = ko.observable(data.{1}).extend({{", columnName.LoweredFirstChar(), columnName);
                        outFile.WriteLine("            required: true, ");
                        outFile.WriteLine("            pattern: {");
                        outFile.WriteLine("                message: 'Enter numbers only',");
                        outFile.WriteLine("                params: '^[0-9]+$'");
                        outFile.WriteLine("            }");
                        outFile.WriteLine("        });");
                    }
                    else if (s.MsSqlDataType() == "double")//for money usage
                    {
                        outFile.WriteLine("        this.{0} = ko.observable(data.{1}).extend({{", columnName.LoweredFirstChar(), columnName);
                        outFile.WriteLine("            required: true, ");
                        outFile.WriteLine("            pattern: {");
                        outFile.WriteLine("                message: 'Enter numbers with two decimal points only',");
                        outFile.WriteLine("                params: '^[0-9]+(\\.[0-9][0-9]?)?$'");
                        outFile.WriteLine("            }");
                        outFile.WriteLine("        });");
                    }
                    else if (s.MsSqlDataType() == "decimal")
                    {
                        outFile.WriteLine("        this.{0} = ko.observable(data.{1}).extend({{", columnName.LoweredFirstChar(), columnName);
                        outFile.WriteLine("            required: true, ");
                        outFile.WriteLine("            pattern: {");
                        outFile.WriteLine("                message: 'Enter decimal numbers only',");
                        outFile.WriteLine("                params: '^[0-9]+(\\.[0-9]+)?$'");
                        outFile.WriteLine("            }");
                        outFile.WriteLine("        });");
                    }
                    else if (s.MsSqlDataType() == "float")
                    {
                        outFile.WriteLine("        this.{0} = ko.observable(data.{1}).extend({{", columnName.LoweredFirstChar(), columnName);
                        outFile.WriteLine("            required: true, ");
                        outFile.WriteLine("            pattern: {");
                        outFile.WriteLine("                message: 'Enter decimal numbers only',");
                        outFile.WriteLine("                params: '^[0-9]+(\\.[0-9]+)?$'");
                        outFile.WriteLine("            }");
                        outFile.WriteLine("        });");
                    }
                    else
                    {
                        outFile.WriteLine("        this.{0} = ko.observable(data.{1}).extend({{ required: true }});", columnName.LoweredFirstChar(), columnName);
                    }
                }
            }

            outFile.WriteLine("}");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }

        private static void WriterForViewModel(string path, string tableName, string className, DatabaseInfo db)
        {
            Folder.Create(string.Format(@"{0}\{1}", path, className.UpperedFirstChar()));

            var textPath = string.Format(@"{0}\{1}\{2}-viewmodel.js", path, className.UpperedFirstChar(), className.ToLower());
            var id = db.Schemas.Where(p => p.Table == tableName).FirstOrDefault().Column;
            var outFile = File.CreateText(textPath);

            outFile.WriteLine("var url = \"/api/{0}/\";", className.ToLower());            
            outFile.WriteLine("function {0}ViewModel() {{", className.LoweredFirstChar());
            outFile.WriteLine("    var self = this;");
            outFile.WriteLine("");
            outFile.WriteLine("    self.currentView = ko.observable(\"List\");");
            outFile.WriteLine("    self.listSize = ko.observable(0);");
            outFile.WriteLine("    self.filteredList = ko.observableArray();");
            outFile.WriteLine("    self.model = ko.observable();");
            outFile.WriteLine("    self.pageSize = ko.observable(10);");
            outFile.WriteLine("    self.pageIndex = ko.observable(0);");
            outFile.WriteLine("    self.availableSuppliers = ko.observableArray([]);");
            outFile.WriteLine("    self.availableCategories = ko.observableArray([]);");
            outFile.WriteLine("    self.pagedList = ko.dependentObservable(function () {");
            outFile.WriteLine("        return self.filteredList();");
            outFile.WriteLine("    });");
            outFile.WriteLine("    self.maxPageIndex = ko.dependentObservable(function () {");
            outFile.WriteLine("        return Math.ceil(self.listSize() / self.pageSize()) - 1;");
            outFile.WriteLine("    });");
            outFile.WriteLine("    self.previousPage = function () {");
            outFile.WriteLine("        if (self.pageIndex() > 0) {");
            outFile.WriteLine("            self.pageIndex(self.pageIndex() - 1);");
            outFile.WriteLine("            self.getFilteredList();");
            outFile.WriteLine("        }");
            outFile.WriteLine("    };");
            outFile.WriteLine("    self.nextPage = function () {");
            outFile.WriteLine("        if (self.pageIndex() < self.maxPageIndex()) {");
            outFile.WriteLine("            self.pageIndex(self.pageIndex() + 1);");
            outFile.WriteLine("            self.getFilteredList();");
            outFile.WriteLine("        }");
            outFile.WriteLine("    };");
            outFile.WriteLine("    self.allPages = ko.dependentObservable(function () {");
            outFile.WriteLine("        var pages = [];");
            outFile.WriteLine("        for (i = 0; i <= self.maxPageIndex() ; i++) {");
            outFile.WriteLine("            pages.push({ pageNumber: (i + 1) });");
            outFile.WriteLine("        }");
            outFile.WriteLine("        return pages;");
            outFile.WriteLine("    });");
            outFile.WriteLine("    self.moveToPage = function (index) {");
            outFile.WriteLine("        self.pageIndex(index);");
            outFile.WriteLine("        self.getFilteredList();");
            outFile.WriteLine("    };");
            outFile.WriteLine("    self.itemNumber = function (index) {");
            outFile.WriteLine("        return index + 1 + (self.pageSize() * self.pageIndex());");
            outFile.WriteLine("    };");
            outFile.WriteLine("    self.getListSize = function () {");
            outFile.WriteLine("        $.getJSON(url + 'getsize',", className.ToLower());
            outFile.WriteLine("            function (data) {");
            outFile.WriteLine("                self.listSize(data);");
            outFile.WriteLine("            });");
            outFile.WriteLine("    };");
            outFile.WriteLine("    self.getFilteredList = function () {");
            outFile.WriteLine("        $.getJSON(url + 'getallby',", className.ToLower());
            outFile.WriteLine("            { page: self.pageIndex(), pageSize: self.pageSize() },");
            outFile.WriteLine("            function (data) {");
            outFile.WriteLine("                self.filteredList.removeAll();");
            outFile.WriteLine("                $.each(data, function (index, item) {");
            outFile.WriteLine("                    self.filteredList.push(new {0}Model(item));", className.LoweredFirstChar());
            outFile.WriteLine("                });");
            outFile.WriteLine("            });");
            outFile.WriteLine("    };");
            outFile.WriteLine("    self.getModel = function (id) {");
            outFile.WriteLine("        $.getJSON(url + 'get',", className.ToLower());
            outFile.WriteLine("            { id: id },");
            outFile.WriteLine("            function (data) {");
            outFile.WriteLine("                self.model(new {0}Model(data));", className.LoweredFirstChar());
            outFile.WriteLine("            });");
            outFile.WriteLine("    };");
            outFile.WriteLine("    self.createModel = function () {");
            outFile.WriteLine("        $.ajax({");
            outFile.WriteLine("            url: url + 'create',", className.ToLower());
            outFile.WriteLine("            type: 'post',");
            outFile.WriteLine("            dataType: 'json',");
            outFile.WriteLine("            data: ko.toJSON(self.model),");
            outFile.WriteLine("            contentType: 'application/json',");
            outFile.WriteLine("            success: function (result) {");
            outFile.WriteLine("            },");
            outFile.WriteLine("            error: function (err) {");
            outFile.WriteLine("            },");
            outFile.WriteLine("            complete: function () {");
            outFile.WriteLine("                self.onLoad();");
            outFile.WriteLine("            }");
            outFile.WriteLine("        });");
            outFile.WriteLine("    };");
            outFile.WriteLine("    self.editModel = function () {");
            outFile.WriteLine("        $.ajax({");
            outFile.WriteLine("            url: url + 'edit',", className.ToLower());
            outFile.WriteLine("            type: 'post',");
            outFile.WriteLine("            dataType: 'json',");
            outFile.WriteLine("            data: ko.toJSON(self.model),");
            outFile.WriteLine("            contentType: 'application/json',");
            outFile.WriteLine("            success: function (result) {");
            outFile.WriteLine("            },");
            outFile.WriteLine("            error: function (err) {");
            outFile.WriteLine("            },");
            outFile.WriteLine("            complete: function () {");
            outFile.WriteLine("                self.onLoad();");
            outFile.WriteLine("            }");
            outFile.WriteLine("        });");
            outFile.WriteLine("    };");
            outFile.WriteLine("    self.deleteModel = function () {");
            outFile.WriteLine("        $.ajax({");
            outFile.WriteLine("            url: url + 'delete',", className.ToLower());
            outFile.WriteLine("            type: 'post',");
            outFile.WriteLine("            dataType: 'json',");
            outFile.WriteLine("            data: ko.toJSON(self.model),");
            outFile.WriteLine("            contentType: 'application/json',");
            outFile.WriteLine("            success: function (result) {");
            outFile.WriteLine("            },");
            outFile.WriteLine("            error: function (err) {");
            outFile.WriteLine("            },");
            outFile.WriteLine("            complete: function () {");
            outFile.WriteLine("                self.onLoad();");
            outFile.WriteLine("                var currentMaxPage = Math.ceil((self.listSize() - 1) / self.pageSize()) - 1;");
            outFile.WriteLine("                self.moveToPage(currentMaxPage);");
            outFile.WriteLine("            }");
            outFile.WriteLine("        });");
            outFile.WriteLine("    };");
            outFile.WriteLine("    self.setCurrentView = function (view) {");
            outFile.WriteLine("        self.currentView(view);");
            outFile.WriteLine("    };");
            outFile.WriteLine("    self.showList = function () {");
            outFile.WriteLine("        self.setCurrentView('List');");
            outFile.WriteLine("    };");
            outFile.WriteLine("    self.showDetails = function (obj, event) {");
            outFile.WriteLine("        self.getModel(obj.ProductID());");
            outFile.WriteLine("        self.setCurrentView('Details');");
            outFile.WriteLine("    };");
            outFile.WriteLine("    self.showCreate = function () {");
            outFile.WriteLine("        self.model(new {0}Model({{}}));", className.ToLower());
            outFile.WriteLine("        self.setCurrentView('Create');");
            outFile.WriteLine("    };");
            outFile.WriteLine("    self.showEdit = function () {");
            outFile.WriteLine("        self.setCurrentView('Edit');");
            outFile.WriteLine("    };");
            outFile.WriteLine("    self.showDelete = function (obj, event) {");
            outFile.WriteLine("        self.getModel(obj.{0}ID());", className.ToLower());
            outFile.WriteLine("        self.setCurrentView('Delete');");
            outFile.WriteLine("    };");
            outFile.WriteLine("    self.clickCreate = function () {");
            outFile.WriteLine("        self.createModel();");
            outFile.WriteLine("        self.showList();");
            outFile.WriteLine("    };");
            outFile.WriteLine("    self.clickEdit = function () {");
            outFile.WriteLine("        self.editModel();");
            outFile.WriteLine("        self.showList();");
            outFile.WriteLine("    };");
            outFile.WriteLine("    self.clickDelete = function () {");
            outFile.WriteLine("        self.deleteModel();");
            outFile.WriteLine("        self.showList();");
            outFile.WriteLine("    };");
            outFile.WriteLine("    self.onLoad = function () {");
            outFile.WriteLine("        self.getListSize();");
            outFile.WriteLine("        self.getFilteredList();");
            outFile.WriteLine("    };");
            
            foreach (var r in db.Relations.Where(p => p.ChildTable == tableName).OrderBy(p => p.ParentTable))
            {
                outFile.WriteLine("    self.getAvailable{0}List = function () {{", r.ParentTable.UpperedFirstChar());
                outFile.WriteLine("        $.getJSON('/{0}/getall',", r.ParentTable.ToLower());
                outFile.WriteLine("            function (data) {");
                outFile.WriteLine("                self.available{0}List.removeAll();", r.ParentTable.UpperedFirstChar());
                outFile.WriteLine("                $.each(data, function (index, item) {");
                outFile.WriteLine("                    self.available{0}List.push({{ ID: item.{1}, Name: item.{1} }});", r.ParentTable.UpperedFirstChar(), r.FkConstraintColumn);
                outFile.WriteLine("                });");
                outFile.WriteLine("            });");
                outFile.WriteLine("    };");
            }

            outFile.WriteLine("");
            outFile.WriteLine("    self.onLoad();");

            foreach (var r in db.Relations.Where(p => p.ChildTable == tableName).OrderBy(p => p.ParentTable))
            {
                outFile.WriteLine("    self.getAvailable{0}List();", r.ParentTable.UpperedFirstChar());
            }

            outFile.WriteLine("}");
            outFile.WriteLine("");
            outFile.WriteLine("// apply binding");
            outFile.WriteLine("$(function () {");
            outFile.WriteLine("    $.ajaxSetup({ cache: false });");
            outFile.WriteLine("    ko.applyBindings(new {0}ViewModel());", className.ToLower());
            outFile.WriteLine("});");

            outFile.Close();

            Console.Write(string.Format("\n{0} created", textPath));
        }
    }
}
