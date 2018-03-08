
// NationalID
myApp.getController('searchGovernmentIdIDController',
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
                    if (lstPermission[i].jsonpath == "governmentID") {
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
       
        $scope.isEdit = false;
        $scope._form = {
            _label: vaultForm.label,
            _name: vaultForm.name,
            _default: vaultForm._default,
            _privacy: vaultForm.privacy
        };

        $scope.listForm = [];
        $(vaultForm.value).each(function (index, object) {
            var indefiniteDate = true;
            if (vaultForm.value[index].expiryDate == undefined)
                vaultForm.value[index].expiryDate = "";
            if (vaultForm.value[index].expiryDate != "")
                indefiniteDate = false;
            $scope.listForm.push({
                isEdit: false,
                _id: vaultForm.value[index]._id,
                privacy: vaultForm.value[index].privacy,
                description: vaultForm.value[index].description,
                firstName: vaultForm.value[index].firstName,
                lastName: vaultForm.value[index].lastName,
                cardType: vaultForm.value[index].cardType,
                cardNumber: vaultForm.value[index].cardNumber,
                middleName: vaultForm.value[index].middleName,
                certificateNumber: vaultForm.value[index].certificateNumber,
                nationality: vaultForm.value[index].nationality,
                nationalID: vaultForm.value[index].nationalID,
                race: vaultForm.value[index].race,
                tier: vaultForm.value[index].tier,
                country: { country: vaultForm.value[index].country, city: vaultForm.value[index].city },
                address: vaultForm.value[index].address,
                issuedBy: vaultForm.value[index].issuedBy,
                issuedIn: vaultForm.value[index].issuedIn,
                issuedDate: vaultForm.value[index].issuedDate,
                expiryDate: vaultForm.value[index].expiryDate,
                classTier: vaultForm.value[index].classTier,
                bloodType: vaultForm.value[index].bloodType,
                indefiniteDate: indefiniteDate,
              
                note: vaultForm.value[index].note,
                _default: vaultForm.value[index]._default,
            })

        });
    }
    //
   
   
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
            lstFormSave.push({
                _id: $scope.listForm[index]._id,
                privacy: $scope.listForm[index].privacy,
                description: $scope.listForm[index].description,
                firstName: $scope.listForm[index].firstName,
                lastName: $scope.listForm[index].lastName,
                middleName: $scope.listForm[index].middleName,
                cardType: $scope.listForm[index].cardType,
                cardNumber: $scope.listForm[index].cardNumber,
                classTier: $scope.listForm[index].classTier,
                certificateNumber: $scope.listForm[index].certificateNumber,
                nationality: $scope.listForm[index].nationality,
                nationalID: $scope.listForm[index].nationalID,
                race: $scope.listForm[index].race,
                country: $scope.listForm[index].country.country,
                city: $scope.listForm[index].country.city,
                address: $scope.listForm[index].address,
                issuedDate: $scope.listForm[index].issuedDate,
                expiryDate: $scope.listForm[index].expiryDate,
                bloodType: $scope.listForm[index].bloodType,
                note: $scope.listForm[index].note,
                _default: $scope.listForm[index]._default,
            })
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
