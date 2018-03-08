// var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.cellNav'], true);
var myApp = getApp("myApp", true);

//3.0 Group Address
myApp.getController('GroupAddressController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter', 'vaultService', 'NetworkService','rguNotify',
function ($scope, $rootScope, $http, _userManager, _authService, alertService, VaultInformationService, CountryCityService, dateFilter, vaultService, _networkService, rguNotify) {

    "use strict";
    var BasicInfo = VaultInformationService.VaultInformation.groupAddress; 
    // Begin push to Vault
    $scope.result = {};
    //$scope.sendTo = {};
    $scope.sendPush = function () {
        console.log($scope.result);
        $scope.PushToVault();
    };
    $scope.vaultGroup = {
        name: 'address',
        type: 'listgroup',
        label: BasicInfo.label,
        model: BasicInfo.value
    };

    vaultService.populateGroup($scope.vaultGroup);
    //
    $scope.GetAllFriends = function () {
        _networkService.GetFriends().then(function (result) {
            $scope.network = result;
        })
    }
    $scope.GetAllFriends();
    $scope.nameItem = "";
    $scope.PushToVault = function () {
        var data = new Object();
        var campaign = new Object();
        campaign.type = "ManualPushToVault";
        campaign.status = "Active";
     
        campaign.description = $scope.result.sendTo.comment;
        if ($scope.result.sendTo.comment === '' || $scope.result.sendTo.comment === undefined)
            campaign.description = "Manual push to Vault";
        campaign.fields = [];
        data.UserId = $scope.result.sendTo.userId;
        data.CampaignType = "ManualPushToVault";


        if ($scope.result.fields[0].jsPath != null || $scope.result.fields[0].jsPath != undefined) {
            var cat = $scope.result.fields[0].jsPath;
          
            var catArr = cat.split(".");
            var catName = catArr[1];

            if (catName === 'currentAddress') {
                $scope.nameItem = "Current address";
            } else if (catName === 'deliveryAddress') {
                $scope.nameItem = "Delivery address";

            } else if (catName === 'billingAddress') {
                $scope.nameItem = "Billing address";

            } else if (catName === 'mailingAddress') {
                $scope.nameItem = "Mailing Address";
            }
            else if (catName === 'pobox') {
                $scope.nameItem = "P.O Box";
            }
           
        }
        campaign.name = "Address" + " / " + $scope.nameItem;

        data.Listvaults = [];
          data.pushtype = "PushToVault";
        if ($scope.result.sendTo.userId != null && $scope.result.sendTo.userId != undefined)
            data.usernotifycationid = $scope.result.sendTo.userId;

        if ($scope.result.sendTo.userEmail != "")
        {
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
                displayName: $scope.result.fields[index].label,
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
            //
            campaign.fields.push(field);
            //

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
                //  $uibModalInstance.close(response);
                rguNotify.add('Push to Vault completed.');
            })
            .error(function (errors) {
                var errorObj = __errorHandler.ProcessErrors(errors);
                __errorHandler.Swal(errorObj, _sweetAlert);
            });
    }

    // End push to vault
   
    $scope.InitData = function () {
        $scope.IsEdit = false;
        $scope.group = {
            _label: BasicInfo.label,
            _name: BasicInfo.name,
            _default: BasicInfo.default,
            _privacy: BasicInfo.privacy
        };
      
        //list address
        $scope.list_group = ["Current Address", "Delivery Address", "Billing Address", "Mailing Address", "Pobox"];
        $scope._select = { _value: '' };
        if ($scope.group._default == '') {
            $scope.group._default = "Current Address";
        }

        if ($scope._select._value == '')
            $scope._select._value = $scope.group._default;

     
        //

    }
    //  Son
   
    $scope.InitData();
    $scope.Edit = function () {
        $scope.IsEdit = true;
    }

    // == edit == 
    $scope.Save = function () {

        //address
        BasicInfo.label = $scope.group._label;
        if ($scope.group._name == '') {
            $scope.group._name = $scope.group._label;
        }
        BasicInfo.name = $scope.group._name;
        BasicInfo.privacy = $scope.group._privacy;
        BasicInfo.default = $scope.group._default;

        // ===
        VaultInformationService.VaultInformation.groupAddress = BasicInfo;
        VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
            VaultInformationService.VaultInformation._id).then(function (response) {
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
        $scope.InitData();
    }

    $scope.goBack = function () {
        $scope.$location.path('/VaultInformation');
    };
    $scope.closePanel = function () {
        $scope.Cancel();
    };

}])

//3.1 currentAddress
myApp.getController('CurrentAddressController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter', 'vaultService', 'NetworkService',
function ($scope, $rootScope, $http, _userManager, _authService, alertService, VaultInformationService, CountryCityService, dateFilter, vaultService, _networkService) {

    "use strict";

    //currentAddress   
    var BasicInfo = VaultInformationService.VaultInformation.groupAddress.value.currentAddress;

    $scope.InitData = function () {
        $scope.IsEdit = false;

        $scope._form = {
            _label: BasicInfo.label,
            _name: BasicInfo.name,
            _default: BasicInfo.default,
            _privacy: BasicInfo.privacy
        };
        //
      
        $scope._listForm = [];

        $(BasicInfo.value).each(function (index, object) {
            var endDateValue = new Date(BasicInfo.value[index].endDate);
            var dateValue = new Date('1000-01-01');
            var checkEndDate = true;
            if (BasicInfo.value[index].endDate == undefined)
                BasicInfo.value[index].endDate = "";
            if (BasicInfo.value[index].endDate == "" || endDateValue < dateValue)
                checkEndDate = false;

            $scope._listForm.push({
                IsEdit: false,
                
                privacy: BasicInfo.value[index].privacy,
                description: BasicInfo.value[index].description,
                addressLine: BasicInfo.value[index].addressLine,
                addressLine_lat: BasicInfo.value[index].addressLine_lat,
                addressLine_lng: BasicInfo.value[index].addressLine_lng,
                instruction: BasicInfo.value[index].instruction,
                startDate: BasicInfo.value[index].startDate,
                endDate: BasicInfo.value[index].endDate,
                checkEndDate: checkEndDate,

                state: BasicInfo.value[index].state,
                zipCode: BasicInfo.value[index].zipCode,
                note: BasicInfo.value[index].note,
                _default: BasicInfo.value[index]._default,
                countryCity: { country: BasicInfo.value[index].country, city: BasicInfo.value[index].city }
            })

            //if ($scope._listForm[index].startDate != "" && $scope._listForm[index].startDate != undefined)

            //    $scope._listForm[index].startDate = new Date();

            //if ($scope._listForm[index].endDate != "" && $scope._listForm[index].endDate != undefined)
            //    $scope._listForm[index].endDate = new Date($scope._listForm[index].endDate);
            //else
            //    $scope._listForm[index].endDate = new Date(today.getTime() + (24 * 60 * 60 * 1000));
        });
        // Or Date.today()
        //var tomorrow = today.setDate(today.getDate() + 1);
        //.getDate() + 1
        $scope._new =
            {
                'IsEdit': false,
                'privacy': "",
                'description': "",
                '_default': true,
                'startDate': new Date(),
                'endDate': '',
                'checkEndDate': false,
                'addressLine': "",
                'addressLine_lat': "",
                'addressLine_lng': "",
                'instruction': "",
                'countryCity': "",

                'state': "",
                'zipCode': "",
                'note': ""

            };

        // $scope._new.endDate = new Date(today.addDays(2));

        $scope.addNewForm = function (newForm) {
            if (newForm.checkEndDate == false)
                newForm.endDate = "";
            $scope._listForm.push({
                'IsEdit': false,
                'privacy': newForm.privacy,
                'description': newForm.description,
                '_default': newForm._default,
                'startDate': newForm.startDate,
                'checkEndDate': newForm.checkEndDate,
                'endDate': newForm.endDate,
                'addressLine': newForm.addressLine,
                'addressLine_lat': newForm.addressLine_lat,
                'addressLine_lng': newForm.addressLine_lng,
                'instruction': newForm.instruction,
                'countryCity': newForm.countryCity,
                'state': newForm.state,
                'zipCode': newForm.zipCode,
                'note': newForm.note,
            });

        }

        $scope.removeForm = function (value) {
            var index = $scope._listForm.indexOf(value);
            swal({
                title: "Are you sure to delete form " + value.description + " ?",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes",
                cancelButtonText: "No"
                //,
                //closeOnConfirm: false,
                //closeOnCancel: true
            }).then(function () {
                $scope._listForm.splice(index, 1);
                $scope.Save();
                swal(
                    'Deleted',
                    'Form ' + value.description + ' has been deleted.',
                    'success'
                  )
            }
            , function (dismiss) {
                if (dismiss === 'cancel') {

                }
            });
        };

        ////  

        //
        $scope.messageBox = "";
        $scope.alerts = [];

    }
    $scope.$on('currentAddress', function () {
        $scope.InitData();
    });
    $scope.InitData();
    $scope.Edit = function () {
        $scope.IsEdit = true;
    }

    // == edit == 
    $scope.Save = function () {

        //address
        BasicInfo.label = $scope._form._label;
        if ($scope._form._name == '') {
            $scope._form._name = $scope._form._label;
        }
        BasicInfo.name = $scope._form._name;
        BasicInfo.privacy = $scope._form._privacy;

        //
        var checkDefault = true;
        $($scope._listForm).each(function (index) {

            if (checkDefault == true && $scope._listForm[index]._default == true && $scope._listForm[index].description != $scope._form._default) {
                $scope._form._default = $scope._listForm[index].description;
                checkDefault = false;
            }

        });

        BasicInfo.default = $scope._form._default;

        // instruction
        var lstFormSave = [];
        $($scope._listForm).each(function (index, object) {


            if (checkDefault == false && $scope._listForm[index].description != $scope._form._default) {
                $scope._listForm[index]._default = false;
            }
            //
            var endDateValue = new Date($scope._listForm[index].endDate);
            var dateValue = new Date('1000-01-01');
          
            if (endDateValue < dateValue)
                $scope._listForm[index].endDate = "";
            //
            if ($scope._listForm[index].checkEndDate == false)
                $scope._listForm[index].endDate = "";
            lstFormSave.push({
                privacy: $scope._listForm[index].privacy,
                description: $scope._listForm[index].description,
                _default: $scope._listForm[index]._default,
                startDate: $scope._listForm[index].startDate,
                endDate: $scope._listForm[index].endDate,

                addressLine: $scope._listForm[index].addressLine,
                addressLine_lat: $scope._listForm[index].addressLine_lat,
                addressLine_lng: $scope._listForm[index].addressLine_lng,
                instruction: $scope._listForm[index].instruction,
                country: $scope._listForm[index].countryCity.country,

                city: $scope._listForm[index].countryCity.city,
                state: $scope._listForm[index].state,
                zipCode: $scope._listForm[index].zipCode,
                note: $scope._listForm[index].note
            })
            $scope._listForm[index].IsEdit = false;
        });

        // $scope.checkDefault();
        //save

        // === 
        var addressName = VaultInformationService.VaultInformation.groupAddress.name;
        if (addressName == "")
            VaultInformationService.VaultInformation.groupAddress.name = "Address";
        //
        VaultInformationService.VaultInformation.groupAddress.value.currentAddress = BasicInfo;
        VaultInformationService.VaultInformation.groupAddress.value.currentAddress.value = lstFormSave;
        VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,

            VaultInformationService.VaultInformation._id).then(function (response) {
                $scope.IsEdit = false;
                $scope._new = "";
                $rootScope.$broadcast('currentAddress');

            }, function (errors) {

            });
    }

    $scope.saveVaultInformationOnSuccess = function (response) {
        $scope.IsEdit = false;
        $scope._new = "";
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
//3.2 deliveryAddress
myApp.getController('DeliveryAddressController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter) {

    "use strict";
    var BasicInfo = VaultInformationService.VaultInformation.groupAddress.value.deliveryAddress;
    $scope.InitData = function () {
        $scope.IsEdit = false;

        $scope._form = {
            _label: BasicInfo.label,
            _name: BasicInfo.name,
            _default: BasicInfo.default,
            _privacy: BasicInfo.privacy
        };
        //
        var today = new Date();
        $scope._listForm = [];

        $(BasicInfo.value).each(function (index, object) {
            var endDateValue = new Date(BasicInfo.value[index].endDate);
            var dateValue = new Date('1000-01-01');
            var checkEndDate = true;
            if (BasicInfo.value[index].endDate == undefined)
                BasicInfo.value[index].endDate = "";
            if (BasicInfo.value[index].endDate == "" || endDateValue < dateValue)
                checkEndDate = false;

            $scope._listForm.push({
                IsEdit: false,
                privacy: BasicInfo.value[index].privacy,
                description: BasicInfo.value[index].description,
                addressLine: BasicInfo.value[index].addressLine,
                addressLine_lat: BasicInfo.value[index].addressLine_lat,
                addressLine_lng: BasicInfo.value[index].addressLine_lng,
                instruction: BasicInfo.value[index].instruction,
                startDate: BasicInfo.value[index].startDate,
                endDate: BasicInfo.value[index].endDate,
                checkEndDate: checkEndDate,
                state: BasicInfo.value[index].state,
                zipCode: BasicInfo.value[index].zipCode,
                note: BasicInfo.value[index].note,
                _default: BasicInfo.value[index]._default,
                countryCity: { country: BasicInfo.value[index].country, city: BasicInfo.value[index].city }
            })

            if ($scope._listForm[index].startDate != "" && $scope._listForm[index].startDate != undefined)
                $scope._listForm[index].startDate = new Date($scope._listForm[index].startDate);
            else
                $scope._listForm[index].startDate = new Date();
        });

        $scope._new =
            {
                'IsEdit': false,
                'privacy': "",
                'description': "",
                '_default': true,
                'startDate': new Date(),
                'endDate': '',
                'addressLine': "",
                'addressLine_lat': "",
                'addressLine_lng': "",
                'instruction': "",
                'countryCity': "",

                'state': "",
                'zipCode': "",
                'note': "",
                'checkEndDate': false

            };


        $scope.addNewForm = function (newForm) {
            if (newForm.checkEndDate == false)
                newForm.endDate = "";

            $scope._listForm.push({
                'IsEdit': false,
                'privacy': newForm.privacy,
                'description': newForm.description,
                '_default': newForm._default,
                'startDate': newForm.startDate,
                'endDate': newForm.endDate,
                'checkEndDate': newForm.checkEndDate,
                'addressLine': newForm.addressLine,
                'addressLine_lat': newForm.addressLine_lat,
                'addressLine_lng': newForm.addressLine_lng,
                'instruction': newForm.instruction,
                'countryCity': newForm.countryCity,
                'state': newForm.state,
                'zipCode': newForm.zipCode,
                'note': newForm.note,
            });

        }

        $scope.removeForm = function (value) {

            //
            var index = $scope._listForm.indexOf(value);
            swal({
                title: "Are you sure to delete form " + value.description + " ?",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes",
                cancelButtonText: "No!"
                //,
                //closeOnConfirm: false,
                //closeOnCancel: true
            }).then(function () {
                $scope._listForm.splice(index, 1);
                $scope.Save();
                swal(
                    'Deleted!',
                    'Form ' + value.description + ' has been deleted.',
                    'success'
                  )
            }
            , function (dismiss) {
                if (dismiss === 'cancel') {

                }
            });
        };

        ////  

        //
        $scope.messageBox = "";
        $scope.alerts = [];

    }
    $scope.$on('deliveryAddress', function () {
        $scope.InitData();
    });
    $scope.InitData();
    $scope.Edit = function () {
        $scope.IsEdit = true;
    }

    // == edit == 
    $scope.Save = function () {

        //address
        BasicInfo.label = $scope._form._label;
        if ($scope._form._name == '') {
            $scope._form._name = $scope._form._label;
        }
        BasicInfo.name = $scope._form._name;
        BasicInfo.privacy = $scope._form._privacy;


        //
        var checkDefault = true;
        $($scope._listForm).each(function (index) {

            if (checkDefault == true && $scope._listForm[index]._default == true && $scope._listForm[index].description != $scope._form._default) {
                $scope._form._default = $scope._listForm[index].description;
                checkDefault = false;
            }


        });

        BasicInfo.default = $scope._form._default;

        //
        var lstFormSave = [];
        $($scope._listForm).each(function (index, object) {


            if (checkDefault == false && $scope._listForm[index].description != $scope._form._default) {
                $scope._listForm[index]._default = false;
            }
            // Check enddate
            var endDateValue = new Date($scope._listForm[index].endDate);
            var dateValue = new Date('1000-01-01');
            if (endDateValue < dateValue)
                $scope._listForm[index].endDate = "";
            if ($scope._listForm[index].checkEndDate == false)
                $scope._listForm[index].endDate = "";

            lstFormSave.push({
                privacy: $scope._listForm[index].privacy,

                description: $scope._listForm[index].description,
                _default: $scope._listForm[index]._default,
                startDate: $scope._listForm[index].startDate,
                endDate: $scope._listForm[index].endDate,
               
                addressLine: $scope._listForm[index].addressLine,
                addressLine_lat: $scope._listForm[index].addressLine_lat,
                addressLine_lng: $scope._listForm[index].addressLine_lng,
                instruction: $scope._listForm[index].instruction,
                country: $scope._listForm[index].countryCity.country,
                city: $scope._listForm[index].countryCity.city,
                state: $scope._listForm[index].state,
                zipCode: $scope._listForm[index].zipCode,
                note: $scope._listForm[index].note

            })


            $scope._listForm[index].IsEdit = false;
        });

        // $scope.checkDefault();


        // === 
        var addressName = VaultInformationService.VaultInformation.groupAddress.name;
        if (addressName == "")
            VaultInformationService.VaultInformation.groupAddress.name = "Address";
        //
        VaultInformationService.VaultInformation.groupAddress.value.deliveryAddress = BasicInfo;
        VaultInformationService.VaultInformation.groupAddress.value.deliveryAddress.value = lstFormSave;
        VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
            VaultInformationService.VaultInformation._id).then(function (response) {
                $scope.IsEdit = false;
                $scope._new = "";
                $rootScope.$broadcast('deliveryAddress');
               
            }, function (errors) {

            });
    }

    $scope.saveVaultInformationOnSuccess = function (response) {
        $scope.IsEdit = false;
        $scope._new = "";
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
//3.3 Billing Address
myApp.getController('BillingAddressController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter', 
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter) {

    "use strict";
    var BasicInfo = VaultInformationService.VaultInformation.groupAddress.value.billingAddress;
    $scope.InitData = function () {
        $scope.IsEdit = false;

        $scope._form = {
            _label: BasicInfo.label,
            _name: BasicInfo.name,
            _default: BasicInfo.default,
            _privacy: BasicInfo.privacy
        };
        //
        var today = new Date();
        $scope._listForm = [];

        $(BasicInfo.value).each(function (index, object) {
            var endDateValue = new Date(BasicInfo.value[index].endDate);
            var dateValue = new Date('1000-01-01');
            var checkEndDate = true;
            if (BasicInfo.value[index].endDate == undefined)
                BasicInfo.value[index].endDate = "";
            if (BasicInfo.value[index].endDate == "" || endDateValue < dateValue)
                checkEndDate = false;

            $scope._listForm.push({
                IsEdit: false,
                privacy: BasicInfo.value[index].privacy,
                description: BasicInfo.value[index].description,
                addressLine: BasicInfo.value[index].addressLine,
                addressLine_lat: BasicInfo.value[index].addressLine_lat,
                addressLine_lng: BasicInfo.value[index].addressLine_lng,
                instruction: BasicInfo.value[index].instruction,
                startDate: BasicInfo.value[index].startDate,
                endDate: BasicInfo.value[index].endDate,
                checkEndDate: checkEndDate,
                state: BasicInfo.value[index].state,
                zipCode: BasicInfo.value[index].zipCode,
                note: BasicInfo.value[index].note,
                _default: BasicInfo.value[index]._default,
                countryCity: { country: BasicInfo.value[index].country, city: BasicInfo.value[index].city }
            })

            if ($scope._listForm[index].startDate != "" && $scope._listForm[index].startDate != undefined)
                $scope._listForm[index].startDate = new Date($scope._listForm[index].startDate);
            else
                $scope._listForm[index].startDate = new Date();

            //if ($scope._listForm[index].endDate != "" && $scope._listForm[index].endDate != undefined)
            //    $scope._listForm[index].endDate = new Date($scope._listForm[index].endDate);
            //else
            //    $scope._listForm[index].endDate = new Date(today.getTime() + (24 * 60 * 60 * 1000));
        });

        $scope._new =
            {
                'IsEdit': false,
                'privacy': "",
                'description': "",
                '_default': true,
                'startDate': new Date(),
                'endDate': '',
                'checkEndDate': false,
                'addressLine': "",
                'addressLine_lat': "",
                'addressLine_lng': "",
                'instruction': "",
                'countryCity': "",
                'state': "",
                'zipCode': "",
                'note': ""
                

            };


        $scope.addNewForm = function (newForm) {
            if (newForm.checkEndDate == false)
                newForm.endDate = "";
            $scope._listForm.push({
                'IsEdit': false,
                'privacy': newForm.privacy,
                'description': newForm.description,
                '_default': newForm._default,
                'startDate': newForm.startDate,
                'endDate': newForm.endDate,
                'checkEndDate': newForm.checkEndDate,
                'addressLine': newForm.addressLine,
                'addressLine_lat': newForm.addressLine_lat,
                'addressLine_lng': newForm.addressLine_lng,
                'instruction': newForm.instruction,
                'countryCity': newForm.countryCity,
                'state': newForm.state,
                'zipCode': newForm.zipCode,
                'note': newForm.note
            });

        }

        $scope.removeForm = function (value) {

            //
            var index = $scope._listForm.indexOf(value);
            swal({
                title: "Are you sure to delete form " + value.description + " ?",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes",
                cancelButtonText: "No"
                //,
                //closeOnConfirm: false,
                //closeOnCancel: true
            }).then(function () {
                $scope._listForm.splice(index, 1);
                $scope.Save();
                swal(
                    'Deleted',
                    'Form ' + value.description + ' has been deleted.',
                    'success'
                  )
            }
            , function (dismiss) {
                if (dismiss === 'cancel') {

                }
            });

        };

        ////  

        //
        $scope.messageBox = "";
        $scope.alerts = [];

    }
    $scope.$on('billingAddress', function () {
        $scope.InitData();
    });
    $scope.InitData();
    $scope.Edit = function () {
        $scope.IsEdit = true;
    }

    // == edit == 
    $scope.Save = function () {

        //address
        BasicInfo.label = $scope._form._label;
        if ($scope._form._name == '') {
            $scope._form._name = $scope._form._label;
        }
        BasicInfo.name = $scope._form._name;
        BasicInfo.privacy = $scope._form._privacy;


        //
        var checkDefault = true;
        $($scope._listForm).each(function (index) {

            if (checkDefault == true && $scope._listForm[index]._default == true && $scope._listForm[index].description != $scope._form._default) {
                $scope._form._default = $scope._listForm[index].description;
                checkDefault = false;
            }


        });

        BasicInfo.default = $scope._form._default;

        //
        var lstFormSave = [];
        $($scope._listForm).each(function (index, object) {


            if (checkDefault == false && $scope._listForm[index].description != $scope._form._default) {
                $scope._listForm[index]._default = false;
            }

            // Check enddate
            var endDateValue = new Date($scope._listForm[index].endDate);
            var dateValue = new Date('1000-01-01');
            if (endDateValue < dateValue)
                $scope._listForm[index].endDate = "";
            if ($scope._listForm[index].checkEndDate == false)
                $scope._listForm[index].endDate = "";

            lstFormSave.push({
                privacy: $scope._listForm[index].privacy,

                description: $scope._listForm[index].description,
                _default: $scope._listForm[index]._default,

                startDate: $scope._listForm[index].startDate,
                endDate: $scope._listForm[index].endDate,
             
                addressLine: $scope._listForm[index].addressLine,
                addressLine_lat: $scope._listForm[index].addressLine_lat,
                addressLine_lng: $scope._listForm[index].addressLine_lng,
                instruction: $scope._listForm[index].instruction,
                country: $scope._listForm[index].countryCity.country,
                city: $scope._listForm[index].countryCity.city,
                state: $scope._listForm[index].state,
                zipCode: $scope._listForm[index].zipCode,
                note: $scope._listForm[index].note

            })


            $scope._listForm[index].IsEdit = false;
        });

        // $scope.checkDefault();


        // === 
        var addressName = VaultInformationService.VaultInformation.groupAddress.name;
        if (addressName == "")
            VaultInformationService.VaultInformation.groupAddress.name = "Address";
        //
        VaultInformationService.VaultInformation.groupAddress.value.billingAddress = BasicInfo;
        VaultInformationService.VaultInformation.groupAddress.value.billingAddress.value = lstFormSave;
        VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
            VaultInformationService.VaultInformation._id).then(function (response) {
                $scope.IsEdit = false;
                $scope._new = "";
                $rootScope.$broadcast('billingAddress');
               
            }, function (errors) {

            });
    }

    $scope.saveVaultInformationOnSuccess = function (response) {
        $scope.IsEdit = false;
        $scope._new = "";
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

//3.4 mailingAddress
myApp.getController('MailingAddressController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter) {

    "use strict";
    var BasicInfo = VaultInformationService.VaultInformation.groupAddress.value.mailingAddress;
    $scope.InitData = function () {
        $scope.IsEdit = false;

        $scope._form = {
            _label: BasicInfo.label,
            _name: BasicInfo.name,
            _default: BasicInfo.default,
            _privacy: BasicInfo.privacy
        };
        //
        var today = new Date();
        $scope._listForm = [];

        $(BasicInfo.value).each(function (index, object) {
            var endDateValue = new Date(BasicInfo.value[index].endDate);
            var dateValue = new Date('1000-01-01');
            var checkEndDate = true;
            if (BasicInfo.value[index].endDate == undefined)
                BasicInfo.value[index].endDate = "";
            if (BasicInfo.value[index].endDate == "" || endDateValue < dateValue)
                checkEndDate = false;
            $scope._listForm.push({
                IsEdit: false,
                privacy: BasicInfo.value[index].privacy,
                description: BasicInfo.value[index].description,
                addressLine: BasicInfo.value[index].addressLine,
                addressLine_lat: BasicInfo.value[index].addressLine_lat,
                addressLine_lng: BasicInfo.value[index].addressLine_lng,
                instruction: BasicInfo.value[index].instruction,
                startDate: BasicInfo.value[index].startDate,
                endDate: BasicInfo.value[index].endDate,
                checkEndDate: checkEndDate,
                state: BasicInfo.value[index].state,
                zipCode: BasicInfo.value[index].zipCode,
                note: BasicInfo.value[index].note,
                _default: BasicInfo.value[index]._default,
                countryCity: { country: BasicInfo.value[index].country, city: BasicInfo.value[index].city }
            })

            if ($scope._listForm[index].startDate != "" && $scope._listForm[index].startDate != undefined)
                $scope._listForm[index].startDate = new Date($scope._listForm[index].startDate);
            else
                $scope._listForm[index].startDate = new Date();

            //if ($scope._listForm[index].endDate != "" && $scope._listForm[index].endDate != undefined)
            //    $scope._listForm[index].endDate = new Date($scope._listForm[index].endDate);
            //else
            //    $scope._listForm[index].endDate = new Date(today.getTime() + (24 * 60 * 60 * 1000));
        });

        $scope._new =
            {
                'IsEdit': false,
                'privacy': "",
                'description': "",
                '_default': true,
                'startDate': new Date(),
                'endDate': '',
                'checkEndDate': false,
                'addressLine': "",
                'addressLine_lat': "",
                'addressLine_lng': "",
                'instruction': "",
                'countryCity': "",
                'state': "",
                'zipCode': "",
                'note': "",

            };


        $scope.addNewForm = function (newForm) {
            if (newForm.checkEndDate == false)
                newForm.endDate = "";
            $scope._listForm.push({
                'IsEdit': false,
                'privacy': newForm.privacy,
                'description': newForm.description,
                '_default': newForm._default,
                'startDate': newForm.startDate,
                'endDate': newForm.endDate,
                'checkEndDate': newForm.checkEndDate,
                'addressLine': newForm.addressLine,
                'addressLine_lat': newForm.addressLine_lat,
                'addressLine_lng': newForm.addressLine_lng,
                'instruction': newForm.instruction,
                'countryCity': newForm.countryCity,
                'state': newForm.state,
                'zipCode': newForm.zipCode,
                'note': newForm.note,
            });

        }

        $scope.removeForm = function (value) {

            //
            var index = $scope._listForm.indexOf(value);
            swal({
                title: "Are you sure to delete form " + value.description + " ?",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes",
                cancelButtonText: "No"
                //,
                //closeOnConfirm: false,
                //closeOnCancel: true
            }).then(function () {
                $scope._listForm.splice(index, 1);
                $scope.Save();
                swal(
                    'Deleted',
                    'Form ' + value.description + ' has been deleted.',
                    'success'
                  )
            }
            , function (dismiss) {
                if (dismiss === 'cancel') {

                }
            });
        };

        //
        $scope.messageBox = "";
        $scope.alerts = [];

    }
    $scope.$on('mailingAddress', function () {
        $scope.InitData();
    });
    $scope.InitData();
    $scope.Edit = function () {
        $scope.IsEdit = true;
    }

    // == edit == 
    $scope.Save = function () {

        //address
        BasicInfo.label = $scope._form._label;
        if ($scope._form._name == '') {
            $scope._form._name = $scope._form._label;
        }
        BasicInfo.name = $scope._form._name;
        BasicInfo.privacy = $scope._form._privacy;


        //
        var checkDefault = true;
        $($scope._listForm).each(function (index) {

            if (checkDefault == true && $scope._listForm[index]._default == true && $scope._listForm[index].description != $scope._form._default) {
                $scope._form._default = $scope._listForm[index].description;
                checkDefault = false;
            }


        });

        BasicInfo.default = $scope._form._default;

        //
        var lstFormSave = [];
        $($scope._listForm).each(function (index, object) {


            if (checkDefault == false && $scope._listForm[index].description != $scope._form._default) {
                $scope._listForm[index]._default = false;
            }
            // Check enddate
            var endDateValue = new Date($scope._listForm[index].endDate);
            var dateValue = new Date('1000-01-01');
            if (endDateValue < dateValue)
                $scope._listForm[index].endDate = "";
            if ($scope._listForm[index].checkEndDate == false)
                $scope._listForm[index].endDate = "";
            lstFormSave.push({
                privacy: $scope._listForm[index].privacy,
                description: $scope._listForm[index].description,
                _default: $scope._listForm[index]._default,
                startDate: $scope._listForm[index].startDate,
                endDate: $scope._listForm[index].endDate,
               
                addressLine: $scope._listForm[index].addressLine,
                addressLine_lat: $scope._listForm[index].addressLine_lat,
                addressLine_lng: $scope._listForm[index].addressLine_lng,
                instruction: $scope._listForm[index].instruction,
                country: $scope._listForm[index].countryCity.country,
                city: $scope._listForm[index].countryCity.city,
                state: $scope._listForm[index].state,
                zipCode: $scope._listForm[index].zipCode,
                note: $scope._listForm[index].note

            })


            $scope._listForm[index].IsEdit = false;
        });

        // $scope.checkDefault();


        // === 
        var addressName = VaultInformationService.VaultInformation.groupAddress.name;
        if (addressName == "")
            VaultInformationService.VaultInformation.groupAddress.name = "Address";
        //
        VaultInformationService.VaultInformation.groupAddress.value.mailingAddress = BasicInfo;
        VaultInformationService.VaultInformation.groupAddress.value.mailingAddress.value = lstFormSave;
        VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
            VaultInformationService.VaultInformation._id).then(function (response) {
                $scope.IsEdit = false;
                $scope._new = "";
                $rootScope.$broadcast('mailingAddress');
               
            }, function (errors) {

            });
    }

    $scope.saveVaultInformationOnSuccess = function (response) {
        $scope.IsEdit = false;
        $scope._new = "";
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

//3.5 Pobox
myApp.getController('PoboxController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter) {

    "use strict";
    var BasicInfo = VaultInformationService.VaultInformation.groupAddress.value.pobox;
    $scope.InitData = function () {
        $scope.IsEdit = false;

        $scope._form = {
            _label: BasicInfo.label,
            _name: BasicInfo.name,
            _default: BasicInfo.default,
            _privacy: BasicInfo.privacy
        };
        //
        $scope._listForm = [];

        $(BasicInfo.value).each(function (index, object) {

            $scope._listForm.push({
                IsEdit: false,
                privacy: BasicInfo.value[index].privacy,
                description: BasicInfo.value[index].description,
                addressLine: BasicInfo.value[index].addressLine,
                addressLine_lat: BasicInfo.value[index].addressLine_lat,
                addressLine_lng: BasicInfo.value[index].addressLine_lng,
                zipCode: BasicInfo.value[index].zipCode,
                instruction: BasicInfo.value[index].instruction,
                note: BasicInfo.value[index].note,
                _default: BasicInfo.value[index]._default

            })


        });

        $scope._new =
            {
                'IsEdit': false,
                'privacy': "",
                'description': "",
                '_default': true,
                'addressLine': "",
                'addressLine_lat': "",
                'addressLine_lng': "",
                'zipCode': "",
                'instruction': "",
                'note': "",

            };


        $scope.addNewForm = function (newForm) {

            $scope._listForm.push({
                'IsEdit': false,
                'privacy': newForm.privacy,
                'description': newForm.description,
                '_default': newForm._default,
                'instruction': newForm.instruction,
                'addressLine': newForm.addressLine,
                'addressLine_lat': newForm.addressLine_lat,
                'addressLine_lng': newForm.addressLine_lng,

                'zipCode' : newForm.zipCode,
                'note': newForm.note,
            });

        }

        $scope.removeForm = function (value) {

            //
            var index = $scope._listForm.indexOf(value);
            swal({
                title: "Are you sure to delete form " + value.description + " ?",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes",
                cancelButtonText: "No"
                //,
                //closeOnConfirm: false,
                //closeOnCancel: true
            }).then(function () {
                $scope._listForm.splice(index, 1);
                $scope.Save();
                swal(
                    'Deleted',
                    'Form ' + value.description + ' has been deleted.',
                    'success'
                  )
            }
            , function (dismiss) {
                if (dismiss === 'cancel') {

                }
            });


        };

        ////  

        //
        $scope.messageBox = "";
        $scope.alerts = [];

    }
    $scope.$on('pobox', function () {
        $scope.InitData();
    });
    $scope.InitData();
    $scope.Edit = function () {
        $scope.IsEdit = true;
    }

    // == edit == 
    $scope.Save = function () {

        //address
        BasicInfo.label = $scope._form._label;
        if ($scope._form._name == '') {
            $scope._form._name = $scope._form._label;
        }
        BasicInfo.name = $scope._form._name;
        BasicInfo.privacy = $scope._form._privacy;


        //
        var checkDefault = true;
        $($scope._listForm).each(function (index) {

            if (checkDefault == true && $scope._listForm[index]._default == true && $scope._listForm[index].description != $scope._form._default) {
                $scope._form._default = $scope._listForm[index].description;
                checkDefault = false;
            }


        });

        BasicInfo.default = $scope._form._default;

        //
        var lstFormSave = [];
        $($scope._listForm).each(function (index, object) {


            if (checkDefault == false && $scope._listForm[index].description != $scope._form._default) {
                $scope._listForm[index]._default = false;
            }


            lstFormSave.push({
                privacy: $scope._listForm[index].privacy,

                description: $scope._listForm[index].description,
                _default: $scope._listForm[index]._default,
                addressLine: $scope._listForm[index].addressLine,
                addressLine_lat: $scope._listForm[index].addressLine_lat,
                addressLine_lng: $scope._listForm[index].addressLine_lng,
                instruction: $scope._listForm[index].instruction,
                zipCode: $scope._listForm[index].zipCode,
                note: $scope._listForm[index].note

            })


            $scope._listForm[index].IsEdit = false;
        });

        // $scope.checkDefault();


        // === 
        var addressName = VaultInformationService.VaultInformation.groupAddress.name;
        if (addressName == "")
            VaultInformationService.VaultInformation.groupAddress.name = "Address";
        //
        VaultInformationService.VaultInformation.groupAddress.value.pobox = BasicInfo;
        VaultInformationService.VaultInformation.groupAddress.value.pobox.value = lstFormSave;
        VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
            VaultInformationService.VaultInformation._id).then(function (response) {
                $scope.IsEdit = false;
                $scope._new = "";
                $rootScope.$broadcast('pobox');
               
            }, function (errors) {

            });
    }

    $scope.saveVaultInformationOnSuccess = function (response) {
        $scope.IsEdit = false;
        $scope._new = "";
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

}]);

