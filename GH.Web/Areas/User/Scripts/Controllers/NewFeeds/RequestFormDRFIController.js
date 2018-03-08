var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule', 'interaction'], true);

myApp.controller('RequestFormDRFIController',
    ['$scope', '$rootScope', '$uibModalInstance', '$uibModal', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService',
        'NotificationService', 'formService', 'registerPopup', 'SmSAuthencationService', 'fileUpload', 'interactionFormService','msgService','$moment',

        function ($scope, $rootScope, $uibModalInstance, $uibModal, $http, userManager, sweetAlert, authService, alertService, 
        notificationService, formService, registerPopup, SmSAuthencationService, fileUpload, interactionFormService, msgService, $moment) {
            $scope.messageBox = "";
            $scope.alerts = [];
            $scope.dataregisterform = registerPopup.ListOfFields;
            $scope.userid = registerPopup.userid;
            $scope.Iscustomrole = false;
            $scope.Mode = "Read";
            $scope.IsReadOnly = false;
            $scope.Isregis = true;
            $scope.drfiMessage = registerPopup.drfiMessage;

            var delegation = new Object();
            $scope.missingInfomationOfVault = false;
            delegation.FromUserDisplayName = "Myself";
            $scope.ListManagerDelegatedTo = [delegation];
            $scope.DelegationSelected = delegation;


            interactionFormService.initFieldGroups($scope.dataregisterform);
            $scope.renderFieldGroup = interactionFormService.renderFieldGroup;

            $scope.cancel = function () {
                $uibModalInstance.dismiss('cancel');
            };
            $scope.canRegister = function () {
                return formService.validateAllFields($scope.dataregisterform);
            };

            $scope.Edit = function () {
                SmSAuthencationService.OpenPopupAuthencationPIN(function () {
                    $scope.Mode = "Edit";
                });
            }
            $scope.checkterm = function (term) {
                if (term == null)
                    return false;
                if (term.search(".") >= 0)
                    return true;
                else return false;
            }
           
            $scope.uploadFile = function (element) {
                var file = element.files[0];
                var uploadUrl = "/api/CampaignService/UploadFileRegisform";
                fileUpload.uploadFileToUrl(file, uploadUrl, function (reponse) {
                    var jsonpath = $(element).attr("jsonpath");
                    $($scope.dataregisterform).each(function (index, field) {
                        if (field.jsPath == jsonpath) {
                            field.model.push({fname: reponse.fileName});
                        }
                    });
                    var input = $(element);
                    input.replaceWith(input.val('').clone(true));
                }, function () {

                }, "");
            };
             $scope.uploadFileReg = function (element) {
                var file = element.files[0];

                var jsPath = element.id;
                var uploadUrl = "/api/CampaignService/UploadFileRegisform?jsPath=" + jsPath;
                fileUpload.uploadFileToUrl(file, uploadUrl, function (reponse) {
                    var jsonpath = $(element).attr("jsonpath");
                    $($scope.dataregisterform).each(function (index, field) {
                        if (field.jsPath == jsonpath) {
                            field.model.push({ fname: reponse.fileName });
                        }
                    });
                    var input = $(element);
                    input.replaceWith(input.val('').clone(true));

                }, function () {

                }, "");
            };
            $scope.changedocument = function (path) {
                $($scope.dataregisterform).each(function (index, field) {
                    if (field.jsPath == path) {
                        field.modeldocvault = [];
                        field.modeldocvault.push({fname: field.docselect.fname});
                    }
                });
            }
            $scope.uploadFileDocQA = function (element) {
                var file = element.files[0];
                var uploadUrl = "/api/CampaignService/UploadFileRegisform";
                fileUpload.uploadFileToUrl(file, uploadUrl, function (reponse) {
                    var path = $(element).attr("path");
                    $($scope.dataregisterform).each(function (index, field) {
                        if (field.jsPath == path) {
                            field.modeldocvault = [];
                            field.modeldocvault.push({fname: reponse.fileName});
                        }
                    });
                    var input = $(element);
                    input.replaceWith(input.val('').clone(true));
                }, function () {
                }, "");
            };
            $scope.deleteDoc = function (model, index) {
                model.splice(index, 1);
            };

            $scope.restructFormFields = function () {
                $($scope.dataregisterform).each(function (index, field) {
                    switch ($scope.dataregisterform[index].type) {
                        case "doc":
                            if ($scope.dataregisterform[index].qa) {
                                $scope.dataregisterform[index].modeldocvault = [];
                            }
                            break;
                    }
                })
            }

            $scope.restructFormFields();
            $scope.Deny = function () {
                $scope.drfiMessage.type = "drfiresponsedenied";
                $scope.drfiMessage.drfiRequestId = $scope.drfiMessage.messageid;
                $scope.drfiMessage.jsonFieldsdrfi ="";
                msgService.addMessage($scope.drfiMessage.conversationId, $scope.drfiMessage, function () {
                });
                $uibModalInstance.close(new Object());
            }
            $scope.Register = function () {

                if ($scope.Mode === "Edit") {
                    $scope.UpdateVaultInformation();
                }
                else {
                    $scope.RegisterCampaign();
                }
            }
            $scope.vaildfiled = function (field) {
                if (field.type === 'date' && field.options === 'indef') return true;
                var isvalid = true;
                if (field.model == "" || field.model == undefined || field.model == null) {
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
                        if (field.model + "" == "Invalid Date")
                            isvalid = false;
                        break;
                }
                return isvalid;
            }
            $scope.RegisterCampaign = function () {
                var data = new Object();
                data.UserId ="";
                data.Listvaults = [];
                var isvalid = true;
                $($scope.dataregisterform).each(function (index) {
                    if (!$scope.dataregisterform[index].optional || $scope.dataregisterform[index].selected) {
                        var field = {
                            id: $scope.dataregisterform[index].id,
                            jsPath: $scope.dataregisterform[index].jsPath,
                            displayName: $scope.dataregisterform[index].displayName,
                            optional: $scope.dataregisterform[index].optional,
                            type: $scope.dataregisterform[index].type,
                            options: $scope.dataregisterform[index].options,
                            model: $scope.dataregisterform[index].model,
                            unitModel: $scope.dataregisterform[index].unitModel,
                            value: $scope.dataregisterform[index].value,
                            membership: $scope.dataregisterform[index].membership + "",
                            modelarrays: $scope.dataregisterform[index].modelarrays,
                            choices: $scope.dataregisterform[index].choices + "",
                            qa: $scope.dataregisterform[index].qa + ""
                        }
                        if (isvalid)
                            isvalid = $scope.vaildfiled($scope.dataregisterform[index]);
                        if (!isvalid)
                            return;

                        var allowadd = true;
                        switch (field.type) {
                            case "location":
                                var model = $scope.dataregisterform[index].model;
                                field.model = model.country;
                                field.unitModel = model.city;
                                break
                            case "address":
                                var model = $scope.dataregisterform[index].model;
                                field.model = model.address;
                                field.unitModel = model.address2;
                                break;
                            case "range":
                                var model = $scope.dataregisterform[index].model;
                                if (model == -1 || model == "-1")
                                    allowadd = false;
                                var arrays = [];
                                $($scope.dataregisterform[index].modelarrays).each(function (index, object) {
                                    var array = [];
                                    array.push(object[0]);
                                    array.push(object[1]);
                                    arrays.push(array);

                                });
                                field.modelarrays = arrays;
                                break;
                            case "qa":
                                if ($scope.dataregisterform[index].choices) {
                                    var arrays = [];
                                    $($scope.dataregisterform[index].modelarrays).each(function (index, object) {
                                        arrays.push({value: object.value});
                                    });
                                    field.modelarrays = arrays;
                                    field.model = $scope.dataregisterform[index].model.value;
                                }
                                else
                                    field.model = $scope.dataregisterform[index].model;
                                break;
                            case "doc":
                                var arrays = [];
                                     $(field.model).each(function (index, object) {
                                            arrays.push(object.fname);
                                        });
                                     field.modelarrays = arrays;
                                    field.pathfile=$scope.dataregisterform[index].pathfile;
                                field.model = "";
                                break;
                            case "history":
                                allowadd = false;
                                break;
                            case "datecombo":
                            case "date":
                                allowadd = true;
                                field.model = $moment(field.model).format('YYYY-MM-DD');
                                break;
                        }
                        if (allowadd == true)
                            data.Listvaults.push(field);
                    }
                });
                if (!isvalid)
                    return;
                $scope.drfiMessage.type = "drfiresponseaccepted";
                $scope.drfiMessage.drfiRequestId = $scope.drfiMessage.messageid;
                $scope.drfiMessage.jsonFieldsdrfi= JSON.stringify(data.Listvaults);
                msgService.addMessage($scope.drfiMessage.conversationId, $scope.drfiMessage, function () {
                 });
                $uibModalInstance.close(data.Listvaults);
            }

            $scope.UpdateVaultInformation = function () {
                var data = new Object();
                data.UserId = "";
                data.Listvaults = [];
                var isvalid = true;
                $($scope.dataregisterform).each(function (index) {
                    if (!$scope.dataregisterform[index].optional || $scope.dataregisterform[index].selected) {
                        var field = {
                            id: $scope.dataregisterform[index].id,
                            jsPath: $scope.dataregisterform[index].jsPath,
                            displayName: $scope.dataregisterform[index].displayName,
                            optional: $scope.dataregisterform[index].optional,
                            type: $scope.dataregisterform[index].type,
                            options: $scope.dataregisterform[index].options,
                            model: $scope.dataregisterform[index].model,
                            unitModel: $scope.dataregisterform[index].unitModel,
                            value: $scope.dataregisterform[index].value,
                            membership: $scope.dataregisterform[index].membership + "",
                            modelarrays: $scope.dataregisterform[index].modelarrays,
                            choices: $scope.dataregisterform[index].choices + "",
                            qa: $scope.dataregisterform[index].qa + ""
                        }

                        if (isvalid)
                            isvalid = $scope.vaildfiled($scope.dataregisterform[index]);
                        if (!isvalid)
                            return;
                        var allowadd = true;
                        switch (field.type) {
                            case "location":
                                var model = $scope.dataregisterform[index].model;
                                field.model = model.country;
                                field.unitModel = model.city;
                                break
                            case "address":
                                var model = $scope.dataregisterform[index].model;
                                field.model = model.address;
                                field.unitModel = model.address2;
                                break
                            case "range":
                                var model = $scope.dataregisterform[index].model;
                                if (model == -1 || model == "-1")
                                    allowadd = false;
                                var arrays = [];
                                $($scope.dataregisterform[index].modelarrays).each(function (index, object) {
                                    var array = [];
                                    array.push(object[0]);
                                    array.push(object[1]);
                                    arrays.push(array);

                                });
                                field.modelarrays = arrays;
                                break;
                            case "qa":
                                var arrays = [];
                                $($scope.dataregisterform[index].modelarrays).each(function (index, object) {

                                    arrays.push({value: object.value});
                                });
                                field.modelarrays = arrays;
                                field.model = $scope.dataregisterform[index].model.value;
                                break;
                            case "doc":
                                var arrays = [];
                                if (!$scope.dataregisterform[index].qa) {
                                    $($scope.dataregisterform[index].model).each(function (index, object) {

                                        arrays.push(object.fname);
                                    });
                                    field.modelarrays = arrays;
                                }
                                else {
                                    $($scope.dataregisterform[index].modeldocvault).each(function (index, object) {

                                        arrays.push(object.fname);
                                    });
                                    field.modelarrays = arrays;

                                }
                                field.model = "";
                                break;
                            case "history":
                                allowadd = false;
                                break;
                        }
                        if (allowadd == true)
                            data.Listvaults.push(field);
                    }
                });
                if (!isvalid)
                    return;
                $http.post('/api/InformationVaultService/UpdateInformationVaultById', data)
                    .success(function (response) {
                        $scope.RegisterCampaign();
                    })
                    .error(function (errors, status) {
                        $uibModalInstance.dismiss('cancel');
                    });
            }

            $scope.filterRequired = function (field) {
                return !field.optional && !field.membership && !field.qa;
            };
            $scope.filterOptional = function (field) {
                return !!field.optional;
            };
            $scope.filterMembership = function (field) {
                return !!field.membership;
            };
            $scope.filterQA = function (field) {
                return !!field.qa;
            }

            $scope.changedelegate = function () {
                var data = new Object();
                $scope.Mode = "Read";
                data.UserId = $scope.DelegationSelected.FromAccountId;
                data.BusinessUserId = $scope.BusinessUserId;
                data.CampaignType = $scope.CampaignType;
                data.CampaignId = $scope.CampaignId;
                $scope.Isregis = true;
                var isvalid = true;

                if ($scope.DelegationSelected.DelegationRole == "Normal" &&
                    $scope.DelegationSelected.FromUserDisplayName !== "Myself") {
                    $scope.IsReadOnly = true;
                    $scope.missingInfomationOfVault = false;
                    alertService.closeAlert();
                }
                else {
                    $scope.IsReadOnly = false;
                    $scope.missingInfomationOfVault = false;
                    alertService.closeAlert();
                }
                if ($scope.DelegationSelected.DelegationRole == "Custom") {
                    $scope.Iscustomrole = true;
                }
                else {
                    $scope.Iscustomrole = false;
                }

                $http.post('/api/CampaignService/GetUserInformationForCampaign', data)
                    .success(function (response) {
                        $scope.dataregisterform = response.ListOfFields;
                        $($scope.dataregisterform).each(function (index) {
                            if ($scope.Iscustomrole) {
                                $($scope.DelegationSelected.GroupVaultsPermission).each(function (index1, permission) {
                                    var jsPath = $scope.dataregisterform[index].jsPath;
                                    if (jsPath.search("." + permission.jsonpath) >= 0) {
                                        $scope.dataregisterform[index].read = permission.read;
                                        $scope.dataregisterform[index].write = permission.write;
                                        if (!$scope.dataregisterform[index].write && $scope.dataregisterform[index].read) {
                                            $scope.IsReadOnly = true;
                                            if (!$scope.vaildfiled($scope.dataregisterform[index])) {
                                                $scope.Isregis = false;
                                            }
                                        }
                                        if (!$scope.dataregisterform[index].write && !$scope.dataregisterform[index].read) {
                                            $scope.Isregis = false;
                                            $scope.IsReadOnly = true;
                                        }
                                    }
                                })
                            }

                            if ($scope.DelegationSelected.DelegationRole == "Normal" &&
                                $scope.DelegationSelected.FromUserDisplayName !== "Myself") {
                                if (isvalid) {
                                    isvalid = $scope.vaildfiled($scope.dataregisterform[index]);
                                    $scope.Isregis = isvalid;
                                }
                            }

                            $scope.dataregisterform[index].selected = true;
                            $scope.dataregisterform[index].membership = $scope.dataregisterform[index].membership == "true" ? true : false;
                            switch ($scope.dataregisterform[index].type) {
                                case "date":
                                case "datecombo":
                                    $scope.dataregisterform[index].model = new Date($scope.dataregisterform[index].model);
                                    break;
                                case "location":
                                    var mode = {
                                        country: $scope.dataregisterform[index].model,
                                        city: $scope.dataregisterform[index].unitModel
                                    }
                                    $scope.dataregisterform[index].model = mode;
                                    break;
                                case "address":
                                    var mode = {
                                        address: $scope.dataregisterform[index].model,
                                        address2: $scope.dataregisterform[index].unitModel
                                    }
                                    $scope.dataregisterform[index].model = mode;
                                    break;
                                case "doc":
                                    $scope.dataregisterform[index].model = [];
                                    var m = [];
                                    $($scope.dataregisterform[index].modelarrays).each(function (index, fname) {
                                        m.push({
                                            fname: fname
                                        })
                                    })
                                    $scope.dataregisterform[index].model = m;
                                    break;
                            }
                        });
                        $scope.restructFormFields();
                    })
                    .error(function (errors, status) {
                    });
            };

            $scope.notifyMissingInformationOfVault = function () {
                var campaignid = $scope.CampaignId;
                var delegatorId = $scope.DelegationSelected.FromAccountId;
                $http.get('/api/Notifications/NotifyMissingInformationOfVault?campaignId=' + campaignid + '&delegatorId=' + delegatorId)
                    .success(function (response) {
                        $uibModalInstance.close();
                    })
                    .error(function (errors, status) {
                    });
            }

            $scope.getListDelegatedTo = function () {

                var delegationModelView = new Object();
                delegationModelView.Direction = "DelegationIn";
                $http.post('/api/DelegationManager/GetListDelegation', delegationModelView)
                    .success(function (response) {
                        $(response.Listitems).each(function (index, delegate) {
                            if (delegate.Status === "Accepted") {
                                $scope.ListManagerDelegatedTo.push(delegate);
                            }
                        });
                    })
                    .error(function (errors, status) {
                    });
            };
            $scope.getListDelegatedTo();
        }
    ]);

myApp.getController('datetimecontroller', ['$scope', function ($scope) {
            var am = this;
            am.dateformat = 'yyyy-MM-dd';
            this.datetime = new Date();
            this.openeddatetime = false;
            this.opendatetime = function () {
                this.openeddatetime = true;
            };
            this.InitData = function (date) {
                am.datetime = new Date(date);
            };

        }]);
