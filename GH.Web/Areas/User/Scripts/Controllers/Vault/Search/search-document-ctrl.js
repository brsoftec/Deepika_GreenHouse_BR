
var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.cellNav'], true);


myApp.getController('SearchDocumentController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter', '$window', 'DocumentVaultService', 'rguModal',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter, $window, DocumentVaultService, rguModal) {

    "use strict";
    var vault = VaultInformationService.VaultInformation;
  
    $scope.InitData = function () {
        $scope.IsEdit = false;
       
        //begin get document form
        $scope._doc = {
            _label: BasicInfo.label,
            _name: BasicInfo.name,
            _default: BasicInfo.default,
            _privacy: BasicInfo.privacy
        };

        // get list document passportID
        $scope._listdoc = [];
        $scope._new = "";
        $scope._type = "";
        $scope.hasFile = false;

        var maxindex = 0;
        $(BasicInfo.value).each(function (index, object) {
            if (maxindex < BasicInfo.value[index]._id)
                maxindex = BasicInfo.value[index]._id;
            var path = BasicInfo.value[index].jsPath;
            var arrJsPath = path.split("/");
            var len = arrJsPath.length;
            if (len > 2) {
                var jsPath1 = arrJsPath[len - 1];
                var jsPath2 = arrJsPath[len - 2];
                if (BasicBirth.label == arrJsPath[len - 2]) {
                    $(BasicBirth.value).each(function (ix, object) {
                        if (BasicBirth.value[ix]._id == arrJsPath[len - 1])
                            BasicInfo.value[index].expiryDate = BasicBirth.value[ix].expiryDate;
                    });
                }
                if (BasicPass.label == arrJsPath[len - 2]) {

                    //
                    $(BasicPass.value).each(function (ix, object) {
                        if (BasicPass.value[ix]._id == arrJsPath[len - 1])
                            BasicInfo.value[index].expiryDate = BasicPass.value[ix].expiryDate;
                    });
                }
                if (BasicEdu.label == arrJsPath[len - 2]) {
                    //BasicEdu
                    $(BasicEdu.value).each(function (ix, object) {
                        if (BasicEdu.value[ix]._id == arrJsPath[len - 1])
                            BasicInfo.value[index].expiryDate = BasicEdu.value[ix].expiryDate;
                    });
                }
                if (BasicEmp.label == arrJsPath[len - 2]) {
                    //BasicEdu
                    $(BasicEmp.value).each(function (ix, object) {
                        if (BasicEmp.value[ix]._id == arrJsPath[len - 1])
                            BasicInfo.value[index].expiryDate = BasicEmp.value[ix].expiryDate;
                    });
                }
                //BasicBankCard
                if (BasicBankCard.label == arrJsPath[len - 2]) {

                    $(BasicBankCard.value).each(function (ix, object) {
                        if (BasicBankCard.value[ix]._id == arrJsPath[len - 1])
                            BasicInfo.value[index].expiryDate = BasicBankCard.value[ix].expiryDate;
                    });
                }
            }



            $scope._listdoc.push({
                _id: BasicInfo.value[index]._id,
                IsEdit: false,
                privacy: BasicInfo.value[index].privacy,
                name: BasicInfo.value[index].name,
                saveName: BasicInfo.value[index].saveName,
                category: BasicInfo.value[index].category,
                description: BasicInfo.value[index].description,
                type: BasicInfo.value[index].type,
                uploadDate: BasicInfo.value[index].uploadDate,
                expiryDate: BasicInfo.value[index].expiryDate,
                jsPath: BasicInfo.value[index].jsPath,

                nosearch: BasicInfo.value[index].nosearch
            });

        });

        // processfile
        $scope.fnUpload = function () {
            var fd = new FormData()
            for (var i in $scope.files) {
                fd.append("uploadedFile", $scope.files[i])
            }

            $scope._type = $scope.files[0].type;
            var xhr = new XMLHttpRequest();
            xhr.addEventListener("load", uploadComplete, false);
            xhr.open("POST", "/api/InformationVaultService/AttachFile?userid=" + $rootScope.ruserid
                + "&fileName=" + $scope._name);
            $scope.progressVisible = true;
            xhr.send(fd);
        }

        function uploadComplete(evt) {
            $scope.progressVisible = false;
            if (evt.target.status == 201) {
                $scope.FilePath = evt.target.responseText;
                $scope.AttachStatus = "Upload Done";
                $scope.hasFile = false;
            }
            else {
                $scope.AttachStatus = evt.target.responseText;
            }
        }

        $scope.setFiles = function (element) {

            if (element.value != "") {
                $scope.hasFile = true;


                //check file
                var inputFile = element.files[0].name;
                var outputFile = element.files[0].name;
                var checkFileTemp = false;
                var temp = 1;
                while (checkFileTemp == false) {
                    checkFileTemp = true;
                    $($scope._listdoc).each(function (index) {
                        if (outputFile == $scope._listdoc[index].name) {
                            var tempFile = inputFile.split('.');

                            var len = tempFile.length;
                            if (len > 1) {
                                tempFile[len - 2] += temp;
                                var tempFilep = tempFile[0];
                                for (var i = 1; i < tempFile.length ; i++)
                                    tempFilep += '.' + tempFile[i];
                                outputFile = tempFilep;
                            }
                            else
                                outputFile += temp;


                            checkFileTemp = false;
                        }

                    });
                    temp = temp + 1;
                }
            }

            else
                $scope.hasFile = false;

            $scope.$apply(function (scope) {
                $scope.AttachStatus = "";
                $scope.files = []
                for (var i = 0; i < element.files.length; i++) {
                    $scope.files.push(element.files[i])

                }
                $scope.progressVisible = false

            });
            $scope._name = outputFile;
        }

        //


        // Begin Delete file

        $scope.DeleteFile = function (_file) {
            var model = new Object();
            model.UserID = $rootScope.ruserid;
            model.FileName = _file.name
            var index = $scope._listdoc.indexOf(_file);

            swal({
                title: "Are you sure to remove file " + _file.description + "?",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes",
                cancelButtonText: "No"

            }).then(function () {
                DocumentVaultService.DeleteFile(model).then(function () {
                    $scope._listdoc.splice(index, 1);
                    $scope.Save();
                    swal(
                    'Deleted',
                    'File ' + _file.description + ' has been deleted.',
                    'success'
                  )

                }, function (errors) {

                });
            }
            , function (dismiss) {
                if (dismiss === 'cancel') {

                }
            });
        };

        //$scope.DeleteFile = function (_file) {
        //    var model = new Object();
        //    model.UserID = $rootScope.ruserid;
        //    model.FileName = _file.name
        //    var index = $scope._listdoc.indexOf(_file);
        //    swal({
        //        title: "Are you sure to remove file " + _file.name + "?",
        //        type: "warning",
        //        showCancelButton: true,
        //        confirmButtonColor: "#DD6B55",
        //        confirmButtonText: "Yes",
        //        cancelButtonText: "No!",
        //        closeOnConfirm: false,
        //        closeOnCancel: true
        //    }, function (isConfirm) {
        //        if (isConfirm) {

        //            DocumentVaultService.DeleteFile(model).then(function () {
        //                $scope._listdoc.splice(index, 1);
        //                $scope.Save();
        //                swal('OK', '', 'success');

        //            }, function (errors) {
        //                __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
        //            });
        //            //  $scope.Save();
        //        }
        //    });
        //};
        // End Delete file

        $scope.RenderFile = function (_file) {
            var s = '/api/InformationVaultService/DownLoadFile?'
           + "userid=" + $rootScope.ruserid
           + "&FileName=" + _file.name;
            $window.open(s);
        }

        $scope.vaultDocSearch = {
            query: '',
            pageStart: 0,
            pageSize: 5,
            pages: [0]
        };

        $scope.clearDocSearch = function () {
            $scope.vaultDocSearch.query = '';
        };

        $scope.uploadDoc = function () {
            var doc = {
                'IsEdit': false,
                'privacy': true,
                'name': "",
                'saveName': "",
                'description': "",
                'category': "",
                'type': "",
                'uploadDate': new Date(),
                'jsPath': "",
                'nosearch': true
            };
            $scope.openDocEditor(doc, true);
        };

        $scope.openDocEditor = function (doc, newDoc) {

            $scope.doc = doc;
            $scope.newDoc = newDoc;
            rguModal.openModal('vault.doc.editor', $scope);
        };

        //birthID
        $scope.uploadDocFormTest = function () {
            var doc = {
                'IsEdit': false,
                'privacy': true,
                'name': "",
                'saveName': "",
                'description': "",
                'category': "",
                'type': "",
                'uploadDate': new Date(),
                'expiryDate': "",
                'jsPath': "",
                'nosearch': true
            };
            $scope.openDocEditorForm(doc, true)
        };

        //begin process upload file

        //1. uploadDocForm 
        $scope.uploadDocForm = function (_jsPath) {
            var doc = {
                'IsEdit': false,
                'privacy': true,
                'name': "",
                'saveName': "",
                'description': "",
                'category': "",
                'type': "",
                'uploadDate': new Date(),
                'expiryDate': "",
                'jsPath': _jsPath,
                'nosearch': true
            };
            var jsPath = _jsPath;
            var cat = '';
            if (/passport/i.test(jsPath)) {
                cat = 'Passport';
            } else if (/birth/i.test(jsPath)) {
                cat = 'Birth Certificate';
            } else if (/driver/i.test(jsPath)) {
                cat = 'Driver License';
            } else if (/member/i.test(jsPath)) {
                cat = 'Membership Card';
            } else if (/resume/i.test(jsPath)) {
                cat = 'Resume';
            } else if (/employment/i.test(jsPath)) {
                cat = 'Employment Pass';
            } else if (/tax/i.test(jsPath)) {
                cat = 'Employment Pass';
            } else if (/visa/i.test(jsPath)) {
                cat = 'Credit Card';
            } else if (/master/i.test(jsPath)) {
                cat = 'Credit Card';
            } else if (/paypal/i.test(jsPath)) {
                cat = 'Credit Card';
            } else if (/bank/i.test(jsPath)) {
                cat = 'Credit Card';
            }
            else if (/education/i.test(jsPath)) {
                cat = 'Education Degree';
            }
            else
                cat = 'Document';

            doc.category = cat;
            $scope.openDocEditorNewForm(doc, true)
        };

        //2. GenerateFileType
        $scope.GenerateFileType = function (type) {
            switch (type) {

                case "image/jpeg":
                case "image/bmp":
                case "image/png":
                case "image/gif":
                    return "img";
                    break;
                default:
                    return "doc";
                    break;
            }
        }

        //3. openDocEditorForm
        $scope.openDocEditorNewForm = function (doc, newDoc) {
            $scope.doc = doc;
            $scope.newDoc = newDoc;
            $scope.source = 'form';
            rguModal.openModal('vault.doc.editor', $scope);
        };

        //4. Add new document
        $scope.addNewDoc = function (newDoc) {
            if ($scope.hasFile == true) {
                $scope.fnUpload();
                $scope._listdoc.push({
                    '_id': maxindex + 1,
                    'IsEdit': false,
                    'privacy': newDoc.privacy,
                    'description': newDoc.description,
                    'category': newDoc.category,
                    'name': $scope._name,
                    'type': $scope._type,
                    'saveName': $scope._name,
                    'uploadDate': new Date(),
                    'expiryDate': "",
                    'jsPath': newDoc.jsPath,
                    'nosearch': newDoc.nosearch
                });
            }
            $scope.Save();
        }
        $scope.openDocEditorForm = function (doc, newDoc) {
            $scope.doc = doc;
            $scope.newDoc = newDoc;
            $scope.source = 'form';
            rguModal.openModal('vault.doc.editor', $scope);
        };
        //end birthID
        $scope.filterDocsByQuery = function (doc) {
            var re = new RegExp($scope.vaultDocSearch.query, 'i');
            return re.test(doc.description) || re.test(doc.category);
        };

        $scope.initPages = function () {
            var pageCount = Math.ceil($scope.matchedDocs.length / $scope.vaultDocSearch.pageSize);
            $scope.vaultDocSearch.pages = [];
            for (var i = 1; i <= pageCount; i++) {
                $scope.vaultDocSearch.pages.push(i);
            }
        };

        $scope.gotoPage = function (page) {
            $scope.vaultDocSearch.pageStart = page * $scope.vaultDocSearch.pageSize;
        };
        $scope.onVaultDocSearchInput = function () {
        };
    }
    $scope.$on('document', function () {
        $scope.InitData();
    });
    $scope.InitData();


    //save
    // == edit == 
    $scope.Save = function () {

        //address
        BasicInfo.label = $scope._doc._label;
        if ($scope._doc._name == '') {
            $scope._doc._name = $scope._doc._label;
        }
        BasicInfo.name = $scope._doc._name;
        BasicInfo.privacy = $scope._doc._privacy;


        //
        var checkDefault = true;
        $($scope._listdoc).each(function (index) {

            if (checkDefault == true && $scope._listdoc[index]._default == true && $scope._listdoc[index].description != $scope._doc._default) {
                $scope._doc._default = $scope._listdoc[index].description;
                checkDefault = false;
            }

        });

        BasicInfo.default = $scope._doc._default;
        //
        var lstDocSave = [];
        $($scope._listdoc).each(function (index, object) {

            lstDocSave.push({
                _id: $scope._listdoc[index]._id,
                privacy: $scope._listdoc[index].privacy,
                name: $scope._listdoc[index].name,
                SaveName: $scope._listdoc[index].SaveName,
                description: $scope._listdoc[index].description,
                type: $scope._listdoc[index].type,
                category: $scope._listdoc[index].category,
                uploadDate: $scope._listdoc[index].uploadDate,
                expiryDate: $scope._listdoc[index].expiryDate,
                jsPath: $scope._listdoc[index].jsPath,
                nosearch: $scope._listdoc[index].nosearch,

            })
            $scope._listdoc[index].IsEdit = false;
        });


        //Save
        VaultInformationService.VaultInformation.document = BasicInfo;
        VaultInformationService.VaultInformation.document.value = lstDocSave;
        VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
            VaultInformationService.VaultInformation._id).then(function (response) {
                $scope.IsEdit = false;
                $scope._new = "";

                $scope.hasFile = false;
                $rootScope.$broadcast('document');
            }, function (errors) {

            });
    }

    $scope.saveVaultInformationOnSuccess = function (response) {
        $scope.IsEdit = false;
        $scope._new = "";
    }
    $scope.saveVaultInformationOnError = function (response) {
        alertService.renderSuccessMessage(response.ReturnMessage);
        $scope.messageBox = alertService.returnFormattedMessage();
        $scope.alerts = alertService.returnAlerts();
    }

    $scope.Cancel = function () {
        $scope.InitData();
    }

    $scope.goBack = function () {
        $scope.$location.path('/VaultInformation');
    };
    $scope.closePanel = function () {
        $scope.Cancel();
    };


}]);

