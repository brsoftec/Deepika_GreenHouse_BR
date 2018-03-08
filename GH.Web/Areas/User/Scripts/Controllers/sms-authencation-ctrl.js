var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule'], true);
myApp.getController('SMSAuthencationController',
    ['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'SmSAuthencationService',
        function ($scope, $rootScope, $http, userManager, sweetAlert, authService, alertService, smSAuthencationService) {

            $scope.ErrorMessagePin = "";
            $scope.PIN = "";
            $scope.PhoneNumber = "";
            $scope.RequestId = "";
            $scope.alerts = [];
            $scope.messageBox = "";

            $scope.newPin = "";
            $scope.confirmNewPin = "";
            $scope.errorNewPin = null;
            $scope.isSetPIN = false;
            $scope.checkIsSetPIN = function () {
                $http.post('/api/Account/IsSetPIN', null)
                    .success(function (resp) {
                        $scope.isSetPIN = resp;
                    });
            }
            $scope.checkIsSetPIN();
            // window.location.href = "/VaultInformation/Index";
            $scope.updatePinCode = function (pin, confirmPin) {
                $scope.errorNewPin = null;
                if (pin == "" || confirmPin == "") {
                    $scope.errorNewPin = "Enter and confirm PIN.";
                    return;
                }


                if (pin != confirmPin) {
                    $scope.errorNewPin = "Confirmed PIN not the same.";
                    return;

                }
                var pinCode = {Pin: pin};
                $http.post('/api/Account/SetPinCode', pinCode)
                    .success(function (resp) {
                        if (resp) {
                            window.location.href = "/VaultInformation/Index";
                        }
                    });
            }
            $scope.cancelUpdatePinCode = function () {
                window.location.href = "/";
            }
            // window.location.href = "/";
            $scope.SendPin = function () {
                authService.VerifyPhoneNumber({PhoneNumber: ""}).then(function (requestId) {
                    $scope.RequestId = requestId;

                }, function (errors) {
                    alertService.renderErrorMessage("Server Error");
                    $scope.messageBox = alertService.returnFormattedMessage();
                    $scope.alerts = alertService.returnAlerts();
                });

            }
            $scope.ReSendPin = function () {
                authService.VerifyPhoneNumber({PhoneNumber: $scope.PhoneNumber}).then(function (requestId) {
                    $scope.RequestId = requestId;
                    alertService.renderSuccessMessage("The PIN code has been sent to you, please check it.");
                    $scope.messageBox = alertService.returnFormattedMessage();
                    $scope.alerts = alertService.returnAlerts();
                }, function (errors) {
                    alertService.renderErrorMessage("Server Error");
                    $scope.messageBox = alertService.returnFormattedMessage();
                    $scope.alerts = alertService.returnAlerts();
                });
            };
            $scope.auth = { PIN : ''};
            $scope.Authencation = function () {

                authService.CheckPIN({PIN: $scope.auth.PIN, RequestId: $scope.RequestId}).then(function () {
                    if (smSAuthencationService.CallbackOkAuthencationSMS != undefined)
                        smSAuthencationService.CallbackOkAuthencationSMS();
                }, function (errors) {
                    alertService.renderErrorMessage("Incorrect PIN. Please re-enter");
                    $scope.messageBox = alertService.returnFormattedMessage();
                    $scope.alerts = alertService.returnAlerts();
                })
            }

            $scope.Cancel = function () {
                if (smSAuthencationService.CallbackCancelAuthencationSMS != undefined)
                    smSAuthencationService.CallbackCancelAuthencationSMS();
            }
            // $scope.SendPin();
        }])
