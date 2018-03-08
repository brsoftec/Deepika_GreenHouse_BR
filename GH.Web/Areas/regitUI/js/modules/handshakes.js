angular.module('handshakes', ['regit.ui', 'notifications'])
    .directive('handshakeGrid', function ($rootScope, $timeout, $http, $q, notificationService, rguCache, rguView, rguNotify, BusinessAccountService) {
        return {
            restrict: 'EA',
            scope: {
                type: '@'
            },
            templateUrl: '/Areas/regitUI/templates/handshake-grid.html?v=1',
            link: function (scope, elem, attrs) {

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

                function updateInvites() {
                    $rootScope.$broadcast('workflow:invites-update');
                }

                updateInvites();


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


