angular.module('interactionEditor').controller('InteractionEditorController',
    ['$scope', '$rootScope', '$document', '$timeout', '$location', 'rguModal', 'rguNotify', 'rguAlert', '$http', 'rgu', 'rguCache', 'rguView', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'NotificationService', 'notificationService', 'SmSAuthencationService', 'fileUpload', 'interactionService', 'interactionFormService', 'formService',
        function ($scope, $rootScope, $document, $timeout, $location, rguModal, rguNotify, rguAlert, $http, rgu, rguCache, rguView, userManager, sweetAlert, authService, alertService, notificationService, notifService, SmSAuthencationService, fileUpload, interactionService, interactionFormService, formService) {

            var isAdmin = false;
            var isApprover = false;
            if (angular.isArray(regitGlobal.workflowAccount.roles)) {
                regitGlobal.workflowAccount.roles.forEach(function (role) {
                    if (role === 'admin') isAdmin = true;
                    else if (role === 'approver') isApprover = true;
                });
            }

            var interaction = $scope.interaction = regitGlobal.interaction;
            var reviewing = $scope.reviewing = interaction.reviewing;
            var isNew = $scope.isNew = interaction.isNew;
            var newFromTemplate = !!interaction.newFromTemplate;
            var isBrandNew = $scope.isBrandNew = interaction.isNew && !newFromTemplate;

            var type = $scope.type = interaction.type;
            var status = $scope.status = interaction.status.toLowerCase();
            var isEvent = $scope.isEvent = type === 'event';
            var isBroadcast = $scope.isBroadcast = type === 'broadcast';
            var isTemplate = $scope.isTemplate = status === 'template';
            $scope.Type = rgu.toTitleCase(type);

            var participants = interaction.participants || 0;

            $scope.rguView = rguView;

            $scope.pages = [
                {
                    name: 'details',
                    label: 'Interaction Details'
                },
                {
                    name: 'publish',
                    label: 'Publishing'
                },
                {
                    name: 'form',
                    label: 'Form'
                },
                {
                    name: 'workflow',
                    label: 'Workflow'
                }
            ];

            $scope.loadInteraction = function (interaction) {
                $http.get('/api/Interactions/Get/' + interaction.id)
                    .success(function (response) {
                    }).error(function (errors) {
                    console.log('Error loading interaction', errors)
                });
            };

            interaction.editing = true;
            if (!interaction.business) {
                interaction.business = {
                    id: regitGlobal.businessAccount.id,
                    accountId: regitGlobal.businessAccount.accountId,
                    name: regitGlobal.businessAccount.displayName,
                    avatar: regitGlobal.businessAccount.avatar
                }
            }

            if (!interaction.status || interaction.newFromTemplate) {
                interaction.status = 'New';
            }
            if (!interaction.name) {
                interaction.name = '';
            }
            if (!interaction.from) {
                interaction.from = new Date();
            }
            if (!interaction.until) {
                interaction.indefinite = true;
                interaction.until = new Date();
            }
            if (!interaction.price) {
                interaction.paid = false;
            }
            if (!interaction.termsType) {
                interaction.termsType = 'url';
                interaction.termsUrl = '';
            }

            if (isEvent && !interaction.eventInfo) {

                interaction.eventInfo = {
                    fromDate: '',
                    fromTime: '',
                    toDate: '',
                    toTime: '',
                    theme: '',
                    location: ''
                };

            }

            if (!interaction.target) {
                interaction.target = 'all';
            }
            if (!interaction.distribute) {
                interaction.distribute = 'public';
            }
            if (!interaction.criteria) {
                interaction.criteria = {
                    gender: 'All',
                    age: {
                        type: 'all',
                        min: 1,
                        max: 100
                    },
                    location: {
                        type: 'all',
                        country: '',
                        area: ''
                    }
                };
            }

            if (!interaction.verb) {
                var verb = 'register';
                switch (type) {
                    case 'event':
                    case 'handshake':
                        verb = 'join';
                        break;
                    case 'srfi':
                        verb = 'submit';
                        break;
                    case 'pushtovault':
                        verb = 'accept';
                        break;
                }
                interaction.verb = verb;
            }


            if (!angular.isString(interaction.socialShare)) {
                $scope.social = {
                    sharing: true,
                    providers: 'all'
                };
            } else {
                $scope.social = {
                    sharing: interaction.socialShare.length > 0,
                    providers: interaction.socialShare
                };
            }

            if (!interaction.notes) {
                interaction.notes = '';
            }
            $scope.comments = {
                general: '',
                details: '',
                publish: '',
                form: ''
            };
            if (!interaction.comments) {
                interaction.comments = [];
            }
            interaction.comments.forEach(function (c) {
                $scope.comments[c.category] = c.text;
            });

            interaction.publicUrl = interactionService.publicUrl(interaction);
            if (!interaction.participants) {
                interaction.participants = 0;
            }

            $scope.touched = isBrandNew ? {
                all: false,
                any: false,
                details: false,
                publish: false,
                form: false,
                workflow: false
            } : {
                all: true,
                any: true,
                details: true,
                publish: true,
                form: true,
                workflow: true
            };

            $scope.validity = {
                any: true,
                all: true,
                pages: {
                    details: true,
                    publish: true,
                    form: true,
                    workflow: true
                },
                fields: {
                    name: false,
                    terms: false,
                    event: false,
                    choices: true
                }
            };

            $scope.ageRange = {
                invalid: false
            };

            $scope.form = {
                reviewing: reviewing,
                dirty: false,
                formDirty: false,
                fields: [],
                groups: []
            };
            if (reviewing) {
                $scope.review = {
                    dirty: false
                };
            }

            $scope.verbs = ['register', 'join', 'participate', 'submit', 'send', 'enroll', 'apply', 'accept'];
            $scope.verbLabels = ['Register', 'Join', 'Participate', 'Submit', 'Send', 'Enroll', 'Apply', 'Accept'];
            $scope.verbLabel = '';

            if (isBrandNew) {
                $scope.form.templates = [];
                $http.post('/api/CampaignService/GetFormTemplate', {})
                    .success(function (response) {
                        $scope.form.templates = response.TreeVault.formTemplates;
                    })
                    .error(function (errors) {
                        console.log('Error loading form templates', errors)
                    });
                $scope.form.selectedTemplate = null;
                $scope.onSelectTemplate = function () {
                    if ($scope.form.formDirty) return;
                    var template = $scope.form.selectedTemplate;
                    $scope.deleteAllFields();
                    if (!template || !(template.hasOwnProperty('fields'))) return;

                    angular.forEach(template.fields, function (field) {
                        $scope.addEntryToFormByPath(field);
                    });
                };
            }

            $scope.completeDetails = function () {
                return interaction.name && (isBroadcast || interaction.termsUrl) && (!isEvent || interaction.eventInfo.fromDate && interaction.eventInfo.fromTime
                    && interaction.eventInfo.location && interaction.eventInfo.location.length > 0);
            };
            $scope.canPreviewDetails = function () {
                return $scope.completeDetails();
            };
            $scope.completePublish = function () {
                return interaction.target==='all' || interaction.criteria.age.type!=='range' || !$scope.ageRange.invalid;
            };
            $scope.completeForm = function () {
                return isBroadcast || $scope.form.fields.length;
                // return isBroadcast || $scope.form.fields.length && !$scope.form.fields.some(function(field) {
                //     return field.edit && field.edit.invalid;
                // });
            };
            $scope.canPreviewForm = function () {
                return $scope.completeDetails() && $scope.completeForm();
            };
            $scope.canSaveInteraction = function () {
                return $scope.completeDetails() && $scope.completePublish() && $scope.completeForm();
            };
            $scope.hasComments = function () {
                return $scope.comments.general.length || $scope.comments.details.length
                    || $scope.comments.publish.length || $scope.comments.form.length;
            };
            $scope.canFeedback = function () {
                return $scope.review.dirty && $scope.hasComments();
            };

            $scope.checkValidity = function (obj) {
                if (!obj || $scope.touched.details && obj === 'details') {
                    $scope.validity.pages.details = $scope.completeDetails();
                }
                if (!obj || $scope.touched.publish && obj === 'publish') {
                    $scope.validity.pages.publish = $scope.completePublish();
                }
                if (!isBroadcast && (!obj || $scope.touched.form && obj === 'form')) {
                    $scope.validity.pages.form = $scope.completeForm();
                }
                if (!obj || $scope.touched.details && obj === 'details.name') {
                    $scope.validity.fields.name = interaction.name && interaction.name.length > 0;
                }
                if (!isBroadcast && (!obj || $scope.touched.details && obj === 'details.terms')) {
                    $scope.validity.fields.terms = interaction.termsUrl && interaction.termsUrl.length > 0;
                }
                if (isEvent) {
                    if (!obj || $scope.touched.details && obj === 'details.event') {
                        $scope.validity.fields.event = interaction.eventInfo.fromDate && interaction.eventInfo.fromTime
                            && interaction.eventInfo.location && interaction.eventInfo.location.length > 0;
                    }
                }
            };

            $scope.addedToForm = function (entry) {
                var found = false;
                angular.forEach($scope.form.fields, function (field) {
                    if (entry.jsPath === field.jsPath) {
                        found = true;
                        return;
                    }
                });
                return found;
            };
            $scope.addedToFormByPath = function (jsPath) {
                var found = false;
                angular.forEach($scope.form.fields, function (field) {
                    if (jsPath === field.jsPath) {
                        found = true;
                    }
                });
                return found;
            };

            $scope.addEntryToForm = function (entry, source) {
                if (entry.undraggable) return;
                if ($scope.addedToForm(entry))
                    return;
                var ranks = ['basicInformation', 'contact', 'address', 'financial', 'governmentID', 'family', 'employment', 'education', 'others', 'userInformation'];

                function labelFromName(name) {
                    return rgu.deCamelize(name);
                }

                var field = angular.copy(entry);

                if (field.leaf || source === 'template') {
                    if (!angular.isString(field.jsPath)) return;

                    if (!field.displayName) field.displayName = entry.label;
                    // if (source === 'template') {
                    //     field.label = labelFromName()
                    // }

                    if (!field.optional && entry.label === 'Middle Name') {
                        field.optional = true;
                    }
                    //field.optional = field.optional || false;

                    if (field.type === 'static') {
                        switch (field.jsPath) {
                            case '.membership.businessName':
                                field.displayName = 'Business Name';
                                break;
                            case '.membership.membershipProgramName':
                                field.displayName = 'Program Name';
                                break;
                        }
                        if (field.hasOwnProperty('value')) {
                            field.options = field.value || '';
                        } else if (!field.options) {
                            if (field.jsPath === '.membership.businessName') {
                                field.options = regitGlobal.businessAccount.displayName;
                            }
                        }
                    }


                    var path = field.jsPath.split('.');
                    var group = path[1];
                    if (group === 'address' || group === 'financial' || group === 'governmentID') {
                        group = path[2];
                    }

                    if (!field.jsPath || field.jsPath.startsWith("Custom")) {
                        group = "userInformation"
                    }
                    field.group = group;
                    field.bucket = path[1];
                    var groups = $scope.form.groups;
                    var found = groups.find(function (g) {
                        return group === g.name;
                    });
                    if (!found) {
                        found = {
                            name: group,
                            label: labelFromName(group),
                            displayName: labelFromName(group),
                            fields: []
                        };
                        groups.push(found)
                    }
                    field.id = rgu.guid();
                    if (!found.hasOwnProperty('fields')) found.fields = [];
                    found.fields.push(field);
                    $scope.form.fields.push(field);
                    $scope.checkValidity('form');

                }
                else {    // Add whole group
                    var path = entry.path + '.' + entry.label;
                    index = $.inArray(entry, $scope.vaultEntries);
                    if (index < 0) return;
                    while (++index < $scope.vaultEntries.length) {
                        var field = $scope.vaultEntries[index];
                        if (field.path !== path)
                            break;
                        if (field.hasOwnProperty('nogroup') && field.nogroup)
                            continue;
                        if (!$scope.addedToForm(field)) {
                            $scope.addEntryToForm(field);
                        }
                    }
                }


            };


            var fields = [];
            var groups = [];
            if (!isBrandNew) {
                fields = interaction.fields || [];
                groups = interaction.groups || [];
                if (!groups.length) {
                }
            }

            $scope.populateGroups = function (groups) {
                if (!angular.isArray(groups) || !groups.length) {
                    $scope.form.groups = [];
                } else {
                    angular.forEach(groups, function (group) {
                        group.fields = [];
                    });
                    $scope.form.groups = groups;
                }
            };

            $scope.populateFields = function (fields) {
                if (!angular.isArray(fields)) return;
                angular.forEach(fields, function (field) {
                    field.leaf = true;
                    $scope.addEntryToForm(field, false);
                });
            };

            $scope.populateGroups(groups);
            $scope.populateFields(fields);

// if (!isNew && !groups.length) {
//     console.log($scope.form.fields);
//     $scope.form.groups = interactionFormService.softField($scope.form.fields);
//     // $scope.form.groups = interactionFormService.initFieldGroups($scope.form.fields);
//     console.log($scope.form.groups);
// }


            $scope.vaultPane = {
                searchQuery: '',
                matchedEntries: []
            };

            if (!isBrandNew) $scope.checkValidity();

            var hash = $location.hash();
            switch (hash) {
                case 'details':
                case 'publish':
                case 'form':
                case 'workflow':
                    $scope.activePage = hash;
                    break;
                default:
                    $scope.activePage = 'details';
            }

            $scope.$on('$locationChangeStart', function (event, next, current) {
                // console.log(next);
                event.preventDefault();
            });


            $scope.navTo = function (page) {
                var lastPage = $scope.activePage;
                $scope.touched[lastPage] = true;
                $scope.touched.any = true;
                $scope.touched.all = $scope.touched.details && $scope.touched.publish && $scope.touched.form;
                $scope.checkValidity(lastPage);
                $scope.activePage = page;
                $location.hash(page);
            };
            $scope.navNext = function () {
                var next;
                switch ($scope.activePage) {
                    case 'details':
                        next = 'publish';
                        break;
                    case 'publish':
                        next = isBroadcast ? 'workflow' : 'form';
                        break;
                    case 'form':
                        next = 'workflow';
                        break;
                }
                $scope.navTo(next);
            };

            $scope.vaultEntries = [];
            $scope.upload = {};
            $scope.uploadFile = function (element) {
                var file = element.files[0];
                if (!/\.(gif|jpg|jpeg|tiff|png)$/i.test(file.name)) {
                    $timeout(function () {
                        $scope.upload.imageTypeInvalid = true;
                        $scope.interaction.image = null;
                    });
                    return;
                }


                var uploadUrl = "/api/CampaignService/UploadImage";
                fileUpload.uploadFileToUrl(file, uploadUrl, function (reponse) {
                    $scope.interaction.image = reponse.fileName;
                    var input = $(element);
                    input.replaceWith(input.val('').clone(true));
                    $scope.upload.imageTypeInvalid = false;
                }, function () {

                }, "");
            };

            $scope.uploadFileConditions = function (element) {
                var file = element.files[0];
                if (!/\.(txt|doc|docx|pdf|gif|jpg|jpeg|tiff|png)$/i.test(file.name)) {
                    $timeout(function () {
                        $scope.upload.termsTypeInvalid = true;
                        $scope.interaction.termsUrl = '';
                    });
                    return;
                }


                var uploadUrl = "/api/CampaignService/UploadImage";
                fileUpload.uploadFileToUrl(file, uploadUrl, function (reponse) {
                    $scope.interaction.termsUrl = reponse.fileName;
                    var input = $(element);
                    input.replaceWith(input.val('').clone(true));
                    $scope.upload.termsTypeInvalid = false;

                }, function () {

                }, "");
            };

            $scope.deleteUploadTerms = function() {
                interaction.termsUrl = '';
            };
            $scope.$watch('interaction.name', function (oldVal, newVal) {
                $scope.checkValidity('details.name');
            });
            $scope.$watch('interaction.termsType', function (oldVal, newVal) {
                if (oldVal !== newVal) {
                    $scope.interaction.termsUrl = '';
                }
            });
            $scope.$watch('interaction.termsUrl', function (oldVal, newVal) {
                if (oldVal !== newVal) {
                    $scope.checkValidity('details.terms');
                }
                $scope.checkValidity('details.terms');
            });
            $scope.$watch('interaction.eventInfo', function (oldVal, newVal) {
                $scope.checkValidity('details.event');
            }, true);
            if (!reviewing) {
                $scope.checkAgeRange = function () {
                    if (interaction.criteria.age.type !== 'range') {
                        $scope.ageRange.invalid = false;
                        return;
                    }
                    $scope.ageRange.invalid = !interaction.criteria.age.min || interaction.criteria.age.min < 1 || !interaction.criteria.age.max
                        || interaction.criteria.age.max < interaction.criteria.age.min;
                    return $scope.ageRange.invalid;
                };
                $scope.$watch('interaction.criteria.age.min', $scope.checkAgeRange);
                $scope.$watch('interaction.criteria.age.max', $scope.checkAgeRange);

                // $scope.checkPrice = function (oldVal, newVal) {
                //     //var price = newVal;
                //     // if (oldVal === newVal) return;
                //     var price = interaction.price
                //     if (true || angular.isString(price)) {
                //         var fractions = '00';
                //         if (price.length) {
                //             var priceParts = price.split('.');
                //             if (priceParts.length < 2) {
                //                 fractions = '00';
                //             } else {
                //                 fractions = priceParts[1];
                //                 if (!fractions.length) fractions = '00';
                //                 if (fractions.length === 2) return;
                //                 else if (fractions.length === 1) fractions += '0';
                //                 else fractions = fractions.slice(0, 2);
                //             }
                //         }
                //         price = priceParts[0] + '.' + fractions;
                //         interaction.price = price;
                //     }
                // };
                // $scope.$watch('interaction.price', $scope.checkPrice);

                $scope.$watch('interaction', function (oldVal, newVal) {
                    if (oldVal !== newVal)
                        $scope.form.dirty = true;
                }, true);
                $scope.$watch('social', function (oldVal, newVal) {
                    if (oldVal !== newVal)
                        $scope.form.dirty = true;
                }, true);
            } else {
                $scope.$watch('comments', function (oldVal, newVal) {
                    if (oldVal !== newVal)
                        $scope.review.dirty = true;
                }, true);
            }


//  Prepare Vault tree for search
            $http.post('/api/CampaignService/GetVaultTreeForRegistration', {})
                .success(function (response) {
                    $scope.vaultTree = response.TreeVault;
                    interactionFormService.traverseVault($scope, $scope.vaultTree, 1, '', '');
                   // console.log($scope.vaultTree);
                })
                .error(function (errors) {
                    console.log('Error loading vault tree', errors);
                });

            function labelFromName(name) {
                return rgu.deCamelize(name);
            }

            function createFieldGroups(fields) {
                var groups = {};
                angular.forEach(fields, function (field) {
                    var group = field.group;
                    if (!groups.hasOwnProperty(group)) {
                        groups[group] = {
                            name: group,
                            label: labelFromName(group),
                            fields: []
                        }
                    }
                    groups[group].fields.push(field);
                });
                return groups;
            }

            $scope.initFieldGroups = function () {
                //interactionFormService.initFieldGroups($scope.dataregisterform);
                //$scope.groups = createFieldGroups($scope.dataregisterform);
                $scope.groups = interactionFormService.sortField($scope.form.fields);
            };

            $scope.editField = function (field) {
                field.edit = angular.copy(field);
                if (field.type === 'static') {
                    field.edit.model = field.options;
                }
            };
            $scope.stopEditField = function (field) {
                if (field.hasOwnProperty('edit'))
                    delete field.edit;
            };
            $scope.cancelField = function (field) {
                $scope.stopEditField(field);
            };
            $scope.saveField = function (field) {
                var edit = field.edit;

                if (edit.type === 'range') {
                    var ranges = edit.model[0];
                    if (!ranges[0]) {
                        ranges[0] = 1;
                    }
                    if (!ranges[1]) {
                        ranges[1] = 100;
                    }
                }
                $scope.stopEditField(field);
                angular.merge(field, edit);
                if (field.type === 'static') {
                    field.options = edit.model;
                }
                delete field.edit;
                $scope.form.formDirty = true;
                $scope.form.dirty = true;
            };
            $scope.toggleViewField = function (field) {
                if (!field.edit) $scope.editField(field);
                else $scope.stopEditField(field);
            };
            $scope.movingField = function (field) {
               field.moving = true;
            };
            $scope.movedField = function (group) {
               var index = group.fields.findIndex(function(f) {
                   return f.moving;
               });
                if (index !== -1)
                    group.fields.splice(index,1);

            };

            var qaId = 1;
            var docId = 1;

            $scope.formActions = {

                addCustomField: function (type) {
                    var isQa = type === 'qa';
                    var id = isQa ? qaId++ : docId++;
                    var field = {
                        leaf: true,
                        qa: true,
                        type: type,
                        displayName: isQa ? 'Question ' + id : 'Upload File',
                        jsPath: isQa ? 'Custom.Question.' + id : 'Custom.UploadFile.' + id,
                        path: isQa ? 'Custom.Question.' + id : 'Custom.UploadFile.' + id,
                    };
                    if (isQa) field.choices = false;
                    $scope.formActions.addEntryToForm(field);
                },
                addEntryToForm: $scope.addEntryToForm,

                addEntryToFormByPath: $scope.addEntryToFormByPath
            };


            if (isBrandNew) {
                $scope.formActions.onSelectTemplate = $scope.onSelectTemplate;
            }


            $scope.previewForm = function () {
                interaction.fields = $scope.fieldList = $scope.form.fields;
                $scope.groups = $scope.form.groups;
                formService.openInteractionForm(interaction, $scope);
            };

            $scope.previewDetails = function () {
                interaction.fields = $scope.form.fields;
                rguModal.openModal('interaction.preview', $scope, null);
            };

            $scope.isMembershipStatic = function (entry) {
                return (entry.membership && entry.type === 'static');
            };
            $scope.isDeletable = function (field) {
                return true; //!$scope.isMembershipStatic(field);
            };

            $scope.filterEntriesByQuery = function (entry) {
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

            $scope.IsvalidRange = function (field) {
                var model = field.model;
                if (field.type != "range")
                    return true;
                if (model == null || model == undefined) {
                    return false;
                }
                if (!(model instanceof Array))
                    return false;

                if (model.length <= 0)
                    return false;
                var check = true;
                $(model).each(function (index, object) {
                    if ((object[0] == undefined || object[0] == null) && (object[1] == undefined || object[1] == null))
                        check = false;

                })
                return check;
            }
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
                var entryCount = $scope.vaultPane.matchedEntries.length;
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

            $scope.hasMembership = function () {
                var found = false;
                angular.forEach($scope.form.fields, function (field) {
                    if (field.membership && field.type !== 'static') {
                        found = true;
                    }
                });
                return found;
            };
            $scope.checkMembership = function () {
                return;
                if ($scope.hasMembership()) {
                    angular.forEach($scope.vaultEntries, function (entry) {
                        if ($scope.isMembershipStatic(entry)) {
                            $scope.addEntryToForm(entry, false);
                        }
                    });

                } else {
                    angular.forEach($scope.form.fields, function (field) {

                        if ($scope.isMembershipStatic(field)) {

                            $scope.deleteField(field, false);
                        }
                    });
                }
            };

            $scope.formFieldPath = function (jspath) {
                if (!path) return '';
                var pathParts = path.split('.');
                return pathParts[1];
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
            $scope.addEntryToFormByPath = function (template) {
                var jsPath = template.jsPath;
                if ($scope.addedToFormByPath(jsPath))
                    return;

                function findEntryByPath(jsPath) {
                    var foundEntry = null;
                    angular.forEach($scope.vaultEntries, function (entry) {
                        if (jsPath === entry.jsPath) {
                            foundEntry = entry;
                        }
                    });
                    return foundEntry;
                }

                var entry = findEntryByPath(jsPath);

                if (!entry) return;
                var field = angular.copy(entry);
                field.displayName = template.label;
                field.optional = !!template.optional;
                $scope.addEntryToForm(field);

            };
            $scope.addEntryToFormByIndex = function (index) {

                var entry = $scope.vaultPane.matchedEntries[index];
                $scope.addEntryToForm(entry, true);
            };
            $scope.deleteField = function (field) {
                var index = $scope.form.fields.findIndex(function (f) {
                    return f.jsPath === field.jsPath;
                });
                if (index >= 0) {
                    $scope.form.fields.splice(index, 1);
                    $scope.checkValidity('form');

                    var group = $scope.form.groups.find(function (g) {
                        return field.group === g.name;
                    });
                    if (!group) return;
                    index = group.fields.findIndex(function (f) {
                        return f.jsPath === field.jsPath;
                    });
                    if (index >= 0) {
                        group.fields.splice(index, 1);
                        if (!group.fields.length) {
                            index = $scope.form.groups.findIndex(function (g) {
                                return g.name === group.name;
                            });
                            if (index >= 0) {
                                $scope.form.groups.splice(index, 1);

                            }
                        }
                    }
                    // if (checkMembership) {
                    //     $scope.checkMembership();
                    // }
                }
            };
            $scope.deleteAllFields = function () {
                $scope.form.fields.splice(0);
                $scope.form.groups.splice(0);
            };
            $scope.sendReview = function () {
                $scope.saveInteraction('review');
            };
            $scope.sendFeedback = function () {
                $scope.saveInteraction('feedback');
            };
            $scope.approve = function (editing) {
                $scope.saveInteraction(editing ? 'approve-self' : 'approve');
            };

            $scope.saveInteraction = function (mode) {
                var i = $scope.interaction;

                var status = 'Saved';

                if ($scope.interaction.status === 'Template') {
                    mode = 'template';
                }
                switch (mode) {
                    case 'review':
                    case 'feedback':
                        status = "Pending";
                        break;
                    case 'template':
                        status = "Template";
                        break;
                    case 'approve':
                    case 'approve-self':
                        status = "Active";
                        break;
                }

                var everyone = i.target === 'Everyone';
                if (everyone || i.criteria.age.type === 'Any Age') {
                    i.criteria.age.max = 100;
                    i.criteria.age.min = 1;
                } else {
                    if (!i.criteria.age.min) i.criteria.age.min = 1;
                    if (!i.criteria.age.max) i.criteria.age.max = i.criteria.age.min;
                }
                if (everyone || i.criteria.location.type === 'Global') {
                    i.criteria.location.country = '';
                    i.criteria.location.area = '';
                }

                if (!i.paid) {
                    var price = '';
                }
                else {
                    price = i.price.toString();
/*                    if (angular.isString(price)) {
                        var fractions = '00';
                        if (price.length) {
                            var priceParts = price.split('.');
                            if (priceParts.length < 2) {
                                fractions = '00';
                            } else {
                                fractions = priceParts[1];
                                if (!fractions.length) fractions = '00';
                                else if (fractions.length === 1) fractions += '0';
                                else fractions = fractions.slice(0, 2);
                            }
                        }
                        price = priceParts[0] + '.' + fractions;
                    }*/
                }

                var c = {
                    type: type === 'broadcast' ? 'Advertising' : (type === 'srfi' ? 'SRFI' : (type === 'pushtovault' ? 'PushToVault' : $scope.Type)),
                    status: status,
                    name: i.name,
                    description: i.description,
                    image: i.image,
                    targetLink: i.url,
                    termsType: i.termsType,
                    termsUrl: i.termsUrl,
                    target: i.target,
                    distribute: i.distribute,
                    boost: i.boost,
                    socialShare: $scope.social.sharing ? $scope.social.providers : '',
                    indefinite: i.indefinite,
                    criteria: {
                        gender: i.criteria.gender,
                        age: {
                            type: i.criteria.age.type,
                            min: i.criteria.age.min,
                            max: i.criteria.age.max
                        },
                        location: {
                            type: i.criteria.location.type,
                            country: i.criteria.location.country,
                            area: i.criteria.location.area
                        },
                        spend: {
                            type: i.indefinite ? 'Daily' : 'Duration',
                            effectiveDate: i.from,
                            endDate: i.until
                        }
                    },
                    paid: i.paid,
                    price: price || '',
                    priceCurrency: i.priceCurrency || '',
                    verb: i.verb,
                    notes: i.notes,
                    participants: i.participants.toString()
                };
                if (!i.paid) {
                    c.usercodetype = 'Free';
                } else {
                    c.usercodetype = 'Paid';
                    c.usercode = i.price.toString();
                    c.usercodecurrentcy = i.priceCurrency;
                }
                if (isEvent) {
                    var e = interaction.eventInfo;
                    c.event = {
                        startdate: e.fromDate,
                        starttime: e.fromTime,
                        enddate: e.toDate,
                        endtime: e.toTime,
                        theme: e.theme,
                        location: e.location
                    };
                }
                var comments = [];
                angular.forEach($scope.comments, function (comment, cat) {

                    if (!comment.length) return;
                    comments.push({
                        type: 'review',
                        category: cat,
                        creatorId: '',
                        text: comment
                    });
                });
                if (comments.length) {
                    c.comments = comments;
                }
                c.fields = [];
                angular.forEach($scope.form.groups, function (g) {
                    angular.forEach(g.fields, function (f) {
                        var field = {
                            id: f.id,
                            displayName: f.displayName,
                            jsPath: f.jsPath,
                            path: f.path,
                            label: f.label,
                            optional: f.optional === true,
                            type: f.type,
                            options: f.options,
                            value: f.value,
                            choices: f.choices,
                            qa: f.qa,
                            group: g.name || g.displayName
                        };
                        if ((f.type === "qa" && f.choices || f.type === 'range')) {
                            if (f.type === 'range' && !f.model) {
                                f.model = [[1,100]];
                            }
                            field.model = f.model;
                        }
                        c.fields.push(field);

                    });
                });
                //return;
                // c.fields = $.map($scope.form.fields, function(f, index) {
                //     var field = {
                //         id: f.id,
                //         order: index,
                //         displayName: f.displayName,
                //         jsPath: f.jsPath,
                //         path: f.path,
                //         label: f.label,
                //         optional: !!f.optional,
                //         type: f.type,
                //         options: f.options,
                //         value: f.value,
                //         model: f.model,
                //         unitModel: f.unitModel,
                //         membership: f.membership,
                //         choices: f.choices,
                //         qa: f.qa
                //     };
                //     return field;
                // });
                c.groups = $.map($scope.form.groups, function (g) {
                    var group = {
                        name: g.name,
                        displayName: g.displayName
                    };
                    return group;
                });
                var campaign = {
                    userId: i.business.accountId,
                    campaign: c
                };
                var json = angular.toJson(campaign);
                var model = {json: json};
                if (!isNew) {
                    model.id = interaction.id
                }
                var verb = isNew ? 'New' : 'Save';
                // $http.post('api/campaign', campaign);
                //console.log(interaction.id);
                $http.post('/api/Interactions/' + verb, model)
                    .success(function (response) {
                        if (!mode) {
                            rguNotify.add('Saved interaction "' + interaction.name + '"');
                            $scope.form.dirty = false;
                        } else if (mode === 'review') {
                            rguNotify.add('Saved and sent interaction "' + interaction.name + '" for review. Waiting for approval.');
                        } else if (mode === 'feedback') {
                            rguNotify.add('Saved comments for interaction "' + interaction.name + '".');
                        } else if (mode === 'template') {
                            rguNotify.add('Saved interaction template "' + interaction.name + '".');
                        } else if (mode === 'approve') {
                            rguNotify.add('Approved interaction "' + interaction.name + '".');
                        } else if (mode === 'approve-self') {
                            rguNotify.add('Saved and approved interaction "' + interaction.name + '".');
                        }
                        if (true || mode) {
                            location.href = '/Campaign/ManagerCampaign';
                        }
                    }).error(function (errors) {
                    console.log('Error saving interaction', errors);
                    rguAlert('Error saving interaction. Please try again later', 'error');
                });


            };

            $scope.exportQR = function (event) {
                var canvas = document.querySelector('qr canvas');
                var link = event.target;
                link.href = canvas.toDataURL();
                link.download = 'QR-' + interaction.id + '.png';
            }

        }
    ])

    .controller('FormFieldEditorController', ['$scope', function ($scope) {
        var field = $scope.field;
        $scope.isStatic = field.type === 'static';
        $scope.$watch('field.edit', function (oldVal, newVal) {
            if (oldVal !== newVal) {
                field.edit.dirty = true;
            }
        }, true);
    }]);

