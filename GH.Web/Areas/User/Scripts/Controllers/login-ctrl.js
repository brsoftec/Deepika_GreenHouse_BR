var myApp = angular.module('regitPublic');
myApp.run(['$rootScope', '$cookies', 'AuthorizationService', function ($rootScope, $cookies, _authService) {
    $rootScope.authorized = false;

    $rootScope.authorized = _authService.IsAuthorized();
}]);

myApp.controller("LoginController", ['$scope', '$rootScope', '$http', 'AuthorizationService', 'SweetAlert', '$cookies', 'CommonService', function ($scope, $rootScope, $http, _authService, _sweetAlert, $cookies, CommonService) {
    $scope.providers = [];
    $scope.provider = {};
    $scope.externalRegisterInfo = { Email: null };
    $scope.signModel = {
        Email: null,
        Password: null,
        ConfirmPassword: null
    };
    $scope.forgotModel = { Email: null }

    _authService.GetExternalLoginProviders().then(function (providers) {
        $scope.providers = providers;
    });

    $scope.externalLogin = function (provider) {
        _authService.ExternalLogin(provider).then(function (fragment) {
            var authToken = fragment.token_type + ' ' + fragment.access_token;
            _authService.GetUserAuthInfo(authToken).then(function (authInfo) {
                if (authInfo.HasRegistered) {
                    _authService.Auth(authInfo.AccessToken);
                    $rootScope.authorized = _authService.IsAuthorized();
                    window.location = "/";
                } else {
                    $cookies.put('register_external', authToken, { path: '/' });
                    $cookies.putObject('external_provider', provider, { path: '/' });
                    window.location.href = '/User/SignUp';
                }
            })
        })
    };

    $scope.signIn = function () {
        var re = /^(([^<>()\[\]\.,;:\s@\"]+(\.[^<>()\[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,})$/i;
        if (!re.test($scope.signModel.Email)) {
            __common.swal(_sweetAlert, 'error', $rootScope.translate('Invalid_Email'), 'error');
            return;
        }
        if (!$scope.signModel.Password || $scope.signModel.Password.trim().length < 6) {
            __common.swal(_sweetAlert, 'error', $rootScope.translate('Invalid_Password'), 'error');
            return;
        }
        _authService.SignInLocal($scope.signModel.Email, $scope.signModel.Password)
            .then(function () {
                var urlreturn = CommonService.GetQuerystring("ReturnUrl");
                $rootScope.authorized = _authService.IsAuthorized();


                if (!urlreturn) {
                    urlreturn = '/';
                }

                window.location.href = urlreturn;

            },
                function (errors) {
                console.log(errors)
                    if (errors.error_description == 'EMAIL_NOT_VERIFIED') {
                        window.location.href = '/User/VerifyingEmail?email=' + $scope.signModel.Email;
                    } else if (errors.error_description == 'DISABLE_USER_BY_MAIL') {
                        window.location.href = '/User/DisableUserByEmail?email=' + $scope.signModel.Email;
                    } else {
                        __common.swal(_sweetAlert, 'errors', errors.error_description, 'error');
                    }
                });
    };

    $scope.signUp = function () {
        window.location.href = '/User/SignUp'
    };

    $rootScope.resendEmailVerify = function (email) {
        _authService.SendVerifyEmail(email)
            .then(function () {
                __common.swal(_sweetAlert, $rootScope.translate('success'), $rootScope.translate('New_Verification_Email_Sent'), 'success');
            },
                function (errors) {
                    __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
                });
    }
}])

myApp.controller("SigninBusinessController", ['$scope', '$rootScope', '$http', 'AuthorizationService', 'SweetAlert', '$cookies', function ($scope, $rootScope, $http, _authService, _sweetAlert, $cookies) {
    $scope.providers = [];
    $scope.provider = {};
    $scope.externalRegisterInfo = { Email: null };
    $scope.signModel = {
        Email: null,
        Password: null,
        ConfirmPassword: null
    };
    $scope.forgotModel = { Email: null };

    _authService.GetExternalLoginProviders().then(function (providers) {
        $scope.providers = providers;
    });

    $scope.externalLogin = function (provider) {
        //try to login with external provider
        _authService.ExternalLogin(provider).then(function (fragment) {
            var authToken = fragment.token_type + ' ' + fragment.access_token;
            //login with external provider success - go to process info at green house
            //check authentication info for register new or logged in
            _authService.GetUserAuthInfo(authToken).then(function (authInfo) {
                //if user has registered before, save access token and redirect to homepage
                if (authInfo.HasRegistered) {
                    _authService.Auth(authInfo.AccessToken);
                    $rootScope.authorized = _authService.IsAuthorized();
                    window.location = "/";
                } else {
                    //if user does not have an account, prepare token for sign up and redirect to sign up page
                    $cookies.put('register_external', authToken, { path: '/' });
                    $cookies.putObject('external_provider', provider, { path: '/' });
                    window.location.href = '/BusinessAccount/SignUp';
                }
            })
        })
    };

    $scope.signIn = function () {
        var re = /^(([^<>()\[\]\.,;:\s@\"]+(\.[^<>()\[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,})$/i;
        if (!re.test($scope.signModel.Email)) {
            // __common.swal(_sweetAlert, 'error', $rootScope.translate('Invalid_Email'), 'error');
            return;
        }
        if (!$scope.signModel.Password || $scope.signModel.Password.trim().length < 6) {
            //  __common.swal(_sweetAlert, 'error', $rootScope.translate('Invalid_Password'), 'error');
            return;
        }
        _authService.SignInLocal($scope.signModel.Email, $scope.signModel.Password)
            .then(function () {
                console.log('a')
                // var urlreturn = CommonService.GetQuerystring("ReturnUrl");
                $rootScope.authorized = _authService.IsAuthorized();
                //EnableSMSAuthencation
                $http.post('/Api/Account/IsBusinessAccount', null)
                    .success(function (resp) {
                        if (resp) {
                            _authService.SendSMS().then(function (rs) {
                                if (rs)
                                    window.location.href = '/BusinessAccount/SignIn?requestId=' + rs;
                                else
                                    window.location.href = '/BusinessAccount/SignIn';
                            });
                        }
                        else
                            window.location.href = '/';
                    });



            },
                function (errors) {

                console.log(errors)

                    if (errors.error_description == 'EMAIL_NOT_VERIFIED') {
                        window.location.href = '/User/VerifyingEmail?email=' + $scope.signModel.Email;
                    } else if (errors.error_description == 'DISABLE_USER_BY_MAIL') {
                        window.location.href = '/User/DisableUserByEmail?email=' + $scope.signModel.Email;
                    } else {
                        //  __common.swal(_sweetAlert, 'errors', errors.error_description, 'error');
                    }
                });
    };

    $scope.signUp = function () {
        window.location.href = '/BusinessAccount/SignUp'
    };

    $rootScope.resendEmailVerify = function (email) {
        _authService.SendVerifyEmail(email)
            .then(function () {
                //   __common.swal(_sweetAlert, $rootScope.translate('success'), $rootScope.translate('New_Verification_Email_Sent'), 'success');
            },
                function (errors) {
                    //  __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
                });
    }
}])

myApp.controller("ConfirmSMSController", ['$scope', '$rootScope', '$http', 'AuthorizationService', '$cookies', '$timeout', function ($scope, $rootScope, $http, _authService, $cookies, $timeout) {

    
   //sms login new
    $scope.confirmSMS =
       {
           StaticPIN: '',
           RequestId: '',
           PIN: ''
       };

    $scope.otpButtonLabel = 'GET OTP PIN';
    $scope.disableSend = false;
    $scope.errors = { PIN: null };
    $scope.sentOtp = false;
    $scope.GetConfirmSMS = function () {
        _authService.SendSMS().then(function (rs) {
            $scope.confirmSMS.RequestId = rs;
            $scope.disableSend = true;
            $scope.sentOtp = true;

        }, function (errors) {
            $scope.errors.PIN = 'Send SMS error';
        })

        $timeout(function () {
            if ($scope.disableSend) {
                $scope.otpButtonLabel = 'Resend OTP';
                $scope.disableSend = false;
            }

        }, 60000);

    }


    $scope.checkPinSMS = function () {
     
        if ($scope.confirmSMS.PIN == '') {
            $scope.errors.PIN = 'Please enter your PIN';
            return;
        }

        var model = {
            StaticPIN: '',
            RequestId: $scope.confirmSMS.RequestId,
            PIN: $scope.confirmSMS.PIN
        }
        _authService.CheckSetPIN(model).then(function () {
            window.location.href = "/";
        }, function (errors) {

            $scope.errors.PIN = 'Incorrect PIN. Please re-enter';
        })
    };

    $scope.cancelOtp = function () {

        $http.post('/api/Account/Logout', null)
                .success(function (resp) {
                    window.location.href = '/';
                });

    }

}])

//SMSEmailAuthencationPoPupController
myApp.controller('ConfirmSMSEmailController',
['$scope', '$rootScope', '$http',  'SweetAlert', 'AuthorizationService', '$timeout',
function ($scope, $rootScope, $http, sweetAlert, authService,  $timeout) {

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
                      window.location.href = "/";
                  else
                      $scope.verify.error = response.Status;

              }, function (errors) {
                  $scope.verify.error = "Incorrect PIN. Please re-enter.";
              });
    };

    $scope.cancelOtp = function () {
        $http.post('/api/Account/Logout', null)
              .success(function (resp) {
                  window.location.href = '/';
              });
    }
    // end phone

}])
