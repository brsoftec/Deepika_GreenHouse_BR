var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule'], true);
myApp.getController('AnalyticsCtrl',
    ['$scope', '$rootScope', '$http', '$timeout', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'NotificationService', 'CampaignService', 'CommonService', '$moment', 'analyticsService', 'BusinessAccountService',
        function ($scope, $rootScope, $http, $timeout, userManager, sweetAlert, authService, alertService, notificationService, CampaignService, CommonService, $moment, analyticsService, BusinessAccountService) {

            $scope.interaction = {
                id: CommonService.GetQuerystring("campaignid"),
                name: CommonService.GetQuerystring("CampaignNameValue")
            };
            $scope.master = false;
            $scope.view = {
                selectedView: 'Current Month',
                showKeywords: false,
                showUnknownGender: true,
                exportOption: 'Export current view'
            };

            $scope.search = {
                query: ''
            };
            $scope.matchedItems = [];
            $scope.onSearch = function () {
                var query = $scope.search.query;
                if (!query.length) {
                    $scope.matchedItems = $scope.participators;
                    return;
                }
                var re = new RegExp(query, 'gi');
                $scope.matchedItems = [];
                angular.forEach($scope.participators, function (user) {
                    var matched = false;
                    angular.forEach(user.fields, function (field) {
                        if (re.test(field.value)) {
                            matched = true;
                        }
                    });
                    if (matched) {
                        $scope.matchedItems.push(user);
                    }
                });
                $scope.updatePaging();
            };
            $scope.filterUsers = function (query) {
                var re = new RegExp(query, 'gi');
                return function (user) {
                    if (!query) return true;
                    angular.forEach(user, function (prop) {
                        if (re.test(user[prop]))
                            return true;
                    });
                    return false;
                };
            };

            //  PAGINATION
            $scope.paging = {
                pageSize: 20,
                currentPage: 1,
                totalItems: 0
            };

            $scope.updatePaging = function () {
                if (!$scope.search.query.length) {
                    $scope.matchedItems = $scope.participators;
                }
                $scope.paging.totalItems = $scope.matchedItems.length;
            };
            $scope.setPage = function (pageNo) {
                $scope.paging.currentPage = pageNo;
            };
            $scope.getCurrentItems = function () {
                var start = ($scope.paging.currentPage - 1) * $scope.paging.pageSize;
                var end = Math.min($scope.matchedItems.length, start + $scope.paging.pageSize);
                return $scope.matchedItems.slice(start, end);
            };

            $scope.onPageChanged = function () {
            };

            Highcharts.setOptions({
                chart: {
                    style: {
                        fontFamily: 'Roboto, "Helvetica Neue", Helvetica, Arial, Geneva, sans-serif'
                    }
                }
            });

            // Get Campaign Fields
            //if ($scope.master ? $scope.business.id : $scope.interaction.id != "") {
            //    CampaignService.GetCampaignById($scope.master ? $scope.business.id : $scope.interaction.id).then(function (response) {
            //        $scope.campaignFields = response.Campaign.campaign.fields;
            //    }, function (errors) {

            //    });
            //}

            //var apiUrl = "/api/BusinessUserSystemService/GetChartDataByCampaign";
            //var apiUrlBus = "/api/BusinessUserSystemService/GetChartDataByBus";
            var start = $moment("20160101", "YYYYMMDD").toDate();

            $http.get('/api/interactions/get/' + $scope.interaction.id)
                .success(function (response) {
                    if (response.success) {
                        $scope.fields = response.data.fields;
                        $scope.InitCampaign();
                        $scope.getUser();
                    } else {
                    console.log(response)}
                }).error(function(response) {
                    console.log(response);
            });

/*
            $http.get('/api/Interaction/Get', {
                params: {
                    interactionId: $scope.interaction.id
                }
            })
                .success(function (response) {
                    if (!response.success) {
                        console.log('Error getting interaction details', response);
                        return;
                    }
                    $scope.InitCampaign();
                    $scope.getUser();
                }).error(function (response) {
                console.log('Error getting interaction info', response);
            });*/

            // $scope.loadProfile = function ()
            $scope.InitCampaign = function () {
                $http.get('/api/CampaignService/CampaignById?campaignId=' + $scope.interaction.id)

                    .success(function (response) {
                        $scope.campaignFields = response.Fields;

                    }, function (errors) {
                        __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
                    })
                ///

            }


            $scope.getUser = function () {
                analyticsService.getUsers($scope.master ? $scope.business.id : $scope.interaction.id, start, $scope.master)
                    .then(function (users) {

                            $scope.users = users;
                            $scope.dataLoaded = true;
                            $scope.participatorsWeek = [];
                            var startWeek = $moment().subtract(1, 'week');
                            $scope.participantsMonth = [];
                            var startMonth = $moment().subtract(1, 'month');
                            $.each($scope.users, function (index, user) {
                                user.moment = $moment(user.datefollow);
                                user.followed = user.moment.toDate();
                                user.days = Math.floor(user.moment.unix() / (3600 * 24));
                                if (!user.age && (user.dob_vault || user.dob)) {
                                    var dob = $moment(user.dob_vault || user.dob, 'YYYY-MM-DD');
                                    user.age = $moment().diff(dob, 'years');
                                    user.dob = dob.toDate();
                                }
                                user.firstname = user.firstname || user.firstname_vault;
                                user.lastname = user.lastname || user.lastname_vault;
                                user.country = user.country || user.country_vault;
                                user.city = user.city || user.city_vault;
                                var fields = [];
                                angular.forEach(user.ListFieldsRegis, function (field, index) {

                                    if (field != null) {
                                        switch (field.type) {
                                            case "doc":
                                                var listdocs = [];
                                                $(field.modelarrays).each(function (index, namedoc) {
                                                    listdocs.push({
                                                        name: namedoc,
                                                        path: field.pathfile + '/' + namedoc
                                                    });
                                                })
                                                fields.push({
                                                    label: field.displayName,
                                                    value: field.model,
                                                    listdocs: listdocs,
                                                    type: "doc",
                                                    jsPath: field.jsPath
                                                });
                                                break;

                                            case 'range':
                                                var iField = $scope.fields.find(function (f) {
                                                    return f.path === field.jsPath;
                                                });
                                                var ranges = iField.options;
                                                var rangeIndex = parseInt(field.model);
                                                var range = ranges[rangeIndex];
                                                var text = range[0] + '...' + range[1];
                                                fields.push({
                                                    label: field.displayName,
                                                    value: text,
                                                    type: "range",
                                                    jsPath: field.jsPath
                                                });
                                                break;

                                            default:
                                                fields.push({
                                                    label: field.displayName,
                                                    value: field.model,
                                                    jsPath: field.jsPath
                                                });
                                                break;
                                        }
                                    } else {
                                        fields.push({
                                            label: $scope.campaignFields[index].displayName,
                                            value: "",
                                            jsPath: $scope.campaignFields[index].jsPath
                                        });
                                    }
                                });
                                fields.push({
                                    label: "Registration Date",
                                    value: user.datefollow,
                                    jsPath: ""
                                });
                                user.fields = fields;
                                if (user.moment.isAfter(startWeek)) {
                                    $scope.participatorsWeek.push(user);
                                }
                                if (user.moment.isAfter(startMonth)) {
                                    $scope.participantsMonth.push(user);
                                }
                            });
                            $scope.updateChart();
                            $scope.updatePaging();
                        }

                        , function (reason) {
                            $scope.users = null;
                            $scope.dataLoaded = true;
                        });
            }


            //  $scope.InitUser();
            $scope.updateChart = function () {

                if (!$scope.users) return;

                // Showing data (Current Week | Current Month | All)
                var range;
                if ($scope.view.selectedView === 'Current Week') {
                    range = 'week';
                    $scope.participators = $scope.participatorsWeek;
                }
                else if ($scope.view.selectedView === 'Current Month') {
                    range = 'month';
                    $scope.participators = $scope.participantsMonth;
                }
                else {
                    range = 'all';
                    $scope.participators = $scope.users;
                }

                // Clear search query
                $scope.search.query = '';

                //  Populate data on date
                var moments = [];
                $.each($scope.participators, function (index, user) {
                    moments.push(user.moment);
                });
                var from = $moment.min(moments), to = $moment.max(moments);
                var fromDay = Math.floor(from.unix() / (3600 * 24)), toDay = Math.floor(to.unix() / (3600 * 24));
                var numDays = toDay - fromDay + 1;
                var participatorsByDay = [];
                for (var day = 0; day < numDays; day++) {
                    participatorsByDay[day] = [];
                }
                $.each($scope.participators, function (index, follower) {
                    var day = follower.days - fromDay;
                    participatorsByDay[day].push(follower);
                });

                var menByDay = [], womenByDay = [], unknownByDay = [];
                for (day = 0; day < numDays; day++) {
                    menByDay[day] = 0;
                    womenByDay[day] = 0;
                    unknownByDay[day] = 0;
                }
                $.each(participatorsByDay, function (day, participators) {
                    $.each(participators, function (index, follower) {
                        if (follower.gender === 'Male') {
                            menByDay[day]++;
                        } else if (follower.gender === 'Female') {
                            womenByDay[day]++;
                        } else {
                            unknownByDay[day]++;
                        }
                    });
                });

                // Populate data on age ranges
                var ageRanges = [17, 24, 34, 44, 54, 64, 200];
                var menByAge = [], womenByAge = [], unknownByAge = [];
                for (i = 0; i < ageRanges.length; i++) {
                    menByAge[i] = 0;
                    womenByAge[i] = 0;
                    unknownByAge[i] = 0;
                }
                $.each($scope.participators, function (index, follower) {
                    var age = follower.age;
                    $.each(ageRanges, function (index, range) {
                        if (age <= range) {
                            if (follower.gender === 'Male') {
                                menByAge[index]++;
                            } else if (follower.gender === 'Female') {
                                womenByAge[index]++;
                            } else {
                                unknownByAge[index]++;
                            }
                            return false;
                        }
                    });
                });
                //  Extract data on location
                var countries = [];
                angular.forEach($scope.participators, function (follower, index) {
                    var country = follower.country;
                    var city = follower.city;
                    if (!countries.hasOwnProperty(country)) {
                        countries[country] = {
                            count: 1,
                            cities: []
                        }
                    } else {
                        countries[country].count++;
                    }
                    if (!countries[country].cities.hasOwnProperty(city)) {
                        countries[country].cities[city] = 1;
                    } else {
                        countries[country].cities[city]++;
                    }
                });

                countries.sort(function (a, b) {
                    return b - a;
                });
                $scope.countries = $.extend({}, countries);
                $scope.currentCities = [];
                $scope.showCitiesFromCountry = function (country) {
                    $scope.currentCities = $.extend({}, $scope.countries[country].cities);
                };

                var series = [
                    {
                        name: 'Women',
                        data: womenByDay,
                        pointStart: from.valueOf(),
                        pointIntervalUnit: 'day'
                    }, {
                        name: 'Men',
                        data: menByDay,
                        pointStart: from.valueOf(),
                        pointIntervalUnit: 'day',
                        color: '#0084b6'
                    }
                ];
                if ($scope.view.showUnknownGender) {
                    series.push({
                        name: 'Unknown',
                        data: unknownByDay,
                        pointStart: from.valueOf(),
                        pointIntervalUnit: 'day'
                    });
                }

                $scope.chartConfig = {
                    title: {
                        text: 'Interaction Participants'
                    },
                    subtitle: {
                        text: $scope.view.selectedView
                    },
                    xAxis: {
                        type: 'datetime'
                    },
                    yAxis: {
                        title: {text: 'Number of Participants'},
                        allowDecimals: false
                    },
                    options: {
                        chart: {
                            type: 'areaspline'
                        }
                    },
                    series: series,
                    exporting: {
                        buttons: {
                            contextButton: {
                                menuItems: [{
                                    text: 'Export to PNG',
                                    onclick: function () {
                                        this.exportChartLocal({
                                            filename: 'chart-participants.pdf'
                                        });
                                    }
                                }, {
                                    text: 'Export to PDF',
                                    onclick: function () {
                                        this.exportChart({
                                            type: 'application/pdf',
                                            filename: 'chart-participants.pdf'
                                        });
                                    },
                                    separator: false
                                }]
                            }
                        }
                    }
                };
                var ageSeries = [{
                    name: 'Women',
                    data: womenByAge
                }, {
                    name: 'Men',
                    data: menByAge
                }];
                if ($scope.view.showUnknownGender) {
                    ageSeries.push(
                        {
                            name: 'Unknown',
                            data: unknownByAge
                        });
                }
                $scope.agesChartConfig = {
                    title: {
                        text: 'Participants by Age'
                    },
                    subtitle: {
                        text: $scope.view.selectedView
                    },
                    options: {

                        chart: {
                            type: 'column'
                        }

                    },
                    plotOptions: {
                        column: {
                            dataLabels: {
                                enabled: true
                            }
                        },
                        series: {
                            dataLabels: {
                                enabled: true,
                                formatter: function () {
                                    var pcnt = (this.y / $scope.participators.length) * 100;
                                    return pcnt + '%23';
                                }
                            }
                        }
                    },

                    series: ageSeries,

                    xAxis: {
                        title: {text: 'Age'},
                        categories: ['1-17', '18-24', '25-34', '35-44', '45-54', '55-64', '65+']

                    },
                    yAxis: {
                        title: {text: 'Participants'},
                        labels: {
                            formatter: function () {
                                var pcnt = Math.round(this.value / $scope.participators.length * 100);
                                return pcnt + '%';
                            }
                        },
                        allowDecimals: false,
                        size: {
                            height: 300
                        },
                        func: function (chart) {
                        }
                    }

                };

                $scope.tags = [];
                $scope.$watchCollection('view.showUnknownGender', function (newVal, oldVal) {
                    if (newVal !== oldVal) {
                        $timeout(function () {
                            $scope.updateChart();
                        });
                    }
                });
            };
            $scope.exportData = function () {

                html2canvas(document.getElementById('interaction-analytics-charts')).then(function (canvas) {
                    $timeout(function () {
                        var img = canvas.toDataURL('image/png');
                        var doc = new jsPDF('p', 'px', 'a4');
                        doc.addImage(img, 'PNG', 20, 20);
                        doc.save('interaction-analytics.pdf');
                    });

                });

            };

            function convertCamelCase(text) {
                var result = text.replace(/([a-z])([A-Z])/g, "$1 $2");
                var finalResult = result.charAt(0).toUpperCase() + result.slice(1);
                return finalResult;
            };

            $scope.exportexcelData = function () {
                var items = [];
                $($scope.participators).each(function (index, object) {
                    var item = new Object();
                    $(object.fields).each(function (index1, object1) {
                        {
                            var jsonpath = object1.jsPath;
                            if (!jsonpath) return;
                            var arraygroup = jsonpath.split('.');
                            var group = "";
                            if (arraygroup.length == 3)
                                group = convertCamelCase(arraygroup[1]) + " - ";
                            if (arraygroup.length == 4)
                                group = convertCamelCase(arraygroup[2]) + " - ";
                            switch (object1.type) {
                                case 'doc': {
                                    var fullhostname = $(location).attr('host');
                                    var valuepathdoc = "";
                                    $(object1.listdocs).each(function (index2, doc) {
                                        valuepathdoc += " " + fullhostname + doc.path;
                                    });
                                    item[group + object1.label] = valuepathdoc
                                }
                                    break;
                                default:
                                    item[group + object1.label] = object1.value;
                                    break;

                            }
                        }

                    });
                    items.push(item);
                })

                if (items.length > 0) {
                    var opts = {
                        headers: true,
                        column: {style: {Font: {Bold: "1"}}},

                        //cells: {
                        //    1: {
                        //        1: {
                        //            style: { Font: { Color: "#00FFFF" } }
                        //        }
                        //    }
                        //}
                    };
                    var fileName = $scope.interaction.name + " - Participants.xlsx";
                    // alasql('SELECT * INTO xlsx("ExportAllHandshakes.xlsx",?) FROM ?', [opts, items]);
                    alasql('SELECT * INTO xlsx("' + fileName + '",?) FROM ?', [opts, items]);
                }


            }

        }
    ]);
