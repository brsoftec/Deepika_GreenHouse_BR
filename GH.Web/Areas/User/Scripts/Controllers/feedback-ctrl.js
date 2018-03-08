
var myApp = getApp("myApp", ['SocialModule', 'angularMoment', 'CommonDirectives', 'UserModule', 'oitozero.ngSweetAlert', 'NotificationModule'], true);

myApp.getController("FeedBackController", [
    '$scope', '$rootScope', '$http', '$locale', 'FeedBackService', '$sce', '$q',  function ($scope, $rootScope, $http, $locale, feedBackService, $sce, $q) {
        $scope.init = function () {
         

            $scope.getListFeedBack = [];
            $scope.getListFullFeedBack = [];
         
            
            $scope.constant = {
                //1. url api
                pullUrl: "/api/FeedBackService/GetFeedBack",
                pullUrl2: "/api/FeedBackService/GetFullFeedBack",
            
            };
            //2. call service
      
            $scope.pullFeedBack();
            $scope.pullFullFeedBack();
               
            //
         
        }

        $scope.pullFeedBack = function () {
            var start = $scope.constant.start;
            var take = $scope.constant.take;

            feedBackService.PullFeedBack($scope.constant.pullUrl).then(function (response) {

                for (var i = 0; i < response.length; i++) {
                    $scope.getListFeedBack.push(response[i]);                   
                }            
                   
            });

          
        }

        //getListFullFeedBack
        $scope.pullFullFeedBack = function () {
          
            feedBackService.PullFeedBack($scope.constant.pullUrl2).then(function (response) {

                for (var i = 0; i < response.length; i++) {
                    $scope.getListFullFeedBack.push(response[i]);
                }
              
            });

        }
 
      
        $scope.init();
      
      

        //SON
         $scope.currentView = {
             showingEmails: true,
             showingSystem: false,
             showingBugs: true,
             showingSuggestions: true,

             pageStart: 0,
             pageSize: 20,
             pages: [0]
         }

        $scope.filterFeedbacks = function (feedback) {
        var showing = feedback.Type === 'Bug' && $scope.currentView.showingBugs;
        showing = showing || feedback.Type !== 'Bug' && $scope.currentView.showingSuggestions;
        return showing;
    }

    $scope.initPages = function () {
        var pageCount = Math.ceil($scope.filteredFeedbacks.length / $scope.currentView.pageSize);
        $scope.currentView.pages = [];
        if ($scope.currentView.pageStart >= $scope.filteredFeedbacks.length) {
            $scope.currentView.pageStart = $scope.filteredFeedbacks.length - 1;
        }
        for (i = 1; i <= pageCount; i++) {
            $scope.currentView.pages.push(i);
        }
    };

    $scope.gotoPage = function (page) {
        $scope.currentView.pageStart = page * $scope.currentView.pageSize;
    };

     
    }
]);

myApp.getController("SendFeedBackController", [
    '$scope', '$rootScope', '$http', '$locale', 'FeedBackService', '$sce', '$q', function ($scope, $rootScope, $http, $locale, feedBackService, $sce, $q) {
        $scope.init = function () {
            $scope.locale = $locale.id;
            $scope.feedbackConfig = {
                types: ['Suggestion', 'Feature Request', 'What business you\'d like to see on Regit?', 'Other'],
                components: ['General', 'General (Business)', 'Home Feed', 'Information Vault', 'Interactions', 'Calendar', 'Search', 'Other']
            };

            //  $scope.pullFullFeedBack();
            $scope.Device = "";
            $scope.isShow = true;
            $scope._new =
          {
              'UserId': "",
              'UserIP': "",
              'UserLocal': $locale.id,
              'Device': "",
              'Name': "",
              'DateCreate': new Date(),
              'Description': "",
              'Attachment': $scope._name,
              'Component': "General",
              'Type': "Suggestion",
              'Status': "",
              'FeedBackURL': ""
          };
            $scope.setUrl = function (url) {
                $scope._new.FeedBackURL = url;
            };

        }

       

        $scope.insertFeedBack = function (model) {
            var deferer = $q.defer();
            if (model.Attachment)
                $scope.fnUpload();

            $http.post('/Api/FeedBackService/InsertFeedBack', model)
                .success(function () {
                    deferer.resolve();
                }).error(function (errors, status) {
                    __promiseHandler.Error(errors, status, deferer);
                })

            return deferer.promise;
        }

        // processfile
        $scope.fnUpload = function () {
            if ($scope.files.length > 0) {
                var fd = new FormData()
                for (var i in $scope.files) {
                    fd.append("uploadedFile", $scope.files[i])
                }

                var xhr = new XMLHttpRequest();
                xhr.addEventListener("load", uploadComplete, false);
                xhr.open("POST", "/api/FeedBackService/AttachFile?fileName=" + $scope._name);
                $scope.progressVisible = true;
                xhr.send(fd);
            }

        }

        function uploadComplete(evt) {
            $scope.progressVisible = false;
            if (evt.target.status == 201) {
                $scope.FilePath = evt.target.responseText;
                $scope.AttachStatus = "Upload Done";
                $scope.hasFile = false;
            }
            else {
                $scope.AttachStatus = evt.target.responseText;
            }
        }

        $scope.setFiles = function (element) {
            var fileName = "";
            if (element.value != "") {
                $scope.hasFile = true;
                var dt = new Date();
                fileName = dt.getTime().toString() + element.files[0].name;
                $scope._new.Attachment = fileName;
            }

            else
                $scope.hasFile = false;


            $scope._name = fileName;
            $scope.$apply(function (scope) {
                $scope.AttachStatus = "";
                $scope.files = []
                for (var i = 0; i < element.files.length; i++) {
                    $scope.files.push(element.files[i])

                }
                $scope.progressVisible = false

            });
        }

        //

        $scope.init();

        $scope.cancelFeedback = function () {
            $scope.currentView.showingBetaTestFeedback = false;
        };
        $scope.closeFeedback = function () {
            $scope.currentView.showingBetaTestFeedback = false;
        };

     
    }
]);


