var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule'], true);

//SMSEmailAuthencationPoPupController
myApp.controller('SMSEmailAuthencationPoPupController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'SmSAuthencationService', '$uibModalInstance', '$timeout',
function ($scope, $rootScope, $http, userManager, sweetAlert, authService, alertService, smSAuthencationService, $uibModalInstance, $timeout) {

    //phone
    $scope.sms = {
        buttonLabel: 'Send PIN via SMS',
        requestId: '',
        disable: false
      
    }

    $scope.email = {
        buttonLabel: 'Send PIN via Email',
        requestId: '',
        disable: false
       
    }
    $scope.verify = {
        option: 'sms',
        error: null,
        sent: false,
        pin:''
    }
    $scope.getOtpSms = function()
    {
        $scope.sms.disable = true;
        $scope.verify.sent = true;
        $scope.verify.pin = '';
        $scope.verify.option = 'sms';
        $scope.verify.error = null;
        var option = $scope.verify.option;
        var requestId = $scope.sms.requestId;
        var urlApi = '';
        if (requestId != '')
            urlApi = '/api/account/otp/getcode?option=' + option + '&requestId=' + requestId;
        else
            urlApi = '/api/account/otp/getcode?option=' + option;

        $http.get(urlApi)
           .success(function (response) {

               $scope.sms.requestId = response;
           },
           function (errors) {
               $scope.verify.error = "Can not send PIN to your phone";
           });

        $timeout(function () {
            if ($scope.sms.disable) {
                $scope.sms.buttonLabel = 'Resend PIN via SMS';
                $scope.sms.disable = false;
            }

        }, 60000);
    }

    $scope.getOtpEmail = function () {
        $scope.email.disable = true;
        $scope.verify.sent = true;
        $scope.verify.pin = '';
        $scope.verify.option = 'email';
        $scope.verify.error = null;
        var option = $scope.verify.option;
        var requestId = $scope.email.requestId;

        if (requestId != '')
            urlApi = '/api/account/otp/getcode?option=' + option + '&requestId=' + requestId;
        else
            urlApi = '/api/account/otp/getcode?option=' + option;
        $http.get(urlApi)
           .success(function (response) {
               $scope.email.requestId = response;
           },
           function (errors) {
               $scope.verify.error = "Can not send PIN to your email";
           });

        $timeout(function () {
            if ($scope.email.disable) {
                $scope.email.buttonLabel = 'Resend PIN via Email';
                $scope.email.disable = false;
            }

        }, 60000);

    }

  

    $scope.verifyOtpSms = function(value)
    {
        
        $scope.verifyOtp(value, 'sms');

    }
    $scope.verifyOtpEmail = function (value) {

        $scope.verifyOtp(value, 'email');

    }

    $scope.verifyOtp = function (value) {
        if (value == '')
        {
            $scope.verify.error = "Enter PIN code";
            return;
        }
        var data = new Object();
       
        data.Code = value;
        data.RequestId = $scope.sms.requestId;
        if ($scope.verify.option == 'email')
                data.RequestId = $scope.email.requestId;

        $http.post('/api/account/otp/verifycode', data)
              .success(function (response) {
                  if (response.Verify)
                      $uibModalInstance.close("OK");
                  else
                      $scope.verify.error = response.Status;

              }, function (errors) {
                  $scope.verify.error = "Incorrect PIN. Please re-enter.";
              });
    };

    $scope.cancelOtp = function () {
        $uibModalInstance.dismiss('cancel');
    }
    // end phone

}])

//SMSAuthencationPoPupController
myApp.controller('SMSAuthencationPoPupController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'SmSAuthencationService', '$uibModalInstance',
function ($scope, $rootScope, $http, userManager, sweetAlert, authService, alertService, smSAuthencationService, $uibModalInstance) {
    $scope.ErrorMessagePin = "";
    $scope.PIN = "";
    $scope.PhoneNumber = "";
    $scope.RequestId = "";
    $scope.alerts = [];
    $scope.messageBox = "";

    $scope.SendPin = function () {
        authService.VerifyPhoneNumber({ PhoneNumber: "" }).then(function (requestId) {
            $scope.RequestId = requestId;

        }, function (errors) {
            alertService.renderErrorMessage("Server Error");
            $scope.messageBox = alertService.returnFormattedMessage();
            $scope.alerts = alertService.returnAlerts();
        });
    }
    $scope.ReSendPin = function () {
        authService.VerifyPhoneNumber({ PhoneNumber: $scope.PhoneNumber }).then(function (requestId) {
            $scope.RequestId = requestId;
            alertService.renderSuccessMessage("The PIN code has been sent to you, please check it.");
            $scope.messageBox = alertService.returnFormattedMessage();
            $scope.alerts = alertService.returnAlerts();
        }, function (errors) {
            alertService.renderErrorMessage("Server Error");
            $scope.messageBox = alertService.returnFormattedMessage();
            $scope.alerts = alertService.returnAlerts();
        });

    }
    $scope.Authencation = function () {

        authService.CheckSetPIN({ PIN: $scope.PIN, RequestId: $scope.RequestId }).then(function () {
            $uibModalInstance.close("OK");
        }, function (errors) {
            alertService.renderErrorMessage("Incorrect PIN. Please re-enter");
            $scope.messageBox = alertService.returnFormattedMessage();
            $scope.alerts = alertService.returnAlerts();
        })
    }
    $scope.Cancel = function () {
        $uibModalInstance.dismiss('cancel');
    }
    $scope.SendPin();

}])



myApp.controller('SMSAuthencationPoPupNoSessionController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'SmSAuthencationService', '$uibModalInstance', 'registerPopup',
function ($scope, $rootScope, $http, userManager, sweetAlert, authService, alertService, smSAuthencationService, $uibModalInstance, registerPopup) {
    $scope.ErrorMessagePin = "";
    $scope.PIN = "";
    $scope.PhoneNumber = "";

    if (!(registerPopup == undefined || registerPopup == "" || registerPopup == null))
        $scope.PhoneNumber = registerPopup.newphonenumber;
    else
        $scope.PhoneNumber = "";
    $scope.RequestId = "";
    $scope.alerts = [];
    $scope.messageBox = "";
    $scope.SendPin = function () {
        authService.VerifyPhoneNumber({ PhoneNumber: $scope.PhoneNumber }).then(function (requestId) {
            $scope.RequestId = requestId;

        }, function (errors) {
            alertService.renderErrorMessage("Server Error");
            $scope.messageBox = alertService.returnFormattedMessage();
            $scope.alerts = alertService.returnAlerts();
        });
    }
    $scope.ReSendPin = function () {
        authService.VerifyPhoneNumber({ PhoneNumber: $scope.PhoneNumber }).then(function (requestId) {
            $scope.RequestId = requestId;
            alertService.renderSuccessMessage("The PIN code has been sent to you, please check it.");
            $scope.messageBox = alertService.returnFormattedMessage();
            $scope.alerts = alertService.returnAlerts();
        }, function (errors) {
            alertService.renderErrorMessage("Server Error");
            $scope.messageBox = alertService.returnFormattedMessage();
            $scope.alerts = alertService.returnAlerts();
        });

    }
    $scope.Authencation = function () {

        authService.CheckSetPIN({ PIN: $scope.PIN, RequestId: $scope.RequestId }).then(function () {
            $uibModalInstance.close("OK");
        }, function (errors) {
            alertService.renderErrorMessage("Incorrect PIN. Please re-enter");
            $scope.messageBox = alertService.returnFormattedMessage();
            $scope.alerts = alertService.returnAlerts();
        })
    }
    $scope.Cancel = function () {
        $uibModalInstance.dismiss('cancel');
    }
    $scope.SendPin();

}])
myApp.controller('SMSAuthencationPoPupVaultController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'SmSAuthencationService', '$uibModalInstance',
function ($scope, $rootScope, $http, userManager, sweetAlert, authService, alertService, smSAuthencationService, $uibModalInstance) {
    $scope.ErrorMessagePin = "";
    $scope.PIN = "";
    $scope.PhoneNumber = "";
    $scope.RequestId = "";
    $scope.alerts = [];
    $scope.messageBox = "";
    $scope.SendPin = function () {
        authService.VerifyPhoneNumber({ PhoneNumber: "" }).then(function (requestId) {
            $scope.RequestId = requestId;

        }, function (errors) {
            alertService.renderErrorMessage("Server Error");
            $scope.messageBox = alertService.returnFormattedMessage();
            $scope.alerts = alertService.returnAlerts();
        });
    }
    $scope.ReSendPin = function () {
        authService.VerifyPhoneNumber({ PhoneNumber: $scope.PhoneNumber }).then(function (requestId) {
            $scope.RequestId = requestId;
            alertService.renderSuccessMessage("The PIN code has been sent to you, please check it.");
            $scope.messageBox = alertService.returnFormattedMessage();
            $scope.alerts = alertService.returnAlerts();
        }, function (errors) {
            alertService.renderErrorMessage("Server Error");
            $scope.messageBox = alertService.returnFormattedMessage();
            $scope.alerts = alertService.returnAlerts();
        });

    }
    $scope.Authencation = function () {

        authService.CheckPIN({ PIN: $scope.PIN, RequestId: $scope.RequestId }).then(function () {
            $uibModalInstance.close("OK");
        }, function (errors) {
            alertService.renderErrorMessage("Incorrect PIN. Please re-enter");
            $scope.messageBox = alertService.returnFormattedMessage();
            $scope.alerts = alertService.returnAlerts();
        })
    }
    $scope.Cancel = function () {
        $uibModalInstance.dismiss('cancel');
    }
    $scope.SendPin();

}])

// Vu 
myApp.controller('PinAuthencationPoPupController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'SmSAuthencationService', '$uibModalInstance',
function ($scope, $rootScope, $http, userManager, sweetAlert, authService, alertService, smSAuthencationService, $uibModalInstance) {
    $scope.ErrorMessagePin = "";
    $scope.PIN = "";
    $scope.PhoneNumber = "";
    $scope.RequestId = "";
    $scope.alerts = [];
    $scope.messageBox = "";

    $scope.SendPin = function () {
        authService.VerifyPhoneNumber({ PhoneNumber: "" }).then(function (requestId) {
            $scope.RequestId = requestId;

        }, function (errors) {
            alertService.renderErrorMessage("Server Error");
            $scope.messageBox = alertService.returnFormattedMessage();
            $scope.alerts = alertService.returnAlerts();
        });

    }
    $scope.ReSendPin = function () {
        authService.VerifyPhoneNumber({ PhoneNumber: $scope.PhoneNumber }).then(function (requestId) {
            $scope.RequestId = requestId;
            alertService.renderSuccessMessage("The PIN code has been sent to you, please check it.");
            $scope.messageBox = alertService.returnFormattedMessage();
            $scope.alerts = alertService.returnAlerts();
        }, function (errors) {
            alertService.renderErrorMessage("Server Error");
            $scope.messageBox = alertService.returnFormattedMessage();
            $scope.alerts = alertService.returnAlerts();
        });
    }
    $scope.Authencation = function () {

        authService.CheckPIN({ PIN: $scope.PIN, RequestId: $scope.RequestId }).then(function () {
            $uibModalInstance.close("OK");
            if (smSAuthencationService.CallbackOkAuthencationSMS != undefined)
                smSAuthencationService.CallbackOkAuthencationSMS();
        }, function (errors) {
            alertService.renderErrorMessage("Incorrect PIN. Please re-enter");
            $scope.messageBox = alertService.returnFormattedMessage();
            $scope.alerts = alertService.returnAlerts();
        })
    }

    $scope.Cancel = function () {
        $uibModalInstance.dismiss('cancel');
    }
    // $scope.SendPin();

}])

