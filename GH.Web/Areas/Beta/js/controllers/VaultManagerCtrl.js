angular.module('regitApp').controller('VaultManagerCtrl', function ($scope, $http, $moment, vaultService) {
    $scope.editingMode = false;
    $scope.editingModeContact = false;
    $scope.vaultForm = vaultService.getVaultForm();
    $scope.phoneType = 'Mobile';
    $scope.phoneType2 = 'Home';
    $scope.phoneCode = 'Singapore (+065)';
    $scope.phoneCode2 = 'Vietnam (+084)';
    $scope.phoneNo = '8219 2193';
    $scope.phoneNo2 = '7198 1297';
    $scope.defaultPhone = '1';
    $scope.email = 'johnie.rebuck@mail.com';
    $scope.defaultEmail = '0';

    //  VAULT TREE
    //  Prepare Vault tree for search
    $http.get('js/vault-search-new.json').then(function (response) {
        $scope.vaultInit(response.data);
    });

    $scope.vaultInit = function(vault) {
        $scope.vaultTree = vault;
        var entries = [];
        function traverseVault(node, level, path, jsPath) {
            if (!angular.isObject(node) || node.nosearch) return;
            angular.forEach(node, function (entry, name) {
                if (!angular.isObject(entry) || entry.nosearch)
                    return;
                var label = entry.label;
                var list = angular.isUndefined(label);
                if (!angular.isObject(entry.value) && !list) {
                    var vaultEntry = {
                        id: entry._id,
                        label: label,
                        description: entry.description,
                        options: entry.options,
                        leaf: true,
                        value: entry.value,
                        type: entry.controlType,
                        rules: entry.rules,
                        level: level,
                        path: path,
                        jsPath: jsPath + '.' + name
                    };
                    entries.push(vaultEntry);
                } else {
                    if (list) {
                        label = entry.description;

                    }
                    vaultEntry = {
                        id: entry._id,
                        label: label,
                        leaf: false,
                        level: level,
                        path: path,
                        jsPath: jsPath + '.' + name
                    };
                    entries.push(vaultEntry);
                    if (list) {
                        function toLabel(name) {
                            var result = name.replace( /([A-Z])/g, " $1" );
                            return result.charAt(0).toUpperCase() + result.slice(1);
                        }
                        vaultEntry.list = true;
                        var fields = [];
                        angular.forEach(entry, function (value, key) {
                            if (key !== '_default' && key !== 'description') {
                                var field = {
                                    value: value,
                                    leaf: true,
                                    label: toLabel(key),
                                    level: level + 1,
                                    path: path + '/' + key,
                                    jsPath: jsPath + '.' + name + '.' + key
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
            });
        }
        traverseVault($scope.vaultTree, 1, '', '');
        $scope.vaultEntries = entries;
    };

    $scope.vaultSearch = {
        query: '',
        searching: false,
        delegators: ['My Vault', 'Son Nguyen\'s Vault', 'Vu Nguyen\'s Vault'],
        selectedVault: 'My Vault'
    };

    $scope.selectVault = function() {
        console.log($scope.vaultSearch.selectedVault);
        // $scope.vaultInit(newVault);
    };

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
        if (angular.isObject(value)) {
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

        angular.forEach(entry.children, function (field) {
            matched = matched || re.test(field.label) || field.leaf && deepTest(field.value, re);
        });

        return matched;
    };

    $scope.gotoEditForm = function (path) {
        console.log(path);
    };
    $scope.editField = function (field) {
        $scope.vaultEdit.editingField = field;
        $scope.vaultEdit.editModel = field.value;
        // $document.find('.vault-entry-edit-control input').focus();
    };
    $scope.cancelField = function (field) {
        $scope.vaultEdit.editingField = false;
    };
    $scope.saveField = function (field) {
        $scope.vaultEdit.editingField = false;
        field.value = $scope.vaultEdit.editModel;
        // $scope.clearSearch();
    };
    $scope.test = {
        date: null//new Date()
    };

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

    $scope.followSuggestion = function (suggestion) {

    };

});



