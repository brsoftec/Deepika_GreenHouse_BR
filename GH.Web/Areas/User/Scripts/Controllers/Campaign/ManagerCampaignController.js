var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule'], true);
myApp.getController('ManagerCampaignController',
    ['$scope', '$rootScope', '$document', '$http', '$timeout', 'rguAlert', 'rguNotify', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'NotificationService',
        '$uibModal', 'CampaignService', 'ConfirmDialogService', 'billingService', 'rguModal',
        function ($scope, $rootScope, $document, $http, $timeout, rguAlert, rguNotify, userManager, sweetAlert, authService, alertService, informationVaultService, notificationService, $uibModal,
                  CampaignService, ConfirmDialogService, billingService, rguModal) {
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

                vm.CampaignTypeSelect = "All Interactions";
                vm.CampaignStatusSelect = "All Status";
                vm.ListCampaignNodraff = [];
                vm.ListCampaigndraff = [];
                vm.ListCampaigntemplate = [];
                vm.CampaignSRFIId = "";
                this.getAllCampaignPaging();
                this.GetAllDraftCampaign();
                this.GetAllTemplateCampaign();
                this.GetCampaignRole();
            };

            $scope.view = { openingMoreActions: false };

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

            var isActive = false;

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

            this.ActionAnalytics = function (campaignId, campaignName) {
                window.location.href = "/BusinessUserSystem/AnalyticsCampaign?campaignid=" + campaignId + "&CampaignNameValue=" + campaignName;
            }

            this.ChangeCampaignType = function (campaignType) {
                $timeout(function () {
                    vm.CampaignTypeSelect = campaignType;
                    vm.getAllCampaignPaging();
                });

            }

            this.ChangeCampaignstatus = function (campaignstatus) {
                $timeout(function () {
                    vm.CampaignStatusSelect = campaignstatus;
                    vm.getAllCampaignPaging();
                });

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
            this.ViewCustomer = function (campaignid) {
                window.location.href = "/Campaign/ManagerHandshakeUsers?campaignid=" + campaignid;
            };
            this.ManageCustomers = function (campaignid) {
                window.location.href = "/Interactions/Customers?interaction=" + campaignid;
            };

            this.DeleteDraf = function (index) {

                ConfirmDialogService.Open("Do you want to remove it", function () {
                    var data = new Object();
                    data.CampaignId = vm.ListCampaigndraff[index].Id;
                    $http.post('/api/CampaignService/RemoveCampaign', data)
                        .success(function (response) {
                            vm.GetAllDraftCampaign();
                        })
                        .error(function (errors, status) {

                        });
                });

            }

            this.DeleteCampaign = function (campaign) {
                rguAlert('Are you sure you want to remove interaction "<strong>' + campaign.Name + '</strong>"?', {
                    style: 'delete',
                    actions: 'yes'
                }, function () {
                    $http.post('/api/CampaignService/RemoveCampaign', { CampaignId: campaign.Id })
                        .success(function (response) {
                            rguNotify.add('Deleted interaction: ' + campaign.Name);
                            vm.initializeController();
                        })
                        .error(function (errors) {
                            console.log('Error deleting interaction: ' + campaign.Name, errors)
                        });
                });


            };

            this.edit = function (index) {
                window.location.href = "/Interactions/Edit/" + vm.ListCampaignNodraff[index].Id;
            };
            this.editInteraction = function (campaign) {
                window.location.href = "/Interactions/Edit/" + campaign.Id;
            };

            this.newFromTemplate = function (template) {
                window.location.href = "/Interactions/New/" + template.Type + '?templateId=' + template.Id;
            };

            this.gotoSRFI = function () {
                window.location.href = "/Campaign/InsertSRFICampaign";
            }
            this.gotoPushToVault = function () {
                window.location.href = "/Campaign/InsertPushToVault";
            }
            this.gotoHandShake = function () {
                window.location.href = "/Campaign/InsertHandshakeCampaign";
            }
            this.Approve = function (index) {
                if (vm.ListCampaignNodraff[index].Type == "SRFI")
                    window.location.href = "/Campaign/ApprovedSRFICampaign?campaignid=" + vm.ListCampaignNodraff[index].Id;
                else if (vm.ListCampaignNodraff[index].Type == "Registration")
                    window.location.href = "/Campaign/ApprovedRegistrationCampaign?campaignid=" + vm.ListCampaignNodraff[index].Id;
                else if (vm.ListCampaignNodraff[index].Type == "Event")
                    window.location.href = "/Campaign/ApprovedEventCampaign?campaignid=" + vm.ListCampaignNodraff[index].Id;
                else if (vm.ListCampaignNodraff[index].Type == "PushToVault")
                    window.location.href = "/Campaign/ApprovedPushToVaultCampaign?campaignid=" + vm.ListCampaignNodraff[index].Id;
                else if (vm.ListCampaignNodraff[index].Type == "Handshake")
                    window.location.href = "/Campaign/ApprovedHandShakeCampaign?campaignid=" + vm.ListCampaignNodraff[index].Id;
                else
                    window.location.href = "/Campaign/ApprovedAdvertisingCampaign?campaignid=" + vm.ListCampaignNodraff[index].Id;

            };
            this.review = function (index) {

                window.location.href = "/Interactions/Review/" + vm.ListCampaignNodraff[index].Id;

            };

            this.EditDraf = function (index) {
                if (vm.ListCampaigndraff[index].Type == "SRFI")
                    window.location.href = "/Campaign/InsertSRFICampaign?campaignid=" + vm.ListCampaigndraff[index].Id;
                else if (vm.ListCampaigndraff[index].Type == "Registration")
                    window.location.href = "/Campaign/InsertRegistrationCampaign?campaignid=" + vm.ListCampaigndraff[index].Id;
                else if (vm.ListCampaigndraff[index].Type == "Event")
                    window.location.href = "/Campaign/InsertEventCampaign?campaignid=" + vm.ListCampaigndraff[index].Id;
                else if (vm.ListCampaigndraff[index].Type == "PushToVault")
                    window.location.href = "/Campaign/InsertPushToVault?campaignid=" + vm.ListCampaigndraff[index].Id;
                else if (vm.ListCampaigndraff[index].Type == "Handshake")
                    window.location.href = "/Campaign/InsertHandshakeCampaign?campaignid=" + vm.ListCampaigndraff[index].Id;
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
                    window.location.href = "/Campaign/InsertPushToVault?campaignid=" + vm.ListCampaigntemplate[index].Id;
                else if (vm.ListCampaigntemplate[index].Type == "Handshake")
                    window.location.href = "/Campaign/InsertHandshakeCampaign?campaignid=" + vm.ListCampaigntemplate[index].Id;
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
                else if (vm.ListCampaigntemplate[index].Type == "Handshake")
                    window.location.href = "/Campaign/InsertHandshakeCampaign?action=clone&campaignid=" + vm.ListCampaigntemplate[index].Id;
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

            this.getAllCampaignPaging = function () {
                var data = new Object();
                data.CurrentPageNumber = vm.currentPageNumber;
                data.CampaignType = vm.CampaignTypeSelect;
                if (data.CampaignType == "All Interactions")
                    data.CampaignType = "All";
                data.Isdraff = true;
                data.Istemplate = true;

                data.CampaignStatus = vm.CampaignStatusSelect;
                if (data.CampaignStatus == "All Status")
                    data.CampaignStatus = "All";
                data.PageSize = vm.pageSize;

                $http.post('/api/CampaignService/GetCampaignsByUser', data)
                    .success(function (response) {
                        $timeout(function () {
                            vm.ListCampaignNodraff = response.Listitems;
                            vm.totalCampaigns = response.TotalPages;
                            vm.Isviewapproved = response.Isviewapproved;
                            vm.Isviewedit = response.Isviewedit;
                            vm.CampaignSRFIId = response.CampaignSRFIId;
                        });

                    })
                    .error(function (errors) {
                        console.log('Error loading interactions:', errors)
                    });
            };

            this.GetAllDraftCampaign = function () {
                var data = new Object();
                data.Isdraff = false;
                data.CampaignStatus = "Draft";
                data.CurrentPageNumber = vm.currentPageNumberdraff;
                data.PageSize = vm.pageSizedraff;

                $http.post('/api/CampaignService/GetCampaignsByUser', data)
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
                data.Istemplate = false;

                data.CampaignStatus = "Template";
                data.CurrentPageNumber = vm.currentPageNumbertemplate;
                data.PageSize = vm.pageSizetemplate;

                $http.post('/api/CampaignService/GetCampaignsByUser', data)
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
