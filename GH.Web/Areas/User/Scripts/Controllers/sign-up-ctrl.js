var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'TranslationModule', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'ui.bootstrap', 'DataModule'], true);

myApp.getController("SignUpController",
[
    '$scope', '$rootScope', '$http', 'AuthorizationService', 'SweetAlert', 'UserManagementService', 'DataService',
    function ($scope, $rootScope, $http, _authService, _sweetAlert, _userManager, _dataService) {

      
        //check if current process is register external
        $scope.registerExternal = _authService.IsSignUpExternal();
    
        $scope.location = null;
        var provider = null;
        console.log('a')
        $scope.account = {
            FirstName: null,
            LastName: null,
            Email: null,
            confirmEmail: null,
            PhoneNumber: null,
            PhoneNumberCountryCallingCode: null,
            Password: null,
            ConfirmPassword: null,
            City: 'Singapore',
            Country: 'Singapore',
            Gender: null,
            Birthday: null,
            Avatar: null,
            FileName: null
        }

        if ($scope.registerExternal) {
            provider = _authService.GetExternalProvider();

            //Get sign up external info
            _authService.GetSignUpExternal()
                .then(function (info) {
                    //if there is an exist account associate with external login email, then link external login with exist account. Else go to sign up process
                    if (info.Skip) {
                        registerExt();
                    } else {
                        $scope.account = {
                            FirstName: info.FirstName,
                            LastName: info.LastName,
                            Email: info.Email.toLowerCase(),
                            confirmEmail: null,
                            PhoneNumber: null,
                            PhoneNumberCountryCallingCode: null,
                            DisplayName: info.DisplayName,
                            Avatar: info.Avatar,
                            City: null,
                            Country: null,
                            Gender: null,
                            Birthday: null,
                            FileName: null
                        }
                    }
                });
        }


        $scope.progress = {
            Step: 'REGISTER',
            Errors: {
                Step1: {
                    FirstName: null,
                    LastName: null,
                    Email: null,
                    ConfirmEmail: null,
                    PhoneNumber: null,
                    City: null,
                    Country: null,
                    Password: null,
                    ConfirmPassword: null
                },
                Step2: {
                    PIN: null
                },
                Step3: {
                    Question1: null,
                    Question2: null,
                    Question3: null,
                    Answer1: null,
                    Answer2: null,
                    Answer3: null
                },
                Step4: {
                    Birthday: null,
                    City: null,
                    Country: null,
                    Gender: null
                }
            }
        }

        $scope.countries = [];
        $http.get('/Content/sources/countries.js')
            .success(function (countries) {
                $scope.countries = countries;
            });

        function validateStep1() {
            if (!$scope.location)
            {
                $scope.account.Country = $scope.location.country;
                $scope.account.City = $scope.location.city;
            }
            $scope.location = null;
            $scope.progress.Errors.Step1 = {
                FirstName: null,
                LastName: null,
                Email: null,
                ConfirmEmail: null,
                PhoneNumber: null,
                City: null,
                Country: null,
                Password: null,
                ConfirmPassword: null
            }
            var valid = true;
            var regexEmail =
                /^(([^<>()\[\]\.,;:\s@\"]+(\.[^<>()\[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,})$/i;
            var regexPassword = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%*^#?&$($$)$])[A-Za-z\d$@$!%^*#?&]{8,}/;

            if (!$scope.registerExternal) {
                if (!$scope.account.Password) {
                    $scope.progress.Errors.Step1.Password = $rootScope.translate('Password_Error_Required');
                    valid = false;
                } else if (!regexPassword.test($scope.account.Password)) {
                    $scope.progress.Errors.Step1.Password = $rootScope.translate('Password_Error_Invalidated');
                    valid = false;
                }

                if (!$scope.account.ConfirmPassword) {
                    $scope.progress.Errors.Step1.ConfirmPassword = $rootScope
                        .translate('ConfirmPassword_Error_Required');
                    valid = false;
                } else if (!regexPassword.test($scope.account.ConfirmPassword)) {
                    $scope.progress.Errors.Step1.ConfirmPassword = $rootScope
                        .translate('ConfirmPassword_Error_Invalidated');
                    valid = false;
                } else if ($scope.account.ConfirmPassword !== $scope.account.Password) {
                    $scope.progress.Errors.Step1.ConfirmPassword = $rootScope
                        .translate('ConfirmPassword_Error_Not_Same_Password');
                    valid = false;
                }
            }

            if (!$scope.account.Email) {
                $scope.progress.Errors.Step1.Email = $rootScope.translate('Email_Error_Required');
                valid = false;
            } else if (!regexEmail.test($scope.account.Email)) {
                $scope.progress.Errors.Step1.Email = $rootScope.translate('Email_Error_Invalid');
                valid = false;
            } 

            if (!$scope.account.FirstName) {
                $scope.progress.Errors.Step1.FirstName = $rootScope.translate('First_Name_Error_Required');
                valid = false;
            }
            if (!$scope.account.LastName) {
                $scope.progress.Errors.Step1.LastName = $rootScope.translate('Last_Name_Error_Required');
                valid = false;
            }
            if (!$scope.account.PhoneNumber) {
                $scope.progress.Errors.Step1.PhoneNumber = $rootScope.translate('Phone_Number_Error_Required');
                valid = false;
            }
            if (!$scope.account.Country) {
                $scope.progress.Errors.Step1.Country = $rootScope.translate('Phone_Number_Error_Required');
                valid = false;
            }
            if (!$scope.account.City) {
                $scope.progress.Errors.Step1.City = $rootScope.translate('Phone_Number_Error_Required');
                valid = false;
            }
            if (!$scope.account.PhoneNumberCountryCallingCode || !$scope.account.PhoneNumberCountryCallingCode[0]) {
                $scope.progress.Errors.Step1.PhoneNumberCountryCallingCode = $rootScope
                    .translate('Phone_Number_Error_Country_Code_Required');
                valid = false;
            }
            return valid;
        }
       
        $scope.register = function () {
            if ($scope.outsiteId != '')
            {
                _authService.getOutSiteById($scope.outsiteId)
              .then(function (rs) {

                  $scope.outSite = rs;
              },
                  function (errors) {
                     console.log('Sign up register error', errors)
                  });

            }
            
            var valid = validateStep1();
            if (!valid) {
                return;
            }

            var model = angular.copy($scope.account);
            model.PhoneNumberCountryCallingCode = model.PhoneNumberCountryCallingCode[0];

            if (!$scope.registerExternal) {
                _authService.ValidateRegistrationInfo(model)
                    .then(function () {
                        _authService.VerifyPhoneNumber({
                            PhoneNumber: model.PhoneNumberCountryCallingCode + model.PhoneNumber
                        })
                            .then(function (requestId) {
                                $scope.authentication.RequestId = requestId;
                                $scope.progress.Step = 'AUTHENTICATION';
                            },
                                function (errors) {
                                    var errorObj = __errorHandler.ProcessErrors(errors);
                                    __errorHandler.Swal(errorObj, _sweetAlert);
                                });
                    },
                        function (errors) {
                            var errorObj = __errorHandler.ProcessErrors(errors);
                            __errorHandler.Swal(errorObj, _sweetAlert);
                        });
            } else {
                _authService.ValidateExternalRegistrationInfo(model)
                    .then(function () {
                        _authService.VerifyPhoneNumber({
                            PhoneNumber: model.PhoneNumberCountryCallingCode + model.PhoneNumber
                        })
                            .then(function (requestId) {
                                $scope.authentication.RequestId = requestId;
                                $scope.progress.Step = 'AUTHENTICATION';
                            },
                                function (errors) {
                                    var errorObj = __errorHandler.ProcessErrors(errors);
                                    __errorHandler.Swal(errorObj, _sweetAlert);
                                });
                    },
                        function (errors) {
                            var errorObj = __errorHandler.ProcessErrors(errors);
                            __errorHandler.Swal(errorObj, _sweetAlert);
                        });
            }
        }


        
    // Resend pin
    $scope.ReSendPin = function () {
        var phone = $scope.account.PhoneNumberCountryCallingCode + $scope.account.PhoneNumber;
        _authService.VerifyPhoneNumber({ PhoneNumber: phone }).then(function (requestId) {
            $scope.authentication.RequestId = requestId;

        }, function (errors) {
            if (errors === "Concurrent verifications to the same number are not allowed") {
                __common.swal(_sweetAlert, "warning", "Concurrent verifications to the same number are not allowed", 'warning');

            } else {
                var errorObj = __errorHandler.ProcessErrors(errors);
                __errorHandler.Swal(errorObj, _sweetAlert);
            }
        });
    }
        //
        $scope.authentication = {
            PIN: null,
            RequestId: null,
            StaticPIN: null
        }

        function validateStep2() {
            $scope.progress.Errors.Step2 = {
                PIN: null
            }
            var valid = true;

            if (!$scope.authentication.PIN) {
                $scope.progress.Errors.Step2.PIN = $rootScope.translate('PIN_Error_Required');
                valid = false;
            }

            return valid;
        }

        $scope.authenticate = function () {
            var valid = validateStep2();

            if (!valid) {
                return;
            }
          
           
            _authService.CheckSetPIN($scope.authentication)
                .then(function () {
                  
                    $scope.progress.Step = 'SECURITY_QUESTIONS';
                },
                    function (errors) {
                        var errorsObj = __errorHandler.ProcessErrors(errors);
                        $scope.progress.Errors.Step2.PIN = errorsObj.Messages.join('. ');
                    });
        }

        $scope.securityQuestions = [];

        _authService.GetAllSecurityQuestions()
            .then(function (questions) {
                $scope.securityQuestions = questions;
            });

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

        function validateStep3() {
            $scope.progress.Errors.Step3 = {
                Question1: null,
                Question2: null,
                Question3: null,
                Answer1: null,
                Answer2: null,
                Answer3: null
            }

            var valid = true;

            if (!$scope.security.Question1.QuestionId) {
                $scope.progress.Errors.Step3.Question1 = $rootScope.translate('Security_Question_Error_Required');
                valid = false;
            } else if (!$scope.security.Question1.Answer) {
                $scope.progress.Errors.Step3.Answer1 = $rootScope.translate('Security_Question_Answer_Error_Required');
                valid = false;
            }

            if (!$scope.security.Question2.QuestionId) {
                $scope.progress.Errors.Step3.Question2 = $rootScope.translate('Security_Question_Error_Required');
                valid = false;
            } else if (!$scope.security.Question2.Answer) {
                $scope.progress.Errors.Step3.Answer2 = $rootScope.translate('Security_Question_Answer_Error_Required');
                valid = false;
            }

            if (!$scope.security.Question3.QuestionId) {
                $scope.progress.Errors.Step3.Question3 = $rootScope.translate('Security_Question_Error_Required');
                valid = false;
            } else if (!$scope.security.Question3.Answer) {
                $scope.progress.Errors.Step3.Answer3 = $rootScope.translate('Security_Question_Answer_Error_Required');
                valid = false;
            }

            return valid;
        }

        //register an account associate with external login
        function registerExt(model) {
            _authService.RegisterExternal(model)
                .then(function (response) {
                    if (response) {
                        if (response != "" && response != undefined)
                            $scope.progress.Step = 'CONGRATULATION';
                        
                    } else {
                        _authService.ExternalLoginWithExternalBearer()
                            .then(function () {
                                window.location.href = "/";
                            },
                                function (errors) {
                                    if (errors.Status == 400) {
                                        window.location.href = '/User/VerifyingEmail?email=' + errors.Errors[0].Message;
                                    } else {
                                        __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
                                    }
                                });
                    }
                },
                    function (errors) {
                        var errorObj = __errorHandler.ProcessErrors(errors);
                        __errorHandler.Swal(errorObj, _sweetAlert);
                    });
        }

        $scope.saveSecurityQuestions = function () {
            var valid = validateStep3();
            if (!valid) {
                return;
            }
            $scope.progress.Step = 'PROFILE';
        }

        $scope.openBirthday = function () {
            $scope.popupBirthday.opened = true;
        };

        $scope.popupBirthday = {
            opened: false
        };

        $scope.formats = ['dd-MMMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
        $scope.format = $scope.formats[0];

        $scope.editting = {
            PhotoUrl: null
        }

        $scope.onProfilePictureSelected = function () {
            var file = $scope.account.__files[0];

            if (file && /^image\/.+$/.test(file.type)) {
                var fileReader = new FileReader();
                fileReader.readAsDataURL(file);
                fileReader.onload = function (e) {
                    $scope.$apply(function () {
                        $scope.account.Avatar = e.target.result;
                        $scope.account.FileName = file.name;
                    });
                };
            } else if (file) {
                $scope.account.__files = null;
                $scope.account.Avatar = null;
                __common.swal(_sweetAlert,"warning",
                    $rootScope.translate('EditProfileSettings_NewProfilePicture_Required'),
                    'warning');
            }
        }

        function validateAddtionalProfile() {
            var valid = true;

            $scope.progress.Errors.Step4 = {
                Birthday: null,
                City: null,
                Country: null,
                Gender: null
            };

            if (!$scope.account.City) {
                valid = false;
                $scope.progress.Errors.Step4.City = $rootScope.translate('City_Error_Required');
            }

            if (!$scope.account.Country) {
                valid = false;
                $scope.progress.Errors.Step4.Country = $rootScope.translate('Country_Error_Required');
            }

            if (!$scope.account.Gender) {
                valid = false;
                $scope.progress.Errors.Step4.Gender = $rootScope.translate('Gender_Error_Required');
            }

            var now = new Date();
            now.setDate(now.getDate() - 1);
            now.setHours(0);
            now.setMinutes(0);
            now.setSeconds(0);

            if (!$scope.account.Birthday) {
                valid = false;
                $scope.progress.Errors.Step4.Birthday = $rootScope.translate('Birthday_Error_Required');
            } else if ($scope.account.Birthday > now) {
                $scope.progress.Errors.Step4.Birthday = $rootScope.translate('Birthday_Error_Less_Today');
                valid = false;
            }

            return valid;
        }

        $scope.genders = ['Male', 'Female'];

        //get all contry
        _dataService.getAllCountry()
            .then(function (res) {
                $scope.userCountries = res;
            });

        $scope.onCountrySelect = function (country) {
            _dataService.getCityByCountry(country.Code)
                .then(function (res) {
                    $scope.cities = res;
                });
        }

        $scope.saveAccount = function () {
            var valid = validateAddtionalProfile();
            if (!valid) {
                return;
            }

            var model = {
                Account: angular.copy($scope.account),
                Authentication: $scope.authentication,
                SecurityQuestions: $scope.security,

            };
            if (regitGlobal.inviteId) {
                model.InviteId = regitGlobal.inviteId;
            }


            model.Account.PhoneNumberCountryCallingCode = model.Account.PhoneNumberCountryCallingCode[0];
            if (!$scope.registerExternal) {
                _authService.SignUpLocal(model)
                    .then(function (response) {
                        if (response != "" && response != undefined)
                        { $scope.progress.Step = 'CONGRATULATION'; }
                        else
                        { window.location.href = "/"; }
                    },
                        function (errors) {
                            var errorObj = __errorHandler.ProcessErrors(errors);
                            __errorHandler.Swal(errorObj, _sweetAlert);
                        });
            } else {
                registerExt(model);
            }
        }

        $scope.resendVerifyEmail = function () {
            _authService.SendVerifyEmail($scope.account.Email)
                .then(function () {
                    __common.swal(_sweetAlert,$rootScope.translate('success'),
                        $rootScope.translate('Resend_Verify_Email_Success'),
                        'success');
                },
                    function (errors) {
                        __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
                    });
        }

        $scope.doLater = function () {
            var model = {
                Account: angular.copy($scope.account),
                Authentication: $scope.authentication,
                SecurityQuestions: $scope.security
            };
            if (regitGlobal.inviteId) {
                model.InviteId = regitGlobal.inviteId;
            }

            model.Account.PhoneNumberCountryCallingCode = model.Account.PhoneNumberCountryCallingCode[0];
           
            if (!$scope.registerExternal) {
                _authService.SignUpLocal(model)
                    .then(function (response) {
                        if (response != "" && response != undefined)
                        { $scope.progress.Step = 'CONGRATULATION';}
                        else
                        { window.location.href = "/";}
                        
                    },
                        function (errors) {
                            var errorObj = __errorHandler.ProcessErrors(errors);
                            __errorHandler.Swal(errorObj, _sweetAlert);
                        });
            } else {
                registerExt(model);
            }
        }

       
    }
]);