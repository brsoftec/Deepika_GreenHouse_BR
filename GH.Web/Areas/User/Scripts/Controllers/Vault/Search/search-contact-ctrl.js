var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.cellNav'], true);



//3.0 ContactEmail
myApp.getController('SearchContactEmailController',
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

                    if (lstPermission[i].jsonpath == "contact") {
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
        $scope.listEmail = [];
        $scope._email = {
            'label': VaultForm.label,
            'name': VaultForm.name,
            'privacy': VaultForm.privacy,
            'default': VaultForm.default,
            'edit': false
        };

        // Email
        var indexEmail = 0;
        $(VaultForm.value).each(function (index, object) {
            indexEmail = indexEmail + 1;
            var _num = VaultForm.value[index].id;
            if (_num == '' || _num == undefined)
                VaultForm.value[index].id = indexEmail;
            else
                VaultForm.value[index].id = parseInt(_num);
            var _default = false;
            if (VaultForm.value[index].value == VaultForm.default)
                _default = true;
            $scope.listEmail.push({
                'id': VaultForm.value[index].id,
                'value': VaultForm.value[index].value,
                'default': _default
            })
        });

     
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
    }


    // == edit == 
    $scope.Save = function () {

        //Email
        var lstFormSave = [];
        var isDefault = false;
        for (var i = 0; i < $scope.listEmail.length; i++) {
            if ($scope.listEmail[i].default == true) {
                isDefault = true;
                $scope._email.default = $scope.listEmail[i].value;
            }
            lstFormSave.push({
                'id': $scope.listEmail[i].id,
                'value': $scope.listEmail[i].value
            });
        }

        //

        if (isDefault == false && lstFormSave.length > 0) {

            $scope._email.default = lstFormSave[0].value;
        }

       
        //
        VaultForm.default = $scope._email.default;

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

//3.0 ContactPhone
myApp.getController('SearchContactPhoneController',
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

                    if (lstPermission[i].jsonpath == "contact") {
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
        $scope.mobile = {
            'id': VaultForm.id,
            'label': VaultForm.label,
            'name': VaultForm.name,
            'privacy': VaultForm.privacy,
            'type': VaultForm.type,
            'rules': VaultForm.rules,
            'nosearch': VaultForm.nosearch,
            'default': VaultForm.default,
            'edit': false
        };

        $scope.listMobile = [];

        //    //End star value
        var phoneId = 0;
        $(VaultForm.value).each(function (index, object) {
            phoneId = phoneId + 1;
            var _default = false;
            if (VaultForm.value[index].value == VaultForm.default)
                _default = true;

            var temp = VaultForm.value[index].id;
            if (temp == '')
                VaultForm.value[index].id = phoneId;
            else
                VaultForm.value[index].id = parseInt(temp);

            VaultForm.value[index].id = temp;
            var tempPhone = VaultForm.value[index].value;
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
    }


    // == edit == 
    $scope.Save = function () {

        var lstFormSave = [];
        var isDefault = false;

        for (var i = 0; i < $scope.listMobile.length; i++) {
            var tempP = '+' + $scope.listMobile[i].codeCountry + $scope.listMobile[i].phoneNumber;
            if ($scope.listMobile[i].default == true) {
                isDefault = true;
                $scope.mobile.default = tempP;
            }
            lstFormSave.push({
                'id': $scope.listMobile[i].id,
                'value': tempP
            });
        }
        if (isDefault == false && lstFormSave.length > 0) {

            $scope.mobile.default = lstFormSave[0].value;
        }

        //
        VaultForm.default = $scope.mobile.default;

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

