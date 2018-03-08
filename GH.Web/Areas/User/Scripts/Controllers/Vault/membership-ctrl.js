
var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.cellNav'], true);

//6.0 membership
myApp.getController('membershipController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter', 'DocumentVaultService', 'vaultService', 'NetworkService', 'rguNotify',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter, DocumentVaultService, vaultService, _networkService, rguNotify) {

    "use strict";
    var BasicInfo = VaultInformationService.VaultInformation.membership;
      var _document = VaultInformationService.VaultInformation.document;

    $scope.InitData = function () {
      // Begin push to vault

      $scope.result = {};
    //$scope.sendTo = {};
      $scope.sendPush = function () {
          console.log($scope.result);
          $scope.PushToVault();
      };
        $scope.vaultGroup = {
            name: 'membership',
            type: 'list',
            label: BasicInfo.label,
            model: BasicInfo.value
        };
        vaultService.populateGroup($scope.vaultGroup);
    //
        $scope.GetAllFriends = function () {
            _networkService.GetFriends().then(function (result) {
                $scope.network = result;
            })
        }
        $scope.GetAllFriends();

        $scope.PushToVault = function () {
            var data = new Object();
            var campaign = new Object();
            campaign.type = "ManualPushToVault";
            campaign.status = "Active";
            campaign.name = "Membership";
            campaign.description = $scope.result.sendTo.comment;
            if ($scope.result.sendTo.comment === '' || $scope.result.sendTo.comment === undefined)
                campaign.description = "Manual push to Vault";

            campaign.fields = [];
            data.UserId = $scope.result.sendTo.userId;
            data.CampaignType = "ManualPushToVault";
            data.Listvaults = [];
            data.pushtype = "PushToVault";
            if ($scope.result.sendTo.userId != null && $scope.result.sendTo.userId != undefined)
                data.usernotifycationid = $scope.result.sendTo.userId;

            if ($scope.result.sendTo.userEmail != "") {
                data.usernotifycationemail = $scope.result.sendTo.userEmail;
                data.pushtype = "email";
            }
            for (var index = 0; index < $scope.result.fields.length; index++) {
                if ($scope.result.fields[index].displayName == null && $scope.result.fields[index].displayName == undefined)
                    $scope.result.fields[index].displayName = "";
                if ($scope.result.fields[index].optional == null && $scope.result.fields[index].optional == undefined)
                    $scope.result.fields[index].optional = "";
                if ($scope.result.fields[index].type == null && $scope.result.fields[index].type == undefined)
                    $scope.result.fields[index].type = "";
                if ($scope.result.fields[index].unitModel == null && $scope.result.fields[index].unitModel == undefined)
                    $scope.result.fields[index].unitModel = "";
             

                if ($scope.result.fields[index].choices == null && $scope.result.fields[index].choices == undefined)
                    $scope.result.fields[index].choices = "";
                if ($scope.result.fields[index].qa == null && $scope.result.fields[index].qa == undefined)
                    $scope.result.fields[index].qa = "";
                var field = {
                    id: $scope.result.fields[index].id,
                    jsPath: '.' + $scope.result.fields[index].jsPath,
                    displayName: $scope.result.fields[index].label,
                    optional: $scope.result.fields[index].optional,
                    type: $scope.result.fields[index].type,
                    options: $scope.result.fields[index].options,
                    model: $scope.result.fields[index].value,
                    unitModel: $scope.result.fields[index].unitModel,
                    value: "",
                    membership: true,

                    choices: $scope.result.fields[index].choices + "",
                    qa: $scope.result.fields[index].qa + ""
                }
                //
                campaign.fields.push(field);
                //

                switch (field.type) {
                    case "location":
                        var model = $scope.result.fields[index].value;
                        field.model = model.country;
                        field.unitModel = model.city;
                        break;
                    case "address":
                        var model = $scope.result.fields[index].value;
                        field.model = model.address;
                        field.unitModel = model.address2;
                        break;

                }

                data.Listvaults.push(field);


            };
            var StrVaultInformation = JSON.stringify(campaign);
            data.StrVaultInformation = StrVaultInformation;
            $http.post('/api/CampaignService/ManualPushVault', data)
                .success(function (response) {
                    if (response.ReturnStatus == false) {
                        swal(
                            'Push Vault Send Error!',
                            'Email not found. Push form must be sent to a valid Regit user.',
                             'error'
                          );
                        return;
                    }
                   // $uibModalInstance.close(response);
                    rguNotify.add('Push to Vault completed.');
                })
                .error(function (errors) {
                    var errorObj = __errorHandler.ProcessErrors(errors);
                    __errorHandler.Swal(errorObj, _sweetAlert);
                });

        }

    // End push to vault
        //

        $scope.IsEdit = false;
        // General   
        $scope._form = {
            _label: BasicInfo.label,
            _name: BasicInfo.name,
            _default: BasicInfo.default,
            _privacy: BasicInfo.privacy
        };

        $scope._listForm = [];
        var maxindex = 0;

        $(BasicInfo.value).each(function (index, object) {
            if (maxindex < BasicInfo.value[index]._id)
                maxindex = BasicInfo.value[index]._id;
            var _jspath = "/Membership/" + BasicInfo.value[index]._id;
            var today = new Date();
            var expiryNo = false;
            // indefinite
            var _indefiniteDate = true;
            if (BasicInfo.value[index].expiryDate == undefined)
                BasicInfo.value[index].expiryDate = "";
            if (BasicInfo.value[index].expiryDate != "")
                _indefiniteDate = false;
            // End indefinite
          
            $scope._listForm.push({
                _id: BasicInfo.value[index]._id,
                IsEdit: false,
                privacy: BasicInfo.value[index].privacy,
                description: BasicInfo.value[index].description,
                businessName: BasicInfo.value[index].businessName,
                membershipProgramName: BasicInfo.value[index].membershipProgramName,
                holder: BasicInfo.value[index].holder,
                membershipClass: BasicInfo.value[index].membershipClass,
                membershipNumber: BasicInfo.value[index].membershipNumber,
                expiryDate: BasicInfo.value[index].expiryDate,
                indefiniteDate: _indefiniteDate,
                expiryNo: expiryNo,
                note: BasicInfo.value[index].note,
                loginId: BasicInfo.value[index].loginId,
                password: BasicInfo.value[index].password,
                loginSite: BasicInfo.value[index].loginSite,
                serviceProvider: BasicInfo.value[index].serviceProvider,
                noteOnline: BasicInfo.value[index].noteOnline,
                docJspath: _jspath

            })


          if ($scope._listForm[index].expiryDate != "" && $scope._listForm[index].expiryDate != undefined)
            {
              
                if ($scope._listForm[index].expiryDate < today)
                    $scope._listForm[index].expiryNo = true;
            }
               
           
        });

        maxindex = maxindex + 1;
        $rootScope._newJspath = "/Membership/" + maxindex;
        $scope._new =
           {
               '_id': maxindex,
               'IsEdit': false,
               'privacy': "",
               'description': "",

               'businessName': "",
               'membershipProgramName': "",
               'lastName': "",
               'holder': "",
               'membershipClass': "",
               'membershipNumber': "",
               'expiryDate': "",
               'indefiniteDate': true,
               'note': "",

               'loginId': "",
               'password': "",
               'loginSite': "",
               'serviceProvider': "",
               'noteOnline': "",
           };

        $scope.addNewForm = function (newForm) {
            var jpd = "/Government ID/Membership/" + newForm._id;
            if (newForm.indefiniteDate == true)
                newForm.expiryDate = '';

            $scope._listForm.push({
                '_id': newForm._id,
                'IsEdit': false,
                'privacy': newForm.privacy,
                'description': newForm.description,
                'businessName': newForm.businessName,
                'membershipProgramName': newForm.membershipProgramName,
                'holder': newForm.holder,
                'membershipClass': newForm.membershipClass,
                'membershipNumber': newForm.membershipNumber,
                'expiryDate': newForm.expiryDate,
                'indefiniteDate': newForm.indefiniteDate,
                'docJspath': jpd,
                'note': newForm.note,
                'loginId': newForm.loginId,
                'password': newForm.password,
                'loginSite': newForm.loginSite,
                'serviceProvider': newForm.serviceProvider,
                'noteOnline': newForm.noteOnline,
            });


        };

        $scope.removeForm = function (_value) {
            var index = $scope._listForm.indexOf(_value);
            swal({
                title: "Are you sure to remove " + _value.description + " ?",
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
                     _value.description + ' has been removed.',
                    'success'
                  );
            }
            , function (dismiss) {
                if (dismiss === 'cancel') {

                }
            });

        };

        // Document
        $scope._listdocument = [];
        $(_document.value).each(function (index, object) {

            $scope._listdocument.push({
                _id: _document.value[index]._id,
                IsEdit: false,
                privacy: _document.value[index].privacy,
                name: _document.value[index].name,
                saveName: _document.value[index].saveName,
                category: _document.value[index].category,
                description: _document.value[index].description,
                type: _document.value[index].type,
                uploadDate: _document.value[index].uploadDate,
                jsPath: _document.value[index].jsPath,
                nosearch: _document.value[index].nosearch
            });
        });


        $scope.checkRemoveDoc = false;
        $scope.removeFormDoc = function (_value) {
            $scope.checkRemoveDoc = true;
            var _jsp = "/Membership/" + _value._id;
            var model = new Object();
            model.UserID = $rootScope.ruserid;

            $($scope._listdocument).each(function (index, object) {
                if (_jsp == $scope._listdocument[index].jsPath) {
                    model.FileName = $scope._listdocument[index].name;
                    DocumentVaultService.DeleteFile(model).then(function () {
                        $scope._listdocument.splice(index, 1);
                    }
                    , function (errors) {

                    });
                }
            });
        };
        // end Document
    }
    
    $scope.$on('membership', function () {
        $scope.InitData();
    });
    $scope.InitData();


    $scope.Edit = function () {
        $scope.IsEdit = true;
    }

    $scope.Save = function () {

        //membership
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
            // indefinite
            if ($scope._listForm[index].indefiniteDate == true)
                $scope._listForm[index].expiryDate = "";
            // end indefinite
            lstFormSave.push({
                _id: $scope._listForm[index]._id,
                privacy: $scope._listForm[index].privacy,
                description: $scope._listForm[index].description,
                businessName: $scope._listForm[index].businessName,
                membershipProgramName: $scope._listForm[index].membershipProgramName,
                holder: $scope._listForm[index].holder,
                membershipClass: $scope._listForm[index].membershipClass,
                membershipNumber: $scope._listForm[index].membershipNumber,
                expiryDate: $scope._listForm[index].expiryDate,

                note: $scope._listForm[index].note,
                loginId: $scope._listForm[index].loginId,
                password: $scope._listForm[index].password,
                loginSite: $scope._listForm[index].loginSite,
                serviceProvider: $scope._listForm[index].serviceProvider,
                noteOnline: $scope._listForm[index].noteOnline
            })
            $scope._listForm[index].IsEdit = false;
        });

        //
         //Document 

        var lstDocSave = [];
        $($scope._listdocument).each(function (index, object) {

            lstDocSave.push({
                _id: $scope._listdocument[index]._id,
                privacy: $scope._listdocument[index].privacy,
                name: $scope._listdocument[index].name,
                SaveName: $scope._listdocument[index].SaveName,
                description: $scope._listdocument[index].description,
                type: $scope._listdocument[index].type,
                category: $scope._listdocument[index].category,
                uploadDate: $scope._listdocument[index].uploadDate,

                jsPath: $scope._listdocument[index].jsPath,
                nosearch: $scope._listdocument[index].nosearch,

            })
            $scope._listdocument[index].IsEdit = false;
        });

        //End document
        //save
        VaultInformationService.VaultInformation.membership = BasicInfo;
        VaultInformationService.VaultInformation.membership.value = lstFormSave;
          if ($scope.checkRemoveDoc == true) {
            VaultInformationService.VaultInformation.document = _document;
            VaultInformationService.VaultInformation.document.value = lstDocSave;
        }
        VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
            VaultInformationService.VaultInformation._id).then(function (response) {
                $scope.IsEdit = false;
                $scope._new = '';
                $rootScope.$broadcast('membership');
             
                $scope.checkRemoveDoc == false;
                $rootScope._newJspath = "";
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
