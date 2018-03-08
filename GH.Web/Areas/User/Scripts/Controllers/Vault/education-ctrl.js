var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.cellNav'], true);


//9.0 education
myApp.getController('educationController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter', 'DocumentVaultService',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter, DocumentVaultService)  {

    "use strict";
    var BasicInfo = VaultInformationService.VaultInformation.education;
    var _document = VaultInformationService.VaultInformation.document;
    $scope.InitData = function () {
        $scope.IsEdit = false;
        // General     //_form _listForm addNewForm newForm removeForm
        $scope._form = {
            _label: BasicInfo.label,
            _name: BasicInfo.name,
            _default: BasicInfo.default,
            _privacy: BasicInfo.privacy
        };

        //Education
        $scope._listForm = [];
        var maxindex = 0;

        $(BasicInfo.value).each(function (index, object) {
            if (maxindex < BasicInfo.value[index]._id)
                maxindex = BasicInfo.value[index]._id;
            var _jspath = "/Education/" + BasicInfo.value[index]._id;

            $scope._listForm.push({
                _id: BasicInfo.value[index]._id,
                IsEdit: false,
                privacy: BasicInfo.value[index].privacy,

                description: BasicInfo.value[index].description,
                schoolName: BasicInfo.value[index].schoolName,

                major: BasicInfo.value[index].major,
                minor: BasicInfo.value[index].minor,
                schoolId: BasicInfo.value[index].schoolId,
                expiryDate: BasicInfo.value[index].expiryDate,
                certificationType: BasicInfo.value[index].certificationType,

                grade: BasicInfo.value[index].grade,
                certificateNumber: BasicInfo.value[index].certificateNumber,
                graduatedDate: BasicInfo.value[index].graduatedDate,
                 docJspath: _jspath,
                note: BasicInfo.value[index].note,
                _default: BasicInfo.value[index]._default,

            });

            if ($scope._listForm[index].expiryDate != "" && $scope._listForm[index].expiryDate != undefined)
                $scope._listForm[index].expiryDate = new Date($scope._listForm[index].expiryDate);
            else
                $scope._listForm[index].expiryDate = new Date();

            if ($scope._listForm[index].graduatedDate != "" && $scope._listForm[index].graduatedDate != undefined)
                $scope._listForm[index].graduatedDate = new Date($scope._listForm[index].graduatedDate);
            else
                $scope._listForm[index].graduatedDate = new Date();

        });

        maxindex = maxindex + 1;
        $rootScope._newJspath = "/Education/" + maxindex;
        $scope._new =
           {
               '_id': maxindex,
                'IsEdit': false,
                'privacy': "",
                'description': "",
                '_default': true,

                'schoolName': "",
                'major': "",
                'minor': "",
                'schoolId': "",
                'certificationType': "",
                'grade': "",
                'certificateNumber': "",
                'expiryDate': new Date(),
                'graduatedDate': new Date(),

                'note': "",

            };
        $scope.addNewForm = function (newForm) {
            $scope._listForm.push({
                '_id': newForm._id,
                'IsEdit': false,
                'privacy': newForm.privacy,
                'description': newForm.description,
                '_default': newForm._default,

                'schoolName': newForm.schoolName,
                'major': newForm.major,
                'minor': newForm.minor,
                'schoolId': newForm.schoolId,
                'expiryDate': newForm.expiryDate,
                'certificationType': newForm.certificationType,
                'grade': newForm.grade,
                'certificateNumber': newForm.certificateNumber,

                'graduatedDate': newForm.graduatedDate,

                'note': newForm.note,
            });
        }

        //
        
        //

        $scope.removeForm = function (_value) {
            var index = $scope._listForm.indexOf(_value);
            swal({
                title: "Are you sure to delete form " + _value.description + " ?",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes",
                cancelButtonText: "No"

            }).then(function () {
                $scope._listForm.splice(index, 1);
                $scope.Save();
                swal(
                    'Deleted',
                    'Form ' + _value.description + ' has been deleted.',
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
            var _jsp = "/Education/" + _value._id;
            var model = new Object();
            model.UserID = $rootScope.ruserid;

            for (var i = $scope._listdocument.length - 1; i >= 0; i--) {
                if ($scope._listdocument[i].jsPath == _jsp) {
                    $scope._listdocument.splice(i, 1);
                }
            }
        };
        // end Document

    }
    ///
    $scope.$on('education', function () {
        $scope.InitData();
    });

    $scope.InitData();
    $scope.Edit = function () {
        $scope.IsEdit = true;
    }

    $scope.Save = function () {

        //education general education
        BasicInfo.label = $scope._form._label;
        if ($scope._form._name == '') {
            $scope._form._name = $scope._form._label;

        }
        BasicInfo.name = $scope._form._name;
        BasicInfo.privacy = $scope._form._privacy;
        //
        var lstFormSave = [];
        var checkDefault = true;
        $($scope._listForm).each(function (index) {

            if (checkDefault == true && $scope._listForm[index]._default == true && $scope._listForm[index].description != $scope._form._default) {
                $scope._form._default = $scope._listForm[index].description;
                checkDefault = false;
            }

        });

        $($scope._listForm).each(function (index, object) {


            if (checkDefault == false && $scope._listForm[index].description != $scope._form._default) {
                $scope._listForm[index]._default = false;
            }
            lstFormSave.push({
                _id: $scope._listForm[index]._id,
                privacy: $scope._listForm[index].privacy,
                description: $scope._listForm[index].description,
                _default: $scope._listForm[index]._default,
                schoolName: $scope._listForm[index].schoolName,
                major: $scope._listForm[index].major,
                minor: $scope._listForm[index].minor,
                schoolId: $scope._listForm[index].schoolId,
                expiryDate: $scope._listForm[index].expiryDate,
                certificationType: $scope._listForm[index].certificationType,
                grade: $scope._listForm[index].grade,
                certificateNumber: $scope._listForm[index].certificateNumber,
                graduatedDate: $scope._listForm[index].graduatedDate,


                note: $scope._listForm[index].note

            })


            $scope._listForm[index].IsEdit = false;
        });



        //set default
        BasicInfo.default = $scope._form._default;

        //
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

                jsPath: $scope._listdocument[index].jsPath,
                nosearch: $scope._listdocument[index].nosearch,

            })
            $scope._listdocument[index].IsEdit = false;
        });

        //End document
     

        //save
        VaultInformationService.VaultInformation.education = BasicInfo;
        VaultInformationService.VaultInformation.education.value = lstFormSave;
        if ($scope.checkRemoveDoc == true) {
            VaultInformationService.VaultInformation.document = _document;
            VaultInformationService.VaultInformation.document.value = lstDocSave;
        }
        VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
            VaultInformationService.VaultInformation._id).then(function (response) {
                $scope.IsEdit = false;
                $scope._new = "";
                $scope._new = "";
                $rootScope.$broadcast('education');
                
                $scope.checkRemoveDoc == false;
                $rootScope._newJspath = "";
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

