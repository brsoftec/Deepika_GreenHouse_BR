
var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule'], true);
myApp.getController('ApprovedAdvertisingCampaignController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'NotificationService', 'CountryCityService', 'fileUpload', 'dateFilter', 'CampaignService', 'CommonService',
function ($scope, $rootScope, $http, userManager, sweetAlert, authService, alertService, informationVaultService, notificationService, countryCityService, fileUpload, dateFilter, CampaignService, CommonService) {

    //var vm = this;
    //$scope.demoDate = new Date();
    //$scope.dateOptions = { showWeeks: false };
    //$scope.datePicker = { opened: false };
    $scope.$parent.step = 1;
    var campaign = new Object();
    var fullcampign = new Object();

    fullcampign._id = CommonService.GetQuerystring("campaignid");
    if (fullcampign._id == null)
        fullcampign._id = "";
    var filter = new Object();
    $scope.qr = new Object();
    $scope.Target = new Object();
    //$scope.Effective = new Object();
    //$scope.Effective.FromDate = new Date();
    //$scope.Effective.ToDate = new Date();

    //$scope.Effective.openedFromDate = false;
    //$scope.Effective.openedToDate = false;
    $scope.isshowdraff = true;
    $scope.isshowbackcampaign = false;
    $scope.isshowtemplate = true;
    $scope.isshowbttemplate = false;
    $scope.isshowtitle = false;
    $scope.isshowstatus = false;
    $scope.pageTitle = "";
    $scope.status = "";
    $scope.action = CommonService.GetQuerystring("action");
   
    $scope.unitmodel ="";

    $scope.statusFromDay = false;
    $scope.statusUntilDay = false;

    $scope.messageBox = "";
    $scope.alerts = [];
    $scope.dateformat = 'yyyy-MM-dd';

    $scope.showingEndDate = false;
    $scope.adFlash = false;
    $scope.Budget = 0;
    $scope.unitCost = 1;

    $scope.commentsFromSupervisor = "";
    $scope.commentsCriteria = "";
    $scope.commentsBudgetTime = "";
    $scope.commentsMedia = "";

    $scope.totalUsers = 1000;
    $scope.targetUsers = 0;
    $scope.reachPercentage = 0;

    $scope.listGender = ["All", "Male", "Female"];
    $scope.listTimeType = ["Daily", "Duration"];
    $scope.listAge = [];

    $scope.InitData = function () {

        if (campaign.qrCode.PublicURL != undefined && campaign.qrCode.PublicURL != null)
            $scope.qr.PublicURL = campaign.qrCode.PublicURL;
        if (campaign.qrCode.AllowCreateQrCode != undefined && campaign.qrCode.AllowCreateQrCode != null)
            $scope.qr.AllowCreateQrCode = campaign.qrCode.AllowCreateQrCode;
        $scope.flashCost = 10;
        $scope.AdName = campaign.name;
        $scope.Description = campaign.description;
        $scope.CampaignType = "";
        $scope.AddtoBusinessFeed = campaign.addToBusinessPage;
        $scope.CreateQRCode = campaign.CreateQRCode;
        $scope.URLLink = campaign.targetLink;
        $scope.CampaignImage = campaign.image;
        $scope.FlashAdvertising = campaign.flashAdvertising;

        $scope.commentsFromSupervisor = campaign.commentsFromSupervisor;
        $scope.commentsCriteria = campaign.commentsCriteria;
        $scope.commentsBudgetTime = campaign.commentsBudgetTime;
        $scope.commentsMedia = campaign.commentsMedia;
        $scope.unitmodel = campaign.criteria.spend.currentcy;
       if (campaign.criteria.spend.money === "")
            $scope.Target.Cost = "0";
        else
            $scope.Target.Cost = campaign.criteria.spend.money;

        if (campaign.criteria.targetNetwork === "Public")
            $scope.Target.TargetNetwork = "Regit Network (Public)";
        else
            $scope.Target.TargetNetwork = "Regit Customers (Private)";

        $scope.Target.MaxPeople = 0;
        $scope.Target.People = 0;

        if (campaign.criteria.gender === "" || campaign.criteria.gender == undefined)
            $scope.Target.Gender = "All";
        else
            $scope.Target.Gender = campaign.criteria.gender;

        $scope.locationTypes = ['Global', 'Country/City'];
        if (campaign.criteria.locationtype === "" || campaign.criteria.locationtype == null || campaign.criteria.locationtype == undefined)
            $scope.locationType = 'Global';
        else
            $scope.locationType = campaign.criteria.locationtype;
        $scope.continents = ['Asia', 'Europe', 'America', 'Africa'];
        $scope.continent = 'Asia';
        $scope.countries = ['Vietnam', 'Singapore', 'Canada', 'United States'];
        $scope.country = 'Vietnam';


        //$scope.Effective.FromAge = "1";
        //$scope.Effective.ToAge = "100";
        //$scope.Effective.TimeType = "Daily";


        if (campaign.criteria.spend.effectiveDate != "" && campaign.criteria.spend.effectiveDate != undefined)
            $scope.Target.FromDate = new Date(campaign.criteria.spend.effectiveDate);
        else
            $scope.Target.FromDate = new Date();

        $scope.Target.openedFromDate = false;
        $scope.Target.openFromDate = function () {
            $scope.Target.openedFromDate = true;
        };
        if (campaign.criteria.spend.endDate != "" && campaign.criteria.spend.endDate != undefined)
            $scope.Target.ToDate = new Date(campaign.criteria.spend.endDate);
        else
            $scope.Target.ToDate = new Date();
        $scope.Target.openedToDate = false;
        $scope.Target.openToDate = function () {
            $scope.Target.openedToDate = true;
        };



        $scope.Target.FromAge = campaign.criteria.age.min;
        $scope.Target.ToAge = campaign.criteria.age.max;
        $scope.NameCountry = campaign.criteria.location.country;
        $scope.NameCity = campaign.criteria.location.city;
        $scope.Target.TimeType = campaign.criteria.spend.type;
        if ($scope.Target.TimeType == "Duration")
            $scope.Target.showingEndDate = true;
        else
            $scope.Target.showingEndDate = false;

        countryCityService.InitData($scope.NameCity, $scope.NameCountry).then(function (response) {
            $scope.listCountries = countryCityService.Countries;
            $scope.Target.Country = countryCityService.Country;
            $scope.listcities = countryCityService.Cities;
            $scope.Target.City = countryCityService.City;
            $scope.GetCalculateNumberOfUser();
        }, function (errors) {
        });

        //$scope.fileUploadObj = { UserId: applicationConfiguration.usercurrent.Id };
        //if (campaign.criteria.gender === "" || campaign.criteria.gender == undefined)
        //    $scope.Target.Gender = "All";
        //else
        //    $scope.Target.Gender = campaign.criteria.gender;
        if (campaign.criteria.spend.effectiveDate != "" && campaign.criteria.spend.effectiveDate != undefined)
            $scope.Target.FromDate = new Date(campaign.criteria.spend.effectiveDate);
        else
            $scope.Target.FromDate = new Date();

        if (campaign.criteria.spend.endDate !== "" && campaign.criteria.spend.endDate != undefined)
            $scope.Target.ToDate = new Date(campaign.criteria.spend.endDate);
        else
            $scope.Target.ToDate = new Date();

    }

    $scope.closeAlert = function (index) {
        alerts.splice(index, 1);
    };

    $scope.openFromDay = function () {
        $scope.statusFromDay = true;
    }

    $scope.openUntilDay = function () {
        $scope.statusUntilDay = true;
    }

    $scope.updateLocationType = function () {
        $scope.Target.Country = new Object();
        $scope.Target.City = new Object();
        $scope.GetCalculateNumberOfUser();
    }

    $scope.openFromDate = function () {
        $scope.Effective.openedFromDate = true;
    };

    $scope.openToDate = function () {
        $scope.Effective.openedToDate = true;
    };

    $scope.changeCity = function () {
        if ($scope.Target.Country != null && $scope.Target.Country.Code != undefined) {
            countryCityService.GetCitiesByCountryID($scope.Target.Country.Code).then(function (response) {
                $scope.listcities = response.Cities;
                $scope.GetCalculateNumberOfUser();
            }, function (errors) {
            });
        }
        else
            $scope.listcities = [];
    }

    $scope.changeCityTarget = function () {
        $scope.GetCalculateNumberOfUser(false);
    }

    $scope.changeAge = function () {
        $scope.GetCalculateNumberOfUser();
    }

    $scope.changeTargetNetwork= function() {
        $scope.GetCalculateNumberOfUser();
    }

    $scope.onBudgetChange = function () {
        var budget = $scope.budget;
        if ($scope.adFlash) {
            budget -= $scope.flashCost;
        }
        $scope.targetUsers = Math.floor(budget / $scope.unitCost);
        $scope.reachPercentage = Math.round($scope.targetUsers / $scope.totalUsers * 100);
        console.log($scope.targetUsers, $scope.reachPercentage);
        //$scope.GetCalculateNumberOfUser();
    }

    $scope.ChangeCost = function () {
        $scope.GetCalculateNumberOfUser();
    }

    $scope.GenderChange = function () {
        $scope.GetCalculateNumberOfUser();
    }

    $scope.GetCalculateNumberOfUser = function () {
        var data = new Object();
        var filtercampaign = $scope.getFilterCampaign();
        data.CampaignFilter = filtercampaign;

        $http.post('/api/CampaignService/GetCalculateNumberOfUser', data)
              .success(function (response) {
                  var resultfilter = response.CampaignUserFilterResult;
                  $scope.Target.People = resultfilter.UsersBaseOnSpend;
                  $scope.Target.MaxPeople = resultfilter.TotalUsers;
              })
              .error(function (errors, status) {

              });
    }

    $scope.getFilterCampaign = function () {
        var filter = new Object();

        if ($scope.Target.TargetNetwork === "Regit Network (Public)")
            filter.TargetNetwork = "Public";
        else
            filter.TargetNetwork = "Private";

        filter.MinAge = $scope.Target.FromAge;
        filter.MaxAge = $scope.Target.ToAge;
        filter.Gender = $scope.Target.Gender;

        if ($scope.Target.Country != null)
            filter.Country = $scope.Target.Country.Name;
        else
            filter.Country = "";

        if ($scope.Target.City != null)
            filter.City = $scope.Target.City.Name;
        else
            filter.City = "";

        filter.Money = $scope.Target.Cost;

        filter.Flash = $scope.Target.adFlash;
        if (filter.Flash && filter.Money > $scope.flashCost)
            filter.Money = filter.Money - $scope.flashCost;


        return filter;
    }

    $scope.FindObjectFromName = function (array, name) {
        if (array == null || array == undefined || name == null || name == undefined)
            return null;
        var objectreturn = null;
        $(array).each(function (index, object) {
            if (object.Name == name) {
                objectreturn = object;
                return objectreturn;
            }

        });
        return objectreturn;
    }

    $scope.getAllCountries = function () {
        var data = new Object();
        $http.post('/api/LocationService/GetAllCountries', data)
         .success(function (response) {
             $scope.listRegions = [];
             $scope.listCities = [];
             $scope.Target.Region = new Object();
             $scope.Target.City = new Object();
             $scope.listCountries = response.Countries;
         })
         .error(function (errors, status) {

         });
    }

    $scope.FillToSave = function () {
        campaign.criteria.age.min = $scope.Target.FromAge;
        campaign.criteria.age.max = $scope.Target.ToAge;

        if ($scope.Target.Country != null && $scope.Target.Country != undefined)
            campaign.criteria.location.country = $scope.Target.Country.Name;

        if ($scope.Target.Region != null && $scope.Target.Region != undefined)
            campaign.criteria.location.region = $scope.Target.Region.Name;
        else
            campaign.criteria.location.region = "";

        if ($scope.Target.City != null && $scope.Target.City != undefined)
            campaign.criteria.location.city = $scope.Target.City.Name;
        else
            campaign.criteria.location.city = "";

        campaign.criteria.gender = $scope.Target.Gender;
        campaign.criteria.spend.type = $scope.Target.TimeType;

        if ($scope.Target.FromDate == null || $scope.Target.FromDate == undefined) {
            $scope.Target.FromDate = "";
            campaign.criteria.spend.effectiveDate = $scope.Target.FromDate;
        }

        else
            campaign.criteria.spend.effectiveDate = dateFilter($scope.Target.FromDate, $scope.dateformat);

        if ($scope.Target.ToDate == null || $scope.Target.ToDate == undefined) {
            $scope.Target.ToDate = "";
            campaign.criteria.spend.endDate = $scope.Target.ToDate;
        }
        else
            campaign.criteria.spend.endDate = dateFilter($scope.Target.ToDate, $scope.dateformat);
        campaign.criteria.spend.money = $scope.Target.Cost;

        campaign.criteria.locationtype = $scope.locationType;

        campaign.name = $scope.AdName;
        campaign.description = $scope.Description;

        campaign.addToBusinessPage = $scope.AddtoBusinessFeed;

        campaign.flashAdvertising = $scope.Target.adFlash;

        if ($scope.Target.TargetNetwork == "Regit Network (Public)")
            campaign.criteria.targetNetwork = "Public";
        else
            campaign.criteria.targetNetwork = "Private";

        campaign.targetLink = $scope.URLLink;
        campaign.image = $scope.CampaignImage;
        campaign.criteria.estimatedReach = $scope.Target.People + "";

        campaign.flashAdvertising = $scope.Target.adFlash;

        campaign.criteria.spend.type = $scope.Target.TimeType;


    }

    $scope.InsertCampaignAdvertising = function (campaignAdvertising) {
        var stringCampaignAdvertising = JSON.stringify(campaignAdvertising);
        var data = new Object();

        data.StrCampaignAdvertising = stringCampaignAdvertising;
        data.CampaignAdvertising = campaignAdvertising;

        $http.post('/api/CampaignService/InsertCampaignAdvertising', data)
             .success(function (response) {

             })
             .error(function (errors, status) {

             });

        window.location.href = "/Campaign/AdvertisingCampaignPreview";
    }

    $scope.InsertCampaignRegistration = function (campaignRegistration) {
        var stringCampaignRegistration = JSON.stringify(campaignRegistration);
        var data = new Object();
        data.StrCampaignAdvertising = stringCampaignRegistration;
        data.CampaignAdvertising = campaignRegistration;

        $http.post('/api/CampaignService/InsertCampaignRegistration', data)
             .success(function (response) {
             })
             .error(function (errors, status) {
             });
        window.location.href = "/Campaign/AdvertisingCampaignPreview";
    }

    $scope.SaveAsDraff = function () {
        $scope.FillToSave();
        //CampaignService.CampaignAdvertising = campaign;
        campaign.status = "Draft";
        //$scope.InsertCampaignAdvertising(campaign);

        if (fullcampign._id != "" && $scope.action != "clone") {
            CampaignService.SaveCampaign(fullcampign).then(function (response) {
                CampaignService.CampaignAdvertising = null;
                window.location.href = "/Campaign/ManagerCampaign";
            }, function (errors) {

            });

        }
        else {
            CampaignService.InsertCampaignAdvertising(campaign).then(function (response) {
                CampaignService.CampaignAdvertising = null;
                window.location.href = "/Campaign/ManagerCampaign";
            }, function (errors) {

            });
        }

    }

    $scope.SaveAsTemplate = function () {
        $scope.FillToSave();
        //CampaignService.CampaignAdvertising = campaign;
        campaign.status = "Template";
        //$scope.InsertCampaignAdvertising(campaign);

        if (fullcampign._id != ""  && $scope.action!="clone" ) {
            CampaignService.SaveCampaign(fullcampign).then(function (response) {
                CampaignService.CampaignAdvertising = null;
                window.location.href = "/Campaign/ManagerCampaign";
            }, function (errors) {

            });

        }
        else {
            CampaignService.InsertCampaignAdvertising(campaign).then(function (response) {
                CampaignService.CampaignAdvertising = null;
                window.location.href = "/Campaign/ManagerCampaign";
            }, function (errors) {

            });
        }

    }


    $scope.NextToPreview = function () {
        $scope.FillToSave();
        CampaignService.CampaignAdvertising = campaign;
        $scope.$parent.step = 2;
        $scope.InitDataStep2();

    }
    $scope.showhidebutton = function () {
        $scope.isshowtitle = true;
        if (fullcampign._id != "" &&  $scope.action!="clone" ) {
            $scope.pageTitle = "";
            
            $scope.isshowstatus = true;
            //edit
            if (campaign.status == "Template") {
                $scope.isshowbackcampaign = false;
                $scope.isshowtemplate = false;
                $scope.isshowdraff = false;
                $scope.isshowbttemplate=true;
                $scope.status = "TEMPLATE";
                $scope.pageTitle = "EDIT TEMPLATE: " + campaign.name;
                $scope.isshowstatus = true;
            }
            else if (campaign.status == "Draft") {
                $scope.isshowbackcampaign = true;
                $scope.isshowtemplate = false;
                $scope.isshowdraff = true;
                $scope.isshowbttemplate = false;
                $scope.status = "DRAFT";
                $scope.pageTitle = "EDIT DRAFT: " + campaign.name;
                $scope.isshowstatus = true;
            }
            else {
                $scope.pageTitle = "APPROVE BROADCAST: " + campaign.name;
                $scope.isshowstatus = false;
                $scope.isshowdraff = true;
                $scope.isshowbttemplate = false;
                $scope.isshowtemplate = true;
            }
        }
        else {
            $scope.pageTitle = "NEW CAMPAIGN - ADVERTISEMENT";
            $scope.isshowdraff = true;
            $scope.isshowbackcampaign = false;
            $scope.isshowtemplate = true;
        }
    }

    $scope.savebackcampaign = function () {
        //CampaignService.CampaignAdvertising.status="Pending";
        campaign.status = "Active";

        // CampaignService.InsertCampaignAdvertising(campaign, ajaxService, applicationConfiguration, $scope.successFunction, $scope.failFunction)
        if (fullcampign._id != ""  && $scope.action!="clone" ) {
            CampaignService.SaveCampaign(fullcampign).then(function (response) {
                CampaignService.CampaignAdvertising = null;
                window.location.href = "/Campaign/ManagerCampaign";
            }, function (errors) {

            });

        }
        else {
            CampaignService.InsertCampaignAdvertising(campaign).then(function (response) {
                CampaignService.CampaignAdvertising = null;
                window.location.href = "/Campaign/ManagerCampaign";
            }, function (errors) {

            });
        }

    }

    $scope.GetTemplate = function () {
        var data = new Object();
        $http.post('/api/CampaignService/GetCampaignAdvertisingTemplate', data)
            .success(function (response) {
                campaign = response.CampaignTemplateAdvertising;
                $scope.InitData();
                $scope.showhidebutton();
            })
            .error(function (errors, status) {
            });
    };

    if (fullcampign._id != "") {
        CampaignService.GetCampaignById(fullcampign._id).then(function (response) {
            fullcampign = response.Campaign;
            campaign = response.Campaign.campaign;
            $scope.showhidebutton();
            $scope.InitData();
        }, function (errors) {

        });
    }
    else {
        $scope.GetTemplate();
    }

    $scope.Cancel = function () {

        window.location.href = "/Campaign/ManagerCampaign";
    }
    
    $scope.uploadFile = function (element) {
        var file = element.files[0];

        var uploadUrl = "/api/CampaignService/UploadImage";
        fileUpload.uploadFileToUrl(file, uploadUrl, function (reponse) {
            $scope.CampaignImage = reponse.fileName;

            var input = $(element);
            input.replaceWith(input.val('').clone(true));

        }, function () {

        }, "");
    };
    //Step 2

    $scope.InitDataStep2 = function () {
        if (CampaignService.CampaignAdvertising != null || CampaignService.CampaignAdvertising != undefined) {
            $scope.Step2 = new Object();
            $scope.Step2.Name = CampaignService.CampaignAdvertising.name;
            $scope.Step2.Description = CampaignService.CampaignAdvertising.description;
            $scope.Step2.Link = CampaignService.CampaignAdvertising.targetLink;
            $scope.Step2.Imageurl = CampaignService.CampaignAdvertising.image;
            $scope.messageBox = "";
            $scope.alerts = [];
        }
    }
    $scope.BackStep2 = function () {
        $scope.$parent.step = 1;
    }
    $scope.cancelAndClose = function () {
        window.location.href = "/Campaign/ManagerCampaign";
    }

    $scope.saveCampaignAndSendBackComment = function () {
        //CampaignService.CampaignAdvertising.status="Pending";
        campaign.commentsFromSupervisor = $scope.commentsFromSupervisor;
        campaign.commentsCriteria = $scope.commentsCriteria;
        campaign.commentsBudgetTime = $scope.commentsBudgetTime;
        campaign.commentsMedia = $scope.commentsMedia;

        // CampaignService.InsertCampaignAdvertising(campaign, ajaxService, applicationConfiguration, $scope.successFunction, $scope.failFunction)
        if (fullcampign._id != "" && $scope.action != "clone") {
            CampaignService.SaveCampaign(fullcampign).then(function (response) {
                CampaignService.CampaignAdvertising = null;
                window.location.href = "/Campaign/ManagerCampaign";
            }, function (errors) {

            });

        }
        else {
            CampaignService.InsertCampaignAdvertising(campaign).then(function (response) {
                CampaignService.CampaignAdvertising = null;
                window.location.href = "/Campaign/ManagerCampaign";
            }, function (errors) {

            });
        }


    }
    $scope.saveCampaignAndClose = function () {
        //CampaignService.CampaignAdvertising.status="Pending";
        campaign.status = "Active";
        campaign.commentsFromSupervisor = $scope.commentsFromSupervisor;
        campaign.commentsCriteria = $scope.commentsCriteria;
        campaign.commentsBudgetTime = $scope.commentsBudgetTime;
        campaign.commentsMedia = $scope.commentsMedia;


        // CampaignService.InsertCampaignAdvertising(campaign, ajaxService, applicationConfiguration, $scope.successFunction, $scope.failFunction)
        if (fullcampign._id != "" && $scope.action != "clone") {
            CampaignService.SaveCampaign(fullcampign).then(function (response) {
                CampaignService.CampaignAdvertising = null;
                window.location.href = "/Campaign/ManagerCampaign";
            }, function (errors) {

            });

        }
        else {
            CampaignService.InsertCampaignAdvertising(campaign).then(function (response) {
                CampaignService.CampaignAdvertising = null;
                window.location.href = "/Campaign/ManagerCampaign";
            }, function (errors) {

            });
        }


    }
}]);

