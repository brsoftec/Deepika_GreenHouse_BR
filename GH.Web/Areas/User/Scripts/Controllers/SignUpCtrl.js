var myApp = angular.module('regitSignup');
myApp.controller("SignUpController",
    [
        '$scope', '$rootScope', '$http', 'AuthorizationService', 'SweetAlert', 'DataService', '$timeout',
        function ($scope, $rootScope, $http, _authService, _sweetAlert, _dataService, $timeout) {
            $scope.countryCity = {
                'Country': 'Singapore',
                'City': 'Singapore'
            }
            $scope.otpButtonLabel = 'GET OTP PIN';
            $scope.authentication = {
                PIN: null,
                RequestId: null,
                StaticPIN: null,
                ConfirmStaticPIN: null
            }

            $scope.outsiteId = '';
            $scope.outsite = {};
            $scope.getOutsite = function () {
                var id = $scope.outsiteId;
                _authService.GetOutsiteById(id)
                    .then(function (rs) {
                            $scope.outsite = rs;
                        },
                        function (errors) {
                        });
            }

            $scope.countries = [];


            $scope.initCountry = function () {
                _dataService.getAllCountry().then(function (res) {
                    $scope.countries = res;

                })
            }
            setTimeout(function () {
                $scope.initCountry();
            }, 1000);
            $scope.searchValueCity = null;
            $scope.onCountrySelect = function (country) {
                if (!country)
                    return;
                $scope.countryCity.City = undefined;
                _dataService.getCityByCountry(country.Code).then(function (res) {
                    $scope.cities = res;

                })
            }

            $scope.onCitySelect = function (city) {
                if (!city) {
                    // assign search value to editor city
                    $scope.countryCity.City = $scope.searchValueCity;

                    var FindCity = $scope.cities.findItem('Name', $scope.countryCity.City);
                    if (!FindCity) {
                        var lastCity = {};
                        lastCity.Id = '';
                        lastCity.Name = $scope.countryCity.City;
                        lastCity.Latitude = '';
                        lastCity.Longitude = '';
                        $scope.cities[$scope.cities.length] = lastCity;
                    }
                }
            }


            $scope.getSearchValueCity = function (search) {
                // get search value
                $scope.searchValueCity = search;
                if ($scope.searchValueCity) {
                    $scope.countryCity.City = $scope.searchValueCity;
                }
            }

            $scope.registerExternal = _authService.IsSignUpExternal();
            var provider = null;
            $scope.checkSubmit = false
            $scope.account = {
                FirstName: null,
                LastName: null,
                Email: null,
                confirmEmail: null,
                PhoneNumber: null,
                PhoneNumberCountryCallingCode: null,
                Password: null,
                ConfirmPassword: null,
                City: null,
                Country: null,
                Gender: null,
                Birthday: null,
                Avatar: null,
                FileName: null,
                AgreeTermsAndCondition: false
            };
            if ($scope.registerExternal) {
                provider = _authService.GetExternalProvider();
                _authService.GetSignUpExternal()
                    .then(function (info) {
                        if (info.Skip) {
                            registerExt();
                        } else {
                            $scope.account = {
                                FirstName: info.FirstName,
                                LastName: info.LastName,
                                Email: info.Email,
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
                        confirmEmail: null,
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
                        StaticPIN: null,
                        ConfirmStaticPIN: null
                    },
                    Step4: {
                        Question1: null,
                        Question2: null,
                        Question3: null,
                        Answer1: null,
                        Answer2: null,
                        Answer3: null
                    },
                    Step5: {
                        Birthday: null,
                        City: null,
                        Country: null,
                        Gender: null
                    }
                }
            };
            $scope.countries = [];
            $http.get('/Content/sources/countries.js')
                .success(function (countries) {
                    $scope.countries = countries;
                });

            function validate(password) {
                var minMaxLength = /^[\s\S]{8,32}$/,
                    upper = /[A-Z]/,
                    lower = /[a-z]/,
                    number = /[0-9]/,
                    special = /[ !"#$%&'()*+,\-./:;<=>?@[\\\]^_`{|}~]/;

                if (minMaxLength.test(password) &&
                    upper.test(password) &&
                    lower.test(password) &&
                    number.test(password) &&
                    special.test(password)
                ) {
                    return true;
                }

                return false;
            }

            function validateStep1() {

                $scope.progress.Errors.Step1 = {
                    FirstName: null,
                    LastName: null,
                    Email: null,
                    confirmEmail: null,
                    PhoneNumber: null,
                    City: null,
                    Country: null,
                    Password: null,
                    ConfirmPassword: null
                }
                var valid = true;
                var regexEmail =
                    /^(([^<>()\[\]\.,;:\s@\"]+(\.[^<>()\[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,})$/i;
                var regexPassword = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%*^#?&()$])[A-Za-z\d$@$!%^*#?&]{8,}/;

                if (!$scope.registerExternal) {
                    if (!$scope.account.Password) {
                        $scope.progress.Errors.Step1.Password = $rootScope.translate('Password_Error_Required');
                        valid = false;
                    } else if ($scope.account.Password.length < 8) {
                        $scope.progress.Errors.Step1.Password = $rootScope.translate('Password_Error_Invalidated');
                        valid = false;
                    }

                    if (!$scope.account.ConfirmPassword) {
                        $scope.progress.Errors.Step1.ConfirmPassword = $rootScope
                            .translate('ConfirmPassword_Error_Required');
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

                if (!$scope.account.confirmEmail) {
                    $scope.progress.Errors.Step1.confirmEmail = 'Please confirm email';
                    valid = false;
                } else if ($scope.account.Email !== $scope.account.confirmEmail) {
                    $scope.progress.Errors.Step1.confirmEmail = 'Please enter same email to confirm';
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

                if ($scope.signUpForm.formsignupPhoneInput.signupPhoneInput.$error.internationalPhoneNumber) {
                    $scope.progress.Errors.Step1.PhoneNumber = 'Please enter a valid phone number';
                    valid = false;
                }
                else if (!$scope.account.PhoneNumber) {
                    $scope.progress.Errors.Step1.PhoneNumber = $rootScope.translate('Phone_Number_Error_Required');
                    valid = false;
                }

                if ($scope.countryCity.Country)
                    $scope.account.Country = $scope.countryCity.Country;
                else if (!$scope.countryCity.Country) {
                    $scope.progress.Errors.Step1.Country = $rootScope.translate('Country_Error_Required');
                    valid = false;
                }
                if ($scope.countryCity.City)
                    $scope.account.City = $scope.countryCity.City;
                else if (!$scope.countryCity.City) {
                    $scope.progress.Errors.Step1.City = $rootScope.translate('City_Error_Required');
                    valid = false;
                }

                return valid;
            }


            // var model = {
            //     Account: {
            //         FirstName: "Test",
            //         LastName: "User",
            //         Email: "test@regit.today",
            //         PhoneNumber: "909090593",
            //         PhoneNumberCountryCallingCode: "84",
            //         Password: "Test@123"
            //     },
            //     Authentication: {
            //         PIN: '1234',
            //         RequestId: '123456'
            //     }
            // };
            // _authService.SignUpLocal(model)
            //     .then(function (response) {
            //             console.log(response)
            //         },
            //         function (errors) {
            //             console.log('Error', errors)
            //         });

            // var model = {
            //     Option: 0,
            //     VerifyInfo: {
            //         Email: "sonnhjamy@gmail.com",
            //         Question1: "1",
            //         Question2: "2",
            //         Question3: "3"
            //     }
            // };
            //
            // $http.post('/api/Account/ForgotPassword', model
            //     // { params: {
            //     //     email: 'long.tran@qudy.com.sf'
            //     // }}
            // )
            //     .success(function (response) {
            //         console.log(response)
            //     }).error(function (errors) {
            //     console.log(errors)
            // });


            $scope.register = function () {
                var valid = validateStep1();
                if (!valid) {
                    return;
                }
                if (!$scope.account.AgreeTermsAndCondition) {
                    __common.swal(_sweetAlert, 'warning', $rootScope.translate('Agree_Term_Condition_Required'), 'warning');
                    return;
                }
                var model = angular.copy($scope.account);

                //model.PhoneNumberCountryCallingCode = model.PhoneNumberCountryCallingCode[0];
                if (!$scope.registerExternal) {
                    _authService.ValidateRegistrationInfo(model)
                        .then(function () {
                                $scope.progress.Step = 'AUTHENTICATION';

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


            //phone
            $scope.disableSend = false;
            $scope.errors = {PIN: null};
            $scope.sentOtp = false;

            $scope.GetConfirmSMS = function () {
                var model = angular.copy($scope.account);
                _authService.ValidateExternalRegistrationInfo(model).then(function () {
                    _authService.VerifyPhoneNumber({PhoneNumber: model.PhoneNumberCountryCallingCode + model.PhoneNumber})
                        .then(function (requestId) {
                            $scope.authentication.RequestId = requestId;
                            $scope.disableSend = true;
                            $scope.sentOtp = true;
                        }, function (errors) {
                            var errorObj = __errorHandler.ProcessErrors(errors);
                            __errorHandler.Swal(errorObj, _sweetAlert);
                        });
                }, function (errors) {
                    $scope.errors.PIN = 'Send SMS error';
                });

                $timeout(function () {
                    if ($scope.disableSend) {
                        $scope.otpButtonLabel = 'Resend OTP';
                        $scope.disableSend = false;
                    }

                }, 60000);

            }

            $scope.checkPinSMS = function (value) {

                if (value == '') {
                    $scope.errors.PIN = 'Please enter your PIN';
                    return;
                }

                _authService.CheckSetPIN($scope.authentication).then(function () {
                    $scope.doLater();
                }, function (errors) {
                    $scope.errors.PIN = 'Incorrect PIN. Please re-enter';
                })
            };

            $scope.cancelOtp = function () {
                window.location.href = '/User/SignUp';
            }
            // end phone
            //Step 2
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

            //Vu step 3
            $scope.authenticate = function () {
                var valid = validateStep2();
                if (!valid) {
                    return;
                }
                $scope.authentication.PhoneNumber = $scope.account.PhoneNumberCountryCallingCode + $scope.account.PhoneNumber;

                _authService.CheckSetPIN($scope.authentication)
                    .then(function () {
                            $scope.doLater();
                        },
                        function (errors) {
                            var errorsObj = __errorHandler.ProcessErrors(errors);
                            $scope.progress.Errors.Step1.PIN = errorsObj.Messages.join('. ');
                        });
            };

            //Step 2
            function validateStep3() {
                $scope.progress.Errors.Step3 = {
                    staticPIN: null,
                    ConfirmStaticPIN: null
                }
                var valid = true;
                if (!$scope.authentication.StaticPIN) {
                    $scope.progress.Errors.Step3.StaticPIN = 'Enter a personal PIN';
                    valid = false;
                } else if (!/^\d{4,6}$/.test($scope.authentication.StaticPIN)) {
                    $scope.progress.Errors.Step3.StaticPIN = 'PIN must contain 4 to 6 digits, no other characters allowed';
                    valid = false;
                }
                else if (!$scope.authentication.ConfirmStaticPIN || $scope.authentication.StaticPIN !== $scope.authentication.ConfirmStaticPIN) {
                    $scope.progress.Errors.Step3.ConfirmStaticPIN = 'Confirm PIN does not match PIN';
                    valid = false;
                }
                return valid;
            }


            $scope.setStaticPIN = function () {
                var valid = validateStep3();

                if (!valid) {
                    return;
                }
                _authService.SetStaticPIN($scope.authentication)
                    .then(function () {
                            $scope.progress.Step = 'SECURITY_QUESTIONS';
                        },
                        function (errors) {
                            var errorsObj = __errorHandler.ProcessErrors(errors);
                            $scope.progress.Errors.Step3.StaticPIN = errorsObj.Messages.join('. ');
                        });
            }

            $scope.ReSendPin = function () {
                var phone = $scope.account.PhoneNumberCountryCallingCode + $scope.account.PhoneNumber;
                _authService.VerifyPhoneNumber({PhoneNumber: phone}).then(function (requestId) {
                    $scope.authentication.RequestId = requestId;
                }, function (errors) {
                    var warningPhone = "Your PIN has been sent to the following number: +" + phone +
                        ". Please wait 5 minutes before sending another request to reset your PIN.";
                    __common.swal(_sweetAlert, "warning", warningPhone, 'warning');
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
                    Answer: null,
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

            function validateStep4() {
                $scope.progress.Errors.Step4 = {
                    Question1: null,
                    Question2: null,
                    Question3: null,
                    Answer1: null,
                    Answer2: null,
                    Answer3: null
                }

                var valid = true;

                if (!$scope.security.Question1.QuestionId) {
                    $scope.progress.Errors.Step4.Question1 = $rootScope.translate('Security_Question_Error_Required');
                    valid = false;
                } else if (!$scope.security.Question1.Answer) {
                    $scope.progress.Errors.Step4.Answer1 = $rootScope.translate('Security_Question_Answer_Error_Required');
                    valid = false;
                }

                if (!$scope.security.Question2.QuestionId) {
                    $scope.progress.Errors.Step4.Question2 = $rootScope.translate('Security_Question_Error_Required');
                    valid = false;
                } else if (!$scope.security.Question2.Answer) {
                    $scope.progress.Errors.Step4.Answer2 = $rootScope.translate('Security_Question_Answer_Error_Required');
                    valid = false;
                }

                if (!$scope.security.Question3.QuestionId) {
                    $scope.progress.Errors.Step4.Question3 = $rootScope.translate('Security_Question_Error_Required');
                    valid = false;
                } else if (!$scope.security.Question3.Answer) {
                    $scope.progress.Errors.Step4.Answer3 = $rootScope.translate('Security_Question_Answer_Error_Required');
                    valid = false;
                }

                return valid;
            }

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
                var valid = validateStep4();
                if (!valid) {
                    return;
                }
                $scope.doLater();
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
                    __common.swal(_sweetAlert, "warning",
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

            $scope.saveAccount = function () {
                var valid = validateAddtionalProfile();
                if (!valid) {
                    return;
                }

                var model = {
                    Account: angular.copy($scope.account),
                    Authentication: $scope.authentication,
                    SecurityQuestions: $scope.security,
                    OutsiteId: $scope.outsiteId
                };

                if (!$scope.registerExternal) {
                    _authService.SignUpLocal(model)
                        .then(function (response) {
                                if (response != "" && response != undefined) {
                                    $scope.progress.Step = 'CONGRATULATION';
                                }
                                else {
                                    window.location.href = "/";
                                }
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
                            __common.swal(_sweetAlert, $rootScope.translate('success'),
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
                    SecurityQuestions: $scope.security,
                    OutsiteId: $scope.outsiteId
                };
                if (regitGlobal.inviteId) {
                    model.InviteId = regitGlobal.inviteId;
                }

                if (!$scope.registerExternal) {
                    _authService.SignUpLocal(model)
                        .then(function (response) {
                                $scope.progress.Step = 'CONGRATULATION';
                            },
                            function (errors) {
                                console.log("Error creating account:", errors)
                            });
                } else {
                    registerExt(model);
                }
            }


        }
    ]);


myApp.controller("UserSignupController", ['$scope', '$rootScope', '$http', 'AuthorizationService', '$cookies', function ($scope, $rootScope, $http, _authService, $cookies) {
    $scope.signup = {
        email: '',
        firstName: '',
        lastName: '',
        password: '',
        repass: ''
    };
    $scope.signupError = {
        email: false,
        firstName: false,
        lastName: false,
        password: false,
        repass: false
    };
    $scope.signupView = {
        dirty: false,
        signingUp: false,
        signupError: false,
        signingIn: false
    };

    $scope.onSignupInput = function (field) {
        $scope.signupView.dirty = true;
        if (field === 'email') {
            var email = $scope.signup.email;
            if (!email.length) {
                $scope.signupError.email = 'null';
            } else if (!email.match(/[a-z0-9_\-.]+@[a-z0-9_\-.]+\.([a-z]{2,})/gi)) {
                $scope.signupError.email = 'invalid';
            } else {
                $http.get('/Api/Account/Exists', {
                    params: {
                        email: email
                    }
                }).success(function (response) {
                    if (response.success)
                        $scope.signupError.email = 'taken';
                    else
                        $scope.signupError.email = false;
                }).error(function (response) {
                    console.log(response);
                });
            }
        } else if (field === 'firstName')
            $scope.signupError.firstName = !$scope.signup.firstName.length;
        else if (field === 'password')
            $scope.signupError.password = !$scope.signup.password.length ? 'null' : $scope.signup.password.length < 8 ? 'length' : false;
        else if (field === 'repass')
            $scope.signupError.repass = !$scope.signup.repass.length ? 'null' : $scope.signup.repass !== $scope.signup.password  ? 'mismatch' : false;
    };
    $scope.canSignup = function () {
        return $scope.signupView.dirty && !$scope.signupError.email && !$scope.signupError.firstName && !$scope.signupError.password && !$scope.signupError.repass;
    };
    $scope.signupSubmit = function () {
        if ($scope.canSignup()) {
            $scope.signupView.signingUp = true;
            $http.post('/Api/Account/Signup', {
                email: $scope.signup.email,
                firstName: $scope.signup.firstName,
                lastName: $scope.signup.lastName,
                password: $scope.signup.password
            }).success(function (response) {
                $scope.signupView.signingUp = false;
                if (response.success) {
                    $scope.signupView.signingIn = true;
                    _authService.SignInLocal($scope.signup.email, $scope.signup.password)
                        .then(function () {
                                $rootScope.authorized = _authService.IsAuthorized();
                                window.location.href = "/";
                            },
                            function (response) {
                                $scope.signupView.signingIn = false;
                                $scope.signupView.signupError = response.error_description;
                                console.log(response);
                            });
                }
                else {
                    $scope.signupView.signupError = response.message;
                }
            }).error(function (response) {
                $scope.signupView.signupError = response ? response.message : '';
                console.log(response);
            });
        }
    };
}]);