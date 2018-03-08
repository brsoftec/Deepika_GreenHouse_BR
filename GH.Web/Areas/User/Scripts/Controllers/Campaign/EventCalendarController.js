
//angular.module('myApp', ['ui.rCalendar', 'kendo.directives']);

//angular.module('myApp').controller('CalendarDemoCtrl', ['$scope', function ($scope) {
var myApp = getApp('myApp', ['SocialModule', 'oitozero.ngSweetAlert', 'UserModule', 'BusinessAccountModule', 'ui.rCalendar'],true,true);
//var myApp = angular.module("myApp");

//'$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'CommonService',
myApp.getController('EventCalendarController', function ($scope) {

    $scope.initializeController = function() {

        $scope.alerts = [];
        $scope.messageBox = "";
        

        $scope.changeMode = function(mode) {
            $scope.mode = mode;
            console.log($scope.mode);
        };

        $scope.today = function() {
            $scope.currentDate = new Date();
        };

        $scope.isToday = function() {
            var today = new Date(),
                currentCalendarDate = new Date($scope.currentDate);

            today.setHours(0, 0, 0, 0);
            currentCalendarDate.setHours(0, 0, 0, 0);
            return today.getTime() === currentCalendarDate.getTime();
        };

        $scope.loadEvents = function() {
            $scope.eventSource = createRandomEvents();
        };

        $scope.onEventSelected = function(event) {
            $scope.event = event;
        };

        $scope.onTimeSelected = function(selectedTime) {
            console.log('Selected time: ' + selectedTime);
        };

        function createRandomEvents() {
            var events = [];
            for (var i = 0; i < 50; i += 1) {
                var date = new Date();
                var eventType = Math.floor(Math.random() * 2);
                var startDay = Math.floor(Math.random() * 90) - 45;
                var endDay = Math.floor(Math.random() * 2) + startDay;
                var startTime;
                var endTime;
                if (eventType === 0) {
                    startTime = new Date(Date.UTC(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate() + startDay));
                    if (endDay === startDay) {
                        endDay += 1;
                    }
                    endTime = new Date(Date.UTC(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate() + endDay));
                    events.push({
                        title: 'All Day Event number ' + i,
                        startTime: startTime,
                        endTime: endTime,
                        allDay: true
                    });
                } else {
                    var startMinute = Math.floor(Math.random() * 24 * 60);
                    var endMinute = Math.floor(Math.random() * 180) + startMinute;
                    startTime = new Date(date.getFullYear(), date.getMonth(), date.getDate() + startDay, 0, date.getMinutes() + startMinute);
                    endTime = new Date(date.getFullYear(), date.getMonth(), date.getDate() + endDay, 0, date.getMinutes() + endMinute);
                    events.push({
                        title: 'Event number ' + i,
                        startTime: startTime,
                        endTime: endTime,
                        allDay: false
                    });
                }
            }
            return events;
        }


        $scope.startDate = new Date();
        $scope.endDate = new Date();
        $scope.maxEndDate = new Date();

        $scope.$watch("startDate", function(val) {
            var maxEndDate = new Date(val);
            maxEndDate.setDate(maxEndDate.getDate() + 14);
            $scope.maxEndDate = maxEndDate;
            delete $scope.endDate;
        });

        $scope.schedulerOptions = {
            date: new Date("2013/6/13"),
            startTime: new Date("2013/6/13 07:00 AM"),
            height: 600,
            views: [
                "day",
                { type: "workWeek", selected: true },
                "week",
                "month"
            ],
            timezone: "Etc/UTC",
            dataSource: {
                batch: true,
                transport: {
                    read: {
                        url: "//demos.telerik.com/kendo-ui/service/tasks",
                        dataType: "jsonp"
                    },
                    update: {
                        url: "//demos.telerik.com/kendo-ui/service/tasks/update",
                        dataType: "jsonp"
                    },
                    create: {
                        url: "//demos.telerik.com/kendo-ui/service/tasks/create",
                        dataType: "jsonp"
                    },
                    destroy: {
                        url: "//demos.telerik.com/kendo-ui/service/tasks/destroy",
                        dataType: "jsonp"
                    },
                    parameterMap: function(options, operation) {
                        if (operation !== "read" && options.models) {
                            return { models: kendo.stringify(options.models) };
                        }
                    }
                },
                schema: {
                    model: {
                        id: "taskId",
                        fields: {
                            taskId: { from: "TaskID", type: "number" },
                            title: { from: "Title", defaultValue: "No title", validation: { required: true } },
                            start: { type: "date", from: "Start" },
                            end: { type: "date", from: "End" },
                            startTimezone: { from: "StartTimezone" },
                            endTimezone: { from: "EndTimezone" },
                            description: { from: "Description" },
                            recurrenceId: { from: "RecurrenceID" },
                            recurrenceRule: { from: "RecurrenceRule" },
                            recurrenceException: { from: "RecurrenceException" },
                            ownerId: { from: "OwnerID", defaultValue: 1 },
                            isAllDay: { type: "boolean", from: "IsAllDay" }
                        }
                    }
                },
                filter: {
                    logic: "or",
                    filters: [
                        { field: "ownerId", operator: "eq", value: 1 },
                        { field: "ownerId", operator: "eq", value: 2 }
                    ]
                }
            },
            resources: [
                {
                    field: "ownerId",
                    title: "Owner",
                    dataSource: [
                        { text: "Alex", value: 1, color: "#f8a398" },
                        { text: "Bob", value: 2, color: "#51a0ed" },
                        { text: "Charlie", value: 3, color: "#56ca85" }
                    ]
                }
            ]
        };


    }
    $scope.initializeController();

});

