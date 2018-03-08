angular.module('vault', ['angular-momentjs', 'regit.ui', 'ngTagsInput'])
    .directive('vaultIsDefault', function () {
        return {
            restrict: 'AEC',
            template: 'Default'
            // link: function(scope,elem,attrs) {
            // }
        }
    })

    .filter('vaultPath', function () {
        return function (path) {
            if (!path) return '';
            var pathParts = path.split('/');
            pathParts.splice(0, 1);
            pathParts.push('');
            return pathParts.join(' &raquo; ');
        };
    })

    .filter('vaultHighlightMatch', function () {
        return function (html, query) {
            if (!html) return;
            if (!query.length) return html;
            var re = new RegExp(query, 'i');
            return html.replace(re, '<b class="search-highlight">$&</b>');
        };
    })

    .filter('vaultValue', function ($moment, rgu) {
        return function (value) {
            if (angular.isUndefined(value))
                return ''; //<span class="vault-value-meta">(No value)</span>';
            if (angular.isArray(value))
                return value.join(', ');
            var parsedDate = rgu.parseDate(value);
            if (parsedDate) {
                return $moment(parsedDate.date).format('D MMMM YYYY');
            }
            if (angular.isObject(value)) {
                if (value.hasOwnProperty('country') && value.hasOwnProperty('city')) {
                    var country = value.country || '', city = value.city || '';
                    var text = city;
                    if (country && city) {
                        text += ', ';
                    }
                    text += country;
                    return text;
                }
                return '<span class="vault-value-meta">(Data)</span>'
            }
            return value.toString();
        };
    })

    .directive('editable', function ($compile) {

        function link(scope, elem, attrs) {
            var field = scope.field;
            var name = field.name, value = field.value, type = field.type;

            type = type || 'text';
            if (type === 'text' || type === 'number') {
                //  Render Text Editable
                var input = angular.element('<input/>')
                    .addClass('vault-input')
                    .attr({
                        'type': type,
                        'name': name,
                        'ng-model': 'field.value',
                        'ng-required': field.required
                    });
                // input.attr('required', true);
                $compile(input)(scope);
                elem.replaceWith(input);
            } else if (type === 'list') {
                //  Render List (free text list) editable
                field.tags = $.map(value, function (item) {
                    return {text: item};
                });
                var tags = angular.element('<tags-input/>')
                    .attr({
                        'name': name,
                        'placeholder': field.placeholder || '',
                        'auto-complete': 'autoCompleteLoaders[{{name}}]($query)',
                        'ng-model': 'field.tags'
                    });
                $compile(tags)(scope);
                elem.replaceWith(tags);
            } else if (type === 'date') {
                //  Render Date editable
                var dateInput = angular.element('<rgu-date-input/>')
                    .attr({
                        'name': name,
                        'type': field.displayOptions && field.displayOptions.control === 'combo' ? 'combo' : 'datepicker',
                        'ng-model': 'field.value'
                    });
                $compile(dateInput)(scope);
                elem.replaceWith(dateInput);
            } else if (type === 'option') {
                //  Render Option (radio) editable
                var radioGroup = angular.element('<rgu-radio-group/>')
                    .attr({
                        'name': name,
                        'values': 'field.optionValues',
                        'ng-model': 'field.value'
                    });
                $compile(radioGroup)(scope);
                elem.replaceWith(radioGroup);
            } else if (type === 'select') {
                //  Render Select (dropdown) editable
                var select = angular.element('<select/>')
                    .attr({
                        'name': name,
                        'ng-options': 'option as option for option in field.optionValues',
                        'ng-model': 'field.value'
                    });
                var wrapper = angular.element('<div class="select"/>');
                wrapper.append(select);
                $compile(wrapper)(scope);
                elem.replaceWith(wrapper);
            }
        }

        return {
            restrict: 'E',
            replace: true,
            link: link,
            scope: {
                field: '='
            },
            controller: function ($scope, $element, $attrs) {
                console.log($element);
                if ($element.hasClass('ng-invalid')) {
                    console.log($element);
                }
            }
        };
    })

    .directive('vaultPush', function ($timeout, rguModal, vaultService, rguNotify) {
        var editOldValue = '';

        return {
            restrict: 'EA',
            scope: {
                group: '<',
                result: '=',
                onSend: '&',
                network: '<'
            },
            templateUrl: '/Areas/regitUI/templates/vault-push.html',
            link: function (scope, elem, attrs) {
                scope.view = {
                    formOpen: false,
                    editing: false,
                    adding: false,
                    showingMessage: false
                };
                scope.sendOptions = {
                    optionLabels: ['From network', 'User email'],
                    sendEmail: false,
                    sendToEmail: '',
                    sendToPerson: {},
                    emailFails: ''
                };
                scope.messages = {
                    fields: '',
                    email: '',
                    user: ''
                };
                scope.clearMessages = function (type, msg) {
                    scope.messages.fields = '';
                    scope.messages.email = '';
                    scope.messages.user = '';
                    scope.view.showingMessage = false;
                };
                scope.showMessage = function (type, msg) {
                    if (scope.messages.hasOwnProperty(type)) {
                        scope.clearMessages();
                        scope.messages[type] = msg;
                        scope.view.showingMessage = true;
                    }
                };

                // scope.network = scope.network || [
                //         {
                //             "Id": "2a920733-83c0-4c20-80e4-5095346d7968",
                //             "Avatar": "/Areas/Beta/img/avatars/1.jpg",
                //             "DisplayName": "Vu Delegate Nguyen"
                //         },
                //         {
                //             "Id": "2a920733-83c0-4c20-80e4-5095346d7920",
                //             "Avatar": "/Areas/Beta/img/avatars/2.jpg",
                //             "DisplayName": "Son Quay"
                //         }
                //
                //     ];

                if (!scope.group) return;

                var fields;

                if (scope.group.type === 'listgroup') {
                    var index = attrs.formIndex;
                    if (angular.isUndefined(index))
                        return;
                    if (!attrs.list || !scope.group.lists || !scope.group.lists.hasOwnProperty(attrs.list))
                        return;
                    var list = scope.group.lists[attrs.list];
                    if (list.length <= index)
                        return;
                    fields = list[index].fields;
                    scope.formLabel = deCamelize(attrs.list);
                } else if (scope.group.type === 'list') {
                    var index = attrs.formIndex;
                    if (angular.isUndefined(index))
                        return;
                    var list = scope.group.list;
                    if (list.length <= index)
                        return;
                    fields = list[index].fields;
                    scope.formLabel = scope.group.label;
                } else {
                    fields = scope.group.fields;
                    scope.formLabel = scope.group.label;
                }

                // swal({text: 'test',
                // showCancelButton: true});

                scope.fields = angular.copy(fields);

                scope.result = scope.result || {fields: []};
                scope.sendTo = scope.sendTo || {};

                scope.initFields = function () {
                    vaultService.populateGroup(scope.group);
                    // scope.pushedFields = angular.copy(scope.fields);
                    var fields = {};
                    if (scope.group.type === 'listgroup') {
                        fields = angular.copy(scope.group.lists[attrs.list][index].fields);
                    } else if (scope.group.type === 'list') {
                        fields = angular.copy(scope.group.list[index].fields);
                    } else {
                        fields = angular.copy(scope.group.fields);
                    }
                    scope.fields = fields;
                    scope.fieldCount = 0;
                    scope.pushedCount = 0;
                    scope.pushedFields = {};

                    angular.forEach(fields, function (field, index) {
                        scope.fieldCount++;
                        if (field.value && !(field.type === 'location' && !field.value.country && !field.value.city)) {
                            scope.pushedFields[field.name] = field;
                            scope.pushedCount++;
                        }
                    });

                };

                scope.isPushed = function (field) {
                    var found = false;
                    angular.forEach(scope.pushedFields, function (pushedField) {
                        if (pushedField.jsPath === field.jsPath) {
                            found = true;
                        }
                    });
                    return found;
                };
                scope.fieldsNotPushed = function (field) {
                    return !scope.isPushed(field);
                };
                scope.openPushForm = function () {
                    scope.initFields();
                    scope.view.formOpen = true;
                    rguModal.openModal('vault.push', scope);
                };
                // scope.openPushForm();

                scope.openAdder = function () {
                    scope.view.adding = true;
                };
                scope.closeAdder = function () {
                    scope.view.adding = false;
                };
                scope.addField = function (field) {
                    scope.pushedFields[field.name] = angular.copy(field);
                    scope.pushedCount++;
                    scope.closeAdder();
                };

                scope.editField = function (field) {
                    editOldValue = field.value;
                    scope.view.editing = true;
                    field.editing = true;
                };
                scope.cancelEdit = function (field) {
                    field.value = editOldValue;
                    scope.view.editing = false;
                    field.editing = false;
                };

                scope.saveEdit = function (field) {
                    scope.view.editing = false;
                    field.editing = false;
                };

                scope.removeField = function (field) {
                    $timeout(function () {
                        delete scope.pushedFields[field.name];
                        scope.pushedCount--;
                    });
                };

                scope.cancelPushForm = function () {
                    scope.view.formOpen = false;
                };
                scope.sendPushForm = function (hider) {

                    if (!scope.pushedCount) {
                        scope.showMessage('fields', 'No information to push. Please add from your vault');
                        return;
                    }
                    if (scope.sendOptions.sendEmail) {
                        var rule = scope.sendOptions.emailFails;
                        if (rule) {
                            scope.showMessage('email', 'Can\'t assign recipient: ' +
                                (rule.name === 'require' ? 'Missing email address' : 'Invalid email address'));
                            return;
                        }
                    } else if (!scope.sendOptions.sendToPerson.Id) {
                        scope.showMessage('user', 'Can\'t assign recipient: Please select a person from your network');
                        return;
                    }

                    scope.result.fields = $.map(scope.pushedFields, function (value) {
                        return value;
                    });

                    scope.result.sendTo = {
                        comment: scope.sendOptions.comment
                    };
                    if (scope.sendOptions.sendEmail) {
                        scope.result.sendTo.userId = '';
                        scope.result.sendTo.userEmail = scope.sendOptions.sendToEmail;
                    } else {
                        scope.result.sendTo.userId = scope.sendOptions.sendToPerson.Id;
                        scope.result.sendTo.userEmail = '';
                    }

                    if (angular.isFunction(scope.onSend)) {
                        scope.onSend();
                    }
                    scope.view.formOpen = false;
                    hider();
                };
            }
        };
    })

    .factory('vaultService', function ($timeout, rgu) {
        var groupMap = {
            'basicInformation': {type: 'group'},
            'address': {type: 'listgroup'},
            'membership': {type: 'listgroup'},
            'groupGovernmentID': {type: 'listgroup'}
        };
        var fieldMap = {
            'basicInformation.title': {},
            'basicInformation.firstName': {},
            'basicInformation.middleName': {},
            'basicInformation.lastName': {},
            'basicInformation.alias': {},
            'basicInformation.dob': {type: 'datecombo'},
            'basicInformation.gender': {
                type: 'radio',
                options: ['Male', 'Female', 'Other']
            },
            'basicInformation.location': {
                type: 'location'
            },
            'address.*.description': '',
            'address.*.addressLine': '',
            'address.*.zipCode': '',
            'address.*.startDate': {
                type: 'datecombo'
            },
            'address.*.endDate': {
                type: 'datecombo'
            },
            'membership.*.description': '',
            'membership.*.businessName': '',
            'membership.*.membershipProgramName': '',
            'membership.*.membershipNumber': '',
            'membership.*.membershipClass': '',
            'membership.*.holder': '',
            'membership.*.expiryDate': {type: 'datecombo'},
            'membership.*.loginId': '',
            'membership.*.password': '',
            'membership.*.loginSite': '',
            'membership.*.serviceProvider': '',
            'groupGovernmentID.*.firstName': '',
            'groupGovernmentID.*.middleName': '',
            'groupGovernmentID.*.lastName': '',
            'groupGovernmentID.*.description': '',
            'groupGovernmentID.*.certificateNumber': '',
            'groupGovernmentID.*.cardNumber': '',
            'groupGovernmentID.*.cardType': '',
            'groupGovernmentID.*.tier': '',
            'groupGovernmentID.*.class': '',
            'groupGovernmentID.*.classTier': '',
            'groupGovernmentID.*.nationality': '',
            'groupGovernmentID.*.issuedBy': '',
            'groupGovernmentID.*.issuedIn': '',
            'groupGovernmentID.*.address': '',
            'groupGovernmentID.*.phone': '',
            'groupGovernmentID.*.email': '',
            'groupGovernmentID.*.issuedDate': {type: 'datecombo'},
            'groupGovernmentID.*.expiryDate': {type: 'datecombo'},
            'groupGovernmentID.*.bloodType': {
                type: 'select',
                options: ["A", "B", "AB", "O"]
            },
        };

        return {
            // saveVaultGroup: function (group, callback) {
            //     //  Save group to vault db . . .
            //     var result = {success: true};
            //     if (angular.isFunction(callback)) {
            //         callback.call(null, result);
            //     }
            // },
            // getVaultForm: function () {
            //     var vaultForm = {
            //         groups: [
            //             {
            //                 groupId: 'basic',
            //                 groupName: 'Your basic information',
            //                 fields: [
            //                     {
            //                         name: 'basicTitle',
            //                         label: 'Title',
            //                         value: 'Mr.'
            //                     }, {
            //                         name: 'basicFullName',
            //                         label: 'Full Name',
            //                         value: 'Johnnie Rebuck',
            //                         privacy: true
            //                     }, {
            //                         name: 'basicFirstName',
            //                         label: 'First Name',
            //                         value: 'Johnnie'
            //                     }, {
            //                         name: 'basicMiddleName',
            //                         label: 'Middle Name',
            //                         value: 'Q'
            //                     }, {
            //                         name: 'basicLastName',
            //                         label: 'Last Name',
            //                         value: 'Rebuck'
            //                     }, {
            //                         name: 'basicAlias',
            //                         label: 'Alias',
            //                         value: 'Jojo',
            //                         privacy: true
            //                     }, {
            //                         name: 'basicDOB',
            //                         label: 'D.O.B',
            //                         value: new Date(),
            //                         type: 'date',
            //                         displayOptions: {control: 'combo'}
            //                     }, {
            //                         name: 'basicGender',
            //                         label: 'Gender',
            //                         value: 'Male',
            //                         type: 'option',
            //                         optionValues: ['Male', 'Female']
            //                     },
            //                     {divider: true},
            //                     {
            //                         name: 'basicLanguage',
            //                         label: 'Language',
            //                         value: ['English', 'Vietnamese'],
            //                         type: 'list',
            //                         placeholder: 'Add language',
            //                         privacy: true
            //                     }, {
            //                         name: 'basicEthnicity',
            //                         label: 'Ethnicity',
            //                         value: 'Asian-American',
            //                         type: 'select',
            //                         optionValues: ['Asian', 'Caucasean', 'American', 'Asian-American', 'Other']
            //                     }, {
            //                         name: 'basicReligion',
            //                         label: 'Religion',
            //                         value: 'Buddhism',
            //                         type: 'select',
            //                         optionValues: ['Buddhism', 'Catholic', 'Muslim', 'Other']
            //                     }
            //                 ],
            //                 suggestions: [
            //                     {
            //                         name: 'basicSexuality',
            //                         label: 'Add your sexuality'
            //                     },
            //                     {
            //                         name: 'basicDisability',
            //                         label: 'Add your disability'
            //                     },
            //                     {
            //                         name: 'basicNote',
            //                         label: 'Write a note'
            //                     }
            //                 ]
            //             }
            //         ]
            //     };
            //     return vaultForm;
            // },
            populateGroup: function (group) {
                var groupName = group.name;
                var model = group.model;

                if (!groupMap.hasOwnProperty(groupName))
                    return;

                var type = group.type || 'group';
                if (type === 'group') {
                    group.fields = {};
                    angular.forEach(model, function (item, name) {
                        var path = group.name + '.' + name;
                        if (!fieldMap.hasOwnProperty(path)) return;
                        var model;
                        if (path === 'basicInformation.location') {
                            model = {
                                label: 'City/Country',
                                value: {
                                    country: group.model.country.value,
                                    city: group.model.city.value
                                }
                            }
                        }
                        var spec = fieldMap[path];
                        var type = spec.type || 'textbox';
                        model = model || group.model[name];
                        var value = model.value;
                        var parsedDate = rgu.parseDate(value);
                        if (parsedDate) {
                            value = parsedDate.date;
                        }
                        var field = {
                            name: name,
                            jsPath: path,
                            label: model.label,
                            value: value,
                            type: type
                        };
                        if (spec.options) {
                            field.options = spec.options;
                        }

                        group.fields[name] = field;
                    });

                } else if (type === 'list') {
                    var list = [];
                    angular.forEach(group.model, function (fields, index) {
                        var listPath = group.name;
                        var form = {
                            path: listPath,
                            index: index,
                            fields: {}
                        };
                        var location;
                        angular.forEach(fields, function (item, name) {
                            if (name === 'country') {
                                if (!location) location = {};
                                location.country = item;
                            } else if (name === 'city') {
                                if (!location) location = {};
                                location.city = item;
                            }
                            var path = group.name + '.*.' + name;
                            if (!fieldMap.hasOwnProperty(path))
                                return;
                            var spec = fieldMap[path];
                            var type = spec.type || 'textbox';
                            var label = spec.label || deCamelize(name);
                            var field = {
                                name: name,
                                jsPath: listPath + '.' + name,
                                value: item,
                                label: label,
                                type: type
                            };
                            if (spec.options) {
                                field.options = spec.options;
                            }
                            form.fields[name] = field;
                        });
                        if (location) {
                            form.fields['location'] = {
                                name: 'location',
                                jsPath: listPath + '.location',
                                value: location,
                                label: 'Country/City',
                                type: 'location'
                            };
                        }
                        list.push(form);
                    });

                    group.list = list;

                } else if (type === 'listgroup') {
                    group.lists = {};
                    angular.forEach(model, function (listModel, name) {
                        var listName = camelize(name);
                        var listPath = group.name + '.' + listName;
                        var list = [];
                        angular.forEach(listModel.value, function (fields, index) {
                            var form = {
                                path: listPath,
                                list: list,
                                index: index,
                                fields: {}
                            };
                            var location;
                            angular.forEach(fields, function (item, name) {
                                var path = group.name + '.*.' + name;
                                if (name === 'country') {
                                    if (!location) location = {};
                                    location.country = item;
                                } else if (name === 'city') {
                                    if (!location) location = {};
                                    location.city = item;
                                }
                                if (!fieldMap.hasOwnProperty(path))
                                    return;
                                var spec = fieldMap[path];
                                var type = spec.type || 'textbox';
                                var label = spec.label || deCamelize(name);
                                var field = {
                                    name: name,
                                    jsPath: listPath + '.' + name,
                                    value: item,
                                    label: label,
                                    type: type
                                };
                                if (spec.options) {
                                    field.options = spec.options;
                                }
                                form.fields[name] = field;
                            });
                            if (location) {
                                form.fields['location'] = {
                                    name: 'location',
                                    jsPath: listPath + '.location',
                                    value: location,
                                    label: 'Country/City',
                                    type: 'location'
                                };
                            }
                            list.push(form);
                        });

                        group.lists[listName] = list;

                    });

                }

            }
        };

    });