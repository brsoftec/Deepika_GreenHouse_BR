
myApp.getController('searchFinancialController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter', 'VaultService', 'rguModal', '$window', 'DocumentVaultService',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter, _vaultService, rguModal, $window, DocumentVaultService ) {
    var vaultData = null;  
    var vaultForm = null;

    $scope.initSearchVault = function () {
        var formVault = {
            'FormName': $scope.query.groupName,
            'AccountId': $scope.delegateSearch.FromAccountId
        }
        _vaultService.GetFormVault(formVault).then(function (response) {
            vaultData = response;
            vaultForm = response.value[$scope.query.formName];
            $scope.initDataForm();
        });
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
                    if (lstPermission[i].jsonpath == "basicInformation" || lstPermission[i].jsonpath == "contact" || lstPermission[i].jsonpath == "address" 
                    || lstPermission[i].jsonpath == "governmentID" || lstPermission[i].jsonpath == "education" || lstPermission[i].jsonpath == "employment"
                         || lstPermission[i].jsonpath == "family" || lstPermission[i].jsonpath == "membership" || lstPermission[i].jsonpath == "financial"
                        || lstPermission[i].jsonpath == "others") {
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

        $scope.isEdit = false;
        $scope._form = {
            _label: vaultForm.label,
            _name: vaultForm.name,
            _default: vaultForm._default,
            _privacy: vaultForm.privacy
        };

        $scope.listForm = [];
        $(vaultForm.value).each(function (index, object)
        {
            var formTemp = vaultForm.value[index];
            formTemp.isEdit = false;
            if (formTemp.country)
            {
                formTemp.location = { 'country': formTemp.country, 'city': formTemp.city }
            }
            $scope.listForm.push(formTemp);
        });
    }
   
    // == edit == 
    $scope.Save = function () {
        vaultForm.label = $scope._form._label;
        if ($scope._form._name == '') {
            $scope._form._name = $scope._form._label;
        }
        vaultForm.name = $scope._form._name;
        vaultForm.privacy = $scope._form._privacy;
        var check_default = true;
        $($scope.listForm).each(function (index) {

            if (check_default == true && $scope.listForm[index]._default == true && $scope.listForm[index].description != $scope._form._default) {
                $scope._form._default = $scope.listForm[index].description;
                check_default = false;
            }
        });
        vaultForm._default = $scope._form._default;

        var lstFormSave = [];
        $($scope.listForm).each(function (index, object) {

            if (check_default == false && $scope.listForm[index].description != $scope._form._default) {
                $scope.listForm[index]._default = false;
            }
            if ($scope.listForm[index].indefiniteDate)
                $scope.listForm[index].expiryDate = '';
            var formTemp = angular.copy($scope.listForm[index]);
            if (formTemp.location)
            {
                formTemp.city = formTemp.location.city;
                formTemp.country = formTemp.location.country;
            }
            lstFormSave.push(formTemp)
            $scope.listForm[index].isEdit = false;
        });
      
        //
        vaultForm.value = lstFormSave;
        vaultData.value[$scope.query.formName] = vaultForm;
        var savevaultForm = {
            'AccountId': $scope.delegateSearch.FromAccountId,
            'FormName': $scope.query.groupName,
            'FormString': vaultData
        }

        _vaultService.UpdateFormVault(savevaultForm).then(function () {
            $scope.isEdit = false;
            $scope.UpdateDataSearch();
        }, function (errors) {
            swal('Error', errors, 'error');
        })
    }

    $scope.Cancel = function () {
        $scope.initSearchVault();
    }

}])
