angular.module('campaigns', ['angular-momentjs'])
    .constant('campaignTypeNames', ['Advertising', 'Registration', 'Event', 'SRFI', 'PushToVault', 'Handshake'])
    .constant('campaignTypeAbbrs', ['Ad', 'Reg', 'Evt', 'SRFI', 'P', 'HS'])
    .constant('campaignStatusNames', ['Pending', 'Active', 'Inactive', 'Expired'])

    .factory('formService', function ($http, rguCache, rguModal) {
        function _parseValue(value) {
            if (angular.isUndefined(value))
                return ''; //<span class="vault-value-meta">(No value)</span>';
            if (angular.isArray(value))
                return value.join(', ');
            var parsedDate = rgu.parseDate(value);
            if (parsedDate) {
                // console.log(value);
                var datestr = $moment(parsedDate.date).format('D MMMM YYYY');
                if (datestr === 'Invalid date') {
                    datestr = '';
                }
                return datestr;
            }
            if (angular.isObject(value)) {
                if (value.hasOwnProperty('country') && value.hasOwnProperty('city')) {
                    var country = value.country || '', city = value.city || '';
                    var text = city;
                    if (country && city) {
                        text += ', ';
                    }
                    text += country;
                    return text;
                }
                if (value.hasOwnProperty('address')) {
                    return value.address.hasOwnProperty('address') ? value.address.address : value.address;
                }
                // return '<span class="vault-value-meta">(Data)</span>'
                return '***';
            }

            return value.toString();
        }

        function _fieldNoValue(field) {
            if (field.type === 'static') return false;

            if (field.hasOwnProperty('optional') && field.optional && !field.selected)
                return false;
            var type = field.type;
            var model = field.model;
            var value = field.model;
            var options = field.options;
            if (angular.isUndefined(model) || model === null && field.options !== 'indef') {
                return true;
            }
            if (angular.isString(model) && !model.length) {
                return true;
            }
            switch (type) {

                case "doc":
                    if (angular.isArray(value))
                        return !value.length || !value.some(function (d) {
                            return d.selected;
                        });
                    return true;
                case "address":
                    return !value.length;
                case "location":
                    if (!angular.isString(value.country)) return true;
                    return !value.country.length;
                case "date":
                case 'datecombo':
                    return !model || model === "Invalid Date" || model === 'Invalid date';
                case 'numinput':
                    if (!angular.isString(value.amount)) return true;
                    return !value.amount.length;
                case 'qa':
                    return !angular.isString(model) || !model.length;
                case 'range':
                    return !angular.isString(model) || !model.length || model === '-1';

            }
            if (angular.isArray(model))
                return !model.length;

            return false;
        }

        function _fieldHasNoValue(field) {
            if (field.type === 'static') return false;
            if (field.hasOwnProperty('optional') && field.optional && !field.selected)
                return false;
            var type = field.type;
            var model = field.model;
            var options = field.options;
            if (angular.isUndefined(model) || model === null && field.options !== 'indef') {
                return true;
            }
            if (angular.isString(model) && !model.length) {
                return true;
            }
            switch (type) {

                case "doc":
                    if (angular.isArray(field.model))
                        return !field.model.length || !field.model.some(function (d) {
                            return d.selected;
                        });
                    return true;
                case "address":
                    return angular.isArray(field.model.address) && !field.model.address.length;
                    break;
                case "location":

                    if (!angular.isString(model.country)) return true;
                    return !model.country.length;
                    break;
                case "date":
                case 'datecombo':
                    return !model || model === "Invalid Date" || model === 'Invalid date';
                    break;
                case 'numinput':
                    return !model.toString().length;
                    break;
                case 'qa':
                    if (!field.choices) return !angular.isString(model) || !model.length;
                    if (angular.isString(model.value)) return !model.value.length;
                    return !angular.isString(model) || !model.length;
                    break;
                case 'range':
                    return !angular.isString(model) || !model.length || model === '-1';
                    break;

            }
            if (angular.isArray(model)) {
                return !model.length;
            }

            return false;
        }

        function _validateField(field) {

            if (field.type === 'static') return true;

            if (field.hasOwnProperty('optional') && field.optional && !field.selected)
                return true;
            var type = field.type;
            var model = field.model;
            var options = field.options;
            if (angular.isUndefined(model) || model === null && field.options !== 'indef') {
                return false;
            }
            if (angular.isString(model) && !model.length) {
                return false;
            }
            if (field.options === 'phone') {
                if (angular.isObject(field.validate) && field.validate.invalid) {
                    return false;
                }
            }
            switch (type) {
                case "doc":
                    if (angular.isArray(field.model))
                        return field.model.length && field.model.some(function (d) {
                            return d.selected;
                        });
                    return false;

                case "address":
                    return !!field.model.length;
                case "location":
                    if (!angular.isString(model.country)) return false;
                    return !(!model.country.length);
                case "date":
                case 'datecombo':
                    // var parsedDate = rgu.parseDate(model);
                    // if (parsedDate) {
                    //     var datestr = $moment(parsedDate.date).format('D MMMM YYYY');
                    //     if (datestr === 'Invalid date' || datestr === 'Invalid Date') {
                    //         if (field.options === 'indef') {
                    //             return 'Indefinite';
                    //         }
                    //         datestr = '';
                    //
                    //     }
                    //     return datestr;
                    // } else {
                    //     return '';
                    // }
                    return !(!model || model === "Invalid Date" || model === 'Invalid date');
                case 'numinput':
                    return !!model.toString().length;
                case 'qa':
                    return angular.isString(model) && model.length;

                case 'range':
                    return angular.isString(model) && model.length;

            }
            if (angular.isArray(model)) {
                return !!model.length;
            }

            return !!model.length;
        }

        return {
            parseValue: _parseValue,

            fieldNoValue: _fieldNoValue,
            validateField: _validateField,
            validateAllFields: function (fields) {
                if (!angular.isArray(fields)) return false;
                return !fields.some(function (field) {
                    return !_validateField(field);
                });
            },
            fieldsNoValue: function (fields) {
                if (!angular.isArray(fields)) return true;
                return fields.some(function (field) {
                    return _fieldNoValue(field);
                });
            },
            missingReadonlyFields: function (fields) {
                if (!angular.isArray(fields)) return true;
                return fields.some(function (field) {
                    return field.type !== 'qa' && field.type !== 'range' && field.permission !== 'w' && _fieldNoValue(field);
                });
            },

            interactionVerbs: ['register', 'join', 'participate', 'submit', 'send', 'enroll', 'apply', 'accept'],

            verbPast: function (verb) {
                switch (verb) {
                    case 'send':
                        return 'sent';
                    case 'apply':
                        return 'applied';
                    case 'submit':
                        return 'submitted';
                    case 'participate':
                        return 'participated';
                }
                return verb + 'ed';
            },
            verbGerund: function (verb) {
                switch (verb) {
                    case 'participate':
                        return 'participating';
                    case 'submit':
                        return 'submitting';
                }
                return verb + 'ing';
            },
            openInteractionForm: function (interaction, scope) {
                var campaign = {};
                var model = {
                    // UserId: regitGlobal.userAccount.id,
                    BusinessUserId: interaction.business.accountId,
                    CampaignId: interaction.id,
                    CampaignType: interaction.type,
                    campaign: campaign
                };
                if (!angular.isString(interaction.business.name)) {
                    rguCache.getUserAsync(interaction.business.accountId).then(function (response) {
                        interaction.business.name = response.data.displayName;
                        interaction.business.avatar = response.data.user.avatar;
                    })
                }
                scope.titleSuffix = interaction.type === 'srfi' ? 'Submission Form' : 'Registration Form';

                if (!interaction.editing) {

                    scope.campaign = campaign;
                }
                rguModal.openModal('interaction.form', scope, null);

                /*                $http.post("/api/CampaignService/GetUserInformationForCampaign", model)
                                    .success(function (response) {
                                        dataresult = response;
                                        var fields = scope.fieldList = response.ListOfFields;
                                        var campaignFields = scope.interactionFields = response.Campaign.Fields;

                                        $(response.ListOfFields).each(function (index) {
                                            response.ListOfFields[index].membership = response.ListOfFields[index].membership == "true" ? true : false;
                                            response.ListOfFields[index].selected = true;
                                            switch (response.ListOfFields[index].type) {
                                                case "date":
                                                case "datecombo":
                                                    response.ListOfFields[index].model = new Date(response.ListOfFields[index].model);
                                                    break;
                                                case "location":
                                                    var mode = {
                                                        country: response.ListOfFields[index].model,
                                                        city: response.ListOfFields[index].unitModel
                                                    }
                                                    response.ListOfFields[index].model = mode;
                                                    break;
                                                case "address":
                                                    var mode = {
                                                        address: response.ListOfFields[index].model,
                                                        address2: response.ListOfFields[index].unitModel
                                                    }
                                                    response.ListOfFields[index].model = mode;
                                                    break;

                                                case "doc":
                                                    var m = [];
                                                    if (!response.ListOfFields[index].qa) {
                                                        $(response.ListOfFields[index].modelarrays).each(function (index, fname) {
                                                            m.push({
                                                                fname: fname
                                                            });

                                                        })
                                                    }

                                                    response.ListOfFields[index].model = m;
                                                    break;
                                            }
                                        });

                                        var myfields = [];
                                        var vaultgroup = {
                                            "data": [
                                                {
                                                    "id": "0",
                                                    "name": ".basicInformation"
                                                },
                                                {
                                                    "id": "1",
                                                    "name": ".contact"
                                                },
                                                {
                                                    "id": "2",
                                                    "name": ".address"
                                                },
                                                {
                                                    "id": "3",
                                                    "name": ".financial"
                                                },
                                                {
                                                    "id": "4",
                                                    "name": ".governmentID"
                                                },
                                                {
                                                    "id": "5",
                                                    "name": ".family"
                                                },
                                                {
                                                    "id": "6",
                                                    "name": ".membership"
                                                },
                                                {
                                                    "id": "7",
                                                    "name": ".employment"
                                                },
                                                {
                                                    "id": "8",
                                                    "name": ".education"
                                                },
                                                {
                                                    "id": "9",
                                                    "name": ".others"
                                                },
                                                {
                                                    "id": "10",
                                                    "name": "Custom"
                                                },
                                                {
                                                    "id": "11",
                                                    "name": "undefined"
                                                }
                                            ]
                                        };

                                        var data = vaultgroup.data;

                                        for (var i in data) {
                                            var name = data[i].name;
                                            $(response.ListOfFields).each(function (index) {
                                                if (response.ListOfFields[index].jsPath.startsWith(name) && name != '') {
                                                    myfields.push(response.ListOfFields[index]);
                                                }
                                            });
                                        }

                                        scope.fieldList = response.ListOfFields = myfields;

                                        scope.mode = {
                                            editingForm: false,
                                            regOnly: false,
                                            viewOnly: false
                                        };

                                        var interaction = scope.interaction;

                                        if (true || interaction.type != "handshake") {
                                            // var modalInstance = $uibModal.open({
                                            //     animation: true,
                                            //     templateUrl: 'modal-feed-open-reg.html',
                                            //     controller: 'InteractionFormController',
                                            //     size: "",
                                            //     scope: scope,
                                            //     backdrop: 'static',
                                            //
                                            //     resolve: {
                                            //         registerPopup: function () {
                                            //             return {
                                            //                 ListOfFields: response.ListOfFields
                                            //                 , BusinessUserId: response.BusinessUserId
                                            //                 , CampaignId: response.CampaignId
                                            //                 , CampaignType: response.CampaignType
                                            //                 , campaign: campaign
                                            //                 , BusinessIdList: []//$scope.BusinessIdList
                                            //                 , CampaignIdInPostList: []//$scope.CampaignIdInPostList
                                            //             };
                                            //         }
                                            //     }
                                            // });
                                            //
                                            // modalInstance.result.then(function (campaign) {
                                            //     $scope.initializeController();
                                            // }, function () {
                                            // });

                                            scope.campaign = campaign;
                                            rguModal.openModal('interaction.form', scope, null);

                                        }
                                        else {
                                            // var modalInstance1 = $uibModal.open({
                                            //     animation: true,
                                            //     templateUrl: 'modal-feed-open-reg-handshake.html',
                                            //     controller: 'RegistrationHandshakeOnNewFeedController',
                                            //     size: "",
                                            //     backdrop: 'static',
                                            //
                                            //     resolve: {
                                            //         registerPopup: function () {
                                            //             return {
                                            //                 ListOfFields: response.ListOfFields
                                            //                 ,
                                            //                 BusinessUserId: response.BusinessUserId
                                            //                 ,
                                            //                 CampaignId: response.CampaignId
                                            //                 ,
                                            //                 CampaignType: response.CampaignType
                                            //                 ,
                                            //                 campaign: campaign
                                            //                 ,
                                            //                 BusinessIdList: $scope.BusinessIdList
                                            //                 ,
                                            //                 CampaignIdInPostList: $scope.CampaignIdInPostList
                                            //             };
                                            //         }
                                            //     }
                                            // });
                                            //
                                            // modalInstance1.result.then(function (campaign) {
                                            // }, function () {
                                            // });

                                            scope.campaign = campaign;
                                            //rguModal.openModal('interaction.form', scope, null);
                                            var interaction = scope.interaction = {
                                                id: response.CampaignId,
                                                type: 'handshake',
                                                business: {
                                                    accountId: response.BusinessUserId,
                                                    name: response.Campaign.BusinessUserId,
                                                    avatar: response.Campaign.BusinessImageUrl
                                                },
                                                name: response.Campaign.Name,
                                                description: response.Campaign.Description,
                                                verb: response.Campaign.Verb || 'submit',
                                                termsUrl: response.Campaign.termsAndConditionsFile,
                                                fields: response.ListOfFields
                                            };
                                            formService.openInteractionForm(interaction, scope);
                                        }
                                    })
                                    .error(function (errors) {
                                        console.log('Error pulling interaction data', errors);
                                        if (angular.isFunction(scope.showMessage))
                                            scope.showMessage('error-pulldata');
                                    });*/
            },
            openInteractionFormViewer: function (interaction, mode, scope, customer) {
                // var campaign = {};
                // var model = {
                //     // UserId: regitGlobal.userAccount.id,
                //     BusinessUserId: interaction.business.accountId,
                //     CampaignId: interaction.id,
                //     CampaignType: interaction.type,
                //     campaign: campaign
                // };
                // if (!angular.isString(interaction.business.name)) {
                //     rguCache.getUserAsync(interaction.business.accountId).then(function (user) {
                //         interaction.business.name = user.displayName;
                //         interaction.business.avatar = user.avatar;
                //     })
                // }

                if (mode === 'fields') {
                    scope.formViewer = {
                        mode: mode
                    };

                    scope.groups = scope.interaction.groups = [];
                    $http.get('/api/Interaction/Fields/' + interaction.id)
                        .success(function (fields) {
                            scope.interaction.fields = angular.fromJson(fields);
                            $http.get('/api/Interaction/Groups/' + interaction.id)
                                .success(function (groups) {
                                    scope.groups = scope.interaction.groups = angular.fromJson(groups);
                                    //rguModal.openModal('interaction.form.viewer', scope, null);
                                }).error(function (error) {
                                console.log('Error getting interaction groups', error);
                            });
                            rguModal.openModal('interaction.form.viewer', scope, null);
                        }).error(function (error) {
                        console.log('Error getting interaction fields', error);
                    });

                    return;
                }
                if (mode === 'values') {
                    scope.formViewer.mode = 'values';
                    scope.groups = scope.interaction.groups = [];
                    $http.get('/api/Interaction/Groups/' + interaction.id)
                        .success(function (groups) {
                            scope.groups = scope.interaction.groups = angular.fromJson(groups);
                        }).error(function (error) {
                        console.log('Error getting interaction groups', error);
                    });
                    console.log('a')
                    rguModal.openModal('interaction.participant.data.viewer', scope, null);

                    return;
                }

                /*               $http.post("/api/CampaignService/GetUserInformationForCampaign", model)
                                   .success(function (response) {
                                       dataresult = response;
                                       var fields = scope.fieldList = response.ListOfFields;
                                       var campaignFields = scope.interactionFields = response.Campaign.Fields;

                                       $(response.ListOfFields).each(function (index) {
                                           response.ListOfFields[index].membership = response.ListOfFields[index].membership == "true" ? true : false;
                                           response.ListOfFields[index].selected = true;
                                           switch (response.ListOfFields[index].type) {
                                               case "date":
                                               case "datecombo":
                                                   response.ListOfFields[index].model = new Date(response.ListOfFields[index].model);
                                                   break;
                                               case "location":
                                                   var mode = {
                                                       country: response.ListOfFields[index].model,
                                                       city: response.ListOfFields[index].unitModel
                                                   }
                                                   response.ListOfFields[index].model = mode;
                                                   break;
                                               case "address":
                                                   var mode = {
                                                       address: response.ListOfFields[index].model,
                                                       address2: response.ListOfFields[index].unitModel
                                                   }
                                                   response.ListOfFields[index].model = mode;
                                                   break;

                                               case "doc":
                                                   var m = [];
                                                   if (!response.ListOfFields[index].qa) {
                                                       $(response.ListOfFields[index].modelarrays).each(function (index, fname) {
                                                           m.push({
                                                               fname: fname
                                                           });

                                                       })
                                                   }

                                                   response.ListOfFields[index].model = m;
                                                   break;
                                           }
                                       });

                                       var myfields = [];
                                       var vaultgroup = {
                                           "data": [
                                               {
                                                   "id": "0",
                                                   "name": ".basicInformation"
                                               },
                                               {
                                                   "id": "1",
                                                   "name": ".contact"
                                               },
                                               {
                                                   "id": "2",
                                                   "name": ".address"
                                               },
                                               {
                                                   "id": "3",
                                                   "name": ".financial"
                                               },
                                               {
                                                   "id": "4",
                                                   "name": ".governmentID"
                                               },
                                               {
                                                   "id": "5",
                                                   "name": ".family"
                                               },
                                               {
                                                   "id": "6",
                                                   "name": ".membership"
                                               },
                                               {
                                                   "id": "7",
                                                   "name": ".employment"
                                               },
                                               {
                                                   "id": "8",
                                                   "name": ".education"
                                               },
                                               {
                                                   "id": "9",
                                                   "name": ".others"
                                               },
                                               {
                                                   "id": "10",
                                                   "name": "Custom"
                                               },
                                               {
                                                   "id": "11",
                                                   "name": "undefined"
                                               }
                                           ]
                                       };

                                       var data = vaultgroup.data;

                                       for (var i in data) {
                                           var name = data[i].name;
                                           $(response.ListOfFields).each(function (index) {
                                               if (response.ListOfFields[index].jsPath.startsWith(name) && name != '') {
                                                   myfields.push(response.ListOfFields[index]);
                                               }
                                           });
                                       }

                                       scope.fieldList = response.ListOfFields = myfields;

                                       scope.mode = {
                                           editingForm: false,
                                           regOnly: false,
                                           viewOnly: false
                                       };

                                       if (interaction.type != "handshake") {
                                           // var modalInstance = $uibModal.open({
                                           //     animation: true,
                                           //     templateUrl: 'modal-feed-open-reg.html',
                                           //     controller: 'InteractionFormController',
                                           //     size: "",
                                           //     scope: scope,
                                           //     backdrop: 'static',
                                           //
                                           //     resolve: {
                                           //         registerPopup: function () {
                                           //             return {
                                           //                 ListOfFields: response.ListOfFields
                                           //                 , BusinessUserId: response.BusinessUserId
                                           //                 , CampaignId: response.CampaignId
                                           //                 , CampaignType: response.CampaignType
                                           //                 , campaign: campaign
                                           //                 , BusinessIdList: []//$scope.BusinessIdList
                                           //                 , CampaignIdInPostList: []//$scope.CampaignIdInPostList
                                           //             };
                                           //         }
                                           //     }
                                           // });
                                           //
                                           // modalInstance.result.then(function (campaign) {
                                           //     $scope.initializeController();
                                           // }, function () {
                                           // });

                                           scope.campaign = campaign;
                                           rguModal.openModal('interaction.form', scope, null);

                                       }
                                       else {
                                           var modalInstance1 = $uibModal.open({
                                               animation: true,
                                               templateUrl: 'modal-feed-open-reg-handshake.html',
                                               controller: 'RegistrationHandshakeOnNewFeedController',
                                               size: "",
                                               backdrop: 'static',

                                               resolve: {
                                                   registerPopup: function () {
                                                       return {
                                                           ListOfFields: response.ListOfFields
                                                           ,
                                                           BusinessUserId: response.BusinessUserId
                                                           ,
                                                           CampaignId: response.CampaignId
                                                           ,
                                                           CampaignType: response.CampaignType
                                                           ,
                                                           campaign: campaign
                                                           ,
                                                           BusinessIdList: $scope.BusinessIdList
                                                           ,
                                                           CampaignIdInPostList: $scope.CampaignIdInPostList
                                                       };
                                                   }
                                               }
                                           });

                                           modalInstance1.result.then(function (campaign) {
                                           }, function () {
                                           });
                                       }
                                   })
                                   .error(function (errors) {
                                       console.log('Error pulling interaction data', errors);
                                       scope.showMessage('error-pulldata');
                                   });*/
            }
        };

    })

    .service('interactionFormService', function (rgu) {
        var traverseVault = function ($scope, node, level, path, jsPath) {
            if (!angular.isObject(node) || node.nosearch) return;
            angular.forEach(node, function (entry, name) {
                if (!angular.isObject(entry) || entry.nosearch || !angular.isDefined(entry.hiddenForm) || entry.hiddenForm === true)
                    return;

                if (entry.hasOwnProperty('exception')) {
                    if ($scope.hasOwnProperty('interaction') && $scope.interaction.type === entry.exception)
                        return;
                }

                if (!angular.isObject(entry.value) || angular.isArray(entry.value)) {
                    //  Leaf entry (field)
                    if (name === 'instruction') return;

                    var field = {
                        id: entry._id,
                        label: entry.label,
                        options: entry.value,
                        leaf: true,
                        type: entry.controlType,
                        undraggable: entry.undraggable,
                        rules: entry.rules,
                        level: level,
                        path: path,
                        jsPath: jsPath + '.' + name
                    };
                    if (jsPath.substring(0, 11) === '.membership') {
                        field.membership = true;
                    }
                    if (entry.controlType === 'history') {
                        field.model = 4;
                    }

                    if (entry.hasOwnProperty('nogroup')) {
                        field.nogroup = 'true';
                    }


                    $scope.vaultEntries.push(field);

                } else {
                    //  Non-leaf entry (folder)

                    if (name === 'body' && $scope.hasOwnProperty('interaction') && $scope.interaction.type === 'handshake')
                        return;

                    var group = {
                        id: entry._id,
                        label: entry.label,
                        leaf: false,
                        undraggable: entry.undraggable,
                        level: level,
                        path: path,
                        jsPath: jsPath + '.' + name
                    };

                    $scope.vaultEntries.push(group);
                    traverseVault($scope, entry.value, level + 1, path + '.' + entry.label, jsPath + '.' + name);
                }
            });
        };
        var _addEntryToForm = function ($scope, entry, checkMembership) {
            if (entry.undraggable) return;
            if ($scope.addedToForm(entry))
                return;

            var ranks = ['basicInformation', 'contact', 'address', 'financial', 'governmentID', 'family', 'employment', 'education', 'others', 'userInformation'];

            function labelFromName(name) {
                return rgu.deCamelize(name);
            }

            var field = angular.copy(entry);
            if (field.leaf) {
                if (!angular.isString(field.jsPath)) return;
                if (!field.displayName) field.displayName = entry.label;
                field.optional = false;

                if (checkMembership) {
                    $scope.checkMembership();
                }

                var path = field.jsPath.split('.');
                var group = path[1];
                if (group === 'address' || group === 'financial' || group === 'governmentID' || group === 'others') {
                    group = path[2];
                }
                if (field.jsPath.startsWith("Custom")) {
                    group = "userInformation"
                }
                field.group = group;
                field.bucket = path[1];
                var found = null;
                if (angular.isObject($scope.form) && angular.isArray($scope.form.groups)) {
                    var groups = $scope.form.groups;
                    found = groups.find(function (g) {
                        return group === g.name;
                    });
                }
                if (!found) {
                    found = {
                        name: group,
                        label: labelFromName(group),
                        displayName: labelFromName(group),
                        fields: []
                    };
                    if (groups) {
                        groups.push(found);
                    }
                }
                if (found) {
                    found.fields.push(field);
                }

                if (angular.isObject($scope.form) && angular.isArray($scope.form.groups)) {
                    $scope.form.fields.push(field);
                } else {
                    $scope.formEntries.push(field);
                }

            } else {    // Add whole group
                var path = entry.path + '.' + entry.label;
                index = $.inArray(entry, $scope.vaultEntries);
                if (index < 0) return;
                while (++index < $scope.vaultEntries.length) {
                    var field = $scope.vaultEntries[index];
                    if (field.path !== path)
                        break;
                    if (field.hasOwnProperty('nogroup') && field.nogroup)
                        continue;
                    if (!$scope.addedToForm(field)) {
                        _addEntryToForm($scope, field, checkMembership);
                    }
                }
            }

        };
        return {
            initFieldGroups: function (fields) {
                if (!angular.isArray(fields)) return;
                var ranks = ['basicInformation', 'contact', 'address', 'financial', 'governmentID', 'family', 'employment', 'education', 'others', 'userInformation'];
                angular.forEach(fields, function (field) {
                    //if (!field.hasOwnProperty('jsPath')) return;
                    if (!angular.isString(field.jsPath)) return;
                    var path = field.jsPath.split('.');
                    var group = path[1];
                    if (group === 'address' || group === 'financial' || group === 'governmentID' || group === 'others') {
                        group = path[2];
                    }
                    //var pth = field.path;
                    if (field.jsPath.startsWith("Custom")) {
                        group = "userInformation"
                    }
                    field.group = group;
                    field.bucket = path[1];
                });

                var lastGroup = null;
                angular.forEach(fields, function (field) {
                    if (!field.hasOwnProperty('jsPath')) return;

                    var group = field.group;
                    if (group !== lastGroup) {
                        field.firstInGroup = true;
                        lastGroup = group;
                    }
                    var rank = $.inArray(field.bucket, ranks);
                    if (rank < 0) {
                        rank = 11;
                    }
                    if (rank < 10) {
                        rank = '0' + rank;
                    } else {
                        rank = rank.toString();
                    }
                    field.rank = rank;

                });
                fields.sort(function (f1, f2) {
                    var r1 = f1.rank + f1.group;
                    var r2 = f2.rank + f2.group;
                    //var r1 = f1.id;
                    //var r2 = f1.id;

                    return r1 > r2 ? 1 : -1;
                });
                if (fields.length) {
                    fields[0].firstGroup = true;
                }
            },

            traverseVault: traverseVault,
            addEntryToForm: _addEntryToForm,
            sortField: function (fields) {
                var result = [];
                var dataGroup = [
                    {"id": "0", "jsPath": ".basicInformation", "name": "Basic Information"},
                    {"id": "1", "jsPath": ".contact", "name": "Contact"},
                    {"id": "3", "jsPath": ".address.currentAddress", "name": "Current Address"},
                    {"id": "4", "jsPath": ".address.deliveryAddress", "name": "Delivery Address"},
                    {"id": "5", "jsPath": ".address.billingAddress", "name": "Billing Address"},
                    {"id": "6", "jsPath": ".address.mailingAddress", "name": "Mailing Address"},
                    {"id": "7", "jsPath": ".address.pobox", "name": "Pobox"},
                    {"id": "8", "jsPath": ".governmentID.birthCertificate", "name": "Birth Certificate"},
                    {"id": "9", "jsPath": ".governmentID.driverLicenseCard", "name": "Driver License Card"},
                    {"id": "10", "jsPath": ".governmentID.healthCard", "name": "Health Card"},
                    {"id": "11", "jsPath": ".governmentID.medicalBenifitCard", "name": "Medical Benefit Card"},
                    {"id": "12", "jsPath": ".governmentID.passportID", "name": "PassportID"},
                    {
                        "id": "13",
                        "jsPath": ".governmentID.permanentResidenceIdCard",
                        "name": "Permanent Residence Card"
                    },
                    {"id": "14", "jsPath": ".governmentID.taxCard", "name": "Tax"},
                    {"id": "15", "jsPath": ".governmentID.nationalID", "name": "NationalID"},
                    {"id": "16", "jsPath": ".family", "name": "Family"},
                    {"id": "17", "jsPath": ".membership", "name": "Membership"},
                    {"id": "18", "jsPath": ".employment", "name": "Employment"},
                    {"id": "19", "jsPath": ".education", "name": "Education"},
                    {"id": "20", "jsPath": ".others", "name": "Other"},
                    {"id": "21", "jsPath": "Custom", "name": "User Information"}

                ];

                for (var j = 0; j < dataGroup.length; j++) {

                    var listField = [];
                    for (var i = 0; i < fields.length; i++) {
                        var field = fields[i];
                        if (field.jsPath && field.jsPath.startsWith(dataGroup[j].jsPath)) {
                            listField.push(field);
                        }
                    }
                    if (listField.length > 0) {
                        var rs = {"name": dataGroup[j].name, "displayName": dataGroup[j].name, "fields": listField}
                        result.push(rs);
                    }
                }
                return result;
            }

        };
    })

    .directive('campaignIndicator', function ($timeout, campaignTypeNames, campaignTypeAbbrs) {
        function link(scope, element, attrs) {
            scope.$watchCollection('campaign', function (newVal) {
                //var type = scope.type, status = scope.status;
                if (!newVal) return;
                var c = newVal;
                var type = c.Type || '', status = c.Status;
                scope.type = type;
                scope.status = status;

                var typeIndex = $.inArray(type, campaignTypeNames);
                if (typeIndex >= 0) {
                    scope.abbr = campaignTypeAbbrs[typeIndex];
                }
                //  $scope.abbr = campaignTypeAbbrs[typeIndex];
                var statusIndexes = {
                    'active': 0,
                    'pending': 1,
                    'inactive': 2,
                    'expired': 3
                };

                scope.statusIndex = statusIndexes[status];
            });
        }

        return {
            restrict: 'E',
            link: link,
            scope: {
                type: '@',
                status: '@',
                namecampaign: '@',
                campaign: '='
            },

            template: '<span class="campaign-indicator campaign-type-{{type.toLowerCase()}} '
            + 'campaign-status-{{status}}">'
            + '<span class="campaign-indicator-inner">{{abbr}}</span></span>' + '{{namecampaign}}'

        };
    }).filter('vaultEntryPath', function () {
    return function (path) {
        if (!path) return '';
        var pathParts = path.split('.');
        pathParts.splice(0, 1);
        pathParts.push('');
        return pathParts.join(' &raquo; ');
    };
})
    .filter('vaultEntryHighlightLabel', function () {
        return function (label, query) {
            if (!query.length) return label;
            var re = new RegExp(query, 'i');
            return label.replace(re, '<b>$&</b>');
        };
    })
    .filter('formFieldPath', function () {
        return function (path) {
            if (!path) return '';
            var pathParts = path.split('.');
            pathParts.splice(0, 1);
            pathParts.push('');
            return pathParts.join(' &raquo; ');
        };
    })

    .filter('formFieldJsPath', function () {
        return function (jsonpath) {
            if (!jsonpath) return '';
            var result = "";
            var pathParts = jsonpath.split('.');
            if (pathParts.length === 3)
                result = pathParts[1];

            if (pathParts.length === 4)
                result = pathParts[1] + " " + pathParts[2];
            switch (result) {
                case "address":
                    result = "Address ";
                    break;
                case "address currentAddress":
                    result = "Current Address ";
                    break;
                // address mailingAddress address deliveryAddress  address billingAddress address pobox
                case "address deliveryAddress":
                    result = "Delivery Address ";
                    break;
                case "address billingAddress":
                    result = "Billing Address ";
                    break;
                case "address mailingAddress":
                    result = "Mailing Address ";
                    break;
                case "address pobox":
                    result = "P.O Box";
                    break;

                case "governmentID birthCertificate":
                    result = "Government ID - Birth Certificate";
                    break;
                case "governmentID driverLicenseCard":
                    result = "Government ID - Driver License Card";
                    break;
                case "governmentID healthCard":
                    result = "Government ID - Health Card";
                    break;
                //

                case "governmentID passportID":
                    result = "Government ID - Passport ID";
                    break;
                //
                case "governmentID nationalID":
                    result = "Government ID - National ID";
                    break;

                //membership
                case "membership":
                    result = "Membership";
                    break;

                //employment education
                case "employment":
                    result = "Employment";
                    break;
                //employment education
                case "education":
                    result = "Education";
                    break;
                // others preference others favourite  others body
                case "others preference":
                    result = "Others - Preference";
                    break;
                case "others favourite":
                    result = "Others - Favourite";
                    break;
                case "others body":
                    result = "Others - Body";
                    break;
                case "basicInformation":
                    result = "Basic Information ";
                    break;

                case "contact":
                    result = "Contact ";
                    break;
            }
            return result;

        };
    })
    .filter('controlTypeName', function () {
        return function (type) {
            var names = {
                textbox: 'Text field',
                textarea: 'Multiline text field',
                date: 'Date picker',
                datecombo: 'Date picker combo',
                datedmy: 'Day/Month/Year picker',
                numinput: 'Number with unit',
                radio: 'Radio button',
                checkbox: 'Checkbox',
                select: 'Dropdown select',
                tagsinput: 'Free tag list',
                smartinput: 'Smart tag list',
                range: 'Range select',
                location: 'Location input',
                address: 'Address input',
                history: 'Recent history',
                'static': 'Static',
                qa: 'Custom question',
                doc: 'Document file',
                contact: 'Contact'
            };
            if (names.hasOwnProperty(type))
                return names[type];
            return 'Text field';
        };
    })
    .filter('hiddenField', function () {
        return function (value) {
            var wildCard = '*';
            var text = '';
            for (var i = 0; i < 6; i++) {
                text += wildCard;
            }
            return text;
        };
    })

    .service('billingService', function (rguModal, $http) {
        return {
            CheckBillingCurrent: function (functionsuccess, functionfail, modulename, currentusersync) {
                $http.post('/api/BusinessAccount/GetPaymentDetailByUserId', null)
                    .success(function (response) {
                        if (response.ReturnMessage[0] == "-2") {
                            var scope = {};
                            rguModal.openModal('billing.quota.expired', scope, null)
                        }
                        if (response.ReturnMessage[0] == "-1") {
                            swal(
                                'Billing error',
                                "Your promo code has expired",
                                'error'
                            );
                        }
                        var billingInfo = {
                            plan: {
                                name: response.PaymentPlanName,
                                label: response.PaymentPlanDesc
                            },
                            maxWorkflowMembers: response.PaymentPlanDetailWorkFlow.currentmaxnumber,
                            workflowMembers: response.PaymentPlanDetailWorkFlow.currentnumber,
                            syncMembers: response.PaymentPlanDetailSyncForm.currentnumber,
                            maxSyncMembers: response.PaymentPlanDetailSyncForm.currentmaxnumber
                        }
                        var billing = {
                            plan: billingInfo.plan,
                            planFree: billingInfo.plan.name === 'free',
                            reachedWorkflowMembers: billingInfo.workflowMembers >= billingInfo.maxWorkflowMembers,
                            reachedSyncMembers: billingInfo.syncMembers >= billingInfo.maxSyncMembers
                        }
                        switch (modulename) {
                            case "workflow":
                                if (billing.reachedWorkflowMembers) {
                                    functionfail(billingInfo);
                                }
                                else {
                                    functionsuccess();
                                }
                                break;
                            case "syncmembers":
                                if (billingInfo.syncMembers + parseInt(currentusersync) > billingInfo.maxSyncMembers) {
                                    functionfail(billingInfo);
                                }
                                else {
                                    functionsuccess();
                                }
                                break;
                            case "interactionactive":
                                if (billing.planFree) {
                                    functionfail(billingInfo);
                                }
                                else {
                                    functionsuccess();
                                }
                                break;
                        }
                    })
                    .error(function (errors, status) {
                    });
            },

            getBillingInfo: function () {
                return {
                    plan: {
                        name: 'free',
                        label: 'Free Tier'

                    },
                    workflowMembers: 0,
                    maxWorkflowMembers: 1,
                    syncMembers: 0,
                    maxSyncMembers: 1
                };
            },
            alertNoFree: function (billingInfo) {
                var scope = {
                    plan: billingInfo.plan
                };
                rguModal.openModal('billing.quota.nofree', scope, {billingInfo: billingInfo})
            },
            alertbillingexpired: function (billingInfo) {
                var scope = {
                    plan: billingInfo.plan
                };
                rguModal.openModal('billing.quota.expried', scope, {billingInfo: billingInfo})
            },
            alertQuotaLimit: function (billingInfo) {
                var scope = {
                    plan: billingInfo.plan
                };
                rguModal.openModal('billing.quota.limit', scope, {billingInfo: billingInfo})
            }
        };
    })
    .directive('dragonLawReferral', function () {
        return {
            restrict: 'EAC',
            templateUrl: '/Areas/regitUI/templates/dragon-law-referral.html'
        };
    })
    .directive('interactionField', function ($rootScope, $timeout, $http, fileUpload, rgu, rguNotify, rguModal, formService) {
        var cmpId = 0;
        return {
            restrict: 'E',
            scope: {
                type: '@',
                model: '=ngModel',
                interaction: '<',
                mode: '<',
                validate: '=?'
            },
            templateUrl: '/Areas/regitUI/templates/interaction-form-field.html?v=19',
            link: function (scope, elem, attrs) {
                scope.cmpId = ++cmpId;
                var field = scope.field = scope.model;
                scope.view = {
                    showingMessage: false
                };

                scope.isStatic = field.type === 'static';
                scope.name = field.type + scope.cmpId;
                field.view = {
                    showingValue: false
                };
                field.validate = field.validate || {
                    invalid: false,
                    noValue: false
                };
                field.dirty = false;

                var isViewOnly = scope.isViewOnly = scope.mode.viewOnly || scope.mode.fieldsOnly;
                var fieldsOnly = scope.fieldsOnly = scope.mode.fieldsOnly;
                var viewValues = scope.viewValues = scope.mode.viewValues;

                var interaction = scope.interaction;
                scope.verb = interaction.type === 'registration' ? 'register' : 'join';
                scope.showMessage = function (msg) {
                    scope.view.showingMessage = msg;
                };

                function watchField(newVal, oldVal) {
                    var invalid = scope.field.validate.invalid = !formService.validateField(scope.field);
                    scope.field.validate.invalid = invalid;
                    if (angular.isObject(scope.validate))
                        scope.validate.dirty = true;
                    if (angular.isDefined(oldVal) && newVal !== oldVal)
                        scope.field.dirty = true;
                }

                if (!isViewOnly) {
                    if (field.options === 'phone') {
                        scope.$watch('field.validate', function (newValue) {
                            scope.validate.dirty = true;
                        }, true);
                    } else {
                        scope.$watch('field.model', watchField, true);
                        if (field.optional) {
                            scope.$watch('field.selected', watchField);
                        }
                    }
                }
                scope.toggleShowValue = function () {
                    scope.field.view.showingValue = !scope.field.view.showingValue;
                };
                scope.isIgnored = function () {
                    return !isViewOnly && scope.field.optional && !scope.field.selected;
                };
                scope.hasNoValue = function () {
                    return formService.fieldNoValue(scope.field);
                };
                scope.isValid = function () {
                    return formService.validateField(scope.field);
                };

                scope.$on('interaction:formfields:update', function (event, data) {
                    var fields = data.fields;
                    var field = fields.find(function (field) {
                        return field.jsPath === scope.jsPath;
                    });

                    if (angular.isDefined(field)) {
                        scope.field = field;
                    }
                });

                scope.uploadFile = function (element, field) {
                    var file = element.files[0];
                    var uploadUrl = "/api/CampaignService/UploadFileRegisform?jsPath=" + field.pathfile;

                    fileUpload.uploadFileToUrl(file, uploadUrl, function (reponse) {
                        // console.log(scope.field);
                        if (!scope.field.model) {
                            scope.field.model = [];
                        }
                        scope.field.model.push({
                            selected: true,
                            fileName: reponse.fileName,
                            filePath: reponse.filepath
                        });

                        var input = $(element);
                        input.replaceWith(input.val('').clone(true));

                    }, function () {

                    }, "");
                };

                scope.deleteDoc = function (model, index) {
                    model.splice(index, 1);
                };

                scope.selectDoc = function (doc) {
                    doc.selected = !doc.selected;
                };

            }
        };
    })

    .filter('fieldStaticHtml', function ($moment, rgu) {
        var filterValue = function (field) {
                var type = field.type;
                if (type === 'static') return field.options;
                var value = field.model;
                if (angular.isUndefined(value))
                    return '';
                if (type === 'textbox')
                    return value;
                if (type === 'range') {
                    value = parseInt(value);
                    if (isNaN(value) || value < 0 || value >= field.options.length)
                        return '';
                    var range = field.options[value];
                    var start = range[0];
                    var end = range[1];
                    var rangeText = '';
                    if (!angular.isNumber(start)) {
                        rangeText = '< ' + end;
                    } else if (!angular.isNumber(end)) {
                        rangeText = '> ' + start;
                    } else {
                        rangeText += start + ' ... ' + end;
                    }
                    return rangeText;
                }

                if (type === 'doc') {
                    if (!angular.isArray(value) || !value.length) return '';
                    var docs = value;
                    var html = '<ul class="static-doc-list">';
                    angular.forEach(docs, function (doc) {
                        if (!doc.selected) return;
                        var fname = doc.fileName;
                        html += '<li><a class="static-doc-item" target="_blank" href="' + doc.filePath + '/' + fname + '">';
                        if (/\.(jpe?g|png|gif|bmp)$/i.test(fname)) {
                            html += '<img src="' + doc.filePath + '/' + fname + '">';
                        } else {
                            html += fname;
                        }
                        html += '</a></li>';
                    });
                    html += '</ul>';
                    /*                    var html = '<a class="static-doc-item" target="_blank" href="' + field.pathfile + fname + '">';
                                        if (/\.(jpe?g|png|gif|bmp)$/i.test(fname)) {
                                            html += '<img src="' + field.pathfile + fname + '">';
                                        } else {
                                            html += fname;
                                        }
                                        html += '</a>';*/
                    return html;
                }

                if (angular.isArray(value))
                    return value.join(', ');

                if (type === 'date' || type === 'datecombo') {
                    var parsedDate = rgu.parseDate(value);
                    if (parsedDate) {
                        // console.log(value);
                        var datestr = $moment(parsedDate.date).format('D MMMM YYYY');
                        if (datestr === 'Invalid date' || datestr === 'Invalid Date') {
                            if (field.options === 'indef') {
                                return 'Indefinite';
                            }
                            datestr = '';

                        }
                        return datestr;
                    } else {
                        return '';
                    }
                }
                if (angular.isObject(value)) {
                    switch (type) {
                        case 'location':
                            if (value.hasOwnProperty('country') && value.hasOwnProperty('city')) {
                                var country = value.country || '', city = value.city || '';
                                var text = city;
                                if (country && city) {
                                    text += ', ';
                                }
                                text += country;
                                return text;
                            }
                            break;
                        case 'numinput':
                            if (value.hasOwnProperty('amount') && value.hasOwnProperty('unit')) {
                                var amount = value.amount || '', unit = value.unit || '';
                                return amount + ' ' + unit;
                            }
                            break;
                    }
                    return '***';

                }

                return value.toString();
            }
        ;
        filterValue.$stateful = true;
        return filterValue;
    });
/*
    angular.module('regitInteraction').controller('InteractionPageController', [
      '$scope', function ($scope) {
        console.log('a')
        $scope.pageInteraction = regitGlobal.interaction;
    }]);*/
