﻿var myApp = getApp("myApp", true);
// var myApp = angular.module('myApp');

myApp.getController("HandshakeRequestController", ['$scope', '$http', 'rguModal', 'rguNotify', '$uibModal', function ($scope, $http, rguModal, rguNotify, $uibModal) {

    $scope.profile = new Object();
    $scope.loadShortProfile = function () {
        $http.get('/api/account/shortprofile')
       .success(function (response) {
           $scope.profile = response;
       }, function (errors) {
           __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
       })
    }
    $scope.loadShortProfile();

    var toUser = {
        UserId: '',
        AccountId: '',
        FirstName: '',
        LastName: '',
        Email: ''
    };

    var isUcb = regitGlobal.hasOwnProperty('ucb');

    $scope.checkRequest = function () {
        var userId = regitGlobal.viewedProfileId;
        if (!isUcb) {
            $http.get('/api/account/requesthandshake?userId=' + userId)
                .success(function (response) {
                    $scope.business = response;
                    $scope.enableRequest = response.FieldEnable;
                    toUser.UserId = response.UserId;
                    toUser.AccountId = response.AccountId;
                    toUser.FirstName = response.FirstName;
                    toUser.LastName = response.LastName;
                    toUser.DisplayName = response.DisplayName;
                    toUser.Email = response.Email;
                }, function (errors) {
                    __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
                });
        } else {
            //UserCreatedBusiness

            toUser.AccountId = regitGlobal.ucb.id;
            toUser.Email = regitGlobal.ucb.email;
            toUser.DisplayName = regitGlobal.ucb.name;
            toUser.BusinessType = 'ucb';
            $scope.enableRequest = true;
        }
    };
    $scope.checkRequest();

    $scope.openHsr = function () {
        var user = $scope.profile;
        if (!user) {
            user = {
                userId: '',
                firstName: '',
                lastName: '',
                phone: '',
                email: ''
            }
        }
        $scope.hsrView = { agreed: false };
        $scope.hsr = {
            business: toUser,
            user: user,
            form: {
                firstName: user.firstName,
                lastName: user.lastName,
                phone: user.phone,
                email: user.email,
                message: ''
            }
        };
        rguModal.openModal('handshake.request', $scope);

    };

    $scope.canSendHsr = function () {
        return $scope.hsrView.agreed && ($scope.hsr.form.phone || $scope.hsr.form.email);
    };


    $scope.closeHsr = function (hideFunc) {
        if (angular.isFunction(hideFunc)) {
            hideFunc();
        }
    };


    $scope.sendHsr = function (hideFunc) {
        var data = new Object();
        if (isUcb)
            data.Type = "ucb";

        data.ToUserId = toUser.AccountId;
        data.ToDisplayName = toUser.DisplayName;
        data.ToEmail = toUser.Email;
        data.FromUserId = $scope.hsr.user.userId;
        data.FirstName = $scope.hsr.form.firstName;
        data.LastName = $scope.hsr.form.lastName;
        data.Phone = $scope.hsr.form.phone;
        data.Email = $scope.hsr.form.email;
        data.Message = $scope.hsr.form.message;

        $http.post('/Api/BusinessAccount/requesthandshake/send', data)
          .success(function (response) {
              $scope.closeHsr(hideFunc);
              rguNotify.add('Sent handshake request to ' + $scope.hsr.business.DisplayName);
          })
          .error(function (errors, status) {

          })
    }
}])
myApp.getController("RequestManageController", ['$scope', '$rootScope', '$http', '$document', 'rguModal', 'rguNotify', '$uibModal', function ($scope, $rootScope, $http, $document, rguModal, rguNotify, $uibModal) {

    $scope.listRequest = [];

    $scope.enableRequest = null;

    $scope.checkRequest = function () {
        var field = new Object();

        $http.get('/api/Account/requesthandshake')
         .success(function (response) {
             $scope.enableRequest = response.FieldEnable;

         }, function (errors) {
             __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
         })
    }

    $scope.setRequest = function () {
        var field = new Object();
        if ($scope.enableRequest)
            field.Role = "on";
        else
            field.Role = "off";

        $http.post('/api/Account/requesthandshake/updateprivacy', field)
         .success(function (response) {

         }, function (errors) {
             __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
         })
    }
    $scope.checkRequest();

    $scope.loadRequest = function () {
        $http.get('/api/businessaccount/requesthandshake/business')
         .success(function (response) {
             $scope.listRequest = response;

         }, function (errors) {
             __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
         })
    }

    $scope.loadRequest();
    $scope.$on('loadRequest', function () {
        $scope.loadRequest();
    });
    $scope.$on('handshake:notification', function () {
        $scope.loadRequest();
    });

    var dataForm = new Object();
    $scope.loadShortProfile = function () {

        $http.get('/api/account/shortprofile')
       .success(function (response) {
           dataForm.userId = response.userId;
           dataForm.displayName = response.displayName;
           dataForm.avatarUrl = response.avatarUrl;
       }, function (errors) {
           __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
       })
    }
    $scope.loadShortProfile();
    $scope.mark = function (hs) {

        hs.Status = "Read";
        $http.post('/api/BusinessAccount/requesthandshake/update', hs)
       .success(function (response) {

       })
       .error(function (errors, status) {
       });
    }
    $scope.delete = function (hs) {
        swal({
            title: "Are you sure you want to delete handshake request from " + hs.FirstName + " " + hs.LastName + " ?",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Yes",
            cancelButtonText: "No"
        }).then(function () {
            hs.Status = "Delete";
            $http.post('/api/BusinessAccount/requesthandshake/update', hs)
           .success(function (response) {

           })
           .error(function (errors, status) {
           });

        }, function (dismiss) {
            if (dismiss === 'cancel') {
            }
        });
        //

    }

    $scope.openlistHandShakeForm = function (handshakeRequest, srfi) {
        dataForm.handshakeRequest = handshakeRequest;
        var type = 'Handshake';
        // $http.get('/api/campaign/campaigns/request?type=' + type + '&userid=' + handshakeRequest.ToUserId)
        $http.get('/api/interactions/get/interactions?type=' + type + '&userid=' + handshakeRequest.ToUserId)
     .success(function (response) {
         dataForm.campaigns = response;
         var modalInstance = $uibModal.open({
             animation: $scope.animationsEnabled,
             templateUrl: '/Areas/User/Views/Shared/Template/HandshakeManageForm.html',
             controller: 'FormRequestController',
             size: "",
             backdrop: 'static',
             resolve: {
                 registerPopup: function () {
                     return {
                         data: dataForm
                     };
                 }
             }
         });
     }, function (errors) {
         __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
     })
    }

    //

    $scope.view = { openingMoreActions: false };
    function documentClick() {
        $scope.$apply(function () {
            $scope.view.openingMoreActions = false;
        });
    }
    $scope.view.openMoreActions = function (key, event) {
        event.stopPropagation();
        $scope.view.openingMoreActions = key;
        $document.on('click', documentClick);
    };
    $scope.closeMoreActions = function (interaction) {
        $scope.view.openingMoreActions = false;
        $document.off('click', documentClick);
    };
    $scope.$on('$destroy', function () {
        $document.off('click', documentClick);
    });

    $scope.openHsrViewer = function (hsr) {
        $scope.hsrViewer = {
            id: hsr.Id,
            toBusinessId: hsr.ToUserId,
            firstName: hsr.FirstName,
            lastName: hsr.LastName,
            phoneNumber: hsr.Phone,
            email: hsr.Email
        };
        rguModal.openModal('handshake.request.viewer', $scope);
    };
    $scope.closeHsrViewer = function (hideFunc) {
        if (angular.isFunction(hideFunc)) {
            hideFunc();
        }
    };

    //

    $scope.exportAllData = function () {
        //requesthandshakevalid/business
        var items = [];

        var fileName = dataForm.displayName.replace(/\s+/g, '') + '_AllHandshakeRequest.xlsx';

        for (var i = 0; i < $scope.listRequest.length; i++) {
            var date = new Date();
            if ($scope.listRequest[i].CreatedDate != null)
                date = new Date($scope.listRequest[i].CreatedDate);
            if ($scope.listRequest[i].Status == null)
                $scope.listRequest[i].Status = "Send";
            if ($scope.listRequest[i].Status != "Delete")
            {
                items.push({
                    "First Name": $scope.listRequest[i].FirstName,
                    "Last Name": $scope.listRequest[i].LastName,
                    "Email": $scope.listRequest[i].Email,
                    "Phone": $scope.listRequest[i].Phone,
                    "CreatedDate": date,
                    "Message": $scope.listRequest[i].Message,
                    "Status": $scope.listRequest[i].Status
                })
            }
          
        }
        if (items.length > 0) {
            var opts = {
                headers: true,
                column: { style: { Font: { Bold: "1" } } },

            };

            alasql('SELECT * INTO xlsx("' + fileName + '",?) FROM ?', [opts, items]);
        }



    }


}]);
// myApp.getController('FormRequestController',
myApp.controller('FormRequestController',
    ['$scope', '$rootScope', '$uibModalInstance', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'NotificationService', 'registerPopup', 'SmSAuthencationService', 'BusinessAccountService', 'interactionFormService',

        function ($scope, $rootScope, $uibModalInstance, $http, userManager, sweetAlert, authService, alertService, notificationService, registerPopup, SmSAuthencationService, _baService, interactionFormService) {
            $scope.profile = {};
            $scope.profile.userId = registerPopup.data.userId;
            $scope.profile.displayName = registerPopup.data.displayName;
            $scope.profile.avatar = registerPopup.data.avatarUrl;
            $scope.requestHandshake = registerPopup.data.handshakeRequest;
            $scope.listHandShake = registerPopup.data.campaigns;
            var data = new Object();
            console.log(registerPopup)
            data = registerPopup.data.handshakeRequest;
            $scope.cancel = function () {
                $rootScope.$broadcast('loadRequest');
                $uibModalInstance.dismiss('cancel');
            };

            $scope.sendInvite = function (hs) {
                swal({
                    title: "Are you sure to invite " + $scope.requestHandshake.FirstName + " " + $scope.requestHandshake.LastName + " to this handshake?",
                    type: "warning",
                    showCancelButton: true,
                    confirmButtonColor: "#DD6B55",
                    confirmButtonText: "Yes",
                    cancelButtonText: "No"
                }).then(function () {
                    data.InteractionId = hs.Id;
                    $http.post('/api/CampaignService/request/invite', data)
                   .success(function (response) {
                       hs.status = 'Complete';
                   })
                   .error(function (errors, status) {
                   });

                }, function (dismiss) {
                    if (dismiss === 'cancel') {
                    }
                });
                //

            }
            $scope.showForm = false;
            $scope.viewForm = function (hs) {
                $scope.groupField = null;
                if (hs.campaign.fields)
                    $scope.groupField = interactionFormService.sortField(hs.campaign.fields);
                $scope.showForm = true;
            }
            $scope.closeForm = function () {
                $scope.showForm = false;
            }
        }
    ]);

myApp.getController('HsrEmailNotifController', ['$scope', '$rootScope', '$http', 'AuthorizationService',
    function ($scope, $rootScope, $http, _authorizationService) {
        var syncFromData = new Object();
        $scope.notif = {
            options: [
                {
                    name: 'none',
                    label: 'No email notification'
                },
                {
                    name: 'text',
                    label: 'Notification only'
                },
                {
                    name: 'data',
                    label: 'Notification with data'
                },
                {
                    name: 'attachment',
                    label: 'Notification with data file attachment'
                }
            ],
            option: 'text',
            sendMe: true,
            sendBusiness: false,
            sendMore: false,
            recipients: [],
            message: ''
        };
        $scope.initdata = function () {

            // _authorizationService.GetOutsiteSyncByUserId().then(function (rs) {


            $http.get('/api/businessaccount/requesthandshake/emailnotification').success(function (rs) {
                syncFromData = rs;
                $scope.myEmail = rs.Email;
                if (rs.Id != null) {

                    if (rs.ListEmail != null)
                        $scope.notif.recipientsText = rs.ListEmail.join(' ');

                    $scope.notif.option = rs.Option;
                    $scope.notif.sendMe = rs.SendMe;
                    if (rs.ListEmail.length > 0)
                        $scope.notif.sendMore = true;
                }

            }, function (errors) {
                __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
            });



        }
        $scope.initdata();




        $scope.notif = {
            options: [
                {
                    name: 'none',
                    label: 'No email notification'
                },
                {
                    name: 'text',
                    label: 'Notification only'
                },
                {
                    name: 'data',
                    label: 'Notification with data'
                },
                {
                    name: 'attachment',
                    label: 'Notification with data file attachment'
                }
            ],
            option: 'text',
            sendMe: true,
            sendBusiness: false,
            sendMore: false,
            recipients: [],
            message: ''
        };
        $scope.parseRecipients = function () {
            var recipients = $scope.notif.recipientsText.match(/[a-z0-9_\-.]+@[a-z0-9_\-.]+\.([a-z]{2,})/gi) || [];
            $scope.notif.recipients = [];
            $.each(recipients, function (i, email) {
                email = email.toLowerCase();
                if ($.inArray(email, $scope.notif.recipients) === -1)
                    $scope.notif.recipients.push(email);
            });

        };

        $scope.showMessage = function (msg) {
            $scope.notif.message = msg;
        };

        $scope.close = function () {
            $scope.$parent.view.showingHsrNotifPopover = false;
        };

        $scope.save = function () {


            if ($scope.notif.option !== 'none') {

                if ($scope.notif.sendMore) {

                    if (!$scope.notif.recipientsText) {
                        $scope.showMessage('Please enter at least one email address');
                        return;
                    }
                    $scope.parseRecipients();
                    if (!$scope.notif.recipients.length) {
                        $scope.showMessage('Please enter at least one valid email address');
                        return;
                    }
                }

            }
            if (syncFromData.Id != null) {

                syncFromData.ListEmail = $scope.notif.recipients;
                //
                // syncFromData.vm.campaignid;
                syncFromData.Option = $scope.notif.option;
                syncFromData.SendMe = $scope.notif.sendMe;
                _authorizationService.UpdateOutsiteSync(syncFromData).then(function () {

                }, function (errors) {
                    swal('Error', errors, 'error');

                })

            } else {
                var syncMail = new Object();
                syncMail.ListEmail = $scope.notif.recipients;

                if ($scope.notif.sendMore == false)
                    syncMail.Option = [];
                else
                    syncMail.Option = $scope.notif.option;

                syncMail.SendMe = $scope.notif.sendMe;
                syncMail.Type = "Handshake Request Email Notification";


                // syncMail.CompnentId = $rootScope.camId;
                _authorizationService.InsertOutsite(syncMail).then(function () {

                }, function (errors) {
                    swal('Error', errors, 'error');

                })

            }

            $scope.close();
        };

    }]);