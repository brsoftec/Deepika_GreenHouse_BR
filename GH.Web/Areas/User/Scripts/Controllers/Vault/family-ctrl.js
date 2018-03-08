
var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.cellNav'], true);

//3.0 Group Address
myApp.getController('RelationshipController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter', 'NetworkService',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter, _networkService) {

    "use strict";
   
    $scope.InitData = function () {
        $scope.IsEdit = false;
        $scope.list_group = ["Family", "Pet", "Emergency"];
        $scope._select = { _value: '' };

        if ($scope._select._value == '')
            $scope._select._value = 'Family';
    }

    $scope.InitData();

    $scope.Edit = function () {
        $scope.IsEdit = true;
    }

}])
//7.0 Family 
myApp.getController('FamilyController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter) {

    "use strict";
    var BasicInfo = VaultInformationService.VaultInformation.family;
    $scope.InitData = function () {
        $scope.IsEdit = false;

        $scope._form = {
            _label: BasicInfo.label,
            _name: BasicInfo.name,
            _default: BasicInfo.default,
            _privacy: BasicInfo.privacy
        };
        
        //
        $scope._listForm = [];
        $(BasicInfo.value).each(function (index, object) {
     
            if (BasicInfo.value[index].emergencyUserId == undefined)
            {
                BasicInfo.value[index].emergencyUserId = new Object();
                BasicInfo.value[index].emergencyUserId.label = 'Emergency UserId';
                BasicInfo.value[index].emergencyUserId.value = '';
                BasicInfo.value[index].emergencyUserId.controlType = '';

            }
               
            $scope._listForm.push({
                IsEdit: false,
                emergencyUserId: {
                    label: 'Emergency UserId',
                    value: BasicInfo.value[index].emergencyUserId.value = '',
                    controlType: BasicInfo.value[index].emergencyUserId.controlType = ''
                },

                privacy: {
                    label: BasicInfo.value[index].privacy.label,
                    value: BasicInfo.value[index].privacy.value,
                    controlType: BasicInfo.value[index].privacy.controlType
                },
                description: {
                    label: BasicInfo.value[index].description.label,
                    value: BasicInfo.value[index].description.value,
                    controlType: BasicInfo.value[index].description.controlType
                },

                firstName: {
                    label: BasicInfo.value[index].firstName.label,
                    value: BasicInfo.value[index].firstName.value,
                    controlType: BasicInfo.value[index].firstName.controlType
                },
                middleName: {
                    label: BasicInfo.value[index].middleName.label,
                    value: BasicInfo.value[index].middleName.value,
                    controlType: BasicInfo.value[index].middleName.controlType
                },
                lastName: {
                    label: BasicInfo.value[index].lastName.label,
                    value: BasicInfo.value[index].lastName.value,
                    controlType: BasicInfo.value[index].lastName.controlType
                },
                dob: {
                    label: BasicInfo.value[index].label,
                    value: BasicInfo.value[index].dob.value,
                    controlType: BasicInfo.value[index].dob.controlType
                },
                gender: {
                    label: BasicInfo.value[index].gender.label,
                    value: BasicInfo.value[index].gender.value,
                    controlType: BasicInfo.value[index].gender.controlType
                },
                email: {
                    label: BasicInfo.value[index].email.label,
                    value: BasicInfo.value[index].email.value,
                    controlType: BasicInfo.value[index].email.controlType
                },
                relationship: {
                    label: BasicInfo.value[index].relationship.label,
                    value: BasicInfo.value[index].relationship.value,
                    controlType: BasicInfo.value[index].relationship.controlType
                },

                note: {
                    label: BasicInfo.value[index].note.label,
                    value: BasicInfo.value[index].note.value,
                    controlType: BasicInfo.value[index].note.controlType
                }
            });
            if ($scope._listForm[index].dob.value != "" && $scope._listForm[index].dob.value != undefined)
                $scope._listForm[index].dob.value = new Date($scope._listForm[index].dob.value);
          
              
        });

        $scope._new = {
            'IsEdit': false,
            'privacy': { label: "Privacy", value: "" },
            'description': { label: "Description", value: "" },

            'firstName': { label: "First name", value: "" },
            'middleName': { label: "Middle name", value: "" },
            'lastName': { label: "Last name", value: "" },
            'dob': { label: "D.O.B", value: new Date('2000-01-01') },
            'gender': { label: "Gender", value: "" },
            'email': { label: "Email", value: "" },
            'relationship': { label: "Relationship", value: "" },
            'note': { label: "Note", value: "" },
           "document": [{id: "", name: "", description: ""}]
        }

        $scope.addNewForm = function (newForm) {
            $scope._listForm.push({
                'IsEdit': false,
                'privacy': { label: "Privacy", value: newForm.privacy.value },
                'description': { label: "Description", value: newForm.description.value },

                'firstName': { label: "First name", value: newForm.firstName.value },
                'middleName': { label: "Middle name", value: newForm.middleName.value },
                'lastName': { label: "Last name", value: newForm.lastName.value },
                'dob': { label: "D.O.B", value: newForm.dob.value },
                'gender': { label: "Gender", value: newForm.gender.value },
                'email': { label: "Email", value: newForm.email.value },
                'relationship': { label: "Relationship", value: newForm.relationship.value },
                'note': { label: "Note", value: newForm.note.value },

            });

        }
//
        $scope.removeForm = function (_value) {
            var index = $scope._listForm.indexOf(_value);
            swal({
                title: "Are you sure to remove " + _value.description.value + " ?",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes",
                cancelButtonText: "No"

            }).then(function () {
                $scope._listForm.splice(index, 1);
                $scope.Save();
                swal(
                    'Removed',
                   _value.description.value + ' has been removed.',
                    'success'
                  )
            }
            , function (dismiss) {
                if (dismiss === 'cancel') {

                }
            });

        };
        //
        $scope.messageBox = "";
        $scope.alerts = [];

    }

    $scope.$on('family', function () {
        $scope.InitData();
    });
    $scope.InitData();
    $scope.Edit = function () {
        $scope.IsEdit = true;
    }

    // == document == 
  
    $scope.Save = function () {

        //address
        BasicInfo.label = $scope._form._label;
        if ($scope._form._name == '') {
            $scope._form._name = $scope._form._label;
        }
        BasicInfo.name = $scope._form._name;
        BasicInfo.privacy = $scope._form._privacy;

        BasicInfo.default = $scope._form._default;

        //
        var lstFormSave = [];
        $($scope._listForm).each(function (index, object) {

            lstFormSave.push({
                privacy: {label: "Privacy", value: $scope._listForm[index].privacy.value},
                description: { label: "Description", value: $scope._listForm[index].description.value },

                firstName: { label: "First name", value: $scope._listForm[index].firstName.value },
                middleName: { label: "Middle name", value: $scope._listForm[index].middleName.value },
                lastName: { label: "Last name", value: $scope._listForm[index].lastName.value },

                dob: { label: "D.O.B", value: $scope._listForm[index].dob.value },
                gender: { label: "Gender", value: $scope._listForm[index].gender.value },
                email: { label: "Email", value: $scope._listForm[index].email.value },
                relationship: { label: "Relationship", value: $scope._listForm[index].relationship.value },
                note: { label: "Note", value: $scope._listForm[index].note.value },

            })
            $scope._listForm[index].IsEdit = false;
        });

        VaultInformationService.VaultInformation.family = BasicInfo;
        VaultInformationService.VaultInformation.family.value = lstFormSave;
        VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
            VaultInformationService.VaultInformation._id).then(function (response) {
                $scope.IsEdit = false;
                $scope._new = "";
                $rootScope.$broadcast('family');
             
            }, function (errors) {

            });
    }

    $scope.saveVaultInformationOnSuccess = function (response) {
        $scope.IsEdit = false;

    }
    $scope.saveVaultInformationOnError = function (response) {
        alertService.renderSuccessMessage(response.ReturnMessage);
        $scope.messageBox = alertService.returnFormattedMessage();
        $scope.alerts = alertService.returnAlerts();
    }

    $scope.Cancel = function () {
        $scope.InitData();
    }

    $scope.goBack = function () {
        $scope.$location.path('/VaultInformation');
    };
    $scope.closePanel = function () {
        $scope.Cancel();
    };

}])

//7.0 Pet 
myApp.getController('PetController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter) {

    "use strict";
    var BasicInfo = VaultInformationService.VaultInformation.pet;
    $scope.InitData = function () {
        $scope.IsEdit = false;

        $scope._form = {
            _label: BasicInfo.label,
            _name: BasicInfo.name,
            _default: BasicInfo.default,
            _privacy: BasicInfo.privacy
        };
        //
        $scope._listForm = [];

        $(BasicInfo.value).each(function (index, object) {

            $scope._listForm.push({
                IsEdit: false,
                privacy: {
                    label: "Privacy",
                    value: BasicInfo.value[index].privacy.value,
                    controlType: BasicInfo.value[index].privacy.controlType
                },
                description: {
                    label: "Description",
                    value: BasicInfo.value[index].description.value,
                    controlType: BasicInfo.value[index].description.controlType
                },

                name: {
                    label: "Name",
                    value: BasicInfo.value[index].name.value,
                    controlType: BasicInfo.value[index].name.controlType
                },
                type: {
                    label: "Type",
                    value: BasicInfo.value[index].type.value,
                    controlType: BasicInfo.value[index].type.controlType
                },
                breed: {
                    label: "Breed",
                    value: BasicInfo.value[index].breed.value,
                    controlType: BasicInfo.value[index].breed.controlType
                },
                dob: {
                    label: "Time of Birth",
                    value: BasicInfo.value[index].dob.value,
                    controlType: BasicInfo.value[index].dob.controlType
                },
                gender: {
                    label: "Gender",
                    value: BasicInfo.value[index].gender.value,
                    controlType: BasicInfo.value[index].gender.controlType
                },
              

                note: {
                    label: "Note",
                    value: BasicInfo.value[index].note.value,
                    controlType: BasicInfo.value[index].note.controlType
                }
            });
           

         
        });

        $scope._new = {
            'IsEdit': false,
            'privacy': { label: "Privacy", value: "" },
            'description': { label: "Description", value: "" },

            'name': { label: "Name", value: "" },
            'type': { label: "Type", value: "" },
            'breed': { label: "Breed", value: "" },
            'dob': { label: "Time of Birth", value: "" },
            'gender': { label: "Gender", value: "" },
          
            'note': { label: "Note", value: "" },
            "document": [{ id: "", name: "", description: "" }]
        }

        $scope.addNewForm = function (newForm) {
            $scope._listForm.push({
                'IsEdit': false,
                'privacy': { label: "Privacy", value: newForm.privacy.value },
                'description': { label: "Description", value: newForm.description.value },

                'name': { label: "Name", value: newForm.name.value },
                'type': { label: "Type", value: newForm.type.value },
                'breed': { label: "Breed", value: newForm.breed.value },
                'dob': { label: "D.O.B", value: newForm.dob.value },
                'gender': { label: "Gender", value: newForm.gender.value },
              
                'note': { label: "Note", value: newForm.note.value },

            });

        }

        $scope.removeForm = function (_value) {
            var index = $scope._listForm.indexOf(_value);
            swal({
                title: "Are you sure to remove " + _value.description.value + " ?",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes",
                cancelButtonText: "No"

            }).then(function () {
                $scope._listForm.splice(index, 1);
                $scope.Save();
                swal(
                    'Removed',
                    _value.description.value + ' has been Removed.',
                    'success'
                  )
            }
            , function (dismiss) {
                if (dismiss === 'cancel') {

                }
            });

        };
        //
        $scope.messageBox = "";
        $scope.alerts = [];

    }

    $scope.InitData();
    $scope.Edit = function () {
        $scope.IsEdit = true;
    }

    // == document == 

    $scope.Save = function () {

        //address
        BasicInfo.label = $scope._form._label;
        if ($scope._form._name == '') {
            $scope._form._name = $scope._form._label;
        }
        BasicInfo.name = $scope._form._name;
        BasicInfo.privacy = $scope._form._privacy;

        BasicInfo.default = $scope._form._default;

        //
        var lstFormSave = [];
        $($scope._listForm).each(function (index, object) {

            lstFormSave.push({
                privacy: { label: "Privacy", value: $scope._listForm[index].privacy.value },
                description: { label: "Description", value: $scope._listForm[index].description.value },

                name: { label: "Name", value: $scope._listForm[index].name.value },
                type: { label: "Type", value: $scope._listForm[index].type.value },
                breed: { label: "Breed", value: $scope._listForm[index].breed.value },

                dob: { label: "Time of Birth", value: $scope._listForm[index].dob.value },
                gender: { label: "Gender", value: $scope._listForm[index].gender.value },
               
                note: { label: "Note", value: $scope._listForm[index].note.value },

            })
            $scope._listForm[index].IsEdit = false;
        });

      
        VaultInformationService.VaultInformation.pet = BasicInfo;
        VaultInformationService.VaultInformation.pet.value = lstFormSave;
        VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
            VaultInformationService.VaultInformation._id).then(function (response) {
                $scope.IsEdit = false;
                $scope._new = "";
               
            }, function (errors) {

            });
    }

    $scope.saveVaultInformationOnSuccess = function (response) {
        $scope.IsEdit = false;

    }
    $scope.saveVaultInformationOnError = function (response) {
        alertService.renderSuccessMessage(response.ReturnMessage);
        $scope.messageBox = alertService.returnFormattedMessage();
        $scope.alerts = alertService.returnAlerts();
    }

    $scope.Cancel = function () {
        $scope.InitData();
    }

    $scope.goBack = function () {
        $scope.$location.path('/VaultInformation');
    };
    $scope.closePanel = function () {
        $scope.Cancel();
    };

}])
//7.0 Family Additions to Vault - EmergencyContact 
myApp.getController('EmergencyController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter', '$interval', 'NetworkService',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter,$interval, _networkService) {
    "use strict";

    $scope.InitData = function () {
        $scope._new = {
            'firstName': { label: "First name", value: "" },
            'lastName': { label: "Last name", value: "" },
            'email': { label: "Email", value: "" },
            'phone': { label: "Phone", value: "" },
            'relationship': { label: "Relationship", value: "" }
        }
        //
        $scope._invitedEmail = "";
        $scope.outNetwork = false;
       
        //Vu
        $scope.networks = [];
        $scope.emergencies = [];
        $scope.friendFullInfo = {};
        $scope._listForm = [];

      
     
        _networkService.GetNetworks().then(function (networks) {
            $scope.networks = networks;
            $scope.networks.forEach(function (network) {
          
                      if (network.Name == 'Trusted Network') {
                          _networkService.GetFriendsInNetwork(network.Id).then(function (rs) {
                            
                        $scope.emergencies = rs.Friends;
                        //test
                       
                        $($scope.emergencies).each(function (index, object) {
                            if ($scope.emergencies[index].IsEmergency == true)
                                {
                            var address = [];
                            var basic = new Object();
                            var mail = "";
                            var phone = "";
                            var addressDefault = "";
                            _networkService.GetVaultByUserId($scope.emergencies[index].UserId).then(function (bs) {
                                basic = bs.basicInformation;
                                phone = bs.contact.default;
                               
                                //
                                if (bs.contact.value.mobile.default != '')
                                    phone = bs.contact.value.mobile.default;
                                else
                                    phone = bs.contact.value.profilePhone.value;

                                if (bs.contact.value.email.default != '')
                                    mail = bs.contact.value.email.default;
                                else
                                    mail = bs.contact.value.profileEmail.value;

                                //
                                address = bs.groupAddress.value.currentAddress.value;
                                $(address).each(function (ix, object) {
                                    if (address[ix]._default == true)
                                        addressDefault = address[ix].addressLine;
                                });
                                if ($scope.emergencies[index].IsEmergency == true)
                                {
                                    $scope._listForm.push({
                                        IsEdit: false,

                                        FirstName: basic.value.firstName.value,
                                        MiddleName: basic.value.middleName.value,
                                        LastName: basic.value.lastName.value,
                                        Email: mail,
                                        Phone: phone,
                                        Address: addressDefault,

                                        Country: basic.value.country.value,
                                        City: basic.value.city.value,
                                        Gender: basic.value.gender.value,
                                        dob: basic.value.dob.value,

                                        DisplayName: $scope.emergencies[index].DisplayName,
                                        Relationship: { value: $scope.emergencies[index].Relationship },
                                        Id: $scope.emergencies[index].Id,
                                        UserId: $scope.emergencies[index].UserId,
                                        NetworkId: $scope.emergencies[index].NetworkId,
                                        IsEmergency: $scope.emergencies[index].IsEmergency,
                                        Rate: $scope.emergencies[index].Rate

                                    });

                                }
                               

                            });
                            }
                        });
                       
                      // checkss
                    });

                }
            })
        })
       //
    }
    $scope.$on('emergency', function () {
        $scope.InitData();
    });
    $scope.InitData();
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
            return _networkService.SearchUserForTrustEmergency(keyword, true).then(function (users) {
                return users;
            });
        }

        $scope.invitation = null;

        $scope.invite = function (invitation) {
            if (invitation && invitation.Id) {
                _networkService.InviteTrustEmergency({ ReceiverId: invitation.Id, Relationship: $scope._new.relationship.value, IsEmergency: true, Rate: 1 }).then(function () {
                    var message = $rootScope
                     .translate('A_Request_Has_Been_Sent_To_Message') + ' ' +
                     invitation.DisplayName;
                    swal('warning', message, 'success');
                    $scope.invitation = null;
                }, function (errors) {
                    swal('Error', 'Can not send invitation.', 'error');
                })
            } else {
                swal('warning', $rootScope.translate('Warning_Title'), 'error');
            }
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
                    _networkService.InviteEmergencyByEmail({ ToEmail: value, IsEmergency: true, Rate: 1, InviteId: "abcd1234" }).then(function () {

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
    //check emer    
        $scope.addNewError = '';
        $scope.CheckRelationship = '';

        $scope.CheckAddNew = function (invitation) {
            if (invitation == null) {
                swal('Error', 'Please enter a name', 'error');
            }
              else{    
            $scope.NoticeUpdate = '';
            $scope.addNewError = '';
            for (var i = 0; i < $scope.emergencies.length; i++) {
                if (invitation.Id == $scope.emergencies[i].Id && $scope.emergencies[i].IsEmergency == true) {
                    if ($scope.emergencies[i].Relationship == $scope._new.relationship.value)
                    {
                        $scope.addNewError = invitation.DisplayName + " is " + $scope.emergencies[i].Relationship + " in your emergercy contact already.";
                    } else
                    {
                        var value = {
                            Id: $scope.emergencies[i].Id,
                            NetworkId: $scope.emergencies[i].NetworkId,
                            DisplayName: $scope.emergencies[i].DisplayName,
                            UserId: $scope.emergencies[i].UserId,
                            Relationship: { value: $scope._new.relationship.value },
                           
                            IsEmergency: true,
                            Rate: 1
                        };
                        $scope.NoticeUpdate = "Update emergency of " + $scope.emergencies[i].DisplayName;

                        swal({
                            title: $scope.NoticeUpdate + " ?",
                            type: "warning",
                            showCancelButton: true,
                            confirmButtonColor: "#DD6B55",
                            confirmButtonText: "Yes",
                            cancelButtonText: "No"
                        }).then(function () {
                           
                            $scope.UpdateEmergency(value);
                            swal('OK', '', 'success');
                        }, function (dismiss) {
                             if (dismiss === 'cancel') {

                         }
                 });
                      
                    }

                    break;
                }

            }
            if ($scope.addNewError == '' && $scope.NoticeUpdate == '')
                $scope.invite(invitation);
            }
        }
    //End test friend, fromNetwork, idx

    //
        $scope.removeEmergency = function (value) {
            var idx = $scope._listForm.indexOf(value);
         
            swal({
                title: "Are you sure to remove " + value.DisplayName + " ?",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes",
                cancelButtonText: "No"

            }).then(function () {
                if (value.Id != null) {

                    _networkService.UpdateTrustEmergency({ Id: value.Id, NetworkId: value.NetworkId, DisplayName: value.DisplayName, UserId: value.UserId, Relationship: "", IsEmergency: false, Rate: value.Rate })
                        .then(function () {
                            $scope._listForm.splice(idx, 1);
                            swal(
                                'Removed!',
                                value.DisplayName + ' has been Removed.',
                                'success'
                                )
                        },
                        function (errors) {

                        });
                    $rootScope.$broadcast('emergency');
                }
                

            }
            ,function (dismiss) {
                if (dismiss === 'cancel') {

                }
            });

            setTimeout(function () {
                $('.popup-confirm-close .close-icon').click(function (e, v) {
                    $('.sa-button-container .cancel').trigger("click");
                });
            }, 100);
        }
    
        $scope.UpdateEmergency = function (value) {
           
            if (value.Id != null) {
               
                _networkService.UpdateTrustEmergency({ Id: value.Id, NetworkId: value.NetworkId, DisplayName: value.DisplayName, UserId: value.UserId, Relationship: value.Relationship.value, IsEmergency: value.IsEmergency, Rate: value.Rate })
                    .then(function () {
                        $rootScope.$broadcast('emergency');
                    });
            }
           
        }


}])