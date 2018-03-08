angular.module('workflow', ['regit.ui', 'notifications'])
    .directive('workflowGrid', function ($rootScope, $timeout, $http, $q, notificationService, rguCache, rguView, rguNotify, BusinessAccountService) {
        return {
            restrict: 'EA',
            scope: {
                type: '@'
            },
            templateUrl: '/Areas/regitUI/templates/workflow-grid.html?v=5',
            link: function (scope, elem, attrs) {

                scope.canWrite = regitGlobal.workflowAccount.canWrite;

                scope.users = [];
                scope.invites = [];

                scope.view = {
                    loaded: false,
                    editing: false,
                    editingUser: null,
                    showingAddMember: false,
                    showingQuotaReached: false
                };
                scope.subscription = {
                    plan: null,
                    reachedQuota: false
                };
                scope.rguView = rguView;
                scope.sortUsers = function (user) {
                    var rank = user.pending ? 1 : 2;
                    return rank;
                };

                scope.beginEdit = function (user, evt) {
                    scope.view.editing = true;
                    scope.view.editingUser = user;
                    scope.view.editingUser.roleModel = {
                        admin: user.isAdmin,
                        editor: user.isEditor,
                        approver: user.isApprover
                    };
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
                scope.resendRequest = function (user) {
                    NetworkService.InviteFriend({ReceiverId: user.id}).then(function () {

                        rguNotify.add('Network invitation resent to ' + user.displayName);

                    }, function (errors) {

                    });
                };

                scope.doSaveEdit = function (user) {
                    var roleNames = [];
                    angular.forEach(scope.view.editingUser.roleModel, function(isRole, roleName) {
                        if (isRole) {
                            roleNames.push(roleName);
                        }
                    });
                    // user.roleNames = roleNames;
                    $http.post('/Api/Workflow/UpdateMember', {
                        businessId: regitGlobal.businessAccount.id,
                        memberId: user.id,
                        roles: roleNames
                    })
                        .success(function (response) {
                            rguNotify.add('Updated workflow roles for member ' + user.displayName);
                            updateWorkflow();
                        }).error(function (errors, status) {
                        console.log('Error updating workflow member', errors)
                    });
                };

                scope.viewPermissions = function() {
                    location.href = '/Workflow/Permissions';
                };

                scope.subscription.reachedQuota = false;
                if (regitGlobal.hasOwnProperty('subscriptionPlan')) {
                    scope.subscription.plan = regitGlobal.subscriptionPlan;
                }
                scope.checkQuota = function() {
                    if (regitGlobal.subscriptionPlan.name==='enterprise') {
                        scope.subscription.reachedQuota = scope.view.showingQuotaReached = false;
                        return;
                    }
                    var remaining = regitGlobal.subscriptionPlan.businessMembers - scope.users.length;
                    if (remaining <= 0) {
                        scope.subscription.reachedQuota = true;
                    } else {
                        scope.subscription.reachedQuota = scope.view.showingQuotaReached = false;
                    }
                };

                function updateInvites() {
                    $rootScope.$broadcast('workflow:invites-update');
                }

                function updateWorkflow() {

                    scope.cancelEdit();

                    BusinessAccountService.GetAllRoles().then(function (roles) {

                        //console.log(roles);


                        BusinessAccountService.GetMembersInBusiness().then(function (result) {

                            // pullInviations()
                            scope.users.splice(0);
                            result.Members.forEach(function (member) {
                                var roleNames = [];
                                var userRoles = [];
                                var isAdmin = false, isEditor = false, isApprover = false;
                                member.RoleIds.forEach(function (roleId) {
                                    var role = roles.find(function (role) {
                                        return role.Id === roleId;
                                    });
                                    if (role !== undefined) {
                                        var roleName = role.Name.toLowerCase();
                                        if (roleName === 'administrator') {
                                            roleName = 'admin';
                                            isAdmin = true;
                                        } else if (roleName === 'editor') {
                                            isEditor = true;
                                        } else if (roleName === 'approver' || roleName === 'reviewer') {
                                            roleName = 'approver';
                                            isApprover = true;
                                        }
                                        roleNames.push(roleName);
                                        userRoles.push({
                                            id: role.Id,
                                            name: roleName
                                        });
                                    }
                                });

                                var user = {
                                    id: member.AccountId,
                                    accountId: member.AccountId,
                                    displayName: member.DisplayName,
                                    avatar: member.Avatar,
                                    profileUrl: '/User/Profile?id=' + member.AccountId,
                                    roles: userRoles,
                                    roleNames: roleNames,
                                    isAdmin: isAdmin,
                                    isEditor: isEditor,
                                    isApprover: isApprover
                                };

                                scope.users.push(user);
                            });
                            scope.view.loaded = true;

                            $http.get('/Api/Workflow/Invitations', {
                                params: { businessId: regitGlobal.businessAccount.id }
                            })
                                .success(function (response) {
                                    angular.forEach(response, function(invitation) {
                                        var pendingUser = {
                                            id: invitation.To,
                                            roles: invitation.Roles.map(function(roleName) {
                                                return { id: '',
                                                name: roleName }
                                            }),
                                            pending: true,
                                            invitation: invitation
                                        };
                                        if (invitation.hasOwnProperty('InviteId')) {
                                            pendingUser.inviteId = invitation.InviteId;
                                        }
                                        rguCache.getUserAsyncById(invitation.To).then(function (user) {
                                            $timeout(function () {
                                                pendingUser.displayName = user.displayName;
                                                pendingUser.avatar = user.avatar;
                                                pendingUser.accountId = user.accountId;
                                            });
                                        });
                                        // console.log(pendingUser)
                                        scope.users.unshift(pendingUser);

                                    });
                                    scope.checkQuota();
                                }).error(function (errors, status) {
                                console.log(errors)
                            });

                        })
                    });

                }

                updateWorkflow();
                updateInvites();

                scope.$on('workflow:notification', function (event, data) {
                    updateWorkflow();
                });
                scope.$on('workflow:resolved', function (event, data) {
                    updateWorkflow();
                });
                scope.$on('workflow:update', function (event, data) {
                    updateWorkflow();
                });

                scope.openAddMember = function () {
                    scope.view.showingAddMember = true;
                };
                scope.closeAddMember = function () {
                    scope.view.showingAddMember = false;
                };
                scope.toggleAddMember = function () {
                    if (scope.subscription.reachedQuota) {
                        scope.view.showingQuotaReached = true;
                        scope.view.showingAddMember = false;
                        return;
                    }
                    scope.view.showingAddMember = !scope.view.showingAddMember;
                };

                scope.inviteMember = function (user) {
                    if (user.invite) {
                        updateInvites();
                        return;
                    }
                    $http.post('/Api/Workflow/InviteMember', {
                        businessId: regitGlobal.businessAccount.id,
                        memberId: user.id,
                        roles: user.roleNames
                    })
                        .success(function (response) {
                            rguNotify.add('Invited ' + user.displayName + ' to business workflow');
                            scope.closeAddMember();
                            updateWorkflow();
                        }).error(function (errors, status) {
                        console.log('Error inviting workflow member', errors)
                    });
                };

                scope.doRemoveUser = function (user) {
                    var message = '<p>' + $rootScope.translate('Are_You_Sure_To_Remove') + ' ' + user.displayName + " from your business workflow?</p>";
                    swal({
                        title: 'Remove Business Member',
                        html: message,
                        type: "warning",
                        showCancelButton: true,
                        confirmButtonColor: "#DD6B55",
                        confirmButtonText: "Yes",
                        cancelButtonText: "No"
                    }).then(function () {
                        $http.post('/Api/Workflow/RemoveMember', {
                            businessId: regitGlobal.businessAccount.id,
                            memberId: user.accountId
                        })
                            .success(function (response) {
                                rguNotify.add('Removed ' + user.displayName + ' from business workflow');
                                updateWorkflow();
                            }).error(function (errors, status) {
                            console.log('Error removing workflow member', errors)
                        });
                    });
                };
                scope.cancelRequest = function (user) {
                    $http.post('/Api/Workflow/RemoveInvitation/' + user.invitation.Id)
                        .success(function (response) {
                            rguNotify.add('Cancelled workflow invitation to ' + user.displayName);
                            updateWorkflow();
                        }).error(function (errors, status) {
                        console.log('Error removing workflow invitation', errors)
                    });
                };


            }
        };
    });


