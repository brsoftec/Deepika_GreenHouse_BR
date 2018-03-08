var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule'], true);
myApp.getController('ManagerCampaignPushToVaultController',
    ['$scope', '$document', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'NotificationService', '$uibModal', 'CampaignService', 'ConfirmDialogService',
        function ($scope, $document, $rootScope, $http, userManager, sweetAlert, authService, alertService, informationVaultService, notificationService, $uibModal, CampaignService, ConfirmDialogService) {

            //"use strict";

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
                vm.ListCampaignNodraff = [];
                vm.ListCampaigndraff = [];
                vm.ListCampaigntemplate = [];
                vm.CampaignSRFIId = "";
                this.getAllCampaignPaging();
                this.GetAllDraftCampaign();
                this.GetAllTemplateCampaign();
                //this.GetCampaignRole();
            };
            var isActive = false;

            //  Son

            $scope.view = {openingMoreActions: false};
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
            //

            this.UnJoinOrJoin = function (hsp,c) {

                var message = hsp.IsJoin ? "Click OK to pause this sync relationship with " + c.BusinessName + ": " + c.Name + ". Warning: The business will not be notified of any changes from you for the period that the sync is paused. You will need to join again to re-activate the sync."
                    : "Click OK to join this sync relationship again.";
                ConfirmDialogService.Open(message, function () {
                    var data = new Object();
                    data.CampaignId = hsp.CampaignId;
                    data.Userid = hsp.UserId;
                    data.IsJoin = hsp.IsJoin;
                    data.syncName = c.Name;
                    data.businessName = c.BusinessName;

;
                    $http.post('/api/CampaignService/UserUnjoinorjoinHandshake', data)
                        .success(function (response) {
                            //vm.ListPostHandshakses = response.List;
                            hsp.IsJoin = !hsp.IsJoin;
                            vm.getAllCampaignPaging();
                        })
                        .error(function (errors, status) {

                        });
                });
            }
            this.Remove = function (hsp, c) {
                var message = "Click OK to terminate the sync relationship with " + c.BusinessName + ". Warning: Terminating the relationship would end this sync form. You will need to be re-invited to re-join.";
                ConfirmDialogService.Open(message, function () {
                    var data = new Object();
                    data.CampaignId = hsp.CampaignId;
                    data.Userid = hsp.UserId;
                    $http.post('/api/CampaignService/DeletePostHandshake', data)
                        .success(function (response) {
                            vm.getAllCampaignPaging();

                        })
                        .error(function (errors, status) {
                        });
                });
            }
            this.DeleteTemplate = function (index) {
                ConfirmDialogService.Open("Do you want to remove it", function () {
                    var data = new Object();
                    data.CampaignId = vm.ListCampaigntemplate[index].Id;
                    $http.post('/api/CampaignService/DeleteCampaign', data)
                        .success(function (response) {
                            vm.GetAllTemplateCampaign();
                        })
                        .error(function (errors, status) {
                        });
                });

            }

            this.DeleteDraf = function (index) {

                ConfirmDialogService.Open("Do you want to remove it", function () {
                    var data = new Object();
                    data.CampaignId = vm.ListCampaigndraff[index].Id;
                    $http.post('/api/CampaignService/DeleteCampaign', data)
                        .success(function (response) {
                            vm.GetAllDraftCampaign();
                        })
                        .error(function (errors, status) {

                        });
                });

            }
            this.ShowFieldsHansahke = function (campaignid) {
                var campaign = null;
                $http.get('/api/CampaignService/GetCampaignInfor?campaignId=' + campaignid)
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
                                    dataresult = response;
                                    var fields = response.ListOfFields, campaignFields = response.Campaign.Fields;
                                    /* $.each(campaignFields, function (fieldIndex, campaignField) {
                                     $.each(campaignField, function (index, field) {
                                     if (field._name === 'optional') {
                                     response.ListOfFields[fieldIndex].optional = field._value;
                                     }
                                     });
                                     });
                                     */
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
                                    // re-order form fields by vault group
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
                                    // re-order form fields by vault group

                                    var modalInstance = $uibModal.open({
                                        templateUrl: 'modal-feed-open-reg-handshake.html',
                                        controller: 'RegistrationHandshakeOnNewFeedController',
                                        size: "",

                                        resolve: {
                                            registerPopup: function () {
                                                return {
                                                    ListOfFields: response.ListOfFields
                                                    ,
                                                    BusinessUserId: ""
                                                    ,
                                                    CampaignId: vm.campaignid
                                                    ,
                                                    CampaignType: "Handshake"
                                                    ,
                                                    campaign: campaign
                                                    ,
                                                    BusinessIdList: null
                                                    ,
                                                    CampaignIdInPostList: null
                                                    ,
                                                    posthndshakecomment: ""
                                                    //,currentBusinessId: businessId
                                                    //,currentCampaignType: campaignType
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

            //Inject service
            this.ActionAnalytics = function (campaignId, campaignName) {
                //window.location.href = "/BusinessUserSystem/AnalyticsCampaign?CampaignIdValue=" + campaignId;
                window.location.href = "/BusinessUserSystem/AnalyticsCampaign?campaignid=" + campaignId + "&CampaignNameValue=" + campaignName;
            }

            this.ChangeCampaignType = function (campaignType) {

                vm.CampaignTypeSelect = campaignType;
                this.getAllCampaignPaging();

            }

            this.ChangeCampaignstatus = function (campaignstatus) {
                vm.CampaignStatusSelect = campaignstatus;
                this.getAllCampaignPaging();
            }

            this.Boost = function (campaign) {
                var modalInstance = $uibModal.open({
                    templateUrl: 'modal-boost.html',
                    controller: 'BoostCampaignController',
                    size: "",
                    resolve: {
                        campaign: campaign
                    }
                });
                modalInstance.result.then(function () {
                    vm.getAllCampaignPaging();
                }, function () {

                });

            }

            this.Edit = function (index) {
                if (vm.ListCampaignNodraff[index].Type == "SRFI")
                    window.location.href = "/Campaign/InsertSRFICampaign?campaignid=" + vm.ListCampaignNodraff[index].Id;
                else if (vm.ListCampaignNodraff[index].Type == "Registration")
                    window.location.href = "/Campaign/InsertRegistrationCampaign?campaignid=" + vm.ListCampaignNodraff[index].Id;
                else if (vm.ListCampaignNodraff[index].Type == "Event")
                    window.location.href = "/Campaign/InsertEventCampaign?campaignid=" + vm.ListCampaignNodraff[index].Id;
                else if (vm.ListCampaignNodraff[index].Type == "PushToVault")
                    window.location.href = "/CampaignUser/InsertPushToVault?campaignid=" + vm.ListCampaignNodraff[index].Id;
                else
                    window.location.href = "/Campaign/InsertAdvertisingCampaign?campaignid=" + vm.ListCampaignNodraff[index].Id;
            }

            this.gotoSRFI = function () {
                window.location.href = "/Campaign/InsertSRFICampaign";
            }
            this.gotoPushToVault = function () {
                window.location.href = "/CampaignUser/InsertPushToVault";
            }
            this.Approve = function (index) {
                if (vm.ListCampaignNodraff[index].Type == "SRFI")
                    window.location.href = "/Campaign/ApprovedRegistrationCampaign?campaignid=" + vm.ListCampaignNodraff[index].Id;
                else if (vm.ListCampaignNodraff[index].Type == "Registration")
                    window.location.href = "/Campaign/ApprovedRegistrationCampaign?campaignid=" + vm.ListCampaignNodraff[index].Id;
                else if (vm.ListCampaignNodraff[index].Type == "Event")
                    window.location.href = "/Campaign/ApprovedEventCampaign?campaignid=" + vm.ListCampaignNodraff[index].Id;
                else if (vm.ListCampaignNodraff[index].Type == "PushToVault")
                    window.location.href = "/CampaignUser/ApprovedPushToVaultCampaign?campaignid=" + vm.ListCampaignNodraff[index].Id;
                else
                    window.location.href = "/Campaign/ApprovedAdvertisingCampaign?campaignid=" + vm.ListCampaignNodraff[index].Id;
            }

            this.EditDraf = function (index) {
                if (vm.ListCampaigndraff[index].Type == "SRFI")
                    window.location.href = "/Campaign/InsertSRFICampaign?campaignid=" + vm.ListCampaigndraff[index].Id;
                else if (vm.ListCampaigndraff[index].Type == "Registration")
                    window.location.href = "/Campaign/InsertRegistrationCampaign?campaignid=" + vm.ListCampaigndraff[index].Id;
                else if (vm.ListCampaigndraff[index].Type == "Event")
                    window.location.href = "/Campaign/InsertEventCampaign?campaignid=" + vm.ListCampaigndraff[index].Id;
                else if (vm.ListCampaigndraff[index].Type == "PushToVault")
                    window.location.href = "/CampaignUser/InsertPushToVault?campaignid=" + vm.ListCampaigndraff[index].Id;
                else
                    window.location.href = "/Campaign/InsertAdvertisingCampaign?campaignid=" + vm.ListCampaigndraff[index].Id;

            }
            this.EditTemplate = function (index) {
                if (vm.ListCampaigntemplate[index].Type == "SRFI")
                    window.location.href = "/Campaign/InsertSRFICampaign?campaignid=" + vm.ListCampaigntemplate[index].Id;
                else if (vm.ListCampaigntemplate[index].Type == "Registration")
                    window.location.href = "/Campaign/InsertRegistrationCampaign?campaignid=" + vm.ListCampaigntemplate[index].Id;
                else if (vm.ListCampaigntemplate[index].Type == "Event")
                    window.location.href = "/Campaign/InsertEventCampaign?campaignid=" + vm.ListCampaigntemplate[index].Id;
                else if (vm.ListCampaigntemplate[index].Type == "PushToVault")
                    window.location.href = "/CampaignUser/InsertPushToVault?campaignid=" + vm.ListCampaigntemplate[index].Id;
                else
                    window.location.href = "/Campaign/InsertAdvertisingCampaign?campaignid=" + vm.ListCampaigntemplate[index].Id;
            }

            this.CloneTemplate = function (index) {
                if (vm.ListCampaigntemplate[index].Type == "SRFI")
                    window.location.href = "/Campaign/InsertSRFICampaign?action=clone&campaignid=" + vm.ListCampaigntemplate[index].Id;
                if (vm.ListCampaigntemplate[index].Type == "Registration")
                    window.location.href = "/Campaign/InsertRegistrationCampaign?action=clone&campaignid=" + vm.ListCampaigntemplate[index].Id;
                else if (vm.ListCampaigntemplate[index].Type == "Event")
                    window.location.href = "/Campaign/InsertEventCampaign?action=clone&campaignid=" + vm.ListCampaigntemplate[index].Id;
                else if (vm.ListCampaigntemplate[index].Type == "PushToVault")
                    window.location.href = "/Campaign/InsertPushToVault?action=clone&campaignid=" + vm.ListCampaigntemplate[index].Id;
                else
                    window.location.href = "/Campaign/InsertAdvertisingCampaign?action=clone&campaignid=" + vm.ListCampaigntemplate[index].Id;
            }

            this.pageChanged = function () {
                this.getAllCampaignPaging();
            }

            this.pageChangeddraff = function () {
                this.GetAllDraftCampaign();
            }
            this.pageChangedtemplate = function () {
                this.GetAllTemplateCampaign();
            }

            this.GetClassStatusCampaign = function (status) {
                if (status === "Expired") {
                    return "campaigns-expires campaigns-expires-soon";

                } else {
                    return "campaigns-expires";
                }

            }

            this.GetClasstdDiffrentStatusCampaign = function (status) {
                //if (status === "Expired") {
                //    return "campaigns-row-expired";

                //} else if (status === "Inactive") {
                //    return "campaigns-status campaigns-status-inactive";
                //} else if (status === "Pending") {
                //    return "campaigns-status campaigns-status-pending";
                //} else if (status === "Active") {
                //    return "campaigns-status campaigns-status-active";
                //}
                switch (status) {
                    case ("Expired"):
                        return "campaigns-row-expired";
                    //break;
                    case ("Pending"):
                        return "campaigns-status campaigns-status-pending";
                    //break;
                    case ("Inactive"):
                        return "campaigns-status campaigns-status-inactive";
                    //break;
                    case ("Active"):
                        return "campaigns-status campaigns-status-active";
                    //break;
                    default:
                        return "";
                }
            }

            this.closeAlert = function (index) {
                vm.alerts.splice(index, 1);
            };

            //this.CreateCampaign = function () {
            //    $location.path("Campaign/InsertCampaign");
            //}

            //this.changeCampaignType = function () {
            //    campaignService.CampaignTypeSelect = vm.CampaignTypeSelect;
            //}

            //this.setSortIndicator = function (column) {
            //    return dataGridService.setSortIndicator(column, vm.defaultSort);
            //};

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

                $http.post('/api/CampaignService/GetCampaignsPushtovaultByUserNormal', data)
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

            this.GetAllDraftCampaign = function () {
                var data = new Object();
                //var deferer = $q.defer();
                //data.UserId = applicationConfiguration.usercurrent.Id;
                data.Isdraff = false;
                data.CampaignStatus = "Draft";
                data.CurrentPageNumber = vm.currentPageNumberdraff;
                data.PageSize = vm.pageSizedraff;

                $http.post('/api/CampaignService/GetCampaignsPushtovaultByUserNormal', data)
                    .success(function (response) {
                        vm.ListCampaigndraff = response.Listitems;
                        vm.totalCampaignsdraff = response.TotalPages;
                        //deferer.resolve(response);
                    })
                    .error(function (errors, status) {

                    });
            }

            this.GetAllTemplateCampaign = function () {
                var data = new Object();
                //var deferer = $q.defer();
                //data.UserId = applicationConfiguration.usercurrent.Id;
                data.Istemplate = false;

                data.CampaignStatus = "Template";
                data.CurrentPageNumber = vm.currentPageNumbertemplate;
                data.PageSize = vm.pageSizetemplate;

                $http.post('/api/CampaignService/GetCampaignsPushtovaultByUserNormal', data)
                    .success(function (response) {
                        vm.ListCampaigntemplate = response.Listitems;
                        vm.totalCampaignstemplate = response.TotalPages;
                        //deferer.resolve(response);
                    })
                    .error(function (errors, status) {

                    });
            }

        }]);

myApp.controller('BoostCampaignController',
    ['$scope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'NotificationService', '$uibModalInstance', 'campaign', 'CampaignService',
        function ($scope, $http, userManager, sweetAlert, authService, alertService, informationVaultService, notificationService, $uibModalInstance, campaign, CampaignService) {
            $scope.campaign = campaign;
            $scope.boostCost = 4;
            $scope.submit = function () {
                CampaignService.SetBoostAdvertising(campaign.Id).then(function (response) {
                    $uibModalInstance.close();
                }, function (errors) {

                });
            }

            $scope.cancel = function () {
                $uibModalInstance.dismiss('cancel');
            }
        }]);
