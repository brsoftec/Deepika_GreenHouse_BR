angular.module('calendar', ['angular-momentjs'])

    .config(function () {
    })
    .factory('calendarService', function ($moment,$http,$q,CommonService) {
       
        //  SERVICE FUNCTIONS   //
        return {
            utcOffsetText: function () {
                var offset = Math.round($moment().utcOffset() / 60);
                return 'GMT ' + (offset >= 0 ? '+' : '-') + offset;
            },
            isToday: function (day) {
                return $moment().isSame(day, 'day');
            },
            getDayHours: function () {
                var moment = $moment();
                var hours = [];
                for (var hour = 0; hour < 24; hour++) {
                    moment.hour(hour).minute(0);
                    hours.push(moment.format('hh:mm a'));
                }
                return hours;
            },
            //      EVENTS
            getAllEvents: function () {
                var deferer = $q.defer();
                $http.post('/api/EventService/GetEventsByUser', null)
              .success(function (response) {
                  var events = [];
                  $(response.Listitems).each(function (index,item) {
                      var datestart = new Date(item.startdate);                    
                      var datestarttime = new Date(item.starttime);
                      var start = $moment([datestart.getFullYear(), datestart.getMonth(), datestart.getDate(), datestarttime.getHours(), datestarttime.getMinutes()]);                 
                      var dateend = new Date(item.enddate);                    
                      var dateendtime = new Date(item.endtime);
                      var end = $moment([dateend.getFullYear(), dateend.getMonth(), dateend.getDate(), dateendtime.getHours(), dateendtime.getMinutes()]);
                      var event = {
                          id: item.id,
                          source: item.username,
                          summary: item.name,
                          description: item.description,
                          location: item.location,
                          theme: item.theme,
                          organizer: item.type,
                          note: item.note,
                          duration: item.timetype,
                          start: {
                              date: start.toDate(),
                              dateTime: start.toDate()
                          },
                          end: {
                              date: end.toDate(),
                              dateTime: end.toDate()
                          }
                      };
                      events.push(event);
                  });
                  deferer.resolve(events);
              }).error(function (errors, status) {
                  __promiseHandler.Error(errors, status, deferer);
              })

               return deferer.promise;
              ///  return events;
            },
            getEventsByDay: function (day, events) {
                day = $moment(day);
                var dayEvents = [];
                angular.forEach(events, function (event) {
                    if (day.isSame(event.start.date, 'day')) {
                        dayEvents.push(event);
                    }
                });
                return dayEvents;
            },
            getEventsByHour: function (day,hour,events) {
                var time = $moment(day).hour(hour);
                var hourEvents = [];
                angular.forEach(events, function (event) {
                    if (day.isSame(event.start.date, 'day') && time.isSame(event.start.date, 'hour')) {
                        hourEvents.push(event);
                    }
                });
                return hourEvents;
            },
            
            getDayfromquery: function () {
               
                var date = CommonService.GetQuerystring("date");
                var month = CommonService.GetQuerystring("month");
                var year = CommonService.GetQuerystring("year");
                if (date != "" && date != null && date != undefined)
                    return $moment([year, month, date]);
                else
                   return $moment();
            }
            ,

            getEventById: function (id) {
                var deferer = $q.defer();
                var eventmodel = new Object();
                eventmodel.EventId = id;
                $http.post('/api/EventService/GetEventById', eventmodel)
              .success(function (response) {
               
                  deferer.resolve(response);
              }).error(function (errors, status) {
                  __promiseHandler.Error(errors, status, deferer);
              })

                return deferer.promise;
               
               
               // return events[id];
            },
            deleteEvent:function (eventid)
            {
                var deferer = $q.defer();
                var eventmodel = new Object();
                eventmodel.EventId = eventid;
                $http.post('/api/EventService/DeleteEvent', eventmodel)
              .success(function (response) {
                  
                    //  window.location.href = '/EventCalendar/Calendar';

                  deferer.resolve(response);
              }).error(function (errors, status) {

              })

                return deferer.promise;
               
            },
            saveEvent: function (event,eventid,eventreal) {
                var deferer = $q.defer();
                var eventmodel = new Object();
                if (eventid == undefined || eventid == null || eventid == "") {
                    var stringevent = JSON.stringify(event);
                    eventmodel.StrEvent = stringevent;
                }
                else {
                    eventmodel.EventId = eventid;
                    var stringevent1 = JSON.stringify(eventreal);
                    eventmodel.StrEvent = stringevent1;
                }
                $http.post('/api/EventService/SaveEvent', eventmodel)
              .success(function (response) {

                  if (event.detail.startdate != "" && event.detail.starttime != "") {
                      var datestart = new Date(event.detail.startdate);
                      var datestarttime = new Date(event.detail.starttime);
                      var start = $moment([datestart.getFullYear(), datestart.getMonth(), datestart.getDate(), datestarttime.getHours(), datestarttime.getMinutes()]);

                      var date = start.date();
                      var month = start.month();
                      var year = start.year();
                      window.location.href = '/EventCalendar/Calendar?year=' + year + "&month=" + month + "&date=" + date;
                  }
                  else
                      window.location.href = '/EventCalendar/Calendar';

                  deferer.resolve(response);
              }).error(function (errors, status) {
                 
              })

                return deferer.promise;

                //  Save to backend
            }
        };
    })
    .filter('fullDate', function ($moment) {
        return function (date) {
            return $moment(date).format('dddd, MMMM DD YYYY');
        }
    })
        .filter('formDate', function ($moment) {
            return function (date) {
                return $moment(date).format('DD MMMM YYYY');
            }
        })
    .filter('fullTime', function ($moment) {
        return function (time) {
            return $moment(time).format('hh:mm a');
        }
    })
    .filter('shortTime', function ($moment) {
        return function (time) {
            return $moment(time).format('h:m');
        }
    })
    .filter('dayCell', function () {
        return function (day) {
            return day.date();
        }
    })
    .filter('monthHeader', function ($sce) {
        return function (month) {
            var html = '<span class="month-name">' + month.format('MMMM') + '</span> ' + month.format('YYYY');
            return $sce.trustAsHtml(html);
        }
    })
    .filter('weekDayHeader', function () {
        return function (moment) {
            return moment.format('ddd, DD MMM');
        }
    })
    .directive('miniCalendar', function ($rootScope, $location, $moment, calendarService) {
        return {
            restrict: 'EAC',
            templateUrl: '/Areas/User/Views/EventCalendar/templates/mini-calendar.html',
            link: function (scope, elem, attrs) {
                scope.monthInitials = ['S', 'M', 'T', 'W', 'T', 'F', 'S'];
                scope.utcOffsetText = calendarService.utcOffsetText();
                scope.isToday = calendarService.isToday;
                scope.showingDayCellPopup = false;
              //  scope.$on('$routeChangeSuccess', function (e, current, pre) {
                 scope.inCalendar = (window.location.href .search('EventCalendar/Calendar')>=0);
              //  });


                scope.isActiveDay = function (day) {
                    return day.isSame(scope.activeDay, 'day');
                };
                scope.notInActiveMonth = function (moment) {
                    return !moment.isSame(scope.activeMonth, 'month');
                };
                scope.gotoDay = function (day) {
                    scope.activeDay = day;
                    $rootScope.activeDay = day;
                    $rootScope.$broadcast('calendarChangedActiveDay');
                    return false;
                };
                scope.gotoMonth = function (month) {
                    if (month === 'prev') {
                        scope.activeMonth.subtract(1, 'month');
                    }
                    else if (month === 'next') {
                        scope.activeMonth.add(1, 'month');
                    }
                    scope.updateMonthCalendar();
                };
                scope.gotoCalendar = function (day) {
                    var date = scope.activeDay.date();
                    var month = scope.activeDay.month();
                    var year = scope.activeDay.year();
                    window.location.href = '/EventCalendar/Calendar?year=' + year + "&month=" + month+"&date="+date;
                    scope.showingDayCellPopup = false;
                    return false;
                };
            },
            controller: function ($scope, calendarService) {
                $scope.activeDay = calendarService.getDayfromquery();
                $scope.activeMonth = calendarService.getDayfromquery();
                $scope.updateMonthCalendar = function () {
                    var monthStart = $moment($scope.activeMonth).startOf('month');
                    var monthEnd = $moment($scope.activeMonth).endOf('month');
                    var viewStart = monthStart;
                    viewStart.startOf('week').isoWeekday();
                    var viewEnd = monthEnd;
                    viewEnd.endOf('week').isoWeekday();
                    $scope.weeks = [];
                    var week = [];
                    for (var day = viewStart, count = 0; day.isSameOrBefore(viewEnd, 'day') ; day.add(1, 'day')) {
                        var moment = $moment(day);
                        moment.events = calendarService.getEventsByDay(day,$rootScope.events);
                        moment.hasEvents = !!moment.events.length;
                        week.push(moment);
                        if (count % 7 == 6) {
                            $scope.weeks.push(week);
                            week = [];
                        }
                        count++;
                    }
                };
                $scope.load = function () { 
                    calendarService.getAllEvents().then(function (events) {
                        $rootScope.events = events;
                        $scope.updateMonthCalendar();
                    }, function (errors) {
                    });
                }


                if ($rootScope.events == null || $rootScope.events == undefined || $rootScope.events.length > 0)
                    $scope.load();
                else
                {
                    $rootScope.events = events;
                    $scope.updateMonthCalendar();
                }

                $scope.$on('reloadminicalendar', function () {
                    $scope.load();
                });
            }
        };
    });




