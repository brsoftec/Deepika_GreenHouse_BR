
var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule'], true);
myApp.getController("EventEditorCtrl", function ($scope, $rootScope, $moment, calendarService, CommonService, $http) {
    $scope.coords = new Object();
    $scope.event = new Object();

    $scope.eventreal = new Object();
        var eventid = CommonService.GetQuerystring("eventid");
        if (eventid == undefined || eventid == null)
            eventid = "";
        var eventmodel = new Object();
        eventmodel.EventId = eventid;
        $http.post('/api/EventService/GetEventById', eventmodel)
      .success(function (response) {
          if (eventid == "") {
              $scope.event = response.EventTemplate;

              $scope.event.detail.startdate = calendarService.getDayfromquery();
          }
          else {
              $scope.event = response.EventTemplate.Event;
              $scope.eventreal = response.EventTemplate;
          }
          if ($scope.event.latitude != undefined && $scope.event.longitude != undefined) {
               $timeout(function() {
                   $scope.coords.latitude = $scope.event.latitude;
                   $scope.coords.longitude = $scope.event.longitude;
               });
          }
      }).error(function (errors, status) {
         // __promiseHandler.Error(errors, status, deferer);
      })



       

        $scope.closeEvent = function () {
           // $scope.$location.path('/Calendar');
        };
        $scope.saveEvent = function () {

            if ($scope.formcalendar.$invalid) {
                return;
            }
            if ($scope.event.detail.timetype == "One Time")
                $scope.event.detail.enddate = $scope.event.detail.startdate;
            
            $scope.event.latitude=$scope.coords.latitude+"";
            $scope.event.longitude = $scope.coords.longitude+"";
            calendarService.saveEvent($scope.event, eventid, $scope.eventreal);
            $scope.closeEvent();
        };


    });



