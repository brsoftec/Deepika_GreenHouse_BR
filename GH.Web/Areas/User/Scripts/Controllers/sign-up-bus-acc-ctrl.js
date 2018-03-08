var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'TranslationModule', 'UserModule', 'oitozero.ngSweetAlert', 'BusinessAccountModule', 'CommonDirectives', 'ui.select', 'ui.bootstrap'], true);

myApp.getController("SignUpBusinessAccountController", ['$scope', '$rootScope', '$http', 'BusinessAccountService', 'AuthorizationService', 'SweetAlert', function ($scope, $rootScope, $http, _baService, _authService, _sweetAlert) {
   
    $scope.progress = {
        Step: 'REGISTER',
        Errors: {
            Step1: {},
            Step2: {},
            Step3: {},
            Step4: {}
        }
    }

    $scope.account = {
        FirstName: null,
        LastName: null,
        Email: null,
        confirmEmail: null,
        PhoneNumber: null,
        PhoneNumberCountryCallingCode: null,
        Password: null,
        ConfirmPassword: null,
        AgreeTermsAndCondition: false,
        LinkWithPersonal: false
    }

    $scope.countries = [];
    $http.get('/Content/sources/countries.js').success(function (countries) {
        $scope.countries = countries;
    })
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
            ConfirmEmail: null,
            PhoneNumber: null,
            PhoneNumberCountryCallingCode: null,
            Password: null,
            ConfirmPassword: null
        }

        var valid = true;
        var regexEmail = /^(([^<>()\[\]\.,;:\s@\"]+(\.[^<>()\[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,})$/i;
        var regexPassword = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%*?&])[A-Za-z\d$@$!%*?&]{8,}/;

        if (!$scope.account.Password) {
            $scope.progress.Errors.Step1.Password = $rootScope.translate('Password_Error_Required');
            valid = false;
        } else if (!validate($scope.account.Password)) {
            $scope.progress.Errors.Step1.Password = $rootScope.translate('Password_Error_Invalidated');
            valid = false;
        } else if ($scope.account.Password !== $scope.account.ConfirmPassword) {
            $scope.progress.Errors.Step1.ConfirmPassword = $rootScope.translate('Confirm_password_do_not_match');
            valid = false;
        }

        if (!$scope.account.Email) {
            $scope.progress.Errors.Step1.Email = $rootScope.translate('Email_Error_Required');
            valid = false;
        } else if (!regexEmail.test($scope.account.Email)) {
            $scope.progress.Errors.Step1.Email = $rootScope.translate('Email_Error_Invalid');
            valid = false;
        } else if ($scope.account.Email !== $scope.account.confirmEmail) {
            $scope.progress.Errors.Step1.ConfirmEmail = 'Must enter same email to confirm';
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
        if ($scope.businessSignUpForm.formsignupPhoneInput.signupPhoneInput.$error.internationalPhoneNumber) {
            $scope.progress.Errors.Step1.PhoneNumber = 'Please enter a valid phone number';
            valid = false;
        }
        else if (!$scope.account.PhoneNumber) {
            $scope.progress.Errors.Step1.PhoneNumber = $rootScope.translate('Phone_Number_Error_Required');
            valid = false;
        }
        //if (!$scope.account.PhoneNumberCountryCallingCode ) {
        //    $scope.progress.Errors.Step1.PhoneNumberCountryCallingCode = $rootScope.translate('Phone_Number_Error_Country_Code_Required');
        //    valid = false;
        //}
        return valid;
    }

    $scope.register = function () {
        var valid = validateStep1();
        if (!valid) {
            return;
        }

        if (!$scope.account.AgreeTermsAndCondition) {
            __common.swal(_sweetAlert,'warning', $rootScope.translate('Agree_Term_Condition_Required'), 'warning');
            return;
        }

        var model = angular.copy($scope.account);
        //model.PhoneNumberCountryCallingCode = model.PhoneNumberCountryCallingCode;

        _baService.ValidateRegistrationInfo(model).then(function () {
            _authService.VerifyPhoneNumber({ PhoneNumber: model.PhoneNumberCountryCallingCode + model.PhoneNumber }).then(function (requestId) {
                $scope.authentication.RequestId = requestId;
                $scope.progress.Step = 'AUTHENTICATION';
            }, function (errors) {
                var errorObj = __errorHandler.ProcessErrors(errors);
                __errorHandler.Swal(errorObj, _sweetAlert);
            });
        }, function (errors) {
            var errorObj = __errorHandler.ProcessErrors(errors);
            __errorHandler.Swal(errorObj, _sweetAlert);
        });
    }


    // Resend pin
    $scope.ReSendPin = function () {
        var phone = $scope.account.PhoneNumberCountryCallingCode + $scope.account.PhoneNumber;
        _authService.VerifyPhoneNumber({ PhoneNumber: phone }).then(function (requestId) {
            $scope.authentication.RequestId = requestId;

        }, function (errors) {
            var errorObj = __errorHandler.ProcessErrors(errors);
            __errorHandler.Swal(errorObj, _sweetAlert);
        });
    }

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

        _authService.CheckSetPIN($scope.authentication).then(function () {
            $scope.progress.Step = 'COMPANY_DETAILS';
        }, function (errors) {
            var errorsObj = __errorHandler.ProcessErrors(errors);
            $scope.progress.Errors.Step2.PIN = errorsObj.Messages.join('. ');
        })
    }

    $scope.industries = [
        'Car',
        'Motor',
        'Boat',
        'Business',
        'Bank',
        'Software',
        'Others'
    ];
    $http.get('/Content/sources/industries.json').success(function (data) {
        $scope.industries = data.industries;
    })



    $scope.companyDetails = {
        Industry: null,
        CompanyName: null,
        DisplayName: null,
        Address: null,
        Website: null,
        WebsiteValid: null,
        Description: null
    }

    var oldValidUrl = null;
    function ValidURL(str) {
        // var expression = /[-a-zA-Z0-9@:%_\+.~#?&//=]{2,256}\.[a-z]{2,10}\b(\/[-a-zA-Z0-9@:%_\+.~#?&//=]*)?/gi;
        var expression = '^(https?://)?(www\\.)?([-a-z0-9]{1,63}\\.)*?[a-z0-9][-a-z0-9]{0,61}[a-z0-9]\\.[a-z]{2,6}(/[-\\w@\\+\\.~#\\?&/=%]*)?$';
        // var regex = new RegExp(expression);
        var regex = new RegExp(expression,'gi');
        return str.match(regex)
    }
    $scope.isValidUrl = function () {
        if (oldValidUrl != $scope.companyDetails.Website) {
            oldValidUrl = $scope.companyDetails.Website;

            if (!ValidURL($scope.companyDetails.Website))
                $scope.progress.Errors.Step3.WebsiteValid = $rootScope.translate('Invalid_website_url');
            else
              $scope.progress.Errors.Step3.WebsiteValid = null;

            //$http.get('/api/utilities/IsValidUrl', { params: { url: $scope.companyDetails.Website, hideAjaxLoader: true } })
            //    .success(function (valid) {
            //        if (!valid) {
            //            $scope.progress.Errors.Step3.WebsiteValid = $rootScope.translate('Invalid_website_url');
            //        } else {
            //            $scope.progress.Errors.Step3.WebsiteValid = null;
            //        }
            //    })
        }
    }

    function validateStep3() {
        var valid = true;
        $scope.progress.Errors.Step3 = {};

        if (!$scope.companyDetails.Industry) {
            valid = false;
            $scope.progress.Errors.Step3.Industry = $rootScope.translate('Industry_Error_Required');
        }

        if (!$scope.companyDetails.CompanyName) {
            valid = false;
            $scope.progress.Errors.Step3.CompanyName = $rootScope.translate('Company_Name_Error_Required');
        } else if ($scope.companyDetails.CompanyName.length > 256) {
            valid = false;
            $scope.progress.Errors.Step3.CompanyName = $rootScope.translate('Company_Name_Error_Too_Long');
        }

        if (!$scope.companyDetails.DisplayName) {
            valid = false;
            $scope.progress.Errors.Step3.DisplayName = $rootScope.translate('Display_Name_Error_Required');
        } else if ($scope.companyDetails.DisplayName.length > 256) {
            valid = false;
            $scope.progress.Errors.Step3.DisplayName = $rootScope.translate('Display_Name_Error_Too_Long');
        }

        if (!$scope.companyDetails.Address) {
            valid = false;
            $scope.progress.Errors.Step3.Address = $rootScope.translate('Address_Error_Required');
        } else if ($scope.companyDetails.Address.length > 256) {
            valid = false;
            $scope.progress.Errors.Step3.Address = $rootScope.translate('Address_Error_Too_Long');
        }

        if (!$scope.companyDetails.Website) {
            valid = false;
            $scope.progress.Errors.Step3.Website = $rootScope.translate('Website_Error_Required');
        } else if ($scope.companyDetails.Website.length > 256) {
            valid = false;
            $scope.progress.Errors.Step3.Website = $rootScope.translate('Website_Error_Too_Long');
        }

        return valid;
    }

    $scope.submitCompanyDetails = function () {
        var valid = validateStep3();
        if (!ValidURL($scope.companyDetails.Website))
        {
          $scope.progress.Errors.Step2.WebsiteValid = $rootScope.translate('Invalid_website_url');
                   valid = false;
        }
        else
            toStep('ADDTIONAL_INFO', valid);
      
    }

    $scope.addtionalInfo = {
        Avatar: {},
        Workdays: [
            {
                Name: 'MON',
                FullName: 'Monday',
                Selected: true
            },
            {
                Name: 'TUE',
                FullName: 'Tuesday',
                Selected: true
            },
            {
                Name: 'WED',
                FullName: 'Wednesday',
                Selected: true
            },
            {
                Name: 'THU',
                FullName: 'Thursday',
                Selected: true
            },
            {
                Name: 'FRI',
                FullName: 'Friday',
                Selected: true
            },
            {
                Name: 'SAT',
                FullName: 'Saturday',
                Selected: false
            },
            {
                Name: 'SUN',
                FullName: 'Sunday',
                Selected: false
            }
        ],
        WorkHours: {
            From: new Date(2016, 0, 1, 10),
            To: new Date(2016, 0, 1, 18)
        }
    };

    $scope.formatedWorkHours = '';
    $scope.formatedWorkHourFrom = '';
    $scope.formatedWorkHourTo = '';

    $scope.changeWorkHours = function () {
        if (!$scope.addtionalInfo.WorkHours.From || !$scope.addtionalInfo.WorkHours.To) {
            if ($scope.addtionalInfo.WorkHours.From) {
                $scope.addtionalInfo.WorkHours.To = new Date($scope.addtionalInfo.WorkHours.From);
            } else {
                $scope.addtionalInfo.WorkHours.From = new Date($scope.addtionalInfo.WorkHours.To);
            }

            $scope.addtionalInfo.WorkHours.From.setMinutes(0);
            $scope.addtionalInfo.WorkHours.From.setSeconds(0);
            $scope.addtionalInfo.WorkHours.From.setMilliseconds(0);
            $scope.addtionalInfo.WorkHours.From = new Date($scope.addtionalInfo.WorkHours.From.toISOString());

            $scope.addtionalInfo.WorkHours.To.setMinutes(0);
            $scope.addtionalInfo.WorkHours.To.setSeconds(0);
            $scope.addtionalInfo.WorkHours.To.setMilliseconds(0);
            $scope.addtionalInfo.WorkHours.To = new Date($scope.addtionalInfo.WorkHours.To.toISOString());
        }

        var from = $scope.addtionalInfo.WorkHours.From;
        var to = $scope.addtionalInfo.WorkHours.To;

        $scope.formatedWorkHourFrom = formatWorkHour(from);
        $scope.formatedWorkHourTo = formatWorkHour(to);

        $scope.formatedWorkHours = $scope.formatedWorkHourFrom + ' to ' + $scope.formatedWorkHourTo;
    }

    function formatWorkHour(date) {
        if (!date) {
            return '';
        }
        return to2Digit12BaseTime(date.getHours(), true) + ':' + to2Digit12BaseTime(date.getMinutes()) + (date.getHours() >= 12 ? ' pm' : ' am');
    }

    function to2Digit12BaseTime(str, isHour) {
        if (isHour) {
            str = str % 12;
            if (str === 0) {
                str = 12;
            }
        }
        str = str + '';
        if (str.length < 2) {
            return '0' + str;
        } else {
            return str;
        }
    }

    $scope.workdaysOption = {
        SelectAll: false
    };

    $scope.selectWorkDay = function (workday) {
        workday.Selected = !workday.Selected;
        checkIsSelectAll();
        formatWorkdays();
    }

    function checkIsSelectAll() {
        var selectAll = true;
        for (var i = 0; i < $scope.addtionalInfo.Workdays.length; i++) {
            if (!$scope.addtionalInfo.Workdays[i].Selected) {
                selectAll = false;
                break;
            }
        }

        $scope.workdaysOption.SelectAll = selectAll;
    }

    $scope.selectAllWorkdays = function () {
        angular.forEach($scope.addtionalInfo.Workdays, function (workday) {
            workday.Selected = $scope.workdaysOption.SelectAll;
        })
        formatWorkdays();
    }

    $scope.formatedWorkDays = '';

    function formatWorkdays() {
        var continuous = true;
        var selected = [];
        var lastSelected = null;
        for (var i = 0; i < 7; i++) {
            var workday = $scope.addtionalInfo.Workdays[i];

            if (lastSelected != null && lastSelected + 1 !== i && workday.Selected) {
                continuous = false;
            }

            if (workday.Selected) {
                selected.push(workday.FullName);
                lastSelected = i;
            }

        }

        if (selected.length == 0) {
            $scope.formatedWorkDays = '';
        } else if (selected.length == 1 || selected.length == 2 || !continuous) {
            $scope.formatedWorkDays = selected.join(', ');
        } else {
            $scope.formatedWorkDays = selected.shift() + ' to ' + selected.pop();
        }
    }

    checkIsSelectAll();
    formatWorkdays();
    $scope.changeWorkHours();

    $scope.onProfilePictureSelected = function () {
        var file = $scope.addtionalInfo.Avatar.__files[0];

        if (file && /^image\/.+$/.test(file.type)) {
            var fileReader = new FileReader();
            fileReader.readAsDataURL(file);
            fileReader.onload = function (e) {
                $scope.$apply(function () {
                    $scope.addtionalInfo.Avatar.Data = e.target.result;
                })
            };
        } else if (file) {
            $scope.addtionalInfo.Avatar.__files = null;
            $scope.addtionalInfo.Avatar.Data = null;
            __common.swal(_sweetAlert,"warning", $rootScope.translate('Please_choose_an_image_file'), 'warning');
        }
    }

    $scope.doItLater = function () {
        createBusinessAccount(false);
    }

    $scope.submitAddtionalInfo = function () {
        createBusinessAccount(true);
    }

    function createBusinessAccount(withAddtionalInfo) {
        var model = {
            Account: angular.copy($scope.account),
            Authentication: angular.copy($scope.authentication),
            CompanyDetails: angular.copy($scope.companyDetails),
            AddtionalInfo: withAddtionalInfo ? (function () {
                var workdays = [];
                angular.forEach($scope.addtionalInfo.Workdays, function (workday) {
                    if (workday.Selected) {
                        workdays.push(workday.Name);
                    }
                });
                return {
                    Avatar: $scope.addtionalInfo.Avatar.Data,
                    WorkHourFrom: $scope.addtionalInfo.WorkHours.From,
                    WorkHourTo: $scope.addtionalInfo.WorkHours.To,
                    Workdays: workdays
                }
            })() : null
        }
        model.Account.PhoneNumberCountryCallingCode = model.Account.PhoneNumberCountryCallingCode;

        _baService.RegisterBusinessAccount(model).then(function (success) {

            //if (success != "" && success != undefined)
            //{$scope.progress.Step = 'CONGRATULATION'; }
            //else
            //{ window.location.href = "/"; }
            $scope.progress.Step = 'CONGRATULATION';

            
        }, function (errors) {
            var errorObj = __errorHandler.ProcessErrors(errors);
            __errorHandler.Swal(errorObj, _sweetAlert);
        })
    }

    $scope.resendVerifyEmail = function () {
        _authService.SendVerifyEmail($scope.account.Email).then(function () {
            __common.swal(_sweetAlert,$rootScope.translate('success'), $rootScope.translate('Resend_Verify_Email_Success'), 'success')
        }, function (errors) {
            __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
        })
    }

    function toStep(step, valid) {
        if (valid) {
            $scope.progress.Step = step;
        }
    }

}])