var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.cellNav'], true);


//10.0 Others
myApp.getController('SearchOthersPreferenceController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter', 'VaultService',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter, _vaultService) {

    var vaultData = null;
    $scope.formData = null;
    var VaultForm = null;
    $scope.initSearchVault = function () {

        // Init all Form
        var formVault = {
            'FormName': $scope.query.groupName,
            'AccountId': $scope.delegateSearch.FromAccountId
        }

        // Begin init all Form
        _vaultService.GetFormVault(formVault).then(function (response) {
            vaultData = response;
            $scope.formData = response;
            VaultForm = response.value[$scope.query.formName];
            $scope.initDataForm();
        });
        // End init all Form
    }

    $scope.initSearchVault();

    $scope.initDataForm = function () {


        /* Permission */
        $scope.read = false;
        $scope.write = false;

        var lstPermission = [];
        if ($scope.query.mySelf == false)
        {
            if ($scope.delegateSearch.GroupVaultsPermission != undefined || $scope.delegateSearch.GroupVaultsPermission == '') {

                lstPermission = $scope.delegateSearch.GroupVaultsPermission;
                for (var i = 0; i < lstPermission.length; i++) {

                    if (lstPermission[i].jsonpath == "others") {
                        $scope.read = lstPermission[i].read;
                        $scope.write = lstPermission[i].write;
                    }
                };
            }
        }
        else
        {
            $scope.read = true;
            $scope.write = true;
        }
       
        /* End Permission */
        $scope.IsEdit = false;
     
        //preference
        $scope.preference = {
            _label: VaultForm.label,
            _name: VaultForm.name,
            _default: VaultForm.default,
            _privacy: VaultForm.privacy
        };

        if (VaultForm.value.sexuality === undefined) {
            VaultForm.value.sexuality = {
                'label': 'Sexuality',
                'value': ''
            }

        }

        if (VaultForm.value.languages === undefined) {
            VaultForm.value.languages = {
                'label': 'Languages',
                'value': []
            }
        }

        if (VaultForm.value.ethnicity === undefined) {
            VaultForm.value.ethnicity = {
                'label': 'Ethnicity',
                'value': ''
            }
        }

        if (VaultForm.value.religion === undefined) {
            VaultForm.value.religion = {
                'label': 'Religion',
                'value': ''
            }

        }

        if (VaultForm.value.disability === undefined) {
            VaultForm.value.disability = {
                'label': 'Disability',
                'value': ''
            }

        }

        $scope.preference_sexuality = {
            _label: VaultForm.value.sexuality.label,
            _value: VaultForm.value.sexuality.value
        };

        $scope.preference_languages = {
            _label: VaultForm.value.languages.label,
            _value: VaultForm.value.languages.value
        };

        $scope.preference_ethnicity = {
            _label: VaultForm.value.ethnicity.label,
            _value: VaultForm.value.ethnicity.value
        };

        $scope.preference_religion = {
            _label: VaultForm.value.religion.label,
            _value: VaultForm.value.religion.value
        };

        $scope.preference_disability = {
            _label: VaultForm.value.disability.label,
            _value: VaultForm.value.disability.value
        };


        $scope.preference_interests = {
            _label: VaultForm.value.interests.label,
            _value: VaultForm.value.interests.value
        };
        $scope.preference_sports = {
            _label: VaultForm.value.sports.label,
            _value: VaultForm.value.sports.value
        };

        $scope.preference_food = {
            _label: VaultForm.value.food.label,
            _value: VaultForm.value.food.value
        };

        if (VaultForm.value.allergies === undefined) {
            VaultForm.value.allergies = {
                'label': 'Allergies',
                'value': []
            }
        }

        $scope.preference_allergies = {
            _label: VaultForm.value.allergies.label,
            _value: VaultForm.value.allergies.value
        };


        $scope.preference_seat = {
            _label: VaultForm.value.seat.label,
            _value: VaultForm.value.seat.value
        };

        $scope.preference_season = {
            _label: VaultForm.value.season.label,
            _value: VaultForm.value.season.value
        };
        $scope.preference_note = {
            _label: VaultForm.value.note.label,
            _value: VaultForm.value.note.value
        };

        $scope.messageBox = "";
        $scope.alerts = [];
    }
    // === // ===    

    $scope.Edit = function () {
        $scope.IsEdit = true;
    }



    $scope.Save = function () {  

        //preference
        VaultForm.label = $scope.preference._label;
        if ($scope.preference._name == '') {
            $scope.preference._name = $scope.preference._label;
        }

        VaultForm.name = $scope.preference._name;
        VaultForm.default = $scope.preference._default;
        VaultForm.privacy = $scope.preference._privacy;

        //new preference_sexuality
        VaultForm.value.sexuality.value = $scope.preference_sexuality._value;

        VaultForm.value.languages.value = $scope.preference_languages._value;
        VaultForm.value.ethnicity.value = $scope.preference_ethnicity._value;
        VaultForm.value.religion.value = $scope.preference_religion._value;
        VaultForm.value.disability.value = $scope.preference_disability._value;

        //
        VaultForm.value.interests.label = $scope.preference_interests._label;
        VaultForm.value.interests.value = $scope.preference_interests._value;

        //preference_sports

        VaultForm.value.sports.label = $scope.preference_sports._label;
        VaultForm.value.sports.value = $scope.preference_sports._value;
        //preference_food
        VaultForm.value.food.label = $scope.preference_food._label;
        VaultForm.value.food.value = $scope.preference_food._value;

        //preference_allergies
        VaultForm.value.allergies.label = $scope.preference_allergies._label;
        VaultForm.value.allergies.value = $scope.preference_allergies._value;


        //preference_seat
        VaultForm.value.seat.label = $scope.preference_seat._label;
        VaultForm.value.seat.value = $scope.preference_seat._value;

        //preference_season
        VaultForm.value.season.label = $scope.preference_season._label;
        VaultForm.value.season.value = $scope.preference_season._value;

        //preference_note 
        VaultForm.value.note.label = $scope.preference_note._label;
        VaultForm.value.note.value = $scope.preference_note._value;


        // Save Form Vault
       // VaultForm.value = lstFormSave;
        vaultData.value[$scope.query.formName] = VaultForm;

        var saveVaultForm = {
            'AccountId': $scope.delegateSearch.FromAccountId,
            'FormName': $scope.query.groupName,
            'FormString': vaultData
        }

        _vaultService.UpdateFormVault(saveVaultForm).then(function () {

                $scope.IsEdit = false;       

            }, function (errors) {

            });
    }

    $scope.saveVaultInformationOnSuccess = function (response) {
        $scope.IsEdit = false;
      
    }
    $scope.saveVaultInformationOnError = function (response) {
        alertService.renderSuccessMessage(response.ReturnMessage);
        $scope.messageBox = alertService.returnFormattedMessage();
        $scope.alerts = alertService.returnAlerts();
    }

    $scope.Cancel = function () {
        $scope.initDataForm();

    }

    $scope.goBack = function () {
        $scope.$location.path('/VaultInformation');
    };
    $scope.closePanel = function () {
        $scope.Cancel();
    };

}])

//10.0 Others
myApp.getController('SearchOthersFavouriteController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter', 'VaultService',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter, _vaultService) {
    var vaultData = null;
    $scope.formData = null;
    var VaultForm = null;
    $scope.initSearchVault = function () {

        // Init all Form
        var formVault = {
            'FormName': $scope.query.groupName,
            'AccountId': $scope.delegateSearch.FromAccountId
        }

        // Begin init all Form
        _vaultService.GetFormVault(formVault).then(function (response) {
            vaultData = response;
            $scope.formData = response;
            VaultForm = response.value[$scope.query.formName];
            $scope.initDataForm();
        });
        // End init all Form
    }

    $scope.initSearchVault();

    $scope.initDataForm = function () {


        /* Permission */
        $scope.read = false;
        $scope.write = false;

        var lstPermission = [];
        if ($scope.query.mySelf == false) {
            if ($scope.delegateSearch.GroupVaultsPermission != undefined || $scope.delegateSearch.GroupVaultsPermission == '') {

                lstPermission = $scope.delegateSearch.GroupVaultsPermission;
                for (var i = 0; i < lstPermission.length; i++) {

                    if (lstPermission[i].jsonpath == "others") {
                        $scope.read = lstPermission[i].read;
                        $scope.write = lstPermission[i].write;
                    }
                };
            }
        }
        else {
            $scope.read = true;
            $scope.write = true;
        }

        /* End Permission */

        //favourite
            $scope.IsEdit = false;


      
        $scope.favourite = {
            _label: VaultForm.label,
            _name: VaultForm.name,
            _default: VaultForm.default,
            _privacy: VaultForm.privacy
        };

        $scope.favourite_colour = {
            _label: VaultForm.value.colour.label,
            _value: VaultForm.value.colour.value
        };

        $scope.favourite_holiday = {
            _label: VaultForm.value.holiday.label,
            _value: VaultForm.value.holiday.value
        };
        $scope.favourite_movie = {
            _label: VaultForm.value.movie.label,
            _value: VaultForm.value.movie.value
        };

        $scope.favourite_music_type = {
            _label: VaultForm.value.music_type.label,
            _value: VaultForm.value.music_type.value
        };
        $scope.favourite_song = {
            _label: VaultForm.value.song.label,
            _value: VaultForm.value.song.value
        };
        $scope.favourite_tv_show = {
            _label: VaultForm.value.tv_show.label,
            _value: VaultForm.value.tv_show.value
        };
        $scope.favourite_personality = {
            _label: VaultForm.value.personality.label,
            _value: VaultForm.value.personality.value
        };
        $scope.favourite_preferred_time = {
            _label: VaultForm.value.preferred_time.label,
            _value: VaultForm.value.preferred_time.value
        };

        $scope.favourite_note = {
            _label: VaultForm.value.note.label,
            _value: VaultForm.value.note.value
        };



        $scope.messageBox = "";
        $scope.alerts = [];
    }



    // === // ===    

    $scope.Edit = function () {
        $scope.IsEdit = true;
    }



    $scope.Save = function () {



        //favourite

        VaultForm.label = $scope.favourite._label;
        if ($scope.favourite._name == '') {
            $scope.favourite._name = $scope.favourite._label;
        }
        VaultForm.name = $scope.favourite._name;
        VaultForm.default = $scope.favourite._default;
        VaultForm.privacy = $scope.favourite._privacy;

        //favourite_colour Clour

        VaultForm.value.colour.label = $scope.favourite_colour._label;
        VaultForm.value.colour.value = $scope.favourite_colour._value;


        //favourite_holiday
        VaultForm.value.holiday.label = $scope.favourite_holiday._label;
        VaultForm.value.holiday.value = $scope.favourite_holiday._value;

        //favourite_movie
        VaultForm.value.movie.label = $scope.favourite_movie._label;
        VaultForm.value.movie.value = $scope.favourite_movie._value;


        //favourite_music_type
        VaultForm.value.music_type.label = $scope.favourite_music_type._label;
        VaultForm.value.music_type.value = $scope.favourite_music_type._value;

        //favourite_song
        VaultForm.value.song.label = $scope.favourite_song._label;
        VaultForm.value.song.value = $scope.favourite_song._value;

        //favourite_tv_show
        VaultForm.value.tv_show.label = $scope.favourite_tv_show._label;
        VaultForm.value.tv_show.value = $scope.favourite_tv_show._value;

        //favourite_personality
        VaultForm.value.personality.label = $scope.favourite_personality._label;
        VaultForm.value.personality.value = $scope.favourite_personality._value;


        //favourite_preferred_time
        VaultForm.value.preferred_time.label = $scope.favourite_preferred_time._label;
        VaultForm.value.preferred_time.value = $scope.favourite_preferred_time._value;


        //favourite_note
        VaultForm.value.note.label = $scope.favourite_note._label;
        VaultForm.value.note.value = $scope.favourite_note._value;
        // Save Form Vault
        // VaultForm.value = lstFormSave;
        vaultData.value[$scope.query.formName] = VaultForm;

        var saveVaultForm = {
            'AccountId': $scope.delegateSearch.FromAccountId,
            'FormName': $scope.query.groupName,
            'FormString': vaultData
        }

        _vaultService.UpdateFormVault(saveVaultForm).then(function () {

            $scope.IsEdit = false;

        }, function (errors) {

        });
    }

    $scope.saveVaultInformationOnSuccess = function (response) {
        $scope.IsEdit = false;

    }
    $scope.saveVaultInformationOnError = function (response) {
        alertService.renderSuccessMessage(response.ReturnMessage);
        $scope.messageBox = alertService.returnFormattedMessage();
        $scope.alerts = alertService.returnAlerts();
    }

    $scope.Cancel = function () {
        $scope.initDataForm();

    }

    $scope.goBack = function () {
        $scope.$location.path('/VaultInformation');
    };
    $scope.closePanel = function () {
        $scope.Cancel();
    };

}])

//10.0 Others
myApp.getController('SearchOthersBodyController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter', 'VaultService',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter, _vaultService) {

    "use strict";
    var vaultData = null;
    $scope.formData = null;
    var VaultForm = null;
    $scope.initSearchVault = function () {

        // Init all Form
        var formVault = {
            'FormName': $scope.query.groupName,
            'AccountId': $scope.delegateSearch.FromAccountId
        }

        // Begin init all Form
        _vaultService.GetFormVault(formVault).then(function (response) {
            vaultData = response;
            $scope.formData = response;
            VaultForm = response.value[$scope.query.formName];
            $scope.initDataForm();
        });
        // End init all Form
    }

    $scope.initSearchVault();

    $scope.initDataForm = function () {

        /* Permission */
        $scope.read = false;
        $scope.write = false;

        var lstPermission = [];
        if ($scope.query.mySelf == false) {
            if ($scope.delegateSearch.GroupVaultsPermission != undefined || $scope.delegateSearch.GroupVaultsPermission == '') {

                lstPermission = $scope.delegateSearch.GroupVaultsPermission;
                for (var i = 0; i < lstPermission.length; i++) {

                    if (lstPermission[i].jsonpath == "others") {
                        $scope.read = lstPermission[i].read;
                        $scope.write = lstPermission[i].write;
                    }
                };
            }
        }
        else {
            $scope.read = true;
            $scope.write = true;
        }

        /* End Permission */

        //favourite
        $scope.IsEdit = false;

        $scope.body = {
            _label: VaultForm.label,
            _name: VaultForm.name,
            _default: VaultForm.default,
            _privacy: VaultForm.privacy
        };

        $scope.body_hair_colour = {
            _label: VaultForm.value.hair_colour.label,
            _value: VaultForm.value.hair_colour.value
        };

        $scope.body_eye_colour = {
            _label: VaultForm.value.eye_colour.label,
            _value: VaultForm.value.eye_colour.value
        };
        $scope.body_height = {
            _label: VaultForm.value.height.label,
            _value: VaultForm.value.height.value,
            _unit: VaultForm.value.height.unit
        };

        $scope.body_weight = {
            _label: VaultForm.value.weight.label,
            _value: VaultForm.value.weight.value,
            _unit: VaultForm.value.weight.unit
        };

        $scope.body_type = {
            _label: VaultForm.value.type.label,
            _value: VaultForm.value.type.value
        };
        $scope.body_note_body = {
            _label: VaultForm.value.note_body.label,
            _value: VaultForm.value.note_body.value
        };


        $scope.body_neck = {
            _label: VaultForm.value.neck.label,
            _value: VaultForm.value.neck.value,
            _unit: VaultForm.value.neck.unit
        }

        $scope.body_chest = {
            _label: VaultForm.value.chest.label,
            _value: VaultForm.value.chest.value,
            _unit: VaultForm.value.chest.unit
        };


        $scope.body_arm = {
            _label: VaultForm.value.arm.label,
            _value: VaultForm.value.arm.value,
            _unit: VaultForm.value.arm.unit
        };


        $scope.body_leg = {
            _label: VaultForm.value.leg.label,
            _value: VaultForm.value.leg.value,
            _unit: VaultForm.value.leg.unit
        };


        $scope.body_inner = {
            _label: VaultForm.value.inner.label,
            _value: VaultForm.value.inner.value,
            _unit: VaultForm.value.inner.unit
        };

        $scope.body_bicep = {
            _label: VaultForm.value.bicep.label,
            _value: VaultForm.value.bicep.value,
            _unit: VaultForm.value.bicep.unit
        };
        $scope.body_shoes = {
            _label: VaultForm.value.shoes.label,
            _value: VaultForm.value.shoes.value,
            _unit: VaultForm.value.shoes.unit
        };

        $scope.body_ring = {
            _label: VaultForm.value.ring.label,
            _value: VaultForm.value.ring.value,
            _unit: VaultForm.value.ring.unit
        };

        $scope.body_note_tailor = {
            _label: VaultForm.value.note_tailor.label,
            _value: VaultForm.value.note_tailor.value,

        };


        $scope.body_right_eye = {
            _label: VaultForm.value.right_eye.label,
            _value: VaultForm.value.right_eye.value
        };

        $scope.body_left_eye = {
            _label: VaultForm.value.left_eye.label,
            _value: VaultForm.value.left_eye.value
        };
        $scope.body_note_eye = {
            _label: VaultForm.value.note_eye.label,
            _value: VaultForm.value.note_eye.value
        };


        $scope.messageBox = "";
        $scope.alerts = [];
    }



    // === // ===    

    $scope.Edit = function () {
        $scope.IsEdit = true;
    }



    $scope.Save = function () {


        //body


        VaultForm.label = $scope.body._label;
        if ($scope.body._name == '') {
            $scope.body._name = $scope.body._label;
        }
        VaultForm.name = $scope.body._name;
        VaultForm.default = $scope.body._default;
        VaultForm.privacy = $scope.body._privacy;

        //body_hair_colour Hair Clour

        VaultForm.value.hair_colour.label = $scope.body_hair_colour._label;
        VaultForm.value.hair_colour.value = $scope.body_hair_colour._value;

        //body_eye_colour
        VaultForm.value.eye_colour.label = $scope.body_eye_colour._label;
        VaultForm.value.eye_colour.value = $scope.body_eye_colour._value;

        //body_height
        VaultForm.value.height.label = $scope.body_height._label;
        VaultForm.value.height.value = $scope.body_height._value;
        VaultForm.value.height.unit = $scope.body_height._unit;


        //body_weight
        VaultForm.value.weight.label = $scope.body_weight._label;
        VaultForm.value.weight.value = $scope.body_weight._value;
        VaultForm.value.weight.unit = $scope.body_weight._unit;

        //body_type
        VaultForm.value.type.label = $scope.body_type._label;
        VaultForm.value.type.value = $scope.body_type._value;

        //note_body
        VaultForm.value.note_body.label = $scope.body_note_body._label;
        VaultForm.value.note_body.value = $scope.body_note_body._value;

        //body_weight
        VaultForm.value.neck.label = $scope.body_neck._label;
        VaultForm.value.neck.value = $scope.body_neck._value;
        VaultForm.value.neck.unit = $scope.body_neck._unit;

        //body_chest
        VaultForm.value.chest.label = $scope.body_chest._label;
        VaultForm.value.chest.value = $scope.body_chest._value;
        VaultForm.value.chest.unit = $scope.body_chest._unit;

        //body_arm
        VaultForm.value.arm.label = $scope.body_arm._label;
        VaultForm.value.arm.value = $scope.body_arm._value;
        VaultForm.value.arm.unit = $scope.body_arm._unit;
        //body_leg
        VaultForm.value.leg.label = $scope.body_leg._label;
        VaultForm.value.leg.value = $scope.body_leg._value;
        VaultForm.value.leg.unit = $scope.body_leg._unit;

        //body_inner
        VaultForm.value.inner.label = $scope.body_inner._label;
        VaultForm.value.inner.value = $scope.body_inner._value;
        VaultForm.value.inner.unit = $scope.body_inner._unit;

        //body_bicep
        VaultForm.value.bicep.label = $scope.body_bicep._label;
        VaultForm.value.bicep.value = $scope.body_bicep._value;
        VaultForm.value.bicep.unit = $scope.body_bicep._unit;
        //body_shoes
        VaultForm.value.shoes.label = $scope.body_shoes._label;
        VaultForm.value.shoes.value = $scope.body_shoes._value;
        VaultForm.value.shoes.unit = $scope.body_shoes._unit;

        //body_ring
        VaultForm.value.ring.label = $scope.body_ring._label;
        VaultForm.value.ring.value = $scope.body_ring._value;
        VaultForm.value.ring.unit = $scope.body_ring._unit;
        //body_note_tailor
        VaultForm.value.note_tailor.label = $scope.body_note_tailor._label;
        VaultForm.value.note_tailor.value = $scope.body_note_tailor._value;

        //note_right_eye
        VaultForm.value.right_eye.label = $scope.body_right_eye._label;
        VaultForm.value.right_eye.value = $scope.body_right_eye._value;


        //note_left_eye
        VaultForm.value.left_eye.label = $scope.body_left_eye._label;
        VaultForm.value.left_eye.value = $scope.body_left_eye._value;

        //note_note_eye
        VaultForm.value.note_eye.label = $scope.body_note_eye._label;
        VaultForm.value.note_eye.value = $scope.body_note_eye._value;
        // Save Form Vault
        // VaultForm.value = lstFormSave;
        vaultData.value[$scope.query.formName] = VaultForm;

        var saveVaultForm = {
            'AccountId': $scope.delegateSearch.FromAccountId,
            'FormName': $scope.query.groupName,
            'FormString': vaultData
        }

        _vaultService.UpdateFormVault(saveVaultForm).then(function () {

            $scope.IsEdit = false;

        }, function (errors) {

        });
    }

    $scope.saveVaultInformationOnSuccess = function (response) {
        $scope.IsEdit = false;

    }
    $scope.saveVaultInformationOnError = function (response) {
        alertService.renderSuccessMessage(response.ReturnMessage);
        $scope.messageBox = alertService.returnFormattedMessage();
        $scope.alerts = alertService.returnAlerts();
    }

    $scope.Cancel = function () {
        $scope.initDataForm();

    }

    $scope.goBack = function () {
        $scope.$location.path('/VaultInformation');
    };
    $scope.closePanel = function () {
        $scope.Cancel();
    };

}])


