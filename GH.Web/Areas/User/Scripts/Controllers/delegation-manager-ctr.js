﻿
var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule'], true);

myApp.getController('DelegationController', ['$scope', '$rootScope', '$timeout', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter', 'VaultService',
    function ($scope, $rootScope, $timeout, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter, _vaultService) {
        var selectVault = null;
        $scope.query =
            {
                groupName: '',
                formName: '',
                value: '',
                pathForm: '',
                mySelf: false
            };
        var isAuthencation = false;
        $scope.checkAuthencation = function () {
            $http.post('/Api/Account/IsCheckPinVault', null)
                .success(function (isauthencation) {
                    isAuthencation = isauthencation;
                    if (isAuthencation == false) {
                        window.location.href = "/User/SMSAuthencation?TypeRedirect=Delegate";
                    }
                });
        };

        $scope.checkAuthencation();
        $scope.editForm = {value: false};
        $scope.editingForm = {value: null};
        $scope.statusDate = false;
        $scope.vaultTree = null;
        $scope.details = {value: false};
        $scope.view = {
            selectedDelegation: null
        };

        $scope.HideDetails = function () {
            $scope.isDetails = false;
        }
        $scope.ShowDetails = function () {
            $scope.isDetails = true;
        }
        $scope.activateEmergencyDelegator = function () {
            var DelegationModelView = new Object();
            DelegationModelView.DelegationId = $scope.view.selectedDelegation.DelegationId;
            $http.post('/api/DelegationManager/ActivatedDelegation', DelegationModelView)
                .success(function (response) {
                    $scope.view.selectedDelegation.Status = 'Activated';
                    $scope.selectDelegation($scope.view.selectedDelegation);
                }).error(function (errors, status) {
            });
        };

        $scope.$on('delegation:emergency-activated', function () {
            $scope.view.selectedDelegation.Status = 'Activated';
            $scope.selectDelegation($scope.view.selectedDelegation);
        });

        $scope.selectDelegation = function (del) {
            $scope.view.selectedDelegation = del;
            selectVault = del;
            $scope.statusDate = false;
            $scope.basic = null;
            $scope.contact = null;
            $scope.address = null;
            $scope.passport = null;
            $scope.health = null;
            $scope.blood = null;
            $scope.allergies = null;
            var date = new Date();
            var effectiveDate = new Date($scope.view.selectedDelegation.EffectiveDate);
            var expiredDate = date;
            if ($scope.view.selectedDelegation.ExpiredDate !== 'Indefinite')
                expiredDate = new Date($scope.view.selectedDelegation.ExpiredDate);
            //
            if (date >= effectiveDate && date <= expiredDate)
                $scope.statusDate = true;

            $scope.permission = $scope.view.selectedDelegation.GroupVaultsPermission;
            if (true) {
                var vm = new Object();
                vm.UserId = del.FromAccountId;
                _vaultService.GetVault(vm).then(function (rs) {
                    $scope.vaultInit(rs.VaultInformation);
                }, function (errors) {
                    swal('Error', errors, 'error');
                });
            }
            //Show Emergency
            if ($scope.view.selectedDelegation.Status == 'Activated') {
                var vm = new Object();
                vm.UserId = del.FromAccountId;
                _vaultService.GetVault(vm).then(function (rs) {
                    $scope.vaultE = rs.VaultInformation;
                    $scope.vaultInit(rs.VaultInformation);

                    //search
                    for (var i = 0; i < $scope.permission.length; i++) {
                        if ($scope.permission[i].name == 'Basic Information' && $scope.permission[i].read == true) {
                            $scope.basic = $scope.vaultE.basicInformation;
                        }
                        //Contact
                        if ($scope.permission[i].name == 'Contact Information' && $scope.permission[i].read == true) {
                            $scope.contact = {
                                'mobile': '',
                                'home': '',
                                'office': '',
                                'fax': '',
                                'email': ''

                            }

                            if ($scope.vaultE.contact.value.mobile.default != '' || $scope.vaultE.contact.value.mobile.default != '_')
                                $scope.contact.mobile = $scope.vaultE.contact.value.mobile.default;

                            if ($scope.vaultE.contact.value.home.default != '' || $scope.vaultE.contact.value.home.default != '_')
                                $scope.contact.home = $scope.vaultE.contact.value.home.default;
                            if ($scope.vaultE.contact.value.office.default != '' || $scope.vaultE.contact.value.office.default != '_')
                                $scope.contact.office = $scope.vaultE.contact.value.office.default;
                            if ($scope.vaultE.contact.value.fax.default != '' || $scope.vaultE.contact.value.fax.default != '_')
                                $scope.contact.fax = $scope.vaultE.contact.value.fax.default;
                            if ($scope.vaultE.contact.value.email.default != '' || $scope.vaultE.contact.value.email.default != '_')
                                $scope.contact.email = $scope.vaultE.contact.value.email.default;

                        }

                        //current address
                        if ($scope.permission[i].name == 'Current Address' && $scope.permission[i].read == true) {

                            for (var m = 0; m < $scope.vaultE.groupAddress.value.currentAddress.value.length; m++) {

                                if ($scope.vaultE.groupAddress.value.currentAddress.value[m].description == $scope.vaultE.groupAddress.value.currentAddress.default) {
                                    $scope.address = {
                                        'description': '',
                                        'addressLine': '',
                                        'startDate': '',
                                        'endDate': '',
                                        'country': '',
                                        'city': '',
                                        'instruction': ''
                                    }
                                    $scope.address.description = $scope.vaultE.groupAddress.value.currentAddress.value[m].description;
                                    $scope.address.addressLine = $scope.vaultE.groupAddress.value.currentAddress.value[m].addressLine;
                                    $scope.address.startDate = $scope.vaultE.groupAddress.value.currentAddress.value[m].startDate;
                                    $scope.address.endDate = $scope.vaultE.groupAddress.value.currentAddress.value[m].endDate;
                                    $scope.address.country = $scope.vaultE.groupAddress.value.currentAddress.value[m].country;
                                    $scope.address.city = $scope.vaultE.groupAddress.value.currentAddress.value[m].city;
                                    $scope.address.instruction = $scope.vaultE.groupAddress.value.currentAddress.value[m].instruction;
                                }
                            }
                        }

                        // passport
                        if ($scope.permission[i].name == 'Passport' && $scope.permission[i].read == true) {

                            for (var m = 0; m < $scope.vaultE.groupGovernmentID.value.passportID.value.length; m++) {

                                if ($scope.vaultE.groupGovernmentID.value.passportID.value[m].description == $scope.vaultE.groupGovernmentID.value.passportID.default) {
                                    $scope.passport = {
                                        'description': '',
                                        'firstName': '',
                                        'middleName': '',
                                        'lastName': '',
                                        'nationality': '',
                                        'cardNumber': '',
                                        'issuedDate': '',
                                        'expiryDate': '',
                                        'issuedBy': '',
                                        'issuedIn': ''
                                    }
                                    $scope.passport.description = $scope.vaultE.groupGovernmentID.value.passportID.value[m].description;
                                    $scope.passport.firstName = $scope.vaultE.groupGovernmentID.value.passportID.value[m].firstName;
                                    $scope.passport.middleName = $scope.vaultE.groupGovernmentID.value.passportID.value[m].middleName;
                                    $scope.passport.lastName = $scope.vaultE.groupGovernmentID.value.passportID.value[m].lastName;
                                    $scope.passport.nationality = $scope.vaultE.groupGovernmentID.value.passportID.value[m].nationality;
                                    $scope.passport.cardNumber = $scope.vaultE.groupGovernmentID.value.passportID.value[m].cardNumber;
                                    $scope.passport.issuedDate = $scope.vaultE.groupGovernmentID.value.passportID.value[m].issuedDate;
                                    $scope.passport.expiryDate = $scope.vaultE.groupGovernmentID.value.passportID.value[m].expiryDate;
                                    $scope.passport.issuedBy = $scope.vaultE.groupGovernmentID.value.passportID.value[m].issuedBy;
                                    $scope.passport.issuedIn = $scope.vaultE.groupGovernmentID.value.passportID.value[m].issuedIn;
                                }
                            }
                        }
                        // health
                        if ($scope.permission[i].name == 'Health Card' && $scope.permission[i].read == true) {
                            for (var m = 0; m < $scope.vaultE.groupGovernmentID.value.healthCard.value.length; m++) {
                                if ($scope.vaultE.groupGovernmentID.value.healthCard.value[m].description == $scope.vaultE.groupGovernmentID.value.healthCard.default) {
                                    $scope.health = {
                                        'description': '',
                                        'firstName': '',
                                        'middleName': '',
                                        'lastName': '',
                                        'cardNumber': '',
                                        'bloodType': '',
                                        'issuedDate': '',
                                        'expiryDate': ''
                                    }
                                    $scope.health.description = $scope.vaultE.groupGovernmentID.value.healthCard.value[m].description;
                                    $scope.health.firstName = $scope.vaultE.groupGovernmentID.value.healthCard.value[m].firstName;
                                    $scope.health.middleName = $scope.vaultE.groupGovernmentID.value.healthCard.value[m].middleName;
                                    $scope.health.lastName = $scope.vaultE.groupGovernmentID.value.healthCard.value[m].lastName;
                                    $scope.health.cardNumber = $scope.vaultE.groupGovernmentID.value.healthCard.value[m].cardNumber;
                                    $scope.health.bloodType = $scope.vaultE.groupGovernmentID.value.healthCard.value[m].bloodType;
                                    $scope.health.issuedDate = $scope.vaultE.groupGovernmentID.value.healthCard.value[m].issuedDate;
                                    $scope.health.expiryDate = $scope.vaultE.groupGovernmentID.value.healthCard.value[m].expiryDate;
                                }
                            }
                        }

                        // bloodType
                        if ($scope.permission[i].name == 'Blood Type' && $scope.permission[i].read == true) {
                            for (var m = 0; m < $scope.vaultE.groupGovernmentID.value.healthCard.value.length; m++) {
                                if ($scope.vaultE.groupGovernmentID.value.healthCard.value[m].description == $scope.vaultE.groupGovernmentID.value.healthCard.default) {
                                    $scope.blood = '';
                                    $scope.blood = $scope.vaultE.groupGovernmentID.value.healthCard.value[m].bloodType;
                                }
                            }
                        }

                        //Allergies
                        if ($scope.permission[i].name == 'Allergies' && $scope.permission[i].read == true) {
                            if ($scope.vaultE.others.value.preference.value.hasOwnProperty('allergies'))
                                $scope.allergies = $scope.vaultE.others.value.preference.value.allergies.value;
                        }
                    }
                    var emergencyVault = {
                        basic: $scope.basic,
                        contact: $scope.contact,
                        address: $scope.address,
                        passport: $scope.passport,
                        health: $scope.health,
                        blood: $scope.blood,
                        allergies: $scope.allergies
                    };
                    $scope.ev = emergencyVault;
                    $rootScope.$broadcast('delegation:emergency-vault',emergencyVault);

                }, function (errors) {
                    console.log('Error getting vault', errors);
                });
            }
            //End Emergency
            function toLabel(name) {
                var result = name.replace(/([A-Z])/g, " $1");
                return result.charAt(0).toUpperCase() + result.slice(1);
            }

//Son
            $scope.vaultInit = function (vault) {
                $scope.vaultTree = vault;
                var permissions = [];
                var delegationRole = $scope.view.selectedDelegation.DelegationRole;
                var delegationGranular = delegationRole === 'Custom' || delegationRole === 'Emergency';
                if (delegationGranular) {
                    permissions = $scope.view.selectedDelegation.GroupVaultsPermission;
                }
                var acl = {};
                if (delegationRole === 'Emergency') {
                    var emergencyPermissions = [
                        {
                            name: 'Basic Information',
                            read: false,
                            jsonpaths: ['basicInformation.firstName', 'basicInformation.lastName', 'basicInformation.dob']
                        },
                        {
                            name: 'Contact Information',
                            read: false,
                            jsonpaths: ['contact']
                        },
                        {
                            name: 'Current Address',
                            read: false,
                            jsonpaths: ['groupAddress.currentAddress']
                        },
                        {
                            name: 'Passport',
                            read: false,
                            jsonpaths: ['groupGovernmentID.passportID']
                        },
                        {
                            name: 'Health Card',
                            read: false,
                            jsonpaths: ['groupGovernmentID.healthCard']
                        },
                        {
                            name: 'Blood Type',
                            read: false,
                            jsonpaths: ['groupGovernmentID.healthCard.bloodType']
                        },
                        {
                            name: 'Allergies',
                            read: false,
                            jsonpaths: ['others.preferences.allergies']
                        }
                    ];
                }

                angular.forEach(permissions, function (permission) {
                    if (delegationRole === 'Custom') {
                        var jsPath = permission.jsonpath;
                        switch (jsPath) {
                            case 'governmentID':
                                jsPath = 'groupGovernmentID';
                                break;
                            case 'address':
                                jsPath = 'groupAddress';
                                break;
                            case 'financial':
                                jsPath = 'groupFinancial';
                                break;
                        }

                        var aclItem = {
                            name: permission.name,
                            read: permission.read || permission.write,
                            write: permission.write
                        };
                        var pathNames = permission.jsonpath.split('.');
                        if (pathNames.length === 1) {
                            aclItem.type = 'bucket';
                        } else {
                            aclItem.type = 'field';
                        }
                        acl[jsPath] = aclItem;
                    } else {

                        angular.forEach(emergencyPermissions, function (ep) {
                            if (ep.name === permission.name) {
                                angular.forEach(ep.jsonpaths, function (jsPath) {
                                    var aclItem = {
                                        name: permission.name,
                                        read: permission.read,
                                        write: false,
                                        jsonpath: '.' + jsPath
                                    };
                                    var pathNames = jsPath.split('.');
                                    if (pathNames.length === 1) {
                                        aclItem.type = 'bucket';
                                        acl[jsPath] = aclItem;
                                    } else {
                                        aclItem.type = 'field';
                                        acl['.' + jsPath] = aclItem;
                                    }

                                });
                            }
                        });

                    }
                });
                var entries = [];
                var defaultPermission = delegationRole === 'Super' ? {read: true, write: true} : {
                    read: false,
                    write: false
                };
                traverseVault($scope.vaultTree, 1, '', '', defaultPermission);
                $scope.vaultEntries = entries;
                function traverseVault(node, level, path, jsPath, permission) {
                    if (!angular.isObject(node)) return;
                    angular.forEach(node, function (entry, name) {
                        if (!angular.isObject(entry))
                            return;

                        var label = entry.label;
                        var list = angular.isUndefined(label);
                        var sublist = entry.hasOwnProperty('sublist') && entry.sublist || name === 'contact' && level === 1;
                        list = list || sublist;
                        var ownPermission = false;
                        if (delegationRole !== 'Super') {
                            if (level === 1) {
                                if (acl.hasOwnProperty(name)) {
                                    permission.read = acl[name].read;
                                    permission.write = acl[name].write;
                                    ownPermission = true;
                                }
                            } else {
                                var fullJsPath = jsPath + '.' + name;
                                if (acl.hasOwnProperty(fullJsPath)) {
                                    permission.read = acl[fullJsPath].read;
                                    permission.write = acl[fullJsPath].write;
                                    ownPermission = true;
                                }
                            }
                        }

                        if (!list && (!angular.isObject(entry.value) || entry.hasOwnProperty('type')
                            || entry.hasOwnProperty('label') && entry.hasOwnProperty('value') && level > 2)) {
                            var vaultEntry = {
                                id: entry._id,
                                label: label,
                                description: entry.description,
                                options: entry.options,
                                leaf: true,
                                value: entry.value,
                                type: entry.type,
                                rules: entry.rules,
                                level: level,
                                path: path,
                                jsPath: jsPath + '.' + name,
                                permission: angular.copy(permission)
                            };
                            entries.push(vaultEntry);
                        }
                        else {

                            if (list) {
                                label = sublist ? entry.label : entry.description;
                            }
                            vaultEntry = {
                                id: entry._id,
                                label: label,
                                leaf: false,
                                level: level,
                                path: path,
                                jsPath: jsPath + '.' + name,
                                permission: angular.copy(permission)
                            };
                            if (list) {
                                vaultEntry.list = true;
                            }
                            if (ownPermission) {
                                vaultEntry.ownPermission = true;
                            }
                            entries.push(vaultEntry);
                            if (list) {
                                var fields = [];
                                angular.forEach(entry, function (list, listName) {

                                    if (sublist) {
                                        if (!angular.isObject(list) || list.nosearch) {
                                            return;
                                        }
                                        angular.forEach(list, function (sublist, sublistName) {
                                            if (sublist.nosearch) return;
                                            angular.forEach(sublist.value, function (item, key) {
                                                var field = {
                                                    value: item.value,
                                                    leaf: true,
                                                    label: sublist.label + ' ' + item.id,
                                                    level: level + 1,
                                                    path: path + '/' + entry.label + '/' + sublist.label,
                                                    jsPath: jsPath + '.' + name + '.' + sublistName + '.' + item.id,
                                                    permission: angular.copy(permission)
                                                };
                                                fields.push(field);
                                                entries.push(field);
                                            });
                                        });
                                    } else if (listName !== '_default' && listName !== 'description' && listName !== 'privacy') {
                                        var label = list.label || toLabel(listName);
                                        var field = {
                                            value: list,
                                            leaf: true,
                                            label: label,
                                            level: level + 1,
                                            path: path + '/' + label,
                                            jsPath: jsPath + '.' + name + '.' + listName,
                                            permission: angular.copy(permission)
                                        };
                                        fields.push(field);
                                        entries.push(field);
                                    }

                                });

                                vaultEntry.children = fields;
                            }

                            if (!list) {
                                traverseVault(entry.value, level + 1, path + '/' + entry.label, jsPath + '.' + name, angular.copy(permission));
                            }
                        }
                    });

                }
            };

            $scope.vaultSearch = {
                query: '',
                searching: false
            };
            //
            $scope.vaultEdit = {
                editingField: false
            };
            $scope.onVaultSearchInput = function () {
                $scope.vaultSearch.searching = !!$scope.vaultSearch.query.length;
            };
            $scope.clearSearch = function () {
                $scope.vaultSearch.query = '';
                $scope.vaultSearch.searching = false;
            };

            function deepTest(value, re) {
                if (!angular.isDefined(value))
                    return false;
                if (angular.isString(value))
                    return re.test(value);
                if (angular.isArray(value)) {
                    var matched = false;
                    angular.forEach(value, function (item) {
                        matched = matched || deepTest(item, re);
                    });
                    return matched;
                }
                return re.test(value.toString());
            }

            $scope.filterEntriesByQuery = function (entry) {
                if (!$scope.vaultSearch.query.length) return false;

                if (!entry.permission.read) return false;
                var re = new RegExp($scope.vaultSearch.query, 'i');
                var matched = re.test(entry.label);
                if (entry.leaf && entry.value) {
                    matched = matched || deepTest(entry.value, re);
                }
                if (entry.leaf || entry.level === 1 || !angular.isObject(entry.children))
                    return matched;
                angular.forEach(entry.children, function (entry) {
                    matched = matched || re.test(entry.label) || entry.leaf && deepTest(entry.value, re);
                });
                return matched;
            };

            $scope.editField = function (field) {
                $scope.vaultEdit.editingField = field;
                $scope.vaultEdit.editModel = field.value;
            };

            // $scope.view.selectedDelegation 
            $scope.cancelField = function (field) {
                $scope.vaultEdit.editingField = false;
                $scope.query = null;
            };

            $scope.saveField = function (field) {
                $scope.vaultEdit.editingField = false;

                var data = new Object();

                data.UserId = del.FromAccountId;
                data.InfoField = {
                    Id: field.id,
                    jsPath: field.jsPath,
                    Type: field.type,
                    Value: $scope.vaultEdit.editModel
                }

                $http.post('/api/InformationVaultService/UpdateInfoFieldById', data)
                    .success(function (response) {
                        $scope.vaultEdit.editingField = false;
                        field.value = $scope.vaultEdit.editModel;
                        $rootScope.$broadcast('basicInformation');
                    })
                    .error(function (errors, status) {

                    });

            }

            $scope.editGroup = function (group) {
                if ($scope.editingMode) return;
                $scope.editingMode = true;
                if (angular.isDefined(group)) {
                    $scope.editingModel = {};
                    angular.forEach(group.fields, function (field) {
                        if (field.divider) return;
                        $scope.editingModel[field.name] = angular.extend({}, field);
                    });
                }
            };
            $scope.editContact = function () {
                if ($scope.editingModeContact) return;
                $scope.editingModeContact = true;
            };

            $scope.renderFieldValue = function (field) {
                var value = field.value, type = field.type;
                if (!type) return value;

                if (type === 'date') {
                    return $moment(value).format('DD MMM YYYY');
                }
                if (type === 'list') {
                    return value.join(', ');
                }
                return value;
            };

            $scope.cancelEditing = function () {
                $scope.editingMode = false;
                $scope.editingModeContact = false;
            };

            $scope.saveGroup = function (group) {
                $scope.editingMode = false;
                if (!angular.isDefined(group)) return;
                angular.forEach(group.fields, function (field) {
                    if (field.divider) return;
                    var editingField = $scope.editingModel[field.name];
                    if (editingField.tags) {
                        editingField.value = $.map(editingField.tags, function (tag) {
                            return tag.text;
                        });
                    }
                    angular.extend(field, editingField);
                });

                vaultService.saveVaultGroup(group, function () {
                    //  Check result
                });
            };

        }

        // End Vu
        $scope.delegateSearch = null;

        $scope.SearchResult = function (delegator, jsPath, value, entry) {
            $scope.query =
                {
                    groupName: '',
                    formName: '',
                    value: '',
                    pathForm: '',
                    mySelf: false
                };

            $scope.editForm.value = true;
            $scope.editingForm.value = entry;
            $scope.delegateSearch = delegator;

            var arrJsPath = jsPath.split(".");
            if (entry.value != undefined) {
                $scope.query.value = entry.value;
                if (entry.value == '') {
                    $scope.query.value = entry.label;
                }
            }


            $scope.query.groupName = arrJsPath[1];
            $scope.query.formName = arrJsPath[2];
            $scope.view.selectedDelegation = $scope.$parent.view.selectedDelegation;
            if ($scope.view.selectedDelegation.DelegationRole == 'Super')
                $scope.query.mySelf = true;
            if ($scope.query.groupName == 'education' || $scope.query.groupName == 'employment' || $scope.query.groupName == 'membership' || $scope.query.groupName == 'family') {
                $scope.query.pathForm = '/Areas/User/Views/VaultInformation/search/' + $scope.query.groupName + '.html';
            }
            else if ($scope.query.groupName != '' && $scope.query.formName != '')
                $scope.query.pathForm = '/Areas/User/Views/VaultInformation/search/' + $scope.query.groupName + '/' + $scope.query.formName + '.html';
            else if ($scope.query.groupName != '' && $scope.query.formName == '')
                $scope.query.pathForm = '/Areas/User/Views/VaultInformation/search/' + $scope.query.groupName + '.html';

        }
        $scope.cancelForm = function () {
            $scope.editForm.value = false;
            $scope.editingForm.value = null;
        }
        $scope.saveForm = function () {
            $scope.editForm.value = false;
            $scope.editingForm.value = null;

        }
        $scope.UpdateDataSearch = function () {
            $scope.vaultEdit.editingField = false;
            var vm = new Object();
            vm.UserId = $scope.view.selectedDelegation.FromAccountId;
            _vaultService.GetVault(vm).then(function (rs) {
                $scope.vaultInit(rs.VaultInformation);
                $scope.onVaultSearchInput();
            }, function (errors) {
                swal('Error', errors, 'error');
            });
        };
        $scope.CloseForm = function () {

            $scope.vaultEdit.editingField = false;

        };

    }]);

myApp.getController('ManagerDelegatedToController', ['$scope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', '$uibModal', 'ConfirmDialogService',
    function ($scope, $http, _userManager, _sweetAlert, _authService, $uibModal, ConfirmDialogService) {

        $scope.CreatePopup = function () {
            var modalInstance = $uibModal.open({
                animation: $scope.animationsEnabled,
                templateUrl: 'modal-delegate-new.html',
                controller: 'CreateDelegationController',
                size: "md",
                backdrop: 'static',
                resolve: {}
            });

            modalInstance.result.then(function () {
                $scope.getListDelegatedTo();
            }, function () {
            });
        }

        $scope.DelegationId = "";
        $scope.ListManagerDelegatedTo = [];

        $scope.getListDelegatedTo = function () {
            var DelegationModelView = new Object();
            DelegationModelView.Direction = "DelegationOut";
            $http.post('/api/DelegationManager/GetListDelegationFull', DelegationModelView)
                .success(function (response) {
                    $scope.ListManagerDelegatedTo = response.Listitems;

                }).error(function (errors, status) {
            });
        }

        $scope.Confirmdenieddelegation = function (delegationId) {
            $scope.DelegationId = delegationId;

            ConfirmDialogService.Open("Delete delegation?", function () {
                $scope.denieddelegation();
            });
        }

        $scope.denieddelegation = function () {
            var DelegationModelView = new Object();
            DelegationModelView.DelegationId = $scope.DelegationId;
            $http.post('/api/DelegationManager/DeniedDelegationItemTemplate', DelegationModelView)
                .success(function (response) {
                    $scope.getListDelegatedTo();
                }).error(function (errors, status) {
            });
        };

        $scope.getListDelegatedTo();

    }]);

myApp.getController('ManagerWhoDelegatedToController', ['$scope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'ConfirmDialogService', '$uibModal',
    function ($scope, $http, _userManager, _sweetAlert, _authService, ConfirmDialogService, $uibModal) {
        $scope.ViewPopup = function (delegation) {
            var modalInstance = $uibModal.open({
                animation: $scope.animationsEnabled,
                templateUrl: 'modal-delegate-view.html',
                controller: 'ViewDelegationController',
                size: "",
                backdrop: 'static',

                resolve: {
                    delegation: delegation
                }
            });
            modalInstance.result.then(function () {
                $scope.getListDelegatedTo();
            }, function () {

            });
        };

        $scope.view.selectedDelegation = $scope.$parent.view.selectedDelegation;

        $scope.DelegationId = "";
        $scope.Confirmdenieddelegation = function (delegationId) {
            $scope.DelegationId = delegationId;
            ConfirmDialogService.Open("Delete delegation?", function () {
                $scope.denieddelegation();
            });
        };

        $scope.denieddelegation = function () {
            var DelegationModelView = new Object();
            DelegationModelView.DelegationId = $scope.DelegationId;

            $http.post('/api/DelegationManager/DeniedDelegationItemTemplate', DelegationModelView)
                .success(function (response) {
                    $scope.getListDelegatedTo();
                }).error(function (errors, status) {
            });
        };

        $scope.ListManagerDelegatedTo = [];

        $scope.getListDelegatedTo = function () {
            var DelegationModelView = new Object();
            DelegationModelView.Direction = "DelegationIn";
            // Vu
            $http.post('/api/DelegationManager/GetListDelegationFull', DelegationModelView)
                .success(function (response) {
                    $scope.ListManagerDelegatedTo = response.Listitems;

                }).error(function (errors, status) {
            });
        };

        $scope.getListDelegatedTo();

    }]);

myApp.controller('ViewDelegationController', ['$scope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'ConfirmDialogService', 'delegation', '$uibModalInstance',
    function ($scope, $http, _userManager, _sweetAlert, _authService, ConfirmDialogService, delegation, $uibModalInstance) {
        $scope.messageBox = "";
        $scope.alerts = [];
        $scope.delegation = delegation
        $scope.TermsConditions = false;
        $scope.ishowagree = true;
        $scope.acceptdelegation = function () {

            if ($scope.TermsConditions) {
                var DelegationModelView = new Object();
                DelegationModelView.DelegationId = delegation.DelegationId;
                $http.post('/api/DelegationManager/AcceptDelegationItemTemplate', DelegationModelView)
                    .success(function (response) {
                        $uibModalInstance.close();
                    }).error(function (errors, status) {
                });
            }
            else {
                alertService.renderErrorMessage("Please agree to the Terms & Conditions to proceed.");
                $scope.messageBox = alertService.returnFormattedMessage();
                $scope.alerts = alertService.returnAlerts();
            }
        };

        $scope.Confirmdenieddelegation = function () {
            ConfirmDialogService.Open("Deny Delegation?", function () {
                $scope.denieddelegation();
            });
        };

        $scope.denieddelegation = function () {
            var DelegationModelView = new Object();
            DelegationModelView.DelegationId = delegation.DelegationId;
            $http.post('/api/DelegationManager/DeniedDelegationItemTemplate', DelegationModelView)
                .success(function (response) {
                    $uibModalInstance.close();
                }).error(function (errors, status) {
            });
        };

        $scope.close = function () {
            $uibModalInstance.close();
        }
    }]);

myApp.controller('CreateDelegationController', ['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'dateFilter', '$uibModal', '$uibModalInstance', 'alertService', '$timeout', 'VaultService', 'rguNotify',
    function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, dateFilter, $uibModal, $uibModalInstance, alertService, $timeout, _vaultService, rguNotify) {

        $scope.emergencyPermissions = [
            {
                name: 'Basic Information',
                read: false,
                jsonpaths: ['basicInformation.firstName', 'basicInformation.lastName', 'basicInformation.dob'],
                value: true,
                formName: 'basicInformation',
                accountId: '',
                path: '',
                details: false
            },
            {
                name: 'Contact Information',
                read: false,
                jsonpaths: ['contact.*'],
                value: true,
                formName: 'Contact',
                accountId: '',
                path: '',
                details: false
            },
            {
                name: 'Current Address',
                read: false,
                jsonpaths: ['address.currentAddress'],
                value: false,
                formName: 'currentAddress',
                accountId: '',
                path: '',
                details: false
            },
            {
                name: 'Passport',
                read: false,
                jsonpaths: ['governmentID.passportID'],
                value: false,
                formName: 'passportID',
                accountId: '',
                path: '',
                details: false
            },
            {
                name: 'Health Card',
                read: false,
                jsonpaths: ['governmentID.healthCard'],
                value: false,
                formName: 'healthCard',
                accountId: '',
                path: '',
                details: false
            },
            {
                name: 'Blood Type',
                read: false,
                jsonpaths: ['governmentID.healthCard.bloodType'],
                formName: 'healthCard',
                accountId: '',
                path: '',
                value: false
            },
            {
                name: 'Allergies',
                read: false,
                jsonpaths: ['other.preference.allergies'],
                value: false,
                formName: 'allergies',
                accountId: '',
                path: '',
                details: false
            }
        ];

        $scope.myVault = null;
        $scope.getMyVault = function () {
            var vm = new Object();
            _vaultService.GetVault(vm).then(function (rs) {

                $scope.myVault = rs.VaultInformation;
                for (var i = 0; i < $scope.emergencyPermissions.length; i++) {
                    $scope.emergencyPermissions[i].accountId = $scope.myVault.userId;
                    if ($scope.myVault.groupAddress.value.currentAddress.value.length > 0 && $scope.emergencyPermissions[i].name == 'Current Address')
                        $scope.emergencyPermissions[i].value = true;
                    else if ($scope.myVault.groupGovernmentID.value.passportID.value.length > 0 && $scope.emergencyPermissions[i].name == 'Passport')
                        $scope.emergencyPermissions[i].value = true;
                    else if ($scope.myVault.groupGovernmentID.value.healthCard.value.length > 0 && $scope.emergencyPermissions[i].name == 'Health Card')
                        $scope.emergencyPermissions[i].value = true;
                    else if ($scope.myVault.groupGovernmentID.value.healthCard.value.length > 0 && $scope.emergencyPermissions[i].name == 'Blood Type')
                        $scope.emergencyPermissions[i].value = true;
                    else if ($scope.myVault.others.value.preference.value.allergies.value.length > 0 && $scope.emergencyPermissions[i].name == 'Allergies') {
                        $scope.emergencyPermissions[i].value = true;
                    }
                }

            }, function (errors) {
                swal('Error', errors, 'error');
            });
        };


        $scope.getMyVault();
        $rootScope.cancelAddDataEmergency = function () {
            $scope.getFormEmergency.name = '';
            $scope.getMyVault();
        }
        $scope.getFormEmergency = {
            name: '',
            read: false,
            jsonpaths: [],
            value: true,
            formName: '',
            accountId: '',
            path: '',
            details: false
        }
        $scope.emergencyData = function (value) {
            $scope.getFormEmergency = {
                name: value.name,
                read: value.read,
                jsonpaths: value.jsonpaths,
                value: value.value,
                formName: value.formName,
                accountId: value.accountId,
                path: '',
                details: true
            }
            $scope.getFormEmergency.path = '/Areas/User/Views/DelegationManager/Emergency/' + $scope.getFormEmergency.formName + '.html';
        }
        // End my Vault
        $scope.basic = null;
        $scope.contact = null;
        $scope.address = null;
        $scope.passport = null;
        $scope.health = null;
        $scope.blood = null;
        $scope.allergies = null;
        $scope.Friends = [];
        var delegationtemplate = new Object();
        $scope.messageBox = "";
        $scope.alerts = [];
        $scope.InitData = function () {
            $scope.Role = delegationtemplate.delegationRole;
            if ($scope.Role == null || $scope.Role == null || $scope.Role == undefined) {
                $scope.Role = "Normal";
            }
            $scope.TermsConditions = false;
            $scope.dateformat = 'yyyy-MM-dd';
            $scope.Message = delegationtemplate.message;
            $scope.Note = delegationtemplate.note;
            $scope.Status = delegationtemplate.status;
            $scope.Friend = new Object();
            $scope.ExpiryDate = delegationtemplate.ExpiredDate;
            $scope.openedExpiryDate = false;
            $scope.GroupVaultsPermission = [
                {
                    name: 'Basic Information',
                    read: false,
                    write: false,
                    jsonpath: 'basicInformation'
                },
                {
                    name: 'Contact',
                    read: false,
                    write: false,
                    jsonpath: 'contact'
                },
                {
                    name: 'Address',
                    read: false,
                    write: false,
                    jsonpath: 'address'
                }, {
                    name: 'Govenment ID',
                    read: false,
                    write: false,
                    jsonpath: 'governmentID'
                }, {
                    name: 'Education',
                    read: false,
                    write: false,
                    jsonpath: 'education'
                }, {
                    name: 'Employment',
                    read: false,
                    write: false,
                    jsonpath: 'employment'
                }, {
                    name: 'Family',
                    read: false,
                    write: false,
                    jsonpath: 'family'
                }, {
                    name: 'Membership',
                    read: false,
                    write: false,
                    jsonpath: 'membership'
                }, {
                    name: 'Financial',
                    read: false,
                    write: false,
                    jsonpath: 'financial'

                }, {
                    name: 'Other',
                    read: false,
                    write: false,
                    jsonpath: 'others'
                }
            ];
            $scope.openExpiryDate = function () {
                $scope.openedExpiryDate = true;
            };
            $scope.ExpiryDateType = "Indefinite";
            if ("Indefinite" == $scope.ExpiryDate) {
                $scope.ExpiryDateType = $scope.ExpiryDate;
                $scope.showingEndDate = true;
            }
            else {
                $scope.showingEndDate = false;
            }
            $scope.EffectiveDate = new Date();
            $scope.openedEffectiveDate = false;
            $scope.openEffectiveDate = function () {
                $scope.openedEffectiveDate = true;
            };
        };
        // Get friends
        $scope.getDelegatedTemplate = function () {
            var DelegationModelView = new Object();
            $http.post('/api/DelegationManager/GetDelegationItemTemplate', DelegationModelView)
                .success(function (response) {
                    $scope.Friends = response.ListFriend;
                }).error(function (errors, status) {
            });
        }
        $scope.getDelegatedTemplate();
        // End Get friend
        $scope.FillData = function () {
            if ($scope.Friend != null) {
                delegationtemplate.toAccountId = $scope.Friend.UserId;
            }

            // Vu
            if ($scope.Role == 'Emergency') {
                if ("Indefinite" == $scope.ExpiryDateType) {
                    delegationtemplate.ExpiredDate = "Indefinite";
                }
                else
                    delegationtemplate.ExpiredDate = dateFilter($scope.ExpiryDate, $scope.dateformat);
                delegationtemplate.delegationRole = $scope.Role;
                delegationtemplate.message = $scope.Message;
                delegationtemplate.note = $scope.Note;
                delegationtemplate.status = "Pending";
                delegationtemplate.effectiveDate = dateFilter($scope.EffectiveDate, $scope.dateformat);
                delegationtemplate.GroupVaultsPermission = [];
                $($scope.emergencyPermissions).each(function (index, permission) {
                    delegationtemplate.GroupVaultsPermission.push({
                            name: permission.name,
                            jsonpaths: permission.jsonpaths,
                            read: permission.read,
                            write: false
                        }
                    );
                });
            }
            else {
                if ("Indefinite" == $scope.ExpiryDateType) {
                    delegationtemplate.ExpiredDate = "Indefinite";
                }
                else
                    delegationtemplate.ExpiredDate = dateFilter($scope.ExpiryDate, $scope.dateformat);
                delegationtemplate.delegationRole = $scope.Role;
                delegationtemplate.message = $scope.Message;
                delegationtemplate.note = $scope.Note;
                delegationtemplate.status = "Pending";
                delegationtemplate.effectiveDate = dateFilter($scope.EffectiveDate, $scope.dateformat);
                delegationtemplate.GroupVaultsPermission = [];
                $($scope.GroupVaultsPermission).each(function (index, permission) {
                    delegationtemplate.GroupVaultsPermission.push({
                            name: permission.name,
                            jsonpath: permission.jsonpath,
                            read: permission.read,
                            write: permission.write
                        }
                    );
                });
            }
        }

        $scope.InitData();

        $scope.InvitedDelegatedTemplate = function () {
            $scope.FillData();
            var DelegationModelView = new Object();

            //DelegationModelView.UserId = applicationConfiguration.usercurrent.Id;
            DelegationModelView.DelegationItemTemplateInsert = delegationtemplate;
            delegationtemplate.invitedEmail = $scope.InvitedEmail;

            $http.post('/api/DelegationManager/SaveDelegationItemTemplate', DelegationModelView)
                .success(function (response) {
                    if (response == "INVITED") {
                        alertService.renderErrorMessage("You cannot invite again.");
                    }
                    else {
                        alertService.renderSuccessMessage("Sent");
                    }
                    $scope.messageBox = alertService.returnFormattedMessage();
                    $scope.alerts = alertService.returnAlerts();
                    //$uibModalInstance.close();
                }).error(function (errors, status) {
                alertService.renderSuccessMessage("Sent");
                $scope.messageBox = alertService.returnFormattedMessage();
                $scope.alerts = alertService.returnAlerts();
            });
        }

        $scope.InvitedDelegatedTemplateOnSuccess = function (response) {
            alertService.renderSuccessMessage("Sent");
            $scope.messageBox = alertService.returnFormattedMessage();
            $scope.alerts = alertService.returnAlerts();
        }

        $scope.InvitedDelegatedTemplateOnError = function (response) {
            alertService.renderSuccessMessage("Sent");
            $scope.messageBox = alertService.returnFormattedMessage();
            $scope.alerts = alertService.returnAlerts();

        }
        $scope.SmSAuthencation = function () {
            if ($scope.TermsConditions)
                SMSAuthencationService.Open($uibModal, $scope.SaveDelegatedTemplate);
            else {
                alertService.renderErrorMessage("Please agree to the Terms & Conditions to proceed.");
                $scope.messageBox = alertService.returnFormattedMessage();
                $scope.alerts = alertService.returnAlerts();
            }
        };
        $scope.onPermissionChange = function (permission) {
            $timeout(function () {
                if (permission.write) {
                    permission.read = true;
                }
            });
        };
        $scope.SaveDelegatedTemplate = function () {

            $scope.FillData();
            var DelegationModelView = new Object();
            DelegationModelView.DelegationItemTemplateInsert = delegationtemplate;

            if ($scope.TermsConditions) {
                $http.post('/api/DelegationManager/SaveDelegationItemTemplate', DelegationModelView).success(function (response) {
                    $uibModalInstance.close();
                    rguNotify.add('Sent delegation request');
                    // console.log(delegationtemplate);
                    $rootScope.$broadcast('delegation:update');
                }).error(function (errors, status) {
                });
            }
            else {
                alertService.renderErrorMessage("Please agree to the Terms & Conditions to proceed.");
                $scope.messageBox = alertService.returnFormattedMessage();
                $scope.alerts = alertService.returnAlerts();
            }

        };

        $scope.canDelegate = function () {
            if (!$scope.Friends) return false;
            return $scope.Friends.length && ($scope.Friend && $scope.Friend.hasOwnProperty('Id')) && $scope.TermsConditions;
        };

        $scope.close = function () {
            $uibModalInstance.close();
        }

        //

    }]);








