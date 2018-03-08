var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.cellNav'], true);

//Tax ID
myApp.getController('TaxIDController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter', 'DocumentVaultService',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter, DocumentVaultService) {

    "use strict";
    var BasicInfo = VaultInformationService.VaultInformation.groupGovernmentID.value.taxID;
    var _document = VaultInformationService.VaultInformation.document;
    $scope.InitData = function () {
        $scope.IsEdit = false;

        // General   
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
            var _jspath = "/Government ID/Tax ID/" + BasicInfo.value[index]._id;
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
                IsEdit: false,
                _id: BasicInfo.value[index]._id,
                privacy: BasicInfo.value[index].privacy,
                description: BasicInfo.value[index].description,

                firstName: BasicInfo.value[index].firstName,
                middleName: BasicInfo.value[index].middleName,
                lastName: BasicInfo.value[index].lastName,
               
                cardNumber: BasicInfo.value[index].cardNumber,
                issuedDate: BasicInfo.value[index].issuedDate,
                expiryDate: BasicInfo.value[index].expiryDate,
                expiryNo: expiryNo,
                indefiniteDate: _indefiniteDate,
                issuedBy: BasicInfo.value[index].issuedBy,
              
                note: BasicInfo.value[index].note,
                docJspath: _jspath,
                _default: BasicInfo.value[index]._default

            });
            if ($scope._listForm[index].issuedDate != "" && $scope._listForm[index].issuedDate != undefined)
                $scope._listForm[index].issuedDate = new Date($scope._listForm[index].issuedDate);
            else
                $scope._listForm[index].issuedDate = new Date();

            if ($scope._listForm[index].expiryDate != "" && $scope._listForm[index].expiryDate != undefined)
            {
                $scope._listForm[index].expiryDate = new Date($scope._listForm[index].expiryDate);
                if ($scope._listForm[index].expiryDate < today)
                    $scope._listForm[index].expiryNo = true;
            }
               
           
        });


        maxindex = maxindex + 1;
        $rootScope._newJspath = "/Government ID/Tax ID/" + maxindex;
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
             
               'cardNumber': "",     
               'issuedDate': new Date('1960-01-01'),
               'expiryDate': '',
               'indefiniteDate': true,
               'issuedBy': "",
               'note': ""        
           };

        $scope.addNewForm = function (newForm) {
          var jpd = "/Government ID/Tax ID/" + newForm._id;
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
                'cardNumber': newForm.cardNumber,
                'issuedDate': newForm.issuedDate,
                'expiryDate': newForm.expiryDate,
                'indefiniteDate': newForm.indefiniteDate,
                'docJspath': jpd,
                'issuedBy': newForm.issuedBy,
                'note': newForm.note
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
                jsPath: _document.value[index].jsPath,
                nosearch: _document.value[index].nosearch
            });
        });

        $scope.checkRemoveDoc = false;
        $scope.removeFormDoc = function (_value) {
            $scope.checkRemoveDoc = true;
            var _jsp = "/Government ID/Tax ID/" + _value._id;
            var model = new Object();
            model.UserID = $rootScope.ruserid;

            $($scope._listdocument).each(function (index, object) {
                if (_jsp == $scope._listdocument[index].jsPath) {
                    model.FileName = $scope._listdocument[index].name;
                    DocumentVaultService.DeleteFile(model).then(function () {
                        $scope._listdocument.splice(index, 1);
                    }
                    , function (errors) {

                    });
                }
            });
        };
        // end document
        
        $scope.messageBox = "";
        $scope.alerts = [];

    }

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

        });

        BasicInfo.default = $scope._form._default;


        //
        var lstFormSave = [];
        $($scope._listForm).each(function (index, object) {
           
             // indefinite
            if ($scope._listForm[index].indefiniteDate == true)
                $scope._listForm[index].expiryDate = "";
            // end indefinite
          
            lstFormSave.push({
                _id: $scope._listForm[index]._id,
                privacy: $scope._listForm[index].privacy,

                description: $scope._listForm[index].description,
                firstName: $scope._listForm[index].firstName,
                middleName: $scope._listForm[index].middleName,
                lastName: $scope._listForm[index].lastName,
            
                cardNumber: $scope._listForm[index].cardNumber,
                issuedDate: $scope._listForm[index].issuedDate,
                expiryDate: $scope._listForm[index].expiryDate,
                issuedBy: $scope._listForm[index].issuedBy,
           
                note: $scope._listForm[index].note,
           
                _default: $scope._listForm[index]._default,

            })
            $scope._listForm[index].IsEdit = false;
        });


        // === 
        var groupName = VaultInformationService.VaultInformation.groupGovernmentID.name;
        if (groupName == "")
            VaultInformationService.VaultInformation.groupGovernmentID.name = "Government ID";

        //Save
        VaultInformationService.VaultInformation.groupGovernmentID.value.taxID = BasicInfo;
        VaultInformationService.VaultInformation.groupGovernmentID.value.taxID.value = lstFormSave;
        if ($scope.checkRemoveDoc == true) {
            VaultInformationService.VaultInformation.document = _document;
            VaultInformationService.VaultInformation.document.value = lstDocSave;
        }
        VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
            VaultInformationService.VaultInformation._id).then(function (response) {
                $scope.IsEdit = false;
                $scope._new = "";
              
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