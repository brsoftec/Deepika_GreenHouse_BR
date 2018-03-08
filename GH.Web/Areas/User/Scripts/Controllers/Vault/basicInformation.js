var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule'], true);

//1. Basic Infomation 
myApp.getController('basicInformationController',
  ['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'vaultService', 'NetworkService', 'rguNotify', 'VaultService',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, vaultService, _networkService, rguNotify, _vaultService) {
            var vaultData = null;
            $scope.formData = null;
            $scope.formData2 = null;
            var FormName = 'basicInformation';
            $scope.InitForm = function () {
                var formVault = {
                    'FormName': FormName,
                    'AccountId': ''
                }
                _vaultService.GetFormVault(formVault).then(function (response) {
                    vaultData = response;
                    $scope.formData = response;
                    $scope.Init();
                }, function (errors) {

                });
            }
            $scope.InitForm();

            $scope.Init = function()
            {
                $scope.lstField = [];
                var i = 0;
                for (var field in vaultData.value) {
                    i++;
                    var _field = { 'name': field, 'data': vaultData.value[field] }
                    if (typeof (_field.data._id) !== 'number')
                        _field.data._id = i;
                    $scope.lstField.push(_field);
                }
                
                for (var i = 0; i < $scope.lstField.length; i++) {
                    vaultData.value[$scope.lstField[i].Name] = $scope.lstField[i].data;    
                }
                $scope.formData2 = vaultData;
            }
         

        }])
