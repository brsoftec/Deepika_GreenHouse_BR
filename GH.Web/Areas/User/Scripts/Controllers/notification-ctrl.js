var myApp = getApp("myApp", "ngJsonExportExcel");

// myApp.run(['UserManagementService', 'AuthorizationService', '$rootScope', function (_userManager, _authService, $rootScope) {
// }]);
myApp.getController("NotificationController2", ['$scope', '$rootScope', '$http', '$timeout', 'SweetAlert', 'AuthorizationService', 'NotificationService', 'notificationService', '$uibModal', 'rguNotify','$moment',
    function ($scope, $rootScope, $http, $timeout, SweetAlert, authService, notificationService, notifService, $uibModal, rguNotify,$moment) {
       

        $rootScope.CountNewNotifycation = "";
        $rootScope.ListNotifycation = [];
        $rootScope.notifMessages = {};
        $scope.GetCountNewNotifycation = function () {
            if (authService.IsAuthorized()) {
                notificationService.LatestNotification().then(function (info) {
//                     if (info) {
//                         // console.log(info)
//                         $rootScope.CountNewNotifycation = info.UnViewedNotifications;
//                         $rootScope.ListNotifycation = info.Notifications;
//                         var delay = 0;
//                         var count = 0;
//                         var notifications = info.Notifications;
//                         // notifications.sort(function (n1, n2) {
//                         //     return (n1.DateTime < n2.DateTime) ? 1 : -1;
//                         // });
//                         $.each(notifications, function (index, object) {
//                             object.datalist = [];
//                             if (!info.UnViewedNotifications || count>1) return;
//                             var id = object.Id;
// /*                            if (!localStorage.getItem('notification-' + id)) {
//                                 var text = object.Title;
//
//                                 // if (object.FromUserDisplayName) {
//                                 //     text = object.FromUserDisplayName + ' ' + text;
//                                 // }
//                                 if (!$rootScope.notifMessages.hasOwnProperty(text)) {
//                                     $timeout(function () {
//                                         rguNotify.add(text);
//                                     }, delay);
//                                     delay += 800;
//                                     count++;
//                                     $rootScope.notifMessages[text] = true;
//                                 }
//                                 localStorage.setItem('notification-' + id,'true');
//                                 localStorage.getItem('notification-' + id);
//                             }*/
//                         });
//                     }
                }, function (error) {
                });
            }
        };

        $scope.GetCountNewNotifycation();

        $scope.opennotifycation = function () {
            $("#sidebar-notifications").click();
        };

        $scope.ViewPopup = function (delegationId) {
            //notificationService.closepopup();
            $("#sidebar-notifications").click();
            var modalInstance = $uibModal.open({
                animation: $scope.animationsEnabled,
                templateUrl: 'modal-delegate-view.html',
                controller: 'ViewDelegationMasterController',
                size: "",
                backdrop: 'static',
                resolve: {
                    objectdelegationId: function () {
                        return {
                            Id: delegationId
                        }
                    }
                }
            });
            modalInstance.result.then(function () {
            }, function () {
            })
        }

        $scope.showmore = function () {
            //notificationService.closepopup();
            $location.path("Notifycation/ManagerNotifycation");
        }

        $scope.getListNotifycation = function () {
            $rootScope.CountNewNotifycation = "";
            /*
             $http.post('/api/NotifycationService/GetLisNotificationNewView')
             .success(function (response) {
             }).error(function (errors, status) {
             });
             */
        }

        $scope.countUnread = notifService.countUnread;

        $scope.pullNotifications = notifService.pullNotifications;
        $scope.pullNotifications().then(function(notifs) {
            $scope.notifications = notifs;
        });
        $scope.$on('notifications:pulled', function(event, data) {
            $scope.notifications = data;
        });

        // $http.get('/Api/AccountSettings/GetAccountId')
        //     .success(function (response) {
        //         $scope.eventAggregator().subscribe(GH.Core.SignalR.Events.ToUserConstrainedEvent, onEvent, {AccountId: response});
        //         function onEvent(e) {
        //
        //             switch (e.RuntimeNotifyType) {
        //                 case "Notify":
        //                     $scope.GetCountNewNotifycation();
        //                     break;
        //             }
        //
        //             //alert('just got message');
        //         };
        //     }).error(function (errors, status) {
        //     // console.log(errors);
        // });

    }
]);

myApp.getController("NotificationListController2", ['$scope', '$rootScope', '$http', 'SweetAlert', 'AuthorizationService', 'NotificationService', '$uibModal','$moment',
    function ($scope, $rootScope, $http, SweetAlert, authService, notificationService, $uibModal, $moment) {
        $scope.getListNotifycation = function () {
            var NotifycationModelView = new Object();

            if (authService.IsAuthorized()) {
                $http.post('/api/Notifications/GetLisNotificationNewView', NotifycationModelView)
                    .success(function (info) {
                        if (info) {
                            $rootScope.CountNewNotifycation = "";
                        }
                    }).error(function (errors, status) {
                    });
            }
        }

       // $scope.getListNotifycation();
        $scope.getNotificationWhen = function (notif) {
            if (notif.Title.indexOf('expired') >= 0)
                return '';
            return notif.DateTime;
        };

        $scope.opennotifycation = function () {
            $("#sidebar-notifications").click();
        }

        $scope.ViewPopup = function (delegationId) {
            $(document).click();
            var modalInstance = $uibModal.open({
                animation: $scope.animationsEnabled,
                templateUrl: 'modal-delegate-view.html',
                controller: 'ViewDelegationMasterController',
                size: "",
                backdrop: 'static',
                resolve: {
                    objectdelegationId: function () {
                        return {
                            Id: delegationId
                        }
                    }
                }
            });
            modalInstance.result.then(function () {
            }, function () {
            })
        }

        $scope.ViewReistrationFormPushVault = function (campaignId) {

            $(document).click();

            var campaign = null;
            var businessname = "";
            var businessavatar = ""
            $http.get('/api/CampaignService/GetCampaignInfor?campaignId=' + campaignId)
                .success(function (info) {
                    campaign = info;

                    if (campaign) {
                        var data = new Object();
                        data.BusinessUserId = campaign.BusinessUserId;
                        data.CampaignId = campaign.CampaignId;
                        data.CampaignType = campaign.CampaignType;
                        data.campaign = campaign;
                        businessname = campaign.BusinessName;
                        businessavatar = campaign.BusinessImageUrl;

                        var dataresult;

                        $http.post("/api/CampaignService/GetUserInformationForCampaignPushVault", data)
                            .success(function (response) {

                                dataresult = response;
                                var fields = response.ListOfFields, campaignFields = response.Campaign.Fields;
                               
                                $(response.ListOfFields).each(function (index) {
                                    response.ListOfFields[index].selected = true;
                                    response.ListOfFields[index].membership = response.ListOfFields[index].membership === "true" ? true : false;
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

                                var modalInstance = $uibModal.open({
                                    animation: $scope.animationsEnabled,
                                    templateUrl: 'modal-feed-open-reg-pushtovault-user.html',
                                    controller: 'RegistrationOnPushToVaultUserController',
                                    size: "",
                                    backdrop: 'static',

                                    resolve: {
                                        registerPopup: function () {
                                            return {
                                                ListOfFields: response.ListOfFields
                                                ,
                                                BusinessUserId: response.BusinessUserId
                                                ,
                                                CampaignId: response.CampaignId
                                                ,
                                                CampaignType: response.CampaignType
                                                ,
                                                campaign: campaign
                                                ,
                                                BusinessIdList: $scope.BusinessIdList
                                                ,
                                                CampaignIdInPostList: $scope.CampaignIdInPostList
                                                ,
                                                businessname: businessname
                                                ,
                                                businessavatar: businessavatar
                                              
                                            };
                                        }
                                    }
                                });

                                modalInstance.result.then(function (campaign) {
                                }, function () {
                                });
                            })
                            .error(function (errors, status) {
                                $scope.listcampaigns = response.NewFeedsItemsList;
                                $scope.CampaignUserId = response.UserId;
                                $scope.CurrentBusinessUserId = response.BusinessUserId;

                            });
                    }

                }).error(function (errors, status) {
            });

        }
        $scope.datalist = [];
        $scope.DownloadFormHandshake = function (campaignId, notify) {
            var items = [];

            var campaign = null;

            $http.get('/api/CampaignService/GetCampaignInfor?campaignId=' + campaignId)
                .success(function (info) {
                    campaign = info;
                    if (campaign) {
                        var data = new Object();
                        data.BusinessUserId = campaign.BusinessUserId;
                        data.CampaignId = campaign.CampaignId;
                        data.CampaignType = campaign.CampaignType;
                        data.campaign = campaign;
                        data.UserId = notify.FromAccountId;
                        var dataresult;
                        $http.post("/api/CampaignService/GetUserInformationForCampaign", data)
                            .success(function (response) {
                                dataresult = response;
                                var fields = response.ListOfFields;
                                var campaignFields = response.Campaign.Fields;
                                $(response.ListOfFields).each(function (index) {
                                    response.ListOfFields[index].selected = true;
                                    response.ListOfFields[index].membership = response.ListOfFields[index].membership === "true" ? true : false;
                                    switch (response.ListOfFields[index].type) {
                                        case "date":
                                        case "datecombo":
                                            // response.ListOfFields[index].model = new Date(response.ListOfFields[index].model);
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
                                            if (response.ListOfFields[index]!=undefined && response.ListOfFields[index].unitModel + "" != "null" )
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
                                alasql('SELECT * INTO XLSX("' + notify.FromUserDisplayName + '.xlsx",{headers:true}) FROM ?', [items]);
                            })
                            .error(function (errors, status) {

                            });
                    }

                }).error(function (errors, status) {
            });

        }

        //HandShake
        $scope.RedirectHandshake = function (campaignId, posthndshakecomment) {
            var campaign = null;

            $http.get('/api/CampaignService/GetCampaignInfor?campaignId=' + campaignId)
                .success(function (info) {
                    campaign = info;

                    if (campaign) {
                        var data = new Object();
                        data.BusinessUserId = campaign.BusinessUserId;
                        data.CampaignId = campaign.CampaignId;
                        data.CampaignType = campaign.CampaignType;
                        data.campaign = campaign;
                        var statusjoin = campaign.PostHandShake == null ? false : campaign.PostHandShake.IsJoin;
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

                                // START
                                // re-order form fields by vault group
                                var myfields = [];
                                var vaultgroup = {
                                    "data": [
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
                                        { "id": "10", "name": "Custom" },
                                        { "id": "11", "name": "undefined" }
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
                                
                                var modalInstance = $uibModal.open({
                                    templateUrl: 'modal-feed-open-reg-handshake.html',
                                    controller: 'RegistrationHandshakeOnNewFeedController',
                                    size: "",
                                    backdrop: 'static',

                                    resolve: {
                                        registerPopup: function () {
                                            return {
                                                ListOfFields: response.ListOfFields
                                                ,
                                                BusinessUserId: response.BusinessUserId
                                                ,
                                                CampaignId: response.CampaignId
                                                ,
                                                CampaignType: response.CampaignType
                                                ,
                                                campaign: campaign
                                                ,
                                                BusinessIdList: null
                                                ,
                                                CampaignIdInPostList: null
                                                ,
                                                posthndshakecomment: posthndshakecomment.Content
                                                ,
                                                statusjoin: statusjoin
                                              
                                            };
                                        }
                                    }
                                });

                                modalInstance.result.then(function (campaign) {
                                }, function () {
                                });
                            })
                            .error(function (errors, status) {
                                $scope.listcampaigns = response.NewFeedsItemsList;
                                $scope.CampaignUserId = response.UserId;
                                $scope.CurrentBusinessUserId = response.BusinessUserId;

                            });
                    }

                }).error(function (errors, status) {
            });
        }

        $scope.ViewReistrationForm = function (campaignId) {
            $(document).click();
            var campaign = null;
            $http.get('/api/CampaignService/GetCampaignInfor?campaignId=' + campaignId)
                .success(function (info) {
                    campaign = info;

                    if (campaign) {
                        var data = new Object();
                        data.BusinessUserId = campaign.BusinessUserId;
                        data.CampaignId = campaign.CampaignId;
                        data.CampaignType = campaign.CampaignType;
                        data.campaign = campaign;

                        var dataresult;

                        $http.post("api/CampaignService/GetUserInformationForCampaign", data)
                            .success(function (response) {
                                dataresult = response;
                                var fields = response.ListOfFields;
                                var campaignFields = response.Campaign.Fields;
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

                                var modalInstance = $uibModal.open({
                                    animation: $scope.animationsEnabled,
                                    templateUrl: 'modal-feed-open-reg.html',
                                    controller: 'RegistrationOnNewFeedController',
                                    size: "",
                                    backdrop: 'static',

                                    resolve: {
                                        registerPopup: function () {
                                            return {
                                                ListOfFields: response.ListOfFields,
                                                BusinessUserId: response.BusinessUserId,
                                                CampaignId: response.CampaignId
                                                ,
                                                CampaignType: response.CampaignType
                                                ,
                                                campaign: campaign
                                                ,
                                                BusinessIdList: $scope.BusinessIdList
                                                ,
                                                CampaignIdInPostList: $scope.CampaignIdInPostList
                                               
                                            };
                                        }
                                    }
                                });

                                modalInstance.result.then(function (campaign) {
                                }, function () {
                                });
                            })
                            .error(function (errors, status) {
                                $scope.listcampaigns = response.NewFeedsItemsList;
                                $scope.CampaignUserId = response.UserId;
                                $scope.CurrentBusinessUserId = response.BusinessUserId;

                            });
                    }

                }).error(function (errors, status) {
            });

        }

        $scope.showmore = function () {
           
            $("#sidebar-notifications").click();
            window.location.href = "/notification"

        }
        $scope.feedLoadMore = function () {
         
            $location.path("Notifycation/ManagerNotifycation");
        }
    }
]);

myApp.controller("ViewDelegationMasterController", ['$scope', '$rootScope', '$http', 'SweetAlert', 'AuthorizationService', 'NotificationService', 'notificationService', '$uibModalInstance', 'objectdelegationId', 'alertService', 'SmSAuthencationService',
    function ($scope, $rootScope, $http, SweetAlert, authService, notificationService, notifService, $uibModalInstance, objectdelegationId, alertService, SmSAuthencationService) {
        $scope.messageBox = "";
        $scope.alerts = [];
        $scope.delegation = null;
        $scope.delegationId = objectdelegationId.Id;
        $scope.acceptdelegation = function () {
            if ($scope.TermsConditions) {
                SmSAuthencationService.OpenPopupAuthencationPIN(function () {
                    var DelegationModelView = new Object();
                    DelegationModelView.DelegationId = $scope.delegationId;
                    $http.post('/api/DelegationManager/AcceptDelegationItemTemplate', DelegationModelView)
                        .success(function (response) {
                            $uibModalInstance.close();
                            notifService.resolveRequestById('delegation', $scope.delegationId);
                        }).error(function (errors, status) {
                    });
                });
            }
            else {
                alertService.renderErrorMessage("You need Agree  Terms & Conditions");
                $scope.messageBox = alertService.returnFormattedMessage();
                $scope.alerts = alertService.returnAlerts();
            }
        };

        $scope.denieddelegation = function () {
            var DelegationModelView = new Object();
          
            DelegationModelView.DelegationId = $scope.delegationId;

            $http.post('/api/DelegationManager/DeniedDelegationItemTemplate', DelegationModelView)
                .success(function (response) {
                    $uibModalInstance.close();
                    notifService.resolveRequestById('delegation', $scope.delegationId);
                }).error(function (errors, status) {
            });
        }
        $scope.getdelegation = function () {
            var DelegationModelView = new Object();
            DelegationModelView.DelegationId = $scope.delegationId;
            $http.post('/api/DelegationManager/GetDelegationItemTemplateFullById', DelegationModelView)
                .success(function (response) {
                    $scope.delegation = response.DelegationItemTemplate;
                }).error(function (errors, status) {
            });
        }
        $scope.getdelegation();
        $scope.close = function () {
            $uibModalInstance.close();
        }
    }]);

myApp.getController("ManagerNotificationController", [
    '$scope', '$rootScope', '$http', 'SweetAlert', 'NotificationService', '$sce', '$q', '$uibModal', 'rguNotify',
    function ($scope, $rootScope, $http, _sweetAlert, notificationService, $sce, $q, $uibModal, rguNotify) {
        $scope.ListNotification = [];
        $scope.notifications = [];
        $scope.init = function () {
            $scope.constant = {
              
                pullFeedUrl: "/Api/Notifications/GetNotifications",
                accountId: regitGlobal.activeAccountId,
                start: 0,
                take: 10
            };
            $scope.pullNotification();

        };

        $scope.pullNotification = function () {
          
            var start = $scope.constant.start;
            var take = $scope.constant.take;
            notificationService.PullNotification($scope.constant.pullFeedUrl, $scope.constant.accountId, start, take).then(function (response) {
                if (response && response.length > 0) {
                    console.log(response)
                    $scope.ListNotification = $scope.notifications = response;
                }
            });
        };

        // Pull more
        $scope.pullMoreNotification = function () {
            var start = $scope.constant.start;
            var take = $scope.constant.take;
            notificationService.PullNotification($scope.constant.pullFeedUrl,  $scope.constant.accountId, start, take).then(function (response) {
                if (response.length > 0) {
                   
                    $scope.notifications = $scope.notifications.concat(response);

                    $scope.constant.start += $scope.constant.take;
                }

            }, function (error) {
                // swal('Pull more notification', 'Error', 'error');
            });

        }
        // ViewPopup
        $scope.ViewPopup = function (delegationId) {
            $(document).click();
            var modalInstance = $uibModal.open({
                animation: $scope.animationsEnabled,
                templateUrl: 'modal-delegate-view.html',
                controller: 'ViewDelegationMasterController',
                size: "",
                resolve: {
                    objectdelegationId: function () {
                        return {
                            Id: delegationId
                        }
                    }
                }
            });
            modalInstance.result.then(function () {
            }, function () {
            })
        }

        $scope.ViewReistrationFormPushVault = function (campaignId) {

            $(document).click();

            var campaign = null;
            var businessname = "";
            var businessavatar = ""
            $http.get('/api/CampaignService/GetCampaignInfor?campaignId=' + campaignId)
                .success(function (info) {
                    campaign = info;

                    if (campaign) {
                        var data = new Object();
                        data.BusinessUserId = campaign.BusinessUserId;
                        data.CampaignId = campaign.CampaignId;
                        data.CampaignType = campaign.CampaignType;
                        data.campaign = campaign;
                        businessname = campaign.BusinessName;
                        businessavatar = campaign.BusinessImageUrl;

                        var dataresult;

                        $http.post("/api/CampaignService/GetUserInformationForCampaignPushVault", data)
                            .success(function (response) {
                                dataresult = response;
                                var fields = response.ListOfFields, campaignFields = response.Campaign.Fields;
                               
                                $(response.ListOfFields).each(function (index) {
                                    response.ListOfFields[index].selected = true;
                                    response.ListOfFields[index].membership = response.ListOfFields[index].membership === "true" ? true : false;
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

                                var modalInstance = $uibModal.open({
                                    animation: $scope.animationsEnabled,
                                    templateUrl: 'modal-feed-open-reg-pushtovault-user.html',
                                    controller: 'RegistrationOnPushToVaultUserController',
                                    size: "",
                                    backdrop: 'static',

                                    resolve: {
                                        registerPopup: function () {
                                            return {
                                                ListOfFields: response.ListOfFields
                                                ,
                                                BusinessUserId: response.BusinessUserId
                                                ,
                                                CampaignId: response.CampaignId
                                                ,
                                                CampaignType: response.CampaignType
                                                ,
                                                campaign: campaign
                                                ,
                                                BusinessIdList: $scope.BusinessIdList
                                                ,
                                                CampaignIdInPostList: $scope.CampaignIdInPostList
                                                ,
                                                businessname: businessname
                                                ,
                                                businessavatar: businessavatar
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
                                $scope.listcampaigns = response.NewFeedsItemsList;
                                $scope.CampaignUserId = response.UserId;
                                $scope.CurrentBusinessUserId = response.BusinessUserId;

                            });
                    }

                }).error(function (errors, status) {
                });

        }
        $scope.datalist = [];
        $scope.DownloadFormHandshake = function (campaignId, notify) {
            var items = [];

            var campaign = null;

            $http.get('/api/CampaignService/GetCampaignInfor?campaignId=' + campaignId)
                .success(function (info) {
                    campaign = info;
                    if (campaign) {
                        var data = new Object();
                        data.BusinessUserId = campaign.BusinessUserId;
                        data.CampaignId = campaign.CampaignId;
                        data.CampaignType = campaign.CampaignType;
                        data.campaign = campaign;
                        data.UserId = notify.FromAccountId;
                        var dataresult;
                        $http.post("/api/CampaignService/GetUserInformationForCampaign", data)
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
                                    response.ListOfFields[index].membership = response.ListOfFields[index].membership === "true" ? true : false;
                                    switch (response.ListOfFields[index].type) {
                                        case "date":
                                        case "datecombo":
                                            // response.ListOfFields[index].model = new Date(response.ListOfFields[index].model);
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
                                alasql('SELECT * INTO XLSX("' + notify.FromUserDisplayName + '.xlsx",{headers:true}) FROM ?', [items]);
                            })
                            .error(function (errors, status) {

                            });
                    }

                }).error(function (errors, status) {
                });

        }

        $scope.RedirectHandshake = function (campaignId, posthndshakecomment) {
            var campaign = null;

            $http.get('/api/CampaignService/GetCampaignInfor?campaignId=' + campaignId)
                .success(function (info) {
                    campaign = info;

                    if (campaign) {
                        var data = new Object();
                        data.BusinessUserId = campaign.BusinessUserId;
                        data.CampaignId = campaign.CampaignId;
                        data.CampaignType = campaign.CampaignType;
                        data.campaign = campaign;
                        var statusjoin = campaign.PostHandShake == null ? false : campaign.PostHandShake.IsJoin;
                        var dataresult;

                        $http.post("/api/CampaignService/GetUserInformationForCampaign", data)
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
                                    backdrop: 'static',

                                    resolve: {
                                        registerPopup: function () {
                                            return {
                                                ListOfFields: response.ListOfFields
                                                ,
                                                BusinessUserId: response.BusinessUserId
                                                ,
                                                CampaignId: response.CampaignId
                                                ,
                                                CampaignType: response.CampaignType
                                                ,
                                                campaign: campaign
                                                ,
                                                BusinessIdList: null
                                                ,
                                                CampaignIdInPostList: null
                                                ,
                                                posthndshakecomment: posthndshakecomment.Content
                                                ,
                                                statusjoin: statusjoin
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
                                $scope.listcampaigns = response.NewFeedsItemsList;
                                $scope.CampaignUserId = response.UserId;
                                $scope.CurrentBusinessUserId = response.BusinessUserId;

                            });
                    }

                }).error(function (errors, status) {
                });
        }

        $scope.ViewReistrationForm = function (campaignId) {
            $(document).click();
            var campaign = null;
            $http.get('/api/CampaignService/GetCampaignInfor?campaignId=' + campaignId)
                .success(function (info) {
                    campaign = info;

                    if (campaign) {
                        var data = new Object();
                        data.BusinessUserId = campaign.BusinessUserId;
                        data.CampaignId = campaign.CampaignId;
                        data.CampaignType = campaign.CampaignType;
                        data.campaign = campaign;

                        var dataresult;

                        $http.post("api/CampaignService/GetUserInformationForCampaign", data)
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

                                var modalInstance = $uibModal.open({
                                    animation: $scope.animationsEnabled,
                                    templateUrl: 'modal-feed-open-reg.html',
                                    controller: 'RegistrationOnNewFeedController',
                                    size: "",
                                    backdrop: 'static',

                                    resolve: {
                                        registerPopup: function () {
                                            return {
                                                ListOfFields: response.ListOfFields
                                                ,
                                                BusinessUserId: response.BusinessUserId
                                                ,
                                                CampaignId: response.CampaignId
                                                ,
                                                CampaignType: response.CampaignType
                                                ,
                                                campaign: campaign
                                                ,
                                                BusinessIdList: $scope.BusinessIdList
                                                ,
                                                CampaignIdInPostList: $scope.CampaignIdInPostList
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
                                $scope.listcampaigns = response.NewFeedsItemsList;
                                $scope.CampaignUserId = response.UserId;
                                $scope.CurrentBusinessUserId = response.BusinessUserId;

                            });
                    }

                }).error(function (errors, status) {
                });

        }

        //
        $scope.init();
    }
]);

