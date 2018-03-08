// ====== //
myApp.getController('ManageNetworkController', ['$scope', '$rootScope', '$http', 'NetworkService', 'SweetAlert', '$interval', 'rguNotify', function ($scope, $rootScope, $http, _networkService, _sweetAlert, $interval, rguNotify) {

    $scope.networks = [];
    $scope.normalNetwork = {};
    $scope.trustedNetwork = {};

    $scope.relationships = ['Father', 'Mother', 'Son', 'Daughter', 'Brother', 'Sister', 'Husband', 'Wife', 'Uncle', 'Aunt', 'Niece',
        'Nephew', 'Grandmother', 'Grandfather', 'Friend', 'Father-in-law', 'Mother-in-law', 'In-law', 'Other'];
    $scope.relationship = {value: 'Emergency'};
    $scope.list_rate = [1, 2, 3];
    $scope.editTrustFriends = [];
    // Init data
    $scope.InitData = function () {
        $scope.showSendInvitations = false;
        $scope.sendInvitations = [];
        $scope.getSend = function getSendInvitations() {
            _networkService.GetSendInvitations(true).then(function (invitations) {
                $scope.sendInvitations = invitations;
            })
        }
        $scope.getSend();

        _networkService.GetNetworks().then(function (networks) {
            $scope.networks = networks;
            $scope.networks.forEach(function (network) {
                if (network.Name == 'Trusted Network') {
                    _networkService.GetFriendsInNetwork(network.Id).then(function (rs) {
                        $scope.trustedNetwork.Id = network.Id;
                        $scope.trustedNetwork.Name = network.Name;
                        $scope.trustedNetwork.TotalFriends = rs.Total;
                        $scope.trustedNetwork.Friends = [];
                        _networkService.GetSendInvitations(true).then(function (invitations) {
                            $(rs.Friends).each(function (index, object) {
                                var status = "";
                                $(invitations).each(function (ix, object) {
                                    if (invitations[ix].Receiver.Id == rs.Friends[index].Id)
                                        status = "Pending";
                                })
                                var friend = {
                                    Id: rs.Friends[index].Id,
                                    Avatar: rs.Friends[index].Avatar,
                                    DisplayName: rs.Friends[index].DisplayName,
                                    UserId: rs.Friends[index].UserId,
                                    Relationship: rs.Friends[index].Relationship,
                                    Rate: rs.Friends[index].Rate,
                                    NetworkId: rs.Friends[index].NetworkId,
                                    IsEmergency: rs.Friends[index].IsEmergency,
                                    Status: status,
                                    Editting: false,
                                    SetEmergency: rs.Friends[index].IsEmergency
                                };
                                $scope.trustedNetwork.Friends.push(friend);
                            });
                            $scope.editTrustFriends = angular.copy($scope.trustedNetwork.Friends);
                        });
                    });
                }
                if (network.Name == 'Normal Network') {
                    _networkService.GetFriendsInNetwork(network.Id).then(function (friends) {
                        $scope.normalNetwork.Id = network.Id;
                        $scope.normalNetwork.Name = network.Name;
                        $scope.normalNetwork.TotalFriends = friends.Total;
                        $scope.normalNetwork.Friends = [];
                        $(friends.Friends).each(function (index, object) {
                            if (friends.Friends[index].Id)
                                $scope.normalNetwork.Friends.push({
                                    Id: friends.Friends[index].Id,
                                    Avatar: friends.Friends[index].Avatar,
                                    DisplayName: friends.Friends[index].DisplayName,
                                    Relationship: "",
                                    IsInviteEmergency: false,
                                    IsEmergency: false,
                                    IsSendEmergency: false
                                });
                        });
                    });
                }
            })
        })
    }
    $scope.InitData();


    $scope.$on('InitData', function () {
        $scope.InitData();
    });
    // End init data

    var fromId = null;
    $scope.invitations = [];

    function getInvitations() {
        _networkService.GetInvitations(fromId, true).then(function (invitations) {
            $scope.invitations = $scope.invitations.concat(invitations);
            var from = invitations[invitations.length - 1];
            if (from) {
                fromId = from.Id;
            }
        })
    }

    getInvitations();

    $interval(getInvitations, 10000);

    $scope.findFriends = function (keyword) {
        return _networkService.SearchUserForInvitation(keyword, true).then(function (users) {
            return users;
        });
    }

    $scope.invitation = null;

    $scope.invite = function () {
        if ($scope.invitation && $scope.invitation.Id) {
            swal({
                title: "Are you sure to invite this user to your network?",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes",
                cancelButtonText: "No"
            }).then(function () {
                _networkService.InviteFriend({ReceiverId: $scope.invitation.Id}).then(function () {

                    rguNotify.add('Network invitation sent to ' + $scope.invitation.DisplayName);
                    $scope.invitation = null;
                    $rootScope.$broadcast('InitData');
                    $rootScope.$broadcast('network:update');

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

    $scope.accept = function (invitation, idx) {
        _networkService.AcceptInvitation({InvitationId: invitation.Id}).then(function () {
            $scope.normalNetwork.Friends.push(invitation.Sender);
            var message = $rootScope
                    .translate('You_Become_Friend_With_Message') + ' ' +
                invitation.Sender.DisplayName;
            swal('Success', message, 'success');
            $scope.invitations.splice(idx, 1);
        }, function (errors) {
            __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
        })
    }


    $scope.deny = function (invitation, idx) {
        swal({
            title: "Are you sure to decline this invitation?",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Yes",
            cancelButtonText: "No"
        }).then(function () {
                _networkService.DenyInvitation({InvitationId: invitation.Id})
                    .then(function () {
                            swal('Deny', 'OK', 'success');
                            $scope.invitations.splice(idx, 1);
                        },
                        function (errors) {
                            swal('Deny', 'OK', 'error');
                        });

            }
            , function (dismiss) {
                if (dismiss === 'cancel') {
                }
            });
    }
    $scope.moveFriend = function (friend, fromNetwork, toNetwork, idx) {
        _networkService.MoveFriend({FriendId: friend.Id, FromNetworkId: fromNetwork.Id, ToNetworkId: toNetwork.Id})
            .then(function () {
                    fromNetwork.Friends.splice(idx, 1);
                    toNetwork.Friends.push(friend);
                    $rootScope.$broadcast('InitData');
                    var message = friend.DisplayName +
                        ' ' +
                        $rootScope.translate('Has_Been_Moved_To_Message') +
                        ' ' +
                        toNetwork.Name;
                    swal('Info', message, 'success');

                },
                function (errors) {
                    swal('Move friend', 'error', 'Error');
                });
    }

    $scope.removeFriend = function (friend, fromNetwork, idx) {
        var message = $rootScope.translate('Are_You_Sure_To_Remove') + ' ' + friend.DisplayName + " ?";
        swal({
            title: message,
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Yes",
            cancelButtonText: "No"
        }).then(function () {
                _networkService.RemoveFriend({FriendId: friend.Id, NetworkId: fromNetwork.Id})
                    .then(function () {
                            fromNetwork.Friends.splice(idx, 1);
                            // swal('Remove friend', 'Ok', 'success');
                            rguNotify.add('Removed one member from network')
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

    // Invite by email
    $scope.inviteByEmail = function (value) {
        if (value != '') {
            swal({
                title: "Are you sure to send email to " + value + " ?",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes",
                cancelButtonText: "No"
            }).then(function () {
                    _networkService.InviteEmergencyByEmail({
                        ToEmail: value,
                        IsEmergency: true,
                        Rate: 1,
                        InviteId: ""
                    }).then(function () {

                        $scope.invitation = null;
                        swal('OK', '', 'success');
                    });

                }

                , function (dismiss) {
                    if (dismiss === 'cancel') {

                    }
                });
        } else {
            swal('Error', $rootScope.translate('Warning_Title'), 'Please enter email', 'error');
        }
    }

    // Son  
    $scope.beginEditing = function (member) {
        member.Editting = true;
        member.SetEmergency = member.IsEmergency;
    };

    $scope.endEditing = function (member) {
        member.Editting = false;
    };
    $scope.cancelEditing = function (member) {
        var index = $scope.trustedNetwork.Friends.indexOf(member);
        $($scope.editTrustFriends).each(function (indx, object) {
            if (member.Id == $scope.editTrustFriends[indx].Id) {
                $scope.trustedNetwork.Friends[index] = angular.copy($scope.editTrustFriends[indx]);
            }
        });

        member.Editting = false;
    };
    $scope.updateEditing = function (member) {
        if (member.SetEmergency) {
            $scope.UpdateTrustEmergency(member);
        }
        $scope.endEditing(member);
    };
    $scope.addEditing = function (member) {
        if (!member.SetEmergency) {
            $scope.InviteTrustEmergency(member);
        }
        $scope.endEditing(member);
    };
    // end son
    $scope.InviteTrustEmergency = function (value) {
        if (value.IsEmergency == false) {

            _networkService.UpdateTrustEmergency({
                Id: value.Id,
                NetworkId: value.NetworkId,
                DisplayName: value.DisplayName,
                UserId: value.UserId,
                Relationship: value.Relationship,
                IsEmergency: value.IsEmergency,
                Rate: value.Rate
            })
                .then(function () {
                    var idx = $scope.trustedNetwork.Friends.indexOf(value);
                    $scope.trustedNetwork.Friends[idx].Editting = false;
                });
        }
        else {
            var msg = "You are inviting " + value.DisplayName + " to be your emergency contact. If accepted by the recipient, you will be able to see their basic contact information and use it for registration purposes.";
            swal({
                title: msg,
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes",
                cancelButtonText: "No"
            }).then(function () {
                var index = $scope.trustedNetwork.Friends.indexOf(value);
                $($scope.editTrustFriends).each(function (indx, object) {
                    if (value.Id == $scope.editTrustFriends[indx].Id) {
                        $scope.trustedNetwork.Friends[index] = angular.copy($scope.editTrustFriends[indx]);
                    }
                });

                _networkService.InviteTrustEmergency({
                    ReceiverId: value.Id,
                    Relationship: value.Relationship,
                    IsEmergency: value.IsEmergency,
                    Rate: value.Rate
                }).then(function () {
                    $scope.InitData();
                    value.Editting = false;
                    var message = $rootScope
                            .translate('A_Request_Has_Been_Sent_To_Message') + ' ' +
                        value.DisplayName;
                    swal('Info', message, 'success');
                }, function (errors) {
                    swal('Invite emergency', 'Error', 'error');
                })
            }, function (dismiss) {
                if (dismiss === 'cancel') {
                }
            });
        }

    }
    //update
    $scope.UpdateTrustEmergency = function (value) {
        if (value.Id != null) {
            _networkService.UpdateTrustEmergency({
                Id: value.Id,
                NetworkId: value.NetworkId,
                DisplayName: value.DisplayName,
                UserId: value.UserId,
                Relationship: value.Relationship,
                IsEmergency: value.IsEmergency,
                Rate: value.Rate
            })
                .then(function () {
                    var idx = $scope.trustedNetwork.Friends.indexOf(value);
                    $scope.trustedNetwork.Friends[idx].Editting = false;
                });
        }
    }

    $scope.acceptTrustEmergency = function (invitation, idx) {
        _networkService.AcceptTrustEmergency({
            InvitationId: invitation.Id,
            Relationship: invitation.Relationship,
            IsEmergency: invitation.IsEmergency,
            Rate: invitation.Rate
        }).then(function () {
            $scope.invitations.splice(idx, 1);
            $rootScope.$broadcast('InitData');
            var message = "You have accepted " + invitation.Sender.DisplayName + "'s emergency contact invitation.";
            swal('Accept Emergency Contact Invitation', 'Ok', 'success');
        }, function (errors) {
            swal('Accept Emergency Contact Invitation', 'Error', 'error');
        })

    }

    $scope.ViewInvitation = function (invitation, idx) {
        var message = "<span  class='text-justify' style='font-size: 11'> <strong>" + invitation.Sender.DisplayName + "</trong>" +
            " has invited you to be an emergency contact. By accepting, you authorize to share certain information with the requester on a continuous basis for emergency purposes." + "</span>";
        swal({
            title: 'Emergency Contact Invitation',
            html: message,
            showCancelButton: true,
            showCloseButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Yes",
            cancelButtonText: "Decline"
        }).then(function () {
                _networkService.AcceptTrustEmergency({
                    InvitationId: invitation.Id,
                    Relationship: invitation.Relationship,
                    IsEmergency: invitation.IsEmergency,
                    Rate: invitation.Rate
                }).then(function () {
                    $scope.invitations.splice(idx, 1);
                    $rootScope.$broadcast('InitData');
                    var message = "You have accepted " + invitation.Sender.DisplayName + "'s emergency contact invitation.";
                    swal('Accept Emergency Contact Invitation', 'Ok', 'success');
                }, function (errors) {
                    swal('Accept Emergency Contact Invitation', 'Error', 'error');
                })
            }
            , function (dismiss) {
                if (dismiss === 'cancel') {
                    _networkService.DenyInvitation({InvitationId: invitation.Id})
                        .then(function () {
                                swal('Decline Emergency Contact Invitation', 'OK', 'success');
                                $scope.invitations.splice(idx, 1);
                            },
                            function (errors) {
                                swal('Decline Emergency Contact Invitation', 'OK', 'error');
                            });
                }
            });

    }

}])
