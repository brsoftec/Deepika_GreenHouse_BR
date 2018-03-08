angular.module('users', ['regit.ui'])
    .directive('rguUserGrid', function ($rootScope, $timeout, $http, $q, NetworkService, notificationService, rguView, rguNotify) {
        return {
            restrict: 'EA',
            scope: {
                type: '@',
                model: '<source'
            },
            templateUrl: '/Areas/regitUI/templates/user-grid.html?v=9',
            link: function (scope, elem, attrs) {

                scope.users = [];
                scope.invites = [];
                scope.view = {
                    loaded: false,
                    editing: false,
                    editingUser: null,
                    showingAddMember: true
                };
                scope.rguView = rguView;
                scope.sortUsers = function (user) {
                    var rank = 0;
                    if (!user.trusted) {
                        rank = user.incoming ? 1 : (user.pending ? 2 : 3);
                    }
                    return rank;
                };

                scope.beginEdit = function (user, evt) {
                    scope.view.editing = true;
                    scope.view.editingUser = user;
                    var tr = $(evt.target).closest('tr');
                    tr.addClass('editing');
                };
                scope.endEdit = function (user, evt) {
                    scope.view.editing = false;
                    scope.view.editingUser = null;
                    if (evt) {
                        var tr = $(evt.target).closest('tr');
                        tr.removeClass('editing');
                    }
                };
                scope.cancelEdit = function (user, evt) {
                    scope.endEdit(user, evt);
                };
                scope.saveEdit = function (user, evt) {
                    scope.doSaveEdit(user);
                    scope.endEdit(user, evt);
                };
                scope.removeUser = function (user, evt) {
                    scope.doRemoveUser(user);
                };


                scope.relationships = ['Father', 'Mother', 'Son', 'Daughter', 'Brother', 'Sister', 'Cousin', 'Husband', 'Wife', 'Uncle', 'Aunt', 'Niece',
                    'Nephew', 'Grandmother', 'Grandfather', 'Friend', 'Father-in-law', 'Mother-in-law', 'In-law', 'Other'];

                scope.networkConfig = {
                    trustedNetworkId: null,
                    normalNetworkId: null
                };

                scope.resendRequest = function (user) {
                    NetworkService.InviteFriend({ReceiverId: user.id}).then(function () {

                        rguNotify.add('Network invitation resent to ' + user.displayName);

                    }, function (errors) {

                    });
                };

                scope.addEmergency = function (user) {
                    var msg = "You are inviting " + user.displayName + " to be your emergency contact. If accepted by the recipient, you will be able to see their basic contact information and use it for registration purposes.";
                    swal({
                        title: 'Add Emergency Contact',
                        html: msg,
                        type: "warning",
                        showCancelButton: true,
                        confirmButtonColor: "#DD6B55",
                        confirmButtonText: "Yes",
                        cancelButtonText: "No"
                    }).then(function () {
                        NetworkService.InviteTrustEmergency({
                            ReceiverId: user.id,
                            Relationship: user.relationship,
                            IsEmergency: true,
                            Rate: 1
                        }).then(function () {
                            rguNotify.add('Sent emergency contact request to ' + user.displayName);
                            updateNetwork();
                        }, function (errors) {
                        })
                    }, function (dismiss) {
                        if (dismiss === 'cancel') {
                        }
                    });
                };

                scope.removeEmergency = function (user) {
                    var msg = "Are you sure you want to remove " + user.displayName + " as your emergency contact?";
                    swal({
                        title: msg,
                        type: "warning",
                        showCancelButton: true,
                        confirmButtonColor: "#DD6B55",
                        confirmButtonText: "Yes",
                        cancelButtonText: "No"
                    }).then(function () {
                        var model = {
                            Id: user.id,
                            DisplayName: user.displayName,
                            NetworkId: user.networkId,
                            UserId: user.accountId,
                            Relationship: user.relationship,
                            IsEmergency: false
                        };
                        $http.post('/Api/Networks/UpdateTrustEmergency', model)
                            .success(function () {
                                rguNotify.add('Removed ' + user.displayName + ' as emergency contact.');
                                updateNetwork();
                            }).error(function (errors, status) {
                            console.log('Error removing emergency contact');
                        });
                    }, function (dismiss) {
                        if (dismiss === 'cancel') {
                        }
                    });
                };

                scope.acceptEmergencyRequest = function (user) {
                    var message = "<span  class='text-justify' style='font-size: 11'> <strong>" + user.displayName + "</trong>" +
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
                                InvitationId: user.emergency.invitation.Id,
                                Relationship: user.emergency.invitation.Relationship,
                                IsEmergency: true,
                                Rate: user.emergency.invitation.Rate
                            }).then(function () {
                                rguNotify.add("You have accepted to be an emergency contact for " + user.displayName + " .");
                                updateNetwork();

                            }, function (errors) {
                            })
                        }
                        , function (dismiss) {
                            if (dismiss === 'cancel') {
                                NetworkService.DenyInvitation({InvitationId: invitation.Id})
                                    .then(function () {
                                            rguNotify.add("You have declined emergency contact request from " + user.displayName + " .");
                                            updateNetwork();
                                        },
                                        function (errors) {
                                        });
                            }
                        });

                };
                scope.denyEmergencyRequest = function (user) {
                    var message = "Are you sure you decline to be an emergency contact for " + user.displayName + "?";
                    swal({
                        title: 'Decline Emergency Contact Request',
                        html: message,
                        showCancelButton: true,
                        showCloseButton: true,
                        confirmButtonColor: "#DD6B55",
                        confirmButtonText: "Decline",
                        cancelButtonText: "Cancel"
                    }).then(function () {
                            NetworkService.DenyInvitation({InvitationId: user.emergency.invitation.Id})
                                .then(function () {
                                        rguNotify.add("You have declined emergency contact request from " + user.displayName + " .");
                                        updateNetwork();
                                    },
                                    function (errors) {
                                    });
                        }
                        , function (dismiss) {
                            if (dismiss === 'cancel') {

                            }
                        });

                };

                scope.doSaveEdit = function (user) {
                    var model = {
                        Id: user.id,
                        DisplayName: user.displayName,
                        NetworkId: user.networkId,
                        UserId: user.accountId,
                        Relationship: user.relationship
                    };
                    $http.post('/Api/Networks/UpdateNetworkRelationship', model)
                        .success(function () {
                            rguNotify.add('Updated ' + user.displayName + '\'s relationship to ' + user.relationship);
                        }).error(function (errors, status) {
                    });

                };

                scope.viewDelegationDetails = function (user) {
                    location.href = '/DelegationManager';
                };
                scope.addDelegatee = function (user) {
                    location.href = '/DelegationManager';
                };

                function pullDelegations() {
                    scope.users.forEach(function (user) {
                        if (angular.isDefined(user.delegations)) {
                            user.delegations.splice(0);
                        }
                    });
                    $http.post('/api/DelegationManager/GetListDelegationFull', {Direction: 'DelegationIn'})
                        .success(function (response) {
                            if (!angular.isArray(response.Listitems)) return;
                            var delegations = response.Listitems;

                            delegations.forEach(function (delegation) {
                                var permissions = [];
                                var role = delegation.DelegationRole;
                                if (role !== 'Normal') {
                                    permissions = delegation.GroupVaultsPermission;
                                }
                                scope.users.forEach(function (user) {

                                    if (user.accountId === delegation.FromAccountId) {
                                        user.delegations.push({
                                            outgoing: false,
                                            id: delegation.DelegationId,
                                            role: role,
                                            since: delegation.EffectiveDate,
                                            expire: delegation.ExpiredDate,
                                            status: delegation.Status,
                                            pending: delegation.Status === 'Pending',
                                            permissions: permissions
                                        });
                                        user.delegated = true;
                                    }
                                });

                            });

                        }).error(function (errors, status) {
                    });

                    $http.post('/api/DelegationManager/GetListDelegationFull', {Direction: 'DelegationOut'})
                        .success(function (response) {
                            if (!angular.isArray(response.Listitems)) return;
                            var delegations = response.Listitems;
                            delegations.forEach(function (delegation) {
                                scope.users.forEach(function (user) {
                                    if (user.accountId === delegation.ToAccountId) {
                                        user.delegations.push({
                                            outgoing: true,
                                            id: delegation.DelegationId,
                                            role: delegation.DelegationRole,
                                            since: delegation.EffectiveDate,
                                            expire: delegation.ExpiredDate,
                                            status: delegation.Status,
                                            pending: delegation.Status === 'Pending'
                                        });
                                        user.delegating = true;
                                    }
                                });

                            });

                        }).error(function (errors, status) {
                    });
                }


                function updateInvites() {
                    $rootScope.$broadcast('network:invites-update');
                }

                function updateNetwork() {

                    scope.cancelEdit();
                    var inRequests = [];
                    var outRequests = [];

                    function addPending(incoming) {
                        var invitations = incoming ? inRequests : outRequests;
                        if (!invitations.length) return;
                        invitations.forEach(function (invitation) {
                            var isEmergency = false;
                            var emergency = {};
                            if (invitation.hasOwnProperty('IsEmergency') || invitation.NetworkName === 'Emergency Network' && invitation.Status === 'Pending') {
                                isEmergency = true;
                                emergency = {
                                    status: 'pending',
                                    incoming: !!incoming,
                                    sent: invitation.SentAt,
                                    invitation: invitation
                                };
                            }
                            var existing = false;
                            var user = incoming ? invitation.Sender : invitation.Receiver;
                            scope.users.forEach(function (member) {
                                if (user.Id === member.id) {
                                    if (isEmergency) {
                                        member.isEmergency = true;
                                        member.emergency = emergency;
                                    } else {

                                    }
                                    existing = true;
                                }
                            });
                            if (!existing) {
                                var pendingUser = {
                                    id: user.Id,
                                    displayName: user.DisplayName,
                                    avatar: user.Avatar,
                                    pending: true,
                                    sent: invitation.SentAt || null,
                                    incoming: !!incoming,
                                    invitation: invitation
                                };
                                if (invitation.hasOwnProperty('InviteId')) {
                                    pendingUser.inviteId = invitation.InviteId;
                                }
                                scope.users.push(pendingUser);
                            }
                        });
                    }

                    NetworkService.GetNetworks().then(function (networks) {
                            if (!angular.isArray(networks)) return;

                            var trustedNetwork = networks[0];
                            var trustedNetworkId = trustedNetwork.Id;
                            scope.networkConfig.trustedNetworkId = trustedNetworkId;
                            var trustedMembers = [];

                            var network = networks[1];
                            var networkId = network.Id;
                            scope.networkConfig.normalNetworkId = networkId;
                            var normalMembers = [];

                            //  Pull Trust network members
                            var pullTrustedMembers = NetworkService.GetFriendsInNetwork(trustedNetworkId);

                            //  Pull Normal network members
                            var pullNormalMembers = NetworkService.GetFriendsInNetwork(networkId);

                            $q.all({trusted: pullTrustedMembers, normal: pullNormalMembers}).then(function (result) {
                                trustedMembers = result.trusted.Friends.map(function (member) {
                                    var emergency = {};
                                    if (member.IsEmergency) {
                                        emergency.rate = member.Rate;
                                        emergency.status = member.Status;
                                    }
                                    return {
                                        id: member.Id,
                                        accountId: member.UserId,
                                        displayName: member.DisplayName,
                                        firstName: member.FirstName,
                                        lastName: member.LastName,
                                        avatar: member.Avatar,
                                        email: member.Email,
                                        relationship: member.Relationship,
                                        isEmergency: member.IsEmergency,
                                        emergency: emergency,
                                        profileUrl: '/User/Profile?id=' + member.Id,
                                        trusted: true,
                                        networkId: trustedNetworkId,
                                        delegations: []
                                    };

                                });

                                normalMembers = result.normal.Friends.map(function (member) {
                                    return {
                                        id: member.Id,
                                        accountId: member.UserId,
                                        displayName: member.DisplayName,
                                        firstName: member.FirstName,
                                        lastName: member.LastName,
                                        avatar: member.Avatar,
                                        email: member.Email,
                                        relationship: member.Relationship,
                                        profileUrl: '/User/Profile?id=' + member.Id,
                                        networkId: networkId,
                                        delegations: []
                                    };
                                });
                                scope.users = trustedMembers.concat(normalMembers);

                                NetworkService.GetSendInvitations(true).then(function (data) {
                                    outRequests = data;
                                    addPending();
                                    NetworkService.GetInvitations(null, true).then(function (data) {
                                        inRequests = data;
                                        addPending(true);
                                        scope.view.loaded = true;
                                        pullDelegations();
                                    });
                                });
                            });

                        },
                        function (error) {
                            console.log('Error loading networks', error);
                        });
                }

                updateNetwork();
                updateInvites();

                scope.$on('network:notification', function (event, data) {
                    updateNetwork();
                });
                scope.$on('network:resolved', function (event, data) {
                    updateNetwork();
                });
                scope.$on('delegation:notification', function (event, data) {
                    pullDelegations();
                });
                scope.$on('delegation:resolved', function (event, data) {
                    pullDelegations();
                });
                scope.$on('network:update', function (event, data) {
                    updateNetwork();
                });

                scope.openAddMember = function () {
                    scope.view.showingAddMember = true;
                };
                scope.closeAddMember = function () {
                    scope.view.showingAddMember = false;
                };
                scope.toggleAddMember = function () {
                    scope.view.showingAddMember = !scope.view.showingAddMember;
                };

                scope.inviteMember = function (user) {
                    if (user.invite) {
                        updateInvites();
                        return;
                    }
                    $http.post('/Api/Networks/Invite', {
                        ReceiverId: user.id
                    })
                        .success(function (response) {
                            rguNotify.add('Invited ' + user.displayName + ' to your network');
                            scope.closeAddMember();
                            updateNetwork();
                        }).error(function (errors, status) {
                        console.log('Error inviting network member', errors)
                    });

                };

                scope.acceptRequest = function (user) {
                    var invitation = user.invitation;
                    NetworkService.AcceptInvitation({InvitationId: invitation.Id}).then(function () {
                        rguNotify.add('You become friend with ' + user.displayName);

                        updateNetwork();
                        notificationService.resolveRequestById('network', invitation.Id);

                    }, function (errors) {
                    });
                };

                scope.denyRequest = function (user) {
                    var invitation = user.invitation;
                    swal({
                        title: "Are you sure to decline this invitation?",
                        type: "warning",
                        showCancelButton: true,
                        confirmButtonColor: "#DD6B55",
                        confirmButtonText: "Yes",
                        cancelButtonText: "No"
                    }).then(function () {

                            NetworkService.DenyInvitation({InvitationId: invitation.Id})
                                .then(function () {
                                        rguNotify.add('You denied network invitation from ' + user.displayName);
                                        updateNetwork();
                                        notificationService.resolveRequestById('network', invitation.Id);

                                    },
                                    function (errors) {
                                    });

                        }
                        , function (dismiss) {
                            if (dismiss === 'cancel') {
                            }
                        });
                };

                scope.trustUser = function (user) {
                    NetworkService.MoveFriend({
                        FriendId: user.id,
                        FromNetworkId: user.networkId,
                        ToNetworkId: scope.networkConfig.trustedNetworkId
                    })
                        .then(function () {
                                rguNotify.add('Moved ' + user.displayName + ' to Trust Network.');
                                updateNetwork();
                            },
                            function (errors) {
                                swal('Move friend', 'error', 'Error');
                            });
                };
                scope.untrustUser = function (user) {
                    NetworkService.MoveFriend({
                        FriendId: user.id,
                        FromNetworkId: scope.networkConfig.trustedNetworkId,
                        ToNetworkId: scope.networkConfig.normalNetworkId
                    })
                        .then(function () {
                                rguNotify.add('Moved ' + user.displayName + ' to Normal Network.');
                                updateNetwork();
                            },
                            function (errors) {
                                console.log('Error untrusting network member')
                            });
                };

                scope.doRemoveUser = function (user) {
                    var message = '<p>' + $rootScope.translate('Are_You_Sure_To_Remove') + ' ' + user.displayName + " from your network?</p>";
                    if (user.delegations.length) {
                        message += '<p>You have ' + user.delegations.length + ' delegation relationship(s) with this user, all of which will be deleted as well.</p>'
                    }
                    swal({
                        title: 'Remove Network Member',
                        html: message,
                        type: "warning",
                        showCancelButton: true,
                        confirmButtonColor: "#DD6B55",
                        confirmButtonText: "Yes",
                        cancelButtonText: "No"
                    }).then(function () {
                            NetworkService.RemoveFriend({FriendId: user.id, NetworkId: user.networkId})
                                .then(function () {
                                        rguNotify.add('Removed ' + user.displayName + ' from network');
                                        updateNetwork();
                                    },
                                    function (errors) {
                                        swal('Remove friend', 'Error', 'error');
                                    });
                            updateNetwork();
                        }
                        , function (dismiss) {
                            if (dismiss === 'cancel') {
                            }
                        });

                };
                scope.cancelRequest = function (user) {
                    var message = 'Are you sure you want to cancel invitation to add ' + user.displayName + " to your network?</p>";
                    swal({
                        title: 'Cancel Network Request',
                        html: message,
                        type: "warning",
                        showCancelButton: true,
                        confirmButtonColor: "#DD6B55",
                        confirmButtonText: "Yes",
                        cancelButtonText: "No"
                    }).then(function () {
                            $http.post('/Api/Networks/Invite/Cancel', {InvitationId: user.invitation.Id})
                                .success(function () {
                                    rguNotify.add('Cancelled network invitation to ' + user.displayName);
                                    updateNetwork();
                                }).error(function (errors, status) {

                            });

                        }
                        , function (dismiss) {
                            if (dismiss === 'cancel') {
                            }
                        });

                }


            }
        };
    })

    .directive('inviteGrid', function ($rootScope, $timeout, $http, $q, notificationService, rguCache, rguView, rguNotify) {
        return {
            restrict: 'EA',
            scope: {
                type: '@',
                invites: '='
            },
            templateUrl: '/Areas/regitUI/templates/invite-grid.html?v=3',
            link: function (scope, elem, attrs) {
                var cat = attrs.type;
                var isNetwork = cat === 'network';
                var isWorkflow = cat === 'workflow';
                var isCustomers = cat === 'customer';
                var isBusiness = isWorkflow || isCustomers;
                scope.invites = scope.invites || [];
                scope.view = {};
                scope.rguView = rguView;
                scope.sortInvites = function (invite) {
                    return invite.sent;
                };

                function updateInvites() {

                    $http.get('/Api/Invite/Invites', {
                        params: {
                            fromUserId: isBusiness ? regitGlobal.businessAccount.id : regitGlobal.userAccount.id,
                            category: cat
                        }
                    })
                        .success(function (response) {
                            scope.invites.splice(0);
                            angular.forEach(response, function (invite) {
                                    var newInvite = {
                                        id: invite.Id,
                                        email: invite.Email,
                                        sent: invite.Sent
                                    };
                                    if (invite.hasOwnProperty('Payload')) {
                                        var payload = $.parseJSON(invite.Payload);
                                        if (angular.isObject(payload)) {
                                            if (isWorkflow && payload.hasOwnProperty('roles')) {
                                                {
                                                    newInvite.roles = payload.roles.map(function (roleName) {
                                                        return {
                                                            id: '',
                                                            name: roleName
                                                        }
                                                    });
                                                }
                                            }
                                        }
                                    }

                                    scope.invites.push(newInvite);
                                }
                            );
                        }).error(function (errors, status) {
                        console.log(errors)
                    });
                }

                updateInvites();

                scope.$on('invites:update', function (event, data) {
                    updateInvites();
                });
                scope.$on(cat + ':invites-update', function (event, data) {
                    updateInvites();
                });

                scope.resendInvite = function (invite) {
                    $http.post('/Api/Invite/ResendInvite/' + invite.id)
                        .success(function (response) {
                            rguNotify.add('Resent ' + cat + ' email invitation to ' + invite.email);
                            updateInvites();
                        }).error(function (errors, status) {
                        console.log('Error cancelling ' + cat + ' email invitation', errors)
                    });
                };
                scope.cancelInvite = function (invite) {
                    $http.post('/Api/Invite/DeleteInvite/' + invite.id)
                        .success(function (response) {
                            rguNotify.add('Cancelled ' + cat + ' email invitation to ' + invite.email);
                            updateInvites();
                        }).error(function (errors, status) {
                        console.log('Error cancelling ' + cat + ' email invitation', errors)
                    });
                };


            }
        };
    })

    .directive('delegationGrid', function ($rootScope, $timeout, $http, $q, $uibModal, NetworkService, VaultService, notificationService, rguCache, rguView, rguNotify) {
        return {
            restrict: 'EA',
            scope: {
                type: '@',
                selectDelegation: '=?'

            },
            templateUrl: '/Areas/regitUI/templates/delegation-grid.html?v=5',
            link: function (scope, elem, attrs) {

                scope.delegations = [];
                scope.view = {
                    loaded: false,
                    expanded: false,
                    expandedDelegation: null
                };
                scope.rguView = rguView;
                scope.sortDelegations = function (delegation) {
                    var rank = 3;
                    if (delegation.pending) {
                        rank = delegation.outgoing ? 2 : 1;
                    }
                    return rank;
                };

                scope.expandRow = function (delegation, evt) {
                    scope.view.expanded = true;
                    scope.view.expandedDelegation = delegation;
                    scope.selectDelegation = scope.view.selectedDelegation = scope.currentDelegation = delegation.model;
                    scope.initDelegationVault(delegation.model);
                    var tr = $(evt.target).closest('tr');
                    tr.addClass('expanded');
                };
                scope.collapseRow = function (delegation, evt) {
                    scope.view.expanded = false;
                    scope.view.expandedDelegation = null;
                    if (evt) {
                        var tr = $(evt.target).closest('tr');
                        tr.removeClass('expanded');
                    }
                };
                scope.showVaultViewer = function (delegation) {
                    delegation.view.showingVault = true;
                    if (delegation.role === 'Emergency' && delegation.status === 'Activated' && !delegation.outgoing) {
                        $rootScope.$broadcast('delegation:emergency-activated');
                    }
                };
                scope.hideVaultViewer = function (delegation) {
                    delegation.view.showingVault = false;
                };

                function updateDelegations() {
                    scope.delegations.splice(0);
                    if (attrs.type === 'in') {
                        $http.post('/api/DelegationManager/GetListDelegationFull', {Direction: 'DelegationIn'})
                            .success(function (response) {
                                if (!angular.isArray(response.Listitems)) return;
                                scope.delegations = response.Listitems.map(function (delegation) {
                                    var permissions = [];
                                    var role = delegation.DelegationRole;
                                    if (role === 'Custom' || role === 'Emergency') {
                                        permissions = delegation.GroupVaultsPermission;
                                    }
                                    var del = {
                                        outgoing: false,
                                        id: delegation.DelegationId,
                                        role: role,
                                        since: delegation.EffectiveDate,
                                        expires: delegation.ExpiredDate,
                                        status: delegation.Status,
                                        pending: delegation.Status === 'Pending',
                                        permissions: permissions,
                                        message: delegation.Message,
                                        model: delegation,
                                        user: {},
                                        view: {}
                                    };
                                    rguCache.getUserAsync(delegation.FromAccountId)
                                        .then(function (user) {
                                            del.user = user;
                                        });
                                    return del;

                                });
                                scope.view.loaded = true;

                            }).error(function (errors, status) {
                        });
                    } else if (attrs.type === 'out') {

                        $http.post('/api/DelegationManager/GetListDelegationFull', {Direction: 'DelegationOut'})
                            .success(function (response) {
                                if (!angular.isArray(response.Listitems)) return;
                                response.Listitems.forEach(function (delegation) {
                                    scope.delegations = response.Listitems.map(function (delegation) {
                                        var permissions = [];
                                        var role = delegation.DelegationRole;
                                        if (role === 'Custom' || role === 'Emergency') {
                                            permissions = delegation.GroupVaultsPermission;
                                        }
                                        var del = {
                                            outgoing: true,
                                            id: delegation.DelegationId,
                                            role: role,
                                            since: delegation.EffectiveDate,
                                            expires: delegation.ExpiredDate,
                                            status: delegation.Status,
                                            pending: delegation.Status === 'Pending',
                                            permissions: permissions,
                                            message: delegation.Message,
                                            model: delegation,
                                            user: {},
                                            view: {}
                                        };
                                        rguCache.getUserAsync(delegation.ToAccountId)
                                            .then(function (user) {
                                                del.user = user;
                                            });
                                        return del;

                                    });

                                });
                                scope.view.loaded = true;

                            }).error(function (errors, status) {
                        });
                    }
                }

                updateDelegations();

                scope.$on('delegation:notification', function (event, data) {
                    updateDelegations();
                });
                scope.$on('delegation:resolved', function (event, data) {
                    updateDelegations();
                });
                scope.$on('delegation:update', function (event, data) {
                    updateDelegations();
                });

                scope.acceptRequest = function (delegation) {
                    $http.post('/api/DelegationManager/AcceptDelegationItemTemplate', {DelegationId: delegation.id})
                        .success(function (response) {
                            rguNotify.add('Accepted delegation request from ' + delegation.user.displayName);
                            notificationService.resolveRequestById('delegation', delegation.id);
                            updateDelegations();
                        }).error(function (errors, status) {
                    });
                    notificationService.resolveRequestById('delegation', delegation.id);
                };


                scope.denyRequest = function (delegation) {
                    var message = 'Are you sure you decline delegation request from ' + delegation.user.displayName + "?";
                    swal({
                        title: 'Decline Delegation Request',
                        html: message,
                        type: "warning",
                        showCancelButton: true,
                        confirmButtonColor: "#DD6B55",
                        confirmButtonText: "Yes",
                        cancelButtonText: "No"
                    }).then(function () {
                            $http.post('/api/DelegationManager/DeniedDelegationItemTemplate', {DelegationId: delegation.id})
                                .success(function (response) {
                                    rguNotify.add('Denied delegation request from ' + delegation.user.displayName);
                                    delegation.removed = true;
                                    notificationService.resolveRequestById('delegation', delegation.id);
                                    updateDelegations();
                                }).error(function (errors, status) {
                            });

                        }
                        , function (dismiss) {
                            if (dismiss === 'cancel') {
                            }
                        });
                };

                scope.activateDelegation = function (delegation) {
                    var message = 'Are you sure you want to activate emergency delegation from ' + delegation.user.displayName + "?";
                    swal({
                        title: 'Active Emergency Delegation',
                        html: message,
                        type: "warning",
                        showCancelButton: true,
                        confirmButtonColor: "#DD6B55",
                        confirmButtonText: "Yes",
                        cancelButtonText: "No"
                    }).then(function () {
                        $http.post('/api/DelegationManager/ActivatedDelegation', {DelegationId: delegation.id})
                            .success(function (response) {
                                rguNotify.add('Activated emergency delegation from ' + delegation.user.displayName);
                                $timeout(function () {
                                    delegation.status = delegation.model.Status = 'Activated';
                                    scope.selectDelegation.Status = 'Activated';
                                });
                                updateDelegations();
                                $rootScope.$broadcast('delegation:activated');
                            }).error(function (errors, status) {
                        });

                    });
                };

                scope.$on('delegation:emergency-vault', function (evt, emergencyVault) {
                    scope.ev = emergencyVault;
                });


                scope.removeDelegation = function (delegation) {
                    var message = 'Are you sure you want to remove this delegation?';
                    swal({
                        title: 'Remove Delegation',
                        html: message,
                        type: "warning",
                        showCancelButton: true,
                        confirmButtonColor: "#DD6B55",
                        confirmButtonText: "Yes",
                        cancelButtonText: "No"
                    }).then(function () {
                            $http.post('/api/DelegationManager/RemoveDelegation', {DelegationId: delegation.id})
                                .success(function (response) {
                                    rguNotify.add('Removed delegation with ' + delegation.user.displayName);
                                    delegation.removed = true;
                                    updateDelegations();

                                }).error(function (errors, status) {
                            });

                        }
                        , function (dismiss) {
                            if (dismiss === 'cancel') {
                            }
                        });

                };

                scope.viewDelegationDetails = function (delegation) {
                    var modalInstance = $uibModal.open({
                        animation: true,
                        templateUrl: 'modal-delegate-view.html',
                        controller: 'ViewDelegationController',
                        size: "",
                        backdrop: 'static',

                        resolve: {
                            delegation: delegation.model
                        }
                    });
                    modalInstance.result.then(function () {
                        //$scope.getListDelegatedTo();
                    }, function () {

                    });

                };

                scope.addDelegation = function () {
                    var modalInstance = $uibModal.open({
                        templateUrl: 'modal-delegate-new.html',
                        controller: 'CreateDelegationController',
                        size: "md",
                        backdrop: 'static',
                        resolve: {}
                    });

                    modalInstance.result.then(function () {
                    }, function () {
                    });
                };

                scope.initDelegationVault = function (del) {
                    scope.view.selectedDelegation = scope.$parent.view.selectedDelegation = del;
                    selectVault = del;
                    scope.statusDate = false;
                    scope.basic = null;
                    scope.contact = null;
                    scope.address = null;
                    scope.passport = null;
                    scope.health = null;
                    scope.blood = null;
                    scope.allergies = null;


                    var date = new Date();
                    var effectiveDate = new Date(scope.view.selectedDelegation.EffectiveDate);
                    var expiredDate = date;

                    if (scope.view.selectedDelegation.ExpiredDate !== 'Indefinite')
                        expiredDate = new Date(scope.view.selectedDelegation.ExpiredDate);
                    //
                    if (date >= effectiveDate && date <= expiredDate)
                        scope.statusDate = true;

                    scope.permission = del.GroupVaultsPermission;
                    var vm = new Object();
                    vm.UserId = del.FromAccountId;
                    VaultService.GetVault(vm).then(function (rs) {
                        scope.vaultInit(rs.VaultInformation);
                    }, function (errors) {
                        swal('Error', errors, 'error');
                    });


                    //Show Emergency
                    if (del.Status == 'Activated') {
                        var vm = new Object();
                        vm.UserId = del.FromAccountId;
                        VaultService.GetVault(vm).then(function (rs) {

                            scope.vaultE = rs.VaultInformation;
                            // scope.vaultTree = rs.VaultInformation;
                            scope.vaultInit(rs.VaultInformation);
                            //search
                            for (var i = 0; i < scope.permission.length; i++) {
                                if (scope.permission[i].name == 'Basic Information' && scope.permission[i].read == true) {
                                    scope.basic = scope.vaultE.basicInformation;
                                }
                                //Contact
                                if (scope.permission[i].name == 'Contact Information' && scope.permission[i].read == true) {
                                    scope.contact = {
                                        'mobile': '',
                                        'home': '',
                                        'office': '',
                                        'fax': '',
                                        'email': ''

                                    }

                                    if (scope.vaultE.contact.value.mobile.default != '' || scope.vaultE.contact.value.mobile.default != '_')
                                        scope.contact.mobile = scope.vaultE.contact.value.mobile.default;

                                    if (scope.vaultE.contact.value.home.default != '' || scope.vaultE.contact.value.home.default != '_')
                                        scope.contact.home = scope.vaultE.contact.value.home.default;
                                    if (scope.vaultE.contact.value.office.default != '' || scope.vaultE.contact.value.office.default != '_')
                                        scope.contact.office = scope.vaultE.contact.value.office.default;
                                    if (scope.vaultE.contact.value.fax.default != '' || scope.vaultE.contact.value.fax.default != '_')
                                        scope.contact.fax = scope.vaultE.contact.value.fax.default;
                                    if (scope.vaultE.contact.value.email.default != '' || scope.vaultE.contact.value.email.default != '_')
                                        scope.contact.email = scope.vaultE.contact.value.email.default;

                                }

                                //current address
                                if (scope.permission[i].name == 'Current Address' && scope.permission[i].read == true) {

                                    for (var m = 0; m < scope.vaultE.groupAddress.value.currentAddress.value.length; m++) {

                                        if (scope.vaultE.groupAddress.value.currentAddress.value[m].description == scope.vaultE.groupAddress.value.currentAddress.default) {
                                            scope.address = {
                                                'description': '',
                                                'addressLine': '',
                                                'startDate': '',
                                                'endDate': '',
                                                'country': '',
                                                'city': '',
                                                'instruction': ''
                                            }
                                            scope.address.description = scope.vaultE.groupAddress.value.currentAddress.value[m].description;
                                            scope.address.addressLine = scope.vaultE.groupAddress.value.currentAddress.value[m].addressLine;
                                            scope.address.startDate = scope.vaultE.groupAddress.value.currentAddress.value[m].startDate;
                                            scope.address.endDate = scope.vaultE.groupAddress.value.currentAddress.value[m].endDate;
                                            scope.address.country = scope.vaultE.groupAddress.value.currentAddress.value[m].country;
                                            scope.address.city = scope.vaultE.groupAddress.value.currentAddress.value[m].city;
                                            scope.address.instruction = scope.vaultE.groupAddress.value.currentAddress.value[m].instruction;
                                        }
                                    }
                                }

                                // passport
                                if (scope.permission[i].name == 'Passport' && scope.permission[i].read == true) {

                                    for (var m = 0; m < scope.vaultE.groupGovernmentID.value.passportID.value.length; m++) {

                                        if (scope.vaultE.groupGovernmentID.value.passportID.value[m].description == scope.vaultE.groupGovernmentID.value.passportID.default) {
                                            scope.passport = {
                                                'description': '',
                                                'firstName': '',
                                                'middleName': '',
                                                'lastName': '',
                                                'nationality': '',
                                                'cardNumber': '',
                                                'issuedDate': '',
                                                'expiryDate': '',
                                                'issuedBy': '',
                                                'issuedIn': ''
                                            }
                                            scope.passport.description = scope.vaultE.groupGovernmentID.value.passportID.value[m].description;
                                            scope.passport.firstName = scope.vaultE.groupGovernmentID.value.passportID.value[m].firstName;
                                            scope.passport.middleName = scope.vaultE.groupGovernmentID.value.passportID.value[m].middleName;
                                            scope.passport.lastName = scope.vaultE.groupGovernmentID.value.passportID.value[m].lastName;
                                            scope.passport.nationality = scope.vaultE.groupGovernmentID.value.passportID.value[m].nationality;
                                            scope.passport.cardNumber = scope.vaultE.groupGovernmentID.value.passportID.value[m].cardNumber;
                                            scope.passport.issuedDate = scope.vaultE.groupGovernmentID.value.passportID.value[m].issuedDate;
                                            scope.passport.expiryDate = scope.vaultE.groupGovernmentID.value.passportID.value[m].expiryDate;
                                            scope.passport.issuedBy = scope.vaultE.groupGovernmentID.value.passportID.value[m].issuedBy;
                                            scope.passport.issuedIn = scope.vaultE.groupGovernmentID.value.passportID.value[m].issuedIn;
                                        }
                                    }
                                }
                                // health
                                if (scope.permission[i].name == 'Health Card' && scope.permission[i].read == true) {
                                    for (var m = 0; m < scope.vaultE.groupGovernmentID.value.healthCard.value.length; m++) {
                                        if (scope.vaultE.groupGovernmentID.value.healthCard.value[m].description == scope.vaultE.groupGovernmentID.value.healthCard.default) {
                                            scope.health = {
                                                'description': '',
                                                'firstName': '',
                                                'middleName': '',
                                                'lastName': '',
                                                'cardNumber': '',
                                                'bloodType': '',
                                                'issuedDate': '',
                                                'expiryDate': ''
                                            }
                                            scope.health.description = scope.vaultE.groupGovernmentID.value.healthCard.value[m].description;
                                            scope.health.firstName = scope.vaultE.groupGovernmentID.value.healthCard.value[m].firstName;
                                            scope.health.middleName = scope.vaultE.groupGovernmentID.value.healthCard.value[m].middleName;
                                            scope.health.lastName = scope.vaultE.groupGovernmentID.value.healthCard.value[m].lastName;
                                            scope.health.cardNumber = scope.vaultE.groupGovernmentID.value.healthCard.value[m].cardNumber;
                                            scope.health.bloodType = scope.vaultE.groupGovernmentID.value.healthCard.value[m].bloodType;
                                            scope.health.issuedDate = scope.vaultE.groupGovernmentID.value.healthCard.value[m].issuedDate;
                                            scope.health.expiryDate = scope.vaultE.groupGovernmentID.value.healthCard.value[m].expiryDate;
                                        }
                                    }
                                }

                                // bloodType
                                if (scope.permission[i].name == 'Blood Type' && scope.permission[i].read == true) {

                                    for (var m = 0; m < scope.vaultE.groupGovernmentID.value.healthCard.value.length; m++) {

                                        if (scope.vaultE.groupGovernmentID.value.healthCard.value[m].description == scope.vaultE.groupGovernmentID.value.healthCard.default) {
                                            scope.blood = '';
                                            scope.blood = scope.vaultE.groupGovernmentID.value.healthCard.value[m].bloodType;
                                        }
                                    }
                                }

                                //Allergies
                                if (scope.permission[i].name == 'Allergies' && scope.permission[i].read == true) {
                                    if (scope.vaultE.others.value.preference.value.hasOwnProperty('allergies'))
                                        scope.allergies = scope.vaultE.others.value.preference.value.allergies.value;

                                }
                            }

                        }, function (errors) {
                            swal('Error', errors, 'error');
                        });
                    }

                    //End Emergency
                    function toLabel(name) {
                        var result = name.replace(/([A-Z])/g, " $1");
                        return result.charAt(0).toUpperCase() + result.slice(1);
                    }

                    // Son search
                    scope.vaultInit = function (vault) {
                        scope.vaultTree = vault;
                        var permissions = [];
                        var delegationRole = del.DelegationRole;
                        var delegationGranular = delegationRole === 'Custom' || delegationRole === 'Emergency';
                        if (delegationGranular) {
                            permissions = del.GroupVaultsPermission;
                        }

                        var acl = {};

                        if (delegationRole === 'Emergency') {

                            var emergencyPermissions = [
                                {
                                    name: 'Basic Information',
                                    read: false,
                                    jsonpaths: ['basicInformation.firstName', 'basicInformation.lastName', 'basicInformation.dob']
                                },
                                {
                                    name: 'Contact Information',
                                    read: false,
                                    jsonpaths: ['contact']
                                },
                                {
                                    name: 'Current Address',
                                    read: false,
                                    jsonpaths: ['groupAddress.currentAddress']
                                },
                                {
                                    name: 'Passport',
                                    read: false,
                                    jsonpaths: ['groupGovernmentID.passportID']
                                },
                                {
                                    name: 'Health Card',
                                    read: false,
                                    jsonpaths: ['groupGovernmentID.healthCard']
                                },
                                {
                                    name: 'Blood Type',
                                    read: false,
                                    jsonpaths: ['groupGovernmentID.healthCard.bloodType']
                                },
                                {
                                    name: 'Allergies',
                                    read: false,
                                    jsonpaths: ['others.preferences.allergies']
                                }
                            ];

                        }

                        angular.forEach(permissions, function (permission) {
                            // console.log(permission);
                            if (delegationRole === 'Custom') {
                                var jsPath = permission.jsonpath;
                                switch (jsPath) {
                                    case 'governmentID':
                                        jsPath = 'groupGovernmentID';
                                        break;
                                    case 'address':
                                        jsPath = 'groupAddress';
                                        break;
                                    case 'financial':
                                        jsPath = 'groupFinancial';
                                        break;
                                }

                                var aclItem = {
                                    name: permission.name,
                                    read: permission.read || permission.write,
                                    write: permission.write
                                };
                                var pathNames = permission.jsonpath.split('.');
                                if (pathNames.length === 1) {
                                    aclItem.type = 'bucket';
                                } else {
                                    aclItem.type = 'field';
                                }
                                acl[jsPath] = aclItem;
                            } else {

                                angular.forEach(emergencyPermissions, function (ep) {
                                    if (ep.name === permission.name) {
                                        angular.forEach(ep.jsonpaths, function (jsPath) {
                                            var aclItem = {
                                                name: permission.name,
                                                read: permission.read,
                                                write: false,
                                                jsonpath: '.' + jsPath
                                            };
                                            var pathNames = jsPath.split('.');
                                            if (pathNames.length === 1) {
                                                aclItem.type = 'bucket';
                                                acl[jsPath] = aclItem;
                                            } else {
                                                aclItem.type = 'field';
                                                acl['.' + jsPath] = aclItem;
                                            }

                                        });
                                    }
                                });

                            }
                        });
                        // console.log(acl);

                        var entries = [];
                        var defaultPermission = delegationRole === 'Super' ? {read: true, write: true} : {
                            read: false,
                            write: false
                        };
                        traverseVault(scope.vaultTree, 1, '', '', defaultPermission);
                        scope.vaultEntries = entries;

                        //push value entries
                        function traverseVault(node, level, path, jsPath, permission) {
                            if (!angular.isObject(node)) return;

                            angular.forEach(node, function (entry, name) {
                                    if (name === '_id' || name === 'userId' || name === 'dateCreate') return;

                                    if (!angular.isObject(entry))
                                        return;

                                    var label = entry.label;
                                    var list = angular.isUndefined(label);
                                    var sublist = entry.hasOwnProperty('sublist') && entry.sublist || name === 'contact' && level === 1;
                                    list = list || sublist;


                                    var ownPermission = false;
                                    var perm = angular.copy(permission);

                                    if (delegationRole !== 'Super') {
                                        if (acl.hasOwnProperty(name)) {
                                            perm.read = acl[name].read;
                                            perm.write = acl[name].write;
                                            ownPermission = true;
                                        }
                                        else {
                                            var fullJsPath = jsPath + '.' + name;
                                            if (acl.hasOwnProperty(fullJsPath)) {
                                                perm.read = acl[fullJsPath].read;
                                                perm.write = acl[fullJsPath].write;
                                                ownPermission = true;
                                            }
                                        }
                                    }


                                    if (!list && (!angular.isObject(entry.value) || entry.hasOwnProperty('type')
                                            || entry.hasOwnProperty('label') && entry.hasOwnProperty('value') && level > 2)) {

                                        //     if (!angular.isObject(entry.value)) {
                                        var vaultEntry = {
                                            id: entry._id,
                                            label: label,
                                            description: entry.description,
                                            options: entry.options,
                                            leaf: true,
                                            value: entry.value,
                                            type: entry.type,
                                            rules: entry.rules,
                                            level: level,
                                            path: path,
                                            jsPath: jsPath + '.' + name,
                                            permission: perm
                                        };
                                        entries.push(vaultEntry);
                                    }
                                    else {

                                        if (list) {
                                            label = sublist ? entry.label : entry.description;
                                        }
                                        vaultEntry = {
                                            id: entry._id,
                                            label: label,
                                            leaf: false,
                                            level: level,
                                            path: path,
                                            jsPath: jsPath + '.' + name,
                                            permission: perm
                                        };
                                        // console.log(vaultEntry)
                                        if (list) {
                                            vaultEntry.list = true;
                                        }
                                        if (ownPermission) {
                                            vaultEntry.ownPermission = true;
                                        }
                                        entries.push(vaultEntry);

                                        if (list) {
                                            var fields = [];
                                            angular.forEach(entry, function (list, listName) {

                                                if (sublist) {
                                                    if (!angular.isObject(list) || list.nosearch) {
                                                        return;
                                                    }
                                                    angular.forEach(list, function (sublist, sublistName) {
                                                        if (sublist.nosearch) return;
                                                        angular.forEach(sublist.value, function (item, key) {
                                                            var field = {
                                                                value: item.value,
                                                                leaf: true,
                                                                label: sublist.label + ' ' + item.id,
                                                                level: level + 1,
                                                                path: path + '/' + entry.label + '/' + sublist.label,
                                                                jsPath: jsPath + '.' + name + '.' + sublistName + '.' + item.id,
                                                                permission: perm
                                                            };
                                                            fields.push(field);
                                                            entries.push(field);
                                                        });
                                                    });
                                                } else if (listName !== '_default' && listName !== 'description' && listName !== 'privacy') {
                                                    var label = list.label || toLabel(listName);
                                                    var field = {
                                                        value: list,
                                                        leaf: true,
                                                        label: label,
                                                        level: level + 1,
                                                        path: path + '/' + label,
                                                        jsPath: jsPath + '.' + name + '.' + listName,
                                                        permission: perm
                                                    };
                                                    fields.push(field);
                                                    entries.push(field);
                                                }

                                            });

                                            vaultEntry.children = fields;
                                        }

                                        if (!list) {
                                            traverseVault(entry.value, level + 1, path + '/' + entry.label, jsPath + '.' + name, angular.copy(perm));
                                        }
                                    }
                                    //end else
                                }
                            );

                        }


// console.log(scope.vaultEntries);
                    }
                    ;


                    scope.vaultSearch = {
                        query: '',
                        searching: false
                    };
//
                    scope.vaultEdit = {
                        editingField: false
                    };
                    scope.onVaultSearchInput = function () {
                        scope.vaultSearch.searching = !!scope.vaultSearch.query.length;
                    };
                    scope.clearSearch = function () {
                        scope.vaultSearch.query = '';
                        scope.vaultSearch.searching = false;
                    };

                    function deepTest(value, re) {
                        if (!angular.isDefined(value))
                            return false;
                        if (angular.isString(value))
                            return re.test(value);
                        if (angular.isArray(value)) {
                            var matched = false;
                            angular.forEach(value, function (item) {
                                matched = matched || deepTest(item, re);
                            });
                            return matched;
                        }
                        return re.test(value.toString());
                    }

                    scope.filterEntriesByQuery = function (entry) {
                        if (!scope.vaultSearch.query.length) return false;
                        if (!entry.permission.read) return false;
                        var re = new RegExp(scope.vaultSearch.query, 'i');
                        var matched = re.test(entry.label);
                        if (entry.leaf && entry.value) {
                            matched = matched || deepTest(entry.value, re);
                        }
                        if (entry.leaf || entry.level === 1 || !angular.isObject(entry.children))
                            return matched;
                        angular.forEach(entry.children, function (entry) {
                            matched = matched || re.test(entry.label) || entry.leaf && deepTest(entry.value, re);
                        });
                        return matched;
                    };

                    scope.editField = function (field) {
                        scope.vaultEdit.editingField = field;
                        scope.vaultEdit.editModel = field.value;
                    };

// scope.view.selectedDelegation
                    scope.cancelField = function (field) {
                        scope.vaultEdit.editingField = false;
                        scope.query = null;
                    };

                    scope.saveField = function (field) {
                        scope.vaultEdit.editingField = false;

                        var data = new Object();

                        data.UserId = del.FromAccountId;
                        data.InfoField = {
                            Id: field.id,
                            jsPath: field.jsPath,
                            Type: field.type,
                            Value: scope.vaultEdit.editModel
                        }

                        $http.post('/api/InformationVaultService/UpdateInfoFieldById', data)
                            .success(function (response) {
                                scope.vaultEdit.editingField = false;
                                field.value = scope.vaultEdit.editModel;
                                $rootScope.$broadcast('basicInformation');
                            })
                            .error(function (errors, status) {

                            });

                    }


                    scope.editGroup = function (group) {
                        if (scope.editingMode) return;
                        scope.editingMode = true;
                        if (angular.isDefined(group)) {
                            scope.editingModel = {};
                            angular.forEach(group.fields, function (field) {
                                if (field.divider) return;
                                scope.editingModel[field.name] = angular.extend({}, field);
                            });
                        }
                    };
                    scope.editContact = function () {
                        if (scope.editingModeContact) return;
                        scope.editingModeContact = true;
                    };

                    scope.renderFieldValue = function (field) {
                        var value = field.value, type = field.type;
                        if (!type) return value;

                        if (type === 'date') {
                            return $moment(value).format('DD MMM YYYY');
                        }
                        if (type === 'list') {
                            return value.join(', ');
                        }
                        return value;
                    };

                    scope.cancelEditing = function () {
                        scope.editingMode = false;
                        scope.editingModeContact = false;
                    };

                    scope.saveGroup = function (group) {
                        scope.editingMode = false;
                        if (!angular.isDefined(group)) return;
                        angular.forEach(group.fields, function (field) {
                            if (field.divider) return;
                            var editingField = scope.editingModel[field.name];
                            if (editingField.tags) {
                                editingField.value = $.map(editingField.tags, function (tag) {
                                    return tag.text;
                                });
                            }
                            angular.extend(field, editingField);
                        });

                        vaultService.saveVaultGroup(group, function () {
                            //  Check result
                        });
                    };

                }


            }
        }
            ;
    })

    .directive('rguPersonPicker', function ($sce) {

        return {
            restrict: 'E',
            scope: {
                people: '=',
                model: '=ngModel',
                onChange: '&?'
            },
            templateUrl: '/Areas/regitUI/templates/person-picker.html',
            link: function link(scope, elem, attrs) {
                scope.matchedPerson = null;
                scope.pickedPerson = scope.model;
                scope.pickMatchedPerson = function ($item, $model, $label, $event) {
                    scope.model = angular.copy(scope.matchedPerson);
                    scope.matchedPerson = null;
                    if (angular.isFunction(scope.onChange)) {
                        scope.onChange();
                    }
                };
                scope.filterMatches = function (person) {
                    return !scope.model || !scope.model.Id || person.Id !== scope.model.Id;
                };
                elem.focus();

            }
        };
    })
    .directive('userSelect', function ($http, rguView, rgu, rguNotify) {

        return {
            restrict: 'E',
            scope: {
                type: '@',
                model: '=ngModel',
                ignores: '<?',
                invites: '<?',
                onAdd: '&',
                onChange: '&?',
                onSelect: '&?',
                selectedUser: '=?'
            },
            templateUrl: '/Areas/regitUI/templates/user-select.html?v=11',
            link: function link(scope, elem, attrs) {
                var cat = attrs.type;
                var isWorkflow = cat === 'workflow';
                var isCustomers = cat === 'customer';
                var isBusiness = isWorkflow || isCustomers;
                var isHandshake = scope.isHandshake = cat === 'handshake';

                scope.rguView = rguView;
                scope.search = {
                    query: '',
                    by: 'auto',
                    byEmail: false,
                    placeholder: '',
                    users: [],
                    ignored: null,
                    invited: false,
                    loaded: false,
                    alert: false
                };
                scope.view = {
                    openingUserPopover: false
                };
                scope.users = [];
                scope.newUser = null;
                scope.validateEmail = rgu.validateEmail;
                scope.clearSearch = function () {
                    scope.search.query = '';
                    scope.search.me = scope.search.ignored = null;
                    scope.search.invited = false;
                    scope.search.alert = false;
                    scope.users.splice(0);
                    elem.find('.user-search-input').focus();
                };
                scope.searchBy = function (by, noClear) {
                    scope.search.by = by;
                    if (!noClear)
                        scope.clearSearch();
                    scope.search.byEmail = by === 'email';
                    scope.search.placeholder = by === 'auto' ? 'Type a name or email' : (by === 'name') ? 'Type a name' : 'Type an email';
                };
                scope.searchBy('auto');
                scope.onUserSearchInput = function () {
                    scope.search.me = null;
                    scope.search.invited = false;
                    scope.search.alert = false;
                    scope.users.splice(0);
                    if (!scope.search.query.length) {
                        scope.search.ignored = null;
                        if (scope.users.length) scope.users.splice(0);
                        scope.search.placeholder = scope.search.by === 'auto' ? 'Type a name or email' : (!scope.search.byEmail ? 'Type a name' : 'Type an email');
                        return;
                    }
                    scope.search.loaded = false;
                    var searchBy = scope.search.by;
                    if (searchBy === 'auto') {
                        scope.search.byEmail = /.+@/.test(scope.search.query);
                        scope.search.placeholder = !scope.search.byEmail ? 'Type a name' : 'Type an email';
                        searchBy = scope.search.byEmail ? 'email' : 'name';
                    }
                    if (searchBy === 'email') {
                        if (!rgu.validateEmail(scope.search.query)) return;
                        if (scope.invites && scope.invites.some(function (invite) {
                                return invite.email === scope.search.query;
                            })) {
                            scope.search.invited = true;
                            return;
                        }
                    }
                    $http.get('/Api/Users/Search', {
                        params: {
                            keyword: scope.search.query,
                            by: searchBy
                        }
                    })
                        .success(function (users) {
                            // console.log(users)
                            if (searchBy === 'email' && scope.validateEmail(scope.search.query)) {
                                scope.selectUser({
                                    email: scope.search.query,
                                    displayName: '',
                                    type: 'public'
                                })
                            }

                            scope.users.splice(0);
                            scope.search.me = scope.search.ignored = null;
                            if (scope.search.users.length) scope.search.users.splice(0);
                            users.forEach(function (user) {
                                var newUser = {
                                    id: user.Id,
                                    accountId: user.AccountId,
                                    displayName: user.DisplayName,
                                    avatar: user.Avatar,
                                    email: user.Email,
                                    profileUrl: '/User/Profile?id=' + user.Id

                                };
                                if (user.Id === regitGlobal.userAccount.id) {
                                    newUser.isMe = true;
                                    scope.search.me = user;
                                }
                                if (angular.isArray(scope.ignores)) {
                                    var ignoredUser = scope.ignores.find(function (ignore) {
                                        return ignore.id === user.Id
                                    });
                                    if (ignoredUser) {
                                        newUser.ignored = ignoredUser;
                                        if (scope.search.by === 'email') {
                                            scope.search.ignored = ignoredUser;
                                        }
                                    }
                                }
                                scope.users.push(newUser);

                            });
                            scope.search.loaded = true;

                        }).error(function (errors) {
                    });
                };


                scope.filterUsers = function (user) {
                    if (user.isMe) return false;
                    if (user.ignored) return false;
                    // if (angular.isArray(scope.ignores)) {
                    //     if (scope.ignores.some(function (ignore) {
                    //             return ignore.id === user.id
                    //         })) {
                    //         return false;
                    //     }
                    // }
                    return true;
                };

                scope.openUserPopover = function (user, evt) {
                    scope.view.openingUserPopover = user.id;
                    rguView.openPopover(elem.find('.rgu-popover'), user.id, evt);
                };
                scope.closeUserPopover = function (user) {
                    scope.view.openingUserPopover = false;
                    rguView.closePopover();
                    elem.find('.rgu-popover').appendTo(elem);
                };

                scope.addUser = function (user, evt) {
                    scope.newUser = angular.copy(user);

                    if (isWorkflow) {
                        scope.newUser.roleModel = {
                            admin: false,
                            editor: true,
                            approver: false
                        };
                    }
                    scope.openUserPopover(user, evt);

                };
                scope.selectUser = function (user) {
                    scope.newUser = angular.copy(user);
                    scope.selectedUser = scope.newUser;
                    if (user.type !== 'public') {
                        scope.searchBy('auto');
                    } else {
                        if (angular.isFunction(scope.onSelect)) {
                            scope.onSelect({user:user});
                        }
                    }
                };
                scope.inviteEmail = function (email, evt) {
                    scope.newUser = {
                        id: 'emailUser',
                        invite: {
                            email: email,
                            message: ''
                        }
                    };

                    if (isWorkflow) {
                        scope.newUser.roleModel = {
                            admin: false,
                            editor: true,
                            approver: false
                        };

                    }
                    scope.openUserPopover(scope.newUser, evt);

                };

                scope.canAddUser = function () {
                    if (!scope.newUser) return false;
                    if (!isWorkflow) {
                        return true;
                    }
                    return scope.newUser.roleModel.admin || scope.newUser.roleModel.editor || scope.newUser.roleModel.approver;
                };
                scope.cancelAddUser = function () {
                    scope.closeUserPopover();
                    scope.newUser = null;
                };

                scope.saveAddUser = function () {

                    var user = scope.newUser;

                    if (isWorkflow) {
                        var roleNames = [];
                        $.each(user.roleModel, function (roleName, isRole) {
                            if (isRole) {
                                roleNames.push(roleName);
                            }
                        });
                        user.roleNames = roleNames;
                    }

                    if (user.invite) {
                        var options = null;
                        var account = isBusiness ? regitGlobal.businessAccount : regitGlobal.userAccount;
                        if (isWorkflow) {
                            options = user.roleNames.join(', ');
                        } else if (isCustomers) {
                            options = null;
                        } else {
                            options = null;
                        }
                        var invite = {
                            category: attrs.type,
                            toEmail: user.invite.email,
                            fromAccountId: account.id,
                            fromDisplayName: account.displayName,
                            inviteId: "",
                            message: user.invite.message,
                            options: options
                        };

/*                        $http.post('/Api/Invite/NewInvite', invite)
                            .success(function (response) {
                                rguNotify.add('Sent ' + cat + ' email invitation to ' + user.invite.email);
                                // updateWorkflow();
                                scope.onAdd({user: user});
                                scope.closeUserPopover();
                                scope.clearSearch();
                                scope.newUser = null;
                            }).error(function (errors, status) {
                            console.log('Error sending ' + cat + ' invitation email', errors)
                        });*/
                    }
                    else {
                        scope.onAdd({user: user});
                        scope.closeUserPopover();
                        scope.clearSearch();
                        scope.newUser = null;
                    }


                }

            }
        };
    });
