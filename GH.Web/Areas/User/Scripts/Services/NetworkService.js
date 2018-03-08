var _userApp = getApp('UserModule');

_userApp.factory('NetworkService', ['$http', '$q', function ($http, $q) {
    var _getNetworks = function (hideAjaxLoader) {
        var deferer = $q.defer();

        $http.get('/Api/Networks', { params: { hideAjaxLoader: hideAjaxLoader } })
            .success(function (networks) {
                deferer.resolve(networks);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })
        return deferer.promise;
    }

    var _getFriendsInNetwork = function (networkId, hideAjaxLoader) {
        var deferer = $q.defer();
        $http.get('/Api/Networks/Friends', { params: { networkId: networkId, hideAjaxLoader: hideAjaxLoader } })
            .success(function (friends) {
                deferer.resolve(friends);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })
        return deferer.promise;
    }

    var _searchUserForInvitation = function (keyword, hideAjaxLoader) {
        var deferer = $q.defer();
        $http.get('/Api/Networks/SearchUsersForInvitation', { params: { keyword: keyword, hideAjaxLoader: hideAjaxLoader } })
            .success(function (users) {
                deferer.resolve(users);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })
        return deferer.promise;
    }

    /* trust emergency */
     var _inviteTrustEmergency = function (model) {
          var deferer = $q.defer();
         $http.post('/Api/Networks/InviteTrustEmergency', model)
              .success(function () {
                  deferer.resolve();
              }).error(function (errors, status) {
                  __promiseHandler.Error(errors, status, deferer);
              })
          return deferer.promise;
     }
     var _updateTrustEmergency = function (model) {
         var deferer = $q.defer();
         $http.post('/Api/Networks/UpdateTrustEmergency', model)
              .success(function () {
                  deferer.resolve();
              }).error(function (errors, status) {
                  __promiseHandler.Error(errors, status, deferer);
              })
         return deferer.promise;
     }

     var _acceptTrustEmergency = function (model) {
         var deferer = $q.defer();
         $http.post('/Api/Networks/AcceptTrustEmergency', model)
            .success(function () {
                deferer.resolve();
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })
         return deferer.promise;
     }

     var _searchUserForTrustEmergency = function (keyword, hideAjaxLoader) {
         var deferer = $q.defer();
         $http.get('/Api/Networks/SearchUsersForTrustEmergency', { params: { keyword: keyword, hideAjaxLoader: hideAjaxLoader } })
             .success(function (users) {
                 deferer.resolve(users);
             }).error(function (errors, status) {
                 __promiseHandler.Error(errors, status, deferer);
             })

         return deferer.promise;
     }

    var _getFriends = function () {
        var deferer = $q.defer();
        $http.get('/Api/Networks/GetFriends')
            .success(function (users) {
                deferer.resolve(users);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })
        return deferer.promise;
    }

    var _getInvitations = function (from, hideAjaxLoader) {
        var deferer = $q.defer();

        $http.get('/Api/Networks/Invitations', { params: { fromId: from, hideAjaxLoader: hideAjaxLoader } })
           .success(function (invitations) {
               deferer.resolve(invitations);
           }).error(function (errors, status) {
               __promiseHandler.Error(errors, status, deferer);
           })
        return deferer.promise;
    }
    
    var _getSendInvitations = function (hideAjaxLoader) {
        var deferer = $q.defer();
        $http.get('/Api/Networks/GetSendInvitations', { params: { hideAjaxLoader: hideAjaxLoader } })
           .success(function (invitations) {
               deferer.resolve(invitations);
           }).error(function (errors, status) {
               __promiseHandler.Error(errors, status, deferer);
           })
        return deferer.promise;
    }

    var _inviteFriend = function (model) {
        var deferer = $q.defer();
        $http.post('/Api/Networks/Invite', model)
            .success(function () {
                deferer.resolve();
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })
        return deferer.promise;
    }

    var _acceptInvitation = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/Networks/Invite/Accept', model)
           .success(function () {
               deferer.resolve();
           }).error(function (errors, status) {
               __promiseHandler.Error(errors, status, deferer);
           })

        return deferer.promise;
    }

    var _denyInvitation = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/Networks/Invite/Deny', model)
          .success(function () {
              deferer.resolve();
          }).error(function (errors, status) {
              __promiseHandler.Error(errors, status, deferer);
          })

        return deferer.promise;
    }


    var _moveFriend = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/Networks/MoveFriend', model)
          .success(function () {
              deferer.resolve();
          }).error(function (errors, status) {
              __promiseHandler.Error(errors, status, deferer);
          })

        return deferer.promise;
    }


    var _removeFriend = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/Networks/RemoveFriend', model)
          .success(function () {
              deferer.resolve();
          }).error(function (errors, status) {
              __promiseHandler.Error(errors, status, deferer);
          })

        return deferer.promise;
    }
    var _inviteEmergencyByEmail = function (model) {
        var deferer = $q.defer();
        $http.post('/Api/Networks/InviteEmergencyByEmail', model)
          .success(function () {
              deferer.resolve();
          }).error(function (errors, status) {
              __promiseHandler.Error(errors, status, deferer);
          })

        return deferer.promise;
    }

    // 
    var _inviteEmergencyByListEmail = function (model) {
        var deferer = $q.defer();
        $http.post('/Api/Networks/InviteEmergencyByListEmail', model)
          .success(function () {
              deferer.resolve();
          }).error(function (errors, status) {
              __promiseHandler.Error(errors, status, deferer);
          })
        return deferer.promise;
    }
       //get form vault 
    var _getVaultByUserId = function (model) {
        var deferer = $q.defer();

        $http.get('/Api/Networks/GetVaultByUserId', { params: { userId: model } })
            .success(function (rs) {
                deferer.resolve(rs);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

  
    return {
        GetNetworks: _getNetworks,
        GetFriendsInNetwork: _getFriendsInNetwork,
        SearchUserForInvitation: _searchUserForInvitation,
       
        GetInvitations: _getInvitations,
        InviteFriend: _inviteFriend,      
        AcceptInvitation: _acceptInvitation,
        DenyInvitation: _denyInvitation,
        MoveFriend: _moveFriend,
        RemoveFriend: _removeFriend,
        GetFriends: _getFriends,
           
        GetSendInvitations: _getSendInvitations,
        InviteEmergencyByEmail: _inviteEmergencyByEmail,
        GetVaultByUserId: _getVaultByUserId,
        InviteTrustEmergency: _inviteTrustEmergency,
        UpdateTrustEmergency: _updateTrustEmergency,
        AcceptTrustEmergency: _acceptTrustEmergency,
        SearchUserForTrustEmergency: _searchUserForTrustEmergency,
        InviteEmergencyByListEmail: _inviteEmergencyByListEmail

    }
}])
