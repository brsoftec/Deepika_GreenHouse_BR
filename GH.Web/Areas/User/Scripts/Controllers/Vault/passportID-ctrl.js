//PassportIDController
var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.cellNav'], true);


//5.1 PassportID 
myApp.getController('PassportIDController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter', 'DocumentVaultService',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter, DocumentVaultService) {

    "use strict";
    var BasicInfo = VaultInformationService.VaultInformation.groupGovernmentID.value.passportID;
    var _document = VaultInformationService.VaultInformation.document;
    $scope.InitData = function () {
        $scope.IsEdit = false;
       
        $scope._form = {
            _label: BasicInfo.label,
            _name: BasicInfo.name,
            _default: BasicInfo.default,
            _privacy: BasicInfo.privacy
        };
        //
        $scope._listForm = [];
        var maxindex = 0;

        $(BasicInfo.value).each(function (index, object) {
            if (maxindex < BasicInfo.value[index]._id)
                maxindex = BasicInfo.value[index]._id;
            var _jspath = "/Government ID/Passport ID/" + BasicInfo.value[index]._id;
            
            var today = new Date();
            var expiryNo = false;
           
            // indefinite
            var _indefiniteDate = true;
            if (BasicInfo.value[index].expiryDate == undefined)
                BasicInfo.value[index].expiryDate = "";
            if (BasicInfo.value[index].expiryDate != "")
                _indefiniteDate = false;
            // End indefinite


            $scope._listForm.push({
                _id: BasicInfo.value[index]._id,
                IsEdit: false,
                privacy: BasicInfo.value[index].privacy,
                description: BasicInfo.value[index].description,

                firstName: BasicInfo.value[index].firstName,
                middleName: BasicInfo.value[index].middleName,
                lastName: BasicInfo.value[index].lastName,
                nationality: BasicInfo.value[index].nationality,
                cardNumber: BasicInfo.value[index].cardNumber,
                issuedDate: BasicInfo.value[index].issuedDate,
                expiryDate: BasicInfo.value[index].expiryDate,
                indefiniteDate: _indefiniteDate,
                issuedBy: BasicInfo.value[index].issuedBy,
                issuedIn: BasicInfo.value[index].issuedIn,
                note: BasicInfo.value[index].note,
                expiryNo: expiryNo,
                docJspath: _jspath,
                _default: BasicInfo.value[index]._default,

            });
            if ($scope._listForm[index].issuedDate != "" && $scope._listForm[index].issuedDate != undefined)
                $scope._listForm[index].issuedDate = new Date($scope._listForm[index].issuedDate);
            else
                $scope._listForm[index].issuedDate = new Date();

            if ($scope._listForm[index].expiryDate != "" && $scope._listForm[index].expiryDate != undefined) {
                $scope._listForm[index].expiryDate = new Date($scope._listForm[index].expiryDate);
                if ($scope._listForm[index].expiryDate < today)
                    $scope._listForm[index].expiryNo = true;
            }

       
        });

        maxindex = maxindex + 1;
        $rootScope._newJspath = "/Government ID/Passport ID/" + maxindex;
        $scope._new =
           {
               '_id': maxindex,
               'IsEdit': false,
               'privacy': "",
               'description': "",
               '_default': true,

               'firstName': "",
               'middleName': "",
               'lastName': "",

               'nationality': "",
               'cardNumber': "",
                      
               'issuedDate': new Date('1960-01-01'),
               'expiryDate': '',
                'indefiniteDate': true,
               'issuedBy': "",
               'issuedIn': "",
               
               'note': "",

           };

        $scope.addNewForm = function (newForm) {
            var jpd = "/Government ID/Passport ID/" + newForm._id;
               if (newForm.indefiniteDate == true)
                newForm.expiryDate = '';
            $scope._listForm.push({
                '_id': newForm._id,
                'IsEdit': false,
                'privacy': newForm.privacy,
                'description': newForm.description,
                '_default': newForm._default,
                'firstName': newForm.firstName,
                'middleName': newForm.middleName,
                'lastName': newForm.lastName,
                'nationality': newForm.nationality,
                'cardNumber': newForm.cardNumber,
                'issuedDate': newForm.issuedDate,
                'expiryDate': newForm.expiryDate,
                'indefiniteDate': newForm.indefiniteDate,
                'issuedBy': newForm.issuedBy,
                'issuedIn': newForm.issuedIn,
                'docJspath': jpd,
                'note': newForm.note,

            });
        }

        $scope.removeForm = function (_value) {
            var index = $scope._listForm.indexOf(_value);
            swal({
                title: "Are you sure to remove " + _value.description + " ?",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes",
                cancelButtonText: "No"

            }).then(function () {
                $scope._listForm.splice(index, 1);
                $scope.Save();
                swal(
                    'Removed',
                     _value.description + ' has been removed.',
                    'success'
                  )
            }
            , function (dismiss) {
                if (dismiss === 'cancel') {

                }
            });


        };

        // Document
        $scope._listdocument = [];
        $(_document.value).each(function (index, object) {

            $scope._listdocument.push({
                _id: _document.value[index]._id,
                IsEdit: false,
                privacy: _document.value[index].privacy,
                name: _document.value[index].name,
                saveName: _document.value[index].saveName,
                category: _document.value[index].category,
                description: _document.value[index].description,
                type: _document.value[index].type,
                uploadDate: _document.value[index].uploadDate,
                expiryDate: _document.value[index].expiryDate,
                jsPath: _document.value[index].jsPath,
                nosearch: _document.value[index].nosearch
               
            });
        });

        $scope.checkRemoveDoc = false;
        $scope.removeFormDoc = function (_value) {
            $scope.checkRemoveDoc = true;
            var _jsp = "/Government ID/Passport ID/" + _value._id;
            var model = new Object();
            model.UserID = $rootScope.ruserid;
          
            for (var i = $scope._listdocument.length - 1; i >= 0; i--) {
                if ($scope._listdocument[i].jsPath == _jsp) {

                    model.FileName = $scope._listdocument[i].name;
                    DocumentVaultService.DeleteFile(model).then(function () {
                        $scope._listdocument.splice(i, 1);
                        
                    }
                    , function (errors) {

                    });
                   
                }
            }
          
        };
        // end Document

        $scope.messageBox = "";
        $scope.alerts = [];

    }
    $scope.$on('passportID', function () {
        $scope.InitData();
    });
    $scope.InitData();
    $scope.Edit = function () {
        $scope.IsEdit = true;
    }

    // == edit == 
    $scope.Save = function () {

        //address
        BasicInfo.label = $scope._form._label;
        if ($scope._form._name == '') {
            $scope._form._name = $scope._form._label;
        }
        BasicInfo.name = $scope._form._name;
        BasicInfo.privacy = $scope._form._privacy;


        //
        var checkDefault = true;
        $($scope._listForm).each(function (index) {

            if (checkDefault == true && $scope._listForm[index]._default == true && $scope._listForm[index].description != $scope._form._default) {
                $scope._form._default = $scope._listForm[index].description;
                checkDefault = false;
            }

            //Check Expiry date
            //$($scope._listdocument).each(function (ix, object) {
            //    if ($scope._listdocument[ix].jsPath == $scope._listForm[index].docJspath) {
            //        $scope._listdocument[ix].expiryDate = $scope._listForm[index].expiryDate;
            //        $scope.checkRemoveDoc = true
            //    }

            //});
        });

        BasicInfo.default = $scope._form._default;
        //
        var lstFormSave = [];
        $($scope._listForm).each(function (index, object) {
            if (checkDefault == false && $scope._listForm[index].description != $scope._form._default) {
                $scope._listForm[index]._default = false;
            }

            // indefinite
            if ($scope._listForm[index].indefiniteDate == true)
                $scope._listForm[index].expiryDate = "";

            lstFormSave.push({
                _id: $scope._listForm[index]._id,
                privacy: $scope._listForm[index].privacy,

                description: $scope._listForm[index].description,
                firstName: $scope._listForm[index].firstName,

                middleName: $scope._listForm[index].middleName,
                lastName: $scope._listForm[index].lastName,
                nationality: $scope._listForm[index].nationality,
                cardNumber: $scope._listForm[index].cardNumber,
                issuedDate: $scope._listForm[index].issuedDate,
                expiryDate: $scope._listForm[index].expiryDate,
                issuedBy: $scope._listForm[index].issuedBy,

                issuedIn: $scope._listForm[index].issuedIn,
                note: $scope._listForm[index].note,
                _default: $scope._listForm[index]._default,

            })
            $scope._listForm[index].IsEdit = false;
        });

        //Document 

        var lstDocSave = [];
        $($scope._listdocument).each(function (index, object) {

            lstDocSave.push({
                _id: $scope._listdocument[index]._id,
                privacy: $scope._listdocument[index].privacy,
                name: $scope._listdocument[index].name,
                SaveName: $scope._listdocument[index].SaveName,
                description: $scope._listdocument[index].description,
                type: $scope._listdocument[index].type,
                category: $scope._listdocument[index].category,
                uploadDate: $scope._listdocument[index].uploadDate,
                //expiryDate: $scope._listdocument[index].expiryDate,
                jsPath: $scope._listdocument[index].jsPath,
                nosearch: $scope._listdocument[index].nosearch,
                //
             
                //

            })
            $scope._listdocument[index].IsEdit = false;
        });

        //End document

        // === 
        var groupName = VaultInformationService.VaultInformation.groupGovernmentID.name;
        if (groupName == "")
            VaultInformationService.VaultInformation.groupGovernmentID.name = "Government ID";

        //Save
        VaultInformationService.VaultInformation.groupGovernmentID.value.passportID = BasicInfo;
        VaultInformationService.VaultInformation.groupGovernmentID.value.passportID.value = lstFormSave;
        if ($scope.checkRemoveDoc == true) {
            VaultInformationService.VaultInformation.document = _document;
            VaultInformationService.VaultInformation.document.value = lstDocSave;
        }

        VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
            VaultInformationService.VaultInformation._id).then(function (response) {
                $scope.IsEdit = false;
                $scope._new = "";
             //   $rootScope.$broadcast('passportID');
               
                $scope.checkRemoveDoc == false;
                $rootScope._newJspath = "";
                $rootScope.delegatorid = null;
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

}])