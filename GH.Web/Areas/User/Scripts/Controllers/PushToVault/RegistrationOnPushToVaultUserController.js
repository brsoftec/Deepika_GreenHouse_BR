var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule'], true);

myApp.controller('RegistrationOnPushToVaultUserController',
    ['$scope', '$rootScope', '$uibModalInstance', '$uibModal', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'NotificationService', 'registerPopup', 'SmSAuthencationService', 'BusinessAccountService', 'interactionFormService',

        function ($scope, $rootScope, $uibModalInstance, $uibModal, $http, userManager, sweetAlert, authService, alertService, notificationService, registerPopup, SmSAuthencationService, _baService, interactionFormService) {
            $scope.messageBox = "";
            $scope.alerts = [];
            $scope.dataregisterform = registerPopup.ListOfFields;
            $scope.BusinessUserId = registerPopup.BusinessUserId;
            $scope.CampaignId = registerPopup.CampaignId;
            $scope.CampaignType = registerPopup.CampaignType;
            $scope.campaign = registerPopup.campaign;
            // $scope.BusinessIdList = registerPopup.BusinessIdList;
            // $scope.CampaignIdInPostList = registerPopup.CampaignIdInPostList;
            $scope.campaign.BusinessName = registerPopup.businessname;
            $scope.campaign.BusinessImageUrl = registerPopup.businessavatar;
            interactionFormService.initFieldGroups(registerPopup.ListOfFields);
            $scope.renderFieldGroup = interactionFormService.renderFieldGroup;

            $scope.Mode = "Edit";
            $scope.IsReadOnly = false;
            var delegation = new Object();

            $scope.missingInfomationOfVault = false;
            $scope.memberBusSelected = null;
            delegation.FromUserDisplayName = "Myself";
            // delegation.FromUserId = applicationConfiguration.usercurrent.Id;
            $scope.ListManagerDelegatedTo = [delegation];
            $scope.DelegationSelected = delegation;

            $scope.ListMemberBus = [];
            $scope.GetMembersInBusiness = function () {
                _baService.GetMembersInBusiness().then(function (result) {
                    $scope.ListMemberBus = result.Members;
                })
            }

            $rootScope.cancel = function () {
                $uibModalInstance.dismiss('cancel');
            };

            $scope.Edit = function () {
                SmSAuthencationService.OpenPopupAuthencationPIN(function () {
                    $scope.Mode = "Edit";
                });
            }

            $scope.Register = function () {

                if ($scope.TermsConditions) {
                    if ($scope.Mode === "Edit") {
                        $scope.UpdateVaultInformation();
                        //$uibModalInstance.close(response);
                    }
                    else {
                        $scope.RegisterCampaign();
                    }
                }
                else {
                    alertService.renderErrorMessage("You need Agree  Terms & Conditions");
                    $scope.messageBox = alertService.returnFormattedMessage();
                    $scope.alerts = alertService.returnAlerts();
                }

            }
            $scope.vaildfiled = function (field) {
                var isvalid = true;
                if (field.model == "" || field.model == undefined || field.model == null)
                    isvalid = false;
                switch (field.type) {
                    case "address":
                        if (field.model == null || field.model.address == "")
                            isvalid = false;
                        break;
                    case "location":
                          if (field.model == null || field.model.country == "" || (field.model.city == null && field.options != 'nocity'))
                            isvalid = false;
                        break;
                    case "date":
                        if (field.model + "" == "Invalid Date")
                            isvalid = false;
                        break;
                }
                return isvalid;
            }
          
            $scope.RegisterCampaign = function () {

                var data = new Object();
                data.UserId = $scope.DelegationSelected.FromAccountId;
                data.BusinessUserId = $scope.BusinessUserId;
                data.CampaignId = $scope.CampaignId;
                data.CampaignType = $scope.CampaignType;
                data.Listvaults = [];
                // data.usernotifycationid = $scope.memberBusSelected.AccountId;
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

                        isvalid = $scope.vaildfiled($scope.dataregisterform[index]);
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
                $http.post('/api/CampaignService/RegisterCampaign', data)
                    .success(function (response) {
                        $uibModalInstance.close(response);
                    })
                    .error(function (errors, status) {
                    });

            }

            $scope.UpdateVaultInformation = function (status) {
                SmSAuthencationService.OpenPopupAuthencationPIN(function () {
                
                    var data = new Object();
                    $uibModalInstance.close();

                    data.UserId = "";
                    data.BusinessUserId = $scope.BusinessUserId;
                    data.CampaignId = $scope.CampaignId;
                    data.CampaignType = $scope.CampaignType;
                    data.Listvaults = [];
                    data.status = status;
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
                                qa: $scope.dataregisterform[index].qa + "",

                            }
                            isvalid = $scope.vaildfiled($scope.dataregisterform[index]);
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

                                        arrays.push({ value: object.value });
                                    });
                                    field.modelarrays = arrays;
                                    field.model = $scope.dataregisterform[index].model.value;
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

                            $uibModalInstance.close(response);
                            notificationService.resolveRequestById('push',$scope.CampaignId);

                        })
                        .error(function (errors, status) {
                            $uibModalInstance.dismiss('cancel');
                        });

                });

            }
                // Vu
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
            }

            $scope.changedelegate = function () {
                var data = new Object();
                $scope.Mode = "Read";
                data.UserId = $scope.DelegationSelected.FromAccountId;
                data.BusinessUserId = $scope.BusinessUserId;
                data.CampaignType = $scope.CampaignType;
                data.CampaignId = $scope.CampaignId;

                if ($scope.DelegationSelected.DelegationRole == "Normal" &&
                    $scope.DelegationSelected.FromUserDisplayName != "Myself") {
                    $scope.IsReadOnly = true;
                    $scope.missingInfomationOfVault = false;
                    alertService.closeAlert();
                }
                else {
                    $scope.IsReadOnly = false;
                    $scope.missingInfomationOfVault = false;
                    alertService.closeAlert();
                }
                $http.post('/api/CampaignService/GetUserInformationForCampaign', data)
                    .success(function (response) {

                        $scope.dataregisterform = response.ListOfFields;
                        $($scope.dataregisterform).each(function (index) {
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

                    })
                    .error(function (errors, status) {

                    });
            };

            $scope.notifyMissingInformationOfVault = function () {
                var campaignid = $scope.CampaignId;
                var delegatorId = $scope.DelegationSelected.FromAccountId;
                $http.get('/api/Notifications/NotifyMissingInformationOfVault?campaignId=' + campaignid + '&delegatorId=' + delegatorId)
                    .success(function (response) {
                        $uibModalInstance.close();
                    })
                    .error(function (errors, status) {
                    });
            }

            $scope.getListDelegatedTo = function () {

                var delegationModelView = new Object();
                // delegationModelView.UserId = applicationConfiguration.usercurrent.Id;
                delegationModelView.Direction = "DelegationIn";

                $http.post('/api/DelegationManager/GetListDelegation', delegationModelView)
                    .success(function (response) {
                        $(response.Listitems).each(function (index, delegate) {
                            if (delegate.Status === "Accepted") {
                                $scope.ListManagerDelegatedTo.push(delegate);
                            }
                        });
                    })
                    .error(function (errors, status) {
                    });
            };
            $scope.getListDelegatedTo();
            $scope.GetMembersInBusiness();
        }
    ]);

myApp.getController('datetimecontroller',
    ['$scope',
        function ($scope) {
            var am = this;
            am.dateformat = 'yyyy-MM-dd';
            this.datetime = new Date();
            this.openeddatetime = false;
            this.opendatetime = function () {
                this.openeddatetime = true;
            };
            this.InitData = function (date) {
                am.datetime = new Date(date);
            };

        }]);
