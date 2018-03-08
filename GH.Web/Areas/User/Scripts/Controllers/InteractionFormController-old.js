﻿var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule'], true);

myApp.controller('InteractionFormController',
    ['$scope', '$rootScope', '$timeout', '$uibModal', '$http', 'rgu', 'rguCache', 'rguNotify', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'NotificationService', 'notificationService', 'SmSAuthencationService', 'fileUpload', 'interactionFormService', 'formService',
        function ($scope, $rootScope, $timeout, $uibModal, $http, rgu, rguCache, rguNotify, userManager, sweetAlert, authService, alertService, notificationService, notifService, SmSAuthencationService, fileUpload, interactionFormService, formService) {

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

            $scope.Iscustomrole = false;
            $scope.Mode = "Read";
            $scope.IsReadOnly = false;
            $scope.Isregis = true;
            $scope.listFollower = [];
            $scope.isRegister = false;
            $scope.isMyRegister = false;

            $scope.missingInfomationOfVault = false;
            var ownDelegation = {
                isMe: true,
                FromUserDisplayName: "Myself (" + regitGlobal.userAccount.displayName + ')',
                fromAvatar: regitGlobal.userAccount.avatar
            };
            $scope.ListManagerDelegatedTo = [ownDelegation];


            function labelFromName(name) {
                return rgu.deCamelize(name);
            }

            function createFieldGroups(fields) {
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
                $scope.initFieldGroups();
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
                return formService.fieldsNoValue($scope.dataregisterform);
            };
            $scope.fieldsInvalid = function () {
                return !formService.validateAllFields($scope.dataregisterform);
            };
            $scope.canRegister = function () {
                return $scope.form.agreed && !$scope.validate.invalid;
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
                $scope.changedelegate();
            };

            $scope.cancel = function () {
                $scope.closeForm();
            };

            $scope.Edit = function () {
                SmSAuthencationService.OpenPopupAuthencationPIN(function () {
                    $scope.Mode = "Edit";
                    $scope.mode.editingForm = true;
                });
            };

            $scope.checkterm = function (term) {
                if (term == null)
                    return false;
                if (term.search(".") >= 0)
                    return true;
                else return false;
            }
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

            $scope.restructFormFields = function () {
                $($scope.dataregisterform).each(function (index, field) {
                    switch ($scope.dataregisterform[index].type) {
                        case "doc":
                            if ($scope.dataregisterform[index].qa) {
                                $scope.dataregisterform[index].modeldocvault = [];
                            }
                            break;
                    }
                })
            }

            //$scope.restructFormFields();
            $scope.Register = function () {

                if (previewing) return;
                if ($scope.Mode === "Edit") {
                    $scope.UpdateVaultInformation();
                }
                else {
                    $scope.RegisterCampaign();
                }
                $scope.closeForm();

                notifService.resolveRequestById('interaction', $scope.interaction.id);

                // $scope.BusinessIdList.push($scope.BusinessUserId);
            };

            $scope.deRegister = function () {
                var data = new Object();
                data.BusinessUserId = $scope.interaction.business.accountId;
                data.CampaignId = $scope.interaction.id;
                data.CampaignType = $scope.interaction.type;
                data.UserId = $scope.DelegationSelected.FromAccountId;
                if ($scope.mode.delegated) {
                    data.DelegateeId = regitGlobal.userAccount.accountId;
                }
                $http.post("/api/CampaignService/DeRegisterCampaign", data)
                    .success(function (response) {
                        $scope.interaction.participations = $.grep($scope.interaction.participations, function (participation) {
                            return ( participation !== $scope.currentParticipation );
                        });
                        if (!$scope.interaction.participations.length) {

                        }
                        $scope.interaction.participants--;
                        $scope.closeForm();
                        rguNotify.add(($scope.interaction.type === 'srfi' ? 'Cancelled submission for "' : 'Cancelled participation in "') + $scope.interaction.name + '"')
                    })
                    .error(function (errors, status) {
                    });
            };

            $scope.vaildfiled = function (field) {

                var isvalid = true;
                if ((field.model == "" || field.model == undefined || field.model == null) && field.options !== 'indef') {
                    isvalid = false;
                }
                switch (field.type) {
                    case "doc":
                        isvalid = (angular.isArray(field.model) && field.model.length)
                            || (field.hasOwnProperty('modeldocvault') && field.modeldocvault.length);
                        break;
                    case "address":
                        if (field.model == null || field.model.address == "")
                            isvalid = false;
                        break;
                    case "location":
                        if (field.model == null || field.model.country == "" || (field.model.city == null && field.options != 'nocity'))
                            isvalid = false;
                        break;
                    case "date":
                        if (field.model === "Invalid Date")
                            isvalid = false;
                        break;
                }
                if (field.optional && !field.selected)
                    isvalid = true;
                return isvalid;
            }

            $scope.RegisterHandshake = function () {
                var data = new Object();
                data.UserId = $scope.DelegationSelected.FromAccountId;
                data.BusinessUserId = $scope.BusinessUserId;
                data.CampaignId = $scope.CampaignId;
                data.CampaignType = $scope.CampaignType;
                data.Listvaults = [];
                //Listvaults

                $($scope.dataregisterform).each(function (index) {
                    if ($scope.dataregisterform[index].selected) {
                        var field = {
                            id: $scope.dataregisterform[index].id,
                            jsPath: $scope.dataregisterform[index].jsPath
                        }
                        data.Listvaults.push(field);
                    }
                });
                // $http.post('/api/CampaignService/RegisterCampaign', data)
                data.isEdit = $scope.Mode === "Edit" ? true : false;
                // console.log($scope.dataregisterform)
                $http.post('/api/CampaignService/RegisterCampaignToHandShake', data)
                    .success(function (response) {
                        $uibModalInstance.close(response);
                        notificationService.resolveRequestById('handshake', $scope.CampaignId);
                    })
                    .error(function (errors, status) {
                        $uibModalInstance.dismiss('cancel');
                    });


            };


            $scope.RegisterCampaign = function () {


                var fromNotification = angular.isObject($scope.interactionForm) && $scope.interactionForm.hasOwnProperty('source') && $scope.interactionForm.source === 'notification';

                var data = {};
                data.UserId = $scope.DelegationSelected.FromAccountId;
                data.BusinessUserId = $scope.interaction.business.accountId;
                data.CampaignId = $scope.interaction.id;
                data.CampaignType = $scope.interaction.type;
                var isHandshake = $scope.interaction.type === 'handshake';

                var isSrfi = $scope.interaction.type === 'srfi' && fromNotification;
                data.Listvaults = [];

                if (!isHandshake && $scope.mode.delegated) {

                    data.DelegationId = $scope.DelegationSelected.DelegationId;
                    data.DelegateeId = $scope.DelegationSelected.ToAccountId;
                }

                if ($scope.campaign.CampaignType == "Event") {
                    var event = new Object();
                    event.type = "Event Campaign";
                    event.name = $scope.campaign.Name;
                    event.description = $scope.campaign.Description;
                    event.detail = new Object();
                    event.detail.timetype = "More days";
                    event.detail.starttime = $scope.campaign.starttime;
                    event.detail.startdate = $scope.campaign.startdate;
                    event.detail.endtime = $scope.campaign.endtime;
                    event.detail.enddate = $scope.campaign.enddate;
                    event.detail.location = $scope.campaign.location;
                    event.detail.theme = $scope.campaign.theme;
                    event.UserSettings = new Object();
                    event.UserSettings.note = "";
                    event.UserSettings.syncgoogle = true;
                    data.StrEvent = JSON.stringify(event);

                }
                var isvalid = true;


                $($scope.dataregisterform).each(function (index) {
                    // console.log($scope.dataregisterform[index]);
                    if (!$scope.dataregisterform[index].optional || $scope.dataregisterform[index].selected) {
                        var field = {
                            id: $scope.dataregisterform[index].id,
                            jsPath: $scope.dataregisterform[index].jsPath,
                            displayName: $scope.dataregisterform[index].displayName,
                            optional: $scope.dataregisterform[index].optional,
                            type: $scope.dataregisterform[index].type,
                            options: $scope.dataregisterform[index].options,
                            model: $scope.dataregisterform[index].model,
                            unitModel: $scope.dataregisterform[index].unitModel,
                            value: $scope.dataregisterform[index].value,
                            membership: $scope.dataregisterform[index].membership + "",
                            modelarrays: $scope.dataregisterform[index].modelarrays,
                            pathfile: "",
                            choices: $scope.dataregisterform[index].choices + "",
                            qa: $scope.dataregisterform[index].qa + ""
                        }
                        if (isvalid)
                            isvalid = $scope.vaildfiled($scope.dataregisterform[index]);
                        if (!isvalid)
                            return;

                        var allowadd = true;
                        switch (field.type) {
                            case "location":
                                var model = $scope.dataregisterform[index].model;
                                field.model = model.country;
                                field.unitModel = model.city;
                                break
                            case "address":
                                var model = $scope.dataregisterform[index].model;
                                field.model = model.address;
                                field.unitModel = model.address2;
                                break;
                            case "range":
                                var model = $scope.dataregisterform[index].model;
                                if (model == -1 || model == "-1")
                                    allowadd = false;
                                var arrays = [];
                                $($scope.dataregisterform[index].modelarrays).each(function (index, object) {
                                    var array = [];
                                    array.push(object[0]);
                                    array.push(object[1]);
                                    arrays.push(array);

                                });
                                field.modelarrays = arrays;
                                break;
                            case "qa":
                                if ($scope.dataregisterform[index].choices) {
                                    var arrays = [];
                                    $($scope.dataregisterform[index].modelarrays).each(function (index, object) {
                                        arrays.push({value: object.value});
                                    });
                                    field.modelarrays = arrays;
                                    field.model = $scope.dataregisterform[index].model.value;
                                }
                                else
                                    field.model = $scope.dataregisterform[index].model;
                                break;
                            case "doc":
                                var arrays = [];
                                if (!$scope.dataregisterform[index].qa) {
                                    $($scope.dataregisterform[index].model).each(function (index, object) {

                                        arrays.push(object.fname);
                                    });
                                    field.modelarrays = arrays;
                                }
                                else {
                                    $($scope.dataregisterform[index].model).each(function (index, object) {
                                        arrays.push(object.fname);
                                    });
                                    field.modelarrays = arrays;
                                    field.pathfile = "/" + $scope.dataregisterform[index].pathfile;
                                }
                                field.model = "";
                                break;
                            case "history":
                                allowadd = false;
                                break;
                        }
                        if (allowadd == true)
                            data.Listvaults.push(field);

                    }
                });


                if (!isvalid)
                    return;


                if (isHandshake) {

                    data.CampaignType = 'Handshake';
                    //Listvaults


                    // $($scope.dataregisterform).each(function (index) {
                    //     if ($scope.dataregisterform[index].selected) {
                    //         var field = {
                    //             id: $scope.dataregisterform[index].id,
                    //             jsPath: $scope.dataregisterform[index].jsPath
                    //         }
                    //         data.Listvaults.push(field);
                    //     }
                    // });
                } else if (isSrfi) {
                    data.comment = $scope.campaign.comment;
                    data.Fields = data.Listvaults;
                    // data.Fields = $scope.dataregisterform;
                }


                var apiPath = '/api/CampaignService/' + (isHandshake ? 'RegisterCampaignToHandShake' : (isSrfi ? 'RegisSRFI' : 'RegisterCampaign'));

                // var regis = new Object();
                // regis.ToUserId = regitGlobal.userAccount.id;
                // regis.CampaignId = $scope.interaction.id;
                // console.log($scope.interaction);
                // $http.post('/api/CampaignService/GetSRFIForRegis', regis)
                //     .success(function (response) {
                //
                //         $scope.groupField = interactionFormService.sortField(response);
                //     }).error(function (errors, status) {
                //         console.log('error',errors)
                // });


                console.log(data.Listvaults);

                // console.log(data)

                $http.post(apiPath, data)
                    .success(function (response) {
                        $scope.campaign.MembersOfBusinessNbr++;
                        var participation = {
                            actor: $scope.mode.delegated ? 'for' : 'self',
                            actorName: $scope.mode.delegated ? $scope.DelegationSelected.FromUserDisplayName : 'you',
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
                        $rootScope.$broadcast('reloadminicalendar');
                        $rootScope.$broadcast('reloadfollowstatus');
                        $rootScope.$broadcast('interaction:participated');
                        rguNotify.add(($scope.interaction.type === 'srfi' ? 'Submitted information for "' : 'Participated in "') + $scope.interaction.name + '"');

                        if ($scope.CampaignIdInPostList)
                            $scope.CampaignIdInPostList.push($scope.CampaignId);
                        //$uibModalInstance.close(response);

                        $scope.interaction.participants++;
                        $scope.closeForm();
                        notifService.resolveRequestById('handshake', $scope.CampaignId);
                    })
                    .error(function (errors) {
                        console.log('Error participating in interaction', errors);
                    });

            };

            $scope.UpdateVaultInformation = function () {
                var data = new Object();

                data.UserId = $scope.DelegationSelected.FromAccountId;
                data.BusinessUserId = $scope.BusinessUserId;
                data.CampaignId = $scope.CampaignId;
                data.CampaignType = $scope.CampaignType;
                data.Listvaults = [];
                var isvalid = true;
                $($scope.dataregisterform).each(function (index) {
                    if (!$scope.dataregisterform[index].optional || $scope.dataregisterform[index].selected) {
                        var field = {
                            id: $scope.dataregisterform[index].id,
                            jsPath: $scope.dataregisterform[index].jsPath,
                            displayName: $scope.dataregisterform[index].displayName,
                            optional: $scope.dataregisterform[index].optional,
                            type: $scope.dataregisterform[index].type,
                            options: $scope.dataregisterform[index].options,
                            model: $scope.dataregisterform[index].model,
                            unitModel: $scope.dataregisterform[index].unitModel,
                            value: $scope.dataregisterform[index].value,
                            membership: $scope.dataregisterform[index].membership + "",
                            modelarrays: $scope.dataregisterform[index].modelarrays,
                            choices: $scope.dataregisterform[index].choices + "",
                            qa: $scope.dataregisterform[index].qa + ""
                        }

                        if (isvalid)
                            isvalid = $scope.vaildfiled($scope.dataregisterform[index]);
                        if (!isvalid)
                            return;
                        var allowadd = true;
                        switch (field.type) {
                            case "location":
                                var model = $scope.dataregisterform[index].model;
                                field.model = model.country;
                                field.unitModel = model.city;
                                break
                            case "address":
                                var model = $scope.dataregisterform[index].model;
                                field.model = model.address;
                                field.unitModel = model.address2;
                                break
                            case "range":
                                var model = $scope.dataregisterform[index].model;
                                if (model == -1 || model == "-1")
                                    allowadd = false;
                                var arrays = [];
                                $($scope.dataregisterform[index].modelarrays).each(function (index, object) {
                                    var array = [];
                                    array.push(object[0]);
                                    array.push(object[1]);
                                    arrays.push(array);

                                });
                                field.modelarrays = arrays;
                                break;
                            case "qa":
                                var arrays = [];
                                $($scope.dataregisterform[index].modelarrays).each(function (index, object) {

                                    arrays.push({value: object.value});
                                });
                                field.modelarrays = arrays;
                                field.model = $scope.dataregisterform[index].model.value;
                                break;
                            case "doc":
                                var arrays = [];
                                if (!$scope.dataregisterform[index].qa) {
                                    $($scope.dataregisterform[index].model).each(function (index, object) {

                                        arrays.push(object.fname);
                                    });
                                    field.modelarrays = arrays;
                                }
                                else {
                                    $($scope.dataregisterform[index].modeldocvault).each(function (index, object) {

                                        arrays.push(object.fname);
                                    });
                                    field.modelarrays = arrays;

                                }
                                field.model = "";
                                break;
                            case "history":
                                allowadd = false;
                                break;
                        }
                        if (allowadd == true)
                            data.Listvaults.push(field);
                    }
                });
                if (!isvalid)
                    return;
                $http.post('/api/InformationVaultService/UpdateInformationVaultById', data)
                    .success(function (response) {

                        $scope.RegisterCampaign();
                        // $uibModalInstance.close(response);
                        $scope.closeForm();

                    })
                    .error(function (errors, status) {
                        // $uibModalInstance.dismiss('cancel');
                        $scope.closeForm();
                    });

            }
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
            $scope.filterMembership = function (field) {
                return !!field.membership;
            };
            $scope.filterQA = function (field) {
                return !!field.qa;
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


                $http.post('/api/CampaignService/GetUserInformationForCampaign', data)
                    .success(function (response) {
                        $scope.dataregisterform = response.ListOfFields;

                        $($scope.dataregisterform).each(function (index) {
                            if ($scope.Iscustomrole) {
                                $($scope.DelegationSelected.GroupVaultsPermission).each(function (index1, permission) {
                                    var jsPath = $scope.dataregisterform[index].jsPath;
                                    if (jsPath.search("." + permission.jsonpath) >= 0) {
                                        $scope.dataregisterform[index].read = permission.read;
                                        $scope.dataregisterform[index].write = permission.write;
                                        if (!$scope.dataregisterform[index].write && $scope.dataregisterform[index].read) {
                                            $scope.IsReadOnly = true;
                                            if (!$scope.vaildfiled($scope.dataregisterform[index])) {
                                                $scope.Isregis = false;
                                            }
                                        }
                                        if (!$scope.dataregisterform[index].write && !$scope.dataregisterform[index].read) {
                                            $scope.Isregis = false;
                                            $scope.IsReadOnly = true;
                                        }
                                    }
                                })

                            }
                            //
                            $scope.dataregisterform[index].selected = true;
                            $scope.dataregisterform[index].membership = $scope.dataregisterform[index].membership == "true" ? true : false;
                            switch ($scope.dataregisterform[index].type) {
                                case "date":
                                case "datecombo":
                                    $scope.dataregisterform[index].model = new Date($scope.dataregisterform[index].model);
                                    break;
                                case "location":
                                    var mode = {
                                        country: $scope.dataregisterform[index].model,
                                        city: $scope.dataregisterform[index].unitModel
                                    }
                                    $scope.dataregisterform[index].model = mode;
                                    break;
                                case "address":
                                    var mode = {
                                        address: $scope.dataregisterform[index].model,
                                        address2: $scope.dataregisterform[index].unitModel
                                    }
                                    $scope.dataregisterform[index].model = mode;
                                    break;
                                case "doc":
                                    $scope.dataregisterform[index].model = [];
                                    var m = [];
                                    $($scope.dataregisterform[index].modelarrays).each(function (index, fname) {
                                        m.push({
                                            fname: fname
                                        })
                                    })
                                    $scope.dataregisterform[index].model = m;
                                    break;
                            }

                        });
                        var groups;
                        if (response.hasOwnProperty('Groups'))
                            groups = response.Groups;
                        if (angular.isArray(groups) && groups.length) {

                            $scope.groups = groups;
                            groups.forEach(function (g) {
                                g.fields = [];
                            });
                            $scope.dataregisterform.forEach(function (f) {


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
                            $scope.initFieldGroups();
                            $scope.restructFormFields();
                        }


                        // $rootScope.$broadcast('interaction:formfields:update', {fields: $scope.dataregisterform});

                    })
                    .error(function (errors) {

                    });
            };
            // end

            $scope.selectDelegator(ownDelegation);


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
            $scope.GetFollowers = function () {
                var campaign = new Object();
                campaign.CampaignId = $scope.CampaignId;
                $http.post('/api/DelegationManager/GetFollowerByCampaignId', campaign)
                    .success(function (res) {
                        $scope.listFollower = res;
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
            $scope.GetFollowers();
            $scope.CheckRegister();
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

