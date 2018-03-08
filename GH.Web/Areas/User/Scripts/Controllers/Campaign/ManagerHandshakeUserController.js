﻿
var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule'], true);
myApp.getController('ManagerHandshakeUserController',
    ['$scope', '$document', '$rootScope', '$http', '$location', 'UserManagementService', 'SweetAlert', 'AuthorizationService',
        'alertService', 'InformationVaultService', 'NotificationService', '$uibModal', 'CampaignService', 'ConfirmDialogService', 'CommonService', 'interactionFormService', '$moment', 'rguNotify', 'formService',
        function ($scope, $document, $rootScope, $http, $location, userManager, sweetAlert, authService,
                  alertService, informationVaultService, notificationService, $uibModal, CampaignService, ConfirmDialogService, CommonService, interactionFormService, $moment, rguNotify, formService) {
            //"use strict";

            //Outsite
            $scope.listHandShakeOutSite = [];
            $scope.getHandShakeOutSite = function () {
                var outsite = new Object();
                outsite.CompnentId = vm.campaignid;
                $scope.listHandShakeOutSite = [];
                authService.GetListHandShakeOutsite(outsite).then(function (rs) {
                    $scope.listHandShakeOutSite = rs;
                }, function (errors) {
                    swal('Error', errors, 'error');
                })
            }

            $scope.listHandShakeTerminate = [];
            $scope.deleteHandShake = function (hsp) {
                var message = "Click OK to remove the terminated handshake relationship with " + hsp.UserName;

                ConfirmDialogService.Open(message, function () {
                    var data = new Object();
                    data.CampaignId = vm.campaignid;
                    data.Userid = hsp.UserId;
                    $http.post('/api/CampaignService/DeletePostHandshake', data)
                        .success(function (response) {
                            $scope.getHandShakeTerminates();
                            rguNotify.add('Removed terminated member: ' + hsp.UserName);
                        })
                        .error(function (errors, status) {
                        });
                });
            };
            $scope.getHandShakeTerminates = function () {
                var data = new Object();
                data.CampaignId = vm.campaignid;
                $scope.listHandShakeTerminate = [];
                $http.post('/api/CampaignService/GetPostHandShakeTerminateByCamapignId', data)
                    .success(function (res) {
                        $scope.listHandShakeTerminate = res.List;
                    })
                    .error(function (errors, status) {
                    });
            }

            $scope.removeOutside = function (userOutsite) {
                var message = "Click OK to remove " + userOutsite.Email + " from the pending list."
                    + "?";

                ConfirmDialogService.Open(message, function () {
                    var outsite = new Object();
                    outsite.Id = userOutsite.Id;

                    $http.post('/api/Account/DeleteOutsiteById', outsite)
                        .success(function (response) {
                            $scope.getHandShakeOutSite();
                            rguNotify.add('Removed handshake email invitation: ' + userOutsite.Email);
                            $scope.checkQuota();
                        })
                        .error(function (errors, status) {
                        });
                });
            };
            // vu
            var vm = this;
            this.initializeController = function () {

                vm.alerts = [];
                vm.messageBox = "";
                vm.Isviewapproved = true;
                vm.Isviewedit = true;

                vm.currentPageNumber = 1;
                vm.pageSize = 10;
                vm.totalCampaigns = 0;

                vm.currentPageNumberdraff = 1;
                vm.pageSizedraff = 10;
                vm.totalCampaignsdraff = 0;

                vm.currentPageNumbertemplate = 1;
                vm.pageSizetemplate = 10;
                vm.totalCampaignstemplate = 0;

                vm.CampaignTypeSelect = "All Campaigns";
                vm.CampaignStatusSelect = "All Status";
                vm.ListPostHandshakses = [];
                vm.ListCampaigndraff = [];
                vm.ListCampaigntemplate = [];
                vm.CampaignSRFIId = "";
                vm.campaignid = CommonService.GetQuerystring("campaignid");
                $rootScope.camId = CommonService.GetQuerystring("campaignid");

                this.getSyncInfo();
                this.getAllPostHandShakes();

            };

            //  Son
            $scope.view = {
                openingMoreActions: false,
                showingQuotaReached: false
            };
            $scope.interaction = {
                id: CommonService.GetQuerystring("campaignid")
            };
            function documentClick() {
                $scope.$apply(function () {
                    $scope.view.openingMoreActions = false;
                });
            }

            $scope.openMoreActions = function (interaction, event) {
                event.stopPropagation();
                $scope.view.openingMoreActions = interaction.Id;
                $document.on('click', documentClick);

            };
            $scope.closeMoreActions = function (interaction) {
                $scope.view.openingMoreActions = false;
                $document.off('click', documentClick);
            };
            $scope.$on('$destroy', function () {
                $document.off('click', documentClick);
            });
            $scope.refreshPage = function () {
                window.location.reload(true);
            };
            $scope.refreshMembers = function () {
                vm.getAllPostHandShakes();
                // $scope.getAllHandshakes();
            };

            $scope.$on('handshake:notification', function (event, data) {
                var notif = data.notification;
                $scope.refreshMembers();
            });
            $scope.$on('handshake:invited', function (event, data) {
                $scope.refreshMembers();
            });
            $scope.search = {
                query: ''
            };
            $scope.filterMembers = function (query) {

                var re = new RegExp(query, 'gi');
                return function (member) {
                    if (!query) return true;
                    return re.test(member.UserName);
                };
            };
           
           var isActive = false;
            $scope.invite = {
                sendOption: 'Single email',
                recipientsText: '',
                recipients: [],
                message: ''
            };
            $scope.parseRecipients = function () {
                var recipients = $scope.invite.recipientsText.match(/[a-z0-9_\-.]+@[a-z0-9_\-.]+\.([a-z]{2,})/gi) || [];
                $scope.invite.recipients = [];
                $.each(recipients, function (i, email) {
                    email = email.toLowerCase();
                    if ($.inArray(email, $scope.invite.recipients) === -1)
                        $scope.invite.recipients.push(email);
                });
                if ($scope.invite.recipients.length) {
                    $scope.showMessage('Detected ' + $scope.invite.recipients.length + ' email addresses');
                    $scope.checkQuotaExceeds();
                }

            };
            $scope.countMembers = function() {
                return vm.ListPostHandshakses ? vm.ListPostHandshakses.length : 0 + $scope.listHandShakeOutSite ? $scope.listHandShakeOutSite.length : 0;
            };
            $scope.subscription = {
                plan: null,
                reachedQuota: false,
                exceededQuota: false
            };
            if (regitGlobal.hasOwnProperty('subscriptionPlan')) {
                $scope.subscription.plan = regitGlobal.subscriptionPlan;
            }
            $scope.checkQuota = function() {
                if (regitGlobal.subscriptionPlan.name==='enterprise') {
                    $scope.subscription.reachedQuota = $scope.view.showingQuotaReached = false;
                    return;
                }

                var remaining = regitGlobal.subscriptionPlan.handshakeRelationships - $scope.countMembers();
                if (remaining <= 0) {
                    $scope.subscription.reachedQuota = true;
                } else {
                    $scope.subscription.reachedQuota = $scope.view.showingQuotaReached = false;
                }
            };
            $scope.checkQuotaExceeds = function() {
                var remaining = regitGlobal.subscriptionPlan.handshakeRelationships - $scope.countMembers() - $scope.invite.recipients.length;
                if (remaining < 0) {
                    $scope.subscription.exceededQuota = true;
                } else {
                    $scope.subscription.exceededQuota = false;
                }
            };


            $scope.showMessage = function (msg) {
                $scope.invite.message = msg;
            };

            this.InviteMembers = function () {

                if ($scope.subscription.reachedQuota) {
                    $scope.view.showingReachedQuota = true;
                    return;
                }

                $scope.invite = {
                    sendOption: 'Single email',
                    recipientsText: '',
                    recipients: [],
                    message: ''
                };

                var modalInstance = $uibModal.open({
                    templateUrl: 'modal-open-invite-member.html',
                    controller: 'BusInviteHanshakepopupcontroller',
                    size: "",
                    scope: $scope,
                    resolve: {
                        registerPopup: function () {
                            return {
                                CampaignId: vm.campaignid
                            };
                        }
                    }
                });

                modalInstance.result.then(
                    function (campaign) {
                    },
                    function () {

                    });

            }
            this.AcknowledgeHandshake = function (hsp) {
                var message = "Click OK to acknowledge update from " + hsp.UserName;
                ConfirmDialogService.Open(message, function () {
                    var data = new Object();
                    data.CampaignId = vm.campaignid;
                    data.Userid = hsp.UserId;

                    $http.post('/api/CampaignService/BusAcknowledgeHandshake', data)
                        .success(function (response) {
                            hsp.Status = "acknowledged";
                        })
                        .error(function (errors, status) {

                        });
                });

            }
            $scope.resent = function (h) {

                var data = new Object();
                data.userinvitedid = h.UserId;
                data.CampaignId = vm.campaignid;
                $http.post('/api/CampaignService/BusReInvitedMemberHandshake', data)
                    .success(function (response) {
                        rguNotify.add('Invitation resent succesfully');
                        $scope.refreshMembers();
                    })
                    .error(function (errors, status) {
                    });

            }
            $scope.resentTerminate = function (h) {

                var data = new Object();
                data.Userid = h.BusId;
                data.userinvitedid = h.UserId;
                data.CampaignId = h.CampaignId;
                data.invitetype = "member";

                data.posthandshakecomment = h.Comment
                $http.post('/api/CampaignService/BusReInvitedMemberTerminateHandshake', data)
                    .success(function (response) {
                        swal('Invitation resent succesfully', 'OK', 'success');
                    })
                    .error(function (errors, status) {
                    });

            }

            this.ExportAll = function () {
                var items = [];
                $http.post("/api/CampaignService/GetUserInformationsExportAllHandshake", {CampaignId: vm.campaignid})
                    .success(function (response) {
                        var dataresult = response.ExportAllHandshakeModels;
                        $(dataresult).each(function (index1) {
                            $(dataresult[index1].ListOfFields).each(function (index) {

                                dataresult[index1].ListOfFields[index].selected = true;
                                dataresult[index1].ListOfFields[index].membership = dataresult[index1].ListOfFields[index].membership == "true" ? true : false;
                                switch (dataresult[index1].ListOfFields[index].type) {
                                    case "date":
                                    case "datecombo":
                                        if (dataresult[index1].ListOfFields[index] != undefined)
                                            dataresult[index1].ListOfFields[index].model = $moment(dataresult[index1].ListOfFields[index].model).format('YYYY-MM-DD');
                                        if (dataresult[index1].ListOfFieldsOld[index] != undefined)
                                            dataresult[index1].ListOfFieldsOld[index].model = $moment(dataresult[index1].ListOfFieldsOld[index].model).format('YYYY-MM-DD');

                                        items.push({
                                            "User Name": dataresult[index1].DisplayName,
                                            "Field Name": dataresult[index1].ListOfFields[index].displayName,
                                            NewValue: dataresult[index1].ListOfFields[index].model,
                                            OldValue: dataresult[index1].ListOfFieldsOld[index] == undefined ? "" : dataresult[index1].ListOfFieldsOld[index].model,
                                            "Update Date": dataresult[index1].DateUpdateJson,

                                        })
                                        break;
                                    case "location":
                                        items.push({
                                            "User Name": dataresult[index1].DisplayName,
                                            "Field Name": "Country",
                                            NewValue: dataresult[index1].ListOfFields[index].model,
                                            OldValue: dataresult[index1].ListOfFieldsOld[index] == undefined ? "" : dataresult[index1].ListOfFieldsOld[index].model,
                                            "Update Date": dataresult[index1].DateUpdateJson,

                                        })
                                        if (dataresult[index1].ListOfFields[index] != undefined &&
                                            dataresult[index1].ListOfFields[index].unitModel + "" != "null")
                                            items.push({
                                                "User Name": dataresult[index1].DisplayName,
                                                "Field Name": "City",
                                                NewValue: dataresult[index1].ListOfFields[index].unitModel,
                                                OldValue: dataresult[index1].ListOfFieldsOld[index] == undefined ? "" : dataresult[index1].ListOfFieldsOld[index].unitModel,
                                                "Update Date": dataresult[index1].DateUpdateJson,

                                            })

                                        break;
                                    case "address":
                                        items.push({
                                            "User Name": dataresult[index1].DisplayName,
                                            "Field Name": dataresult[index1].ListOfFields[index].displayName,
                                            NewValue: dataresult[index1].ListOfFields[index] == undefined ? "" : dataresult[index1].ListOfFields[index].model,
                                            OldValue: dataresult[index1].ListOfFieldsOld[index] == undefined ? "" : dataresult[index1].ListOfFieldsOld[index].model,
                                            "Update Date": dataresult[index1].DateUpdateJson,

                                        });

                                        break;

                                    case "doc":
                                        break;
                                    default:
                                        items.push({
                                            "User Name": dataresult[index1].DisplayName,
                                            "Field Name": dataresult[index1].ListOfFields[index].displayName,
                                            NewValue: dataresult[index1].ListOfFields[index].model,
                                            OldValue: dataresult[index1].ListOfFieldsOld[index] == undefined ? "" : dataresult[index1].ListOfFieldsOld[index].model,
                                            "Update Date": dataresult[index1].DateUpdateJson,
                                        });
                                }
                            });
                        })

                        if (items.length > 0) {
                            var opts = {
                                headers: true,
                                column: {style: {Font: {Bold: "1"}}},

                            };
                            var fileName = $scope.syncInfo.syncName + ' - All Handshakes.xlsx';
                            alasql('SELECT * INTO xlsx("' + fileName + '",?) FROM ?', [opts, items]);
                        }
                    })
                    .error(function (errors, status) {

                    });
            }
            this.DownloadVaultchange = function (posthandshake) {

                var items = [];

                var campaign = null;

                $http.get('/api/CampaignService/GetCampaignInfor?campaignId=' + vm.campaignid)
                    .success(function (info) {
                        campaign = info;
                        if (campaign) {
                            var data = new Object();
                            data.BusinessUserId = campaign.BusinessUserId;
                            data.CampaignId = campaign.CampaignId;
                            data.CampaignType = campaign.CampaignType;
                            data.campaign = campaign;
                            data.UserId = posthandshake.UserId;
                            var dataresult;
                            $http.post("/api/CampaignService/GetUserInformationForCampaign", data)
                                .success(function (response) {
                                    dataresult = response;
                                    var fields = response.ListOfFields, campaignFields = response.Campaign.Fields;
                                    $(response.ListOfFields).each(function (index) {
                                        response.ListOfFields[index].selected = true;
                                        response.ListOfFields[index].membership = response.ListOfFields[index].membership === "true" ? true : false;
                                        switch (response.ListOfFields[index].type) {
                                            case "date":
                                            case "datecombo":
                                                if (response.ListOfFields[index] != undefined)
                                                    response.ListOfFields[index].model = $moment(response.ListOfFields[index].model).format('YYYY-MM-DD');
                                                if (response.ListOfFieldsOld[index] != undefined)
                                                    response.ListOfFieldsOld[index].model = $moment(response.ListOfFieldsOld[index].model).format('YYYY-MM-DD');

                                                items.push({
                                                    "Field Name": response.ListOfFields[index].displayName,
                                                    NewValue: response.ListOfFields[index].model,
                                                    OldValue: response.ListOfFieldsOld[index] == undefined ? "" : response.ListOfFieldsOld[index].model,
                                                    "Update Date": response.handshakeupadte
                                                })
                                                break;
                                            case "location":
                                                items.push({
                                                    "Field Name": "Country",
                                                    NewValue: response.ListOfFields[index].model,
                                                    OldValue: response.ListOfFieldsOld[index] == undefined ? "" : response.ListOfFieldsOld[index].model,
                                                    "Update Date": response.handshakeupadte
                                                })
                                                if (response.ListOfFields[index] != undefined && response.ListOfFields[index].unitModel + "" != "null"
                                                )
                                                    items.push({
                                                        "Field Name": "City",
                                                        NewValue: response.ListOfFields[index].unitModel,
                                                        OldValue: response.ListOfFieldsOld[index] == undefined ? "" : response.ListOfFieldsOld[index].unitModel,
                                                        "Update Date": response.handshakeupadte

                                                    })

                                                break;
                                            case "address":
                                                items.push({
                                                    "Field Name": response.ListOfFields[index].displayName,
                                                    NewValue: response.ListOfFields[index].model === null ? "" : response.ListOfFields[index].model,
                                                    OldValue: response.ListOfFieldsOld[index] == undefined || response.ListOfFieldsOld[index].model === null ? "" : response.ListOfFieldsOld[index].model,
                                                    "Update Date": response.handshakeupadte
                                                });

                                                break;

                                            case "doc":
                                                break;
                                            default:
                                                items.push({
                                                    "Field Name": response.ListOfFields[index].displayName,
                                                    NewValue: response.ListOfFields[index].model,
                                                    OldValue: response.ListOfFieldsOld[index] == undefined ? "" : response.ListOfFieldsOld[index].model,
                                                    "Update Date": response.handshakeupadte

                                                });

                                        }
                                    });
                                    alasql('SELECT * INTO XLSX("' + posthandshake.UserName + '.xlsx",{headers:true}) FROM ?', [items]);
                                })
                                .error(function (errors, status) {

                                });
                        }

                    }).error(function (errors, status) {
                });

            };
            this.ShowFieldsHansahke = function () {
                var campaign = null;
                // Vu 
                $scope.testListFile = null;
                $http.get('/api/CampaignService/GetCampaignInfor?campaignId=' + vm.campaignid)
                    .success(function (info) {
                        campaign = info;
                        if (campaign) {
                            var data = new Object();
                            data.BusinessUserId = campaign.BusinessUserId;
                            data.CampaignId = campaign.CampaignId;
                            data.CampaignType = campaign.CampaignType;
                            data.campaign = campaign;
                            var dataresult;
                            $http.post("/api/CampaignService/GetUserInformationForCampaignEmpty", data)
                                .success(function (response) {
                                    $scope.testListFile = response.ListOfFields;
                                    $(response.ListOfFields).each(function (index) {
                                        response.ListOfFields[index].selected = true;
                                        response.ListOfFields[index].membership = response.ListOfFields[index].membership == "true" ? true : false;
                                        switch (response.ListOfFields[index].type) {
                                            case "date":
                                            case "datecombo":
                                                response.ListOfFields[index].model = new Date(response.ListOfFields[index].model);
                                                break;
                                            case "location":
                                                var mode = {
                                                    country: response.ListOfFields[index].model,
                                                    city: response.ListOfFields[index].unitModel
                                                }
                                                response.ListOfFields[index].model = mode;
                                                break;
                                            case "address":
                                                var mode = {
                                                    address: response.ListOfFields[index].model,
                                                    address2: response.ListOfFields[index].unitModel
                                                }
                                                response.ListOfFields[index].model = mode;
                                                break;

                                            case "doc":
                                                var m = [];
                                                $(response.ListOfFields[index].modelarrays).each(function (index, fname) {
                                                    m.push({
                                                        fname: fname
                                                    });

                                                })
                                                response.ListOfFields[index].model = m;
                                                break;
                                        }
                                    });

                                    // START
                                    var myfields = [];
                                    var vaultgroup = {
                                        "data":[
                                            { "id": "0", "name": ".basicInformation" },
                                            { "id": "1", "name": ".contact" },
                                            { "id": "2", "name": ".address" },
                                            { "id": "3", "name": ".financial" },
                                            { "id": "4", "name": ".governmentID" },
                                            { "id": "5", "name": ".family" },
                                            { "id": "6", "name": ".membership" },
                                            { "id": "7", "name": ".employment" },
                                            { "id": "8", "name": ".education" },
                                            { "id": "9", "name": ".others" },
                                            { "id": "10", "name": "undefined" }
                                        ]
                                    };

                                    var data = vaultgroup.data;

                                    for (var i in data) {
                                        var name = data[i].name;
                                        $(response.ListOfFields).each(function (index) {                                            
                                            if (response.ListOfFields[index].jsPath.startsWith(name) && name != '') {
                                                myfields.push(response.ListOfFields[index]);
                                            }
                                        });
                                    }

                                    response.ListOfFields = myfields;
                                    // END 
                                    var modalInstance = $uibModal.open({
                                        templateUrl: 'modal-feed-open-reg-handshake.html',
                                        controller: 'RegistrationHandshakeOnNewFeedController',
                                        size: "",
                                        scope: $scope,
                                        resolve: {
                                            registerPopup: function () {
                                                return {
                                                    ListOfFields: response.ListOfFields,
                                                    BusinessUserId: "",
                                                    CampaignId: vm.campaignid,
                                                    CampaignType: "Handshake",
                                                    campaign: campaign,
                                                    BusinessIdList: null,
                                                    CampaignIdInPostList: null,
                                                    posthndshakecomment: ""
                                                };
                                            }
                                        }
                                    });

                                    modalInstance.result.then(function (campaign) {
                                    }, function () {
                                    });
                                })
                                .error(function (errors, status) {

                                });
                        }

                    }).error(function (errors, status) {
                });

            };
            this.openHandshakeFieldsViewer = function () {
                formService.openInteractionFormViewer($scope.interaction,'fields',$scope);
            };
            this.openHandshakeFormViewer = function (memberId) {
                var campaign = null;
                $http.get('/api/CampaignService/GetCampaignInfor?campaignId=' + $scope.interaction.id)
                    .success(function (info) {
                        campaign = info;

                        if (campaign) {
                            var data = new Object();
                            data.CampaignId = campaign.CampaignId;
                            data.Userid = memberId;

                            var dataresult;

                            $http.post("/api/CampaignService/GetUserInfomationfromahndshakeid", data)
                                .success(function (response) {
                                    // console.log(response);
                                    var paths = {};
                                    response.ListOfFields = [];
                                    response.listinformations.forEach(function (field) {
                                        if (paths.hasOwnProperty(field.jsPath)) return;
                                        response.ListOfFields.push(field);
                                        paths[field.jsPath] = true;
                                    });
                                    // response.ListOfFields = response.listinformations;
                                    $(response.ListOfFields).each(function (index) {

                                        response.ListOfFields[index].selected = true;
                                        response.ListOfFields[index].membership = response.ListOfFields[index].membership == "true" ? true : false;
                                        switch (response.ListOfFields[index].type) {
                                            case "date":
                                            case "datecombo":
                                                response.ListOfFields[index].model = new Date(response.ListOfFields[index].model);
                                                break;
                                            case "location":
                                                var mode = {
                                                    country: response.ListOfFields[index].model,
                                                    city: response.ListOfFields[index].unitModel
                                                }
                                                response.ListOfFields[index].model = mode;
                                                break;
                                            case "address":
                                                var mode = {
                                                    address: response.ListOfFields[index].model,
                                                    address2: response.ListOfFields[index].unitModel
                                                }
                                                response.ListOfFields[index].model = mode;
                                                break;

                                            case "doc":
                                                var m = [];
                                                $(response.ListOfFields[index].modelarrays).each(function (index, fname) {
                                                    m.push({
                                                        fname: fname
                                                    });

                                                })
                                                response.ListOfFields[index].model = m;
                                                break;
                                        }
                                        interactionFormService.initFieldGroups(response.ListOfFields);
                                        // $scope.renderFieldGroup = interactionFormService.renderFieldGroup;
                                    });
                                    //
                                    // // START
                                    // var myfields = [];
                                    // var vaultgroup = {
                                    //     "data":[
                                    //         { "id": "0", "name": ".basicInformation" },
                                    //         { "id": "1", "name": ".contact" },
                                    //         { "id": "2", "name": ".address" },
                                    //         { "id": "3", "name": ".financial" },
                                    //         { "id": "4", "name": ".governmentID" },
                                    //         { "id": "5", "name": ".family" },
                                    //         { "id": "6", "name": ".membership" },
                                    //         { "id": "7", "name": ".employment" },
                                    //         { "id": "8", "name": ".education" },
                                    //         { "id": "9", "name": ".others" },
                                    //         { "id": "10", "name": "undefined" }
                                    //     ]
                                    // };
                                    //
                                    // var data = vaultgroup.data;
                                    //
                                    // for (var i in data) {
                                    //     var name = data[i].name;
                                    //     $(response.ListOfFields).each(function (index) {
                                    //         if (response.ListOfFields[index].jsPath.startsWith(name) && name != '') {
                                    //             myfields.push(response.ListOfFields[index]);
                                    //         }
                                    //     });
                                    // }
                                    //
                                    // response.ListOfFields = myfields;
                                    // END

                                    $scope.formViewer = {
                                        memberId: memberId
                                    };
                                    $scope.interaction.fields = response.ListOfFields;
                                    console.log($scope.interaction)

                                    formService.openInteractionFormViewer($scope.interaction,'values',$scope);
/*
                                    var modalInstance = $uibModal.open({
                                        templateUrl: 'modal-feed-open-reg-handshake.html',
                                        controller: 'RegistrationHandshakeOnNewFeedController',
                                        size: "",
                                        scope: $scope,
                                        resolve: {
                                            registerPopup: function () {
                                                return {
                                                    ListOfFields: response.ListOfFields,
                                                    BusinessUserId: "",
                                                    CampaignId: vm.campaignid,
                                                    CampaignType: "Handshake",
                                                    campaign: campaign,
                                                    BusinessIdList: null,
                                                    CampaignIdInPostList: null,
                                                    posthndshakecomment: ""
                                                };
                                            }
                                        }
                                    });

                                    modalInstance.result.then(function (campaign) {
                                    }, function () {
                                    });*/
                                })
                                .error(function (errors, status) {

                                });
                        }

                    }).error(function (errors, status) {
                });


            };

            this.ShowFieldsHansahkeOfUser = function (userid) {
                var campaign = null;
                $http.get('/api/CampaignService/GetCampaignInfor?campaignId=' + vm.campaignid)
                    .success(function (info) {
                        campaign = info;

                        if (campaign) {
                            var data = new Object();
                            data.CampaignId = campaign.CampaignId;
                            data.Userid = userid

                            var dataresult;

                            $http.post("/api/CampaignService/GetUserInfomationfromahndshakeid", data)
                                .success(function (response) {
                                    response.ListOfFields = response.listinformations;
                                    $(response.ListOfFields).each(function (index) {
                                        response.ListOfFields[index].selected = true;
                                        response.ListOfFields[index].membership = response.ListOfFields[index].membership == "true" ? true : false;
                                        switch (response.ListOfFields[index].type) {
                                            case "date":
                                            case "datecombo":
                                                response.ListOfFields[index].model = new Date(response.ListOfFields[index].model);
                                                break;
                                            case "location":
                                                var mode = {
                                                    country: response.ListOfFields[index].model,
                                                    city: response.ListOfFields[index].unitModel
                                                }
                                                response.ListOfFields[index].model = mode;
                                                break;
                                            case "address":
                                                var mode = {
                                                    address: response.ListOfFields[index].model,
                                                    address2: response.ListOfFields[index].unitModel
                                                }
                                                response.ListOfFields[index].model = mode;
                                                break;

                                            case "doc":
                                                var m = [];
                                                $(response.ListOfFields[index].modelarrays).each(function (index, fname) {
                                                    m.push({
                                                        fname: fname
                                                    });

                                                })
                                                response.ListOfFields[index].model = m;
                                                break;
                                        }
                                        interactionFormService.initFieldGroups(response.ListOfFields);
                                        $scope.renderFieldGroup = interactionFormService.renderFieldGroup;
                                    });

                                    // START
                                    var myfields = [];
                                    var vaultgroup = {
                                        "data":[
                                            { "id": "0", "name": ".basicInformation" },
                                            { "id": "1", "name": ".contact" },
                                            { "id": "2", "name": ".address" },
                                            { "id": "3", "name": ".financial" },
                                            { "id": "4", "name": ".governmentID" },
                                            { "id": "5", "name": ".family" },
                                            { "id": "6", "name": ".membership" },
                                            { "id": "7", "name": ".employment" },
                                            { "id": "8", "name": ".education" },
                                            { "id": "9", "name": ".others" },
                                            { "id": "10", "name": "undefined" }
                                        ]
                                    };

                                    var data = vaultgroup.data;

                                    for (var i in data) {
                                        var name = data[i].name;
                                        $(response.ListOfFields).each(function (index) {                                            
                                            if (response.ListOfFields[index].jsPath.startsWith(name) && name != '') {
                                                myfields.push(response.ListOfFields[index]);
                                            }
                                        });
                                    }

                                    response.ListOfFields = myfields;
                                    // END 
                                    var modalInstance = $uibModal.open({
                                        templateUrl: 'modal-feed-open-reg-handshake.html',
                                        controller: 'RegistrationHandshakeOnNewFeedController',
                                        size: "",
                                        scope: $scope,
                                        resolve: {
                                            registerPopup: function () {
                                                return {
                                                    ListOfFields: response.ListOfFields,
                                                    BusinessUserId: "",
                                                    CampaignId: vm.campaignid,
                                                    CampaignType: "Handshake",
                                                    campaign: campaign,
                                                    BusinessIdList: null,
                                                    CampaignIdInPostList: null,
                                                    posthndshakecomment: ""
                                                };
                                            }
                                        }
                                    });

                                    modalInstance.result.then(function (campaign) {
                                    }, function () {
                                    });
                                })
                                .error(function (errors, status) {

                                });
                        }

                    }).error(function (errors, status) {
                });

            }

            this.GetCampaignRole = function () {
                var data = new Object();

                $http.post('/api/CampaignService/GetCampaignRole', data)
                    .success(function (response) {
                        isActive = response.IsActive;
                    })
                    .error(function (errors, status) {

                    });
            }

            this.IsShow = function () {
                return isActive;
            }

            this.UnJoinOrJoin = function (hsp) {
                var message = hsp.IsJoin ? "Click OK to pause this handshake relationship with " + hsp.UserName + ". Warning: You will not be notified of any changes from " + hsp.UserName + " for the period that the handshake is paused. You will need to re-invite to re-activate the handshake."
                    : "Click OK to resume the handshake relationship with " + hsp.UserName + ".";
                ConfirmDialogService.Open(message, function () {
                    var data = new Object();
                    data.CampaignId = vm.campaignid;
                    data.Userid = hsp.UserId;
                    data.IsJoin = hsp.IsJoin;

                    $http.post('/api/CampaignService/UserUnjoinorjoinHandshake', data)
                        .success(function (response) {
                            //vm.ListPostHandshakses = response.List;
                            hsp.IsJoin = !hsp.IsJoin;
                        })
                        .error(function (errors, status) {

                        });
                });
            }
            this.Remove = function (hsp) {
                var message = "Click OK to terminate the handshake relationship with " + hsp.UserName + ". Warning: Terminating the relationship would move the user to the terminated members. You will not be notified of any changes from the user for the period that the handshake is terminated. You will need to re-invite terminated members to re-activate the handshake."
                ConfirmDialogService.Open(message, function () {
                    var data = new Object();
                    data.CampaignId = vm.campaignid;
                    data.Userid = hsp.UserId;
                    $http.post('/api/CampaignService/TerminatePostHandshake', data)
                        .success(function (response) {
                            vm.ListPostHandshakses = response.List;
                            $scope.getHandShakeTerminates();
                            rguNotify.add('Handshake relationship terminated: ' + hsp.UserName);
                            $scope.checkQuota();
                        })
                        .error(function (errors, status) {
                        });
                });
            };

             this.closeAlert = function (index) {
                vm.alerts.splice(index, 1);
            };

            this.getAllPostHandShakes = function () {
                var data = new Object();
                data.CampaignId = vm.campaignid;

                $http.post('/api/CampaignService/GetPostHandShakeByCamapignId', data)
                    .success(function (response) {
                        vm.ListPostHandshakses = response.List;
                         // console.log(response)
                    })
                    .error(function (errors, status) {

                    });
                //$scope.listHandShakeTerminate
                $scope.listHandShakeTerminate = [];
                $http.post('/api/CampaignService/GetPostHandShakeTerminateByCamapignId', data)
                    .success(function (res) {
                        $scope.listHandShakeTerminate = res.List;
                    })
                    .error(function (errors, status) {
                    });

                var outsite = new Object();
                outsite.CompnentId = vm.campaignid;
                //outsite.FromUserId = vm.BusId;
                $scope.listHandShakeOutSite = [];
                authService.GetListHandShakeOutsite(outsite).then(function (rs) {
                    $scope.listHandShakeOutSite = rs;
                    $scope.checkQuota();

                }, function (errors) {
                    swal('Error', errors, 'error');

                })
            };

            // Resend GetPostHandShakeTerminateByCamapignId
            $scope.resendOutside = function (outsite) {
                var data = new Object();
                var listEmail = [];
                listEmail.push(outsite.Email);

                data.CampaignId = outsite.CompnentId;
                data.Userid = outsite.FromUserId;

                data.listEmailInvite = listEmail;
                data.invitetype = 'email';
                data.posthandshakecomment = outsite.Description;

                $http.post('/api/CampaignService/BusInvitedMemberHandshakeList', data)
                    .success(function (response) {
                        $scope.getHandShakeOutSite();
                        rguNotify.add('Email notification resent to ' + outsite.Email);
                    });
            }
            //

            this.getSyncInfo = function () {

                $http.get('/api/CampaignService/GetCampaignInfor?campaignId=' + vm.campaignid)
                    .success(function (c) {
                        if (!c) return;
                        $scope.syncInfo = {
                            syncName: c.Name,
                            businessName: c.BusinessName
                        };
                        $scope.interaction = {
                            id: c.CampaignId,
                            type: c.CampaignType,
                            name: c.Name,
                            business: {
                                accountId: c.BusinessUserId,
                                name: c.BusinessName,
                                avatar: c.BusinessImageUrl
                            },
                        }

                    }).error(function (errors, status) {
                    // console.log(errors)
                });
            };
            this.getAllCampaignPaging = function () {
                var data = new Object();
                //var deferer = $q.defer();
                //campaignModel.UserId = applicationConfiguration.usercurrent.Id;
                data.CurrentPageNumber = vm.currentPageNumber;
                data.CampaignType = vm.CampaignTypeSelect;
                if (data.CampaignType == "All Campaigns")
                    data.CampaignType = "All";
                data.Isdraff = true;
                data.Istemplate = true;

                data.CampaignStatus = vm.CampaignStatusSelect;
                if (data.CampaignStatus == "All Status")
                    data.CampaignStatus = "All";
                data.PageSize = vm.pageSize;

                $http.post('/api/CampaignService/GetCampaignsByUser', data)
                    .success(function (response) {

                        vm.ListCampaignNodraff = response.Listitems;
                        vm.totalCampaigns = response.TotalPages;

                        vm.Isviewapproved = response.Isviewapproved;
                        vm.Isviewedit = response.Isviewedit;
                        vm.CampaignSRFIId = response.CampaignSRFIId;

                        // deferer.resolve(response);
                    })
                    .error(function (errors, status) {

                    });
            }


        }
    ]);


myApp.getController('SyncEmailNotifController', ['$scope', '$rootScope', 'AuthorizationService',
    function ($scope, $rootScope, _authorizationService) {
        var syncFromData = new Object();
        $scope.notif = {
            options: [
                {
                    name: 'none',
                    label: 'No email notification'
                },
                {
                    name: 'text',
                    label: 'Notification only'
                },
                {
                    name: 'data',
                    label: 'Notification with data'
                },
                {
                    name: 'attachment',
                    label: 'Notification with data file attachment'
                }
            ],
            option: 'text',
            sendMe: true,
            sendBusiness: false,
            sendMore: false,
            recipients: [],
            message: ''
        };
        $scope.initdata = function () {
           
            // _authorizationService.GetOutsiteSyncByUserId().then(function (rs) {
            var outsite = new Object();
            outsite.CompnentId = $rootScope.camId;
            _authorizationService.GetOutsiteSyncByCampaignId(outsite).then(function (rs) {
                syncFromData = rs;
                if (rs.Id !=null) {
                    if (rs.ListEmail != null)
                        $scope.notif.recipientsText = rs.ListEmail.join(' ');

                    $scope.notif.option = rs.Option;
                    $scope.notif.sendMe = rs.SendMe;
                    if (rs.ListEmail.length > 0)
                        $scope.notif.sendMore = true;

                }

            }, function (errors) {
                swal('Error', errors, 'error');

            })

        }
        $scope.initdata();

        $scope.myEmail = $scope.user.Email;
        $scope.myBusinessEmail = $scope.userBusiness.Email || '';

        $scope.notif = {
            options: [
                {
                    name: 'none',
                    label: 'No email notification'
                },
                {
                    name: 'text',
                    label: 'Notification only'
                },
                {
                    name: 'data',
                    label: 'Notification with data'
                },
                {
                    name: 'attachment',
                    label: 'Notification with data file attachment'
                }
            ],
            option: 'text',
            sendMe: true,
            sendBusiness: false,
            sendMore: false,
            recipients: [],
            message: ''
        };
        $scope.parseRecipients = function () {
            var recipients = $scope.notif.recipientsText.match(/[a-z0-9_\-.]+@[a-z0-9_\-.]+\.([a-z]{2,})/gi) || [];
            $scope.notif.recipients = [];
            $.each(recipients, function (i, email) {
                email = email.toLowerCase();
                if ($.inArray(email, $scope.notif.recipients) === -1)
                    $scope.notif.recipients.push(email);
            });

        };
        $scope.showMessage = function (msg) {
            $scope.notif.message = msg;
        };
        $scope.close = function () {
            $scope.$parent.view.showingEmailNotifPopover = false;
        };

        $scope.save = function () {


            if ($scope.notif.option !== 'none') {
                /*                if (!$scope.notif.sendMore && !$scope.notif.sendMore) {
                 $scope.showMessage('Please select at least one recipient. If you want to turn of email notification, select from option.');
                 return;
                 }*/
                if ($scope.notif.sendMore) {

                    if (!$scope.notif.recipientsText) {
                        $scope.showMessage('Please enter at least one email address');
                        return;
                    }
                    $scope.parseRecipients();
                    if (!$scope.notif.recipients.length) {
                        $scope.showMessage('Please enter at least one valid email address');
                        return;
                    }
                }

            }
            if (syncFromData != null) {
                syncFromData.CompnentId = $rootScope.camId;
                syncFromData.ListEmail = $scope.notif.recipients;
                //
               // syncFromData.vm.campaignid;
                syncFromData.Option = $scope.notif.option;
                syncFromData.SendMe = $scope.notif.sendMe;
                _authorizationService.UpdateOutsiteSync(syncFromData).then(function () {

                }, function (errors) {
                    swal('Error', errors, 'error');

                })

            } else {
                var syncMail = new Object();
                syncMail.ListEmail = $scope.notif.recipients;
                if ($scope.notif.sendMore == false)
                    syncMail.Option = [];
                else
                    syncMail.Option = $scope.notif.option;

                syncMail.SendMe = $scope.notif.sendMe;
                syncMail.Type = "Sync Email Notification";

           
                syncMail.CompnentId = $rootScope.camId;
                _authorizationService.InsertOutsite(syncMail).then(function () {

                }, function (errors) {
                    swal('Error', errors, 'error');

                })

            }

            $scope.close();
        };

    }]);