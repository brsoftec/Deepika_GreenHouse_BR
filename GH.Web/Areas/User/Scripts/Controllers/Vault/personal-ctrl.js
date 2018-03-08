var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule'], true);

//1. Basic Infomation 
myApp.getController('BasicController',
    ['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'vaultService', 'NetworkService', 'rguNotify',
        function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, vaultService, _networkService, rguNotify) {
            var BasicInfo = VaultInformationService.VaultInformation.basicInformation;
         
         $scope.$on('GetBasicInformation', function () {
                BasicInfo = VaultInformationService.VaultInformation.basicInformation;
                $scope.InitData();
            });
       
            $scope.GetAllFriends = function () {
                _networkService.GetFriends().then(function (result) {
                    $scope.network = result;
                })
            }
            $scope.GetAllFriends();

            //  Son
            $scope.result = {};
            $scope.sendTo = {};
            $scope.sendPush = function () {
                $scope.PushToVault();
            };
            //sendTo.comment
            $scope.vaultGroup = {
                name: 'basicInformation',
                label: BasicInfo.label,
                model: BasicInfo.value
            };
            vaultService.populateGroup($scope.vaultGroup);

            $scope.PushToVault = function () {

                var data = new Object();
                var campaign = new Object();
                campaign.type = "ManualPushToVault";
                campaign.status = "Active";
                campaign.name = "Basic Infomation";
                campaign.description = $scope.result.sendTo.comment;
                if ($scope.result.sendTo.comment === '' || $scope.result.sendTo.comment === undefined)
                    campaign.description = "Manual push to Vault";

                campaign.fields = [];
                data.UserId = $scope.result.sendTo.userId;
                data.CampaignType = "ManualPushToVault";
                data.Listvaults = [];
                data.pushtype = "ManualPushToVault";
                if ($scope.result.sendTo.userId != null && $scope.result.sendTo.userId != undefined)
                    data.usernotifycationid = $scope.result.sendTo.userId;

                if ($scope.result.sendTo.userEmail != "") {
                    data.usernotifycationemail = $scope.result.sendTo.userEmail;
                    data.pushtype = "email";
                }

                for (var index = 0; index < $scope.result.fields.length; index++) {
                    if ($scope.result.fields[index].displayName == null && $scope.result.fields[index].displayName == undefined)
                        $scope.result.fields[index].displayName = "";
                    if ($scope.result.fields[index].optional == null && $scope.result.fields[index].optional == undefined)
                        $scope.result.fields[index].optional = "";
                    if ($scope.result.fields[index].type == null && $scope.result.fields[index].type == undefined)
                        $scope.result.fields[index].type = "";
                    if ($scope.result.fields[index].unitModel == null && $scope.result.fields[index].unitModel == undefined)
                        $scope.result.fields[index].unitModel = "";
                    if ($scope.result.fields[index].membership == null && $scope.result.fields[index].membership == undefined)
                        $scope.result.fields[index].membership = "";

                    if ($scope.result.fields[index].choices == null && $scope.result.fields[index].choices == undefined)
                        $scope.result.fields[index].choices = "";
                    if ($scope.result.fields[index].qa == null && $scope.result.fields[index].qa == undefined)
                        $scope.result.fields[index].qa = "";
                    var field = {
                        id: $scope.result.fields[index].id,
                        jsPath: '.' + $scope.result.fields[index].jsPath,
                        displayName: $scope.result.fields[index].displayName,
                        optional: $scope.result.fields[index].optional,
                        type: $scope.result.fields[index].type,
                        options: $scope.result.fields[index].options,
                        model: $scope.result.fields[index].value,
                        unitModel: $scope.result.fields[index].unitModel,
                        value: "",
                        membership: $scope.result.fields[index].membership + "",

                        choices: $scope.result.fields[index].choices + "",
                        qa: $scope.result.fields[index].qa + ""
                    }
                    campaign.fields.push(field);

                    switch (field.type) {
                        case "location":
                            var model = $scope.result.fields[index].value;
                            field.model = model.country;
                            field.unitModel = model.city;
                            break;
                        case "address":
                            var model = $scope.result.fields[index].value;
                            field.model = model.address;
                            field.unitModel = model.address2;
                            break;
                    }

                    data.Listvaults.push(field);
                };
                var StrVaultInformation = JSON.stringify(campaign);
                data.StrVaultInformation = StrVaultInformation;
                $http.post('/api/CampaignService/ManualPushVault', data)
                    .success(function (response) {
                        if (response.ReturnStatus == false) {
                            swal(
                                'Push Vault Send Error!',
                                'Email not found. Push form must be sent to a valid Regit user.',
                                 'error'
                              );
                            return;
                        }
                    })
                    .error(function (errors) {
                        var errorObj = __errorHandler.ProcessErrors(errors);
                        __errorHandler.Swal(errorObj, _sweetAlert);
                    });

            }

            // End manual push to vault
            $scope.InitData = function () {
                $scope.IsEdit = false;
                $scope.label = BasicInfo.label;
                $scope.privacy = { value: BasicInfo.privacy };
              
                //Title
                $scope.listTitle = ['Mr.', 'Ms.', 'Mrs.', 'Dr.', 'Sir'];
                $scope.title = {
                    value: BasicInfo.value.title.value,
                    label: BasicInfo.value.title.label,
                    privacy: BasicInfo.value.title.privacy
                }
                $scope.edit_title = angular.copy($scope.title);

                //First name
                $scope.firstName = {
                    value: BasicInfo.value.firstName.value,
                    label: BasicInfo.value.firstName.label,
                    privacy: BasicInfo.value.firstName.privacy
                }
                $scope.edit_firstName = angular.copy($scope.firstName);

                //Middle name
                $scope.middleName = {
                    value: BasicInfo.value.middleName.value,
                    label: BasicInfo.value.middleName.label,
                    privacy: BasicInfo.value.middleName.privacy
                }
                $scope.edit_middleName = angular.copy($scope.middleName);

                //Last name
                $scope.lastName = {
                    value: BasicInfo.value.lastName.value,
                    label: BasicInfo.value.lastName.label,
                    privacy: BasicInfo.value.lastName.privacy
                }
                $scope.edit_lastName = angular.copy($scope.lastName);


                //Alias
                $scope.alias = {
                    value: BasicInfo.value.alias.value,
                    label: BasicInfo.value.alias.label,
                    privacy: BasicInfo.value.alias.privacy
                }
                $scope.edit_alias = angular.copy($scope.alias);

                $scope.dob = {
                    value: BasicInfo.value.dob.value,
                    label: BasicInfo.value.dob.label,
                    privacy: BasicInfo.value.dob.privacy
                }
                $scope.edit_dob = angular.copy($scope.dob);

                $scope.gender = {
                    label: BasicInfo.value.gender.label,
                    value: BasicInfo.value.gender.value,
                    privacy: BasicInfo.value.gender.privacy
                }
                $scope.edit_gender = angular.copy($scope.gender);

                //
                if (BasicInfo.value.relationshipStatus === undefined) {
                    BasicInfo.value.relationshipStatus = {
                        label: 'Relationship status',
                        value: '',
                        type: 'select',
                        options: ['Single', 'Married', 'In-a-Relationship', 'Common-Law', 'Divorced', 'Widowed'],
                        privacy: true
                    }
                }
                $scope.relationshipStatus = {
                    label: BasicInfo.value.relationshipStatus.label,
                    value: BasicInfo.value.relationshipStatus.value,
                    privacy: BasicInfo.value.relationshipStatus.privacy,
                };
                $scope.edit_relationshipStatus = angular.copy($scope.relationshipStatus);

                // country
                $scope.country = {
                    label: BasicInfo.value.country.label,
                    value: BasicInfo.value.country.value,
                    privacy: BasicInfo.value.country.privacy,
                };
                $scope.edit_country = angular.copy($scope.country);
                // state
                $scope.state = {
                    label: BasicInfo.value.state.label,
                    value: BasicInfo.value.state.value,
                    privacy: BasicInfo.value.state.privacy,
                };
                $scope.edit_state = angular.copy($scope.state);

                // city
                $scope.city = {
                    label: BasicInfo.value.city.label,
                    value: BasicInfo.value.city.value,
                    privacy: BasicInfo.value.city.privacy,
                };
                $scope.edit_city = angular.copy($scope.city);


                CountryCityService.InitData($scope.edit_city.value, $scope.edit_country.value).then(function (response) {
                    $scope.listCountries = CountryCityService.Countries;
                    $scope._country = { value: CountryCityService.Country };
                    $scope.listcities = CountryCityService.Cities;
                    $scope._city = { value: CountryCityService.City };

                }, function (errors) {
                });

                $scope.messageBox = "";
                $scope.alerts = [];
            }

            //city
            $scope.changeCity = function () {

                if ($scope._country.value != null && $scope._country.value.Code != undefined) {
                    CountryCityService.GetCitiesByCountryID($scope._country.value.Code).then(function (response) {
                        $scope.listcities = response.Cities;
                    }, function (errors) {
                    });
                }
                else
                    $scope.listcities = [];

            }
            $scope.InitData();
            $scope.$on('basicInformation', function () {
                $scope.InitData();
            });
            $scope.Edit = function () {
                $scope.IsEdit = true;
            }

            $scope.Save = function () {

                //General
                BasicInfo.label = $scope.label;
                BasicInfo.privacy = $scope.privacy.value;

                //Title
                BasicInfo.value.title = $scope.edit_title;

                //First name
                BasicInfo.value.firstName = $scope.edit_firstName;

                BasicInfo.value.middleName = $scope.edit_middleName;
                BasicInfo.value.lastName = $scope.edit_lastName;

                //Alias
                BasicInfo.value.alias = $scope.edit_alias;

                //D.O.B
                BasicInfo.value.dob = $scope.edit_dob;

                //Gender
                BasicInfo.value.gender = $scope.edit_gender;

                //relationshipStatus 
                BasicInfo.value.relationshipStatus = $scope.edit_relationshipStatus;

                //country
                BasicInfo.value.country.privacy = $scope.edit_country.privacy;

                BasicInfo.value.city.privacy = $scope.edit_country.privacy;
                if ($scope._country.value != null)
                    BasicInfo.value.country.value = $scope._country.value.Name;

                if ($scope._city.value != null)
                    BasicInfo.value.city.value = $scope._city.value.Name;

              
                //Write database
                VaultInformationService.VaultInformation.basicInformation = BasicInfo;
                VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
                    VaultInformationService.VaultInformation._id).then(function (response) {
                        $scope.IsEdit = false;
                        $scope.InitData();
                        // $rootScope.$broadcast('basicInformation');
                    $http.post('/Api/AccountSettings/ProfileProperty', {
                        propName: 'Gender', propValue: BasicInfo.value.gender.value
                    }).success(function () {
                    });
                        rguNotify.add('Saved change to vault');

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
                $scope.InitData();
            }


        }])

//2. Contact
myApp.getController('ContactController',
    ['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService',
        function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService) {

            var BasicInfo = VaultInformationService.VaultInformation.contact;
            $scope.InitData = function () {
                if (BasicInfo.name == '' || BasicInfo.name == 'contact')
                    BasicInfo.name = 'Contact';
                //star value
                $scope.contact = {
                    'IsEdit': false,
                    'label': BasicInfo.label,
                    'name': BasicInfo.name,
                    'default': BasicInfo.default,
                    'privacy': BasicInfo.privacy,
                    'sublist': true
                };
            }
        }]);

//3. Mobile
myApp.getController('MobileController',
    ['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService',
        function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService) {


            var BasicInfo = VaultInformationService.VaultInformation.contact;
            $scope.InitData = function () {
                $scope.addNewError = '';
                if (BasicInfo.value.mobile == undefined) {
                    BasicInfo.value.mobile = {
                        '_id': '',
                        'label': 'Mobile',
                        'name': '',
                        'privacy': true,
                        'rules': 'phone',
                        'type': 'textbox',
                        'nosearch': true,
                        'default': '',
                        'value': []
                    }
                }


                $scope.mobile = {
                    'id': BasicInfo.value.mobile.id,
                    'label': BasicInfo.value.mobile.label,
                    'name': BasicInfo.value.mobile.name,
                    'privacy': BasicInfo.value.mobile.privacy,
                    'type': BasicInfo.value.mobile.type,
                    'rules': BasicInfo.value.mobile.rules,
                    'nosearch': BasicInfo.value.mobile.nosearch,
                    'default': BasicInfo.value.mobile.default,
                    'edit': false
                };

                $scope.listMobile = [];

                //    //End star value
                var phoneId = 0;
                $(BasicInfo.value.mobile.value).each(function (index, object) {
                    phoneId = phoneId + 1;
                    var _default = false;
                    if (BasicInfo.value.mobile.value[index].value == BasicInfo.value.mobile.default)
                        _default = true;

                    var temp = BasicInfo.value.mobile.value[index].id;
                    if (temp == '')
                        BasicInfo.value.mobile.value[index].id = phoneId;
                    else
                        BasicInfo.value.mobile.value[index].id = parseInt(temp);

                    BasicInfo.value.mobile.value[index].id = temp;
                    var tempPhone = BasicInfo.value.mobile.value[index].value;
                    _userManager.GetPhoneCode({ phoneNumber: tempPhone }).then(function (phone) {
                        $scope.listMobile.push({
                            'id': temp,
                            'value': tempPhone,
                            'codeCountry': phone.CodeCountry,
                            'phoneNumber': phone.PhoneNumber,
                            'default': _default
                        });

                    }, function (errors) {

                    });

                });

                // add
                $scope.newMobile = { value: false };
                $scope.addNew = function (newPhone) {
                    $scope.newMobile.value = false;
                    if (newPhone.default == true) {
                        $scope.mobile.default = newPhone.value;
                        for (var i = 0; i < $scope.listMobile.length; i++) {
                            $scope.listMobile[i].default = false;
                        }
                    }
                    $scope.listMobile.push({
                        'id': phoneId + 1,
                        'value': newPhone.value,
                        'codeCountry': newPhone.codeCountry,
                        'phoneNumber': newPhone.phoneNumber,
                        'default': newPhone.default,
                    });
                    $scope.isNew.value = false;
                    $scope._new = {};
                }
                $scope.isNew = { value: false };
                $scope.new = function () {
                    $scope.isNew.value = true;
                    $scope._new =
                        {
                            'id': phoneId,
                            'value': "",
                            'codeCountry': "65",
                            'phoneNumber': "",
                            'default': true
                        };
                }

                $scope.removeMobile = function (mobile) {
                    var index = $scope.listMobile.indexOf(mobile);
                    swal({
                        title: "Are you sure to deleted  " + mobile.value + " ?",
                        type: "warning",
                        showCancelButton: true,
                        confirmButtonColor: "#DD6B55",
                        confirmButtonText: "Yes",
                        cancelButtonText: "No"

                    }).then(function () {
                        mobile.removed = true;
                        $scope.listMobile.splice(index, 1);
                        swal(
                            'Deleted',
                             mobile.value + ' has been deleted.',
                            'success'
                          )
                    }
                    , function (dismiss) {
                        if (dismiss === 'cancel') {

                        }
                    });
                };

                //check
                $scope.addNewError = '';
                $scope.CheckAddNew = function (_new) {
                    $scope.addNewError = '';
                    var check = true;
                    _new.value = '+' + _new.codeCountry + _new.phoneNumber;
                    for (var i = 0; i < $scope.listMobile.length; i++) {
                        if (_new.value == $scope.listMobile[i].value) {
                            $scope.addNewError = _new.value + " is exist";
                            break;
                        }
                    }
                    if ($scope.addNewError == '') {
                        $scope.addNew(_new);
                    }
                }

                $scope.CheckDefault = function (value) {
                    $scope.addNewError = '';
                    var check = true;
                    $scope.mobile.default = '+' + value.codeCountry + value.phoneNumber;
                    for (var i = 0; i < $scope.listMobile.length; i++) {
                        if ($scope.listMobile[i].value != value.value)
                            $scope.listMobile[i].default = false;
                        if ($scope.listMobile[i].value == value.value) {
                            $scope.listMobile[i].default = true;
                            $scope.mobile.default = value.value;
                        }
                    }
                }

                //check
                $scope.IsvalidPhoneNumber = function () {
                    $scope.addNewError = "";
                    var lstPhone = "";
                    if ($scope.listMobile.length > 0) {
                        for (var i = 0; i < $scope.listMobile.length; i++) {
                            var phone = '+' + $scope.listMobile[i].codeCountry + $scope.listMobile[i].phoneNumber;
                            if (i == 0) {
                                lstPhone = phone;
                            }
                            else
                                lstPhone = lstPhone + "," + phone;

                            for (var j = i + 1; j < $scope.listMobile.length; j++) {

                                if ($scope.listMobile[j].codeCountry + $scope.listMobile[j].phoneNumber == $scope.listMobile[i].codeCountry + $scope.listMobile[i].phoneNumber)
                                    $scope.addNewError = '+' + $scope.listMobile[i].codeCountry + $scope.listMobile[i].phoneNumber + " is duplicate.";
                            }

                        }
                        if ($scope.addNewError == '') {
                            _userManager.ValidPhone({ phoneNumber: lstPhone }).then(function (rs) {

                                if (rs.ValidPhone == true) {
                                    $scope.Save();
                                }
                                else
                                    $scope.addNewError = rs.PhoneNumber + " is invalid";
                            }, function (errors) {

                                $scope.addNewError = " Is invalid";

                                swal(
                                   'Deleted', 'Save mobile errror', 'error'
                                 )
                            })
                        }
                    }
                    else {
                        $scope.mobile.default = "";
                        $scope.Save();
                    }
                }
                //End Mobile
                $scope.messageBox = "";
                $scope.alerts = [];
            }

            $scope.InitData();
            $scope.Save = function () {

                var lstMobileSave = [];
                var isDefault = false;

                for (var i = 0; i < $scope.listMobile.length; i++) {
                    var tempP = '+' + $scope.listMobile[i].codeCountry + $scope.listMobile[i].phoneNumber;
                    if ($scope.listMobile[i].default == true) {
                        isDefault = true;
                        $scope.mobile.default = tempP;
                    }

                    lstMobileSave.push({
                        'id': $scope.listMobile[i].id,
                        'value': tempP
                    });
                }
                if (isDefault == false && lstMobileSave.length > 0) {

                    $scope.mobile.default = lstMobileSave[0].value;
                }

                BasicInfo.value.mobile.default = $scope.mobile.default;

                BasicInfo.name = "Contact";

                VaultInformationService.VaultInformation.contact = BasicInfo;
                VaultInformationService.VaultInformation.contact.value.mobile.value = lstMobileSave;
                VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
                    VaultInformationService.VaultInformation._id).then(function (response) {
                        $scope.mobile.edit = false;
                        $scope.addNewError = '';
                        $scope.InitData();

                    }, function (errors) {

                    });

            }

            $scope.saveVaultInformationOnSuccess = function (response) {
                $scope.mobile.edit = false;
                $scope.addNewError = '';

            }
            $scope.saveVaultInformationOnError = function (response) {
                alertService.renderSuccessMessage(response.ReturnMessage);
                $scope.messageBox = alertService.returnFormattedMessage();
                $scope.alerts = alertService.returnAlerts();
            }

            $scope.Cancel = function () {

                $scope.InitData();
            }

        }]);


//4. Home
myApp.getController('HomeController',
    ['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService',
        function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService) {


            var BasicInfo = VaultInformationService.VaultInformation.contact;
            $scope.InitData = function () {
                $scope.addNewError = '';

                // Mobile type default
                if (BasicInfo.value.home._id == undefined) {
                    BasicInfo.value.home._id = "";
                }
                if (BasicInfo.value.home.rules == undefined) {
                    BasicInfo.value.home.rules = "phone";
                }
                if (BasicInfo.value.home.nosearch == undefined) {
                    BasicInfo.value.home.nosearch = true;
                }
                if (BasicInfo.value.home.default == undefined) {
                    BasicInfo.value.home.default = "";
                }
                if (BasicInfo.value.home.type == undefined) {
                    BasicInfo.value.home.type = "textbox";
                }

                $scope.mobile = {
                    '_id': BasicInfo.value.home._id,
                    'label': BasicInfo.value.home.label,
                    'name': BasicInfo.value.home.name,
                    'privacy': BasicInfo.value.home.privacy,
                    'type': BasicInfo.value.home.type,
                    'rules': BasicInfo.value.home.rules,
                    'nosearch': BasicInfo.value.home.nosearch,
                    'default': BasicInfo.value.home.default,
                    'edit': false
                };

                $scope.listMobile = [];


                var phoneId = 0;
                $(BasicInfo.value.home.value).each(function (index, object) {
                    phoneId = phoneId + 1;
                    var _default = false;
                    if (BasicInfo.value.home.value[index].value == BasicInfo.value.home.default)
                        _default = true;
                    if (BasicInfo.value.home.value[index].id == undefined)
                        BasicInfo.value.home.value[index].id = phoneId;

                    var temp = BasicInfo.value.home.value[index].id;
                    if (temp == '')
                        BasicInfo.value.home.value[index].id = phoneId;
                    else
                        BasicInfo.value.home.value[index].id = parseInt(temp);
                    BasicInfo.value.home.value[index].id = temp;

                    var tempPhone = BasicInfo.value.home.value[index].value;

                    _userManager.GetPhoneCode({ phoneNumber: tempPhone }).then(function (phone) {
                        $scope.listMobile.push({
                            'id': temp,
                            'value': tempPhone,
                            'codeCountry': phone.CodeCountry,
                            'phoneNumber': phone.PhoneNumber,
                            'default': _default
                        });

                    }, function (errors) {

                    });

                });


                // add
                $scope.newMobile = { value: false };
                $scope.addNew = function (newPhone) {
                    $scope.newMobile.value = false;
                    if (newPhone.default == true) {
                        $scope.mobile.default = newPhone.value;
                        for (var i = 0; i < $scope.listMobile.length; i++) {
                            $scope.listMobile[i].default = false;
                        }
                    }

                    $scope.listMobile.push({
                        'id': phoneId + 1,
                        'value': newPhone.value,
                        'codeCountry': newPhone.codeCountry,
                        'phoneNumber': newPhone.phoneNumber,
                        'default': newPhone.default,
                    });
                    $scope.isNew.value = false;
                    $scope._new = {};
                }

                $scope.isNew = { value: false };
                $scope.new = function () {
                    $scope.isNew.value = true;
                    $scope._new =
                        {
                            'id': phoneId,
                            'value': "",
                            'codeCountry': "65",
                            'phoneNumber': "",
                            'default': true

                        };
                }

                $scope.removeMobile = function (mobile) {
                    var index = $scope.listMobile.indexOf(mobile);
                    swal({
                        title: "Are you sure to deleted  " + mobile.value + " ?",
                        type: "warning",
                        showCancelButton: true,
                        confirmButtonColor: "#DD6B55",
                        confirmButtonText: "Yes",
                        cancelButtonText: "No"

                    }).then(function () {
                        mobile.removed = true;
                        $scope.listMobile.splice(index, 1);
                        swal(
                            'Deleted',
                             mobile.value + ' has been deleted.',
                            'success'
                          )
                    }
                    , function (dismiss) {
                        if (dismiss === 'cancel') {

                        }
                    });
                };

                //check
                $scope.addNewError = '';
                $scope.CheckAddNew = function (_new) {
                    $scope.addNewError = '';
                    var check = true;
                    _new.value = '+' + _new.codeCountry + _new.phoneNumber;
                    for (var i = 0; i < $scope.listMobile.length; i++) {
                        if (_new.value == $scope.listMobile[i].value) {
                            $scope.addNewError = _new.value + " is exist";
                            break;
                        }
                    }
                    if ($scope.addNewError == '') {
                        $scope.addNew(_new);
                    }
                }

                $scope.CheckDefault = function (value) {
                    $scope.addNewError = '';
                    var check = true;
                    $scope.mobile.default = '+' + value.codeCountry + value.phoneNumber;
                    for (var i = 0; i < $scope.listMobile.length; i++) {
                        if ($scope.listMobile[i].value != value.value)
                            $scope.listMobile[i].default = false;
                        if ($scope.listMobile[i].value == value.value) {
                            $scope.listMobile[i].default = true;
                            $scope.mobile.default = value.value;
                        }
                    }
                }

                //check
                $scope.IsvalidPhoneNumber = function () {
                    $scope.addNewError = "";
                    var lstPhone = "";
                    if ($scope.listMobile.length > 0) {
                        for (var i = 0; i < $scope.listMobile.length; i++) {
                            var phone = '+' + $scope.listMobile[i].codeCountry + $scope.listMobile[i].phoneNumber;
                            if (i == 0) {
                                lstPhone = phone;
                            }
                            else
                                lstPhone = lstPhone + "," + phone;

                            for (var j = i + 1; j < $scope.listMobile.length; j++) {

                                if ($scope.listMobile[j].codeCountry + $scope.listMobile[j].phoneNumber == $scope.listMobile[i].codeCountry + $scope.listMobile[i].phoneNumber)
                                    $scope.addNewError = '+' + $scope.listMobile[i].codeCountry + $scope.listMobile[i].phoneNumber + " is duplicate.";
                            }

                        }
                        if ($scope.addNewError == '') {
                            _userManager.ValidPhone({ phoneNumber: lstPhone }).then(function (rs) {

                                if (rs.ValidPhone == true) {
                                    $scope.Save();
                                }
                                else
                                    $scope.addNewError = rs.PhoneNumber + " is invalid";
                            }, function (errors) {

                                $scope.addNewError = " Is invalid";

                                swal(
                                   'Deleted', 'Save mobile errror', 'error'
                                 )
                            })
                        }
                    }
                    else {
                        $scope.mobile.default = "";
                        $scope.Save();
                    }
                }
                //End Mobile
                $scope.messageBox = "";
                $scope.alerts = [];
            }


            $scope.InitData();

            $scope.Save = function () {

                var lstMobileSave = [];
                var isDefault = false;

                for (var i = 0; i < $scope.listMobile.length; i++) {
                    var tempP = '+' + $scope.listMobile[i].codeCountry + $scope.listMobile[i].phoneNumber;
                    if ($scope.listMobile[i].default == true) {
                        isDefault = true;
                        $scope.mobile.default = tempP;
                    }

                    lstMobileSave.push({
                        'id': $scope.listMobile[i].id,
                        'value': tempP
                    });
                }
                if (isDefault == false && lstMobileSave.length > 0) {

                    $scope.mobile.default = lstMobileSave[0].value;
                }

                BasicInfo.value.home.default = $scope.mobile.default;

                BasicInfo.name = "Contact";

                VaultInformationService.VaultInformation.contact = BasicInfo;
                VaultInformationService.VaultInformation.contact.value.home.value = lstMobileSave;
                VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
                    VaultInformationService.VaultInformation._id).then(function (response) {
                        $scope.mobile.edit = false;
                        $scope.addNewError = '';
                        $scope.InitData();

                    }, function (errors) {

                    });

            }

            $scope.saveVaultInformationOnSuccess = function (response) {
                $scope.mobile.edit = false;
                $scope.addNewError = '';

            }
            $scope.saveVaultInformationOnError = function (response) {
                alertService.renderSuccessMessage(response.ReturnMessage);
                $scope.messageBox = alertService.returnFormattedMessage();
                $scope.alerts = alertService.returnAlerts();
            }

            $scope.Cancel = function () {

                $scope.InitData();
            }

        }]);

//4. Office
myApp.getController('OfficeController',
    ['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService',
        function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService) {


            var BasicInfo = VaultInformationService.VaultInformation.contact;
            $scope.InitData = function () {
                $scope.addNewError = '';

                // Mobile type default
                if (BasicInfo.value.office._id == undefined) {
                    BasicInfo.value.office._id = "";
                }
                if (BasicInfo.value.office.rules == undefined) {
                    BasicInfo.value.office.rules = "phone";
                }
                if (BasicInfo.value.office.nosearch == undefined) {
                    BasicInfo.value.office.nosearch = true;
                }
                if (BasicInfo.value.office.default == undefined) {
                    BasicInfo.value.office.default = "";
                }
                if (BasicInfo.value.office.type == undefined) {
                    BasicInfo.value.office.type = "textbox";
                }

                $scope.mobile = {
                    '_id': BasicInfo.value.office._id,
                    'label': BasicInfo.value.office.label,
                    'name': BasicInfo.value.office.name,
                    'privacy': BasicInfo.value.office.privacy,
                    'type': BasicInfo.value.office.type,
                    'rules': BasicInfo.value.office.rules,
                    'nosearch': BasicInfo.value.office.nosearch,
                    'default': BasicInfo.value.office.default,
                    'edit': false
                };

                $scope.listMobile = [];


                var phoneId = 0;
                $(BasicInfo.value.office.value).each(function (index, object) {
                    phoneId = phoneId + 1;
                    var _default = false;
                    if (BasicInfo.value.office.value[index].value == BasicInfo.value.office.default)
                        _default = true;
                    if (BasicInfo.value.office.value[index].id == undefined)
                        BasicInfo.value.office.value[index].id = phoneId;

                    var temp = BasicInfo.value.office.value[index].id;
                    if (temp == '')
                        BasicInfo.value.office.value[index].id = phoneId;
                    else
                        BasicInfo.value.office.value[index].id = parseInt(temp);
                    BasicInfo.value.office.value[index].id = temp;

                    var tempPhone = BasicInfo.value.office.value[index].value;

                    _userManager.GetPhoneCode({ phoneNumber: tempPhone }).then(function (phone) {
                        $scope.listMobile.push({
                            'id': temp,
                            'value': tempPhone,
                            'codeCountry': phone.CodeCountry,
                            'phoneNumber': phone.PhoneNumber,
                            'default': _default
                        });

                    }, function (errors) {

                    });

                });


                // add
                $scope.newMobile = { value: false };
                $scope.addNew = function (newPhone) {
                    $scope.newMobile.value = false;
                    if (newPhone.default == true) {
                        $scope.mobile.default = newPhone.value;
                        for (var i = 0; i < $scope.listMobile.length; i++) {
                            $scope.listMobile[i].default = false;
                        }
                    }

                    $scope.listMobile.push({
                        'id': phoneId + 1,
                        'value': newPhone.value,
                        'codeCountry': newPhone.codeCountry,
                        'phoneNumber': newPhone.phoneNumber,
                        'default': newPhone.default,
                    });
                    $scope.isNew.value = false;
                    $scope._new = {};
                }

                $scope.isNew = { value: false };
                $scope.new = function () {
                    $scope.isNew.value = true;
                    $scope._new =
                        {
                            'id': phoneId,
                            'value': "",
                            'codeCountry': "65",
                            'phoneNumber': "",
                            'default': true

                        };
                }

                $scope.removeMobile = function (mobile) {
                    var index = $scope.listMobile.indexOf(mobile);
                    swal({
                        title: "Are you sure to deleted  " + mobile.value + " ?",
                        type: "warning",
                        showCancelButton: true,
                        confirmButtonColor: "#DD6B55",
                        confirmButtonText: "Yes",
                        cancelButtonText: "No"

                    }).then(function () {
                        mobile.removed = true;
                        $scope.listMobile.splice(index, 1);
                        swal(
                            'Deleted',
                             mobile.value + ' has been deleted.',
                            'success'
                          )
                    }
                    , function (dismiss) {
                        if (dismiss === 'cancel') {

                        }
                    });
                };

                //check
                $scope.addNewError = '';
                $scope.CheckAddNew = function (_new) {
                    $scope.addNewError = '';
                    var check = true;
                    _new.value = '+' + _new.codeCountry + _new.phoneNumber;
                    for (var i = 0; i < $scope.listMobile.length; i++) {
                        if (_new.value == $scope.listMobile[i].value) {
                            $scope.addNewError = _new.value + " is exist";
                            break;
                        }
                    }
                    if ($scope.addNewError == '') {
                        $scope.addNew(_new);
                    }
                }

                $scope.CheckDefault = function (value) {
                    $scope.addNewError = '';
                    var check = true;
                    $scope.mobile.default = '+' + value.codeCountry + value.phoneNumber;
                    for (var i = 0; i < $scope.listMobile.length; i++) {
                        if ($scope.listMobile[i].value != value.value)
                            $scope.listMobile[i].default = false;
                        if ($scope.listMobile[i].value == value.value) {
                            $scope.listMobile[i].default = true;
                            $scope.mobile.default = value.value;
                        }
                    }
                }

                //check
                $scope.IsvalidPhoneNumber = function () {
                    $scope.addNewError = "";
                    var lstPhone = "";
                    if ($scope.listMobile.length > 0) {
                        for (var i = 0; i < $scope.listMobile.length; i++) {
                            var phone = '+' + $scope.listMobile[i].codeCountry + $scope.listMobile[i].phoneNumber;
                            if (i == 0) {
                                lstPhone = phone;
                            }
                            else
                                lstPhone = lstPhone + "," + phone;

                            for (var j = i + 1; j < $scope.listMobile.length; j++) {

                                if ($scope.listMobile[j].codeCountry + $scope.listMobile[j].phoneNumber == $scope.listMobile[i].codeCountry + $scope.listMobile[i].phoneNumber)
                                    $scope.addNewError = '+' + $scope.listMobile[i].codeCountry + $scope.listMobile[i].phoneNumber + " is duplicate.";
                            }

                        }
                        if ($scope.addNewError == '') {
                            _userManager.ValidPhone({ phoneNumber: lstPhone }).then(function (rs) {

                                if (rs.ValidPhone == true) {
                                    $scope.Save();
                                }
                                else
                                    $scope.addNewError = rs.PhoneNumber + " is invalid";
                            }, function (errors) {

                                $scope.addNewError = " Is invalid";

                                swal(
                                   'Deleted', 'Save mobile errror', 'error'
                                 )
                            })
                        }
                    }
                    else {
                        $scope.mobile.default = "";
                        $scope.Save();
                    }
                }
                //End Mobile
                $scope.messageBox = "";
                $scope.alerts = [];
            }


            $scope.InitData();

            $scope.Save = function () {

                var lstMobileSave = [];
                var isDefault = false;
                for (var i = 0; i < $scope.listMobile.length; i++) {
                    var tempP = '+' + $scope.listMobile[i].codeCountry + $scope.listMobile[i].phoneNumber;
                    if ($scope.listMobile[i].default == true) {
                        isDefault = true;
                        $scope.mobile.default = tempP;
                    }

                    lstMobileSave.push({
                        'id': $scope.listMobile[i].id,
                        'value': tempP
                    });
                }
                if (isDefault == false && lstMobileSave.length > 0) {

                    $scope.mobile.default = lstMobileSave[0].value;
                }

                BasicInfo.value.office.default = $scope.mobile.default;

                BasicInfo.name = "Contact";

                VaultInformationService.VaultInformation.contact = BasicInfo;
                VaultInformationService.VaultInformation.contact.value.office.value = lstMobileSave;
                VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
                    VaultInformationService.VaultInformation._id).then(function (response) {
                        $scope.mobile.edit = false;
                        $scope.addNewError = '';
                        $scope.InitData();

                    }, function (errors) {

                    });

            }

            $scope.saveVaultInformationOnSuccess = function (response) {
                $scope.mobile.edit = false;
                $scope.addNewError = '';

            }
            $scope.saveVaultInformationOnError = function (response) {
                alertService.renderSuccessMessage(response.ReturnMessage);
                $scope.messageBox = alertService.returnFormattedMessage();
                $scope.alerts = alertService.returnAlerts();
            }

            $scope.Cancel = function () {

                $scope.InitData();
            }

        }]);

//5. Fax fax

myApp.getController('FaxController',
    ['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService',
        function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService) {
            var BasicInfo = VaultInformationService.VaultInformation.contact;
            $scope.InitData = function () {
                $scope.addNewError = '';

                // Mobile type default
                if (BasicInfo.value.fax._id == undefined) {
                    BasicInfo.value.fax._id = "";
                }
                if (BasicInfo.value.fax.rules == undefined) {
                    BasicInfo.value.fax.rules = "phone";
                }
                if (BasicInfo.value.fax.nosearch == undefined) {
                    BasicInfo.value.fax.nosearch = true;
                }
                if (BasicInfo.value.fax.default == undefined) {
                    BasicInfo.value.fax.default = "";
                }
                if (BasicInfo.value.fax.type == undefined) {
                    BasicInfo.value.fax.type = "textbox";
                }

                $scope.mobile = {
                    '_id': BasicInfo.value.fax._id,
                    'label': BasicInfo.value.fax.label,
                    'name': BasicInfo.value.fax.name,
                    'privacy': BasicInfo.value.fax.privacy,
                    'type': BasicInfo.value.fax.type,
                    'rules': BasicInfo.value.fax.rules,
                    'nosearch': BasicInfo.value.fax.nosearch,
                    'default': BasicInfo.value.fax.default,
                    'edit': false
                };

                $scope.listMobile = [];


                var phoneId = 0;
                $(BasicInfo.value.fax.value).each(function (index, object) {
                    phoneId = phoneId + 1;
                    var _default = false;
                    if (BasicInfo.value.fax.value[index].value == BasicInfo.value.fax.default)
                        _default = true;
                    if (BasicInfo.value.fax.value[index].id == undefined)
                        BasicInfo.value.fax.value[index].id = phoneId;

                    var temp = BasicInfo.value.fax.value[index].id;
                    if (temp == '')
                        BasicInfo.value.fax.value[index].id = phoneId;
                    else
                        BasicInfo.value.fax.value[index].id = parseInt(temp);
                    BasicInfo.value.fax.value[index].id = temp;

                    var tempPhone = BasicInfo.value.fax.value[index].value;

                    _userManager.GetPhoneCode({ phoneNumber: tempPhone }).then(function (phone) {
                        $scope.listMobile.push({
                            'id': temp,
                            'value': tempPhone,
                            'codeCountry': phone.CodeCountry,
                            'phoneNumber': phone.PhoneNumber,
                            'default': _default
                        });

                    }, function (errors) {

                    });

                });


                // add
                $scope.newMobile = { value: false };
                $scope.addNew = function (newPhone) {
                    $scope.newMobile.value = false;
                    if (newPhone.default == true) {
                        $scope.mobile.default = newPhone.value;
                        for (var i = 0; i < $scope.listMobile.length; i++) {
                            $scope.listMobile[i].default = false;
                        }
                    }

                    $scope.listMobile.push({
                        'id': phoneId + 1,
                        'value': newPhone.value,
                        'codeCountry': newPhone.codeCountry,
                        'phoneNumber': newPhone.phoneNumber,
                        'default': newPhone.default,
                    });
                    $scope.isNew.value = false;
                    $scope._new = {};
                }

                $scope.isNew = { value: false };
                $scope.new = function () {
                    $scope.isNew.value = true;
                    $scope._new =
                        {
                            'id': phoneId,
                            'value': "",
                            'codeCountry': "65",
                            'phoneNumber': "",
                            'default': true

                        };
                }

                $scope.removeMobile = function (mobile) {
                    var index = $scope.listMobile.indexOf(mobile);
                    swal({
                        title: "Are you sure to deleted  " + mobile.value + " ?",
                        type: "warning",
                        showCancelButton: true,
                        confirmButtonColor: "#DD6B55",
                        confirmButtonText: "Yes",
                        cancelButtonText: "No"

                    }).then(function () {
                        mobile.removed = true;
                        $scope.listMobile.splice(index, 1);
                        swal(
                            'Deleted',
                             mobile.value + ' has been deleted.',
                            'success'
                          )
                    }
                    , function (dismiss) {
                        if (dismiss === 'cancel') {

                        }
                    });
                };

                //check
                $scope.addNewError = '';
                $scope.CheckAddNew = function (_new) {
                    $scope.addNewError = '';
                    var check = true;
                    _new.value = '+' + _new.codeCountry + _new.phoneNumber;
                    for (var i = 0; i < $scope.listMobile.length; i++) {
                        if (_new.value == $scope.listMobile[i].value) {
                            $scope.addNewError = _new.value + " is exist";
                            break;
                        }
                    }
                    if ($scope.addNewError == '') {
                        $scope.addNew(_new);
                    }
                }

                $scope.CheckDefault = function (value) {
                    $scope.addNewError = '';
                    var check = true;
                    $scope.mobile.default = '+' + value.codeCountry + value.phoneNumber;
                    for (var i = 0; i < $scope.listMobile.length; i++) {
                        if ($scope.listMobile[i].value != value.value)
                            $scope.listMobile[i].default = false;
                        if ($scope.listMobile[i].value == value.value) {
                            $scope.listMobile[i].default = true;
                            $scope.mobile.default = value.value;
                        }
                    }
                }

                //check
                $scope.IsvalidPhoneNumber = function () {
                    $scope.addNewError = "";
                    var lstPhone = "";
                    if ($scope.listMobile.length > 0) {
                        for (var i = 0; i < $scope.listMobile.length; i++) {
                            var phone = '+' + $scope.listMobile[i].codeCountry + $scope.listMobile[i].phoneNumber;
                            if (i == 0) {
                                lstPhone = phone;
                            }
                            else
                                lstPhone = lstPhone + "," + phone;

                            for (var j = i + 1; j < $scope.listMobile.length; j++) {

                                if ($scope.listMobile[j].codeCountry + $scope.listMobile[j].phoneNumber == $scope.listMobile[i].codeCountry + $scope.listMobile[i].phoneNumber)
                                    $scope.addNewError = '+' + $scope.listMobile[i].codeCountry + $scope.listMobile[i].phoneNumber + " is duplicate.";
                            }

                        }
                        if ($scope.addNewError == '') {
                            _userManager.ValidPhone({ phoneNumber: lstPhone }).then(function (rs) {

                                if (rs.ValidPhone == true) {
                                    $scope.Save();
                                }
                                else
                                    $scope.addNewError = rs.PhoneNumber + " is invalid";
                            }, function (errors) {

                                $scope.addNewError = " Is invalid";

                                swal(
                                   'Deleted', 'Save mobile errror', 'error'
                                 )
                            })
                        }
                    }
                    else {
                        $scope.mobile.default = "";
                        $scope.Save();
                    }
                }
                //End Mobile
                $scope.messageBox = "";
                $scope.alerts = [];
            }
            $scope.InitData();
            $scope.Save = function () {
                var lstMobileSave = [];
                var isDefault = false;
                for (var i = 0; i < $scope.listMobile.length; i++) {
                    var tempP = '+' + $scope.listMobile[i].codeCountry + $scope.listMobile[i].phoneNumber;
                    if ($scope.listMobile[i].default == true) {
                        isDefault = true;
                        $scope.mobile.default = tempP;
                    }


                    lstMobileSave.push({
                        'id': $scope.listMobile[i].id,
                        'value': tempP
                    });
                }
                if (isDefault == false && lstMobileSave.length > 0) {

                    $scope.mobile.default = lstMobileSave[0].value;
                }

                BasicInfo.value.fax.default = $scope.mobile.default;

                BasicInfo.name = "Contact";

                VaultInformationService.VaultInformation.contact = BasicInfo;
                VaultInformationService.VaultInformation.contact.value.fax.value = lstMobileSave;
                VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
                    VaultInformationService.VaultInformation._id).then(function (response) {
                        $scope.mobile.edit = false;
                        $scope.addNewError = '';
                        $scope.InitData();

                    }, function (errors) {

                    });

            }

            $scope.saveVaultInformationOnSuccess = function (response) {
                $scope.mobile.edit = false;
                $scope.addNewError = '';

            }
            $scope.saveVaultInformationOnError = function (response) {
                alertService.renderSuccessMessage(response.ReturnMessage);
                $scope.messageBox = alertService.returnFormattedMessage();
                $scope.alerts = alertService.returnAlerts();
            }

            $scope.Cancel = function () {

                $scope.InitData();
            }

        }]);


// 5 Email
myApp.getController('EmailController',
    ['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService',
        function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService) {


            var BasicInfo = VaultInformationService.VaultInformation.contact;
            $scope.InitData = function () {
                $scope.addNewError = '';
                $scope.addNewEmailError = '';
                //star value
                $scope.addNewEmailError != ''
                $scope.listEmail = []
                $scope.showListEmail = {}
                $scope._email = {
                    'label': BasicInfo.value.email.label,
                    'name': BasicInfo.value.email.name,
                    'privacy': BasicInfo.value.email.privacy,
                    'default': BasicInfo.value.email.default,
                    'edit': false
                };

                if ($scope._email.name == '') {
                    $scope._email.name = BasicInfo.value.email.label;

                  

                }

                // Email
                var indexEmail = 0;
                $(BasicInfo.value.email.value).each(function (index, object) {
                    indexEmail = indexEmail + 1;
                    var _num = BasicInfo.value.email.value[index].id;
                    if (_num == '' || _num == undefined)
                        BasicInfo.value.email.value[index].id = indexEmail;
                    else
                        BasicInfo.value.email.value[index].id = parseInt(_num);
                    var _default = false;
                    if (BasicInfo.value.email.value[index].value == BasicInfo.value.email.default)
                        _default = true;
                    $scope.listEmail.push({
                        'id': BasicInfo.value.email.value[index].id,
                        'value': BasicInfo.value.email.value[index].value,

                        'default': _default
                    })
                });

                $scope.showListEmail = angular.copy($scope.listEmail);
                $scope.addEmail = { value: false };
                $scope.newEmail = function () {
                    $scope.addEmail.value = true;
                    indexEmail = indexEmail + 1;
                    $scope._newEmail =
                        {
                            'id': indexEmail,
                            'value': "",

                            'default': true
                        };
                }

                $scope.addNewEmail = function (newMail) {
                    $scope.addEmail.value = false;
                    if (newMail.default == true) {
                        $scope._email.default = newMail.value;
                        for (var i = 0; i < $scope.listEmail.length; i++) {
                            $scope.listEmail[i].default = false;
                        }
                    }


                    $scope.listEmail.push({
                        'id': newMail.id,
                        'value': newMail.value,
                        'default': newMail.default
                    });
                }

                $scope.removeEmail = function (email) {
                    var index = $scope.listEmail.indexOf(email);
                    swal({
                        title: "Are you sure to deleted  " + email.value + " ?",
                        type: "warning",
                        showCancelButton: true,
                        confirmButtonColor: "#DD6B55",
                        confirmButtonText: "Yes",
                        cancelButtonText: "No!"

                    }).then(function () {
                        email.removed = true;
                        $scope.listEmail.splice(index, 1);
                        swal(
                            'Deleted!',
                             email.value + ' has been deleted.',
                            'success'
                          )
                    }
                    , function (dismiss) {
                        if (dismiss === 'cancel') {

                        }
                    });

                };
                //end email

                $scope.IsExistEmail = function (_email) {
                    $scope.addNewEmailError = '';
                    for (var i = 0; i < $scope.listEmail.length; i++) {
                        if (_email.value == $scope.listEmail[i].value)
                            $scope.addNewEmailError = _email.value + " is exist.";
                    }
                    if ($scope.addNewEmailError == '') {
                        $scope.addNewEmail(_email);
                    }
                }

                $scope.CheckSave = function () {
                    $scope.addNewEmailError = '';
                    if ($scope.listEmail.length > 0) {
                        for (var i = 0; i < $scope.listEmail.length; i++) {
                            for (var j = i + 1; j < $scope.listEmail.length; j++) {
                                if ($scope.listEmail[j].value == $scope.listEmail[i].value) {
                                    $scope.addNewEmailError = _email.value + " is duplicate.";
                                    break;
                                }
                            }
                            if ($scope.addNewEmailError != '') {
                                break;
                            }
                        }

                        if ($scope.addNewEmailError == '')
                            $scope.Save();
                    }
                    else {
                        $scope._email.default = "";
                        $scope.Save();
                    }


                }
                $scope.CheckDefault = function (value) {

                    var check = true;

                    for (var i = 0; i < $scope.listEmail.length; i++) {
                        if ($scope.listEmail[i].value != value.value)
                            $scope.listEmail[i].default = false;
                        if ($scope.listEmail[i].value == value.value) {
                            $scope.listEmail[i].default = true;
                            $scope._email.default = value.value;
                        }
                    }
                }

                //
                $scope.messageBox = "";
                $scope.alerts = [];
            }

            $scope.$on('email', function () {
                $scope.InitData();
            });
            $scope.InitData();

            $scope.Save = function () {

                //Email
                var lstEmailSave = [];
                var isDefault = false;
                for (var i = 0; i < $scope.listEmail.length; i++) {
                    if ($scope.listEmail[i].default == true) {
                        isDefault = true;
                        $scope._email.default = $scope.listEmail[i].value;
                    }

                    lstEmailSave.push({
                        'id': $scope.listEmail[i].id,
                        'value': $scope.listEmail[i].value
                    });
                }

                //

                if (isDefault == false && lstEmailSave.length > 0) {

                    $scope._email.default = lstEmailSave[0].value;
                }
                //
                BasicInfo.value.email.default = $scope._email.default;

                BasicInfo.value.email.name = 'Email';
                VaultInformationService.VaultInformation.contact = BasicInfo;

                VaultInformationService.VaultInformation.contact.value.email.value = lstEmailSave;

                VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
                    VaultInformationService.VaultInformation._id).then(function (response) {
                        $scope._email.edit = false;
                        $scope.addNewEmailError = '';
                        $scope.InitData();

                    }, function (errors) {

                    });

            }

            $scope.saveVaultInformationOnSuccess = function (response) {
                $scope._email.isEdit = false;
                $scope.addNewEmailError = '';
            }
            $scope.saveVaultInformationOnError = function (response) {
                alertService.renderSuccessMessage(response.ReturnMessage);
                $scope.messageBox = alertService.returnFormattedMessage();
                $scope.alerts = alertService.returnAlerts();
            }

            $scope.Cancel = function () {
                $scope._email.edit = false;
                $scope.InitData();
            }

        }]);


// 5 Office Email
myApp.getController('OfficeEmailController',
    ['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService',
        function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService) {

            var BasicInfo = VaultInformationService.VaultInformation.contact;
            $scope.InitData = function () {
                $scope.addNewError = '';
                $scope.addNewEmailError = '';
                //star value
                $scope.addNewEmailError != ''
                $scope.listEmail = []
                $scope.showListEmail = {}
                if (BasicInfo.value.officeEmail === undefined || BasicInfo.value.officeEmail === null) {
                    BasicInfo.value.officeEmail = {
                        'label': 'Office email',
                        'name': 'Office email',
                        'privacy': 'Office email',
                        'default': 'Office email',
                        'value': []
                    }

                }

                $scope._email = {
                    'label': BasicInfo.value.officeEmail.label,
                    'name': BasicInfo.value.officeEmail.name,
                    'privacy': BasicInfo.value.officeEmail.privacy,
                    'default': BasicInfo.value.officeEmail.default,
                    'edit': false
                };

                // Email
                var indexEmail = 0;
                $(BasicInfo.value.officeEmail.value).each(function (index, object) {
                    indexEmail = indexEmail + 1;
                    var _num = BasicInfo.value.officeEmail.value[index].id;
                    if (_num == '' || _num == undefined)
                        BasicInfo.value.officeEmail.value[index].id = indexEmail;
                    else
                        BasicInfo.value.officeEmail.value[index].id = parseInt(_num);
                    var _default = false;
                    if (BasicInfo.value.officeEmail.value[index].value == BasicInfo.value.officeEmail.default)
                        _default = true;
                    $scope.listEmail.push({
                        'id': BasicInfo.value.officeEmail.value[index].id,
                        'value': BasicInfo.value.officeEmail.value[index].value,
                        'default': _default
                    })
                });

                $scope.showListEmail = angular.copy($scope.listEmail);
                $scope.addEmail = { value: false };
                $scope.newEmail = function () {
                    $scope.addEmail.value = true;
                    indexEmail = indexEmail + 1;
                    $scope._newEmail =
                        {
                            'id': indexEmail,
                            'value': "",
                            'default': true
                        };
                }

                $scope.addNewEmail = function (newMail) {
                    $scope.addEmail.value = false;
                    if (newMail.default == true) {
                        $scope._email.default = newMail.value;
                        for (var i = 0; i < $scope.listEmail.length; i++) {
                            $scope.listEmail[i].default = false;
                        }
                    }


                    $scope.listEmail.push({
                        'id': newMail.id,
                        'value': newMail.value,
                        'default': newMail.default
                    });
                }

                $scope.removeEmail = function (email) {
                    var index = $scope.listEmail.indexOf(email);
                    swal({
                        title: "Are you sure to deleted  " + email.value + " ?",
                        type: "warning",
                        showCancelButton: true,
                        confirmButtonColor: "#DD6B55",
                        confirmButtonText: "Yes",
                        cancelButtonText: "No!"

                    }).then(function () {
                        email.removed = true;
                        $scope.listEmail.splice(index, 1);
                        swal(
                            'Deleted!',
                             email.value + ' has been deleted.',
                            'success'
                          )
                    }
                    , function (dismiss) {
                        if (dismiss === 'cancel') {
                        }
                    });
                };

                //end email
                $scope.IsExistEmail = function (_email) {
                    $scope.addNewEmailError = '';
                    for (var i = 0; i < $scope.listEmail.length; i++) {
                        if (_email.value == $scope.listEmail[i].value)
                            $scope.addNewEmailError = _email.value + " is exist.";
                    }
                    if ($scope.addNewEmailError == '') {
                        $scope.addNewEmail(_email);
                    }
                }

                $scope.CheckSave = function () {
                    $scope.addNewEmailError = '';
                    if ($scope.listEmail.length > 0) {
                        for (var i = 0; i < $scope.listEmail.length; i++) {
                            for (var j = i + 1; j < $scope.listEmail.length; j++) {
                                if ($scope.listEmail[j].value == $scope.listEmail[i].value) {
                                    $scope.addNewEmailError = _email.value + " is duplicate.";
                                    break;
                                }
                            }
                            if ($scope.addNewEmailError != '') {
                                break;
                            }
                        }

                        if ($scope.addNewEmailError == '')
                            $scope.Save();
                    }
                    else {
                        $scope._email.default = "";
                        $scope.Save();
                    }


                }
                $scope.CheckDefault = function (value) {

                    var check = true;

                    for (var i = 0; i < $scope.listEmail.length; i++) {
                        if ($scope.listEmail[i].value != value.value)
                            $scope.listEmail[i].default = false;
                        if ($scope.listEmail[i].value == value.value) {
                            $scope.listEmail[i].default = true;
                            $scope._email.default = value.value;
                        }
                    }
                }

                //
                $scope.messageBox = "";
                $scope.alerts = [];
            }

            $scope.$on('email', function () {
                $scope.InitData();
            });
            $scope.InitData();

            $scope.Save = function () {

                //Email
                var lstEmailSave = [];
                var isDefault = false;
                for (var i = 0; i < $scope.listEmail.length; i++) {
                    if ($scope.listEmail[i].default == true) {
                        isDefault = true;
                        $scope._email.default = $scope.listEmail[i].value;
                    }
                    lstEmailSave.push({
                        'id': $scope.listEmail[i].id,
                        'value': $scope.listEmail[i].value
                    });
                }

                //

                if (isDefault == false && lstEmailSave.length > 0) {

                    $scope._email.default = lstEmailSave[0].value;
                }
                //
                BasicInfo.value.officeEmail.default = $scope._email.default;

                BasicInfo.value.officeEmail.name = 'Office email';
                VaultInformationService.VaultInformation.contact = BasicInfo;

                VaultInformationService.VaultInformation.contact.value.officeEmail.value = lstEmailSave;

                VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
                    VaultInformationService.VaultInformation._id).then(function (response) {
                        $scope._email.edit = false;
                        $scope.addNewEmailError = '';
                        $scope.InitData();

                    }, function (errors) {

                    });

            }

            $scope.saveVaultInformationOnSuccess = function (response) {
                $scope._email.isEdit = false;
                $scope.addNewEmailError = '';
            }
            $scope.saveVaultInformationOnError = function (response) {
                alertService.renderSuccessMessage(response.ReturnMessage);
                $scope.messageBox = alertService.returnFormattedMessage();
                $scope.alerts = alertService.returnAlerts();
            }

            $scope.Cancel = function () {
                $scope._email.edit = false;
                $scope.InitData();
            }

        }]);


