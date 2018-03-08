
var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule', 'ang-drag-drop'], true);
myApp.getController('ApprovedEventCampaignController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'NotificationService', 'CountryCityService', 'fileUpload', 'dateFilter', 'CampaignService', 'CommonService', '$uibModal', 'interactionFormService',
function ($scope, $rootScope, $http, userManager, sweetAlert, authService, alertService, informationVaultService, notificationService, countryCityService, fileUpload, dateFilter, CampaignService, CommonService, $uibModal, interactionFormService) {
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
    $scope.event = new Object();
    $scope.Target = new Object();
    $scope.Effective = new Object();
    $scope.Effective.FromDate = new Date();
    $scope.Effective.ToDate = new Date();
    $scope.qr = new Object();
    $scope.unitmodel = "USD";
    $scope.usercodetype = "USD";
    $scope.usercode = "";
    $scope.usercodecurrentcy = "";

    $scope.commentsFromSupervisor = "";
    $scope.commentsCriteria = "";
    $scope.commentsBudgetTime = "";
    $scope.commentsMedia = "";
    $scope.commentsEvent = "";
    $scope.Effective.openedFromDate = false;
    $scope.Effective.openedToDate = false;

    $scope.isshowdraff = true;
    $scope.isshowbackcampaign = false;
    $scope.isshowtemplate = true;
    $scope.isshowbttemplate = false;
    $scope.isshowtitle = false;
    $scope.isshowstatus = false;
    $scope.pageTitle = "";
    $scope.status = "";
    $scope.action = CommonService.GetQuerystring("action");

    $scope.statusFromDay = false;
    $scope.statusUntilDay = false;

    $scope.messageBox = "";
    $scope.alerts = [];
    $scope.dateformat = 'yyyy-MM-dd';

    $scope.showingEndDate = false;
    $scope.adFlash = false;
    $scope.Budget = 0;
    $scope.unitCost = 1;


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
        $scope.termsAndConditionsFile=campaign.termsAndConditionsFile;
        $scope.CampaignImage = campaign.image;
        $scope.FlashAdvertising = campaign.flashAdvertising;
        $scope.commentsFromSupervisor = campaign.commentsFromSupervisor;
        $scope.commentsCriteria = campaign.commentsCriteria;
        $scope.commentsBudgetTime = campaign.commentsBudgetTime;
        $scope.commentsMedia = campaign.commentsMedia;
        $scope.commentsEvent = campaign.commentsEvent;
        $scope.commentsFormFields = campaign.commentsFormFields;
        $scope.status=campaign.status;
        $scope.unitmodel = campaign.criteria.spend.currentcy;
        $scope.usercodetype = campaign.usercodetype;
        $scope.usercode = campaign.usercode;
        $scope.usercodecurrentcy = campaign.usercodecurrentcy;

        if (campaign.criteria.spend.money === "")
            $scope.Target.Cost = "0";
        else
            $scope.Target.Cost = campaign.criteria.spend.money;

        //$scope.Target.Cost = campaign.criteria.spend.money;
        $scope.RegistrationTypes = ["Event-Based", "Non Event-Based"];

        if (campaign.registrationType == "" || campaign.registrationType == null)
            $scope.RegistrationType = "Event-Based"
        else
            $scope.RegistrationType = campaign.registrationType;
        
        if (campaign.criteria.targetNetwork == "Public")
            $scope.Target.TargetNetwork = "Regit Network (Public)";
        else
            $scope.Target.TargetNetwork = "Regit Customers (Private)";
             
        //event 
        $scope.event.starttime = campaign.event.starttime;

        if (campaign.event.startdate != "" && campaign.event.startdate != undefined)
            $scope.event.startdate = new Date(campaign.event.startdate);
        else
            $scope.event.startdate = new Date();

        $scope.event.endtime = campaign.event.endtime;

        if (campaign.event.enddate != "" && campaign.event.enddate != undefined)
            $scope.event.enddate = new Date(campaign.event.enddate);
        else
            $scope.event.enddate = new Date();
        $scope.event.location = campaign.event.location;
        $scope.event.theme = campaign.event.theme;


        $scope.Target.MaxPeople = 0;
        $scope.Target.People = 0;
        if (campaign.criteria.gender == "" || campaign.criteria.gender == undefined)
            $scope.Target.Gender = "All";
        else
            $scope.Target.Gender = campaign.criteria.gender;
        $scope.locationTypes = ['Global', 'Country/City'];
        if (campaign.criteria.locationtype == "" || campaign.criteria.locationtype == null || campaign.criteria.locationtype == undefined)
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

        $scope.Target.TimeType = campaign.criteria.spend.type;
        if ($scope.Target.TimeType == "Duration")
            $scope.Target.showingEndDate = true;
        else
            $scope.Target.showingEndDate = false;
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


        //$scope.Target.Cost = campaign.criteria.spend.money;

        //$scope.Effective.FromAge = campaign.criteria.age.min;;
        //$scope.Effective.ToAge = campaign.criteria.age.max;;
        //$scope.Effective.TimeType = campaign.criteria.spend.type;

        // $scope.city = 'Vietnam';

        //$scope.listCountries = countryCityService.Countries;
        //$scope.Country = countryCityService.Country;
        //$scope.listcities = countryCityService.Cities;
        //$scope.City = countryCityService.City;


        //$scope.Target.Country = new Object();
        // $scope.Target.Country.Name =  campaign.criteria.location.Country;
        //if (campaign.criteria.location.country != null && campaign.criteria.location.country != undefined)
        //    $scope.Target.Country = $scope.FindObjectFromName($scope.listCountries, campaign.criteria.location.country);
        //else
        //    $scope.Target.Country = new Object();

        //if (campaign.criteria.location.region !== "" && campaign.criteria.location.region != undefined) {
        //    var obregion = new Object();
        //    obregion.Name = campaign.criteria.location.region;
        //    $scope.listRegions = [obregion];
        //    $scope.Target.Region = $scope.FindObjectFromName($scope.listRegions, campaign.criteria.location.region);
        //}
        //else
        //    $scope.Target.Region = new Object();
        //if (campaign.criteria.location.region !== "" && campaign.criteria.location.region != undefined) {
        //    var obcity = new Object();
        //    obcity.Name = campaign.criteria.location.city;
        //    $scope.listCities = [obcity];
        //    $scope.Target.City = $scope.FindObjectFromName($scope.listCities, campaign.criteria.location.city);
        //}
        //else
        //    $scope.Target.City = new Object();



    }
    $scope.exportQR = function (event) {
        var canvas = document.querySelector('qr canvas');
        // Canvas2Image.saveAsPNG(canvas, 400, 400);
        var link = event.target;
        link.href = canvas.toDataURL();
        link.download = 'QR-' + fullcampign._id + '.png';
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

    //$scope.openDatePicker = function () {
    //    $scope.datePicker.opened = true;
    //};

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

    $scope.changeTargetNetwork = function () {
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

    //$scope.changeGender = function () {
    //    $scope.GetCalculateNumberOfUser(false);
    //}

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

        //event 
        campaign.event.starttime = $scope.event.starttime;
        if ($scope.event.startdate == null || $scope.event.startdate == undefined) {
            $scope.event.startdate = "";
            campaign.event.startdate = $scope.event.startdate;
        }

        else
            campaign.event.startdate = dateFilter($scope.event.startdate, $scope.dateformat);

        campaign.event.endtime = $scope.event.endtime;
        if ($scope.event.enddate == null || $scope.event.enddate == undefined) {
            $scope.event.enddate = "";
            campaign.event.enddate = $scope.event.enddate;
        }

        else
            campaign.event.enddate = dateFilter($scope.event.enddate, $scope.dateformat);

        campaign.event.theme = $scope.event.theme;
        campaign.event.location = $scope.event.location;


        if ($scope.Target.ToDate == null || $scope.Target.ToDate == undefined) {
            $scope.Target.ToDate = "";
            campaign.criteria.spend.endDate = $scope.Target.ToDate;
        }
        else
            campaign.criteria.spend.endDate = dateFilter($scope.Target.ToDate, $scope.dateformat);
        campaign.criteria.spend.money = $scope.Target.Cost;

        campaign.criteria.locationtype = $scope.locationType;
        campaign.registrationType = $scope.RegistrationType;
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
            fullcampign.campaign = campaign;
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
        //CampaignService.CampaignAdvertising = campaign;
        $scope.$parent.step = 2;
        $scope.InitData2();

    }

     $scope.showhidebutton = function () {
         $scope.pageTitle = "";
         $scope.isshowtitle = true;
        if (fullcampign._id != "" &&  $scope.action!="clone" ) {
          
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
                $scope.pageTitle = "APPROVE INTERACTION: " + campaign.name;
                $scope.isshowstatus = false;
                $scope.isshowdraff = true;
                $scope.isshowbttemplate = false;
                $scope.isshowtemplate = true;
            }
        }
        else {

            $scope.isshowdraff = true;
            $scope.isshowbackcampaign = false;
            $scope.isshowtemplate = true;
            $scope.pageTitle = "NEW CAMPAIGN - EVENT";
        }
    }

     $scope.savebackcampaign = function () {
         //CampaignService.CampaignAdvertising.status="Pending";
         campaign.status = "Active";

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


    $scope.GetTemplate = function () {
        var data = new Object();
        $http.post('/api/CampaignService/GetCampaignEventTemplate', data)
             .success(function (response) {
                 campaign = response.CampaignTemplateAdvertising;
                 $scope.InitData();
                 $scope.showhidebutton();
             })
             .error(function (errors, status) {
             });
    }

    if (fullcampign._id != "") {
        CampaignService.GetCampaignById(fullcampign._id).then(function (response) {
            fullcampign = response.Campaign;
            campaign = response.Campaign.campaign;
            $scope.InitData();
            $scope.showhidebutton();
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

    $scope.InitData2 = function () {
        $scope.Vault2 = new Object();
        // 
        $scope.GetVaultTreeForRegistration2();
        // if (fullcampign._id != "")
        $scope.formFields2 = campaign.fields;
    }

    $scope.GetVaultTreeForRegistration2 = function () {
        var CampaignModelView = new Object();

        $http.post('/api/CampaignService/GetVaultTreeForRegistration', CampaignModelView)
          .success(function (response) {
              $scope.Vault2 = response.TreeVault.vaultTreeForRegistration;
              // Added by Son Nguyen: add group name & optionality to every field
              $.each($scope.Vault2[0].values, function (index, group) {
                  var groupName = group.displayName;
                  $.each(group.values, function (index, field) {
                          field.group = groupName;
                          field.optional = false;
                  });
              });

          })
          .error(function (errors, status) {
          });


    }

    $scope.NexttoPreview2 = function () {
        campaign.fields = $scope.formFields2;
        $scope.$parent.step = 3;
        $scope.InitData3();
    }

    $scope.Back2 = function () {
        $scope.$parent.step = 1;
    }

    $scope.successFunction2 = function () {
        CampaignService.CampaignAdvertising = null;
        $location.path("Campaign/ManagerCampaign");
    }

    $scope.failFunction2 = function () {
        alertService.renderSuccessMessage(response.ReturnMessage);
        $scope.messageBox = alertService.returnFormattedMessage();
        $scope.alerts = alertService.returnAlerts();
    }

    $scope.SaveAsDraft2 = function () {

        $scope.FillToSave();
        //CampaignService.CampaignAdvertising = campaign;
        campaign.status = "Draft";
        //$scope.InsertCampaignAdvertising(campaign);
        campaign.fields = $scope.formFields2;
        if (fullcampign._id != "") {
            fullcampign.campaign = campaign;
            CampaignService.SaveCampaign(fullcampign).then(function (response) {

                window.location.href = "/Campaign/ManagerCampaign";
            }, function (errors) {

            });

        }
        else {
            CampaignService.InsertCampaignAdvertising(campaign).then(function (response) {
                window.location.href = "/Campaign/ManagerCampaign";
            }, function (errors) {

            });
        }
    }

    $scope.deleteField2 = function (index, array) {
        if (array === $scope.formFields2) {
            array.splice(index, 1);
        }
    };

    $scope.onDrop2 = function ($event, $data, array) {
        if (array === $scope.formFields2) {
            array.push($data);
        }
    };
    //Step 3
    $scope.InitData3 = function () {
        $scope.Name3 = campaign.name;
        $scope.Description3 = campaign.description;
        $scope.ImageUrl3 = campaign.image;
        $scope.starttime = campaign.event.starttime;
        $scope.startdate = campaign.event.startdate;
        $scope.endtime = campaign.event.endtime;
        $scope.enddate = campaign.event.enddate;
        $scope.location = campaign.event.location;
        $scope.theme = campaign.event.theme;
    }

    $scope.open3 = function (size) {
        CampaignService.CampaignAdvertising = campaign;
        
        // START
        // re-order form fields by vault group
        var myfields = [];
        var vaultgroup = {
            "data": [
                        { "id": "0", "name": ".basicInformation" },
                        { "id": "1", "name": ".contact" },
                        { "id": "2", "name": ".address" },
                        { "id": "3", "name": ".financial" },
                        { "id": "4", "name": ".governmentID" },
                        { "id": "5", "name": ".family" },
                        { "id": "6", "name": ".membership" },
                        { "id": "7", "name": ".employment" },
                        { "id": "8", "name": ".education" },
                        { "id": "9", "name": ".others" },
                        { "id": "10", "name": "Custom" },
                        { "id": "11", "name": "undefined" }
                    ]
        };

        var data = vaultgroup.data;

        for (var i in data) {
            var name = data[i].name;
            $(campaign.fields).each(function (index) {
                if (campaign.fields[index].jsPath.startsWith(name) && name != '') {
                    myfields.push(campaign.fields[index]);
                }
            });
        }

        campaign.fields = myfields;
        // END 
        // re-order form fields by vault group

        interactionFormService.initFieldGroups(campaign.fields);
        $scope.renderFieldGroup = interactionFormService.renderFieldGroup;
        var modalInstance = $uibModal.open({
            animation: $scope.animationsEnabled,
            templateUrl: 'formregister.html',
            controller: 'ApprovedRegisformeventController',
            size: size,
            scope: $scope,
            resolve: {
                campaign: CampaignService.CampaignAdvertising
            }
        });
        modalInstance.result.then(function (campaign) {

        }, function () {

        });
    };

    // $scope.InitData3();

    $scope.Back3 = function () {
        $scope.$parent.step = 2;
        $.each($scope.formFields2, function (index, field) {
            field.removed = undefined;
        });
    }

    $scope.Done3 = function () {
        campaign.status = "Active";
        //$scope.InsertCampaignAdvertising(campaign);
        $.each($scope.formFields2, function (index, field) {
            field.removed = undefined;
        });
        campaign.fields = $scope.formFields2;
        if (fullcampign._id != "" && $scope.action!="clone") {
            fullcampign.campaign = campaign;
            CampaignService.SaveCampaign(fullcampign).then(function (response) {
                window.location.href = "/Campaign/ManagerCampaign";
            }, function (errors) {

            });
        }
        else {
            CampaignService.InsertCampaignAdvertising(campaign).then(function (response) {
                window.location.href = "/Campaign/ManagerCampaign";
            }, function (errors) {

            });
        }
    }

    $scope.successFunction3 = function () {
        CampaignService.CampaignAdvertising = null;
        $location.path("Campaign/ManagerCampaign");
    }

    $scope.failFunction3 = function () {
        alertService.renderSuccessMessage(response.ReturnMessage);
        $scope.messageBox = alertService.returnFormattedMessage();
        $scope.alerts = alertService.returnAlerts();
    }

    //  Added by Son Nguyen: form template processing
    var formTemplates =
    {
        'Membership': ['576187da9e9e822f503fd87d', '576187da9e9e822f503fd87e', '576187da9e9e822f503fd876'],
        'Event': ['576187da9e9e822f503fd87d', '576187da9e9e822f503fd87e', '576187da9e9e822f503fd876', '576187da9e9e822f503fd878'],
        'Seminar': ['576187da9e9e822f503fd87d', '576187da9e9e822f503fd87e', '576187da9e9e822f503fd876', '576187da9e9e822f503fd878'],
        'Hotel': ['576187da9e9e822f503fd87d', '576187da9e9e822f503fd87e', '576187da9e9e822f503fd91a'],
        'Flight': ['576187da9e9e822f503fd87d', '576187da9e9e822f503fd87e', '576187da9e9e822f503fd91a'],
        'Insurance': ['576187da9e9e822f503fd87d', '576187da9e9e822f503fd87e', '576187da9e9e822f503fd881', '576187da9e9e822f503fd926',
            '576187da9e9e822f503fd8b3', '576187da9e9e822f503fd8ae', '576187da9e9e822f503fd8af'],
    }
    ;
    $scope.formTemplate = 'Blank Form';
    $scope.onSelectTemplate = function () {
        //console.log($scope.Vault2[0].values);
        var templateFields = [];
        $scope.formFields2 = [];
        $.each($scope.Vault2[0].values, function (index, group) {
            $.each(group.values, function (index, field) {
                switch (field._id) {
                    case '576187da9e9e822f503fd8af': field.displayName = 'Street Name'; break; 
                    case '576187da9e9e822f503fd91a': field.displayName = 'Passport Number'; break;
                    case '576187da9e9e822f503fd926': field.displayName = 'Social Insurance Number'; break;
                }

                if ($.inArray(field._id, formTemplates[$scope.formTemplate]) != -1 && field.displayName && field.displayName.length) {
                    templateFields.push(field);
                }
            });
        });
        $scope.formFields2 = templateFields;
        //console.log($scope.formFields2);
    }
    $scope.openField = function (event, formField) {
        event.preventDefault();
        var modalInstance = $uibModal.open({
            templateUrl: 'modal-formfield-edit.html',
            controller: 'formFieldEditCtrl',
            size: 'sm',
            resolve: {
                field: function () {
                    return $.extend({},formField);
                }
               
            }
        });
        modalInstance.result.then(function (field) {
            formField = $.extend(formField,field);
        }, function () {

        });
    };

    $scope.cancelAndClose = function () {
        window.location.href = "/Campaign/ManagerCampaign";
    }

    $scope.saveCampaignAndSendBackComment = function () {
        //CampaignService.CampaignAdvertising.status="Pending";
        campaign.commentsFromSupervisor = $scope.commentsFromSupervisor;
        campaign.commentsCriteria = $scope.commentsCriteria;
        campaign.commentsBudgetTime = $scope.commentsBudgetTime;
        campaign.commentsMedia = $scope.commentsMedia;
        campaign.commentsEvent = $scope.commentsEvent;
        campaign.commentsFormFields = $scope.commentsFormFields;;
         var fields = [];
        if (campaign.fields.length > 0)
            $(campaign.fields).each(function (index, field) {
                fields.push({
                    displayName: field.displayName,
                    displayName2: field.displayName,
                    id: field.id,
                    jsPath: field.jsPath + "",
                    path: field.path,
                    label: field.label,
                    optional: field.optional != true ? false : true,
                    optional2: field.optional2,
                    type: field.type,
                    options: field.options,
                    value: field.value,
                    model: field.model,
                    unitModel: field.unitModel,
                    membership: field.membership,
                    choices: field.choices,
                    qa: field.qa
                })
            });
        campaign.fields = fields;
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
        campaign.commentsEvent = $scope.commentsEvent;
        
        campaign.commentsFormFields = $scope.commentsFormFields;;
        var fields = [];
        if (campaign.fields.length > 0)
            $(campaign.fields).each(function (index, field) {
                fields.push({
                    displayName: field.displayName,
                    displayName2: field.displayName,
                    id: field.id,
                    jsPath: field.jsPath + "",
                    path: field.path,
                    label: field.label,
                    optional: field.optional != true ? false : true,
                    optional2: field.optional2,
                    type: field.type,
                    options: field.options,
                    value: field.value,
                    model: field.model,
                    unitModel: field.unitModel,
                    membership: field.membership,
                    choices: field.choices,
                    qa: field.qa
                })
            });
        campaign.fields = fields;
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

myApp.controller('formFieldEditCtrl', 
function ($scope, $uibModalInstance, field) {
    $scope.field = field;
    $scope.ok = function () {
        $uibModalInstance.close(field);
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    };
});

myApp.controller('ApprovedRegisformeventController',
    ['$scope', '$routeParams', '$uibModalInstance', 'campaign', 'interactionFormService',
function ($scope, $routeParams, $uibModalInstance, campaign, interactionFormService) {

    $scope.campaign = campaign;

    $scope.renderFieldGroup = interactionFormService.renderFieldGroup;

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
    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    };
}]);


myApp.getController('datetimecontroller',
    ['$scope', '$routeParams',
function ($scope, $routeParams) {
    $scope.dateformat = 'yyyy-MM-dd';
    $scope.datetime = "";
    $scope.openeddatetime = false;
    $scope.opendatetime = function () {
        $scope.openeddatetime = true;
    };


}]);


