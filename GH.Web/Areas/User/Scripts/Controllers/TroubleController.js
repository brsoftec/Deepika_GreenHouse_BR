//var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'TranslationModule', 'UserModule', 'oitozero.ngSweetAlert', 'CommonDirectives', 'ui.select'], true);

var regitPublic = angular.module('regitPublic');
regitPublic.controller("TroubleController", ['$scope', '$rootScope', '$http', 'AuthorizationService', 'SweetAlert', '$timeout', function ($scope, $rootScope, $http, _authService, _sweetAlert, $timeout) {

    var regexEmail = /^(([^<>()\[\]\.,;:\s@\"]+(\.[^<>()\[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,})$/i;
    var regexPassword = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%*?&])[A-Za-z\d$@$!%*?&]{8,}/;

    $scope.progress = {
        Step: 'FIND_ACCOUNT',
        Errors: null
    };

    $scope.resetFor = {
        Email: null
    };

    $scope.otpButtonLabel = 'GET OTP PIN';
    $scope.FullQuestion = [];
    $scope.GetQuestion = function (email) {

        _authService.GetQuestionByEmail(email)
            .then(function (response) {
                $scope.FullQuestion = response;
                console.log(response);
            });
    };

    $scope.findAccount = function () {
        $scope.progress.Errors = '';

        if (!regexEmail.test($scope.resetFor.Email)) {
            $scope.progress.Errors = 'Invalid email address';
            return;
        }

        $scope.security = {
            question1: {
                question: null,
                answer: null,
                userAnswer: null
            }, question2: {
                question: null,
                answer: null,
                userAnswer: null
            }

        };

        $scope.resetOption = {
            Option: null
        };

        $http.get('/Api/Account/Query', {
            params: {
                email: $scope.resetFor.Email
            }
        }).success(function (response) {
            if (response.success) {
                $scope.unverified = response.data.unverified;
                if ($scope.unverified) {
                    $scope.resetOption.Option = 'EMAIL';
                }
            }
            else {
            }
        }).error(function (response) {
            console.log(response);
        });

        _authService.CheckDuplicatedEmail($scope.resetFor).then(function (duplicated) {
            if (duplicated) {
                if (!$scope.unverified) {
                    _authService.GetQuestionByEmail($scope.resetFor.Email)
                        .then(function (response) {
                            _authService.VerifySecurityQuestions({email: $scope.resetFor.Email})
                                .then(function (accountInfo) {
                                    accountInfo.Last2PhoneDigits = accountInfo.EncodedPhoneNumber.substring(accountInfo.EncodedPhoneNumber.length - 2, accountInfo.EncodedPhoneNumber.length);
                                    if (response.AccountStatus == 'Lock reset password') {
                                        $scope.accountInfo = accountInfo;
                                        $scope.progress.Step = 'CLOCK_RESET';

                                        $scope.progress.Errors = 'Your account has been locked out because of a maximum number of incorrect account verification attempts. You will NOT be able to change your password for 30 minutes.';
                                    }
                                    else {
                                        $scope.accountInfo = accountInfo;
                                        $scope.progress.Step = 'RESET_OPTION';


                                    }

                                }, function (errors) {
                                    console.log('Error getting account info', errors);
                                });

                        });
                }
            } else {
                $scope.progress.Errors = "We couldn't find a Regit account associated with " + $scope.resetFor.Email;
            }
        });
    };

    function validateSecurity() {
        $scope.progress.Errors = '';
        var valid = true;
        if (!$scope.security.question1.userAnswer || !$scope.security.question2.userAnswer) {
            $scope.progress.Errors = 'Please answer the question';
            valid = false;
        }

        return valid;
    }

    $scope.matchAccountWithSecurity = function () {
        var valid = validateSecurity();
        if (!valid) {
            return;
        }

        if ($scope.security.userAnswer === $scope.security.answer) {
            var model = {Email: $scope.resetFor.Email};
            _authService.VerifySecurityQuestions(model)
                .then(function (accountInfo) {
                    accountInfo.Last2PhoneDigits = accountInfo.EncodedPhoneNumber.substring(accountInfo.EncodedPhoneNumber.length - 2, accountInfo.EncodedPhoneNumber.length);
                    $scope.accountInfo = accountInfo;
                    $scope.progress.Step = 'RESET_OPTION';
                }, function (errors) {
                    $scope.progress.Errors = __errorHandler.ProcessErrors(errors).Messages.join('. ');
                });
        } else {
            $scope.progress.Errors = 'Please enter a correct answer';
        }
    };

    $scope.accountInfo = {};

    $scope.verify = {
        RequestId: null,
        Token: null
    };
    $scope.disableSend = false;
    $scope.errors = {PIN: null};
    $scope.sentOtp = false;

    $scope.resendVerifyEmail = function () {

        $http.post('/Api/Account/VerifyEmail/Resend?email='+$scope.resetFor.Email)
            .success(function (response) {
                if (response.success) {
                    $scope.resentVerifyEmail = true;
                } else {
                }
            }).error(function (response) {
            console.log(response);
        });

    };

    $scope.selectResetWay = function () {
        $scope.progress.Errors = '';
        if (!$scope.resetOption.Option) {
            $scope.progress.Errors = 'Please select a method to receive reset password code';
            return;
        }

        var model = angular.copy($scope.resetOption);
        model.VerifyInfo = angular.copy($scope.security);
        model.VerifyInfo.Email = $scope.resetFor.Email;

        if (model.Option == "QUESTION") {
            _authService.GetQuestionByEmail($scope.resetFor.Email)
                .then(function (response) {

                    if (response.AccountType == 'Business') {
                        $scope.progress.Step = 'RESET_OPTION';
                    }
                    else {
                        if (!response || !response.Question1 || !response.Question2 || !response.Question3) {
                            $scope.progress.Errors = 'Error loading security question. Please try again';
                            return;
                        }
                        var index1 = Math.ceil(Math.random() * 3);
                        var index2 = Math.ceil(Math.random() * 3);
                        while (index2 == index1)
                            index2 = Math.ceil(Math.random() * 3);

                        $scope.security.question1.question = response['Question' + index1];
                        $scope.security.question1.answer = response['Answer' + index1];
                        $scope.security.question2.question = response['Question' + index2];
                        $scope.security.question2.answer = response['Answer' + index2];
                        //$scope.progress.Step = 'SECURITY';
                        $scope.progress.Step = 'VERIFY';
                    }

                });


        }
        else {
            $scope.progress.Step = 'VERIFY';
            //phone


            $scope.GetConfirmSMS = function () {

                _authService.ForgotPassword(model)
                    .then(function (verificationRequestId) {
                        $scope.verify.RequestId = verificationRequestId;
                        $scope.disableSend = true;
                        $scope.sentOtp = true;
                    }, function (errors) {
                        var errorObj = __errorHandler.ProcessErrors(errors);
                        __errorHandler.Swal(errorObj, _sweetAlert);
                    });


                $timeout(function () {
                    if ($scope.disableSend) {
                        $scope.otpButtonLabel = 'Resend OTP';
                        $scope.disableSend = false;
                    }

                }, 60000);

            };

            $scope.checkPinSMS = function (value) {

                if (value == '') {
                    $scope.errors.PIN = 'Please enter your PIN';
                    return;
                }
                var model = angular.copy($scope.verify);
                model.Email = $scope.resetFor.Email;

                _authService.VerifyResetPasswordToken(model).then(function () {
                    $scope.progress.Step = 'RESET_PASSWORD';
                }, function (errors) {
                    $scope.progress.Errors = __errorHandler.ProcessErrors(errors).Messages.join('. ');
                });

                //_authService.CheckSetPIN($scope.authentication).then(function () {
                //    $scope.doLater();
                //}, function (errors) {
                //    $scope.errors.PIN = 'Incorrect PIN. Please re-enter';
                //})
            };

            $scope.cancelOtp = function () {
                window.location.href = '/About';
            };
            // end phone

            _authService.ForgotPassword(model).then(function (verificationRequestId) {
               $scope.progress.Step = 'VERIFY';
               $scope.verify.RequestId = verificationRequestId;
            }, function (errors) {
               $scope.progress.Errors = __errorHandler.ProcessErrors(errors).Messages.join('. ');
            })

        }


    };


    $scope.verifyCode = function () {
        $scope.progress.Errors = '';
        if (!$scope.verify.Token) {
            $scope.progress.Errors = 'Please enter verify code';
            return;
        }

        var model = angular.copy($scope.verify);
        model.Email = $scope.resetFor.Email;

        _authService.VerifyResetPasswordToken(model).then(function () {
            $scope.progress.Step = 'RESET_PASSWORD';
        }, function (errors) {
            $scope.progress.Errors = __errorHandler.ProcessErrors(errors).Messages.join('. ');
        });
    };

    var gets = 0;

    $scope.CheckSecurityQuestion = function () {
        $scope.checkAgain = false;
        var valid = validateSecurity();
        if (!valid) {
            return;
        }

        if ($scope.security.question1.userAnswer === $scope.security.question1.answer && $scope.security.question2.userAnswer === $scope.security.question2.answer) {

            $scope.verify.Token = 'Question';
            $scope.verify.RequestId = 'Question';
            $scope.progress.Step = 'RESET_PASSWORD';
        } else {

            if (gets < 10) {
                $scope.progress.Errors = "Incorrect answer. Try again.";

                gets = gets + 1;

                _authService.GetQuestionByEmail($scope.resetFor.Email)
                    .then(function (response) {

                        if (response.AccountType == 'Business') {
                            $scope.progress.Step = 'RESET_OPTION';
                        }
                        else {
                            if (!response || !response.Question1 || !response.Question2 || !response.Question3) {
                                $scope.progress.Errors = 'Error loading security question. Please try again';
                                return;
                            }
                            var index1 = Math.ceil(Math.random() * 3);
                            var index2 = Math.ceil(Math.random() * 3);
                            while (index2 == index1)
                                index2 = Math.ceil(Math.random() * 3);

                            $scope.security.question1.question = response['Question' + index1];
                            $scope.security.question1.answer = response['Answer' + index1];
                            $scope.security.question2.question = response['Question' + index2];
                            $scope.security.question2.answer = response['Answer' + index2];
                            $scope.progress.Step = 'VERIFY';
                        }

                    });
            }
            else {
                _authService.LockResetPassword($scope.resetFor.Email)
                    .then(function (response) {

                        $scope.progress.Step = 'FIND_ACCOUNT';

                        $scope.progress.Errors = 'Your account has been locked out because of a maximum number of incorrect account verification attempts. You will NOT be able to change your password for 30 minutes.';
                    });

            }
        }
    };


    $scope.resendToken = function () {
        var model = angular.copy($scope.resetOption);
        model.VerifyInfo = angular.copy($scope.security);
        model.VerifyInfo.Email = $scope.resetFor.Email;

        _authService.ResendResetPasswordToken(model).then(function (verificationRequestId) {
            $scope.progress.Step = 'VERIFY';
            $scope.verify.RequestId = verificationRequestId;
        }, function (errors) {
            $scope.progress.Errors = __errorHandler.ProcessErrors(errors).Messages.join('. ');
        });
    };

    $scope.resetPasswordModel = {
        Password: null,
        ConfirmPassword: null
    };

    $scope.resetPassword = function () {
        $scope.progress.Errors = '';

        if (!$scope.resetPasswordModel.Password) {
            $scope.progress.Errors = 'Please enter new password';
            return;
        }

        if ($scope.resetPasswordModel.Password.length < 8) {
            $scope.progress.Errors = 'Password minimum length is 8 characters';
            return;
        }

        if ($scope.resetPasswordModel.ConfirmPassword != $scope.resetPasswordModel.Password) {
            $scope.progress.Errors = 'Confirm password does not match with password';
            return;
        }

        var model = {
            Email: $scope.resetFor.Email,
            Token: $scope.verify.Token,
            RequestId: $scope.verify.RequestId,
            Password: $scope.resetPasswordModel.Password
        };

        _authService.ResetPassword(model).then(function () {
            $scope.progress.Step = 'DONE';
        }, function (errors) {
            $scope.progress.Errors = __errorHandler.ProcessErrors(errors).Messages.join('. ');
        });

    };
}]);