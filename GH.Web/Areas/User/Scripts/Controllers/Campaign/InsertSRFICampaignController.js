var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule', 'ang-drag-drop'], true);
myApp.getController('InsertSRFICampaignController',
    ['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'NotificationService', 'CountryCityService', 'fileUpload', 'dateFilter', 'CampaignService', 'CommonService', '$uibModal', '$document', '$timeout', 'interactionFormService',
        function ($scope, $rootScope, $http, userManager, sweetAlert, authService, alertService, informationVaultService, notificationService, countryCityService, fileUpload, dateFilter, CampaignService, CommonService, $uibModal, $document, $timeout, interactionFormService) {
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

            $scope.Target = new Object();
            $scope.event = new Object();
            $scope.Effective = new Object();
            $scope.qr = new Object();

            $scope.Effective.FromDate = new Date();
            $scope.Effective.ToDate = new Date();
            $scope.usercodetypecheck = true;
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

            $scope.commentsFromSupervisor = "";
            $scope.commentsCriteria = "";
            $scope.commentsBudgetTime = "";
            $scope.commentsMedia = "";

            $scope.unitmodel = "USD";
            $scope.usercodetype = "USD";
            // $scope.test = 0;
            $scope.usercodecurrentcy = "";

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

            $scope.Target.keywords = [];
            $scope.termsAndConditionsFile = "";
            $scope.InitData = function () {

                $scope.qr.PublicURL = location.protocol + '//' + location.host + '/interaction/' + CommonService.GetQuerystring("campaignid");
                if (campaign.qrCode.AllowCreateQrCode != undefined && campaign.qrCode.AllowCreateQrCode != null)
                    $scope.qr.AllowCreateQrCode = campaign.qrCode.AllowCreateQrCode;
                else
                    $scope.qr.AllowCreateQrCode = true;

                $scope.status = campaign.status;
                $scope.flashCost = 10;
                $scope.AdName = campaign.name;
                $scope.Description = campaign.description;
                $scope.CampaignType = "";
                if (campaign.addToBusinessPage != undefined && campaign.addToBusinessPage != null && campaign.addToBusinessPage != "")
                    $scope.AddtoBusinessFeed = campaign.addToBusinessPage;
                else
                    $scope.AddtoBusinessFeed = "Yes";
                $scope.CreateQRCode = campaign.CreateQRCode;

                $scope.URLLink = campaign.targetLink;
                $scope.CampaignImage = campaign.image;
                $scope.FlashAdvertising = campaign.flashAdvertising;
                $scope.commentsFromSupervisor = campaign.commentsFromSupervisor;
                $scope.commentsCriteria = campaign.commentsCriteria;
                $scope.commentsBudgetTime = campaign.commentsBudgetTime;
                $scope.commentsMedia = campaign.commentsMedia;
                $scope.termsAndConditionsFile = campaign.termsAndConditionsFile;
                $scope.unitmodel = campaign.criteria.spend.currentcy;
                $scope.usercodetype = campaign.usercodetype;
                $scope.commentsFormFields = campaign.commentsFormFields;
                if (campaign.usercode === "")
                    $scope.Target.usercode = 0;
                else
                    $scope.Target.usercode = parseInt(campaign.usercode);

                $scope.usercodecurrentcy = campaign.usercodecurrentcy;

                if (campaign.criteria.spend.money === "")
                    $scope.Target.Cost = 0;
                else
                    $scope.Target.Cost = parseInt(campaign.criteria.spend.money);

                //$scope.Target.Cost = campaign.criteria.spend.money;
                $scope.RegistrationTypes = ["Event-Based", "Non Event-Based"];

                if (campaign.registrationType == "" || campaign.registrationType == null)
                    $scope.RegistrationType = "Event-Based"
                else
                    $scope.RegistrationType = campaign.registrationType;
                ;
                if (campaign.criteria.targetNetwork == "Public")
                    $scope.Target.TargetNetwork = "Regit Network (Public)";
                else
                    $scope.Target.TargetNetwork = "Regit Customers (Private)";

                if (campaign.criteria.keywords != undefined && campaign.criteria.keywords != null) {
                    $scope.Target.keywords = campaign.criteria.keywords;
                }
                else {
                    $scope.Target.keywords = [];
                }

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

                if (campaign.criteria.spend.type != undefined && campaign.criteria.spend.type != null && campaign.criteria.spend.type != "")
                    $scope.Target.TimeType = campaign.criteria.spend.type;
                else
                    $scope.Target.TimeType = "Duration";
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

                if (campaign.criteria.spend.effectiveDate != "" && campaign.criteria.spend.effectiveDate != undefined)
                    $scope.Target.FromDate = new Date(campaign.criteria.spend.effectiveDate);
                else
                    $scope.Target.FromDate = new Date();

                if (campaign.criteria.spend.endDate !== "" && campaign.criteria.spend.endDate != undefined)
                    $scope.Target.ToDate = new Date(campaign.criteria.spend.endDate);
                else
                    $scope.Target.ToDate = new Date();
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
                filter.ListKeywork = $scope.Target.keywords
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

                campaign.usercodetype = $scope.usercodetype;
                campaign.usercode = $scope.Target.usercode + "";
                campaign.usercodecurrentcy = $scope.unitmodel;
                campaign.criteria.spend.currentcy = $scope.unitmodel;
                campaign.termsAndConditionsFile = $scope.termsAndConditionsFile;
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
                campaign.criteria.keywords = $scope.Target.keywords;
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
                campaign.criteria.spend.money = $scope.Target.Cost + "";
                campaign.qrCode.AllowCreateQrCode = $scope.qr.AllowCreateQrCode;
                campaign.criteria.locationtype = $scope.locationType;
                campaign.registrationType = $scope.RegistrationType;
                campaign.name = $scope.AdName;
                campaign.description = $scope.Description;
                campaign.termsAndConditionsFile = $scope.termsAndConditionsFile;
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

            $scope.NextToPreview = function () {

                if ($scope.formCE.$invalid) {
                    return;
                }
                $scope.FillToSave();
                //CampaignService.CampaignAdvertising = campaign;
                $scope.$parent.step = 2;
                $scope.InitData2();

            }
            $scope.showhidebutton = function () {
                $scope.pageTitle = "";
                $scope.isshowtitle = true;
                if (fullcampign._id != "" && $scope.action != "clone") {

                    $scope.isshowstatus = true;
                    //edit
                    if (campaign.status == "Template") {
                        $scope.isshowbackcampaign = false;
                        $scope.isshowtemplate = false;
                        $scope.isshowdraff = false;
                        $scope.isshowbttemplate = true;
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
                        $scope.pageTitle = "APPROVE CAMPAIGN: " + campaign.name;
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
                    $scope.pageTitle = "NEW CAMPAIGN - SRFI";
                }
            }

            $scope.savebackcampaign = function () {
                //CampaignService.CampaignAdvertising.status="Pending";
                campaign.status = "Pending";

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
                $http.post('/api/CampaignService/GetCampaignSRFITemplate', data)
                    .success(function (response) {
                        campaign = response.CampaignTemplateAdvertising;
                        campaign.qrCode.PublicURL = response.Urlpublic;
                        campaign.qrCode.AllowCreateQrCode = true;
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
            $scope.uploadFileConditions = function (element) {
                var file = element.files[0];

                var uploadUrl = "/api/CampaignService/UploadImage";
                fileUpload.uploadFileToUrl(file, uploadUrl, function (reponse) {
                    $scope.termsAndConditionsFile = reponse.fileName;

                    var input = $(element);
                    input.replaceWith(input.val('').clone(true));

                }, function () {

                }, "");
            };
            //Step 2

            $scope.InitData2 = function () {

                $scope.selectedTemplate = null;
                $scope.onSelectTemplate = function () {
                    var template = $scope.selectedTemplate;
                    if (!template || !(template.hasOwnProperty('fields'))) return;
                    angular.forEach(template.fields, function (field) {
                        $scope.addEntryToFormByPath(field.jsPath, field.label);
                    });
                };
                $http.post('/api/CampaignService/GetFormTemplate', CampaignModelView)
                    .success(function (response) {
                        $scope.formTemplates = response.TreeVault.formTemplates;
                    })
                    .error(function (errors, status) {
                    });

                var CampaignModelView = new Object();
                $scope.formEntries = [];
                $scope.formEntries = campaign.fields;
                //  Prepare Vault tree for search
                $http.post('/api/CampaignService/GetVaultTreeForRegistration', CampaignModelView)
                    .success(function (response) {
                        $scope.vaultTree = response.TreeVault;
                        var entries = [];

                        function traverseVault(node, level, path, jsPath) {
                            if (!angular.isObject(node) || node.nosearch) return;
                            angular.forEach(node, function (entry, name) {
                                if (!angular.isObject(entry) || entry.nosearch || !angular.isDefined(entry.hiddenForm) || entry.hiddenForm === true)
                                    return;

                                if (!angular.isObject(entry.value) || angular.isArray(entry.value)) {
                                    //  Leaf entry (field)
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
                                    entries.push(field);
                                } else {
                                    //  Non-leaf entry (folder)
                                    entries.push({
                                        id: entry._id,
                                        label: entry.label,
                                        leaf: false,
                                        undraggable: entry.undraggable,
                                        level: level,
                                        path: path,
                                        jsPath: jsPath + '.' + name
                                    });
                                    traverseVault(entry.value, level + 1, path + '.' + entry.label, jsPath + '.' + name);
                                }
                            });
                        }

                        traverseVault($scope.vaultTree, 1, '', '');
                        $scope.vaultEntries = entries;

                    })
                    .error(function (errors, status) {
                    });

            }

            $scope.vaultPane = {
                searchQuery: ''
            };

            $scope.isMembershipStatic = function (entry) {
                return (entry.membership && entry.type === 'static');
            };
            $scope.isDeletable = function (field) {
                return !$scope.isMembershipStatic(field);
            };

            $scope.filterEntriesByQuery = function (entry) {
                if ($scope.isMembershipStatic(entry))
                    return false;
                var re = new RegExp($scope.vaultPane.searchQuery, 'i');
                return re.test(entry.label);
            };

            $scope.onVaultSearchInput = function () {
                if (!$scope.vaultPane.searchQuery.length) {
                    $scope.activeEntry = null;
                } else {
                    $scope.activeEntry = 0;
                    $scope.vaultPane.searchQuery = $scope.vaultPane.searchQuery.replace(/[^a-zA-Z0-9 .-/]/g, '');

                }
            };
            $scope.IsvalidRange = function (field) {
                var model = field.model;
                if (field.type != "range")
                    return true;
                if (model == null || model == undefined) {
                    return false;
                }
                if (!(model instanceof Array))
                    return false;

                if (model.length <= 0)
                    return false;
                var check = true;
                $(model).each(function (index, object) {
                    if ((object[0] == undefined || object[0] == null) && (object[1] == undefined || object[1] == null))
                        check = false;

                })
                return check;
            }
            $scope.isActiveEntry = function (entry, index) {
                return $scope.activeEntry === index;
            };

            $scope.clearVaultSearch = function () {
                $scope.activeEntry = null;
                $scope.vaultPane.searchQuery = '';
            };
            $scope.gotoVaultEntry = function (index) {
                $scope.activeEntry = index;
            };
            $scope.selectVaultEntry = function (index) {
                $scope.addEntryToFormByIndex(index);
                $scope.activeEntry = index;
                $document.find('#vault-tree-search-input').focus();
                $scope.clearVaultSearch();

            };
            $scope.onVaultEntryKeyPress = function (event) {
                // event.preventDefault();
                var keyCode = event.which;
                var entryCount = $scope.matchedEntries.length;
                $scope.activeEntry = $scope.activeEntry || 0;
                var index = $scope.activeEntry;
                // console.log(keyCode, index, entryCount)
                if (keyCode === 40 && index < entryCount - 1) {
                    $scope.activeEntry++;
                } else if (keyCode === 38 && index > 0) {
                    $scope.activeEntry--;
                } else if (keyCode === 27) {
                    $scope.activeEntry = null;
                    $scope.vaultPane.searchQuery = '';
                } else if (keyCode === 13) {
                    $scope.selectVaultEntry($scope.activeEntry);
                }
            };

            $scope.addedToForm = function (entry) {
                var found = false;
                angular.forEach($scope.formEntries, function (field) {
                    if (entry.jsPath === field.jsPath) {
                        found = true;
                        return;
                    }
                });
                return found;
            };
            $scope.addedToFormByPath = function (jsPath) {
                var found = false;
                angular.forEach($scope.formEntries, function (field) {
                    if (jsPath === field.jsPath) {
                        found = true;
                    }
                });
                return found;
            };
            $scope.hasMembership = function () {
                var found = false;
                angular.forEach($scope.formEntries, function (field) {
                    if (field.membership && field.type !== 'static') {
                        found = true;
                    }
                });
                return found;
            };
            $scope.checkMembership = function () {
                if ($scope.hasMembership()) {
                    angular.forEach($scope.vaultEntries, function (entry) {
                        if ($scope.isMembershipStatic(entry)) {
                            $scope.addEntryToForm(entry, false);
                        }
                    });

                } else {
                    angular.forEach($scope.formEntries, function (field) {

                        if ($scope.isMembershipStatic(field)) {

                            $scope.deleteField(field, false);
                        }
                    });
                }
            };
            $scope.addEntryToForm = function (entry, checkMembership) {
                if (entry.undraggable) return;
                if ($scope.addedToForm(entry))
                    return;
                if (entry.leaf) {

                    entry.displayName = entry.label;
                    entry.displayName2 = entry.label;
                    entry.optional2 = false;
                    entry.optional = false;
                    $scope.formEntries.push(angular.extend({}, entry));
                    if (checkMembership) {
                        $scope.checkMembership();
                    }
                } else {    // Add whole group
                    var path = entry.path + '.' + entry.label;
                    index = $.inArray(entry, $scope.vaultEntries);
                    if (index < 0) return;
                    while (++index < $scope.vaultEntries.length) {
                        var field = $scope.vaultEntries[index];
                        if (field.path !== path)
                            break;
                        if (!$scope.addedToForm(field)) {
                            field.displayName = field.label;
                            field.displayName2 = field.label;
                            field.optional2 = false;
                            field.optional = false;
                            $scope.formEntries.push(angular.extend({}, field));
                        }
                    }
                }

            };
            $scope.findEntryByPath = function (jsPath) {
                var foundEntry = null;
                angular.forEach($scope.vaultEntries, function (entry) {
                    if (jsPath === entry.jsPath) {
                        foundEntry = entry;
                    }
                });
                return foundEntry;
            };
            $scope.addEntryToFormByPath = function (jsPath, displayName) {
                if ($scope.addedToFormByPath(jsPath))
                    return;
                var entry = $scope.findEntryByPath(jsPath);
                if (!entry) return;
                if (entry.leaf) {
                    entry.displayName = displayName || entry.label;
                    $scope.formEntries.push(angular.extend({}, entry));

                } else {    // Add whole group
                    var path = entry.path + '.' + entry.label;
                    index = $.inArray(entry, $scope.vaultEntries);
                    if (index < 0) return;
                    while (++index < $scope.vaultEntries.length) {
                        var field = $scope.vaultEntries[index];
                        if (field.path !== path)
                            break;
                        if (!$scope.addedToForm(field)) {
                            field.displayName = field.label;
                            $scope.formEntries.push(angular.extend({}, field));
                        }
                    }
                }
            };
            $scope.addEntryToFormByIndex = function (index) {
                var entry = $scope.matchedEntries[index];
                $scope.addEntryToForm(entry, true);
            };
            $scope.deleteField = function (field, checkMembership) {
                var index = $.inArray(field, $scope.formEntries);
                if (index >= 0) {
                    $scope.formEntries.splice(index, 1);
                    if (checkMembership) {
                        $scope.checkMembership();
                    }
                }
                // console.log(field,checkMembership)
            };

            $scope.filterRequired = function (field) {
                return !field.optional && !field.membership && !field.qa;
                ;
            };
            $scope.filterOptional = function (field) {
                return !!field.optional;
            };
            $scope.filterMembership = function (field) {
                return !!field.membership;
            };
            $scope.filterQA = function (field) {
                return field.qa && !field.optional;
            };

            $scope.getUniqueId = function (field) {
                return field.path + '.' + field.label;
            };
            $scope.formPane = {
                showingFieldPopup: []
            };
            
            function FieldGuid() {
                return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                    var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
                    return v.toString(16);
                });
            }

            //$scope.qaId = 0;
            $scope.addQA = function () {
                //var id = ++$scope.qaId;
                var id = FieldGuid();
                var field = {
                    qa: true,
                    type: 'qa',
                    displayName: 'Custom Question',
                    path: 'Custom.Question.' + id,
                    choices: false
                };
                $scope.formEntries.push(angular.extend({}, field));
            };

            //$scope.docId = 0;
            $scope.addDoc = function () {
                //var id = ++$scope.docId;
                var id = FieldGuid();
                var field = {
                    qa: true,
                    type: 'doc',
                    displayName: 'Upload File',
                    path: 'Custom.UploadFile.' + id,
                    choices: false
                };
                $scope.formEntries.push(angular.extend({}, field));
            };

            $scope.close = function (field) {
                field.optional2 = field.optional;
                field.displayName2 = field.displayName;
                $scope.formPane.showingFieldPopup[$scope.getUniqueId(field)] = false;
            };
            $scope.saveField = function (field) {
                field.optional = field.optional2;
                field.displayName = field.displayName2 ? field.displayName2 : field.displayName;
                $scope.close(field);
            };
            $scope.saveForm = function () {
                workingForm.fields = $scope.formEntries;
            };
            $scope.IsvalidRangeCampaign = true;
            $scope.NexttoPreview2 = function () {
                $scope.IsvalidRangeCampaign = true;
                campaign.fields = [];

                if ($scope.formEntries.length > 0) {
                    $($scope.formEntries).each(function (index, field) {
                        campaign.fields.push({
                            displayName: field.displayName,
                            displayName2: field.displayName2,
                            id: field.id,
                            jsPath: field.jsPath + "",
                            label: field.label,
                            optional: field.optional != true ? false : true,
                            optional2: field.optional2,
                            type: field.type,
                            path: field.path,
                            options: field.options,
                            value: field.value,
                            model: field.model,
                            unitModel: field.unitModel,
                            membership: field.membership,
                            choices: field.choices,
                            qa: field.qa
                        });
                        if ($scope.IsvalidRangeCampaign == true) {
                            $scope.IsvalidRangeCampaign = $scope.IsvalidRange(field) && (field.type != "static" || (field.value != undefined && field.value != ""));
                        }

                    });
                    interactionFormService.initFieldGroups(campaign.fields);
                    $scope.renderFieldGroup = interactionFormService.renderFieldGroup;
                }
                if ($scope.IsvalidRangeCampaign) {
                    $scope.$parent.step = 3;
                    $scope.InitData3();
                }
            }

            $scope.Back2 = function () {
                $scope.$parent.step = 1;
            }
            //Step 3
            $scope.InitData3 = function () {
                $scope.Name3 = campaign.name;
                $scope.Description3 = campaign.description;
                $scope.UserCost = campaign.usercode + " " + campaign.usercodecurrentcy;
                $scope.Usercodetype = campaign.usercodetype;
                $scope.AllowCreateQrCode3 = campaign.qrCode.AllowCreateQrCode;
                $scope.PublicURL3 = campaign.qrCode.PublicURL;
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
                        { "id": "10", "name": "undefined" }
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

                var modalInstance = $uibModal.open({
                    animation: $scope.animationsEnabled,
                    templateUrl: 'formregister.html',
                    controller: 'eventformregisterController',
                    size: size,
                    resolve: {
                        campaign: CampaignService.CampaignAdvertising
                    }
                });
                modalInstance.result.then(function (campaign) {

                }, function () {

                });
            };
            //Step 3
            $scope.InitData3 = function () {
                $scope.Name3 = campaign.name;
                $scope.Description3 = campaign.description;
                $scope.UserCost = campaign.usercode + " " + campaign.usercodecurrentcy;
                $scope.Usercodetype = campaign.usercodetype;
                $scope.AllowCreateQrCode3 = campaign.qrCode.AllowCreateQrCode;
                $scope.PublicURL3 = campaign.qrCode.PublicURL;
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
                        { "id": "10", "name": "undefined" }
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

                var modalInstance = $uibModal.open({
                    animation: $scope.animationsEnabled,
                    templateUrl: 'formregister.html',
                    controller: 'eventformregisterController',
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
            //Step 3
            $scope.InitData3 = function () {
                $scope.Name3 = campaign.name;
                $scope.Description3 = campaign.description;
                $scope.ImageUrl3 = campaign.image;
                $scope.AllowCreateQrCode3 = campaign.qrCode.AllowCreateQrCode;
                $scope.PublicURL3 = campaign.qrCode.PublicURL;
                $scope.UserCost = campaign.usercode + " " + campaign.usercodecurrentcy;
                $scope.Usercodetype = campaign.usercodetype;
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
                        { "id": "10", "name": "undefined" }
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

                var modalInstance = $uibModal.open({
                    animation: $scope.animationsEnabled,
                    templateUrl: 'formregister.html',
                    controller: 'RegisformregisterController',
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
                /*  $.each($scope.formFields2, function (index, field) {
                 field.removed = undefined;
                 });*/
            }

            $scope.Done3 = function () {
                campaign.status = "Pending";

                campaign.fields = [];
                if ($scope.formEntries.length > 0)
                    $($scope.formEntries).each(function (index, field) {
                        campaign.fields.push({

                            displayName: field.displayName,
                            displayName2: field.displayName2,
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

                    })
                //$scope.InsertCampaignA`   `dvertising(campaign);
                /* $.each($scope.formFields2, function (index, field) {
                 field.removed = undefined;
                 });*/

                if (fullcampign._id != "" && $scope.action != "clone") {
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

            $scope.SaveandClose = function () {
                campaign.status = "Saved";
                //$scope.InsertCampaignA`   `dvertising(campaign);
                $.each($scope.formFields2, function (index, field) {
                    field.removed = undefined;
                });
                campaign.fields = $scope.formFields2;
                if (fullcampign._id != "" && $scope.action != "clone") {
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
                            case '576187da9e9e822f503fd8af':
                                field.displayName = 'Street Name';
                                break;
                            case '576187da9e9e822f503fd91a':
                                field.displayName = 'Passport Number';
                                break;
                            case '576187da9e9e822f503fd926':
                                field.displayName = 'Social Insurance Number';
                                break;
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
                            return $.extend({}, formField);
                        }

                    }
                });
                modalInstance.result.then(function (field) {
                    formField = $.extend(formField, field);
                }, function () {

                });
            };

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

myApp.controller('RegisformregisterController',
    ['$scope', '$routeParams', '$uibModalInstance', 'campaign',
        function ($scope, $routeParams, $uibModalInstance, campaign) {

            $scope.campaign = campaign;

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


