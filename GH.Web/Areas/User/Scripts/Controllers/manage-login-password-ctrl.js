myApp.getController('ManageLoginPasswordController', ['$scope', '$rootScope', '$http', 'UserManagementService', 'AuthorizationService', 'SweetAlert', 'SmSAuthencationService', '$timeout', function ($scope, $rootScope, $http, _userManager, _authService, _sweetAlert, SmSAuthencationService, $timeout) {
    $scope.configs = {
        ShowPinAuthenticateModal: false
    }

    //sms
    $scope.sms = { enable: true }
    $scope.setSMSOTP = function () {
        var data = new Object();

        data.Value = 'disable';
        if ($scope.sms.enable)
            data.Value = 'enable';
        $http.post('/api/BusinessAccount/setotp', data)
        .success(function (response) {

        })
        .error(function (errors, status) {

        })
    }

    $scope.getSMSOTP = function () {
        $scope.sms.enable = false;
        $http.get('/api/BusinessAccount/getotp')
       .success(function (response) {
           if (response.Value == "enable")
               $scope.sms.enable = true;
       })
       .error(function (errors, status) {

       })
    }
    $scope.getSMSOTP();


    $scope.pinAuthenticate = {
        PIN: null,
        Authenticated: false,
        PinSent: false,
        RequestId: null,
        VerifiedToken: null,
        VerifyNewPhone: false,
        SentNewPhone: null
    }

    $scope.showPinAuthenticateModal = function (model) {
        if ($scope.pinAuthenticate.PinSent) {
            $scope.configs.ShowPinAuthenticateModal = true;
        } else {
            _authService.VerifyPhoneNumber(model).then(function (requestId) {
                $scope.pinAuthenticate.RequestId = requestId;
                $scope.pinAuthenticate.PinSent = true;
                $scope.configs.ShowPinAuthenticateModal = true;
            }, function (errors) {
                __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
            })
        }
    }

    $scope.resendPIN = function () {
        var model = null;
        if ($scope.pinAuthenticate.VerifyNewPhone) {
            model = { PhoneNumber: $scope.changePhoneNumberModel.NewPhoneNumber }
        }

        _authService.VerifyPhoneNumber(model).then(function (requestId) {
            $scope.pinAuthenticate.RequestId = requestId;
            $scope.pinAuthenticate.PinSent = true;
            $scope.configs.ShowPinAuthenticateModal = true;
            __common.swal(_sweetAlert, $rootScope.translate('Ok_Title'), $rootScope.translate('New_PIN_has_been_sent'), 'success');
        }, function (errors) {
            __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
        })
    }

    $scope.onPinAuthenticateModalHidden = function () {
        $scope.pinAuthenticate.PIN = null;
        $scope.pinAuthenticate.VerifyNewPhone = false;
    }

    $scope.authenticatePin = function () {
        if ($scope.pinAuthenticate.VerifyNewPhone) {
            $scope.saveNewPhoneNumber();
        } else {
            _authService.VerifyPIN({ RequestId: $scope.pinAuthenticate.RequestId, PIN: $scope.pinAuthenticate.PIN })
                .then(function (verifiedToken) {
                    $scope.pinAuthenticate.PIN = null;
                    $scope.pinAuthenticate.RequestId = null;
                    $scope.pinAuthenticate.PinSent = false;
                    $scope.pinAuthenticate.Authenticated = true;
                    $scope.pinAuthenticate.VerifiedToken = verifiedToken;
                    $scope.configs.ShowPinAuthenticateModal = false;
                }, function (errors) {
                    __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
                })
        }
    }

    $scope.editting = {};

    $scope.edit = function (field) {
        var arg = arguments;
        if (field === 'PhoneNumber' && !$scope.encodedPhoneNumber
            || field === 'SecurityQuestion' && !$scope.securityQuestionsAnswers.Question1.QuestionId) {
            $scope.editting[field] = true;
        } else {
            SmSAuthencationService.OpenPopupAuthencationSMSEmail(function () {
                $timeout(function () {
                    var edittingFields = '';
                    for (var i = 0; i < arg.length; i++) {
                        edittingFields += arg[i];
                    }

                    $scope.editting[edittingFields] = true;
                });
            });
        }
    }

    $scope.hasLocalPassword = null;
    _userManager.HasLocalPassword().then(function (hasLocalPassword) {
        $scope.hasLocalPassword = hasLocalPassword;
    })

    $scope.changePasswordModel = {
        VerifiedToken: '',
        OldPassword: '',
        NewPassword: '',
        ConfirmPassword: ''
    }

    $scope.cancelChangePassword = function () {
        $scope.changePasswordModel = {

            VerifiedToken: '',
            OldPassword: '',
            NewPassword: '',
            ConfirmPassword: ''
        }
        $scope.editting.Password = false;
    }
    $scope.changePassword = function () {

        if ($scope.hasLocalPassword) {
            if (!$scope.changePasswordModel.OldPassword || $scope.changePasswordModel.OldPassword.length < 8) {
                __common.swal(_sweetAlert, $rootScope.translate('Warning_Title'),
                    $rootScope.translate('Old_password_invalid'),
                    'warning');
                return;
            }
        }

        if (!$scope.changePasswordModel.NewPassword || $scope.changePasswordModel.NewPassword.length < 8) {
            __common.swal(_sweetAlert, $rootScope.translate('Warning_Title'),
                $rootScope.translate('Password_Error_Invalidated'),
                'warning');
            return;
        }

        if ($scope.changePasswordModel.NewPassword != $scope.changePasswordModel.ConfirmPassword) {
            __common.swal(_sweetAlert, $rootScope.translate('Warning_Title'),
                $rootScope.translate('Confirm_New_Password_Invalid_Message'),
                'warning');
            return;
        }

        _userManager.ChangePassword($scope.changePasswordModel, $scope.pinAuthenticate.VerifiedToken).then(function () {
            __common.swal(_sweetAlert, $rootScope.translate('Ok_Title'),
                $rootScope.translate('Change_password_success'),
                'success');
            $scope.hasLocalPassword = true;
            $scope.editting.Password = false;
            $scope.changePasswordModel = { OldPassword: null, NewPassword: null, ConfirmPassword: null };
        }, function (errors) {
            var errorObj = __errorHandler.ProcessErrors(errors);
            if (errorObj.Codes.indexOf(902001) >= 0) {
                $scope.pinAuthenticate.VerifyNewPhone = false;
                $scope.pinAuthenticate.PinSent = false;
                $scope.pinAuthenticate.Authenticated = false;
                $scope.pinAuthenticate.SentNewPhone = null;
                $scope.showPinAuthenticateModal();
            }
            __errorHandler.Swal(errorObj, _sweetAlert);
        })
    }
    /* Change Phone number */
    $scope.countries = [];
    $http.get('/Content/sources/countries.js').success(function (countries) {
        $scope.countries = countries;
    })

    $scope.encodedPhoneNumber = null;
    _userManager.GetEncodedPhoneNumber().then(function (phoneNumber) {
        $scope.encodedPhoneNumber = phoneNumber;
    })

    $scope.changePhoneNumberModel = {
        NewPhoneNumber: '',
        ConfirmNewPhoneNumber: '',
    }

    $scope.cancelChangePhoneNumber = function () {
        $scope.changePhoneNumberModel = {
            NewPhoneNumber: '',
            ConfirmNewPhoneNumber: '',
        }
        $scope.editting.PhoneNumber = false;
    }

    $scope.changePhoneNumber = function () {
        if (!$scope.changePhoneNumberModel.NewPhoneNumberCountryCallingCode || !$scope.changePhoneNumberModel.NewPhoneNumberCountryCallingCode[0]) {
            __common.swal(_sweetAlert, $rootScope.translate('Error_Title'),
                $rootScope.translate('Country_Error_Required'),
                'error');
            return;
        }
        if (!$scope.changePhoneNumberModel.NewPhoneNumber) {
            __common.swal(_sweetAlert, $rootScope.translate('Error_Title'),
                $rootScope.translate('Phone_Number_Error_Required'),
                'error');
            return;
        }
        if (!$scope.changePhoneNumberModel.ConfirmNewPhoneNumberCountryCallingCode || !$scope.changePhoneNumberModel.ConfirmNewPhoneNumberCountryCallingCode[0]) {
            __common.swal(_sweetAlert, $rootScope.translate('Error_Title'),
                $rootScope.translate('Phone_Number_Error_Required'),
                'error');
            return;
        }

        var formatedNumber = '+' + $scope.changePhoneNumberModel.NewPhoneNumberCountryCallingCode[0] + $scope.changePhoneNumberModel.NewPhoneNumber;
        var formatedConfirmNumber = '+' + $scope.changePhoneNumberModel.ConfirmNewPhoneNumberCountryCallingCode[0] + $scope.changePhoneNumberModel.ConfirmNewPhoneNumber;
        if (formatedNumber != formatedConfirmNumber) {
            __common.swal(_sweetAlert, $rootScope.translate('Error_Title'),
                $rootScope.translate('Confirm_New_Phone_Number_Error_Required'),
                'error');
            return;
        }

        $scope.saveNewPhoneNumber();
        //SmSAuthencationService.OpenPopupAuthencationSMSNoSession(formatedNumber, function () {
        //    $timeout(function () {
        //        $scope.saveNewPhoneNumber();
        //    });
        //});

           
    }
      $scope.newPhonePIN = {
        PIN: null,
        RequestId: null
    }

    $scope.saveNewPhoneNumber = function () {
        var formatedNumber = '+' + $scope.changePhoneNumberModel.NewPhoneNumberCountryCallingCode[0] + $scope.changePhoneNumberModel.NewPhoneNumber;
        var formatedConfirmNumber = '+' + $scope.changePhoneNumberModel.ConfirmNewPhoneNumberCountryCallingCode[0] + $scope.changePhoneNumberModel.ConfirmNewPhoneNumber;
        _userManager.UpdatePhoneNumber({ NewPhoneNumber: formatedNumber, ConfirmNewPhoneNumber: formatedConfirmNumber }, $scope.pinAuthenticate.PIN, $scope.pinAuthenticate.RequestId, $scope.pinAuthenticate.VerifiedToken).then(function (phoneNumber) {
            __common.swal(_sweetAlert, $rootScope.translate('Ok_Title'),
                $rootScope.translate('Change_phone_number_success'),
                'success');
            $scope.encodedPhoneNumber = phoneNumber;
            $scope.changePhoneNumberModel = {
                NewPhoneNumber: '',
                ConfirmNewPhoneNumber: '',
            }
            $scope.editting.PhoneNumber = false;
            $scope.configs.ShowPinAuthenticateModal = false;
            $scope.pinAuthenticate.PinSent = false;
            $scope.pinAuthenticate.PIN = null;
            $scope.pinAuthenticate.SentNewPhone = null;
            $scope.newPhonePIN.PIN = null;
            $scope.newPhonePIN.RequestId = null;
        }, function (errors) {
            var errorObj = __errorHandler.ProcessErrors(errors);
            if (errorObj.Codes.indexOf(902001) >= 0) {
                $scope.newPhonePIN.PIN = $scope.pinAuthenticate.PIN;
                $scope.newPhonePIN.RequestId = $scope.pinAuthenticate.RequestId;
                $scope.pinAuthenticate.VerifyNewPhone = false;
                $scope.pinAuthenticate.PinSent = false;
                $scope.pinAuthenticate.Authenticated = false;
                $scope.pinAuthenticate.PIN = null;
                $scope.showPinAuthenticateModal();
            }
            __errorHandler.Swal(errorObj, _sweetAlert);
        })
    }
/* End Change Phone number */

    /* Pin Code */
    $scope.encodedPinCode = null;
    _userManager.GetEncodedPinCode().then(function (pinCode) {
        $scope.encodedPinCode = pinCode;
    })

    $scope.changePinCodeModel = {
        NewPinCode: '',
        ConfirmNewPinCode: '',
    }

    $scope.cancelChangePinCode = function () {
        $scope.changePinCodeModel = {
            NewPinCode: '',
            ConfirmNewPinCode: '',
        }
        $scope.editting.PinCode = false;
    }

    $scope.changePinCode = function () {


        var formatedPinCode = $scope.changePinCodeModel.NewPinCode;
        var formatedConfirmPinCode = $scope.changePinCodeModel.ConfirmNewPinCode;
        if (formatedPinCode != formatedConfirmPinCode) {
            __common.swal(_sweetAlert, $rootScope.translate('Error_Title'),
                $rootScope.translate('Confirm_New_Pin_Code_Error_Required'),
                'error');
            return;
        }

        _userManager.UpdatePinCode($scope.changePinCodeModel, $scope.pinAuthenticate.VerifiedToken)
            .then(function () {
                __common.swal(_sweetAlert, $rootScope.translate('Ok_Title'),
                    $rootScope.translate('Change_PIN_Code_Success_Message'),
                    'success');
                // $scope.encodedPinCode = angular.copy($scope.changePinCodeModel);
                $scope.editting.PinCode = false;
            },
                function (errors) {
                    var errorObj = __errorHandler.ProcessErrors(errors);
                    if (errorObj.Codes.indexOf(902001) >= 0) {
                        $scope.pinAuthenticate.VerifyNewPhone = false;
                        $scope.pinAuthenticate.PinSent = false;
                        $scope.pinAuthenticate.Authenticated = false;
                        $scope.pinAuthenticate.SentNewPhone = null;
                        $scope.showPinAuthenticateModal();
                    }
                    __errorHandler.Swal(errorObj, _sweetAlert);
                });
    }


    /* End Pin Code */

    $scope.securityQuestions = [];

    _authService.GetAllSecurityQuestions().then(function (questions) {
        $scope.securityQuestions = questions;
    })

    $scope.changeSecurityQuestionsModel = {}

    $scope.securityQuestionsAnswers = {};

    _userManager.GetSecurityQuestionsAnswers().then(function (answers) {
        $scope.securityQuestionsAnswers = answers;
        $scope.changeSecurityQuestionsModel = angular.copy(answers);
    })
    $scope.errors = {};
    function validateSecurityQuestionsAnswers() {
        $scope.errors = {}
        var valid = true;

        if (!$scope.changeSecurityQuestionsModel.Question1.QuestionId) {
            $scope.errors.Question1 = $rootScope.translate('Security_Question_Error_Required');
            valid = false;
        } else if (!$scope.changeSecurityQuestionsModel.Question1.Answer) {
            $scope.errors.Answer1 = $rootScope.translate('Security_Question_Answer_Error_Required');
            valid = false;
        }

        if (!$scope.changeSecurityQuestionsModel.Question2.QuestionId) {
            $scope.errors.Question2 = $rootScope.translate('Security_Question_Error_Required');
            valid = false;
        } else if (!$scope.changeSecurityQuestionsModel.Question2.Answer) {
            $scope.errors.Answer2 = $rootScope.translate('Security_Question_Answer_Error_Required');
            valid = false;
        }

        if (!$scope.changeSecurityQuestionsModel.Question3.QuestionId) {
            $scope.errors.Question3 = $rootScope.translate('Security_Question_Error_Required');
            valid = false;
        } else if (!$scope.changeSecurityQuestionsModel.Question3.Answer) {
            $scope.errors.Answer3 = $rootScope.translate('Security_Question_Answer_Error_Required');
            valid = false;
        }

        return valid;
    }

    $scope.changeSecurityQuestions = function (changeSecurityQuestionsModel) {
        var valid = validateSecurityQuestionsAnswers();
        if (!valid) {
            return;
        }

        _userManager.UpdateSecurityQuestionsAnswers(changeSecurityQuestionsModel, $scope.pinAuthenticate.VerifiedToken)
            .then(function () {
                __common.swal(_sweetAlert, $rootScope.translate('Ok_Title'),
                    $rootScope.translate('Change_Security_Questions_Success_Message'),
                    'success');
                $scope.securityQuestionsAnswers = angular.copy(changeSecurityQuestionsModel);
                $scope.editting.SecurityQuestion = false;
            },
                function (errors) {
                    var errorObj = __errorHandler.ProcessErrors(errors);
                    if (errorObj.Codes.indexOf(902001) >= 0) {
                        $scope.pinAuthenticate.VerifyNewPhone = false;
                        $scope.pinAuthenticate.PinSent = false;
                        $scope.pinAuthenticate.Authenticated = false;
                        $scope.pinAuthenticate.SentNewPhone = null;
                        $scope.showPinAuthenticateModal();
                    }
                    __errorHandler.Swal(errorObj, _sweetAlert);
                });
    }

    $scope.cancelChangeSecurityQuestions = function () {
        $scope.changeSecurityQuestionsModel = angular.copy($scope.securityQuestionsAnswers);
        $scope.editting.SecurityQuestion = false;
    }

    $scope.encodedEmail = '';

    _userManager.GetEncodedEmail(true).then(function (encodedEmail) {
        $scope.encodedEmail = encodedEmail;
    })

    $scope.changeEmailModel = {
        Email: '',
        Password: ''
    }

    $scope.emailError = '';
    $scope.duplicated = false;
    $scope.validEmail = false;
    $scope.checkDuplicatedEmail = function () {
        var regexEmail = /^(([^<>()\[\]\.,;:\s@\"]+(\.[^<>()\[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,})$/i;
        if (regexEmail.test($scope.changeEmailModel.Email)) {
            $scope.validEmail = true;
            _authService.CheckDuplicatedEmail({ Email: $scope.changeEmailModel.Email, IgnoreCurrent: true }, true)
                .then(function (duplicated) {
                    $scope.duplicated = duplicated;
                    if (duplicated) {
                        $scope.emailError = $rootScope.translate('Email_Error_Not_Available');
                    } else {
                        $scope.emailError = '';
                    }
                }, function (errors) {
                    $scope.emailError = __errorHandler.ProcessErrors(errors).Messages.join('. ');
                    $scope.validEmail = false;
                })
        } else {
            $scope.validEmail = false;
            $scope.emailError = $rootScope.translate('Email_Error_Invalid');
        }
    }

    $scope.changeEmail = function () {
        if ($scope.validEmail && !$scope.duplicated) {
            _userManager.UpdateEmail({ Email: $scope.changeEmailModel.Email, Password: $scope.changeEmailModel.Password, VerifiedToken: $scope.pinAuthenticate.VerifiedToken })
                .then(function (encodedEmail) {
                    $scope.encodedEmail = encodedEmail;
                    $scope.editting.Email = false;
                    $scope.changeEmailModel = {
                        Email: '',
                        Password: ''
                    }
                }, function (errors) {
                    var errorObj = __errorHandler.ProcessErrors(errors);
                    if (errorObj.Codes.indexOf(902001) >= 0) {
                        $scope.pinAuthenticate.VerifyNewPhone = false;
                        $scope.pinAuthenticate.PinSent = false;
                        $scope.pinAuthenticate.Authenticated = false;
                        $scope.pinAuthenticate.SentNewPhone = null;
                        $scope.showPinAuthenticateModal();
                    }
                    __errorHandler.Swal(errorObj, _sweetAlert);
                })
        }
    }

    $scope.cancelChangeEmail = function () {
        $scope.editting.Email = false;
        $scope.changeEmailModel = {
            Email: '',
            Password: ''
        }
    }

}])
