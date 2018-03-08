myApp.getController('EditBusinessPageController', ['$scope', '$rootScope', '$http', 'AuthorizationService', 'UserManagementService', 'BusinessAccountService', 'SweetAlert', 'DataService', function ($scope, $rootScope, $http, _authService, _userManager, _busAccService, _sweetAlert, _dataService) {
//
    $scope.profile = {};
    $scope.editor = {};
    $scope.privacy = {
        'PhotoUrl': 'public',
        'PictureAlbum': 'public',
        'Address': 'public',
        'Phone': 'public',
        'Email': 'public',
        'Website': 'public',
        'WorkTime': 'public',
        'Profile': 'public'
    };
    $scope.ProfilePrivacy = {
        'AccountId': '',
        'ListField': []
    };
    $scope.ProfilePrivacyId = '';
    var _CompanyDetails = new Object();
    //get all contry

    $scope.updatePrivacy = function () {
        var id = $scope.profile.AccountId;
      
        var privacy = {
            'AccountId': id,
            'ListField': []
        }
        privacy.ListField = [
            { 'Field': 'PhotoUrl', 'Role': $scope.privacy.PhotoUrl },
            { 'Field': 'PictureAlbum', 'Role':  $scope.privacy.PictureAlbum },
            { 'Field': 'Address', 'Role':  $scope.privacy.Address },
            { 'Field': 'Phone', 'Role':  $scope.privacy.Phone },
            { 'Field': 'Email', 'Role':  $scope.privacy.Email },
            { 'Field': 'Website', 'Role':  $scope.privacy.Website },
            { 'Field': 'WorkTime', 'Role': $scope.privacy.WorkTime },
            { 'Field': 'Profile', 'Role': $scope.privacy.Profile }
        ]

    
        
        _authService.UpdatePrivacy(privacy)
           .then(function (rs) {
               $scope.ProfilePrivacyId = rs;
              
           },
               function (errors) { });
    }

    $scope.InitDataPrivacy = function () {
        if ($scope.ProfilePrivacy)
        {
            for (var i = 0; i < $scope.ProfilePrivacy.ListField.length; i++) {

                if ($scope.ProfilePrivacy.ListField[i].Field == 'PhotoUrl')
                    $scope.privacy.PhotoUrl = $scope.ProfilePrivacy.ListField[i].Role;
                else if ($scope.ProfilePrivacy.ListField[i].Field == 'PictureAlbum')
                    $scope.privacy.PictureAlbum = $scope.ProfilePrivacy.ListField[i].Role;
                else if ($scope.ProfilePrivacy.ListField[i].Field == 'Address')
                    $scope.privacy.Address = $scope.ProfilePrivacy.ListField[i].Role;
                else if ($scope.ProfilePrivacy.ListField[i].Field == 'Phone')
                    $scope.privacy.Phone = $scope.ProfilePrivacy.ListField[i].Role;
                else if ($scope.ProfilePrivacy.ListField[i].Field == 'Email')
                    $scope.privacy.Email = $scope.ProfilePrivacy.ListField[i].Role;
                else if ($scope.ProfilePrivacy.ListField[i].Field == 'Website')
                    $scope.privacy.Website = $scope.ProfilePrivacy.ListField[i].Role

                else if ($scope.ProfilePrivacy.ListField[i].Field == 'WorkTime')
                    $scope.privacy.WorkTime = $scope.ProfilePrivacy.ListField[i].Role;
                else if ($scope.ProfilePrivacy.ListField[i].Field == 'Profile')
                    $scope.privacy.Profile = $scope.ProfilePrivacy.ListField[i].Role;
            }
        }
      

    }
    _dataService.getAllCountry().then(function (res) {
        $scope.countries = res;
        _busAccService.GetBusinessProfile().then(function (profile) {
            profile.DescriptionHtml = __common.GetNewLineCharInHtml(profile.Description);
            $scope.profile = profile;
            var id = profile.AccountId;
            _authService.GetPrivacy(id).then(function (rs) {
                $scope.ProfilePrivacy = rs;
                $scope.InitDataPrivacy();
            },
                 function (errors) { });
            _busAccService.GetCompanyObjectDetailsById(id).then(function (response) {

                _CompanyDetails = response;
                $scope.InitData();
            });
            $scope.editor = angular.copy($scope.profile);
            var country = $scope.countries.findItem('Name', $scope.profile.Country);
            if (!country)
                return;

        })
    })

    $scope.currentAlbumPictureUploadIndex = 0;

    $scope.pictureAlbum = { __album: [{}, {}, {}, {}, {}, {}], Urls: [] };
    $scope.originalAlbum = [];

    _busAccService.GetPictureAlbum().then(function (album) {
        $scope.originalAlbum = album;
        $scope.pictureAlbum.Urls = angular.copy($scope.originalAlbum);
    })

    $scope.editting = {};

    $scope.edit = function () {
        var edittingFields = '';
        for (var i = 0; i < arguments.length; i++) {
            edittingFields += arguments[i];
        }
        $scope.editting[edittingFields] = true;
    }

    $scope.cancel = function () {
        var edittingFields = '';
        for (var i = 0; i < arguments.length; i++) {
            edittingFields += arguments[i];
            $scope.editor[arguments[i]] = $scope.profile[arguments[i]];
            if (arguments[i] == 'PhotoUrl') {
                $scope.editor.__files = null;
                $scope.editor.NewProfilePicture = null;
            }
        }
        $scope.editting[edittingFields] = false;
    }

    $scope.save = function () {
        var edittingFields = '';

        var saveModel = angular.copy($scope.profile);
        saveModel.UpdateFields = [];

        for (var i = 0; i < arguments.length; i++) {
            edittingFields += arguments[i];
            saveModel.UpdateFields.push(arguments[i]);
            if (arguments[i] == 'DisplayName') {
                if (!saveModel[arguments[i]]) {
                    __common.swal(_sweetAlert, 'warning', $rootScope.translate('Please_enter_display_name'), 'warning');
                    return;
                } else if (saveModel[arguments[i]].length > 128) {
                    __common.swal(_sweetAlert, 'warning', $rootScope.translate('Display_Name_length_cannot_exceed_128_characters'), 'warning');
                    return;
                }
            }
            if (arguments[i] == 'PhotoUrl') {
                if ($scope.editor.__files && $scope.editor.__files.length) {
                    saveModel['PhotoUrl'] = $scope.editor.__files[0];
                } else {
                    __common.swal(_sweetAlert, 'warning', $rootScope.translate('Please_choose_an_image'), 'warning');
                    return;
                }
            } else if (arguments[i] == 'PictureAlbum') {

            }
            else {
                saveModel[arguments[i]] = $scope.editor[arguments[i]];
            }
        }

        saveModel.ForAccount = $rootScope.translate('Business');

        _userManager.UpdateProfile(saveModel).then(function (updatedProfile) {
            updatedProfile.DescriptionHtml = __common.GetNewLineCharInHtml(updatedProfile.Description);
            $scope.profile = updatedProfile;
            $rootScope.BAProfile = updatedProfile;
            $scope.editting[edittingFields] = false;
        }, function (errors) {
            var errorObj = __errorHandler.ProcessErrors(errors);
            __errorHandler.Swal(errorObj, _sweetAlert);
        })
    }

    $scope.onProfilePictureSelected = function () {
        var file = $scope.editor.__files[0];

        if (file && /^image\/.+$/.test(file.type)) {
            var fileReader = new FileReader();
            fileReader.readAsDataURL(file);
            fileReader.onload = function (e) {
                $scope.$apply(function () {
                    $scope.editor.NewProfilePicture = e.target.result;
                })
            };
        } else if (file) {
            $scope.editor.__files = null;
            $scope.editor.NewProfilePicture = null;
            __common.swal(_sweetAlert, "warning", $rootScope.translate('Please_choose_an_image_file'), 'warning');
        }
    }

    $scope.onPictureAlbumSelected = function () {
        var file = $scope.pictureAlbum.__album[$scope.currentAlbumPictureUploadIndex].__files[0];

        if (!$scope.pictureAlbum.Urls) {
            $scope.pictureAlbum.Urls = [];
        }

        if (file && /^image\/.+$/.test(file.type)) {
            if ($scope.pictureAlbum.Urls.length >= 6) {
                $scope.pictureAlbum.__album[$scope.currentAlbumPictureUploadIndex].__files = null;
                __common.swal(_sweetAlert, 'warning', $rootScope.translate('Upload_picture_limited'), 'warning');
                return;
            }

            var fileReader = new FileReader();
            fileReader.readAsDataURL(file);
            fileReader.onload = function (e) {
                $scope.$apply(function () {
                    $scope.pictureAlbum.Urls.push(e.target.result);
                    $scope.currentAlbumPictureUploadIndex++;
                })
            };
        } else if (file) {
            __common.swal(_sweetAlert, "warning", $rootScope.translate('Please_choose_an_image_file'), 'warning');
        }
    }

    var deletingPhotos = [];

    $scope.removePictureAlbum = function (index) {
        if (!$scope.pictureAlbum.Urls) {
            $scope.pictureAlbum.Urls = [];
        }

        if ($scope.pictureAlbum.__album[index].__files && $scope.pictureAlbum.__album[index].__files[0]) {
            $scope.pictureAlbum.__album[index].__control.clearFile();
            $scope.pictureAlbum.__album.splice(index, 1);
            $scope.pictureAlbum.__album.push({});
            $scope.currentAlbumPictureUploadIndex--;
        }

        if ($scope.pictureAlbum.Urls[index]) {
            if ($scope.originalAlbum.indexOf($scope.pictureAlbum.Urls[index]) >= 0) {
                deletingPhotos.push($scope.pictureAlbum.Urls[index]);
            }
            $scope.pictureAlbum.Urls.splice(index, 1);
        }
    }

    $scope.cancelPictureAlbum = function () {
        $scope.pictureAlbum.Urls = angular.copy($scope.originalAlbum);
        $scope.pictureAlbum.__album = [{}, {}, {}, {}, {}, {}];
        $scope.editting.PictureAlbum = false;
        $scope.currentAlbumPictureUploadIndex = 0;
        deletingPhotos = [];
    }

    $scope.savePictureAlbum = function () {
        var newPhoto = [];
        for (var i = 0; i < $scope.pictureAlbum.__album.length; i++) {
            var file = $scope.pictureAlbum.__album[i].__files ? $scope.pictureAlbum.__album[i].__files[0] : null;
            if (file) {
                newPhoto.push(file);
            }
        }
        _busAccService.UploadPictureAlbum(newPhoto, deletingPhotos).then(function (photos) {
            $scope.originalAlbum = photos;
            $scope.pictureAlbum.Urls = angular.copy($scope.originalAlbum);
            $scope.pictureAlbum.__album = [{}, {}, {}, {}, {}, {}];
            $scope.editting.PictureAlbum = false;
            $scope.currentAlbumPictureUploadIndex = 0;
            deletingPhotos = [];
        }, function (errors) {
            __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
        })
    }


    $scope.configs = {
        ShowPinAuthenticateModal: false
    }

    $scope.pinAuthenticate = {
        PIN: null,
        PinSent: false,
        RequestId: null,
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
        model = { PhoneNumber: $scope.changePhoneNumberModel.NewPhoneNumber };
        _authService.VerifyPhoneNumber(model).then(function (requestId) {
            $scope.pinAuthenticate.RequestId = requestId;
            $scope.pinAuthenticate.PinSent = true;
            $scope.configs.ShowPinAuthenticateModal = true;
            __common.swal(_sweetAlert, 'OK', $rootScope.translate('New_PIN_has_been_sent'), 'success');
        }, function (errors) {
            __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
        })
    }

    $scope.onPinAuthenticateModalHidden = function () {
        $scope.pinAuthenticate.PIN = null;
    }

    $scope.authenticatePin = function () {
        $scope.saveNewPhoneNumber();
    }

    $scope.countryCodes = [];
    $http.get('/Content/sources/countries.js').success(function (countries) {
        $scope.countryCodes = countries;
    })

    $scope.encodedPhoneNumber = null;

    _userManager.GetEncodedPhoneNumber().then(function (phoneNumber) {
        $scope.encodedPhoneNumber = phoneNumber;
    })

    $scope.changePhoneNumberModel = {
        NewPhoneNumber: ''
    }

    $scope.cancelChangePhoneNumber = function () {
        $scope.changePhoneNumberModel = {
            NewPhoneNumber: ''
        }
        $scope.editting.PhoneNumber = false;
    }

    $scope.changePhoneNumber = function () {
        if (!$scope.changePhoneNumberModel.NewPhoneNumberCountryCallingCode[0] || !$scope.changePhoneNumberModel.NewPhoneNumberCountryCallingCode[0]) {
            __common.swal(_sweetAlert, 'errors', $rootScope.translate('Country_Error_Required'), 'warning');
            return;
        }
        if (!$scope.changePhoneNumberModel.NewPhoneNumber) {
            __common.swal(_sweetAlert, 'errors', $rootScope.translate('Please_enter_new_phone_number'), 'warning');
            return;
        }

        var formatedNumber = '+' + $scope.changePhoneNumberModel.NewPhoneNumberCountryCallingCode[0] + $scope.changePhoneNumberModel.NewPhoneNumber;
        var formatedConfirmNumber = formatedNumber;

        if (formatedNumber != $scope.pinAuthenticate.SentNewPhone) {
            $scope.pinAuthenticate.SentNewPhone = formatedNumber;
            $scope.pinAuthenticate.PinSent = false;
        } else if ($scope.newPhonePIN.PIN != null && $scope.newPhonePIN.RequestId != null) {
            $scope.pinAuthenticate.PinSent = true;
            $scope.pinAuthenticate.PIN = $scope.newPhonePIN.PIN;
            $scope.pinAuthenticate.RequestId = $scope.newPhonePIN.RequestId;
            $scope.newPhonePIN = {
                PIN: null,
                RequestId: null
            }
        }

        $scope.pinAuthenticate.VerifyNewPhone = true;
        $scope.showPinAuthenticateModal({ PhoneNumber: formatedNumber });
    }

    $scope.newPhonePIN = {
        PIN: null,
        RequestId: null
    }

    $scope.saveNewPhoneNumber = function () {
        var formatedNumber = '+' + $scope.changePhoneNumberModel.NewPhoneNumberCountryCallingCode[0] + $scope.changePhoneNumberModel.NewPhoneNumber;
        var formatedConfirmNumber = formatedNumber;
        _userManager.UpdatePhoneNumberForBusinessAccount({ NewPhoneNumber: formatedNumber, ConfirmNewPhoneNumber: formatedConfirmNumber }, $scope.pinAuthenticate.PIN, $scope.pinAuthenticate.RequestId).then(function (phoneNumber) {
            __common.swal(_sweetAlert, 'OK', $rootScope.translate('Change_phone_number_success'), 'success');
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
            __errorHandler.Swal(errorObj, _sweetAlert);
        })
    }

    $scope.baseWorkHours = { From: null, To: null };
    $scope.workHours = { From: null, To: null };
    $scope.formatedWorkHours = '';
    $scope.formatedWorkHourFrom = '';
    $scope.formatedWorkHourTo = '';

    var firstTimeSetWorkHours = false;
    $scope.changeWorkHours = function () {
        if (!$scope.baseWorkHours.From && !$scope.baseWorkHours.To && !firstTimeSetWorkHours) {
            if ($scope.workHours.From) {
                $scope.workHours.From.setMinutes(0);
                $scope.workHours.From.setSeconds(0);
                $scope.workHours.From.setMilliseconds(0);
                $scope.workHours.From = new Date($scope.workHours.From.toISOString());
            } else {
                $scope.workHours.To.setMinutes(0);
                $scope.workHours.To.setSeconds(0);
                $scope.workHours.To.setMilliseconds(0);
                $scope.workHours.To = new Date($scope.workHours.To.toISOString());
            }
        }

        firstTimeSetWorkHours = true;

        var from = $scope.workHours.From;
        var to = $scope.workHours.To;

        if (from && to) {
            $scope.formatedWorkHourFrom = formatWorkHour(from);
            $scope.formatedWorkHourTo = formatWorkHour(to);

            $scope.formatedWorkHours = $scope.formatedWorkHourFrom + ' to ' + $scope.formatedWorkHourTo;
        } else {
            if (from) {
                $scope.workHours.To = from;
            } else {
                $scope.workHours.From = to;
            }

            $scope.changeWorkHours();
        }
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

    $scope.baseWorkdays = [
         {
             Name: 'MON',
             FullName: 'Monday',
             Selected: false
         },
        {
            Name: 'TUE',
            FullName: 'Tuesday',
            Selected: false
        },
        {
            Name: 'WED',
            FullName: 'Wednesday',
            Selected: false
        },
        {
            Name: 'THU',
            FullName: 'Thursday',
            Selected: false
        },
        {
            Name: 'FRI',
            FullName: 'Friday',
            Selected: false
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
    ]

    $scope.workdays = [
        {
            Name: 'MON',
            FullName: 'Monday',
            Selected: false
        },
        {
            Name: 'TUE',
            FullName: 'Tuesday',
            Selected: false
        },
        {
            Name: 'WED',
            FullName: 'Wednesday',
            Selected: false
        },
        {
            Name: 'THU',
            FullName: 'Thursday',
            Selected: false
        },
        {
            Name: 'FRI',
            FullName: 'Friday',
            Selected: false
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
    ]

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
        for (var i = 0; i < $scope.workdays.length; i++) {
            if (!$scope.workdays[i].Selected) {
                selectAll = false;
                break;
            }
        }

        $scope.workdaysOption.SelectAll = selectAll;
    }

    $scope.selectAllWorkdays = function () {
        angular.forEach($scope.workdays, function (workday) {
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
            var workday = $scope.workdays[i];

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

    $scope.saveWorkTime = function () {
        var workdays = [];
        angular.forEach($scope.workdays, function (workday) {
            if (workday.Selected) {
                workdays.push(workday.Name);
            }
        });

        _busAccService.UpdateWorkTime({ WorkHourFrom: $scope.workHours.From, WorkHourTo: $scope.workHours.To, Workdays: workdays }).then(function () {
            $scope.baseWorkHours = angular.copy($scope.workHours);
            $scope.baseWorkdays = angular.copy($scope.workdays);
            $scope.editting.WorkTime = false;
        }, function (errors) {
            __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
        })
    }

    $scope.cancelWorkTime = function () {
        $scope.workHours = angular.copy($scope.baseWorkHours);
        $scope.workdays = angular.copy($scope.baseWorkdays);
        formatWorkdays();
        checkIsSelectAll();
        if (!$scope.workHours.From || !$scope.workHours.To) {
            $scope.formatedWorkHours = '';
            $scope.formatedWorkHourFrom = '';
            $scope.formatedWorkHourTo = '';
        } else {
            $scope.formatedWorkHourFrom = formatWorkHour($scope.workHours.From);
            $scope.formatedWorkHourTo = formatWorkHour($scope.workHours.To);
            $scope.formatedWorkHours = $scope.formatedWorkHourFrom + ' to ' + $scope.formatedWorkHourTo;
        }

        firstTimeSetWorkHours = false;
        $scope.editting.WorkTime = false;
    }

    _busAccService.GetWorkTime(true).then(function (worktime) {
        if (worktime != null) {
            if (worktime.WorkHourFrom) {
                $scope.baseWorkHours.From = new Date(worktime.WorkHourFrom);
            }
            if (worktime.WorkHourTo) {
                $scope.baseWorkHours.To = new Date(worktime.WorkHourTo);
            }

            if (worktime.Workdays != null) {
                angular.forEach($scope.baseWorkdays, function (workday) {
                    if (worktime.Workdays.indexOf(workday.Name) >= 0) {
                        workday.Selected = true;
                    }
                })
            }

            $scope.workdays = angular.copy($scope.baseWorkdays);
            $scope.workHours = angular.copy($scope.baseWorkHours);

            if ($scope.workHours.From && $scope.workHours.To) {
                $scope.changeWorkHours();
            }
            formatWorkdays();
            checkIsSelectAll();
        }

    })


    $scope.InitData = function () {
        
        $scope.company = {
            Industry: '',
            EditIndustry: false,
            ErrorIndustry: '',
            CompanyName: '',
            EditCompanyName: false,
            ErrorCompanyName: '',
            Description: '',
            EditDescription: '',
            ErrorDescription: '',
            Phone: '',
            EditPhone: false,
            ErrorPhone: '',

            Email: '',
            EditEmail: false,
            ErrorEmail: '',

            Website: '',
            EditWebsite: false,
            ErrorWebsite: '',
            WorkHourFrom: '',
            WorkHourTo: '',
            Workdays: [],

            WorkTime: {},
            EditWorkTime: false

        };
       

        $scope.company.Industry = _CompanyDetails.Industry;
        $scope.ValueIndustry = _CompanyDetails.Industry;

        $scope.company.CompanyName = _CompanyDetails.CompanyName;
        $scope.ValueCompanyName = _CompanyDetails.CompanyName;

        $scope.company.Description = _CompanyDetails.Description;
        $scope.ValueDescription = _CompanyDetails.Description;

        $scope.company.Phone = _CompanyDetails.Phone;
        if(_CompanyDetails.Phone != '')
        $scope.ValuePhone = _CompanyDetails.Phone;

        $scope.company.Email = _CompanyDetails.Email;
        $scope.ValueEmail = angular.copy( $scope.company.Email);


        $scope.company.Website = _CompanyDetails.Website;
        $scope.ValueWebsite = angular.copy($scope.company.Website);

        $scope.company.WorkHourFrom = _CompanyDetails.WorkHourFrom;
        $scope.company.WorkHourTo = _CompanyDetails.WorkHourTo;
   

        $(_CompanyDetails.Workdays).each(function (index) {
           
            $scope.company.Workdays.push( _CompanyDetails.Workdays[index] );
        })


        if (_CompanyDetails.WorkTime === undefined) {
            _CompanyDetails.WorkTime = new Object();
        }
        $scope.company.WorkTime = _CompanyDetails.WorkTime;
        $scope.ValueWorkTime = angular.copy($scope.company.WorkTime);

        $scope.CompanyCodeCountry = { Value: '' };
        $scope.CompanyPhoneNumber = { Value: '' };

        //get phone
        if ($scope.company.Phone != '')
        {
          //  _userManager
            _userManager.GetPhoneCode({ phoneNumber: $scope.company.Phone }).then(function (phone) {
                $scope.CompanyCodeCountry.Value = phone.CodeCountry;
                $scope.CompanyPhoneNumber.Value = phone.PhoneNumber;

            });
        }
        if ($scope.CompanyCodeCountry.Value == '')
            $scope.CompanyCodeCountry.Value = '65';

    }
    $scope.CheckValidEmail = function ()
    {
        $scope.company.ErrorEmail = '';

        if ($scope.company.Email === undefined) {
            return;
           
        }
        else
            $scope.SaveCompanyDetails();
    }

    $scope.CheckValidPhone = function () {
        $scope.company.ErrorPhone = '';
        var tempPhone = '+' + $scope.CompanyCodeCountry.Value + $scope.CompanyPhoneNumber.Value;
        _userManager.ValidPhone({ phoneNumber: tempPhone }).then(function (rs) {

            if (rs.ValidPhone == true) {
                $scope.SaveCompanyDetails();
            }
            else
                $scope.company.ErrorPhone = rs.PhoneNumber + " is invalid";
        }, function (errors) {

            $scope.company.ErrorPhone = " Is invalid";

            var errorObj = __errorHandler.ProcessErrors(errors);
            __errorHandler.Swal(errorObj, _sweetAlert);
        })
    }

    $scope.SaveCompanyDetails = function () {
        _CompanyDetails.WorkHourFrom = $scope.company.WorkHourFrom;
        _CompanyDetails.WorkHourTo = $scope.company.WorkHourTo;
      

        var lstWorkdaysSave = [];
        $($scope.company.Workdays).each(function (index, object) {
            lstWorkdaysSave.push($scope.company.Workdays[index])
        });
        _CompanyDetails.Workdays = lstWorkdaysSave;

        _CompanyDetails.Industry = $scope.company.Industry;
     

        _CompanyDetails.CompanyName = $scope.company.CompanyName;
       

        _CompanyDetails.Description = $scope.company.Description;
        var tempPhone = '+' + $scope.CompanyCodeCountry.Value + $scope.CompanyPhoneNumber.Value;
     
        if (tempPhone != $scope.company.Phone && $scope.company.ErrorPhone == '')
            $scope.company.Phone = tempPhone;

        _CompanyDetails.Phone = $scope.company.Phone;
    
        _CompanyDetails.Email = $scope.company.Email;
      
        _CompanyDetails.Website = $scope.company.Website;

        var lstRangs = [];

      
        $($scope.company.WorkTime.ranges).each(function (index, object) {
            var week = [];
            $($scope.company.WorkTime.ranges[index].weekdays).each(function (ix) {
                week.push(
                    {
                        name: $scope.company.WorkTime.ranges[index].weekdays[ix].name,
                        open: $scope.company.WorkTime.ranges[index].weekdays[ix].open
                    })
            });
            lstRangs.push(
                {
                    from: $scope.company.WorkTime.ranges[index].from,
                    to: $scope.company.WorkTime.ranges[index].to,
                    weekdays: week
                })
        })

       
        _CompanyDetails.WorkTime.ranges = lstRangs;
        var company = new Object();
        company.id = $scope.profile.AccountId;
        company.details = _CompanyDetails;

        _busAccService.SaveCompanyObjectDetails(company).then(function (response) {


            $scope.InitData();
        });
    }



}])
