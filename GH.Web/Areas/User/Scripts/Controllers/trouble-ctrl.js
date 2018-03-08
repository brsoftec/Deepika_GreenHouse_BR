var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'TranslationModule', 'UserModule', 'oitozero.ngSweetAlert', 'CommonDirectives', 'ui.select'], true);

myApp.getController("TroubleController", ['$scope', '$rootScope', '$http', 'AuthorizationService', 'SweetAlert', function ($scope, $rootScope, $http, _authService, _sweetAlert) {

    var regexEmail = /^(([^<>()\[\]\.,;:\s@\"]+(\.[^<>()\[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,})$/i;
    var regexPassword = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%*?&])[A-Za-z\d$@$!%*?&]{8,}/;

    $scope.progress = {
        Step: 'FIND_ACCOUNT',
        Errors: null
    }

    $scope.resetFor = {
        Email: null
    }

    $scope.findAccount = function () {
        $scope.progress.Errors = '';
        if (!regexEmail.test($scope.resetFor.Email)) {
            $scope.progress.Errors = 'Invalid email address';
            return;
        }

        _authService.CheckDuplicatedEmail($scope.resetFor).then(function (duplicated) {
            if (duplicated) {
                $scope.progress.Step = 'SECURITY';
            } else {
                $scope.progress.Errors = "We couldn't find a Regit account associated with " + $scope.resetFor.Email;
            }
        })
    }


    $scope.securityQuestions = [];

    _authService.GetAllSecurityQuestions().then(function (questions) {
        $scope.securityQuestions = questions;
    })

    $scope.security = {
        Question1: {
            QuestionId: null,
            Answer: null
        },
        Question2: {
            QuestionId: null,
            Answer: null
        },
        Question3: {
            QuestionId: null,
            Answer: null
        }
    }

    function validateSecurity() {
        $scope.progress.Errors = '';
        var valid = true;

        if (!$scope.security.Question1.QuestionId || !$scope.security.Question1.Answer || !$scope.security.Question2.QuestionId || !$scope.security.Question2.Answer || !$scope.security.Question3.QuestionId || !$scope.security.Question3.Answer) {
            $scope.progress.Errors = 'Please select and answer all questions';
            valid = false;
        }

        return valid;
    }

    $scope.matchAccountWithSecurity = function () {
        var valid = validateSecurity();
        if (!valid) {
            return;
        }
        var model = angular.copy($scope.security);
        model.Email = $scope.resetFor.Email;
        _authService.VerifySecurityQuestions(model)
            .then(function (accountInfo) {
                accountInfo.Last2PhoneDigits = accountInfo.EncodedPhoneNumber.substring(accountInfo.EncodedPhoneNumber.length - 2, accountInfo.EncodedPhoneNumber.length);
                $scope.accountInfo = accountInfo;
                $scope.progress.Step = 'RESET_OPTION';
            }, function (errors) {
                $scope.progress.Errors = __errorHandler.ProcessErrors(errors).Messages.join('. ');
            })
    }

    $scope.accountInfo = {};

    $scope.resetOption = {
        Option: null
    }

    $scope.selectResetWay = function () {
        $scope.progress.Errors = '';
        if (!$scope.resetOption.Option) {
            $scope.progress.Errors = 'Please select a method to receive reset password code';
            return;
        }

        var model = angular.copy($scope.resetOption);
        model.VerifyInfo = angular.copy($scope.security);
        model.VerifyInfo.Email = $scope.resetFor.Email;

        _authService.ForgotPassword(model).then(function (verificationRequestId) {
            $scope.progress.Step = 'VERIFY';
            $scope.verify.RequestId = verificationRequestId;
        }, function (errors) {
            $scope.progress.Errors = __errorHandler.ProcessErrors(errors).Messages.join('. ');
        })
    }

    $scope.verify = {
        RequestId: null,
        Token: null
    }

    $scope.verifyCode = function () {
        $scope.progress.Errors = '';
        if (!$scope.verify.Token) {
            $scope.progress.Errors = 'Please enter verify code';
            return;
        }

        var model = angular.copy($scope.verify);
        model.Email = $scope.resetFor.Email;

        _authService.VerifyResetPasswordToken(model).then(function () {
            $scope.progress.Step = 'RESET_PASSWORD'
        }, function (errors) {
            $scope.progress.Errors = __errorHandler.ProcessErrors(errors).Messages.join('. ');
        })
    }

    $scope.resendToken = function () {
        var model = angular.copy($scope.resetOption);
        model.VerifyInfo = angular.copy($scope.security);
        model.VerifyInfo.Email = $scope.resetFor.Email;

        _authService.ResendResetPasswordToken(model).then(function (verificationRequestId) {
            $scope.progress.Step = 'VERIFY';
            $scope.verify.RequestId = verificationRequestId;
        }, function (errors) {
            $scope.progress.Errors = __errorHandler.ProcessErrors(errors).Messages.join('. ');
        })
    }

    $scope.resetPasswordModel = {
        Password: null,
        ConfirmPassword: null
    }

    $scope.resetPassword = function () {
        $scope.progress.Errors = '';

        if (!$scope.resetPasswordModel.Password) {
            $scope.progress.Errors = 'Please enter new password';
            return;
        }

        if ($scope.resetPasswordModel.Password.length < 8) {
            $scope.progress.Errors = 'Password minimum length is 8 characters.';
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
        }

        _authService.ResetPassword(model).then(function () {
            $scope.progress.Step = 'DONE';
        }, function (errors) {
            $scope.progress.Errors = __errorHandler.ProcessErrors(errors).Messages.join('. ');
        })
    }
}])