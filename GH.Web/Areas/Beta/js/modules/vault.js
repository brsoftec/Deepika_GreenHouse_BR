angular.module('vault', ['angular-momentjs'])
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
            if (!query.length) return html;
            var re = new RegExp(query, 'i');
            return html.replace(re, '<b class="search-highlight">$&</b>');
        };
    })

    .filter('vaultValue', function ($moment) {
        return function (value) {
            if (angular.isUndefined(value))
                return ''; //<span class="vault-value-meta">(No value)</span>';
            if (/^\d{1,2}-\d{1,2}-\d\d\d\d$/.test(value)) {
                // var parts = value.split('-');
                value = $moment(value, 'D-M-YYYY').toDate();
            } else if (angular.isDate(value))
                return $moment(value).format('D MMM YYYY');
            if (angular.isArray(value))
                return value.join(', ');
            if (angular.isObject(value))
                return ''; //<span class="vault-value-meta">(Object)</span>'
            return value.toString();
        };
    })

    .filter('expiry', function ($moment) {
        return function (value) {
            if (!value)
                return '';
            if (/^\d{1,2}-\d{1,2}-\d\d\d\d$/.test(value)) {
                value = $moment(value, 'D-M-YYYY').toDate();
            } else {
                var moment = $moment(value);
                if (moment.isValid()) {
                    value = moment.toDate();
                }
            }
            if (!angular.isDate(value)) {
                return $moment(value).format('D MMM YYYY');
            } else
                return '';
        };
    })

    .filter('html', function ($sce) {
        return function (html) {
            return $sce.trustAsHtml(html);
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
                var tags = angular.element('<rgu-tags-input/>')
                    .attr({
                        'name': name,
                        'placeholder': field.placeholder || '',
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
                if ($element.hasClass('ng-invalid')) {
                    console.log($element);
                }
            }
        };
    })

    .factory('vaultService', function () {
        return {
            saveVaultGroup: function (group, callback) {
                //  Save group to vault db . . .
                var result = {success: true};
                if (angular.isFunction(callback)) {
                    callback.call(null, result);
                }
            },
            getVaultForm: function () {
                var vaultForm = {
                    groups: [
                        {
                            groupId: 'basic',
                            groupName: 'Your basic information',
                            fields: [
                                {
                                    name: 'basicTitle',
                                    label: 'Title',
                                    value: 'Mr.'
                                }, {
                                    name: 'basicFullName',
                                    label: 'Full Name',
                                    value: 'Johnnie Rebuck',
                                    privacy: true
                                }, {
                                    name: 'basicFirstName',
                                    label: 'First Name',
                                    value: 'Johnnie'
                                }, {
                                    name: 'basicMiddleName',
                                    label: 'Middle Name',
                                    value: 'Q'
                                }, {
                                    name: 'basicLastName',
                                    label: 'Last Name',
                                    value: 'Rebuck'
                                }, {
                                    name: 'basicAlias',
                                    label: 'Alias',
                                    value: 'Jojo',
                                    privacy: true
                                }, {
                                    name: 'basicDOB',
                                    label: 'D.O.B',
                                    value: new Date(),
                                    type: 'date',
                                    displayOptions: {control: 'combo'}
                                }, {
                                    name: 'basicGender',
                                    label: 'Gender',
                                    value: 'Male',
                                    type: 'option',
                                    optionValues: ['Male', 'Female']
                                },
                                {divider: true},
                                {
                                    name: 'basicLanguage',
                                    label: 'Language',
                                    value: ['English', 'Vietnamese'],
                                    type: 'list',
                                    placeholder: 'Add language',
                                    privacy: true
                                }, {
                                    name: 'basicEthnicity',
                                    label: 'Ethnicity',
                                    value: 'Asian-American',
                                    type: 'select',
                                    optionValues: ['Asian', 'Caucasean', 'American', 'Asian-American', 'Other']
                                }, {
                                    name: 'basicReligion',
                                    label: 'Religion',
                                    value: 'Buddhism',
                                    type: 'select',
                                    optionValues: ['Buddhism', 'Catholic', 'Muslim', 'Other']
                                }
                            ],
                            suggestions: [
                                {
                                    name: 'basicSexuality',
                                    label: 'Add your sexuality'
                                },
                                {
                                    name: 'basicDisability',
                                    label: 'Add your disability'
                                },
                                {
                                    name: 'basicNote',
                                    label: 'Write a note'
                                }
                            ]
                        }
                    ]
                };
                return vaultForm;
            }
        }
    })
    .directive('vaultFormDocManager', function (rguModal, defLists) {
        return {
            restrict: 'EA',
            templateUrl: RegitTemplatePath + '/vault-form-doc-manager.html',
            scope: {
                restrict: '@',
                docs: '=ngModel',
                jsPath: '@'
            },
            link: function (scope, elem, attrs) {

                scope.docCats = defLists.getList('docCats');

                scope.addDoc = function () {
                    var doc = {
                        name: '',
                        cat: '',
                        type: 'img',
                        fname: 'filename.jpg',
                        uploaded: new Date(),
                        url: ''
                    };
                    scope.openDocEditor(doc, true)
                };

                scope.openDocEditor = function (doc, newDoc) {

                    var jsPath = scope.jsPath;

                    if (newDoc) {
                        var cat = '';
                        if (/passport/i.test(jsPath)) {
                            cat = 'Passport';
                        } else if (/birth/i.test(jsPath)) {
                            cat = 'Birth Certificate';
                        } else if (/driver/i.test(jsPath)) {
                            cat = 'Driver License';
                        } else if (/member/i.test(jsPath)) {
                            cat = 'Membership Card';
                        } else if (/resume/i.test(jsPath)) {
                            cat = 'Resume';
                        } else if (/employment/i.test(jsPath)) {
                            cat = 'Employment Pass';
                        } else if (/credit/i.test(jsPath)) {
                            cat = 'Credit Card';
                        } else if (/credit/i.test(jsPath)) {
                            cat = 'Debit Card';
                        }
                        scope.doc = doc;
                        scope.doc.cat = cat;
                        scope.newDoc = newDoc;

                    }
                    scope.source = 'form';
                    rguModal.openModal('vault.doc.editor', scope);
                };

                scope.deleteDoc = function (doc) {
                    var index = $.inArray(doc, scope.docs);
                    if (index >= 0) {
                        scope.docs.splice(index, 1);
                    }
                };

                scope.saveDoc = function () {

                }

            }
        }

    });

