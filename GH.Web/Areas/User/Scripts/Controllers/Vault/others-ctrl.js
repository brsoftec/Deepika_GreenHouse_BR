var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.cellNav'], true);

//10.0 Others
myApp.getController('OthersController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter) {

    "use strict";
    var Info = VaultInformationService.VaultInformation.basicInformation;
    var BasicInfo = VaultInformationService.VaultInformation.others;
  
    $scope.InitData = function () {
        $scope.IsEdit = false;
        // General   
        $scope.others = {
            _label: BasicInfo.label,
            _name: BasicInfo.name,
            _default: BasicInfo.default,
            _privacy: BasicInfo.privacy
        };

        //list_others
        $scope.list_others = [BasicInfo.value.preference.label, BasicInfo.value.favourite.label, BasicInfo.value.body.label];
        $scope.select_others = { _value: '' };
        if ($scope.others._default == '') {
            $scope.others._default = BasicInfo.value.preference.label;
        }

        if ($scope.select_others._value == '') {
            $scope.select_others._value = $scope.others._default;
        }

        //preference
        $scope.preference = {
            _label: BasicInfo.value.preference.label,
            _name: BasicInfo.value.preference.name,
            _default: BasicInfo.value.preference.default,
            _privacy: BasicInfo.value.preference.privacy
        };

        if (BasicInfo.value.preference.value.sexuality === undefined)
        {
            BasicInfo.value.preference.value.sexuality = {
                'label': 'Sexuality',
                'value': ''
            }
           
        }

        if (BasicInfo.value.preference.value.languages === undefined)
        {
            BasicInfo.value.preference.value.languages = {
                'label': 'Languages',
                'value': []
            }
           
       
        }
          
        if (BasicInfo.value.preference.value.ethnicity === undefined)
        {
            BasicInfo.value.preference.value.ethnicity = {
                'label': 'Ethnicity',
                'value': ''
            }  
        }
            
     
        if (BasicInfo.value.preference.value.religion === undefined)
        {
            BasicInfo.value.preference.value.religion = {
                'label': 'Religion',
                'value': ''
            }
           
        }
          
        if (BasicInfo.value.preference.value.disability === undefined)
        {
            BasicInfo.value.preference.value.disability = {
                'label': 'Disability',
                'value': ''
            }
         
        }
           
        $scope.preference_sexuality = {
            _label: BasicInfo.value.preference.value.sexuality.label,
            _value: BasicInfo.value.preference.value.sexuality.value
        };

        $scope.preference_languages = {
            _label: BasicInfo.value.preference.value.languages.label,
            _value: BasicInfo.value.preference.value.languages.value
        };

        $scope.preference_ethnicity = {
            _label: BasicInfo.value.preference.value.ethnicity.label,
            _value: BasicInfo.value.preference.value.ethnicity.value
        };
     
        $scope.preference_religion = {
            _label: BasicInfo.value.preference.value.religion.label,
            _value: BasicInfo.value.preference.value.religion.value
        };
   
        $scope.preference_disability = {
            _label: BasicInfo.value.preference.value.disability.label,
            _value: BasicInfo.value.preference.value.disability.value
        };


        $scope.preference_interests = {
            _label: BasicInfo.value.preference.value.interests.label,
            _value: BasicInfo.value.preference.value.interests.value
        };
        $scope.preference_sports = {
            _label: BasicInfo.value.preference.value.sports.label,
            _value: BasicInfo.value.preference.value.sports.value
        };

        $scope.preference_food = {
            _label: BasicInfo.value.preference.value.food.label,
            _value: BasicInfo.value.preference.value.food.value
        };

        if (BasicInfo.value.preference.value.allergies === undefined) {
            BasicInfo.value.preference.value.allergies = {
                'label': 'Allergies',
                'value': []
            }
          

        }

        $scope.preference_allergies = {
            _label: BasicInfo.value.preference.value.allergies.label,
            _value: BasicInfo.value.preference.value.allergies.value
        };
      

        $scope.preference_seat = {
            _label: BasicInfo.value.preference.value.seat.label,
            _value: BasicInfo.value.preference.value.seat.value
        };

        $scope.preference_season = {
            _label: BasicInfo.value.preference.value.season.label,
            _value: BasicInfo.value.preference.value.season.value
        };
        $scope.preference_note = {
            _label: BasicInfo.value.preference.value.note.label,
            _value: BasicInfo.value.preference.value.note.value
        };

        //favourite season
        $scope.IsEdit_favourite = false;
        $scope.favourite = {
            _label: BasicInfo.value.favourite.label,
            _name: BasicInfo.value.favourite.name,
            _default: BasicInfo.value.favourite.default,
            _privacy: BasicInfo.value.favourite.privacy
        };

        $scope.favourite_colour = {
            _label: BasicInfo.value.favourite.value.colour.label,
            _value: BasicInfo.value.favourite.value.colour.value
        };

        $scope.favourite_holiday = {
            _label: BasicInfo.value.favourite.value.holiday.label,
            _value: BasicInfo.value.favourite.value.holiday.value
        };
        $scope.favourite_movie = {
            _label: BasicInfo.value.favourite.value.movie.label,
            _value: BasicInfo.value.favourite.value.movie.value
        };

        $scope.favourite_music_type = {
            _label: BasicInfo.value.favourite.value.music_type.label,
            _value: BasicInfo.value.favourite.value.music_type.value
        };
        $scope.favourite_song = {
            _label: BasicInfo.value.favourite.value.song.label,
            _value: BasicInfo.value.favourite.value.song.value
        };
        $scope.favourite_tv_show = {
            _label: BasicInfo.value.favourite.value.tv_show.label,
            _value: BasicInfo.value.favourite.value.tv_show.value
        };
        $scope.favourite_personality = {
            _label: BasicInfo.value.favourite.value.personality.label,
            _value: BasicInfo.value.favourite.value.personality.value
        };
        if (BasicInfo.value.favourite.value.preferred_time == undefined)
        {
            BasicInfo.value.favourite.value.preferred_time = {
                'label': 'Preferred time of day',
                'value': ''
            }
        }
        $scope.favourite_preferred_time = {
            _label: BasicInfo.value.favourite.value.preferred_time.label,
            _value: BasicInfo.value.favourite.value.preferred_time.value
        };

        $scope.favourite_note = {
            _label: BasicInfo.value.favourite.value.note.label,
            _value: BasicInfo.value.favourite.value.note.value
        };


        //Body

        //$scope.bodyTypes = ['Average', 'Slim', 'Athletic', 'Obese'];


        $scope.IsEdit_body = false;
        $scope.body = {
            _label: BasicInfo.value.body.label,
            _name: BasicInfo.value.body.name,
            _default: BasicInfo.value.body.default,
            _privacy: BasicInfo.value.body.privacy
        };

        $scope.body_hair_colour = {
            _label: BasicInfo.value.body.value.hair_colour.label,
            _value: BasicInfo.value.body.value.hair_colour.value
        };

        $scope.body_eye_colour = {
            _label: BasicInfo.value.body.value.eye_colour.label,
            _value: BasicInfo.value.body.value.eye_colour.value
        };
        $scope.body_height = {
            _label: BasicInfo.value.body.value.height.label,
            _value: BasicInfo.value.body.value.height.value,
            _unit: BasicInfo.value.body.value.height.unit
        };

        $scope.body_weight = {
            _label: BasicInfo.value.body.value.weight.label,
            _value: BasicInfo.value.body.value.weight.value,
            _unit: BasicInfo.value.body.value.weight.unit
        };

        $scope.body_type = {
            _label: BasicInfo.value.body.value.type.label,
            _value: BasicInfo.value.body.value.type.value
        };
        $scope.body_note_body = {
            _label: BasicInfo.value.body.value.note_body.label,
            _value: BasicInfo.value.body.value.note_body.value
        };


        $scope.body_neck = {
            _label: BasicInfo.value.body.value.neck.label,
            _value: BasicInfo.value.body.value.neck.value,
            _unit: BasicInfo.value.body.value.neck.unit
        }

        $scope.body_chest = {
            _label: BasicInfo.value.body.value.chest.label,
            _value: BasicInfo.value.body.value.chest.value,
            _unit: BasicInfo.value.body.value.chest.unit
        };


        $scope.body_arm = {
            _label: BasicInfo.value.body.value.arm.label,
            _value: BasicInfo.value.body.value.arm.value,
            _unit: BasicInfo.value.body.value.arm.unit
        };


        $scope.body_leg = {
            _label: BasicInfo.value.body.value.leg.label,
            _value: BasicInfo.value.body.value.leg.value,
            _unit: BasicInfo.value.body.value.leg.unit
        };


        $scope.body_inner = {
            _label: BasicInfo.value.body.value.inner.label,
            _value: BasicInfo.value.body.value.inner.value,
            _unit: BasicInfo.value.body.value.inner.unit
        };

        $scope.body_bicep = {
            _label: BasicInfo.value.body.value.bicep.label,
            _value: BasicInfo.value.body.value.bicep.value,
            _unit: BasicInfo.value.body.value.bicep.unit
        };
        $scope.body_shoes = {
            _label: BasicInfo.value.body.value.shoes.label,
            _value: BasicInfo.value.body.value.shoes.value,
            _unit: BasicInfo.value.body.value.shoes.unit
        };

        $scope.body_ring = {
            _label: BasicInfo.value.body.value.ring.label,
            _value: BasicInfo.value.body.value.ring.value,
            _unit: BasicInfo.value.body.value.ring.unit
        };

        $scope.body_note_tailor = {
            _label: BasicInfo.value.body.value.note_tailor.label,
            _value: BasicInfo.value.body.value.note_tailor.value,

        };


        $scope.body_right_eye = {
            _label: BasicInfo.value.body.value.right_eye.label,
            _value: BasicInfo.value.body.value.right_eye.value
        };

        $scope.body_left_eye = {
            _label: BasicInfo.value.body.value.left_eye.label,
            _value: BasicInfo.value.body.value.left_eye.value
        };
        $scope.body_note_eye = {
            _label: BasicInfo.value.body.value.note_eye.label,
            _value: BasicInfo.value.body.value.note_eye.value
        };

        //check grammar
        if ($scope.preference_interests._label == "interests")
            $scope.preference_interests._label = "Interests";

        if ($scope.preference_sports._label == "sports")
            $scope.preference_sports._label = "Sports"

        if ($scope.favourite_colour._label == "Clour")
            $scope.favourite_colour._label = "Colour";

        if ($scope.body_hair_colour._label == "Hair Clour")
            $scope.body_hair_colour._label = "Hair colour";


        $scope.messageBox = "";
        $scope.alerts = [];
    }
    $scope.$on('others', function () {
        $scope.InitData();
    });

    $scope.InitData();
    // === // ===    

    $scope.Edit = function () {
        $scope.IsEdit = true;
    }


    //IsEdit_favourite
    $scope.Edit_favourite = function ()
    { $scope.IsEdit_favourite = true; }

    //IsEdit_body
    $scope.Edit_body = function ()
    { $scope.IsEdit_body = true; }

    $scope.Save = function () {

        //body
        BasicInfo.label = $scope.others._label;
        if ($scope.others._name == '') {
            $scope.others._name = $scope.others._label;
        }
        BasicInfo.name = $scope.others._name;

        BasicInfo.privacy = $scope.others._privacy;
        BasicInfo.default = $scope.others._default;


        //preference
     

            BasicInfo.value.preference.label = $scope.preference._label;
            if ($scope.preference._name == '') {
                $scope.preference._name = $scope.preference._label;
            }

            BasicInfo.value.preference.name = $scope.preference._name;
            BasicInfo.value.preference.default = $scope.preference._default;
            BasicInfo.value.preference.privacy = $scope.preference._privacy;

        //preference_interests sports

        //new preference_sexuality
            BasicInfo.value.preference.value.sexuality.value = $scope.preference_sexuality._value;

            BasicInfo.value.preference.value.languages.value = $scope.preference_languages._value;
            BasicInfo.value.preference.value.ethnicity.value = $scope.preference_ethnicity._value;
            BasicInfo.value.preference.value.religion.value = $scope.preference_religion._value;
            BasicInfo.value.preference.value.disability.value = $scope.preference_disability._value;

        //
            BasicInfo.value.preference.value.interests.label = $scope.preference_interests._label;
            BasicInfo.value.preference.value.interests.value = $scope.preference_interests._value;

            //preference_sports
         
            BasicInfo.value.preference.value.sports.label = $scope.preference_sports._label;
            BasicInfo.value.preference.value.sports.value = $scope.preference_sports._value;
            //preference_food
            BasicInfo.value.preference.value.food.label = $scope.preference_food._label;
            BasicInfo.value.preference.value.food.value = $scope.preference_food._value;
           
        //preference_allergies
            BasicInfo.value.preference.value.allergies.label = $scope.preference_allergies._label;
            BasicInfo.value.preference.value.allergies.value = $scope.preference_allergies._value;
          

            //preference_seat
            BasicInfo.value.preference.value.seat.label = $scope.preference_seat._label;
            BasicInfo.value.preference.value.seat.value = $scope.preference_seat._value;

            //preference_season
            BasicInfo.value.preference.value.season.label = $scope.preference_season._label;
            BasicInfo.value.preference.value.season.value = $scope.preference_season._value;

            //preference_note 
            BasicInfo.value.preference.value.note.label = $scope.preference_note._label;
            BasicInfo.value.preference.value.note.value = $scope.preference_note._value;
  

        //favourite


            BasicInfo.value.favourite.label = $scope.favourite._label;
            if ($scope.favourite._name == '') {
                $scope.favourite._name = $scope.favourite._label;
            }
            BasicInfo.value.favourite.name = $scope.favourite._name;
            BasicInfo.value.favourite.default = $scope.favourite._default;
            BasicInfo.value.favourite.privacy = $scope.favourite._privacy;

        //favourite_colour Clour
    
            BasicInfo.value.favourite.value.colour.label = $scope.favourite_colour._label;
            BasicInfo.value.favourite.value.colour.value = $scope.favourite_colour._value;


            //favourite_holiday
            BasicInfo.value.favourite.value.holiday.label = $scope.favourite_holiday._label;
            BasicInfo.value.favourite.value.holiday.value = $scope.favourite_holiday._value;

            //favourite_movie
            BasicInfo.value.favourite.value.movie.label = $scope.favourite_movie._label;
            BasicInfo.value.favourite.value.movie.value = $scope.favourite_movie._value;


            //favourite_music_type
            BasicInfo.value.favourite.value.music_type.label = $scope.favourite_music_type._label;
            BasicInfo.value.favourite.value.music_type.value = $scope.favourite_music_type._value;


            //favourite_song
            BasicInfo.value.favourite.value.song.label = $scope.favourite_song._label;
            BasicInfo.value.favourite.value.song.value = $scope.favourite_song._value;

            //favourite_tv_show
            BasicInfo.value.favourite.value.tv_show.label = $scope.favourite_tv_show._label;
            BasicInfo.value.favourite.value.tv_show.value = $scope.favourite_tv_show._value;

            //favourite_personality
            BasicInfo.value.favourite.value.personality.label = $scope.favourite_personality._label;
            BasicInfo.value.favourite.value.personality.value = $scope.favourite_personality._value;


            //favourite_preferred_time
            BasicInfo.value.favourite.value.preferred_time.label = $scope.favourite_preferred_time._label;
            BasicInfo.value.favourite.value.preferred_time.value = $scope.favourite_preferred_time._value;

            //favourite_note
            BasicInfo.value.favourite.value.note.label = $scope.favourite_note._label;
            BasicInfo.value.favourite.value.note.value = $scope.favourite_note._value;

     
        //body

            BasicInfo.value.body.label = $scope.body._label;
            if ($scope.body._name == '') {
                $scope.body._name = $scope.body._label;
            }
            BasicInfo.value.body.name = $scope.body._name;
            BasicInfo.value.body.default = $scope.body._default;
            BasicInfo.value.body.privacy = $scope.body._privacy;

        //body_hair_colour Hair Clour
         
            BasicInfo.value.body.value.hair_colour.label = $scope.body_hair_colour._label;
            BasicInfo.value.body.value.hair_colour.value = $scope.body_hair_colour._value;

            //body_eye_colour
            BasicInfo.value.body.value.eye_colour.label = $scope.body_eye_colour._label;
            BasicInfo.value.body.value.eye_colour.value = $scope.body_eye_colour._value;

            //body_height
            BasicInfo.value.body.value.height.label = $scope.body_height._label;
            BasicInfo.value.body.value.height.value = $scope.body_height._value;
            BasicInfo.value.body.value.height.unit = $scope.body_height._unit;


            //body_weight
            BasicInfo.value.body.value.weight.label = $scope.body_weight._label;
            BasicInfo.value.body.value.weight.value = $scope.body_weight._value;
            BasicInfo.value.body.value.weight.unit = $scope.body_weight._unit;

            //body_type
            BasicInfo.value.body.value.type.label = $scope.body_type._label;
            BasicInfo.value.body.value.type.value = $scope.body_type._value;

            //note_body
            BasicInfo.value.body.value.note_body.label = $scope.body_note_body._label;
            BasicInfo.value.body.value.note_body.value = $scope.body_note_body._value;

            //body_weight
            BasicInfo.value.body.value.neck.label = $scope.body_neck._label;
            BasicInfo.value.body.value.neck.value = $scope.body_neck._value;
            BasicInfo.value.body.value.neck.unit = $scope.body_neck._unit;

            //body_chest
            BasicInfo.value.body.value.chest.label = $scope.body_chest._label;
            BasicInfo.value.body.value.chest.value = $scope.body_chest._value;
            BasicInfo.value.body.value.chest.unit = $scope.body_chest._unit;

            //body_arm
            BasicInfo.value.body.value.arm.label = $scope.body_arm._label;
            BasicInfo.value.body.value.arm.value = $scope.body_arm._value;
            BasicInfo.value.body.value.arm.unit = $scope.body_arm._unit;
            //body_leg
            BasicInfo.value.body.value.leg.label = $scope.body_leg._label;
            BasicInfo.value.body.value.leg.value = $scope.body_leg._value;
            BasicInfo.value.body.value.leg.unit = $scope.body_leg._unit;

            //body_inner
            BasicInfo.value.body.value.inner.label = $scope.body_inner._label;
            BasicInfo.value.body.value.inner.value = $scope.body_inner._value;
            BasicInfo.value.body.value.inner.unit = $scope.body_inner._unit;

            //body_bicep
            BasicInfo.value.body.value.bicep.label = $scope.body_bicep._label;
            BasicInfo.value.body.value.bicep.value = $scope.body_bicep._value;
            BasicInfo.value.body.value.bicep.unit = $scope.body_bicep._unit;
            //body_shoes
            BasicInfo.value.body.value.shoes.label = $scope.body_shoes._label;
            BasicInfo.value.body.value.shoes.value = $scope.body_shoes._value;
            BasicInfo.value.body.value.shoes.unit = $scope.body_shoes._unit;

            //body_ring
            BasicInfo.value.body.value.ring.label = $scope.body_ring._label;
            BasicInfo.value.body.value.ring.value = $scope.body_ring._value;
            BasicInfo.value.body.value.ring.unit = $scope.body_ring._unit;
            //body_note_tailor
            BasicInfo.value.body.value.note_tailor.label = $scope.body_note_tailor._label;
            BasicInfo.value.body.value.note_tailor.value = $scope.body_note_tailor._value;

            //note_right_eye
            BasicInfo.value.body.value.right_eye.label = $scope.body_right_eye._label;
            BasicInfo.value.body.value.right_eye.value = $scope.body_right_eye._value;


            //note_left_eye
            BasicInfo.value.body.value.left_eye.label = $scope.body_left_eye._label;
            BasicInfo.value.body.value.left_eye.value = $scope.body_left_eye._value;

            //note_note_eye
            BasicInfo.value.body.value.note_eye.label = $scope.body_note_eye._label;
            BasicInfo.value.body.value.note_eye.value = $scope.body_note_eye._value;

   
     //end basic

        VaultInformationService.VaultInformation.others = BasicInfo;
        VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
            VaultInformationService.VaultInformation._id).then(function (response) {
                $scope.IsEdit = false;
                $scope.IsEdit_body = false;
                $scope.IsEdit_favourite = false;
                $scope.list_others = [];
                $scope.select_others._value = '';
                $rootScope.$broadcast('others');
              
            }, function (errors) {

            });
    }

    $scope.saveVaultInformationOnSuccess = function (response) {
        $scope.IsEdit = false;
        $scope.IsEdit_body = false;
        $scope.IsEdit_favourite = false;
        $scope.select_others._value = '';
    }
    $scope.saveVaultInformationOnError = function (response) {
        alertService.renderSuccessMessage(response.ReturnMessage);
        $scope.messageBox = alertService.returnFormattedMessage();
        $scope.alerts = alertService.returnAlerts();
    }

    $scope.Cancel = function () {
        $scope.InitData();

    }

    $scope.goBack = function () {
        $scope.$location.path('/VaultInformation');
    };
    $scope.closePanel = function () {
        $scope.Cancel();
    };

}])