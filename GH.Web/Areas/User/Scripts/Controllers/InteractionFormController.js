﻿var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule'], true);

myApp.controller('InteractionFormController',
    ['$scope', '$rootScope', '$timeout', '$uibModal', '$http', '$moment', 'rgu', 'rguCache', 'rguNotify', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'NotificationService', 'notificationService', 'SmSAuthencationService', 'fileUpload', 'interactionFormService', 'formService',
        function ($scope, $rootScope, $timeout, $uibModal, $http, $moment, rgu, rguCache, rguNotify, userManager, sweetAlert, authService, alertService, notificationService, notifService, SmSAuthencationService, fileUpload, interactionFormService, formService) {

            var type = $scope.type = $scope.interaction.type;
            var previewing = !!$scope.interaction.editing;
            var isEvent = $scope.isEvent = type === 'event';
            var isBroadcast = $scope.isBroadcast = type === 'broadcast';
            var isHandshake = $scope.isHandshake = type === 'handshake';
            var isFeedBased = $scope.isFeedBased = isBroadcast || type === 'registration' || type === 'event';

            $scope.verb = $scope.interaction.verb || 'register';
            $scope.verbPast = formService.verbPast($scope.verb);
            $scope.verbGerund = formService.verbGerund($scope.verb);
            $scope.messageBox = "";
            $scope.alerts = [];
            $scope.dataregisterform = $scope.fieldList;
            $scope.BusinessUserId = $scope.interaction.business.accountId;
            $scope.CampaignId = $scope.interaction.id;
            $scope.CampaignType = $scope.interaction.type;

            $scope.view = {
                initializing: !previewing
            };

            $scope.missingInfomationOfVault = false;
            var ownDelegation = {
                isMe: true,
                FromUserDisplayName: "Myself (" + regitGlobal.userAccount.displayName + ')',
                fromAvatar: regitGlobal.userAccount.avatar
            };
            $scope.ListManagerDelegatedTo = $scope.delegations = [ownDelegation];

            $scope.changeActor = function () {

            };
            /*            $scope.getDelegationDetails = function (delegationId) {
                            $http.get('/api/Delegation/List/In')
                                .success(function (response) {
                                    if (!response.success) {
                                        console.log('Error getting delegation details', response);
                                        return;
                                    }
                                    $scope.delegations = $scope.delegations.concat(response.data);
                                    console.log($scope.delegations);
                                })
                                .error(function (response) {
                                    console.log('Error getting delegation details', response);
                                });
                        };

                        $scope.getDelegationDetails();*/

            $scope.getInteractionDetails = function (delegationId) {
                $http.get('/api/Interaction/Details', {
                    params: {
                        interactionId: $scope.interaction.id,
                        delegationId: delegationId
                    }
                })
                    .success(function (response) {
                        if (!response.success) {
                            console.log('Error getting interaction details', response);
                            return;
                        }
                        $scope.fields.splice();
                        $scope.fields = response.data.form.fields;
                        $scope.formData = response.data.formData;
                        var i = response.data;
                        $scope.interaction.name = i.name;
                        $scope.interaction.type = i.type;
                        $scope.interaction.description = i.description;
                        $scope.interaction.paid = i.paid;
                        $scope.interaction.price = i.price;
                        $scope.interaction.priceCurrency = i.priceCurrency;
                        $scope.interaction.termsUrl = i.termsUrl;
                        $scope.interaction.verb = i.verb;
                        $scope.verb = $scope.interaction.verb || 'register';
                        $scope.verbPast = formService.verbPast($scope.verb);
                        $scope.verbGerund = formService.verbGerund($scope.verb);

                        var type = $scope.type = $scope.interaction.type;
                        var isEvent = $scope.isEvent = type === 'event';
                        var isBroadcast = $scope.isBroadcast = type === 'broadcast';
                        var isHandshake = $scope.isHandshake = type === 'handshake';
                        var isFeedBased = $scope.isFeedBased = isBroadcast || type === 'registration' || type === 'event';

                        if (i.participated) {
                            var actor = 'self';
                            if ($scope.DelegationSelected.isMe) {
                                if (!i.participation.delegateeId)
                                    actor = 'self';
                                else if (i.participation.delegateeId === regitGlobal.userAccount.accountId)
                                    actor = 'for';
                                else {
                                    actor = 'by';
                                }
                            } else {
                                actor = i.participation.delegateeId === regitGlobal.userAccount.accountId ? 'for' : 'other';

                            }

                            $scope.currentParticipation = {
                                actor: actor,
                                actorName: $scope.DelegationSelected.FromUserDisplayName,
                                delegationId: actor === 'self' ? '' : $scope.DelegationSelected.DelegationId,
                                participated: i.participation.participatedAt
                            };
                            if (actor === 'by')
                                rguCache.getUserAsync(i.participation.delegateeId).then(function (response) {
                                    $scope.currentParticipation.actorName = response.displayName;
                                });

                        } else {
                            $scope.currentParticipation = null;
                        }
                        $scope.interaction.participated = i.participated;
                        $scope.interaction.delegationId = delegationId;

                        $scope.mode.editingForm = false;

                        if (i.request) {
                            $scope.comment = response.data.request.comment;
                        }

                        //  Init field groups
                        $scope.groups.splice();
                        $scope.groups = response.data.form.groups;

                        $scope.groups.forEach(function (g) {
                            g.fields = [];
                        });

                        var values = response.data.form.userData;
                        $scope.fields.forEach(function (field) {
                            if (field.type === 'qa')
                                field.choices = angular.isArray(field.options);
                            var value = values.find(function (val) {
                                return val.path === field.path;
                            });
                            if (angular.isDefined(value)) {
                                value = value.value;
                                switch (field.type) {
                                    case 'textbox':
                                        if (field.options === 'phone' && value) {
                                            if (value.substring(0, 1) !== '+')
                                                value = '+' + value;
                                        }
                                        break;
                                    case 'doc':
                                        if (value.length) {
                                            value[0].selected = true;
                                        }
                                        break;
                                }
                            }
                            field.value = field.model = value || '';
                            field.selected = true;
                            var group = $scope.groups.find(function (g) {
                                return g.name === field.group;
                            });
                            if (group) {
                                group.fields.push(field);
                            }
                        });

                        $scope.form.agreed = false;

                        $scope.validate.invalid = false;
                        $scope.validate.noValue = false;
                        $scope.validate.dirty = false;

                        $scope.view.initializing = false;
                        $scope.view.missingReadonlyFields = !i.participated && formService.missingReadonlyFields($scope.fields);


                    })
                    .error(function (response) {
                        console.log('Error getting interaction details', response);
                    });
            };

            if ($scope.interaction.editing) {
                //$scope.fields.splice();
                $scope.fields = $scope.interaction.fields;
            } else {
                if ($scope.fields)
                    $scope.fields.splice();
                $scope.fields = [];
                $scope.groups = [];
                //$scope.getInteractionDetails();
            }

            function labelFromName(name) {
                return rgu.deCamelize(name);
            }

            function createFieldGroups2(fields) {
                var groups = $scope.groups || {};
                angular.forEach(fields, function (field) {
                    var group = field.group;
                    if (!groups.hasOwnProperty(group)) {
                        groups[group] = {
                            name: group,
                            label: labelFromName(group),
                            displayName: labelFromName(group),
                            fields: []
                        }
                    }
                    groups[group].fields.push(field);
                });
                return groups;
            }

            $scope.initFieldGroups = function () {
                // interactionFormService.initFieldGroups($scope.dataregisterform);
                //createFieldGroups($scope.dataregisterform);
                $scope.groups = interactionFormService.sortField($scope.dataregisterform);
            };
            if (!previewing) {
                // $scope.initFieldGroups();
            }

            $scope.form = {
                agreed: false
            };
            $scope.mode = {
                editingForm: previewing,
                delegated: false,
                regOnly: false,
                viewOnly: false,
                super: false,
                preview: previewing
            };

            $scope.validate = {
                invalid: false,
                noValue: false,
                dirty: false
            };


            $scope.fieldsNoValue = function () {
                return formService.fieldsNoValue($scope.fields);
            };

            $scope.fieldsInvalid = function () {
                return !formService.validateAllFields($scope.fields);
            };
            $scope.canRegister = function () {
                //console.log($scope.validate);
                return $scope.form.agreed && !$scope.validate.invalid;
                // return $scope.form.agreed && $scope.fieldsInvalid();
            };

            function watchValidate() {
                $scope.validate.invalid = $scope.fieldsInvalid();
                $scope.validate.noValue = $scope.fieldsNoValue();
                $scope.validate.dirty = false;
            }

            $scope.$watch('validate.dirty', watchValidate);

            $scope.delegation = {
                selecting: false,
                selected: null
            };

            $scope.toggleSelectDelegator = function () {
                $scope.delegation.selecting = !$scope.delegation.selecting;
            };
            $scope.selectDelegator = function (del) {
                if (previewing) return;
                $scope.delegation.selected = $scope.DelegationSelected = del;
                $scope.delegation.selecting = false;
                var delegationId = del.isMe ? '' : del.DelegationId;
                $scope.mode.regOnly = del.DelegationRole === 'Normal';

                $scope.getInteractionDetails(delegationId);
            };

            $scope.changedelegate = function () {

                var data = {};
                $scope.Mode = "Read";
                $scope.mode.editingForm = false;
                $scope.mode.regOnly = $scope.mode.super = false;
                $scope.mode.viewOnly = false;
                $scope.form.agreed = false;

                data.UserId = $scope.DelegationSelected.FromAccountId;
                data.BusinessUserId = $scope.interaction.business.accountId;
                data.CampaignType = $scope.interaction.type;
                data.CampaignId = $scope.interaction.id;
                $scope.Isregis = true;
                $scope.isRegister = false;
                $scope.currentParticipation = null;


                //$scope.listFollower
                for (var i = 0; i < $scope.listFollower.length; i++) {
                    if ($scope.listFollower[i].UserId === $scope.DelegationSelected.FromAccountId) {
                        $scope.isRegister = true;

                    }
                }


                $scope.mode.delegated = true;

                var isvalid = true;

                if ($scope.DelegationSelected.isMe) {
                    $scope.isRegister = $scope.isMyRegister;
                    $scope.mode.delegated = false;
                } else if ($scope.DelegationSelected.DelegationRole === "Normal") {
                    $scope.IsReadOnly = true;
                    $scope.missingInfomationOfVault = false;
                    $scope.mode.regOnly = true;
                    alertService.closeAlert();
                } else if ($scope.DelegationSelected.DelegationRole === "Super") {
                    $scope.mode.super = true;
                } else {
                    $scope.IsReadOnly = false;
                    $scope.missingInfomationOfVault = false;
                    alertService.closeAlert();
                }
                if ($scope.DelegationSelected.DelegationRole === "Custom") {
                    $scope.Iscustomrole = true;
                }
                else {
                    $scope.Iscustomrole = false;
                }

                if (angular.isArray($scope.interaction.participations)) {
                    // console.log($scope.interaction.participations);
                    angular.forEach($scope.interaction.participations, function (participation) {
                        if (participation.delegationId === $scope.DelegationSelected.DelegationId
                            || !$scope.mode.delegated && participation.actor !== 'for') {
                            $scope.currentParticipation = participation;
                        }
                    });
                } else if (true || $scope.isRegister) {
                    $http.get('/api/Interaction/Participation/' + $scope.interaction.id)
                        .success(function (response) {
                            if (!response.Actor) return;
                            var participation = {
                                actor: response.Actor,
                                delegationId: response.DelegationId,
                                participated: new Date(response.Participated)
                            };
                            $scope.interaction.participations = [
                                participation
                            ];
                            $scope.currentParticipation = participation;
                            $scope.mode = {viewOnly: true};
                            //$scope.isRegister =  $scope.isMyRegister = true;


                        })
                        .error(function (errors) {
                        });
                }

                if ($scope.isRegister || !$scope.mode.delegated && $scope.currentParticipation) {
                    $scope.mode.viewOnly = true;
                }

            };
            // end

            $scope.selectDelegator(ownDelegation);


            $scope.cancel = function () {
                $scope.closeForm();
            };

            $scope.Edit = function () {
                SmSAuthencationService.OpenPopupAuthencationPIN(function () {
                    $scope.Mode = "Edit";
                    $scope.mode.editingForm = true;
                });
            };

            $scope.uploadFileReg = function (element) {
                var file = element.files[0];

                var jsPath = element.id;
                var uploadUrl = "/api/CampaignService/UploadFileRegisform?jsPath=" + jsPath;
                fileUpload.uploadFileToUrl(file, uploadUrl, function (reponse) {
                    var jsonpath = $(element).attr("jsonpath");
                    $($scope.dataregisterform).each(function (index, field) {
                        if (field.jsPath == jsonpath) {
                            field.model.push({fname: reponse.fileName});
                        }
                    });
                    var input = $(element);
                    input.replaceWith(input.val('').clone(true));

                }, function () {

                }, "");
            };

            $scope.uploadFile = function (element) {
                var file = element.files[0];
                var uploadUrl = "/api/CampaignService/UploadFileRegisform";
                fileUpload.uploadFileToUrl(file, uploadUrl, function (reponse) {
                    console.log(reponse)
                    var jsonpath = $(element).attr("jsonpath");
                    $($scope.fields).each(function (index, field) {
                        if (field.path === jsonpath) {
                            field.model.push({filename: reponse.fileName});
                        }
                    });
                    var input = $(element);
                    input.replaceWith(input.val('').clone(true));

                }, function () {

                }, "");
            };
            $scope.changedocument = function (path) {
                $($scope.dataregisterform).each(function (index, field) {
                    if (field.jsPath == path) {
                        field.modeldocvault = [];
                        field.modeldocvault.push({fname: field.docselect.fname});
                    }
                });

            }
            $scope.uploadFileDocQA = function (element) {
                var file = element.files[0];

                var uploadUrl = "/api/CampaignService/UploadFileRegisform";
                fileUpload.uploadFileToUrl(file, uploadUrl, function (reponse) {
                    var path = $(element).attr("path");
                    $($scope.dataregisterform).each(function (index, field) {
                        if (field.jsPath == path) {
                            field.modeldocvault = [];
                            field.modeldocvault.push({fname: reponse.FileName});
                        }
                    });
                    var input = $(element);
                    input.replaceWith(input.val('').clone(true));

                }, function () {

                }, "");
            };
            $scope.deleteDoc = function (model, index) {
                model.splice(index, 1);
            };

            $scope.Register = function () {

                if (previewing) return;

                var fields = [];
                $scope.fields.forEach(function (f) {
                    if (f.optional && !f.selected) return;
                    var value = f.model;
                    switch (f.type) {
                        case "date":
                        case "datecombo":
                            value = $moment(f.model).format('YYYY-MM-DD');
                            break;
                        case 'smartinput':
                        case 'tagsinput':
                            if (angular.isString(value)) value = f.model.split(',');
                            break;

                        case 'doc':
                            var docs = [];
                            f.model.forEach(function (d) {
                                if (d.selected)
                                    docs.push({
                                        filePath: d.filePath.replace(/\/$/, ''),
                                        fileName: d.fileName
                                    })
                            });
                            value = docs;
                            break;
                    }
                    //console.log(f, field);
                    var field = {
                        source: f.type === 'qa' || f.type === 'range' || $scope.mode.editingForm ? 'newValue' : 'vaultCurrentValue',
                        path: f.path,
                        value: value
                    };
                    fields.push(field);
                });

                // console.log(fields);

                $http.post('/Api/Interaction/Register', {
                    interactionId: $scope.interaction.id,
                    delegationId: $scope.DelegationSelected.DelegationId,
                    fields: fields
                })
                    .success(function (response) {
                        if (!response.success) {
                            console.log('Error participating in interaction:', response.message);
                            return;
                        }

                        var participation = {
                            actor: $scope.DelegationSelected.DelegationId ? 'for' : 'self',
                            actorName: $scope.DelegationSelected.DelegationId ? $scope.DelegationSelected.FromUserDisplayName : 'you',
                            participated: new Date()
                        };
                        if ($scope.mode.delegated) {
                            participation.delegationId = $scope.DelegationSelected.DelegationId;
                        }
                        if (!angular.isArray($scope.interaction.participations)) {
                            $scope.interaction.participations = [];
                        }
                        $scope.interaction.participations.push(participation);
                        $scope.interaction.participants++;
                        $scope.interaction.participated = true;
                        $rootScope.$broadcast('reloadminicalendar');
                        $rootScope.$broadcast('reloadfollowstatus');
                        $rootScope.$broadcast('interaction:participated');
                        rguNotify.add(($scope.interaction.type === 'srfi' ? 'Submitted information for "' : 'Participated in "') + $scope.interaction.name + '"');

                        $scope.closeForm();
                        if (isHandshake)
                            notifService.resolveRequestById('handshake', $scope.CampaignId);
                    })
                    .error(function (response) {
                        console.log('Error participating in interaction', response);
                    });
                $scope.closeForm();

                notifService.resolveRequestById('interaction', $scope.interaction.id);

                // $scope.BusinessIdList.push($scope.BusinessUserId);
            };

            $scope.deRegister = function () {
                var data = {};
                data.BusinessUserId = $scope.interaction.business.accountId;
                data.CampaignId = $scope.interaction.id;
                data.CampaignType = $scope.interaction.type;
                data.UserId = $scope.DelegationSelected.FromAccountId;
                if ($scope.mode.delegated) {
                    data.DelegateeId = regitGlobal.userAccount.accountId;
                }
                //console.log($scope.interaction.participations, $scope.currentParticipation);
                $http.post("/api/CampaignService/DeRegisterCampaign", data)
                    .success(function (response) {
                        $scope.interaction.participations = $.grep($scope.interaction.participations, function (participation) {
                            return (participation.actor !== $scope.currentParticipation.actor);
                        });
                        if (!$scope.interaction.participations.length) {

                        }
                        $scope.currentParticipation = null;
                        $scope.interaction.participants--;
                        $scope.interaction.participated = false;
                        $scope.closeForm();
                        rguNotify.add(($scope.interaction.type === 'srfi' ? 'Cancelled submission for "' : 'Cancelled participation in "') + $scope.interaction.name + '"')
                    })
                    .error(function (errors, status) {
                    });
            };


            $scope.closeForm = function () {
                if (angular.isFunction($scope.hideFunc)) {
                    $scope.hideFunc();
                }
                $scope.$hide();
            };


            $scope.filterRequired = function (field) {
                return !field.optional && !field.membership && !field.qa;
            };
            $scope.filterOptional = function (field) {
                return !!field.optional;
            };


            $scope.notifyMissingInformationOfVault = function () {
                var campaignid = $scope.CampaignId;
                var delegatorId = $scope.DelegationSelected.FromAccountId;
                $http.get('/api/Notifications/NotifyMissingInformationOfVault?campaignId=' + campaignid + '&delegatorId=' + delegatorId)
                    .success(function (response) {
                        // $uibModalInstance.close();
                        $scope.closeForm();
                        rguNotify.add('Forwarded registration to delegator for follow-up');
                    })
                    .error(function (errors) {
                    });
            };

            $scope.getListDelegatedTo = function () {
                var delegationModelView = new Object();
                delegationModelView.Direction = "DelegationIn";
                $http.post('/api/DelegationManager/GetListDelegation', delegationModelView)
                    .success(function (response) {
                        $(response.Listitems).each(function (index, delegate) {
                            if (delegate.Status === "Accepted") {
                                rguCache.getUserAsync(delegate.FromAccountId).then(function (user) {
                                    delegate.fromAvatar = user.avatar;
                                });
                                $scope.ListManagerDelegatedTo.push(delegate);
                            }
                        });
                    })
                    .error(function (errors, status) {
                    });
            }
            $scope.CheckRegister = function () {
                var campaign = new Object();
                campaign.CampaignId = $scope.CampaignId;
                $http.post('/api/DelegationManager/CheckRegisterByCampaignId', campaign)
                    .success(function (res) {
                        $scope.isRegister = res;
                        $scope.isMyRegister = res;
                    })
                    .error(function (errors, status) {
                    });
            }
            $scope.getListDelegatedTo();

        }
    ]);

myApp.controller('InteractionFormViewerController',
    ['$scope', '$rootScope', '$timeout', '$uibModal', '$http', 'rgu', 'rguCache', 'rguNotify', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'NotificationService', 'notificationService', 'SmSAuthencationService', 'fileUpload', 'interactionFormService', 'formService',
        function ($scope, $rootScope, $timeout, $uibModal, $http, rgu, rguCache, rguNotify, userManager, sweetAlert, authService, alertService, notificationService, notifService, SmSAuthencationService, fileUpload, interactionFormService, formService) {
            var interaction = $scope.interaction;
            if (!angular.isObject($scope.formViewer)) {
                $scope.formViewer = {};
            }
            if (!angular.isString($scope.formViewer.mode)) {
                $scope.formViewer.mode = 'values';
            }
            $scope.titleSuffix = '';
            $scope.mode = {};
            switch ($scope.formViewer.mode) {
                case 'fields':
                    $scope.mode.viewOnly = true;
                    $scope.mode.fieldsOnly = true;
                    $scope.titleSuffix = 'List of Fields';
                    break;
                case 'values':
                    $scope.mode.viewOnly = true;
                    $scope.mode.viewValues = true;
                    $scope.titleSuffix = 'Member Updated Data';
                    break;
            }
            $scope.closeForm = function () {
                if (angular.isFunction($scope.hideFunc)) {
                    $scope.hideFunc();
                }
                $scope.$hide();
            };
            $scope.closeViewer = function (hideFunc) {
                $scope.closeForm(hideFunc);
            };

            var groups = $scope.groups || [];
            if (angular.isArray(groups) && groups.length) {
                groups.forEach(function (g) {
                    g.fields = [];
                });
                $scope.interaction.fields.forEach(function (f) {

                    f.group = '';
                    var path = f.jsPath.split('.');
                    var group = path[1];
                    if (group === 'address' || group === 'financial' || group === 'governmentID' || group === 'others') {
                        group = path[2];
                    }
                    groups.forEach(function (g) {

                        if (g.name === 'userInformation' && (f.type === 'qa' || f.type === 'doc' || !f.jsPath || f.jsPath.startsWith('Custom'))) {
                            f.group = g.name;
                            g.fields.push(f);
                        } else if (group === g.name) {
                            f.group = g.name;
                            g.fields.push(f);

                        }
                    });
                });
            } else {
                $scope.groups = interactionFormService.sortField($scope.interaction.fields);
                //$scope.restructFormFields();
            }

        }
    ]);

