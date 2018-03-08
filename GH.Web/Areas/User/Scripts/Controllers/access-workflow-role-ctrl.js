myApp.getController('AccessWorkflowRoleController', ['$scope', '$rootScope', '$http', 'BusinessAccountService', 'SweetAlert', '$interval','billingService', 'rguModal',
function ($scope, $rootScope, $http, _baService, _sweetAlert, $interval, billingService, rguModal) {

    $scope.findMembers = function (keyword) {
        return _baService.SearchMembersForInvitation(keyword, true).then(function (users) {
            return users;
        });
    }

    $scope.invitation = null;
    $scope.showInviteControl = false;
    $scope.invite = function (invitation) {
        billingService.CheckBillingCurrent(function () {
            $scope.invitation = invitation;
            if ($scope.invitation && $scope.invitation.Id) {
                _baService.InviteMemberToBusiness({ ReceiverId: $scope.invitation.Id }).then(function (invitation) {
                    $scope.GetMembersInBusiness();
                    __common.swal(_sweetAlert, 'OK', $scope.invitation.DisplayName + ' ' + $rootScope.translate('has_been_added'), 'success');
                    $scope.invitation = null;
                }, function (errors) {
                    __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
                })
            } else {
             
               __common.swal(_sweetAlert, 'warning', $rootScope.translate('Please_choose_your_member'), 'warning');
            }
           

        }, function (billingInfo) {
            $scope.plan = billingInfo.plan;
            $scope.current = billingInfo.workflowMembers;
            $scope.maxnumber = billingInfo.maxWorkflowMembers;
            rguModal.openModal('billing.quota.limit', $scope, { billingInfo: billingInfo });

        }, "workflow");
        $scope.billing.reachedWorkflowMembers = false;
    }
    

    $scope.members = [];
    $scope.originalMembers = [];

    $scope.adminRole = {};
    $scope.editorRole = {};
    $scope.reviewerRole = {};
    $scope.GetMembersInBusiness = function () {
        _baService.GetAllRoles().then(function (roles) {
            for (var i = 0; i < roles.length; i++) {
                var role = roles[i];
                if (role.Name == 'Administrator') {
                    $scope.adminRole = role;
                } else if (role.Name == 'Reviewer') {
                    $scope.reviewerRole = role;
                } else {
                    $scope.editorRole = role;
                }
            }

            _baService.GetMembersInBusiness().then(function (result) {
                for (var i = 0; i < result.Members.length; i++) {
                    var member = result.Members[i];
                    if (member.RoleIds.indexOf($scope.adminRole.Id) >= 0) {
                        member.IsAdmin = true;
                    } else {
                        member.IsAdmin = false;
                    }
                    if (member.RoleIds.indexOf($scope.editorRole.Id) >= 0) {
                        member.IsEditor = true;
                    } else {
                        member.IsEditor = false;
                    }
                    if (member.RoleIds.indexOf($scope.reviewerRole.Id) >= 0) {
                        member.IsReviewer = true;
                    } else {
                        member.IsReviewer = false;
                    }
                }
                $scope.members = result.Members;
                $scope.originalMembers = angular.copy($scope.members);
            })
        })
    }

    $scope.GetMembersInBusiness();
    $scope.tempDeleteMembers = [];
    $scope.removeMember = function (member) {
        var idx = $scope.tempDeleteMembers.indexOf(member);
        if (idx < 0) {
            $scope.tempDeleteMembers.push(member);
        } else {
            $scope.tempDeleteMembers.splice(idx, 1);
        }
    }

    $scope.cancelModifyMembers = function () {
        $scope.tempDeleteMembers = [];
        $scope.members = angular.copy($scope.originalMembers);
    }

    $scope.saveModifyMembers = function () {
        var modified = [];
        for (var i = 0; i < $scope.originalMembers.length; i++) {
            var original = $scope.originalMembers[i];
            var mayModify = $scope.members[i];
            var isDeleted = $scope.tempDeleteMembers.indexOf(mayModify) >= 0;
            if (isDeleted) {
                modified.push({ Member: original, Action: 'DELETE' });
            } else if (!angular.equals(original, mayModify)) {
                modified.push({ Member: mayModify, Action: 'UPDATE' });
            }
        }
        if (modified.length > 0) {
            _baService.UpdateMembersInBusiness(modified).then(function (result) {
                if (result.Success) {
                    __common.swal(_sweetAlert,$rootScope.translate('Successful'), '', 'success');
                    for (var i = 0; i < modified.length; i++) {
                        if (modified[i].Action == 'DELETE') {
                            var idx = $scope.originalMembers.indexOf(modified[i].Member);
                            $scope.originalMembers.splice(idx, 1);
                            $scope.members.splice(idx, 1);
                        } else {
                            var idx = $scope.members.indexOf(modified[i].Member);
                            $scope.originalMembers[idx] = angular.copy($scope.members[idx]);
                        }
                    }
                } else {
                    var errors = [];
                    for (var j = 0; j < modified.length; j++) {
                        var member = modified[j].Member;
                        var success = true;
                        for (var i = 0; i < result.Errors.length; i++) {
                            var error = result.Errors[i];
                            if (member.AccountId == error.Id) {
                                var reason = error.Handled ? error.Message : ' internal server error';
                                errors.push('Cannot ' + error.Action + ' member ' + member.DisplayName + ' because ' + reason)
                                success = false;
                                break;
                            }
                        }
                        if (success) {
                            for (var i = 0; i < $scope.originalMembers.length; i++) {
                                var original = $scope.originalMembers[i];
                                if (original.AccountId == member.AccountId) {
                                    if (modified[j].Action == 'UPDATE') {
                                        var modify = $scope.members[i];
                                        $scope.originalMembers[i] = angular.copy(modify);
                                    } else {
                                        $scope.tempDeleteMembers.splice($scope.tempDeleteMembers.indexOf($scope.members[i]), 1);
                                        $scope.members.splice(i, 1);
                                        $scope.originalMembers.splice(i, 1);
                                    }
                                }
                            }
                        }
                    }
                    _sweetAlert.swal({
                        title: $rootScope.translate('Update_is_not_completely_success'),
                        text: '<ul class="text-left"><li>' + errors.join('</li><li>') + '</li></ul>',
                        html: true,
                        type: 'warning'
                    })
                }
            }, function (errors) {
                __errorHandler.Swal(__errorHandler.ProcessErrors(errors));
            })
        }
    }

}])
