angular.module('regitBusiness')
    .controller('FormEditorCtrl', function ($scope, $document, $http, workingInteraction, workingForm) {
        //  FORM TEMPLATES
        $http.get('js/form-templates.json').then(function (response) {
            $scope.formTemplates = response.data.formTemplates;
        });
        $scope.selectedTemplate = null;
        $scope.onSelectTemplate = function () {
            var template = $scope.selectedTemplate;
            if (!template || !(template.hasOwnProperty('fields'))) return;
            angular.forEach(template.fields, function (field) {
                $scope.addEntryToFormByPath(field.jsPath, field.label);
            });
        };
        //  VAULT TREE
        //  Prepare Vault tree for search
        $http.get('js/vault-form.json').then(function (response) {
            $scope.vaultTree = response.data;
            var entries = [];

            function traverseVault(node, level, path, jsPath) {
                if (!angular.isObject(node) || node.nosearch) return;
                angular.forEach(node, function (entry, name) {

                    if (!angular.isObject(entry) || entry.nosearch || !angular.isDefined(entry.hiddenForm) || entry.hiddenForm === true)
                        return;

                    if (!angular.isObject(entry.value) || angular.isArray(entry.value)) {
                        //  Leaf entry (field)
                        var field = {
                            id: entry._id,
                            label: entry.label,
                            options: entry.value,
                            leaf: true,
                            type: entry.controlType,
                            undraggable: entry.undraggable,
                            rules: entry.rules,
                            level: level,
                            path: path,
                            jsPath: jsPath + '.' + name
                        };
                        if (jsPath.substring(0, 11) === '.membership') {
                            field.membership = true;
                        }
                        entries.push(field);
                    } else {
                        //  Non-leaf entry (folder)
                        entries.push({
                            id: entry._id,
                            label: entry.label,
                            leaf: false,
                            undraggable: entry.undraggable,
                            level: level,
                            path: path,
                            jsPath: jsPath + '.' + name
                        });
                        traverseVault(entry.value, level + 1, path + '.' + entry.label, jsPath + '.' + name);
                    }
                });
            }

            traverseVault($scope.vaultTree, 1, '', '');
            $scope.vaultEntries = entries;

        });
        $scope.vaultPane = {
            searchQuery: ''
        };
        $scope.formEntries = [];

        $scope.isMembershipStatic = function (entry) {
            return (entry.membership && entry.type === 'static');
        };
        $scope.isDeletable = function (field) {
            return !$scope.isMembershipStatic(field);
        };
        $scope.filterEntriesByQuery = function (entry) {
            if ($scope.isMembershipStatic(entry))
                return false;
            var re = new RegExp($scope.vaultPane.searchQuery, 'i');
            return re.test(entry.label);
        };

        $scope.onVaultSearchInput = function () {
            if (!$scope.vaultPane.searchQuery.length) {
                $scope.activeEntry = null;
            } else {
                $scope.activeEntry = 0;
                $scope.vaultPane.searchQuery = $scope.vaultPane.searchQuery.replace(/[^a-zA-Z0-9 .-/]/g, '');
            }
        };
        $scope.isActiveEntry = function (entry, index) {
            return $scope.activeEntry === index;
        };

        $scope.clearVaultSearch = function () {
            $scope.activeEntry = null;
            $scope.vaultPane.searchQuery = '';
        };
        $scope.gotoVaultEntry = function (index) {
            $scope.activeEntry = index;
        };
        $scope.selectVaultEntry = function (index) {
            $scope.addEntryToFormByIndex(index);
            $scope.activeEntry = index;
            $document.find('#vault-tree-search-input').focus();
            $scope.clearVaultSearch();

        };
        $scope.onVaultEntryKeyPress = function (event) {
            // event.preventDefault();
            var keyCode = event.which;
            var entryCount = $scope.matchedEntries.length;
            $scope.activeEntry = $scope.activeEntry || 0;
            var index = $scope.activeEntry;
            // console.log(keyCode, index, entryCount)
            if (keyCode === 40 && index < entryCount - 1) {
                $scope.activeEntry++;
            } else if (keyCode === 38 && index > 0) {
                $scope.activeEntry--;
            } else if (keyCode === 27) {
                $scope.activeEntry = null;
                $scope.vaultPane.searchQuery = '';
            } else if (keyCode === 13) {
                $scope.selectVaultEntry($scope.activeEntry);
            }
        };

        $scope.addedToForm = function (entry) {
            var found = false;
            angular.forEach($scope.formEntries, function (field) {
                if (entry.jsPath === field.jsPath) {
                    found = true;
                    if (entry.type === 'static') {

                    }
                }
            });
            return found;
        };
        $scope.addedToFormByPath = function (jsPath) {
            var found = false;
            angular.forEach($scope.formEntries, function (field) {
                if (jsPath === field.jsPath) {
                    found = true;
                }
            });
            return found;
        };

        $scope.hasMembership = function () {
            var found = false;
            angular.forEach($scope.formEntries, function (field) {
                if (field.membership && field.type !== 'static') {
                    found = true;
                }
            });
            return found;
        };
        $scope.checkMembership = function () {
            if ($scope.hasMembership()) {
                angular.forEach($scope.vaultEntries, function (entry) {
                    if ($scope.isMembershipStatic(entry)) {
                        $scope.addEntryToForm(entry, false);
                    }
                });

            } else {
                angular.forEach($scope.formEntries, function (field) {

                    if ($scope.isMembershipStatic(field)) {

                        $scope.deleteField(field, false);
                    }
                });
            }
        };

        $scope.addEntryToForm = function (entry, checkMembership) {
            if (entry.undraggable) return;
            if ($scope.addedToForm(entry))
                return;
            if (entry.leaf) {
                entry.displayName = entry.label;
                $scope.formEntries.push(angular.extend({}, entry));
                if (checkMembership) {
                    $scope.checkMembership();
                }
            } else {    // Add whole group
                var path = entry.path + '.' + entry.label;
                index = $.inArray(entry, $scope.vaultEntries);
                if (index < 0) return;
                while (++index < $scope.vaultEntries.length) {
                    var field = $scope.vaultEntries[index];
                    if (field.path !== path)
                        break;
                    if (!$scope.addedToForm(field)) {
                        field.displayName = field.label;
                        $scope.formEntries.push(angular.extend({}, field));
                    }
                }
            }

        };
        $scope.findEntryByPath = function (jsPath) {
            var foundEntry = null;
            angular.forEach($scope.vaultEntries, function (entry) {
                if (jsPath === entry.jsPath) {
                    foundEntry = entry;
                }
            });
            return foundEntry;
        };
        $scope.addEntryToFormByPath = function (jsPath, displayName) {
            if ($scope.addedToFormByPath(jsPath))
                return;
            var entry = $scope.findEntryByPath(jsPath);
            if (!entry) return;
            if (entry.leaf) {
                entry.displayName = displayName || entry.label;
                $scope.formEntries.push(angular.extend({}, entry));

            } else {    // Add whole group
                var path = entry.path + '.' + entry.label;
                index = $.inArray(entry, $scope.vaultEntries);
                if (index < 0) return;
                while (++index < $scope.vaultEntries.length) {
                    var field = $scope.vaultEntries[index];
                    if (field.path !== path)
                        break;
                    if (!$scope.addedToForm(field)) {
                        field.displayName = field.label;
                        $scope.formEntries.push(angular.extend({}, field));
                    }
                }
            }

        };
        $scope.addEntryToFormByIndex = function (index) {
            var entry = $scope.matchedEntries[index];
            $scope.addEntryToForm(entry, true);
        };

        $scope.deleteField = function (field, checkMembership) {
            var index = $.inArray(field, $scope.formEntries);
            if (index >= 0) {
                $scope.formEntries.splice(index, 1);
                if (checkMembership) {
                    $scope.checkMembership();
                }
            }

        };

        $scope.filterRequired = function (field) {
            return !field.optional && !field.membership && !field.qa;
        };
        $scope.filterOptional = function (field) {
            return !!field.optional;
        };
        $scope.filterMembership = function (field) {
            return !!field.membership;
        };
        $scope.filterQA = function (field) {
            return !!field.qa;
        };
        $scope.getUniqueId = function (field) {
            return field.jsPath;
        };
        $scope.formPane = {
            showingFieldPopup: []
        };
        
        function FieldGuid() {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            });
        }

        //$scope.qaId = 0;
        $scope.addQA = function () {
            //var id = ++$scope.qaId;
            var id = FieldGuid();
            var field = {
                qa: true,
                type: 'qa',
                displayName: 'Custom Question',
                path: 'Custom.Question.' + id,
                choices: false
            };
            $scope.formEntries.push(angular.extend({}, field));
        };

        //$scope.docId = 0;
        $scope.addDoc = function () {
            //var id = ++$scope.docId;
            var id = FieldGuid();
            var field = {
                qa: true,
                type: 'doc',
                displayName: 'Upload File',
                path: 'Custom.UploadFile.' + id,
                choices: false
            };
            $scope.formEntries.push(angular.extend({}, field));
        };

        $scope.docs = [
            {
                type: 'img',
                fname: 'f1.png'
            },
            {
                type: 'img',
                fname: 'f2.jpg'
            }
        ];

        $scope.close = function (field) {
            $scope.formPane.showingFieldPopup[$scope.getUniqueId(field)] = false;
        };
        $scope.saveField = function (field) {
            $scope.close(field);
        };
        $scope.saveForm = function () {

            workingForm.fields = $scope.formEntries;
        };

    });