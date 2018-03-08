var myApp = getApp("myApp", true);
myApp.getController('SRFIBusinessController', ['$scope', '$rootScope', '$sce', 'BusinessAccountService', 'SweetAlert', 'UserManagementService', '$uibModal', '$http', function ($scope, $rootScope, $sce, _baService, _sweetAlert, UserManagementService, $uibModal, $http) {

    $scope.srfis = [];
    $scope.profileSRFI = {};
    $scope.selectSRFI = function (srfi) {
        $scope.showingSRFISelect = false;
        $scope.openSRFIForm($scope.profileSRFI, srfi);
    };
    $scope.loadProfile = function () {
        $http.post('/api/CampaignService/GetSRFIActive')
         .success(function (response) {
             $scope.profileSRFI.BusId = response.BusId;
             $scope.profileSRFI.DisplayName = response.DisplayName;
             $scope.profileSRFI.Avatar = response.Avatar;
             $scope.srfis = response.ListCampaign;

         }, function (errors) {
             __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
         })
    }
    $scope.loadProfile();
    $scope.openSRFIForm = function (profile, srfi) {
        var data = new Object();
        data.BusinessUserId = profile.BusId;
        data.DisplayName = profile.DisplayName;
        data.Avatar = profile.Avatar;
        data.CampaignId = srfi.CampaignId;
        data.Fields = srfi.Fields;
        data.Name = srfi.Name;
        data.termsUrl = srfi.TermsUrl;
        data.Description = srfi.Description;
        data.CampaignType = "SRFI";
        data.Verb = srfi.Verb;
        var modalInstance = $uibModal.open({
            animation: $scope.animationsEnabled,
            templateUrl: '/Areas/User/Views/Shared/Template/srfi-form.html?v=2',
            controller: 'FormSRFIBusinessController',
            size: "",
            backdrop: 'static',
            resolve: {
                registerPopup: function () {
                    return {
                        data: data
                    };
                }
            }
        });
    }
}])

myApp.controller('FormSRFIBusinessController',
    ['$scope', '$rootScope', '$uibModalInstance', '$uibModal', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'NotificationService', 'registerPopup', 'SmSAuthencationService', 'BusinessAccountService', 'interactionFormService', 'formService',

        function ($scope, $rootScope, $uibModalInstance, $uibModal, $http, userManager, sweetAlert, authService, alertService, notificationService, registerPopup, SmSAuthencationService, _baService, interactionFormService, formService) {
            $scope.profile = {};
            $scope.profile.userId = registerPopup.data.BusinessUserId;
            $scope.profile.displayName = registerPopup.data.DisplayName;
            $scope.profile.avatar = registerPopup.data.Avatar;

            $scope.campaign = {};
            $scope.campaign.campaignId = registerPopup.data.CampaignId;
            $scope.campaign.campaignType = registerPopup.data.CampaignType;
            $scope.campaign.name = registerPopup.data.Name;
            $scope.campaign.description = registerPopup.data.Description;
            $scope.campaign.verb = registerPopup.data.Verb;
         
            $scope.interaction = {
                id: registerPopup.data.CampaignId,
                type: registerPopup.data.CampaignType,
                name: registerPopup.data.Name,
                description: registerPopup.data.Description,
                verb: registerPopup.data.Verb,
                business: {
                    name: registerPopup.data.DisplayName,
                    id: registerPopup.data.BusinessUserId,
                    avatar: registerPopup.data.Avatar
                },
                fields: registerPopup.data.Fields
            };

            $scope.groupField = interactionFormService.sortField(registerPopup.data.Fields);
            $scope.Mode = "ReadOnly";
            $scope.inviteType = "member";
            $scope.ListMember = [];
            $scope.memberSelected = { UserId: null };
            $scope.recipientsText = null ;
            $scope.GetMembers = function () {
                var userId = $scope.profile.userId;
                _baService.GetMembersFollowInBusiness(userId).then(function (result) {
                    $scope.ListMember = result;
                })
            }
            $scope.GetMembers();
            $scope.listEmail = null;
            $scope.cancel = function () {
                $uibModalInstance.dismiss('cancel');
            };
            $scope.isInvite = false;
            $scope.invite = function ()
            {
                var data = new Object();
                data.FromUserId = $scope.profile.userId;
                    data.FromDisplayName = $scope.profile.displayName;
                    data.Comment = $scope.comment;
                    data.ToUserId = $scope.memberSelected.UserId;
                    data.ToDisplayName = $scope.memberSelected.DisplayName;
                    data.CampaignId = $scope.campaign.campaignId;
                    data.CampaignType = $scope.campaign.campaignType;
                    data.InviteType = $scope.inviteType;
                    data.ListEmail = $scope.listEmail;
                    $http.post('/api/CampaignService/InviteSRFI', data)
                     .success(function (response) {
                         $scope.isInvite = true;
                         $scope.listEmailInsite = response.ListEmailInSite;
                         $scope.listEmailOutsite = response.ListEmailOutSite;
                     })
                        .error(function (errors, status) {
                        });
            }
            //
            $scope.parseRecipients = function () {
                var _recipients = $scope.recipientsText.match(/[a-z0-9_\-.]+@[a-z0-9_\-.]+\.([a-z]{2,})/gi) || [];
                $scope.listEmail = [];
                $.each(_recipients, function (i, email) {
                    email = email.toLowerCase();
                    if ($.inArray(email, $scope.listEmail) === -1)
                        $scope.listEmail.push(email);
                });
                if ($scope.listEmail.length) {
                    $scope.showMessage('Detected ' + $scope.listEmail.length + ' email addresses');

                }

            };
            $scope.showMessage = function (msg) {
                $scope.invite.message = msg;
            };


            $scope.viewSrfiForm = function() {

                formService.openInteractionFormViewer($scope.interaction,'fields',$scope);
                // $scope.interaction.editing = true;
                // formService.openInteractionForm($scope.interaction,$scope);
            }

        }
    ])
myApp.controller('SRFIFormRegisUserController', ['$scope', '$rootScope', '$uibModalInstance', '$uibModal', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'NotificationService', 'registerPopup', 'SmSAuthencationService', 'BusinessAccountService', 'interactionFormService',

    function ($scope, $rootScope, $uibModalInstance, $uibModal, $http, userManager, sweetAlert, authService, alertService, notificationService, registerPopup, SmSAuthencationService, _baService, interactionFormService) {

        $scope.profile = {};
        $scope.profile.userId = registerPopup.data.BusinessUserId;
        $scope.profile.displayName = registerPopup.data.DisplayName;
        $scope.profile.avatar = registerPopup.data.Avatar;
        $scope.campaign = {};
        $scope.campaign.campaignId = registerPopup.data.CampaignId;
        $scope.campaign.campaignType = registerPopup.data.CampaignType;
        $scope.campaign.name = registerPopup.data.Name;
        $scope.campaign.comment = registerPopup.data.Comment;
        $scope.campaign.ToUserId = registerPopup.data.UserId;
        $scope.campaign.NotificationId = registerPopup.data.NotificationId;
        $scope.campaign.status = registerPopup.data.Status;
     
        $scope.getValueField = function()
        {
            var regis = new Object();
            regis.ToUserId = $scope.campaign.ToUserId;
            regis.CampaignId = $scope.campaign.campaignId;
            $http.post('/api/CampaignService/GetSRFIForRegis', regis)
                  .success(function (response) {
               
                      $scope.groupField = interactionFormService.sortField(response);
                  }).error(function (errors, status) { });
        }
        $scope.getValueField();
      
        $scope.Mode = "Edit";
        $scope.isAccept = false;
        if ($scope.campaign.status == 'Accept')
            $scope.isAccept = true;
      
        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        };
      
        $scope.RegisterCampaign = function () {
            var isvalid = true;
            var fields = [];
          
            for (var i = 0; i < $scope.groupField.length; i++)
            {
                for(var j = 0; j < $scope.groupField[i].fields.length; j++ )
                {
                    var field = angular.copy($scope.groupField[i].fields[j]);
                    if (isvalid)
                        isvalid = $scope.vaildfiled($scope.groupField[i].fields[j]);
                    if (!isvalid)
                        return;

                    fields.push(field);
                }
            }
            if (isvalid)
            {
                var dataRegis = new Object();
                dataRegis.CampaignId = $scope.campaign.campaignId;
                dataRegis.ToUserId = $scope.campaign.ToUserId;
                dataRegis.NotificationId = $scope.campaign.NotificationId;
                dataRegis.comment = $scope.campaign.comment;
                dataRegis.Fields = fields
                $http.post('/api/CampaignService/RegisSRFI', dataRegis)
                    .success(function (response) {
                        $scope.isAccept = true;
                    }).error(function (errors, status) {});
            }
           
        }

        //Valid Field
        $scope.vaildfiled = function (field) {

            var isvalid = true;
            if ((field.model == "" || field.model == undefined || field.model == null) && field.options !== 'indef') {
                isvalid = false;
            }
            switch (field.type) {
                case "doc":
                    isvalid = (angular.isArray(field.model) && field.model.length)
                        || (field.hasOwnProperty('modeldocvault') && field.modeldocvault.length);
                    break;
                case "address":
                    if (field.model == null || field.model.address == "")
                        isvalid = false;
                    break;
                case "location":
                    if (field.model == null || field.model.country == "" || (field.model.city == null && field.options != 'nocity'))
                        isvalid = false;
                    break;
                case "date":
                    if (field.model === "Invalid Date")
                        isvalid = false;
                    break;
            }
            if (field.optional && !field.selected)
                isvalid = true;
            return isvalid;
        };


    }
]);
