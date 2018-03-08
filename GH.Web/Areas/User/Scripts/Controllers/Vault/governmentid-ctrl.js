var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.cellNav'], true);

//5.0 Group GovernmentID
myApp.getController('GroupGovernmentIDController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter', 'vaultService', 'NetworkService', 'rguNotify',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter, vaultService, _networkService, rguNotify) {

    "use strict";
    var BasicInfo = VaultInformationService.VaultInformation.groupGovernmentID;
    $scope.InitData = function () {

       
    $scope.result = {};
    //$scope.sendTo = {};
    $scope.sendPush = function () {
        console.log($scope.result);
        $scope.PushToVault();
    };
    $scope.vaultGroup = {
        name: 'groupGovernmentID',
        type: 'listgroup',
        label: BasicInfo.label,
        model: BasicInfo.value
    };

    vaultService.populateGroup($scope.vaultGroup);
    
    $scope.GetAllFriends = function () {
        _networkService.GetFriends().then(function (result) {
            $scope.network = result;
        })
    }
    $scope.GetAllFriends();
    $scope.nameItem = "";
    $scope.PushToVault = function () {
        var data = new Object();
        var campaign = new Object();
        campaign.type = "ManualPushToVault";
        campaign.status = "Active";
     
        campaign.description = $scope.result.sendTo.comment;
        if (campaign.description === '')
            campaign.description = "Manual push to Vault";

        campaign.fields = [];
        data.UserId = $scope.result.sendTo.userId;
        data.CampaignType = "ManualPushToVault";
        data.Listvaults = [];

        data.pushtype = "PushToVault";
        if ($scope.result.sendTo.userId != null && $scope.result.sendTo.userId != undefined)
            data.usernotifycationid = $scope.result.sendTo.userId;
        if ($scope.result.sendTo.userEmail != "") {
            data.usernotifycationemail = $scope.result.sendTo.userEmail;
            data.pushtype = "email";
        }
       
        if ($scope.result.fields[0].jsPath != null || $scope.result.fields[0].jsPath != undefined)
        {
            var cat = $scope.result.fields[0].jsPath;
            var catArr = cat.split(".");
            var catName = catArr[1];
           
            if (catName === 'birthCertificate') {
                $scope.nameItem = "Birthday Certificate";
            } else if (catName === 'driverLicenseCard') {
                $scope.nameItem = "Driver License Card";

            } else if (catName === 'healthCard') {
                $scope.nameItem = "Health Card";

            } else if (catName === 'medicalBenefitCard') {
                $scope.nameItem = "Medical Benefit Card";
            }
            else if (catName === 'passportID') {
                $scope.nameItem = "Passport ID";
            }
            else if (catName === 'permanentResidenceCard') {
                $scope.nameItem = "Permanent Residence Card";
            }
            else if (catName === 'nationalIdCard') {
                $scope.nameItem = "National ID";
            }
            else if (catName === 'socialInsuranceCard') {
                $scope.nameItem = "Social Insurance Card";
            }
            else if (catName === 'taxID') {
                $scope.nameItem = "Tax ID";
            }
         
           
        }
        campaign.name = "Government ID" + " / " + $scope.nameItem;

        for (var index = 0; index < $scope.result.fields.length; index++) {
            if ($scope.result.fields[index].displayName == null && $scope.result.fields[index].displayName == undefined)
                $scope.result.fields[index].displayName = "";
            if ($scope.result.fields[index].optional == null && $scope.result.fields[index].optional == undefined)
                $scope.result.fields[index].optional = "";
            if ($scope.result.fields[index].type == null && $scope.result.fields[index].type == undefined)
                $scope.result.fields[index].type = "";
            if ($scope.result.fields[index].unitModel == null && $scope.result.fields[index].unitModel == undefined)
                $scope.result.fields[index].unitModel = "";
            if ($scope.result.fields[index].membership == null && $scope.result.fields[index].membership == undefined)
                $scope.result.fields[index].membership = "";

            if ($scope.result.fields[index].choices == null && $scope.result.fields[index].choices == undefined)
                $scope.result.fields[index].choices = "";
            if ($scope.result.fields[index].qa == null && $scope.result.fields[index].qa == undefined)
                $scope.result.fields[index].qa = "";
            var field = {
                id: $scope.result.fields[index].id,
                jsPath: '.' + $scope.result.fields[index].jsPath,
                displayName: $scope.result.fields[index].label,
                optional: $scope.result.fields[index].optional,
                type: $scope.result.fields[index].type,
                options: $scope.result.fields[index].options,
                model: $scope.result.fields[index].value,
                unitModel: $scope.result.fields[index].unitModel,
                value: "",
                membership: $scope.result.fields[index].membership + "",

                choices: $scope.result.fields[index].choices + "",
                qa: $scope.result.fields[index].qa + ""
            }
            //
            campaign.fields.push(field);
            //

            switch (field.type) {
                case "location":
                    var model = $scope.result.fields[index].value;
                    field.model = model.country;
                    field.unitModel = model.city;
                    break;
                case "address":
                    var model = $scope.result.fields[index].value;
                    field.model = model.address;
                    field.unitModel = model.address2;
                    break;
            }

            data.Listvaults.push(field);
        };
        var StrVaultInformation = JSON.stringify(campaign);
        data.StrVaultInformation = StrVaultInformation;
        $http.post('/api/CampaignService/ManualPushVault', data)
            .success(function (response) {
                if (response.ReturnStatus == false) {
                    swal(
                    'Push Vault Send Error!',
                    'Email not found. Push form must be sent to a valid Regit user.',
                     'error'
                  );
                    return;
                }
                rguNotify.add('Push to Vault completed.');
              
            })
            .error(function (errors) {
                var errorObj = __errorHandler.ProcessErrors(errors);
                __errorHandler.Swal(errorObj, _sweetAlert);
            });
    }

    // End push to vault
        //
        $scope.IsEdit = false;

        $scope.group = {
            _label: BasicInfo.label,
            _name: BasicInfo.name,
            _default: BasicInfo.default,
            _privacy: BasicInfo.privacy
        };

        //list group
        $scope.list_group = ["Birthday Certificate", "Driver License Card", "Health Card", "Medical Benefit Card",
        "Passport ID", "Citizenship ID", "National Identity Card", "Permanent Residence Card", "Social Insurance Card", "Tax ID"];
  
        $scope._select = { _value: '' };
        if ($scope.group._default == '') {
            $scope.group._default = "Birthday Certificate";
        }

        if ($scope._select._value == '')
            $scope._select._value = $scope.group._default;
        //

    }

    $scope.InitData();
    $scope.Edit = function () {
        $scope.IsEdit = true;
    }

    // == edit == 
    $scope.Save = function () {

        //group
        BasicInfo.label = $scope.group._label;
        if ($scope.group._name == '') {
            $scope.group._name = $scope.group._label;
        }
        BasicInfo.name = $scope.group._name;
        BasicInfo.privacy = $scope.group._privacy;
        BasicInfo.default = $scope.group._default;


        // === 
        VaultInformationService.VaultInformation.groupGovernmentID = BasicInfo;
        VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
            VaultInformationService.VaultInformation._id).then(function (response) {
                $scope.IsEdit = false;

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



//5.6 Custom  ID
myApp.getController('CustomIDController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter', 'DocumentVaultService',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter, DocumentVaultService) {

    "use strict";
    var BasicInfo = VaultInformationService.VaultInformation.groupGovernmentID.value.CustomGovernmentID;
    var _document = VaultInformationService.VaultInformation.document;
    $scope.InitData = function () {

   

        $scope.IsEdit = false;
        // $scope.IsNew = false;
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
            var _jspath = "/Government ID/Custom ID/" + BasicInfo.value[index]._id;
            var today = new Date();
            var expiryNo = false;
            // indefinite
            var _indefiniteDate = true;
            if (BasicInfo.value[index].expiryDate == undefined)
                BasicInfo.value[index].expiryDate = "";
            if (BasicInfo.value[index].expiryDate != "")
                _indefiniteDate = false;
            // End indefinite

            if (BasicInfo.value[index].countryOB == undefined)
                BasicInfo.value[index].countryOB = '';
            if (BasicInfo.value[index].dob == undefined)
                BasicInfo.value[index].dob = '';
            $scope._listForm.push({
                _id: BasicInfo.value[index]._id,
                IsEdit: false,
                privacy: BasicInfo.value[index].privacy,
                description: BasicInfo.value[index].description,

                firstName: BasicInfo.value[index].firstName,
                middleName: BasicInfo.value[index].middleName,
                lastName: BasicInfo.value[index].lastName,
                dob: BasicInfo.value[index].dob,
                nationality: BasicInfo.value[index].nationality,
                cardNumber: BasicInfo.value[index].cardNumber,
                countryOB: { country: BasicInfo.value[index].countryOB},
                issuedDate: BasicInfo.value[index].issuedDate,
                expiryDate: BasicInfo.value[index].expiryDate,
                indefiniteDate: _indefiniteDate,
                expiryNo: expiryNo,
                issuedBy: BasicInfo.value[index].issuedBy,
                issuedIn: BasicInfo.value[index].issuedIn,
              
                note: BasicInfo.value[index].note,
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

        // Get country
        //
      

        maxindex = maxindex + 1;
        $rootScope._newJspath = "/Government ID/Custom ID/" + maxindex;
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
               'dob': '',
               'countryOB': '',
               'issuedBy': "",
               'issuedIn': "",
               'issuedDate': new Date('1960-01-01'),
               'expiryDate': '',
               'indefiniteDate': true,
               'note': "",
           };


        $scope.addNewForm = function (newForm) {
            var jpd = "/Government ID/Custom ID/" + newForm._id;
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
                'dob': newForm.dob,
                'countryOB': newForm.countryOB,
                'issuedBy': newForm.issuedBy,
                'issuedIn': newForm.issuedIn,

                'docJspath': jpd,
                'note': newForm.note,

            });

        }

        // Remove form
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

        ////  
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
            var _jsp = "/Government ID/Custom ID/" + _value._id;
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
        // end Document

        //
        $scope.messageBox = "";
        $scope.alerts = [];

    }
    $scope.$on('CustomGovernmentID', function () {
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
            // end indefinite
            lstFormSave.push({
                _id: $scope._listForm[index]._id,
                privacy: $scope._listForm[index].privacy,

                description: $scope._listForm[index].description,
                firstName: $scope._listForm[index].firstName,

                middleName: $scope._listForm[index].middleName,
                lastName: $scope._listForm[index].lastName,
                nationality: $scope._listForm[index].nationality,
           
                cardNumber: $scope._listForm[index].cardNumber,
                dob: $scope._listForm[index].dob,
                countryOB: $scope._listForm[index].countryOB.country,
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

                jsPath: $scope._listdocument[index].jsPath,
                nosearch: $scope._listdocument[index].nosearch,

            })
            $scope._listdocument[index].IsEdit = false;
        });

        //End document

        // === 
        var groupName = VaultInformationService.VaultInformation.groupGovernmentID.name;
        if (groupName == "")
            VaultInformationService.VaultInformation.groupGovernmentID.name = "Government ID";

        //Save
        VaultInformationService.VaultInformation.groupGovernmentID.value.CustomGovernmentID = BasicInfo;
        VaultInformationService.VaultInformation.groupGovernmentID.value.CustomGovernmentID.value = lstFormSave;
        if ($scope.checkRemoveDoc == true) {
            VaultInformationService.VaultInformation.document = _document;
            VaultInformationService.VaultInformation.document.value = lstDocSave;
        }
        VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
            VaultInformationService.VaultInformation._id).then(function (response) {
                $scope.IsEdit = false;
                $scope._new = "";
                $rootScope.$broadcast('CustomGovernmentID');
              
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

}]);

//5.7 National Identity Card //National ID //  country // nationalID // race

myApp.getController('NationalIDController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter', 'DocumentVaultService',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter, DocumentVaultService) {

    "use strict";
    var BasicInfo = VaultInformationService.VaultInformation.groupGovernmentID.value.nationalID;
    var _document = VaultInformationService.VaultInformation.document;

    $scope.InitData = function () {

        $scope.IsEdit = false;
        // $scope.IsNew = false;
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
            var _jspath = "/Government ID/National ID/" + BasicInfo.value[index]._id;
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
                lastName: BasicInfo.value[index].lastName,
                nationality: BasicInfo.value[index].nationality,
                nationalID: BasicInfo.value[index].nationalID,
                race: BasicInfo.value[index].race,
                country: { country: BasicInfo.value[index].country },
                address: BasicInfo.value[index].address,
                issuedDate: BasicInfo.value[index].issuedDate,
                expiryDate: BasicInfo.value[index].expiryDate,
                indefiniteDate: _indefiniteDate,
                expiryNo: expiryNo,
                note: BasicInfo.value[index].note,
                docJspath: _jspath,
                _default: BasicInfo.value[index]._default

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

        // Get country
        //

        maxindex = maxindex + 1;
        $rootScope._newJspath = "/Government ID/National ID/" + maxindex;
        $scope._new =
           {
               '_id': maxindex,
               'IsEdit': false,
               'privacy': "",
               'description': "",
               '_default': true,
               'firstName': "",
              
               'lastName': "",
               'nationality': "",
               'nationalID': "",
               'race': "",
               'country': '',
               'address': '',
               'issuedDate': new Date('1960-01-01'),
               'expiryDate': '',
               'indefiniteDate': true,
               'note': "",
           };


        $scope.addNewForm = function (newForm) {
            var jpd = "/Government ID/National ID/" + newForm._id;
            if (newForm.indefiniteDate == true)
                newForm.expiryDate = '';
            $scope._listForm.push({
                '_id': newForm._id,
                'IsEdit': false,
                'privacy': newForm.privacy,
                'description': newForm.description,
                '_default': newForm._default,

                'firstName': newForm.firstName,            
                'lastName': newForm.lastName,
                'nationality': newForm.nationality,
                'nationalID': newForm.nationalID,
                'race': newForm.race,
                'issuedDate': newForm.issuedDate,
                'expiryDate': newForm.expiryDate,
                'indefiniteDate': newForm.indefiniteDate,
              
                'country': newForm.country,
                'address': newForm.address,
                'docJspath': jpd,
                'note': newForm.note,

            });

        }

        // Remove form
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

        ////  
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
            var _jsp = "/Government ID/National ID/" + _value._id;
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
        // end Document

        //
        $scope.messageBox = "";
        $scope.alerts = [];

    }
    $scope.$on('NationalGovernmentID', function () {
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
            // end indefinite
            lstFormSave.push({
                _id: $scope._listForm[index]._id,
                privacy: $scope._listForm[index].privacy,

                description: $scope._listForm[index].description,
                firstName: $scope._listForm[index].firstName,
             
                lastName: $scope._listForm[index].lastName,
                nationality: $scope._listForm[index].nationality,
                nationalID: $scope._listForm[index].nationalID,
                race: $scope._listForm[index].race,
              
                country: $scope._listForm[index].country.country,
                address: $scope._listForm[index].address,
                issuedDate: $scope._listForm[index].issuedDate,
                expiryDate: $scope._listForm[index].expiryDate,
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

                jsPath: $scope._listdocument[index].jsPath,
                nosearch: $scope._listdocument[index].nosearch,

            })
            $scope._listdocument[index].IsEdit = false;
        });

        //End document

        // === 
        var groupName = VaultInformationService.VaultInformation.groupGovernmentID.name;
        if (groupName == "")
            VaultInformationService.VaultInformation.groupGovernmentID.name = "Government ID";

        //Save nationalID
        VaultInformationService.VaultInformation.groupGovernmentID.value.nationalID = BasicInfo;
        VaultInformationService.VaultInformation.groupGovernmentID.value.nationalID.value = lstFormSave;

        if ($scope.checkRemoveDoc == true) {
            VaultInformationService.VaultInformation.document = _document;
            VaultInformationService.VaultInformation.document.value = lstDocSave;
        }
        VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
         VaultInformationService.VaultInformation._id).then(function (response) {
                $scope.IsEdit = false;
                $scope._new = "";
                //$rootScope.$broadcast('NationalGovernmentID');

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

}]);
