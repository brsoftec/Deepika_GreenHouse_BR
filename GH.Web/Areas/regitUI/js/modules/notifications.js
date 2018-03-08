﻿﻿
(function () {
    //  Regit Module: Notifications

    var notificationsLoaded = false;

    function isMongoId(text) {
        return angular.isString(text) && /^[09-af]{24}$/.test(text);
    }

    function mongoToId(mongoId) {
        if (angular.isString(mongoId)) return mongoId;

        function pad0(str, len) {
            var zeros = "00000000000000000000000000";
            if (str.length < len) {
                return zeros.substr(0, len - str.length) + str;
            }
            return str;
        }

        return pad0(mongoId.Timestamp.toString(16), 8) +
            pad0(mongoId.Machine.toString(16), 6) +
            pad0(mongoId.Pid.toString(16), 4) +
            pad0(mongoId.Increment.toString(16), 6);
    }


    angular.module('notifications', ['regit.ui'])
        .value('notifications', [])
        .value('notifHashes', {})
        .value('interactions', {})

        .run(function ($rootScope, notificationService, rguNotify) {
            function onReceiveEvent(e) {
                var type = e.RuntimeNotifyType;
                try {
                    var message = JSON.parse(e.Message);
                } catch (e) {
                    return;
                }

                message.Id = mongoToId(message.Id);
                notificationService.addNotification(message, true);

            }

            $rootScope.eventAggregator().subscribe(GH.Core.SignalR.Events.ToUserConstrainedEvent, onReceiveEvent,
                {AccountId: regitGlobal.activeAccountId});

            notificationService.pullNotifications().then(function (notifs) {
            });
        })
        .factory('notificationService', function ($cookies, $rootScope, $http, $q, $timeout, rgu, rguCache, notifications, notifHashes, interactions) {
            var lastExpires = null;

            function _addNotification(notif, addNew) {
                var type = notif.Type;
                var cat = notif.Category;
                var event = '';
                var attention = '';
                var actions = null;
                notif.text = notif.Title;
                if (notif.FromProfile) {
                    notif.fromName = notif.FromProfile.displayName;
                    notif.fromAvatar = notif.FromProfile.avatar;
                } else {
                    rguCache.getUserAsync(notif.FromAccountId).then(function (response) {
                        notif.fromName = response.displayName;
                        notif.fromAvatar = response.avatar;
                    });

                }
                if (!cat) {
                    if (/handshake/i.test(type)) {
                        cat = 'handshake';
                    }
                    else if (/srfi/i.test(type)) {
                        cat = 'srfi';
                    }
                    else if (/push/i.test(type)) {
                        cat = 'push';
                    }
                    else if (/delegation/i.test(type) || /delegate/i.test(type)) {
                        cat = 'delegation';
                    }
                    else if (/vault/i.test(type) || /expired/i.test(type)) {
                        cat = 'vault';
                    }
                    else if (/friend/i.test(type) || /family/i.test(type) || /emergency/i.test(type)) {
                        cat = 'network';
                    }
                    else if (/workflow/i.test(type)) {
                        cat = 'workflow';
                    }
                    else if (/payment/i.test(type) || /billing/i.test(type)) {
                        cat = 'billing';
                    }
                }
                if (cat === 'vault') {
                    if (/expired/i.test(type)) {
                        event = 'expires';
                    }
                } else if (cat === 'network') {
                    if (type === 'Invite Emergency') {
                        event = 'emergency-requesting';
                        attention = 'callbox';
                        actions = [
                            {
                                label: 'Accept',
                                event: 'network:emergency-accept'
                            },
                            {
                                label: 'Deny',
                                event: 'network:emergency-deny'
                            }
                        ];
                    } else if (type === 'Deny Emergency') {
                        event = 'emergency-denied';
                    } else if (type === 'Remove Emergency') {
                        event = 'emergency-removed';
                    } else if (type === 'Update Relationship') {
                        event = 'relationship-updated';
                    } else if (/invite/i.test(type)) {
                        event = 'requesting';
                        attention = 'callbox';
                        if (/email/i.test(type) && notif.hasOwnProperty('Options') && isMongoId(notif.Options)) {
                            notif.inviteId = notif.Options;
                        }
                        actions = [
                            {
                                label: 'Accept',
                                event: 'network:accept'
                            },
                            {
                                label: 'Deny',
                                event: 'network:deny'
                            }
                        ];
                    } else if (/accept/i.test(type)) {
                        event = 'accepted';
                    }
                } else if (cat === 'workflow') {
                    if (/invite/i.test(type)) {
                        event = 'requesting';
                        attention = 'callbox';
                        if (/email/i.test(type) && notif.hasOwnProperty('Options') && isMongoId(notif.Options)) {
                            notif.inviteId = notif.Options;
                        }
                        actions = [
                            {
                                label: 'Accept',
                                event: 'workflow:accept'
                            },
                            {
                                label: 'Deny',
                                event: 'workflow:deny'
                            }
                        ];
                    } else if (/accept/i.test(type)) {
                        event = 'accepted';

                    } else if (/cancel/i.test(type)) {
                        event = 'cancelled';
                        _resolveRequestById('workflow', mongoToId(notif.PreserveBag));
                    }
                }

                else if (cat === 'handshake') {
                    if (/invited/i.test(type)) {
                        event = 'inviting';
                        attention = 'callbox';
                        actions = [
                            {
                                label: 'Open Form',
                                event: 'handshake:inviting'
                            }
                        ];
                    } else if (/vault changed/i.test(type)) {
                        event = 'sync';
                        actions = [
                            {
                                label: 'Download',
                                event: 'handshake:sync'
                            }
                        ];
                    } else if (/request handshake/i.test(type)) {
                        event = 'request';
                    }
                }
                else if (cat === 'srfi') {
                    if (/invited/i.test(type)) {
                        event = 'inviting';
                        attention = 'callbox';
                        actions = [
                            {
                                label: 'Open Form',
                                event: 'srfi:inviting'
                            }
                        ];
                    }
                }
                else if (cat === 'push') {
                    if (/requested/i.test(notif.Title)) {
                        event = 'pushing';
                        attention = 'callbox';
                        actions = [
                            {
                                label: 'Open Form',
                                event: 'push:pushing'
                            }
                        ];
                    } else if (/vault changed/i.test(type)) {
                        event = 'sync';
                        actions = [
                            {
                                label: 'Download',
                                event: 'push:accept'
                            }
                        ];
                    }
                } else if (cat === 'delegation') {
                    if (/request/i.test(type)) {
                        event = 'requesting';
                        attention = 'callbox';
                        actions = [
                            {
                                label: 'View Request',
                                event: 'delegation:requesting'
                            }
                        ];
                    } else if (/accept/i.test(type)) {
                        event = 'accept';
                    } else if (/remove/i.test(type)) {
                        event = 'removed';
                        _resolveRequestById('delegation', notif.PreserveBag);
                    }
                }
                notif.category = cat;
                notif.event = event;
                notif.attention = attention;
                notif.actions = actions;
                notif.created = notif.DateTime;
                notif.hidden = false;

                notif.read = notif.Read;
                if (cat === 'vault') {
                    if (event === 'expires') {

                        if (addNew) {
                            // if (lastExpires) {
                            //     lastExpires.hidden = true;
                            //     lastExpires.read = true;
                            // }
                        } else {
                            // if (lastExpires) {
                            //     notif.hidden = true;
                            //     notif.read = true;
                            // } else {
                            //     lastExpires = notif;
                            // }
                        }

                    }
                } else if (cat === 'network') {
                    switch (event) {
                        case 'requesting':
                            notif.callText = 'Inviting you to join my network.';
                            if (notif.hasOwnProperty('inviteId')) {
                                notif.callText = 'Inviting you to join my network, following invitation by email.';
                            }
                            break;
                        case 'emergency-requesting':
                            notif.callText = 'Requesting to add you as emergency contact.';
                            break;
                    }
                } else if (cat === 'workflow') {
                    switch (event) {
                        case 'requesting':
                            notif.callText = 'Inviting you to join business workflow as ' + rgu.toTitleCase(notif.Payload);
                            if (notif.hasOwnProperty('inviteId')) {
                                notif.callText += ', following invitation by email';
                            }
                            break;
                    }
                }
                else if (cat === 'handshake') {
                    var interactionId = notif.PreserveBag;
                    if (interactions.hasOwnProperty(interactionId)) {
                        notif.interaction = interactions[interactionId];
                        if (event === notif.interaction.event) {

                        }
                    } else {
                        notif.interaction = {
                            event: event,
                            lastNotif: notif
                        };
                        interactions[interactionId] = notif.interaction;
                        $http.get('/api/CampaignService/GetCampaignInfor?campaignId=' + interactionId)
                            .success(function (data) {
                                angular.extend(notif.interaction, {
                                    id: data.CampaignId,
                                    name: data.Name,
                                    description: data.Description,
                                    image: data.Image,
                                    fromName: data.BusinessName,
                                    fromAvatar: data.BusinessImageUrl
                                });
                                // notif.interaction = interactions[interactionId];

                            });
                    }
                    switch (event) {
                        case 'inviting':
                            notif.callText = 'We invite you to join this handshake.';
                    }
                } else if (cat === 'delegation') {
                    switch (event) {
                        case 'requesting':
                            notif.callText = 'Requesting to delegate my information to you.';
                    }
                } else if (cat === 'push') {

                    switch (event) {
                        case 'pushing':
                            notif.callText = 'Requesting to push information to your vault.';
                    }

                } else if (cat === 'srfi') {
                    switch (event) {
                        case 'inviting':
                            notif.callText = notif.Content === 'srfi' ? 'Requesting for your information.' : 'Inviting you to join an interaction.';
                    }

                }
                else if (cat === 'vault') {

                }

                var newNotif = false;
                if (addNew) {
                    notifications.unshift(notif);
                    $rootScope.$broadcast(cat + ':notification', {
                        notification: notif
                    });
                    notif.read = false;
                    newNotif = notif;
                    $rootScope.$broadcast('notifications:update', {
                        newNotification: newNotif
                    });

                } else {
                    notifications.push(notif);
                }
                notifHashes[notif.Id] = true;

            };

            function _resolveRequestById(cat, id) {
                notifications.forEach(function (notif) {
                    if (notif.category !== cat) return;
                    var requestId = mongoToId(notif.PreserveBag);
                    switch (cat) {
                        case 'network':
                            if (notif.event === 'requesting' || notif.event === 'emergency-requesting') {
                                if (requestId === id) {

                                    notif.resolved = notif.read = true;
                                    $rootScope.$broadcast('network:resolved');
                                }
                            }
                            break;
                        case 'workflow':
                            if (true || notif.event === 'requesting' || notif.event === 'cancelled' || notif.event === 'accepted') {
                                if (requestId === id) {

                                    notif.resolved = notif.read = true;
                                    $rootScope.$broadcast('workflow:resolved');
                                }
                            }
                            break;
                        case 'delegation':
                            if (notif.event === 'requesting') {
                                if (requestId === id) {
                                    // console.log('resolved')
                                    notif.resolved = notif.read = true;
                                    $rootScope.$broadcast('delegation:resolved');
                                }
                            }
                            break;
                        case 'handshake':
                            if (notif.event === 'invite') {
                                if (requestId === id) {
                                    // console.log('resolved')
                                    notif.resolved = notif.read = true;
                                    $rootScope.$broadcast('handshake:resolved');
                                }
                            }
                            break;
                        case 'srfi':
                            if (notif.event === 'invite') {
                                if (requestId === id) {
                                    // console.log('resolved')
                                    notif.resolved = notif.read = true;
                                    $rootScope.$broadcast('srfi:resolved');
                                }
                            }
                            break;
                        case 'push':
                            if (notif.event === 'pushing') {
                                if (requestId === id) {
                                    notif.resolved = notif.read = true;
                                    $rootScope.$broadcast('push:resolved');
                                }
                            }
                            break;
                    }

                });
            }

            function _markRead(notif) {
                if (notif.read) return;
                notif.read = true;
                var url = '/api/Notifications/MarkRead/' + notif.Id;
                $http.post(url)
                    .success(function () {
                        // console.log('Marked read ' + notif.Id)
                    })
                    .error(function () {
                        console.log('Error marking read ' + notif.Id);
                    });
            }

            return {
                getNotifications: function () {
                    return notifications;
                },
                addNotification: _addNotification,
                markRead: _markRead,
                countUnread: function () {
                    var count = 0;
                    angular.forEach(notifications, function (notif) {
                        if (!notif.read) count++;
                    });
                    return count;
                },
                markAllRead: function () {
                    angular.forEach(notifications, function (notif) {
                        if (!notif.attention) {
                            _markRead(notif);
                        }
                    });
                },

                resolveRequestById: _resolveRequestById,

                pullNotifications: function (start, limit) {
                    start = start || 0;
                    limit = limit || 40;

                    var deferer = $q.defer();
                    var url = '/api/Notifications/GetNotifications/' + regitGlobal.activeAccountId;
                    // var url = '/api/Notifications/List';
                    $http.get(url, {
                        params: {
                            start: start,
                            take: limit
                        }
                    })
                        .then(function (response) {
                            // var response = r.data;
                            // if (!response.success) return;
                            if (angular.isArray(response.data)) {

                                notifications.splice(0);

                                angular.forEach(response.data, function (notif) {
                                    _addNotification(notif);
                                });

                                $rootScope.$broadcast('notifications:update', {
                                    notifications: response.data
                                });

                            }

                            return deferer.resolve(response.data);
                        }, function (response) {
                            console.log('Error pulling notifications: ', response);
                            return deferer.reject(response);
                        });

                    return deferer.promise;
                }

            };
        })

        .directive('notificationGroup', function ($http, $rootScope, $timeout, $uibModal, $moment, notifications, notificationService, NetworkService, rguCache, rguView, rguNotify, formService) {
            var cmpId = 1;

            return {
                restrict: 'EA',
                replace: true,
                templateUrl: function (elem, attrs) {
                    return attrs.view && attrs.view === 'full' ? '/Areas/regitUI/templates/notification-list.html?v=2' : '/Areas/regitUI/templates/notification-group.html?v=2';
                },

                link: function (scope, elem, attrs) {
                    scope.isFull = attrs.view && attrs.view === 'full';
                    scope.rguView = rguView;
                    scope.cmpId = cmpId++;
                    scope.view = {
                        unreadCount: 0,
                        opening: false,
                        showingCallbox: false
                    };
                    scope.notifications = [];

                    if (!scope.isFull) {

                        scope.openNotificationsPopover = function (evt) {
                            // evt.preventDefault();
                            evt.stopPropagation();
                            if (scope.view.opening) {
                                scope.closeNotificationsPopover();
                                return;
                            } else if (scope.view.showingCallbox) {
                                scope.closeCallbox();
                                scope.openNotificationsPopover();
                                return;
                            }
                            scope.rguView.closeMoreActions();
                            scope.view.opening = true;
                            scope.rguView.openingNotifications = true;
                        };
                        scope.closeNotificationsPopover = function (evt) {
                            scope.view.opening = false;
                            scope.markAllRead();
                        };
                        scope.openCallbox = function (evt) {
                            scope.view.opening = false;
                            scope.view.showingCallbox = true;
                        };
                        scope.closeCallbox = function (evt) {
                            scope.view.showingCallbox = false;
                        };
                        scope.$on('document::click', function (evt) {
                            scope.closeCallbox(evt);
                            scope.closeNotificationsPopover(evt);
                        });
                    } else {
                        scope.view.pageSize = 20;
                        scope.view.showCount = scope.view.pageSize;

                        scope.gotoSettings = function () {
                            location.href = '/User/Settings/#notifications';
                        };

                        scope.pullMoreNotifications = function () {
                            notificationService.pullNotifications();
                        };
                        scope.showMoreNotifications = function () {
                            scope.view.showCount += scope.view.pageSize;
                            notificationService.pullNotifications(0, scope.view.showCount);
                        };
                    }

                    scope.filterNotifications = function (notif) {
                        return !notif.hidden;
                    };


                    scope.markAllRead = function () {
                        angular.forEach(scope.notifications, function (notif) {
                            if (!notif.attention) {
                                notificationService.markRead(notif);
                            }
                        });
                        notificationService.markAllRead();
                    };
                    scope.markAllReadFull = function () {
                        angular.forEach(scope.notifications, function (notif) {
                            notificationService.markRead(notif);
                        });
                        notificationService.markAllRead();
                    };


                    scope.handleAction = function (action, notif) {

                        switch (action.event) {

                            case 'network:accept':
                                var invitationId = mongoToId(notif.PreserveBag);
                                NetworkService.AcceptInvitation({InvitationId: invitationId}).then(function () {
                                    rguNotify.add('You become friend with ' + notif.fromName);
                                    notificationService.resolveRequestById('network', invitationId);
                                    $rootScope.$broadcast('network:update');
                                }, function (errors) {
                                });
                                break;

                            case 'network:deny':
                                var invitationId = mongoToId(notif.PreserveBag);
                                swal({
                                    title: "Are you sure to decline this invitation?",
                                    type: "warning",
                                    showCancelButton: true,
                                    confirmButtonColor: "#DD6B55",
                                    confirmButtonText: "Yes",
                                    cancelButtonText: "No"
                                }).then(function () {
                                        NetworkService.DenyInvitation({InvitationId: invitationId})
                                            .then(function () {
                                                    rguNotify.add('You denied network invitation from ' + notif.fromName);
                                                    notificationService.resolveRequestById('network', invitationId);
                                                    $rootScope.$broadcast('network:update');
                                                    notif.resolved = true;
                                                },
                                                function (errors) {
                                                });

                                    }
                                    , function (dismiss) {
                                        if (dismiss === 'cancel') {
                                        }
                                    });
                                break;

                            case 'network:emergency-accept':
                                var invitationId = mongoToId(notif.PreserveBag);
                                var message = "<span  class='text-justify' style='font-size: 11'> <strong>" + notif.fromName + "</trong>" +
                                    " has invited you to be an emergency contact. By accepting, you authorize to share certain information with the requester on a continuous basis for emergency purposes." + "</span>";
                                swal({
                                    title: 'Emergency Contact Request',
                                    html: message,
                                    showCancelButton: true,
                                    showCloseButton: true,
                                    confirmButtonColor: "#DD6B55",
                                    confirmButtonText: "Accept",
                                    cancelButtonText: "Decline"
                                }).then(function () {
                                        NetworkService.AcceptTrustEmergency({
                                            InvitationId: invitationId,
                                            Relationship: '',
                                            IsEmergency: true,
                                            Rate: 1
                                        }).then(function () {
                                            rguNotify.add("You have accepted to be an emergency contact for " + notif.fromName + ".");
                                            notificationService.resolveRequestById('network', invitationId);
                                            $rootScope.$broadcast('network:update');
                                            notif.resolved = true;

                                        }, function (errors) {
                                        });
                                    }
                                    , function (dismiss) {
                                        if (dismiss === 'cancel') {
                                            NetworkService.DenyInvitation({InvitationId: invitation.Id})
                                                .then(function () {
                                                        rguNotify.add("You have declined emergency contact request from " + notif.fromName + " .");
                                                        notificationService.resolveRequestById('network', invitation.Id);
                                                        $rootScope.$broadcast('network:update');
                                                        notif.resolved = true;
                                                    },
                                                    function (errors) {
                                                    });
                                        }
                                    });


                                break;
                            case 'network:emergency-deny':
                                var invitationId = mongoToId(notif.PreserveBag);
                                swal({
                                    title: "Are you sure to decline this invitation?",
                                    type: "warning",
                                    showCancelButton: true,
                                    confirmButtonColor: "#DD6B55",
                                    confirmButtonText: "Yes",
                                    cancelButtonText: "No"
                                }).then(function () {
                                        NetworkService.DenyInvitation({InvitationId: invitationId})
                                            .then(function () {
                                                    rguNotify.add('You denied network invitation from ' + notif.fromName);
                                                    notificationService.resolveRequestById('network', invitationId);
                                                    $rootScope.$broadcast('network:update');
                                                    notif.resolved = true;
                                                },
                                                function (errors) {

                                                });

                                    }
                                    , function (dismiss) {
                                        if (dismiss === 'cancel') {
                                        }
                                    });

                                break;


                            case 'workflow:accept':
                                var invitationId = mongoToId(notif.PreserveBag);
                                $http.post('/Api/Workflow/AcceptInvitation/' + invitationId)
                                    .success(function (response) {
                                        rguNotify.add('You become workflow member to ' + notif.fromName);
                                        notificationService.resolveRequestById('workflow', invitationId);
                                        $rootScope.$broadcast('workflow:update');
                                        $rootScope.$broadcast('workflow:joined');
                                        regitGlobal.userAccount.isBusinessMember = true;
                                        regitGlobal.businessAccount = {
                                            hasBusinessMember: true,
                                            accountType: "Business",
                                            id: notif.fromAccountId,
                                            displayName: notif.fromName,
                                            avatar: notif.fromAvatar
                                        };
                                    }).error(function (errors, status) {
                                    console.log('Error joining workflow', errors);
                                });
                                break;

                            case 'workflow:deny':
                                var invitationId = mongoToId(notif.PreserveBag);
                                swal({
                                    title: "Are you sure to decline this workflow invitation?",
                                    type: "warning",
                                    showCancelButton: true,
                                    confirmButtonColor: "#DD6B55",
                                    confirmButtonText: "Yes",
                                    cancelButtonText: "No"
                                }).then(function () {
                                    $http.post('/Api/Workflow/DenyInvitation/' + invitationId)
                                        .success(function (response) {
                                            rguNotify.add('Denied workflow invitation');
                                            notificationService.resolveRequestById('workflow', invitationId);
                                            $rootScope.$broadcast('workflow:update');
                                        }).error(function (errors) {
                                        console.log('Error denying workflow invitation', errors);
                                    });
                                });

                                break;

                            case 'srfi:inviting':
                                var campaign = null;
                                var campaignId = notif.PreserveBag;

                                var termsUrl = '';

                                $http.get('/api/CampaignService/GetSRFIForRegis?campaignId=' + campaignId)
                                // $http.post('/api/CampaignService/GetSRFIForRegis',regis)
                                    .success(function (camp) {
                                        // var data = new Object();
                                        // data.BusinessUserId = camp.Id;
                                        // data.DisplayName = camp.DisplayName;
                                        // data.Avatar = camp.Avatar;
                                        // data.CampaignId = camp.CampaignId;
                                        // data.Fields = camp.Fields;
                                        // data.Name = camp.Name;
                                        // data.CampaignType = camp.Type;
                                        // data.Comment = camp.Comment;
                                        // data.Status = camp.Status;
                                        // data.NotificationId = notif.Id;
                                        // data.UserId = notif.ToAccountId;
                                        // var modalInstance = $uibModal.open({
                                        //     templateUrl: '/Areas/User/Views/Shared/Template/srfi-regisform.html',
                                        //     controller: 'SRFIFormRegisUserController',
                                        //     size: "",
                                        //     backdrop: 'static',
                                        //     resolve: {
                                        //         registerPopup: function () {
                                        //             return {
                                        //                 data: data
                                        //             };
                                        //         }
                                        //     }
                                        // });
                                        // console.log(camp)
                                        var interaction = scope.interaction = {
                                            id: camp.CampaignId,
                                            type: 'srfi',
                                            business: {
                                                accountId: camp.UserId,
                                                name: camp.DisplayName,
                                                avatar: camp.Avatar
                                            },
                                            name: camp.Name,
                                            description: camp.Comment,
                                            verb: camp.Verb || 'submit',
                                            termsUrl: camp.TermsUrl,
                                            fields: camp.Fields
                                        };
                                        scope.interactionForm = {
                                            source: 'notification'
                                        };
                                        formService.openInteractionForm(interaction, scope);
                                    });


                                break;
                            case 'handshake:inviting':
                                var campaign = null;
                                var campaignId = notif.PreserveBag;

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
                                                    var fields = response.ListOfFields,
                                                        campaignFields = response.Campaign.Fields;

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
                                                                };
                                                                response.ListOfFields[index].model = mode;
                                                                break;
                                                            case "address":
                                                                var mode = {
                                                                    address: response.ListOfFields[index].model,
                                                                    address2: response.ListOfFields[index].unitModel
                                                                };
                                                                response.ListOfFields[index].model = mode;
                                                                break;

                                                            case "doc":
                                                                var m = [];
                                                                $(response.ListOfFields[index].modelarrays).each(function (index, fname) {
                                                                    m.push({
                                                                        fname: fname
                                                                    });

                                                                });
                                                                response.ListOfFields[index].model = m;
                                                                break;
                                                        }
                                                    });

                                                    // START
                                                    // re-order form fields by vault group


                                                    var myfields = [];
                                                    var vaultgroup = {
                                                        "data": [
                                                            {"id": "0", "name": ".basicInformation"},
                                                            {"id": "1", "name": ".contact"},
                                                            {"id": "2", "name": ".address"},
                                                            {"id": "3", "name": ".financial"},
                                                            {"id": "4", "name": ".governmentID"},
                                                            {"id": "5", "name": ".family"},
                                                            {"id": "6", "name": ".membership"},
                                                            {"id": "7", "name": ".employment"},
                                                            {"id": "8", "name": ".education"},
                                                            {"id": "9", "name": ".others"},
                                                            {"id": "10", "name": "Custom"},
                                                            {"id": "11", "name": "undefined"}
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
                                                    //
                                                    // var modalInstance = $uibModal.open({
                                                    //     templateUrl: 'modal-feed-open-reg-handshake.html',
                                                    //     controller: 'RegistrationHandshakeOnNewFeedController',
                                                    //     size: "",
                                                    //     backdrop: 'static',
                                                    //
                                                    //     resolve: {
                                                    //         registerPopup: function () {
                                                    //             return {
                                                    //                 ListOfFields: response.ListOfFields
                                                    //                 ,
                                                    //                 BusinessUserId: response.BusinessUserId
                                                    //                 ,
                                                    //                 CampaignId: response.CampaignId
                                                    //                 ,
                                                    //                 CampaignType: response.CampaignType
                                                    //                 ,
                                                    //                 campaign: campaign
                                                    //                 ,
                                                    //                 BusinessIdList: null
                                                    //                 ,
                                                    //                 CampaignIdInPostList: null
                                                    //                 ,
                                                    //                 posthndshakecomment: notif.Content
                                                    //                 ,
                                                    //                 statusjoin: statusjoin
                                                    //
                                                    //             };
                                                    //         }
                                                    //     }
                                                    // });

                                                    // modalInstance.result.then(function (campaign) {
                                                    // }, function () {
                                                    // });

                                                    scope.interaction = {
                                                        id: response.CampaignId,
                                                        type: 'handshake',
                                                        business: {
                                                            accountId: response.BusinessUserId,
                                                            name: response.Campaign.BusinessName,
                                                            avatar: response.Campaign.BusinessImageUrl
                                                        },
                                                        name: response.Campaign.Name,
                                                        description: response.Campaign.Description,
                                                        verb: response.Campaign.Verb || 'submit',
                                                        termsUrl: response.Campaign.termsAndConditionsFile,
                                                        fields: response.ListOfFields
                                                    };
                                                    formService.openInteractionForm(scope.interaction, scope);
                                                })
                                                .error(function (errors, status) {
                                                    console.log('Error joining handshake: ', errors);
                                                    // $scope.listcampaigns = response.NewFeedsItemsList;
                                                    // $scope.CampaignUserId = response.UserId;
                                                    // $scope.CurrentBusinessUserId = response.BusinessUserId;

                                                });
                                        }

                                    }).error(function (errors, status) {
                                });
                                break;

                            case 'handshake:sync':

                                var items = [];
                                var campaign = null;
                                var campaignId = notif.PreserveBag;

                                $http.get('/api/CampaignService/GetCampaignInfor?campaignId=' + campaignId)
                                    .success(function (info) {
                                        campaign = info;
                                        if (campaign) {
                                            var data = new Object();
                                            data.BusinessUserId = campaign.BusinessUserId;
                                            data.CampaignId = campaign.CampaignId;
                                            data.CampaignType = campaign.CampaignType;
                                            data.campaign = campaign;
                                            data.UserId = notif.FromAccountId;
                                            var dataresult;
                                            $http.post("/api/CampaignService/GetUserInformationForCampaign", data)
                                                .success(function (response) {
                                                    dataresult = response;
                                                    var fields = response.ListOfFields,
                                                        campaignFields = response.Campaign.Fields;

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
                                                                });
                                                                break;
                                                            case "location":
                                                                items.push({
                                                                    "Field Name": "Country",
                                                                    NewValue: response.ListOfFields[index].model,
                                                                    OldValue: response.ListOfFieldsOld[index] == undefined ? "" : response.ListOfFieldsOld[index].model,
                                                                    "Update Date": response.handshakeupadte
                                                                });
                                                                items.push({
                                                                    "Field Name": "City",
                                                                    NewValue: response.ListOfFields[index].unitModel,
                                                                    OldValue: response.ListOfFieldsOld[index] == undefined ? "" : response.ListOfFieldsOld[index].unitModel,
                                                                    "Update Date": response.handshakeupadte

                                                                });

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
                                                    alasql('SELECT * INTO XLSX("' + notif.FromUserDisplayName + '.xlsx",{headers:true}) FROM ?', [items]);
                                                })
                                                .error(function (errors, status) {

                                                });
                                        }

                                    }).error(function (errors, status) {
                                });
                                break;

                            case 'push:pushing':

                                var campaign = null;
                                var campaignId = notif.PreserveBag;
                                var businessname = "";
                                var businessavatar = "";
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
                                                    var fields = response.ListOfFields,
                                                        campaignFields = response.Campaign.Fields;

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
                                                                };
                                                                response.ListOfFields[index].model = mode;
                                                                break;
                                                            case "address":
                                                                var mode = {
                                                                    address: response.ListOfFields[index].model,
                                                                    address2: response.ListOfFields[index].unitModel
                                                                };
                                                                response.ListOfFields[index].model = mode;
                                                                break;
                                                            case "doc":
                                                                var m = [];
                                                                $(response.ListOfFields[index].modelarrays).each(function (index, fname) {
                                                                    m.push({
                                                                        fname: fname
                                                                    });
                                                                });
                                                                response.ListOfFields[index].model = m;
                                                                break;
                                                        }
                                                    });

                                                    var modalInstance = $uibModal.open({
                                                        animation: true,
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
                                                                    BusinessIdList: scope.BusinessIdList
                                                                    ,
                                                                    CampaignIdInPostList: scope.CampaignIdInPostList
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

                                                });
                                        }

                                    }).error(function (errors, status) {
                                });
                                break;
                            case 'delegation:requesting':

                                var delegationId = notif.PreserveBag;
                                var modalInstance = $uibModal.open({
                                    animation: true,
                                    templateUrl: 'modal-delegate-view.html',
                                    controller: 'ViewDelegationMasterController',
                                    size: "",
                                    resolve: {
                                        objectdelegationId: function () {
                                            return {
                                                Id: delegationId
                                            };
                                        }
                                    }
                                });
                                modalInstance.result.then(function () {
                                }, function () {
                                });
                                break;

                        }
                        notificationService.markRead(notif);
                    };


                    scope.$on('notifications:update', function (event, data) {
                            scope.fromAccounts = {};
                            scope.notifications.splice(0);
                            angular.forEach(notifications, function (notif) {

                                if (notif.hidden) {
                                    return;
                                }

                                /*                                if (notif.FromAccountId) {
                                                                    rguCache.getUserAsync(notif.FromAccountId).then(function (user) {
                                                                        if (user)
                                                                        $timeout(function () {
                                                                            notif.fromName = user.displayName;
                                                                            notif.fromAvatar = user.avatar;
                                                                        });
                                                                    });
                                                                }*/
                                var diff = $moment().diff(notif.created, 'seconds');
                                notif.recent = false;
                                if (diff < 3600 * 24) {
                                    notif.recent = true;
                                    if (diff < 0) {
                                        notif.created = new Date();
                                    }
                                }
                                scope.notifications.push(notif);
                            });
                            if (!scope.isFull) {
                                scope.closeCallbox();
                            }
                            scope.view.unreadCount = data.unreadCount;
                            if (data.newNotification) {
                                var notif = data.newNotification;
                                if (!scope.isFull && notif.attention === 'callbox') {
                                    if (notif.category === 'handshake') {
                                        notif.fromName = notif.interaction.fromName;
                                        notif.fromAvatar = notif.interaction.fromAvatar;
                                    } else {

                                    }
                                    scope.view.callingNotification = notif;
                                    scope.openCallbox();

                                }
                                else {
                                    rguNotify.add(notif.text);
                                }
                            } else {

                            }
                        }
                    );
                }
            };
        });

})();