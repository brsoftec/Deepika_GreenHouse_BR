myApp.getController('CloseSuspendSettingsController', ['$scope', '$rootScope', 'SweetAlert', 'UserManagementService', 'SmSAuthencationService', '$timeout', function ($scope, $rootScope, SweetAlert, _userManagementService, SmSAuthencationService, $timeout) {

    // Get Profile GetDisabledUserByEmail
    $scope.profileUser = {};
    $scope.currentDisableUser = {};
   
    $scope.getProfile = function () {
    
        _userManagementService.IsDisableUser().then(function (rs) {
            $scope.profileUser = rs;
            _userManagementService.GetDisabledUserByEmail(rs.Email).then(function (respone) {
                $scope.currentDisableUser = respone;
            })
        })
    }
    $scope.getProfile();
   
   
    // Regit Close Spend
    $scope.listEnableUsers = [];
    $scope.currentPage = 0;
    $scope.disableUserResponse = null;
    $scope.format = "dd MMM yyyy";
    $scope.user = null;
    $scope.listDisableUsers = [];
    $scope.disableUserParameter = {
        Start: 0,
        Length: 4
    };

    //
    var mappingEnabled = function (disableUsers, enableUsers) {
        for (var i = 0; i < enableUsers.length; i++) {
            for (var j = 0; j < disableUsers.length; j++) {
                if (enableUsers[i].Id === disableUsers[j].Id) {
                    disableUsers[j].IsEnabled = true;
                }
            }
        }
        return disableUsers;
    }

    var getDisableUsers = function () {
        _userManagementService.GetDisableUsers($scope.disableUserParameter)
            .then(function (data) {
                $scope.disableUserResponse = data;
                $scope.disableUserResponse.DisableUsers = mappingEnabled(data.DisableUsers, $scope.listEnableUsers);
            },
                function (errors) {
                    swal(_sweetAlert, 'errors', errors.error_description, 'error');
                });
    }

    $scope.pageChanged = function () {
        $scope.disableUserParameter.Start = $scope.currentPage - 1;
        getDisableUsers();
    }

    $scope.openEffectiveDate = function () {
        $scope.popupEffectiveDate.opened = true;
    };

    $scope.popupEffectiveDate = {
        opened: false
    };

    $scope.openUntil = function () {
        $scope.popupUntil.opened = true;
    };

    $scope.popupUntil = {
        opened: false
    };

    $scope.disableUser = {
        UserId: "",
        EffectiveDate: "",
        Until: "",
        Reason: null
    }

    $scope.disabled = function () {
        //
        $scope.disableUser = {
            UserId: "",
            EffectiveDate: "",
            Until: "",
            Reason: "Close user"
        }

        var message = "<p>Are you sure you want to close your accout associated with this email: " + $scope.profileUser.Email + "?</p>"
     + "<p>Be careful. Your account will be permanently deleted after 30 days. You won't be able to restore it later.</p>"

        swal({

            title: 'Close Regit Account',
            html: message,
            type: 'warning',
            showCancelButton: true,
            showCloseButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Yes",
            cancelButtonText: "Cancel"
        }).then(function () {
           SmSAuthencationService.OpenPopupAuthencationSMS(function () {
                $timeout(function () {
               
                _userManagementService.DisableUser($scope.disableUser)
                    .then(function (response) {
                     
                        $scope.disableUser = {
                            UserId: "",
                            EffectiveDate: "",
                            Until: "",
                            Reason: null
                        }
                        swal('Disable user success', 'OK', 'success');
                         $scope.getProfile();
                             

                    },
                        function (errors) {
                            if (errors.Status === 400) {
                                swal('Disable user success', 'Error', 'error');
                            } else {
                                swal('Disable user success', 'Error', 'error');
                            }
                        });
        });
        });

    });

    }

    $scope.findUsers = function (keyword) {

        return _userManagementService.FindUsersByKeyword(keyword)
            .then(function (data) {
                return data.Users;
            },
                function (errors) {
                    __common.swal(_sweetAlert, 'errors', errors.error_description, 'error');
                });
    }


    $scope.unclose = function (currentDisableUser) {
        //
      
        var message = "You have restored your closed account."
        swal({

            title: 'Restore Close Regit Account',
            html: message,
            type: 'info',
            showCancelButton: true,
            showCloseButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Yes",
            cancelButtonText: "Cancel"
        }).then(function () {
            SmSAuthencationService.OpenPopupAuthencationSMS(function () {
                $timeout(function () {
                   
                    _userManagementService.EnableUser(currentDisableUser.UserId)
                        .then(function (response) {
                             swal('Restore Close Regit Account', 'OK', 'success');
                              $scope.getProfile();
                        },
                            function (errors) {
                                if (errors.Status === 400) {
                                    swal('Restore Close Regit Account', 'Error', 'error');
                                } else {
                                    swal('Restore Close Regit Account', 'Error', 'error');
                                }
                            });
                });
            });

        });

    }

    //
    $scope.enable = function (users) {
        var enableUsers = {
            DisableUsers: $scope.listEnableUsers
        }
        _userManagementService.EnableUsers(enableUsers)
                .then(function (response) {
                    $scope.listEnableUsers = [];
                    getDisableUsers();
                    __common.swal(_sweetAlert, 'OK', 'Enable user success', 'success');
                },
                    function (errors) {
                        __common.swal(_sweetAlert, 'errors', errors.error_description, 'error');
                    });
    }

    $scope.checkBoxChanged = function (user) {
        if (user.IsEnabled) {
            $scope.listEnableUsers.push(user);
        } else {
            $scope.listEnableUsers.pop(user);
        }
    }

    getDisableUsers();
    // End Regit Close Spend
    // End Admin
    
    
}])