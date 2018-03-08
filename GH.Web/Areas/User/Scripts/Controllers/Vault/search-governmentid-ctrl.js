var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.cellNav'], true);

//5.2 Search Driver License
myApp.getController('SearchDriverLicenseController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter) {

    "use strict";
    var vault = new Object();
    var BasicInfo = new Object();
    var vm = new Object();
    if ($rootScope.delegatorid != undefined || $rootScope.delegatorid != null) {     
        vm.UserId = $rootScope.delegatorid;     
    }

    $http.post('/api/InformationVaultService/GetInfoVaultToJson', vm).then(function (response) {
            vault = response.data.VaultInformation;
            BasicInfo = vault.groupGovernmentID.value.driverLicenseCard;
           $scope.InitData();
        });
    $scope.InitData = function () {
        $scope.IsEdit = false;
        $scope._form = {
            _label: BasicInfo.label,
            _name: BasicInfo.name,
            _default: BasicInfo.default,
            _privacy: BasicInfo.privacy
        };
        $scope._listForm = [];

        $(BasicInfo.value).each(function (index, object) {

            $scope._listForm.push({
                IsEdit: false,
                privacy: BasicInfo.value[index].privacy,
                description: BasicInfo.value[index].description,

                firstName: BasicInfo.value[index].firstName,
                middleName: BasicInfo.value[index].middleName,
                lastName: BasicInfo.value[index].lastName,

                cardType: BasicInfo.value[index].cardType,
                cardNumber: BasicInfo.value[index].cardNumber,
                issuedDate: BasicInfo.value[index].issuedDate,
                expiryDate: BasicInfo.value[index].expiryDate,
                classTier: BasicInfo.value[index].classTier,
                note: BasicInfo.value[index].note,
                _default: BasicInfo.value[index]._default,

            });
            if ($scope._listForm[index].issuedDate != "" && $scope._listForm[index].issuedDate != undefined)
                $scope._listForm[index].issuedDate = new Date($scope._listForm[index].issuedDate);
            else
                $scope._listForm[index].issuedDate = new Date();

            if ($scope._listForm[index].expiryDate != "" && $scope._listForm[index].expiryDate != undefined)
                $scope._listForm[index].expiryDate = new Date($scope._listForm[index].expiryDate);
            else
                $scope._listForm[index].expiryDate = new Date();
        });


        $scope._new =
           {
               'IsEdit': false,
               'privacy': "",
               'description': "",
               '_default': true,

               'firstName': "",
               'middleName': "",
               'lastName': "",
               'cardType': "",
               'cardNumber': "",
               'classTier': "",

               'issuedDate': new Date('1960-01-01'),
               'expiryDate': new Date(),

               'note': "",

           };

        $scope.addNewForm = function (newForm) {

            $scope._listForm.push({
                'IsEdit': false,
                'privacy': newForm.privacy,
                'description': newForm.description,
                '_default': newForm._default,

                'firstName': newForm.firstName,
                'middleName': newForm.middleName,
                'lastName': newForm.lastName,
                'cardType': newForm.cardType,
                'cardNumber': newForm.cardNumber,
                'classTier': newForm.classTier,
                'issuedDate': newForm.issuedDate,
                'expiryDate': newForm.expiryDate,

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
            // var lastItem = $scope.membership_value.length - 1;

        };

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

        var checkDefault = true;
        $($scope._listForm).each(function (index) {

            if (checkDefault == true && $scope._listForm[index]._default == true && $scope._listForm[index].description != $scope._form._default) {
                $scope._form._default = $scope._listForm[index].description;
                checkDefault = false;
            }
        });

        BasicInfo.default = $scope._form._default;

        var lstFormSave = [];
        $($scope._listForm).each(function (index, object) {
            if (checkDefault == false && $scope._listForm[index].description != $scope._form._default) {
                $scope._listForm[index]._default = false;
            }
            lstFormSave.push({
                privacy: $scope._listForm[index].privacy,
                description: $scope._listForm[index].description,
                firstName: $scope._listForm[index].firstName,
                middleName: $scope._listForm[index].middleName,
                lastName: $scope._listForm[index].lastName,
                cardType: $scope._listForm[index].cardType,
                cardNumber: $scope._listForm[index].cardNumber,
                issuedDate: $scope._listForm[index].issuedDate,
                expiryDate: $scope._listForm[index].expiryDate,

                classTier: $scope._listForm[index].classTier,
                note: $scope._listForm[index].note,
                _default: $scope._listForm[index]._default,

            })
            $scope._listForm[index].IsEdit = false;
        });

        //save
        vault.groupGovernmentID.value.driverLicenseCard = BasicInfo;
        vault.groupGovernmentID.value.driverLicenseCard.value = lstFormSave;
        VaultInformationService.SaveVaultInformation(vault,
            vault._id).then(function (response) {
                $scope.IsEdit = false;
                $scope._new = "";
             
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

//5.3 Search Health Card
myApp.getController('SearchHealthCardController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter) {

    "use strict";
     var vault = new Object();
    var BasicInfo = new Object();
    var vm = new Object();
    if ($rootScope.delegatorid != undefined || $rootScope.delegatorid != null) {     
        vm.UserId = $rootScope.delegatorid;     
    }

    $http.post('/api/InformationVaultService/GetInfoVaultToJson', vm).then(function (response) {
            vault = response.data.VaultInformation;
            BasicInfo = vault.groupGovernmentID.value.healthCard;
        //run to set data
           $scope.InitData();
        });

   // var BasicInfo = VaultInformationService.VaultInformation.groupGovernmentID.value.healthCard;
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

        $(BasicInfo.value).each(function (index, object) {

            $scope._listForm.push({
                IsEdit: false,
                privacy: BasicInfo.value[index].privacy,
                description: BasicInfo.value[index].description,

                firstName: BasicInfo.value[index].firstName,
                middleName: BasicInfo.value[index].middleName,
                lastName: BasicInfo.value[index].lastName,

                cardNumber: BasicInfo.value[index].cardNumber,
                bloodType: BasicInfo.value[index].bloodType,
                issuedDate: BasicInfo.value[index].issuedDate,
                expiryDate: BasicInfo.value[index].expiryDate,

                note: BasicInfo.value[index].note,
                _default: BasicInfo.value[index]._default,

            });
            if ($scope._listForm[index].issuedDate != "" && $scope._listForm[index].issuedDate != undefined)
                $scope._listForm[index].issuedDate = new Date($scope._listForm[index].issuedDate);
            else
                $scope._listForm[index].issuedDate = new Date();

            if ($scope._listForm[index].expiryDate != "" && $scope._listForm[index].expiryDate != undefined)
                $scope._listForm[index].expiryDate = new Date($scope._listForm[index].expiryDate);
            else
                $scope._listForm[index].expiryDate = new Date();
        });


        $scope._new =
           {
               'IsEdit': false,
               'privacy': "",
               'description': "",
               '_default': true,

               'firstName': "",
               'middleName': "",
               'lastName': "",
               'cardNumber': "",
               'bloodType': "O",


               'issuedDate': new Date('1960-01-01'),
               'expiryDate': new Date(),

               'note': "",

           };


        $scope.addNewForm = function (newForm) {

            $scope._listForm.push({
                'IsEdit': false,
                'privacy': newForm.privacy,
                'description': newForm.description,
                '_default': newForm._default,

                'firstName': newForm.firstName,
                'middleName': newForm.middleName,
                'lastName': newForm.lastName,

                'cardNumber': newForm.cardNumber,
                'bloodType': newForm.bloodType,

                'issuedDate': newForm.issuedDate,
                'expiryDate': newForm.expiryDate,


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
                cancelButtonText: "No!",
                closeOnConfirm: false,
                closeOnCancel: true
            }, function (isConfirm) {
                if (isConfirm) {

                    swal('OK', '', 'success');
                    _value.removed = true;
                    $scope._listForm.splice(index, 1);
                    $scope.Save();
                }
            });
            // var lastItem = $scope.membership_value.length - 1;

        };

        ////  

        //
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
            if (checkDefault == false && $scope._listForm[index].description != $scope._form._default) {
                $scope._listForm[index]._default = false;
            }
            lstFormSave.push({
                privacy: $scope._listForm[index].privacy,

                description: $scope._listForm[index].description,
                firstName: $scope._listForm[index].firstName,

                middleName: $scope._listForm[index].middleName,
                lastName: $scope._listForm[index].lastName,

                cardNumber: $scope._listForm[index].cardNumber,
                bloodType: $scope._listForm[index].bloodType,
                issuedDate: $scope._listForm[index].issuedDate,
                expiryDate: $scope._listForm[index].expiryDate,


                note: $scope._listForm[index].note,
                _default: $scope._listForm[index]._default,

            })
            $scope._listForm[index].IsEdit = false;
        });

        //save
        vault.groupGovernmentID.value.healthCard = BasicInfo;
        vault.groupGovernmentID.value.healthCard.value = lstFormSave;
        VaultInformationService.SaveVaultInformation(vault,
            vault._id).then(function (response) {
                $scope.IsEdit = false;
                $scope._new = "";
              
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

//5.4 Search Medical Card
myApp.getController('SearchMedicalCardController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter) {

    "use strict";
      var vault = new Object();
    var BasicInfo = new Object();
    var vm = new Object();
    if ($rootScope.delegatorid != undefined || $rootScope.delegatorid != null) {     
        vm.UserId = $rootScope.delegatorid;     
    }

    $http.post('/api/InformationVaultService/GetInfoVaultToJson', vm).then(function (response) {
            vault = response.data.VaultInformation;
            BasicInfo = vault.groupGovernmentID.value.medicalBenefitCard;
        //run to set data
           $scope.InitData();
        });
    //var BasicInfo = VaultInformationService.VaultInformation.groupGovernmentID.value.medicalBenefitCard;
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

        $(BasicInfo.value).each(function (index, object) {

            $scope._listForm.push({
                IsEdit: false,
                privacy: BasicInfo.value[index].privacy,
                description: BasicInfo.value[index].description,

                firstName: BasicInfo.value[index].firstName,
                middleName: BasicInfo.value[index].middleName,
                lastName: BasicInfo.value[index].lastName,

                cardNumber: BasicInfo.value[index].cardNumber,

                issuedDate: BasicInfo.value[index].issuedDate,
                expiryDate: BasicInfo.value[index].expiryDate,
                tier: BasicInfo.value[index].tier,
                note: BasicInfo.value[index].note,
                _default: BasicInfo.value[index]._default,

            });
            if ($scope._listForm[index].issuedDate != "" && $scope._listForm[index].issuedDate != undefined)
                $scope._listForm[index].issuedDate = new Date($scope._listForm[index].issuedDate);
            else
                $scope._listForm[index].issuedDate = new Date();

            if ($scope._listForm[index].expiryDate != "" && $scope._listForm[index].expiryDate != undefined)
                $scope._listForm[index].expiryDate = new Date($scope._listForm[index].expiryDate);
            else
                $scope._listForm[index].expiryDate = new Date();
        });


        $scope._new =
           {
               'IsEdit': false,
               'privacy': "",
               'description': "",
               '_default': true,

               'firstName': "",
               'middleName': "",
               'lastName': "",
               'cardNumber': "",
               'tier': "",

               'issuedDate': new Date('1960-01-01'),
               'expiryDate': new Date(),
               'note': "",
           };

        $scope.addNewForm = function (newForm) {

            $scope._listForm.push({
                'IsEdit': false,
                'privacy': newForm.privacy,
                'description': newForm.description,
                '_default': newForm._default,

                'firstName': newForm.firstName,
                'middleName': newForm.middleName,
                'lastName': newForm.lastName,

                'cardNumber': newForm.cardNumber,
                'tier': newForm.tier,
                'issuedDate': newForm.issuedDate,
                'expiryDate': newForm.expiryDate,

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
                cancelButtonText: "No!",
                closeOnConfirm: false,
                closeOnCancel: true
            }, function (isConfirm) {
                if (isConfirm) {

                    swal('OK', '', 'success');
                    _value.removed = true;
                    $scope._listForm.splice(index, 1);
                    $scope.Save();
                }
            });
            // var lastItem = $scope.membership_value.length - 1;
        };

        //
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
            if (checkDefault == false && $scope._listForm[index].description != $scope._form._default) {
                $scope._listForm[index]._default = false;
            }
            lstFormSave.push({
                privacy: $scope._listForm[index].privacy,

                description: $scope._listForm[index].description,
                firstName: $scope._listForm[index].firstName,

                middleName: $scope._listForm[index].middleName,
                lastName: $scope._listForm[index].lastName,

                cardNumber: $scope._listForm[index].cardNumber,

                issuedDate: $scope._listForm[index].issuedDate,
                expiryDate: $scope._listForm[index].expiryDate,
                tier: $scope._listForm[index].tier,

                note: $scope._listForm[index].note,
                _default: $scope._listForm[index]._default,

            })
            $scope._listForm[index].IsEdit = false;
        });
    
        //save
        vault.groupGovernmentID.value.medicalBenefitCard = BasicInfo;
        vault.groupGovernmentID.value.medicalBenefitCard.value = lstFormSave;
        VaultInformationService.SaveVaultInformation(vault,
            vault._id).then(function (response) {
                $scope.IsEdit = false;
                $scope._new = "";
             
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



//5.6 Search Custom  ID
myApp.getController('SearchCustomIDController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter) {

    "use strict";
         var vault = new Object();
    var BasicInfo = new Object();
    var vm = new Object();
    if ($rootScope.delegatorid != undefined || $rootScope.delegatorid != null) {     
        vm.UserId = $rootScope.delegatorid;     
    }

    $http.post('/api/InformationVaultService/GetInfoVaultToJson', vm).then(function (response) {
            vault = response.data.VaultInformation;
            BasicInfo = vault.groupGovernmentID.value.CustomGovernmentID;
        //run to set data
           $scope.InitData();
        });
   // var BasicInfo = VaultInformationService.VaultInformation.groupGovernmentID.value.passportID;
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

        $(BasicInfo.value).each(function (index, object) {

            $scope._listForm.push({
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

                issuedBy: BasicInfo.value[index].issuedBy,
                issuedIn: BasicInfo.value[index].issuedIn,
                email: BasicInfo.value[index].email,
                addressLine: BasicInfo.value[index].addressLine,

                countryCity: { country: BasicInfo.value[index].country, city: BasicInfo.value[index].city },
                city: BasicInfo.value[index].city,
                note: BasicInfo.value[index].note,
                _default: BasicInfo.value[index]._default,

            });
            if ($scope._listForm[index].issuedDate != "" && $scope._listForm[index].issuedDate != undefined)
                $scope._listForm[index].issuedDate = new Date($scope._listForm[index].issuedDate);
            else
                $scope._listForm[index].issuedDate = new Date();

            if ($scope._listForm[index].expiryDate != "" && $scope._listForm[index].expiryDate != undefined)
                $scope._listForm[index].expiryDate = new Date($scope._listForm[index].expiryDate);
            else
                $scope._listForm[index].expiryDate = new Date();
        });


        $scope._new =
           {
               'IsEdit': false,
               'privacy': "",
               'description': "",
               '_default': true,

               'firstName': "",
               'middleName': "",
               'lastName': "",
               'nationality': "",
               'cardNumber': "",
               'issuedBy': "",
               'issuedIn': "",
               'email': "",
               'phone': "",
               'countryCity': "",
               'address': "",
               'issuedDate': new Date('1960-01-01'),
               'expiryDate': new Date(),
               'note': "",
           };


        $scope.addNewForm = function (newForm) {

            $scope._listForm.push({
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

                'issuedBy': newForm.issuedBy,
                'issuedIn': newForm.issuedIn,

                'email': newForm.email,
                'phone': newForm.phone,
                'countryCity': newForm.countryCity,
                'address': newForm.address,

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
                cancelButtonText: "No!",
                closeOnConfirm: false,
                closeOnCancel: true
            }, function (isConfirm) {
                if (isConfirm) {

                    swal('OK', '', 'success');
                    _value.removed = true;
                    $scope._listForm.splice(index, 1);
                    $scope.Save();
                }
            });
            // var lastItem = $scope.membership_value.length - 1;

        };

        ////  

        //
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
            if (checkDefault == false && $scope._listForm[index].description != $scope._form._default) {
                $scope._listForm[index]._default = false;
            }
            lstFormSave.push({
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
                address: $scope._listForm[index].address,
                email: $scope._listForm[index].email,
                phone: $scope._listForm[index].phone,
                country: $scope._listForm[index].countryCity.country,
                city: $scope._listForm[index].countryCity.city,
                _default: $scope._listForm[index]._default,

            })
            $scope._listForm[index].IsEdit = false;
        });

        //Save
        //VaultInformationService.VaultInformation.groupGovernmentID.value.customGovernment = BasicInfo;
        //save
        vault.groupGovernmentID.value.CustomGovernmentID = BasicInfo;
        vault.groupGovernmentID.value.CustomGovernmentID.value = lstFormSave;
        VaultInformationService.SaveVaultInformation(vault,
            vault._id).then(function (response) {
                $scope.IsEdit = false;
                $scope._new = "";
             
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

}]);
