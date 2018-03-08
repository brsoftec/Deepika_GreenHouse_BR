
var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.cellNav'], true);


//3.0 Address
myApp.getController('SearchAddressController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter', 'VaultService',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter, _vaultService) {
    
    var vaultData = null;
    $scope.formData = null;
    var VaultForm = null;
    $scope.initSearchVault = function () {

        // Init all Vault
        //var vm = new Object();
        //vm.UserId = $scope.delegateSearch.FromAccountId;
        //_vaultService.GetVault(vm).then(function (rs) {
        //    vaultData = rs.VaultInformation;
        //    VaultForm = vaultData.groupAddress.value.billingAddress;
        //    $scope.initDataForm();
        //    //  $scope.vaultInit();
        //}, function (errors) {
        //    swal('Error', errors, 'error');
        //})

        // End Init all Vault

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

                    if (lstPermission[i].jsonpath == "address") {
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

        $scope.IsEdit = false;
        $scope._form = {
            _label: VaultForm.label,
            _name: VaultForm.name,
            _default: VaultForm.default,
            _privacy: VaultForm.privacy
        };
        $scope._listForm = [];

        $(VaultForm.value).each(function (index, object) {
            var checkEndDate = false;
            if (VaultForm.value[index].endDate == undefined)
                VaultForm.value[index].endDate = "";
            if (VaultForm.value[index].endDate != "")
                checkEndDate = true;

            $scope._listForm.push({
                IsEdit: false,
                privacy: VaultForm.value[index].privacy,
                description: VaultForm.value[index].description,
                addressLine: VaultForm.value[index].addressLine,
                addressLine_lat: VaultForm.value[index].addressLine_lat,
                addressLine_lng: VaultForm.value[index].addressLine_lng,
                instruction: VaultForm.value[index].instruction,
                startDate: VaultForm.value[index].startDate,
                endDate: VaultForm.value[index].endDate,
                checkEndDate: checkEndDate,
                state: VaultForm.value[index].state,
                zipCode: VaultForm.value[index].zipCode,
                note: VaultForm.value[index].note,
                _default: VaultForm.value[index]._default,
                countryCity: { country: VaultForm.value[index].country, city: VaultForm.value[index].city }
            })

            if ($scope._listForm[index].startDate != "" && $scope._listForm[index].startDate != undefined)
                $scope._listForm[index].startDate = new Date($scope._listForm[index].startDate);
            else
                $scope._listForm[index].startDate = new Date();
        });
    }
  

    // == edit == 
    $scope.Save = function () {

        //address
        VaultForm.label = $scope._form._label;
        if ($scope._form._name == '') {
            $scope._form._name = $scope._form._label;
        }
        VaultForm.name = $scope._form._name;
        VaultForm.privacy = $scope._form._privacy;

        var checkDefault = true;
        $($scope._listForm).each(function (index) {

            if (checkDefault == true && $scope._listForm[index]._default == true && $scope._listForm[index].description != $scope._form._default) {
                $scope._form._default = $scope._listForm[index].description;
                checkDefault = false;
            }
        });

        VaultForm.default = $scope._form._default;

        var lstFormSave = [];
        $($scope._listForm).each(function (index, object) {

            if (checkDefault == false && $scope._listForm[index].description != $scope._form._default) {
                $scope._listForm[index]._default = false;
            }

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

        // Save All Vault
        //vaultData.groupAddress.value.billingAddress = VaultForm;
        //vaultData.groupAddress.value.billingAddress.value = lstFormSave;
        //var saveVault = new Object();
        //saveVault.vaultString = vaultData;
        //saveVault.userId = vaultData._id;

        //_vaultService.UpdateVault(saveVault).then(function () {
        //    $scope.IsEdit = false;

        //}, function (errors) {
        //    swal('Error', errors, 'error');
        //})
        // End Save All Vault

        // Save Form Vault
        VaultForm.value = lstFormSave;
        vaultData.value[$scope.query.formName] = VaultForm;

        var saveVaultForm = {
            'AccountId': $scope.delegateSearch.FromAccountId,
            'FormName': $scope.query.groupName,
            'FormString': vaultData
        }

        _vaultService.UpdateFormVault(saveVaultForm).then(function () {
            $scope.IsEdit = false;

            $scope.UpdateDataSearch();

        }, function (errors) {
            swal('Error', errors, 'error');
        })

        // End Save Form Vault      
    }

    $scope.Cancel = function () {
        $scope.initSearchVault();
    }

}])

//3.1 Pobox 
myApp.getController('SearchAddressPoboxController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter', 'VaultService',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter, _vaultService) {
    var vaultData = null;
    $scope.formData = null;
    var VaultForm = null;
    $scope.initSearchVault = function () {

        // Init all Vault
        //var vm = new Object();
        //vm.UserId = $scope.delegateSearch.FromAccountId;
        //_vaultService.GetVault(vm).then(function (rs) {
        //    vaultData = rs.VaultInformation;
        //    VaultForm = vaultData.groupAddress.value.billingAddress;
        //    $scope.initDataForm();
        //    //  $scope.vaultInit();
        //}, function (errors) {
        //    swal('Error', errors, 'error');
        //})

        // End Init all Vault

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
                    if (lstPermission[i].jsonpath == "address") {
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

        $scope.IsEdit = false;
        $scope._form = {
            _label: VaultForm.label,
            _name: VaultForm.name,
            _default: VaultForm.default,
            _privacy: VaultForm.privacy
        };
        $scope._listForm = [];

     
        $(VaultForm.value).each(function (index, object) {
         
            $scope._listForm.push({
                IsEdit: false,
                privacy: VaultForm.value[index].privacy,
                description: VaultForm.value[index].description,
                addressLine: VaultForm.value[index].addressLine,
                addressLine_lat: VaultForm.value[index].addressLine_lat,
                addressLine_lng: VaultForm.value[index].addressLine_lng,
                instruction: VaultForm.value[index].instruction,            
                zipCode: VaultForm.value[index].zipCode,
                note: VaultForm.value[index].note,
                _default: VaultForm.value[index]._default
               
            })

            if ($scope._listForm[index].startDate != "" && $scope._listForm[index].startDate != undefined)
                $scope._listForm[index].startDate = new Date($scope._listForm[index].startDate);
            else
                $scope._listForm[index].startDate = new Date();

        });
    }
  
    // == edit == 
    $scope.Save = function () {

        //address
        VaultForm.label = $scope._form._label;
        if ($scope._form._name == '') {
            $scope._form._name = $scope._form._label;
        }
        VaultForm.name = $scope._form._name;
        VaultForm.privacy = $scope._form._privacy;

        var checkDefault = true;
        $($scope._listForm).each(function (index) {

            if (checkDefault == true && $scope._listForm[index]._default == true && $scope._listForm[index].description != $scope._form._default) {
                $scope._form._default = $scope._listForm[index].description;
                checkDefault = false;
            }
        });

        VaultForm.default = $scope._form._default;

        var lstFormSave = [];
        $($scope._listForm).each(function (index, object) {

            if (checkDefault == false && $scope._listForm[index].description != $scope._form._default) {
                $scope._listForm[index]._default = false;
            }

            if ($scope._listForm[index].checkEndDate == false)
                $scope._listForm[index].endDate = "";
           
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

        // Save Form Vault
        VaultForm.value = lstFormSave;
        vaultData.value[$scope.query.formName] = VaultForm;

        var saveVaultForm = {
            'AccountId': $scope.delegateSearch.FromAccountId,
            'FormName': $scope.query.groupName,
            'FormString': vaultData
        }

        _vaultService.UpdateFormVault(saveVaultForm).then(function () {
            $scope.IsEdit = false;
            $scope.UpdateDataSearch();

        }, function (errors) {
            swal('Error', errors, 'error');
        })

        // End Save Form Vault      
    }

    $scope.Cancel = function () {
        $scope.initSearchVault();
    }

}])