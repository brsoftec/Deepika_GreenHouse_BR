    var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.cellNav'], true);

    //01 Search
    myApp.getController('SearchVaultController',
        ['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter', 'VaultService',
            function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter, _vaultService) {

                $scope.editForm = { value: false };
                $scope.editingForm = { value: null };
                $scope.query =
                        {
                            groupName: '',
                            formName: '',
                            value: '',
                            pathForm: '',
                            mySelf: true
                        };

                $scope.view = {
                    selectedDelegation: null
                };
                //Get vault
                $scope.GetDataVault = function() {
                    var vm = new Object();
                    _vaultService.GetVault(vm).then(function (rs) {
                        $scope.vaultInit(rs.VaultInformation);
                        $scope.view.selectedDelegation = {'FromAccountId': rs.VaultInformation.userId};
                    }, function (errors) {
                        swal('Error', errors, 'error');
                    });
                }
                $scope.GetDataVault();
                function toLabel(name) {
                    var result = name.replace(/([A-Z])/g, " $1");
                    return result.charAt(0).toUpperCase() + result.slice(1);
                }

                // search
                $scope.vaultInit = function (vault) {
                    $scope.vaultTree = vault;
                    var entries = [];
                    traverseVault($scope.vaultTree, 1, '', '');
                    // console.log(entries)
                    $scope.vaultEntries = entries;
                    //push value entries
                    function traverseVault(node, level, path, jsPath) {
                        if (!angular.isObject(node)) return;
                        angular.forEach(node, function (entry, name) {
                            if (!angular.isObject(entry))
                                return;

                            var label = entry.label;
                            var list = angular.isUndefined(label);
                            var sublist = entry.hasOwnProperty('sublist') && entry.sublist || name === 'contact' && level === 1;
                            list = list || sublist;
                            if (!angular.isObject(entry.value) && !list) {
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
                                    jsPath: jsPath + '.' + name
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
                                    jsPath: jsPath + '.' + name
                                };
                                if (list) {
                                    vaultEntry.list = true;
                                }
                                entries.push(vaultEntry);
                                if (list) {
                                    var fields = [];
                                    angular.forEach(entry, function (list, listName) {
                                        if (list === null) return;
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
                                                        jsPath: jsPath + '.' + name + '.' + sublistName + '.' + item.id
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
                                                jsPath: jsPath + '.' + name + '.' + listName
                                            };
                                            fields.push(field);
                                            entries.push(field);
                                        }

                                    });

                                    vaultEntry.children = fields;
                                }

                                if (!list) {
                                    traverseVault(entry.value, level + 1, path + '/' + entry.label, jsPath + '.' + name);
                                }
                            }
                            //end else
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

                // function search
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
             
                $scope.cancelField = function (field) {
                    $scope.vaultEdit.editingField = false;
                    $scope.query = null;
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
                // end function search
                $scope.delegateSearch = null;
                $scope.SearchResult = function (delegator, jsPath, value, entry) {
                    $scope.query =
                        {
                            groupName: '',
                            formName: '',
                            value: '',
                            pathForm: '',
                            mySelf: true
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
                
                $scope.UpdateDataSearch = function () {
                    $scope.vaultEdit.editingField = false;
                    var vm = new Object();
                    _vaultService.GetVault(vm).then(function (rs) {
                      
                        $scope.vaultInit(rs.VaultInformation);
                        $rootScope.$broadcast('UpdateVaultInformation');
                        $scope.onVaultSearchInput();
                      
                    }, function (errors) {
                        swal('Error', errors, 'error');
                    });
                };
                $scope.CloseForm = function () {
                    $scope.vaultEdit.editingField = false;
                };
          
            }]);
   
  //1. Basic Infomation 
  myApp.getController('SearchBasicController',
        ['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'vaultService', 'NetworkService', 'rguNotify', 'VaultService',
            function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, vaultService, _networkService, rguNotify, _vaultService) {

                var vaultData = null;
                $scope.formData = null;
                $scope.TestBasicForm = null;
                var VaultForm = null;
                $scope.countryCity = null;
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
                            $scope.TestBasicForm = response.value[$scope.query.formName];
                            $scope.initDataForm();
                        });

                   
                    // End init all Form
                }

                $scope.initSearchVault();

                //vault field: {"_id":"","label":"Date Of Birth","value":"","privacy":true,"type":"datecombo","rules":""}

                // End manual push to vault
                $scope.initDataForm = function () {
                    /* Permission */
                   $scope.formData;
                    $scope.read = false;
                    $scope.write = false;
                    var lstPermission = [];
                    if ($scope.query.mySelf == false) {
                        if ($scope.delegateSearch.GroupVaultsPermission != undefined || $scope.delegateSearch.GroupVaultsPermission == '') {

                            lstPermission = $scope.delegateSearch.GroupVaultsPermission;
                            for (var i = 0; i < lstPermission.length; i++) {

                                if (lstPermission[i].jsonpath == "basicInformation") {
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
                 

                    //Title
                
                    var _id = "";
                   
                        if (VaultForm._id === undefined || VaultForm._id === null) {
                            VaultForm._id = '';
                        }
                        if (VaultForm.options === undefined || VaultForm.options === null) {
                            VaultForm.options = '';
                        }
                        if (VaultForm.type === undefined || VaultForm.type === null) {
                            VaultForm.type = '';
                        }
                        if (VaultForm.rules === undefined || VaultForm.rules === null) {
                            VaultForm.rules = '';
                        }
                    $scope.field = {
                        '_id': VaultForm._id,
                        'label': VaultForm.label,
                        'value': VaultForm.value,
                        'privacy': VaultForm.privacy,
                        'options': VaultForm.options,
                        'type': VaultForm.type,
                        'rules': VaultForm.rules
                    }
                    if ($scope.field.label == 'City' || $scope.field.label == 'Country')
                    {
                        $scope.countryCity = { country: vaultData.value.country.value, city: vaultData.value.city.value };
                    }
                      

                    $scope.messageBox = "";
                    $scope.alerts = [];
                }
               
                $scope.Edit = function () {
                    $scope.IsEdit = true;
                }

                $scope.Save = function () {
                   
                    // Save Form Vault
                    VaultForm = $scope.field;
                    vaultData.value[$scope.query.formName] = VaultForm;
                    if ($scope.field.label == 'City' || $scope.field.label == 'Country') {
                        vaultData.value.country.value = $scope.countryCity.country;
                        vaultData.value.city.value = $scope.countryCity.city;
                    }

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
