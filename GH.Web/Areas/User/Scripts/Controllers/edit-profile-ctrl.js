myApp.getController('EditProfileController', ['$scope', '$rootScope', '$http', '$moment',  'AuthorizationService', 'UserManagementService', 'SweetAlert', 'DataService', '$timeout', 'fileUpload', function ($scope, $rootScope, $http, $moment, _authService, _userManager, _sweetAlert, _dataService, $timeout, fileUpload) {

    $scope.profileUrl = regitGlobal.server.baseUrl + '/user/profile/' + regitGlobal.userAccount.id;
 
    // Regit Profile
    $scope.privacy = {
        'CountryCity': 'public',
        'DOB': 'public',
        'PhotoUrl': 'public',
        'Phone': 'public',
        'Email': 'public',
        'Profile': 'public'
    };


    $scope.ProfilePrivacy = {
        'AccountId': '',
        'ListField': []
    };

    $scope.updatePrivacy = function () {
        var id = $scope.profile.AccountId;

        var privacy = {
            'AccountId': id,
            'ListField': []
        }
        privacy.ListField = [
            { 'Field': 'CountryCity', 'Role': $scope.privacy.CountryCity },
            { 'Field': 'DOB', 'Role': $scope.privacy.DOB },
            { 'Field': 'PhotoUrl', 'Role': $scope.privacy.PhotoUrl },
            { 'Field': 'Phone', 'Role': $scope.privacy.Phone },
            { 'Field': 'Email', 'Role': $scope.privacy.Email },
            { 'Field': 'Profile', 'Role': $scope.privacy.Profile }
        ]
        _authService.UpdatePrivacy(privacy)
           .then(function (rs) {
               $scope.ProfilePrivacyId = rs;

           },
               function (errors) { });
    }

    $scope.InitDataPrivacy = function () {
        if ($scope.ProfilePrivacy) {
            for (var i = 0; i < $scope.ProfilePrivacy.ListField.length; i++) {

                if ($scope.ProfilePrivacy.ListField[i].Field == 'CountryCity')
                    $scope.privacy.CountryCity = $scope.ProfilePrivacy.ListField[i].Role;
                else if ($scope.ProfilePrivacy.ListField[i].Field == 'DOB')
                    $scope.privacy.DOB = $scope.ProfilePrivacy.ListField[i].Role;
                else if ($scope.ProfilePrivacy.ListField[i].Field == 'PhotoUrl')
                    $scope.privacy.PhotoUrl = $scope.ProfilePrivacy.ListField[i].Role;
                else if ($scope.ProfilePrivacy.ListField[i].Field == 'Phone')
                    $scope.privacy.Phone = $scope.ProfilePrivacy.ListField[i].Role;
                else if ($scope.ProfilePrivacy.ListField[i].Field == 'Email')
                    $scope.privacy.Email = $scope.ProfilePrivacy.ListField[i].Role;
                else if ($scope.ProfilePrivacy.ListField[i].Field == 'Profile')
                    $scope.privacy.Profile = $scope.ProfilePrivacy.ListField[i].Role;
            }
        }
    }
    // End Privacy
    $scope.profile = {}, $scope.editor = {};
    _dataService.getAllCountry().then(function (res) {
        $scope.countries = res;
        _userManager.GetCurrentUserProfile().then(function (profile) {
            $scope.profile = profile;
            //
            var id = profile.AccountId;
            _authService.GetPrivacy(id).then(function (rs) {
                $scope.ProfilePrivacy = rs;
                $scope.InitDataPrivacy();
            });

            $scope.editor = angular.copy($scope.profile);
            var country = $scope.countries.findItem('Name', $scope.profile.Country);
            if (!country)
                return;
            _dataService.getCityByCountry(country.Code).then(function (res) {
                $scope.cities = res;
                // add profile city to city list if not in the list
                var FindCity = $scope.cities.findItem('Name', $scope.profile.City);
                if (!FindCity) {                    
                    var lastCity = {};
                    lastCity.Id = '';
                    lastCity.Name = $scope.profile.City;
                    lastCity.Latitude = '';
                    lastCity.Longitude = '';
                    $scope.cities[$scope.cities.length] = lastCity;
                }
            })
        })
    })

    $scope.searchValueCity = null;
    $scope.onCountrySelect = function (country) {
        if (!country)
            return;
        $scope.editor.City = undefined;
    
        _dataService.getCityByCountry(country.Code).then(function (res) {
            $scope.cities = res;
        });
    }

    $scope.onCitySelect = function (city) {        
        if (!city) {
            // assign search value to editor city
            $scope.editor.City = $scope.searchValueCity;

            // add search value to city list if not in the list
            var FindCity = $scope.cities.findItem('Name', $scope.editor.City);
            if (!FindCity) {
                var lastCity = {};
                lastCity.Id = '';
                lastCity.Name = $scope.editor.City;
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
            $scope.editor.City = $scope.searchValueCity;
        }
    }

    $scope.editting = {};

    $scope.edit = function () {
        var edittingFields = '';
        for (var i = 0; i < arguments.length; i++) {
            if (arguments[i] == 'Birthdate')
                $scope.editor.Birthdate = $scope.profile.Birthdate

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

        // add search value to city list if not in the list
        if ($scope.editor.City != null)
        {
            var FindCity = $scope.cities.findItem('Name', $scope.editor.City);
            if (!FindCity) {
                var lastCity = {};
                lastCity.Id = '';
                lastCity.Name = $scope.editor.City;
                lastCity.Latitude = '';
                lastCity.Longitude = '';
                $scope.cities[$scope.cities.length] = lastCity;
            }
        }
  

        var saveModel = angular.copy($scope.profile);
        saveModel.UpdateFields = [];

        for (var i = 0; i < arguments.length; i++) {
            edittingFields += arguments[i];
            if (arguments[i] == 'Birthdate') {

                $scope.editor.Birthdate = $scope.editor.Birthdate.toISOString();
            }

            saveModel.UpdateFields.push(arguments[i]);
            if (arguments[i] != 'PhotoUrl') {
                //Vu

                saveModel[arguments[i]] = $scope.editor[arguments[i]];
                if (arguments[i] == 'DisplayName') {
                    if (!saveModel[arguments[i]]) {
                        __common.swal(_sweetAlert, $rootScope.translate('Warning_Title'),
                            $rootScope.translate('EditProfileSettings_DisplayName_Required'),
                            'warning');
                        return;
                    } else if (saveModel[arguments[i]].length > 128) {
                        __common.swal(_sweetAlert, $rootScope.translate('Warning_Title'),
                            $rootScope.translate('EditProfileSettings_DisplayName_MaxLength'),
                            'warning');
                        return;
                    }
                }
                if (arguments[i] == 'Status' && saveModel[arguments[i]] && saveModel[arguments[i]].length > 128) {
                    _sweetAlert.swal($rootScope.translate('Warning_Title'), $rootScope.translate('EditProfileSettings_Status_MaxLength'), 'warning');
                    return;
                }
            } else {
                if ($scope.view.cropper) {
                    $scope.view.cropper.getCroppedCanvas().toBlob(function (blob) {
                        var fileReader = new FileReader();
                        fileReader.readAsDataURL(blob);
                        fileReader.onload = function (e) {
                            $scope.$apply(function () {
                                $scope.editor.NewProfilePicture = e.target.result;
                            });
                        };
                        var fname = $scope.editor.hasOwnProperty('__files') &&  $scope.editor.files.hasOwnProperty('name') ? $scope.editor.__files[0].name : 'new.jpg';
                        var file = new File([blob], fname, {
                            type: "image/jpeg"
                        });
                        fileUpload.uploadFileToUrl(file, '/Api/AccountSettings/UpdateProfilePictureFileName', function (response) {
                            if (response.hasOwnProperty('photoUrl')) {
                                $scope.$evalAsync(function() {
                                    $scope.regitGlobal.activeAccount.avatar = $scope.user.PhotoUrl = $scope.profile.PhotoUrl = response.photoUrl;
                                });
                            }
                        }, function () {
                        }, "");
                     
                        saveModel['PhotoUrl'] = file;
                    } , 'image/jpeg');
                } else {
                    if ($scope.editor.__files && $scope.editor.__files.length) {
                        saveModel['PhotoUrl'] = $scope.editor.__files[0];
                    } else {
                        __common.swal(_sweetAlert, $rootScope.translate('Warning_Title'),
                            $rootScope.translate('EditProfileSettings_PhotoUrl_Required'),
                            'warning');
                        return;
                    }
                }
            }
        }

        _userManager.UpdateProfile(saveModel).then(function (updatedProfile) {
            $scope.editting[edittingFields] = false;
            $scope.profile = updatedProfile;
            //
            $scope.regitGlobal.activeAccount.avatar = $scope.user.PhotoUrl = $scope.profile.PhotoUrl = updatedProfile.PhotoUrl;
            $scope.regitGlobal.activeAccount.displayName = $scope.user.DisplayName = updatedProfile.DisplayName;
            if ($scope.profile.Birthdate) {
                $scope.editor.Birthdate = new Date($scope.editor.Birthdate);
                $scope.editor.Birthdate = $moment($scope.editor.Birthdate).toISOString();
            }
            $rootScope.currentUserProfile = updatedProfile;
           

            $scope.view.cropper = null;

        }, function (errors) {
            var errorObj = __errorHandler.ProcessErrors(errors);
            __errorHandler.Swal(errorObj, _sweetAlert);
        })
   

    };

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
            __common.swal(_sweetAlert, "warning",
                $rootScope.translate('EditProfileSettings_NewProfilePicture_Required'),
                'warning');
        }

        $scope.view.cropper = null;
    };

    // Test only picture

    $scope.getFileDetails = function (e) {

        $scope.files = [];
        $scope.$apply(function () {

            // STORE THE FILE OBJECT IN AN ARRAY.
            for (var i = 0; i < e.files.length; i++) {
                $scope.files.push(e.files[i])
            }

        });
    };

    $scope.view = {
        editingImage: false,
        cropper: null
    };

    $scope.openImageEditor = function () {
        var image = document.getElementById('editing-image');
        if (!image) return;
        $scope.view.editingImage = true;
        $scope.view.cropper = new Cropper(image, {
            aspectRatio: 1,
            crop: function (e) {
            }
        });
    };

    $scope.closeImageEditor = function (save) {
        $scope.view.editingImage = false;
        if (!$scope.view.cropper) return;
        $scope.view.cropper = null;
    };

    $scope.exportQR = function (event) {
        var canvas = document.querySelector('qr canvas');
        // Canvas2Image.saveAsPNG(canvas, 400, 400);
        var link = event.target;
        link.href = canvas.toDataURL();
        link.download = 'QR-UserProfile-' + regitGlobal.userAccount.id + '.png';
    };


}]);

