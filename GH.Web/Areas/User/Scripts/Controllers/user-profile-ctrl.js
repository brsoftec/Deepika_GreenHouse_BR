var myApp = getApp("myApp", ['ngRoute', 'TranslationModule', 'UserModule', 'SocialModule', 'CommonDirectives', 'oitozero.ngSweetAlert', 'ui.select', 'ui.bootstrap', 'NotificationModule', 'CommonService'], true);

myApp.getController('UserProfileController', ['$scope', '$rootScope', '$sce', 'UserManagementService', 'AuthorizationService', 'SweetAlert', 'CommonService', 'NetworkService', function ($scope, $rootScope, $sce, _UserService, _authService, _sweetAlert, CommonService, _networkService) {
     $scope.privacy = {
        'CountryCity': 'public',
        'DOB': 'public',
        'PhotoUrl': 'public',
        'Phone': 'public',
        'Email': 'public',
        'Profile': 'public'
        
    };
    $scope.ProfileViewed = {};
    $scope.userid= CommonService.GetQuerystring("id");
    $scope.IsShowProfile = function () {
        if ($scope.ProfileViewed.IsShowProfile) {
            return true;
        }

        if ($scope.currentUserProfile && $scope.UserId == $scope.currentUserProfile.Id) {
            return true;
        }
        return false;
    };

    $scope.YourProfile = false;
    $scope.initProfile = function () {
        _UserService.GetCurrentUserProfile().then(function (res) {
            if ($scope.userid == res.Id)
                $scope.YourProfile = true;
            if ($scope.userid == null)
                $scope.userid = res.Id;
            _UserService.GetUserProfile($scope.userid).then(function (response) {
                $scope.ProfileViewed = response;
                var accountId = response.AccountId;
                _authService.GetPrivacy(accountId).then(function (rs) {
                    if (rs != null) {
                        for (var i = 0; i < rs.ListField.length; i++) {
                            if (rs.ListField[i].Field == 'CountryCity')
                                $scope.privacy.CountryCity = rs.ListField[i].Role;
                            else if (rs.ListField[i].Field == 'DOB')
                                $scope.privacy.DOB = rs.ListField[i].Role;
                            else if (rs.ListField[i].Field == 'PhotoUrl')
                                $scope.privacy.PhotoUrl = rs.ListField[i].Role;
                            else if (rs.ListField[i].Field == 'Phone')
                                $scope.privacy.Phone = rs.ListField[i].Role;
                            else if (rs.ListField[i].Field == 'Email')
                                $scope.privacy.Email = rs.ListField[i].Role;
                            else if (rs.ListField[i].Field == 'Profile')
                                $scope.privacy.Profile = rs.ListField[i].Role;
                        }
                        if ($scope.privacy.Profile != 'public' || $scope.privacy.PhotoUrl != 'public')
                        {                       
                            $scope.ProfileViewed.PhotoUrl = '';
                        }
                    }

                },
                       function (errors) { });
            }, function (errors) {

            });
        });
    }
    $scope.initProfile();

    $scope.StatusFriend = '';
   
    
    $scope.Friend = {};
    $scope.getFriends = function () {
        _networkService.GetNetworks().then(function (networks) {
            $scope.networks = networks;
            $scope.networks.forEach(function (network) {
                if (network.Name == 'Trusted Network') {
                    _networkService.GetFriendsInNetwork(network.Id).then(function (rs) {
                        for (var i = 0; i < rs.Friends.length; i++)
                            if (rs.Friends[i].Id == $scope.userid)
                            {
                                $scope.StatusFriend = "trusted";
                                $scope.Friend = rs.Friends[i];
                            }
                    });
                }
                if (network.Name == 'Normal Network') {
                    _networkService.GetFriendsInNetwork(network.Id).then(function (res) {
                        for (var i = 0; i < res.Friends.length; i++)
                            if (res.Friends[i].Id == $scope.userid) {
                                $scope.StatusFriend = "normal";
                                $scope.Friend = res.Friends[i];
                            }
                    });
                }
            })
        })

    }
   
    // _getFriends
    $scope.getFriends();
    $scope.invite = function () {
        if ($scope.ProfileViewed && $scope.ProfileViewed.Id) {
            swal({
                title: "Are you sure to invite this user to your network?",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes",
                cancelButtonText: "No"
            }).then(function () {
                _networkService.InviteFriend({ ReceiverId: $scope.ProfileViewed.Id }).then(function () {

                    var message = $rootScope
                        .translate('A_Request_Has_Been_Sent_To_Message') + ' ' +
                        $scope.ProfileViewed.DisplayName;
                    swal('Info', message, 'success');

                    $scope.StatusFriend = "pending";

                }, function (errors) {
                    swal('Error', errors, 'error');

                })

            }, function (dismiss) {
                if (dismiss === 'cancel') {
                }
            });
            //

        } else {
            swal('Warning', $rootScope.translate('Please_Choose_Friend_Message'), 'warning');

        }
    }

    //$scope.sendInvitations = [];
    $scope.getSend = function getSendInvitations() {
        _networkService.GetSendInvitations(true).then(function (invitations) {
            for (var i = 0; i < invitations.length; i++)
                if (invitations[i].Receiver.Id == $scope.userid) {
                    $scope.StatusFriend = "pending";
                }
        })
    }

    //$scope.invitations = [];
    $scope.getInvite = function () {
        var fromId = null;
        _networkService.GetInvitations(fromId, true).then(function (invitations) {
            for (var i = 0; i < invitations.length; i++)
                if (invitations[i].Sender.Id == $scope.userid) {
                    $scope.StatusFriend = "invitation";
                }
        })
    }
    /// run

    $scope.remove = function () {
        var message = $rootScope.translate('Are_You_Sure_To_Remove') + ' ' + $scope.Friend.DisplayName + " ?";
        swal({
            title: message,
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Yes",
            cancelButtonText: "No"
        }).then(function () {
            _networkService.RemoveFriend({ FriendId: $scope.Friend.Id, NetworkId: $scope.Friend.NetworkId })
                            .then(function () {
                             
                                $scope.StatusFriend = '';
                                swal('Remove friend', 'Ok', 'success');
                            },
                                function (errors) {
                                    swal('Remove friend', 'Error', 'error');
                                });


        }
          , function (dismiss) {
              if (dismiss === 'cancel') {
              }
          });

        setTimeout(function () {
            $('.popup-confirm-close .close-icon').click(function (e, v) {
                $('.sa-button-container .cancel').trigger("click");
            });
        }, 100);
    }
   
  
    $scope.getSend();
    $scope.getInvite();

}]);

///


