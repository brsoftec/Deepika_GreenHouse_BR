var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule'], true);

//1. Basic Infomation 
myApp.getController('UpdateDataVaultController',
  ['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'vaultService', 'VaultService', 'NetworkService', 'rguNotify',
        function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, vaultService, _vaultService, _networkService, rguNotify) {
            var BasicInfo = $rootScope._vaultData.basicInformation;
            var _contact = $rootScope._vaultData.contact;

            $scope.initData = function () {
                // Update contact
                if (_contact.name == '') {
                    _contact.value.mobile = {
                        '_id': '',
                        'label': 'Mobile',
                        'name': 'Mobile',
                        'rules': 'phone',
                        'type': 'textbox',
                        'privacy': true,
                        'default': '',
                        'nosearch': true,
                        'value': []
                    }

                    _contact.value.email = {
                        '_id': '',
                        'label': 'Email',
                        'name': 'Email',
                        'rules': 'email',
                        'type': 'textbox',
                        'privacy': true,
                        'default': '',
                        'nosearch': true,
                        'value': []
                    }

                    _contact.name = 'Contact';
                    _contact.sublist = true;
                    var lstMobileSave = [];
                    var lstEmailSave = [];
                    lstMobileSave.push({
                        'id': 0,
                        'value': _contact.value.profilePhone.value
                    });

                    lstEmailSave.push({
                        'id': 0,
                        'value': _contact.value.profileEmail.value,
                        'default': ""
                    });

                    var saveVaultForm = {
                        'AccountId': $rootScope.ruserid,
                        'FormName': 'contact',
                        'FormString': _contact
                    }
                    _vaultService.UpdateFormVault(saveVaultForm).then(function () {
                        $rootScope.$broadcast('UpdateVaultInformation');
                    }, function (errors) {

                    });
                }
                if (_contact.sublist == undefined) {
                    _contact.sublist = true;
                    var saveVaultForm = {
                        'AccountId': $rootScope.ruserid,
                        'FormName': 'contact',
                        'FormString': _contact
                    }
                    _vaultService.UpdateFormVault(saveVaultForm).then(function () {
                        $rootScope.$broadcast('UpdateVaultInformation');
                    }, function (errors) {

                    });
                }

                if ($rootScope._vaultData.groupGovernmentID.value.nationalID == undefined) {
                    $rootScope._vaultData.groupGovernmentID.value.nationalID =
                    {
                        'label': "National Identity Card",
                        'name': "National Identity Card",
                        'default': "",
                        'privacy': true,
                        'value': []
                    }

                    // Update Vault
                    var vm = new Object();
                    vm.vaultString = $rootScope._vaultData;
                    vm.userId = $rootScope.ruserid;
                    var vm = new Object();
                    _vaultService.UpdateVault(vm).then(function (rs) {
                        $rootScope.$broadcast('UpdateVaultInformation');
                    }, function (errors) {
                        swal('Error', errors, 'error');
                    });

                }

              

            }
            $scope.initData();

     


        }])