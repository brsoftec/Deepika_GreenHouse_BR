var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule'], true);
myApp.getController('ManageSyncsController',
    ['$scope', '$document', '$rootScope', '$http', '$timeout', 'UserManagementService', 'SweetAlert', 'rguModal', 'rguCache', 'AuthorizationService', 'alertService', 'InformationVaultService', 'NotificationService', '$uibModal', 'CampaignService', 'ConfirmDialogService', 'rguNotify', 'formService', 'interactionFormService',
        function ($scope, $document, $rootScope, $http, $timeout, userManager, sweetAlert, rguModal, rguCache, authService, alertService, informationVaultService, notificationService, $uibModal, CampaignService, ConfirmDialogService, rguNotify, formService, interactionFormService) {
            var startOut = 0;
            var takeOut = 10;
            var startIn = 0;
            var takeIn = 10;
            $scope.view = {
                openingMoreActions: false

            };
            $scope.page = {
                sections: {
                    individualHandshakes: {
                        opening: false
                    },
                    businessHandshakes: {
                        opening: true
                    }
                }
            };

            $scope.toggleSection = function (section) {
                $scope.page.sections[section].opening = !$scope.page.sections[section].opening;
            };
            $scope.outgoingHandshakes = [];

            $scope.openHandshakeEditModal = function (handshake) {
                if (!handshake.toType) {
                    handshake.toType = 'user';
                }
                var fields = handshake.fields;
                handshake.fields = [
                    {label: 'Current Address', jsPath: '.address.currentAddress', selected: false},
                    {label: 'Mailing Address', jsPath: '.address.mailingAddress', selected: false},
                    {label: 'Mobile Number', jsPath: 'contact.mobile', selected: false},
                    {label: 'Office Number', jsPath: 'contact.office', selected: false},
                    {label: 'Personal Email', jsPath: 'contact.email', selected: false},
                    {label: 'Work Email', jsPath: 'contact.officeEmail', selected: false}
                ];
                handshake.fields.forEach(function(field) {
                   var matchedField = fields.find(function(f) {
                       return f.jsPath === field.jsPath
                   });
                   if (matchedField)
                       field.selected = matchedField.selected;
                });
                $scope.handshake = handshake;

                if (handshake.toType === 'user' && handshake.toAccountId) {
                    rguCache.getUserAsync(handshake.toAccountId).then(function (user) {
                        $timeout(function () {
                            $scope.handshake.toAvatar = user.avatar;
                        });
                    });
                }
                rguModal.openModal('handshake.edit', $scope);
            };
            $scope.openHandshakeViewModal = function (handshake) {
                $scope.handshake = handshake;
                rguModal.openModal('handshake.view', $scope);
            };

            $scope.closeModal = function (hideFunc) {
                if (angular.isFunction(hideFunc)) {
                    hideFunc();
                }
            };

            $scope.filterOutgoingHandshakes = function (handshake) {
                return handshake.status !== 'terminate';
            };
            $scope.filterSelectedFields = function (field) {
                return !!field.selected;
            };
            //
            $scope.saveHandshake = function (hideFunc) {
                var hs = new Object();
                hs = angular.copy($scope.handshake);
                $scope.UpdateManualHandshake(hs);
                rguNotify.add('Updated handshake with ' + (hs.toAccountId ?  hs.toName : hs.toEmail));
                $scope.closeModal(hideFunc);
                $rootScope.$broadcast('LoadManualHandshake');
            };

            $scope.pauseHandshake = function (hs) {
                hs.status = 'paused';
                $scope.UpdateManualHandshake(hs);

            };
            $scope.resumeHandshake = function (hs) {
                hs.status = 'active';
                $scope.UpdateManualHandshake(hs);
            };
            $scope.blockHandshake = function (hs) {
                hs.status = 'blocked';
                $scope.UpdateManualHandshake(hs);
            };
            $scope.unblockHandshake = function (hs) {
                hs.status = 'active';
                $scope.UpdateManualHandshake(hs);
            };
            $scope.terminateHandshake = function (hs) {
                hs.status = 'terminate';
                $scope.UpdateManualHandshake(hs);
                rguNotify.add('Terminated handshake with ' + (hs.toAccountId ?  hs.toName : hs.toEmail));
            };
            
            $scope.UpdateManualHandshake = function (hs) {
                $http.post('/api/manualhandshake/invite', hs)
                    .success(function (response) {
                    $rootScope.$broadcast('LoadManualHandshake');

                  }, function (errors) {
                      console.log('Error update handshake: ', errors)
                  });

                
            }
            // Vu
            $scope.getHandshakesOut = function (hideFunc) {
                var handShake = new Object();
                handShake = angular.copy($scope.handshake);
                $http.get('/api/manualhandshake/accountwithpaging?start=' + startOut + '&take=30')// + takeOut)
                    .success(function (response) {
                        $scope.outgoingHandshakes = response;
                        takeOut += takeOut;
                        $scope.outgoingHandshakes.forEach(function (handshake) {
                            if (!handshake.toAvatar && handshake.toAccountId) {
                                rguCache.getUserAsync(handshake.toAccountId).then(function (user) {
                                    handshake.toAvatar = user.avatar;
                                });
                            }
                            if (handshake.synced.substring(0,4) === '0001')
                                handshake.synced = null;
                        })
                    }, function (errors) {

                    })

            };
            $scope.getHandshakesIn = function (hideFunc) {
                var handShake = new Object();
                handShake = angular.copy($scope.handshake);
                $http.get('/api/manualhandshake/toaccountwithpaging?start=' + startIn + '&take=30')// + takeIn)
                    .success(function (response) {
                        $scope.incomingHandshakes = response;
                        takeIn += takeIn;
                        $scope.incomingHandshakes.forEach(function (handshake) {
                            if (handshake.accountId) {
                                rguCache.getUserAsync(handshake.accountId).then(function (user) {
                                    handshake.fromAvatar = user.avatar;
                                });
                            }
                            if (handshake.synced.substring(0,4) === '0001')
                                handshake.synced = null;

                        })
                    }, function (errors) {

                    })

            };

            $scope.$on('LoadManualHandshake', function () {
                $scope.getHandshakesOut();
                $scope.getHandshakesIn();
            });
            $scope.getHandshakesOut();
            $scope.getHandshakesIn();
            //

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

                vm.CampaignTypeSelect = "Handshake";
                vm.CampaignStatusSelect = "All Status";
                vm.ListCampaignNodraff = [];
                vm.ListCampaigndraff = [];
                vm.ListCampaigntemplate = [];
                vm.CampaignSRFIId = "";
                this.getAllCampaignPaging();
            };
            var isActive = false;
            $scope.listTerminateCampaign = [];

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
            $scope.refreshInteractions = function () {
                vm.getAllCampaignPaging();
            };

            $scope.$on('$destroy', function () {
                $document.off('click', documentClick);
            });
            $scope.$on('handshake:notification', function (event, data) {
                $scope.refreshInteractions();
            });

            this.UnJoinOrJoin = function (hsp, c) {
                var message = hsp.IsJoin ? "Click OK to pause this handshake relationship with " + c.BusinessName + ": " + c.Name + ". Warning: The business will not be notified of any changes from you for the period that the handshake is paused. You will need to join again to re-activate the sync."
                    : "Click OK to join this sync relationship again.";
                ConfirmDialogService.Open(message, function () {
                    var data = new Object();
                    data.CampaignId = hsp.CampaignId;
                    data.Userid = hsp.UserId;
                    data.IsJoin = hsp.IsJoin;
                    data.syncName = c.Name;
                    data.businessName = c.BusinessName;

                    $http.post('/api/CampaignService/UserUnjoinorjoinHandshake', data)
                        .success(function (response) {
                            hsp.IsJoin = !hsp.IsJoin;
                            vm.getAllCampaignPaging();
                        })
                        .error(function (errors, status) {

                        });
                });
            }
            this.Remove = function (hsp, c) {
                var message = "Click OK to terminate the sync relationship with " + c.BusinessName + ". Warning: Terminating the relationship would end this handshake. You will need to be re-invited to re-join.";
                ConfirmDialogService.Open(message, function () {
                    var data = new Object();
                    data.CampaignId = hsp.CampaignId;
                    data.Userid = hsp.UserId;
                    $http.post('/api/CampaignService/TerminatePostHandshakeUser', data)
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
            $scope.deleteCampaign = function (hs) {
                var data = new Object();
                data.CampaignId = hs.postHandShake.CampaignId;
                data.Userid = hs.postHandShake.UserId;
                $http.post('/api/CampaignService/DeletePostHandshake', data)
                    .success(function (response) {
                        //vm.ListPostHandshakses = response.List;
                        $scope.getListTerminateCampaign();
                        rguNotify.add('Removed terminated member: ' + hs.Name);
                    })
                    .error(function (errors, status) {
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

            this.openHandshakeFormViewer = function (campaignId) {
                var campaign = null;
                var memberId = regitGlobal.userAccount.accountId;
                $http.get('/api/CampaignService/GetCampaignInfor?campaignId=' + campaignId)
                    .success(function (info) {
                        campaign = info;

                        if (campaign) {
                            var data = new Object();
                            data.CampaignId = campaign.CampaignId;
                            data.Userid = memberId;

                            var dataresult;

                            $http.post("/api/CampaignService/GetUserInfomationfromahndshakeid", data)
                                .success(function (response) {
                                    console.log(response);
                                    var paths = {};
                                    response.ListOfFields = [];
                                    response.Fields.forEach(function (field) {
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
            this.ShowFieldsHansahke = function (campaignid) {
                var campaign = null;
                var memberId = regitGlobal.userAccount.accountId;
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
                            $http.post("/api/CampaignService/GetUserInformationForCampaign", data)
                                .success(function (response) {
                                    dataresult = response;
                                    var fields = response.ListOfFields, campaignFields = response.Campaign.Fields;
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

                                    $scope.interaction = {
                                        id: campaignid,
                                        type: 'handshake',
                                        fields: response.ListOfFields
                                    };
                                    $scope.formViewer = {
                                        memberId: memberId
                                    };
                                    formService.openInteractionFormViewer($scope.interaction,'values',$scope);

                                    // var modalInstance = $uibModal.open({
                                    //     templateUrl: 'modal-feed-open-reg-handshake.html',
                                    //     controller: 'RegistrationHandshakeOnNewFeedController',
                                    //     size: "",
                                    //     resolve: {
                                    //         registerPopup: function () {
                                    //             return {
                                    //                 ListOfFields: response.ListOfFields,
                                    //                 BusinessUserId: "",
                                    //                 CampaignId: vm.campaignid,
                                    //                 CampaignType: "Handshake",
                                    //                 campaign: campaign,
                                    //                 BusinessIdList: null,
                                    //                 CampaignIdInPostList: null,
                                    //                 posthndshakecomment: ""
                                    //             };
                                    //         }
                                    //     }
                                    // });
                                    //
                                    // modalInstance.result.then(function (campaign) {
                                    // }, function () {
                                    // });
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
                if (data.CampaignType == "All Campaigns")
                    data.CampaignType = "All";
                data.Isdraff = true;
                data.Istemplate = true;
                data.CampaignStatus = vm.CampaignStatusSelect;
                if (data.CampaignStatus == "All Status")
                    data.CampaignStatus = "All";
                data.PageSize = vm.pageSize;

                $http.post('/api/CampaignService/GetCampaignsByUserNormal', data)
                    .success(function (response) {
                        vm.ListCampaignNodraff = response.Listitems;
                        vm.totalCampaigns = response.TotalPages;
                        vm.Isviewapproved = response.Isviewapproved;
                        vm.Isviewedit = response.Isviewedit;
                        vm.CampaignSRFIId = response.CampaignSRFIId;

                    })
                    .error(function (errors, status) {

                    });

                $scope.getListTerminateCampaign = function () {
                    $scope.listTerminateCampaign = [];
                    $http.post('/api/CampaignService/GetTerminateCampaignsByUserNormal', data)
                        .success(function (response) {

                            $scope.listTerminateCampaign = response.Listitems;

                        })
                        .error(function (errors, status) {

                        });

                }
                $scope.getListTerminateCampaign();
            }


        }]);

