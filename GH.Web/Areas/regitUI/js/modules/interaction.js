angular.module('interaction', ['social'])
    .constant('interactionTypeNames', ['Advertising', 'Registration', 'Event', 'SRFI', 'PushToVault', 'Handshake'])
    .constant('interactionTypeAbbrs', ['Ad', 'Reg', 'Evt', 'SRFI', 'P', 'HS'])
    .constant('interactionStatusNames', ['Pending', 'Active', 'Inactive', 'Expired'])
    .factory('interactionService', function ($location) {
        return {
            publicUrl: function (interaction) {
                if (!angular.isString(interaction.id)) return '';
                var url = $location.protocol() + '://' + $location.host();
                var port = $location.port();
                if (port && port !== 80 && port !== 443) {
                    url += ':' + port;
                }
                url += '/interaction/' + interaction.id;
                return url;
            }
        };

    })

    .directive('interaction', function ($rootScope, $timeout, $http, rguNotify, rguModal, formService) {

        return {
            restrict: 'E',
            scope: {
                type: '@',
                interaction: '=ngModel',
                participate: '&?'
            },
            templateUrl: '/Areas/regitUI/templates/interaction.html?v=29',
            link: function (scope, elem, attrs) {
                var type = scope.interaction.type;
                scope.isAd = scope.isBroadcast = type === 'broadcast';

                scope.view = {
                    showingMessage: false
                };
                var interaction = scope.interaction;
                scope.business = interaction.business;
                scope.asBusiness = regitGlobal.asBusiness;
                scope.ownBusiness = regitGlobal.asBusiness && scope.business.id === regitGlobal.businessAccount.id;
                if (scope.ownBusiness) {
                    var isAdmin = false;
                    var isEditor = false;
                    var isApprover = false;
                    if (angular.isArray(regitGlobal.workflowAccount.roles)) {
                        regitGlobal.workflowAccount.roles.forEach(function (role) {
                            if (role === 'admin') isAdmin = true;
                            if (role === 'editor') isEditor = true;
                            else if (role === 'approver') isApprover = true;
                        });
                    }
                    scope.workflow = {
                        canEdit: isAdmin || isEditor,
                        canApprove: isAdmin || isApprover
                    };
                }

                scope.verb = 'register';
                switch (type) {
                    case 'srfi':
                        scope.verb = 'submit';
                        break;
                    case 'event':
                        scope.verb = 'join';
                        break;
                }
                scope.verbPast = formService.verbPast;
                scope.fields = null;
                interaction.participants = interaction.participants || 0;
                interaction.verb = interaction.verb || (type === 'event' ? 'join' : 'register');

                scope.showMessage = function (msg) {
                    scope.view.showingMessage = msg;
                };

                function updateFollowing(status) {

                    interaction.business.following = !!status;
                    $rootScope.$broadcast('interaction:followed', {
                        businessId: scope.interaction.business.id,
                        status: status
                    });
                }

                scope.toggleFollowBusiness = function (business) {
                    if (regitGlobal.isPublic) {
                        scope.showMessage('unauthenticated-follow');
                        return;
                    }
                    if (business.following) {
                        $http.post('/Api/Interaction/Unfollow/' + business.id)
                            .success(function (response) {
                                rguNotify.add('Unfollowed ' + business.name);
                                updateFollowing(false);
                            }).error(function (errors) {
                            console.log('Error unfollowing business', errors);
                        });
                    } else {
                        $http.post('/Api/Interaction/Follow/' + business.id)
                            .success(function (response) {
                                rguNotify.add('Followed ' + business.name);
                                updateFollowing(true);
                            }).error(function (errors) {
                            console.log('Error following business', errors);
                        });
                    }

                };

                scope.openForm = function (interaction) {
                    if (angular.isFunction(scope.participate)) {
                        scope.participate();
                        return;
                    }
                    if (regitGlobal.isPublic) {
                        scope.showMessage('unauthenticated-participate');
                        return;
                    }
                    // $http.get('/Api/Interaction/Fields/' + interaction.id)
                    //     .success(function (response) {
                    //         var fields = $.parseJSON(response);
                    //         if (!angular.isArray(fields) || !fields.length) {
                    //             console.log('Error reading interaction fields', errors);
                    //             scope.showMessage('error-pulldata');
                    //         }
                    //         scope.fields = fields;
                    //     }).error(function (errors) {
                    //     console.log('Error pulling interaction fields', errors);
                    //     scope.showMessage('error-pulldata');
                    // });
                    var campaign = {};
                    var model = {
                        // UserId: regitGlobal.userAccount.id,
                        BusinessUserId: interaction.business.accountId,
                        CampaignId: interaction.id,
                        CampaignType: interaction.type,
                        campaign: campaign
                    };
                    scope.titleSuffix = interaction.type === 'srfi' ? 'Submission Form' : 'Registration Form';
                    $http.post("/api/CampaignService/GetUserInformationForCampaign", model)
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
                                        };
                                        response.ListOfFields[index].model = mode;
                                        break;
                                    case "address":
                                        var mode = {
                                            address: response.ListOfFields[index].model,
                                            address2: response.ListOfFields[index].unitModel
                                        };
                                        response.ListOfFields[index].model = mode;
                                        break;

                                    case "doc":
                                        var m = [];
                                        if (!response.ListOfFields[index].qa) {
                                            $(response.ListOfFields[index].modelarrays).each(function (index, fname) {
                                                m.push({
                                                    fname: fname
                                                });

                                            });
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
                                    if (response.ListOfFields[index].jsPath && response.ListOfFields[index].jsPath.startsWith(name) && name != '') {
                                        myfields.push(response.ListOfFields[index]);
                                    }
                                });
                            }

                            scope.fieldList = response.ListOfFields = myfields;

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
                            console.log(model);
                            console.log('Error pulling interaction data', errors);
                            scope.showMessage('error-pulldata');
                        });
                };

                scope.deParticipate = function (interaction) {
                    if (!interaction.participation) return;
                    var campaign = {};
                    var model = {
                        BusinessUserId: interaction.business.accountId,
                        CampaignId: interaction.id,
                        CampaignType: interaction.type,
                        campaign: campaign
                    };

                    $http.post("/api/CampaignService/DeRegisterCampaign", model)
                        .success(function (response) {
                            delete interaction.participation;
                            $rootScope.$broadcast('interaction:departicipated');
                            rguNotify.add('Cancelled participation in "' + interaction.name + '"');
                        })
                        .error(function (errors) {
                            console.log('Error departicipating interaction');
                        });
                };

                scope.editInteraction = function (interaction) {
                    location.href = '/Interactions/Edit/' + interaction.id;
                };
                scope.approveInteraction = function (interaction) {
                    location.href = '/Interactions/Review/' + interaction.id;
                };
            }
        };
    })
    .directive('interactionFeed', function ($rootScope, $timeout, $http) {

        return {
            restrict: 'E',
            scope: {
                type: '@'
            },
            templateUrl: '/Areas/regitUI/templates/interaction-feed.html?v=3',
            link: function (scope, elem, attrs) {
                var type = attrs.type;
                scope.isBusiness = type === 'business';
                scope.view = {
                    showingMessage: false
                };
                var interactions = scope.interactions = [];

                scope.showMessage = function (msg) {
                    scope.view.showingMessage = msg;
                };

                scope.filterInteractions = function (interaction) {
                    return !interaction.expired;
                };

                function updateFeed(more) {
                    if (!attrs.fromBusiness) {
                        $http.get('/api/Interaction/Feed')
                            .success(function (response) {
                                if (response.success) {
                                    scope.interactions = response.data;
                                } else {
                                    console.log('Error loading interaction feed', response);
                                }
                            })
                            .error(function (errors) {
                                console.log('Error loading interaction feed', errors);
                            });

                        // $http.get('/api/interaction/newsfeed?page=3')
                        //     .success(function (response) {
                        //        console.log(response)
                        //     })
                        //     .error(function (errors) {
                        //         console.log('Error loading interaction feed', errors)
                        //     });
                    } else {
                        $http.get('/api/Interaction/Feed', {
                            params: {businessAccountId: attrs.fromBusiness}
                        })
                            .success(function (response) {
                                if (response.success) {
                                    scope.interactions = response.data;
                                } else {
                                    console.log('Error loading interaction feed', response);
                                }
                            })
                            .error(function (errors) {
                                console.log('Error loading interaction feed', errors);
                            });
                    }
                }

                scope.feedPulls = 0;
                scope.interactions = [];
                scope.pullFeed = function () {
                    if (!scope.interactions.length) {
                        scope.feedPulls = 0;
                    }
                    if (!attrs.fromBusiness) {
                        $http.get('/api/Interaction/Feed?page=' + ++scope.feedPulls)
                            .success(function (response) {
                                if (response.success)
                                    scope.interactions = scope.interactions.concat(response.data);
                            })
                            .error(function (errors) {
                                console.log('Error loading interaction feed', errors);
                            });
                    } else {
                        // console.log(scope.feedPulls, scope.interactions)
                        $http.get('/api/Interaction/Feed', {
                            params: {
                                businessAccountId: attrs.fromBusiness,
                                page: ++scope.feedPulls
                            }
                        })
                            .success(function (response) {
                                // console.log(scope.feedPulls, response)
                                if (response.success)
                                    scope.interactions = scope.interactions.concat(response.data);

                            })
                            .error(function (errors) {
                                console.log('Error loading business interaction feed', errors);
                            });
                    }

                };

                scope.pullFeed();


                scope.$on('interaction:followed', function (event, data) {
                    scope.interactions.forEach(function (interaction) {
                        if (interaction.business.id === data.businessId) {
                            interaction.business.following = data.status;
                        }
                    });
                });
            }
        };
    });



