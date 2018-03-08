angular.module('customers').controller("BusinessCustomersController",
    ['$scope', '$timeout', '$http', '$filter', 'rguNotify', 'rguAlert',
        function ($scope, $timeout, $http, $filter, rguNotify, rguAlert) {
            $scope.view = {
                allCustomers: false,
                interactionLoaded: false,
                customersLoaded: false,
                formExpanded: false,
                interactionSelectorShowing: false
            };
            $scope.grid = {
                search: {
                    query: ''
                },
                filterStatus: {
                    activeIndex: 0,
                    options: [
                        {name: 'all', label: 'All Customers'},
                        {name: 'active', label: 'Active Only'},
                        {name: 'pending', label: 'Pending Only'}
                    ],
                    value: 'all',
                    opening: false
                }
            };
            $scope.openFilterStatus = function () {
                $scope.grid.filterStatus.opening = true;
            };
            $scope.closeFilterStatus = function () {
                $scope.grid.filterStatus.opening = false;
            };
            $scope.selectFilterStatus = function (option, index) {
                $scope.grid.filterStatus.activeIndex = index;
                $scope.grid.filterStatus.value = $scope.grid.filterStatus.options[index].name;
                $scope.closeFilterStatus();
            };
            $scope.interactionSelector = {
                query: ''
            };
            $scope.toggleForm = function () {
                $scope.view.formExpanded = !$scope.view.formExpanded;
            };
            $scope.toggleRow = function (customer) {
                customer.view.expanded = !customer.view.expanded;
            };
            $scope.openInteractionSelector = function () {
                $scope.view.interactionSelectorShowing = true;
            };
            // $scope.openInteractionSelector();

            $scope.mouseEnterInteraction = function (interaction) {
                $scope.interactionSelector.hovering = interaction.id;
            };
            $scope.mouseLeaveInteraction = function (interaction) {
                $scope.interactionSelector.hovering = false;
            };
            $scope.hoverNextInteraction = function (next) {
                var interactions = $filter('filterInteractionsByQuery')($scope.interactions, $scope.interactionSelector.query);
                var index = interactions.findIndex(function (i) {
                    return i.id === $scope.interactionSelector.hovering;
                });
                if (index < 0) index = 0;
                if (next && index >= interactions.length - 1) return;
                if (!next && index < 1) return;
                if (next) index++;
                else index--;
                $scope.interactionSelector.hovering = interactions[index].id;
            };
            $scope.submitInteractionQuery = function () {
                var interaction;
                if ($scope.interactionSelector.hovering) {
                    interaction = $scope.interactions.find(function (i) {
                        return i.id === $scope.interactionSelector.hovering;
                    });
                    if (!interaction) return;
                } else {
                    var interactions = $filter('filterInteractionsByQuery')($scope.interactions, $scope.interactionSelector.query);
                    if (!interactions.length) return;
                    interaction = interactions[0];
                }
                $scope.selectInteraction(interaction);
            };
            $scope.resetInteractionSelector = function () {
                $scope.interactionSelector.query = '';
                $scope.interactionSelector.hovering = false;
                $scope.view.interactionSelectorShowing = false;
            };
            $scope.$on('document::click', function (ev, args) {
                if ($scope.view.interactionSelectorShowing) {
                    $timeout(function () {
                        $scope.resetInteractionSelector();
                    });
                }
                // if ($scope.view.addCustomerShowing) {
                //     $timeout(function () {
                //         $scope.closeAddCustomer();
                //     });
                // }
                if ($scope.grid.filterStatus.opening) {
                    $timeout(function () {
                        $scope.closeFilterStatus();
                    });
                }
            });
            $scope.clearCustomersQuery = function () {
                $scope.grid.search.query = '';
                if ($scope.view.customersLoaded)
                    $scope.customers.forEach(function (c) {
                        if (c.view)
                            c.view.expanded = false;
                        c.participations.forEach(function (p) {
                            if (p.view)
                                p.view.expanded = false;
                        });
                    });
            };
            $scope.updateCustomersByQuery = function () {
                $scope.customers.forEach(function (c) {
                    c.view.expanded = false;
                });
            };

            $scope.toggleParticipation = function (participation) {
                participation.view.expanded = !participation.view.expanded;
            };
            $scope.isParticipationExpanded = function (participation) {
                return participation.view.expanded || participation.view.dataMatched;
            };
            $scope.isCustomerExpanded = function (customer) {
                return customer.view.expanded ||
                    customer.participations.some(function (p) {
                        return $scope.isParticipationExpanded(p);
                    });
            };
            $scope.countParticipants = function (customer, pending) {
                if (!customer) return 0;
                return customer.participations.filter(function (c) {
                    return pending ? c.status === "pending" : c.status !== "pending";
                }).length;
            };
            $scope.countAllParticipants = function (pending) {
                if (!$scope.customers) return 0;
                return $scope.customers.reduce(function (count, customer) {
                    if (!customer.participations) return count;
                    if (pending)
                        return customer.participations.filter(function (p) {
                            return p.status === "pending";
                        }).length + count;
                    return customer.participations.some(function (p) {
                        return p.status !== "pending";
                    }) ? count + 1 : count;
                }, 0);
            };
            $scope.countInteractions = function () {
                if (!$scope.interactions || !$scope.customers) return 0;
                return $scope.interactions.reduce(function (count, interaction) {
                    return $scope.customers.some(function (c) {
                        return c.participations.some(function (p) {
                            return p.interactionId === interaction.id && p.status !== "pending";
                        });
                    }) ? count + 1 : count;
                }, 0);
            };
            $scope.countParticipations = function () {
                if (!$scope.customers) return 0;
                return $scope.customers.reduce(function (count, customer) {
                    if (!customer.participations) return count;
                    return customer.participations.filter(function (p) {
                        return p.status !== "pending";
                    }).length + count;
                }, 0);
            };

            $scope.compareParticipations = function (p1, p2) {
                var d1 = !p1.participatedAt ? 0 : new Date(p1.participatedAt).getTime();
                var s1 = p1.status === 'pending' ? '1' : '0';
                var d2 = !p2.participatedAt ? 0 : new Date(p2.participatedAt).getTime();
                var s2 = p2.status === 'pending' ? '1' : '0';
                var rank1 = parseInt(s1 + d1);
                var rank2 = parseInt(s2 + d2);
                return rank2 - rank1;
            };

            $scope.getLastParticipation = function (customer) {
                var sorted = customer.participations.slice();
                sorted.sort($scope.compareParticipations);
                var participation = sorted.find(function (p) {
                    return p.status !== 'pending';
                });
                if (!participation) return sorted[0];
                return participation;
            };
            $scope.sortCustomers = function (customer) {
                var participation = $scope.getLastParticipation(customer);
                if (!participation || participation.status === 'pending') return -Infinity;
                var d = !participation.participatedAt ? 0 : new Date(participation.participatedAt).getTime();
                return -d;
            };
            $scope.sortParticipations = function (participation) {
                var d = participation.participatedAt ? 0 : participation.participatedAt.getTime();
                var p = participation.status === 'pending' ? '0' : '1';
                return parseInt('-' + p + d);
            };

            $scope.interactions = [];
            $http.get('/api/interactions/list')
                .success(function (response) {
                    if (response.success) {
                        $scope.interactions = response.data;
                        // $scope.interactions.forEach(function (interaction) {
                        // });
                        // console.log($scope.interactions)
                        var interactionId = window.regitGlobal.interactionId;
                        $scope.interaction = {
                            id: interactionId
                        };
                        if (interactionId) {
                            $scope.selectInteraction($scope.interaction);
                        } else {
                            $scope.selectAllInteractions();
                        }
                    } else {
                        console.log(response);
                    }
                }).error(function (response) {
                console.log(response);
            });
            $scope.loadCustomers = function (interaction) {
                function formatUserData(participation) {
                    participation.userData.forEach(function (userData) {
                        var field = participation.fields.find(function (f) {
                            return userData.path === f.jsPath;
                        });
                        if (field) {
                            var value = userData.value;
                            var iField = participation.interaction.fields.find(function (f) {
                                return f.path === field.jsPath;
                            });

                            switch (field.type) {
                                case 'range':
                                    var ranges = iField.options;
                                    var index = parseInt(value);
                                    value = ranges[index][0] + '...' + ranges[index][1];
                                    break;
                            }
                            field.value = value;
                        }
                    });
                }

                $scope.view.customersLoaded = false;
                if (interaction && interaction.type === 'broadcast') return;
                var url = !interaction ? '/api/interactions/customers' : '/api/interactions/participants/' + interaction.id;
                $http.get(url)
                    .success(function (response) {
                        if (response.success) {
                            $scope.customers = [];
                            response.data.forEach(function (customer, index) {
                                if (!customer.profile || !customer.participations.length) return;
                                customer.id = customer.profile ? customer.profile.id : index;
                                var participations = [];
                                customer.participations.forEach(function (participation) {
                                    if (!interaction) {
                                        var inter = $scope.interactions.find(function (i) {
                                            return i.id === participation.interactionId;
                                        });
                                        if (!inter) return;
                                    } else {
                                        inter = interaction;
                                    }
                                    participation.interaction = inter;

                                    if (!inter.fields || !inter.groups) {
                                        $http.get('/api/interactions/get/' + inter.id)
                                            .success(function (response) {
                                                if (response.success) {
                                                    inter.fields = response.data.fields;
                                                    inter.groups = response.data.groups;
                                                    formatUserData(participation);
                                                } else {
                                                    console.log(response);
                                                }
                                            }).error(function (response) {
                                            console.log('Error loading interaction ' + inter.id + ' ' + inter.name, response);
                                        });
                                    } else {
                                        formatUserData(participation);
                                    }

                                    participation.view = {
                                        expanded: false
                                    };
                                    participations.push(participation);
                                });
                                customer.participations = participations;

                                customer.view = {
                                    expanded: false
                                };
                                $scope.customers.push(customer);
                            });
                            // console.log($scope.customers)
                            $scope.view.customersLoaded = true;
                        } else {
                            console.log(response);
                        }
                    })
                    .error(function (response) {
                        console.log(response);
                    });
            };
            $scope.selectAllInteractions = function () {
                $scope.view.allCustomers = true;
                $scope.view.interactionLoaded = false;
                $scope.loadCustomers(null);
            };
            $scope.selectInteraction = function (interaction) {
                $http.get('/api/interactions/get/' + interaction.id)
                    .success(function (response) {
                        if (response.success) {
                            $scope.clearCustomersQuery();
                            $scope.resetInteractionSelector();
                            $scope.view.allCustomers = false;
                            $scope.view.interactionLoaded = true;
                            $scope.interaction = response.data;
                            $scope.interaction.type = $scope.interaction.type.toLowerCase();
                            if ($scope.interaction.groups)
                                $scope.interaction.fields.forEach(function (field) {
                                    var group = $scope.interaction.groups.find(function (g) {
                                        return g.name === field.group;
                                    });
                                    if (group) {
                                        if (!angular.isArray(group.fields)) group.fields = [];
                                        group.fields.push(field);
                                    }
                                });
                            $scope.loadCustomers(interaction);
                        } else {
                            console.log(response);
                            //$scope.openInteractionSelector();
                        }
                    })
                    .error(function (response) {
                        console.log(response);
                    });

            };

            $scope.closeAddCustomer = function () {
                $scope.view.addCustomerShowing = false;
            };
            $scope.toggleAddCustomer = function () {
                $scope.view.addCustomerShowing = !$scope.view.addCustomerShowing;
            };

            $scope.inviteCustomer = function (user) {
                $http.post('/api/interactions/push', {
                    interactionId: $scope.interaction.id,
                    toAccountId: user.accountId,
                    message: user.message
                })
                    .success(function (response) {
                        if (response.success) {
                            rguNotify.add('Pushed interaction to ' + user.displayName);
                            $scope.closeAddCustomer();

                            // var customer = {
                            //     id: user.id,
                            //     accountId: user.accountId,
                            //     profile: {
                            //         id: user.id,
                            //         displayName: user.displayName,
                            //         avatar: user.avatar
                            //     },
                            //     participations: [
                            //         {
                            //             status: 'pending',
                            //             participatedAt: new Date()
                            //         }
                            //     ],
                            //     fresh: true
                            // };
                            // $scope.customers.push(customer);
                            $scope.loadCustomers($scope.interaction);
                        } else {
                            console.log(response);
                        }
                    }).error(function (response) {
                    console.log(response);
                });
            };
            $scope.$on('interaction:notification', function(evt) {
                $scope.loadCustomers($scope.view.allCustomers ? null : $scope.interaction);
            });
            $scope.resendParticipation = function (participation, customer) {
                $http.post('/api/interactions/push', {
                    interactionId: participation.interaction.id,
                    toAccountId: customer.accountId,
                    message: participation.comment
                })
                    .success(function (response) {
                        if (response.success) {
                            rguNotify.add('Resent invitation to ' + customer.profile.displayName);
                        } else {
                            console.log(response);
                        }
                    }).error(function (response) {
                    console.log(response);
                });
            };

            $scope.removeParticipation = function (participation, customer) {
                function terminate() {
                    $http.post('/api/interactions/unregister', {
                        interactionId: participation.interaction.id,
                        fromAccountId: customer.accountId
                    })
                        .success(function (response) {
                            if (response.success) {
                                rguNotify.add( (participation.status==='pending' ?
                                    'Removed invitation to ' : 'Removed registration by ') + customer.profile.displayName);
                                participation.terminated = true;
                                $scope.closeAddCustomer();
                                // var index = customer.participations.findIndex(function(p) {
                                //     return p.interaction.id === participation.interaction.id;
                                // });
                                // if (index !== -1) {
                                //     customer.participations.splice(index,1);
                                // }
                                $scope.loadCustomers($scope.interaction);
                            } else {
                                console.log(response);
                            }
                        }).error(function (response) {
                        console.log(response);
                    });
                }
                if (participation.status === 'pending') {
                    terminate();
                } else {
                    rguAlert('<p>Are you sure to remove <strong>' + customer.profile.displayName + '</strong>\'s registration from<br>"'
                        + participation.interaction.name + '"?</p>'
                        + 'All user data will be lost.', {
                        style: 'delete',
                        actions: 'yes'
                    }, terminate);
                }
            };

        }])
    .filter('participatedDate', function ($filter) {
        return function (participation) {
            if (!participation) return '';
            if (participation.status === 'pending') return 'Pending';
            if (!participation.participatedAt) return '';
            var participatedAt = new Date(participation.participatedAt);
            if (participatedAt.getFullYear() < 1900) return '';
            return $filter('date')(participatedAt, 'dd MMM yyyy');
        };
    })
    .filter('fieldValue', function () {
        return function (field) {
            var value = field.value;
            if (value === null || value === undefined) return '';
            switch (field.type) {
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
                case 'doc':
                    return '';
                    if (!angular.isArray(value) || !value.length) value = '';
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

                    value = html;
                    break;

            }
            return value;
        };
    })
    .filter('filterInteractionsByQuery', function () {
        return function (interactions, query) {
            return interactions.filter(function (interaction) {
                return new RegExp(query, 'i').test(interaction.name);
            });

        };
    })
    .filter('filterCustomersByStatus', function () {
        return function (customers, filter, allCustomers) {
            if (filter === 'all') return customers;
            if (allCustomers) {
                return customers.filter(function (customer) {
                    return customer.participations.some(function (p) {
                        return p.status === filter;
                    });
                });
            }
            return customers.filter(function (customer) {
                return filter === 'active' ? customer.participations[0].status !== 'pending' : customer.participations[0].status === 'pending';
            });
        };
    })
    .filter('filterParticipationsByStatus', function () {
        return function (participations, filter) {
            if (filter === 'all') return participations;
            return participations.filter(function (p) {
                return filter === 'active' ? p.status !== 'pending' : p.status === 'pending';
            });
        };
    })
    .filter('filterCustomersByQuery', function ($filter) {
        return function (customers, query) {
            var rgx = new RegExp(query, 'i');
            return customers.filter(function (customer) {
                if (customer.profile && rgx.test(customer.profile.displayName)) return true;
                return customer.participations.some(function (p) {
                    if (!p.fields) return false;
                    return p.fields.some(function (f) {
                        var value = $filter('fieldValue')(f);
                        var matched = rgx.test(value);
                        if (matched && p.view) p.view.expanded = customer.view.expanded = true;
                        return matched;
                    });
                });
            });

        };
    })
    .filter('filterByGroup', function () {
        return function (fields, groupName) {
            return fields.filter(function (field) {
                return field.group === groupName;
            });

        };
    })
    .filter('filterUserFieldsByGroup', function () {
        return function (fields, groupName, iFields) {
            if (!fields) return [];
            return fields.filter(function (field) {
                var iField = iFields.find(function (f) {
                    return f.path === field.jsPath;
                });
                if (!iField) return false;
                return iField.group === groupName;
            });

        };
    })
    .directive('interactionSelector', function ($timeout) {
            return {
                restrict: 'C',
                link: function (scope, el, attrs) {
                    el.bind("keydown keypress", function (event) {
                        if (event.which === 40) {
                            scope.$apply(function () {
                                scope.hoverNextInteraction(true);
                            });
                            event.preventDefault();
                        }
                        else if (event.which === 38) {
                            scope.$apply(function () {
                                scope.hoverNextInteraction(false);
                            });
                            event.preventDefault();
                        }
                    });
                }
            };
        }
    )
    .directive('interactionSelectorInput', function ($timeout) {
            return {
                restrict: 'C',
                link: function (scope, el, attrs) {
                    /*el.bind("keydown keypress", function(event) {
                        if(event.which === 13) {
                            scope.$apply(function(){
                                scope.$eval(attrs.ngEnter);
                            });
                            event.preventDefault();
                        }
                    });*/
                    scope.$watch('view.interactionSelectorShowing', function (value) {
                        if (value === true) {
                            $timeout(function () {
                                el[0].focus();
                            });
                        }
                    });
                }
            };
        }
    );