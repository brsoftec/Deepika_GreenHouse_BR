
var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.cellNav'], true);


//8.0 employment
myApp.getController('employmentController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter', 'DocumentVaultService',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter, DocumentVaultService) {

    "use strict";
    var BasicInfo = VaultInformationService.VaultInformation.employment;
    //employment

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
            var _jspath = "/Employment/" + BasicInfo.value[index]._id;
         
            if (BasicInfo.value[index].phoneCountry == undefined)
                BasicInfo.value[index].phoneCountry = "";
            if (BasicInfo.value[index].mobileCountry == undefined)
                BasicInfo.value[index].mobileCountry = "";
            if (BasicInfo.value[index].faxCountry == undefined)
                BasicInfo.value[index].faxCountry = "";


        $scope._listForm.push({
            _id: BasicInfo.value[index]._id,
            IsEdit: false,
            privacy: BasicInfo.value[index].privacy,
            description: BasicInfo.value[index].description,
            _default: BasicInfo.value[index]._default,
            companyName: BasicInfo.value[index].companyName,
            title: BasicInfo.value[index].title,
            email: BasicInfo.value[index].email,
            phoneNumber: BasicInfo.value[index].phoneNumber,
            phoneCountry: BasicInfo.value[index].phoneCountry,
            mobileNumber: BasicInfo.value[index].mobileNumber,
            mobileCountry: BasicInfo.value[index].mobileCountry,
            fax: BasicInfo.value[index].fax,
            faxCountry: BasicInfo.value[index].faxCountry,
            salary: BasicInfo.value[index].salary,
            employmentStatus: BasicInfo.value[index].employmentStatus,
            endDate: BasicInfo.value[index].endDate,
            startDate: BasicInfo.value[index].startDate,
            addressLine: BasicInfo.value[index].addressLine,
            addressLine_lat: BasicInfo.value[index].addressLine_lat,
            addressLine_lng: BasicInfo.value[index].addressLine_lng,

            state: BasicInfo.value[index].state,
            zipCode: BasicInfo.value[index].zipCode,
            note: BasicInfo.value[index].note,
            docJspath: _jspath,
            countryCity: { country: BasicInfo.value[index].country, city: BasicInfo.value[index].city }
        });
        if ($scope._listForm[index].startDate != "" && $scope._listForm[index].startDate != undefined)
            $scope._listForm[index].startDate = new Date($scope._listForm[index].startDate);
        else
            $scope._listForm[index].startDate = new Date();

        if ($scope._listForm[index].endDate != "" && $scope._listForm[index].endDate != undefined)
            $scope._listForm[index].endDate = new Date($scope._listForm[index].endDate);
        else
            $scope._listForm[index].endDate = new Date();
                
       
        });

        maxindex = maxindex + 1;
        $rootScope._newJspath = "/Employment/" + maxindex;
        $scope._new =
           {
               '_id': maxindex,
                'IsEdit': false,
                'privacy': "",
                'description': "",
                '_default': true,

                'companyName': "",
                'title': "",
                'email': "",
                'phoneNumber': "",
                'phoneCountry': "65",
                'mobileNumber': "",
                'mobileCountry': "65",
                'fax': "",
                'faxCountry': "65",
                'salary': "",
                'employmentStatus': "",

                'startDate': new Date('1960-01-01'),
                'endDate': new Date(),
                'addressLine': "",
                'addressLine_lat': "",
                'addressLine_lng': "",
                'countryCity': "",

                'state': "",
                'zipCode': "",
                'note': "",

            };

        $scope.addNewForm = function (newForm) {
            $scope._listForm.push({
                '_id': newForm._id,
                'IsEdit': false,
                'privacy': newForm.privacy,
                'description': newForm.description,
                '_default': newForm._default,
                'companyName': newForm.companyName,
                'title': newForm.title,
                'email': newForm.email,
                'phoneNumber': newForm.phoneNumber,
                'phoneCountry': newForm.phoneCountry,
                'mobileNumber': newForm.mobileNumber,
                'mobileCountry': newForm.mobileCountry,
                'fax': newForm.fax,
                'faxCountry': newForm.faxCountry,
               
                'salary': newForm.salary,
                'employmentStatus': newForm.employmentStatus,
                'endDate': newForm.endDate,
                'startDate': newForm.startDate,
                'addressLine': newForm.addressLine,
                'addressLine_lat': newForm.addressLine_lat,
                'addressLine_lng': newForm.addressLine_lng,
                'countryCity': newForm.countryCity,

                'state': newForm.state,
                'zipCode': newForm.zipCode,
                'note': newForm.note,
            });
        };

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
            var _jsp = "/Employment/" + _value._id;
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
     
    $scope.$on('employment', function () {
        $scope.InitData();
    });

    $scope.InitData();
    $scope.Edit = function () {
        $scope.IsEdit = true;
    }

    $scope.Save = function () {

        //employment
        BasicInfo.label = $scope._form._label;
        if ($scope._form._name == '') {
            $scope._form._name = $scope._form._label;

        }
        BasicInfo.name = $scope._form._name;
        BasicInfo.privacy = $scope._form._privacy;

        //
        var lstFormSave = [];
        var checkDefault = true;
        //
        $($scope._listForm).each(function (index) {

            if (checkDefault == true && $scope._listForm[index]._default == true && $scope._listForm[index].description != $scope._form._default) {
                $scope._form._default = $scope._listForm[index].description;
                checkDefault = false;
            }

        });

        //
        $($scope._listForm).each(function (index, object) {



            if (checkDefault == false && $scope._listForm[index].description != $scope._form._default) {
                $scope._listForm[index]._default = false;
            }
          
            lstFormSave.push({
                _id: $scope._listForm[index]._id,
                privacy: $scope._listForm[index].privacy,
                description: $scope._listForm[index].description,
                _default: $scope._listForm[index]._default,
                companyName: $scope._listForm[index].companyName,
                title: $scope._listForm[index].title,
                email: $scope._listForm[index].email,
                phoneNumber: $scope._listForm[index].phoneNumber,
                phoneCountry: $scope._listForm[index].phoneCountry,
              
                mobileNumber: $scope._listForm[index].mobileNumber,
                mobileCountry: $scope._listForm[index].mobileCountry,
                fax: $scope._listForm[index].fax,
                faxCountry: $scope._listForm[index].faxCountry,
                salary: $scope._listForm[index].salary,
                employmentStatus: $scope._listForm[index].employmentStatus,
                startDate: $scope._listForm[index].startDate,
                endDate: $scope._listForm[index].endDate,
                addressLine: $scope._listForm[index].addressLine,
                addressLine_lat: $scope._listForm[index].addressLine_lat,
                addressLine_lng: $scope._listForm[index].addressLine_lng,
                country: $scope._listForm[index].countryCity.country,

                city: $scope._listForm[index].countryCity.city,
                state: $scope._listForm[index].state,
                zipCode: $scope._listForm[index].zipCode,
                note: $scope._listForm[index].note

            })

            $scope._listForm[index].IsEdit = false;
        });

        BasicInfo.default = $scope._form._default;

        ///
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
        VaultInformationService.VaultInformation.employment = BasicInfo;
        VaultInformationService.VaultInformation.employment.value = lstFormSave;
        if ($scope.checkRemoveDoc == true) {
            VaultInformationService.VaultInformation.document = _document;
            VaultInformationService.VaultInformation.document.value = lstDocSave;
        }
        VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
            VaultInformationService.VaultInformation._id).then(function (response) {
                $scope.IsEdit = false;
                $scope._new = "";
                
                $rootScope.$broadcast('employment');
               
                $scope.checkRemoveDoc == false;
                $rootScope._newJspath = "";
            }, function (errors) {

            });
    }

    $scope.saveVaultInformationOnSuccess = function (response) {
        $scope.IsEdit = false;
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

