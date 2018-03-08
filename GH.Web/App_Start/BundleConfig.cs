using System.Security.Policy;
using System.Web;
using System.Web.Optimization;
using GH.Web.Areas.User.Helpers;

namespace GH.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            #region STYLES

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/font-awesome/css/font-awesome.css",
                "~/Content/uiselect/select.css",
                "~/Content/uiselect/select2-bootstrap.css",
                "~/Content/Lightbox/angular-bootstrap-lightbox.min.css",
                "~/Content/ajax-loader.css",
                "~/Content/common-directives.css",
                "~/Content/common.css"
            ));

            bundles.Add(new StyleBundle("~/Content/bootstrap").Include(
                "~/Content/bootstrap.css"
            ));

            bundles.Add(new StyleBundle("~/bundles/sweet-alert/css").Include(
                "~/Content/SweetAlert/sweetalert2.min.css"
            ));


            bundles.Add(new StyleBundle("~/Content/futurify-icon").Include("~/Content/futurify-icon.css"));
            bundles.Add(new StyleBundle("~/bundles/user/account-settings/css").Include(
                "~/Areas/User/Content/account-settings.css",
                "~/Areas/Beta/css/account.css"
            ).Versioned());
            bundles.Add(new StyleBundle("~/Content/angular-material").Include(
                "~/Content/angular-material.min.css"));

            #endregion

            #region SCRIPTS

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                //                "~/Areas/Beta/js/vendor/jquery-3.1.1.min.js"));
                "~/Scripts/jquery-1.10.2.min.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/respond.js"));


            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                "~/Areas/regitUI/js/lodash.min.js",
                "~/Scripts/AngularJS/moment-with-locales.min.js",
                "~/Content/scripts/jquery.growl.js",
                "~/Areas/regitUI/js/chance.min.js",
                "~/Areas/regitUI/js/jqcloud.min.js",
                "~/Areas/regitUI/js/highcharts.js",
                "~/Areas/regitUI/js/exporting.js",
                "~/Areas/regitUI/js/offline-exporting.js",
                "~/Areas/regitUI/js/excellentexport.min.js",
                "~/Scripts/AngularJS/angular.js",
                "~/Scripts/AngularJS/angular-sanitize.min.js",
                "~/Scripts/AngularJS/angular-cookies.min.js",
                "~/Scripts/AngularJS/angular-route.min.js",
                "~/Scripts/AngularJS/angular-touch.min.js",
                "~/Content/scripts/angular-animate.min.js",
                "~/Scripts/AngularJS/angular-messages.min.js",
                "~/Scripts/AngularJS/angular-resource.min.js",
                "~/Areas/regitUI/js/angular-simple-logger.js",
                "~/Areas/regitUI/js/angular-google-maps.min.js",
                "~/Areas/regitUI/js/ngGeolocation.min.js",
                "~/Areas/Beta/js/vendor/angular-momentjs.js",
                "~/Scripts/AngularJS/ui-bootstrap-tpls-1.3.3.min.js",
                "~/Content/uiselect/select.js",
                "~/Content/Lightbox/angular-bootstrap-lightbox.min.js",
                "~/Areas/regitUI/js/ng-tags-input.min.js",
                "~/Areas/regitUI/js/switchery.min.js",
                "~/Areas/regitUI/js/ng-switchery.js",
                "~/Content/uiselect/select.js",
                "~/Areas/regitUI/js/clipboard.min.js",
                "~/Areas/regitUI/js/ngclipboard.min.js",
                "~/Areas/regitUI/js/qrcode.min.js",
                "~/Areas/regitUI/js/angular-qr.min.js",
                "~/Areas/regitUI/js/highcharts-ng.min.js",
                "~/Areas/regitUI/js/html2canvas.min.js",
                "~/Areas/regitUI/js/html2canvas.svg.min.js",
                "~/Areas/regitUI/js/jspdf.min.js",
                "~/Areas/regitUI/js/angular-jqcloud.js"
            ));
            bundles.Add(new ScriptBundle("~/bundles/js/regit-main").Include(
                "~/Scripts/GreenHouse/common.js",
                "~/Scripts/GreenHouse/translator.js",
                "~/Scripts/GreenHouse/common-directives.js",
                "~/Areas/regitUI/js/modules/vault.js",
                "~/Areas/regitUI/js/modules/defs.js",
                "~/Areas/regitUI/js/modules/regit.ui.js",
                "~/Areas/regitUI/js/modules/location.js",
                "~/Areas/regitUI/js/modules/notifications.js",
                "~/Areas/regitUI/js/modules/calendar.js",
                "~/Areas/regitUI/js/modules/windows.js",
                "~/Areas/regitUI/js/modules/msg.js",
                "~/Areas/regitUI/js/modules/campaigns.js",
                "~/Areas/regitUI/js/modules/interaction.js",
                "~/Areas/regitUI/js/modules/users.js",

                "~/Areas/regitUI/js/modules/analytics.js"
            ).Versioned());

            bundles.Add(new ScriptBundle("~/bundles/sweet-alert").Include(
                "~/Scripts/SweetAlert/sweetalert2.min.js",
                "~/Scripts/SweetAlert/angular-sweet-alert.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/ng-tags-input").Include(
                "~/Scripts/TagsInput/ng-tags-input.min.js"
                , "~/Scripts/TagsInput/ng-tags-input.js"
            ));

            bundles.Add(new ScriptBundle("~/Blue/highcharts").Include(
                "~/Content/BlueContent/Themes/Default/Js/highcharts.js"
                , "~/Content/BlueContent/Themes/Default/Js/highcharts-ng.min.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/RegistrationCampaignPublish").Include(
                "~/Areas/User/Scripts/Controllers/Campaign/RegistrationCampaignPublishController.js"
            ).Versioned());

            #endregion

            #region Blue Project

            bundles.Add(new StyleBundle("~/Blue/css").Include(
                "~/Areas/regitUI/css/common.css",
                "~/Areas/regitUI/css/layout.css",
                "~/Areas/regitUI/css/main.css",
                "~/Areas/regitUI/css/regit-ui.css",
                "~/Areas/regitUI/css/ng-tags-input.min.css",
                "~/Areas/regitUI/css/components.css",
                "~/Content/BlueContent/Themes/Default/Css/ui-grid.min.css",
                "~/Areas/regitUI/css/select.css",
                "~/Areas/regitUI/css/jqcloud.min.css"
            ).Versioned());

            bundles.Add(new StyleBundle("~/regitCampaign/css").Include(
                "~/Areas/regitUI/css/campaigns.css"
            ).Versioned());
            bundles.Add(new StyleBundle("~/regitVault/css").Include(
                "~/Areas/regitUI/css/vault.css",
                "~/Areas/regitUI/css/vault-main.css",
                "~/Areas/regitUI/css/vault-docs.css"
            ).Versioned());

            bundles.Add(new StyleBundle("~/regitCalendar/css").Include(
                "~/Areas/regitUI/css/calendar.css"
            ).Versioned());

            #endregion

            BundlesForUser(bundles);


            BundleTable.EnableOptimizations = false;
        }

        public static void BundlesForUser(BundleCollection bundles)
        {
            #region BETA

            bundles.Add(new StyleBundle("~/bundles/css/public").Include(
                "~/Areas/Beta/css/greenhouse.css",
                "~/Areas/Beta/css/public.css"
            ).Versioned());
            bundles.Add(new StyleBundle("~/bundles/css/about").Include(
                "~/Areas/Beta/css/public.css",
                "~/Areas/About/Content/css/pure-min.css",
                "~/Areas/About/Content/css/main.css").Versioned());

            bundles.Add(new StyleBundle("~/bundles/css/main").Include(
                "~/Areas/Beta/css/bootstrap.min.css",
                "~/Content/styles/jquery.growl.css",
                "~/Areas/Beta/css/switchery.min.css",
                "~/Content/SweetAlert/sweetalert2.min.css",
                "~/Areas/Beta/css/select.css",
                "~/Content/common.css",
                "~/Content/common-directives.css",
                "~/Areas/Beta/css/greenhouse.css",
                "~/Areas/Beta/css/components.css",
                "~/Areas/Beta/css/calendar.css",
                "~/Areas/Beta/css/regit-main.css",
                "~/Areas/Beta/css/ajax-loader.css",
                "~/Areas/Beta/css/regit-ui.css",
                "~/Areas/Beta/css/ng-tags-input.min.css",
                "~/Areas/Beta/css/formregit.css",
                "~/Areas/Beta/css/betatest.css"
            ).Versioned());


            bundles.Add(new StyleBundle("~/bundles/css/betatest-feedback").Include(
                "~/Areas/Beta/css/betatest-feedback.css"));

            bundles.Add(new StyleBundle("~/bundles/css/feed").Include(
                "~/Areas/Beta/css/regit-feed.css").Versioned());
            bundles.Add(new StyleBundle("~/bundles/css/individual").Include(
                "~/Areas/Beta/css/vault.css",
                "~/Areas/Beta/css/vault-docs.css").Versioned());

            bundles.Add(new StyleBundle("~/bundles/css/business").Include(
                "~/Areas/Beta/css/business.css",
                "~/Areas/Beta/css/interactions.css").Versioned());

            bundles.Add(new StyleBundle("~/bundles/css/phone-input").Include(
                "~/Content/styles/intlTelInput.css"));

            //        Script bundles

            bundles.Add(new ScriptBundle("~/bundles/js/public-vendor").Include(
                "~/Areas/Beta/js/vendor/jquery-3.1.1.min.js",
                "~/Scripts/AngularJS/angular.js",
                "~/Scripts/AngularJS/angular-cookies.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/js/regit-ui").Include(
                "~/Areas/Beta/js/vendor/angular-strap.min.js",
                "~/Areas/Beta/js/vendor/angular-strap.tpl.min.js",
                "~/Areas/Beta/js/modules/regit.ui.js"
            ).Versioned());

            bundles.Add(new ScriptBundle("~/bundles/js/public").Include(
                "~/Areas/Beta/js/greenhouseApp.js",
                "~/Areas/User/Scripts/Services/UserModule.js",
                "~/Areas/User/Scripts/Services/PublicDataService.js",
                "~/Areas/User/Scripts/Services/AuthorizationService.js",
                "~/Areas/Beta/js/regitPublic.js",
                "~/Areas/User/Scripts/Controllers/PublicSigninController.js"
            ).Versioned());

            bundles.Add(new ScriptBundle("~/bundles/js/vendor").Include(
                "~/Areas/Beta/js/vendor/jquery-3.1.1.min.js",
                "~/Areas/Beta/js/vendor/moment.min.js",
                "~/Areas/Beta/js/vendor/angular.js",
                "~/Areas/Beta/js/vendor/angular-route.js",
                "~/Areas/Beta/js/vendor/angular-sanitize.min.js",
                "~/Areas/Beta/js/vendor/angular-cookies.min.js",
                "~/Areas/Beta/js/vendor/angular-momentjs.js",
                "~/Areas/Beta/js/vendor/ui-bootstrap-tpls-2.1.3.min.js",
                "~/Content/Lightbox/angular-bootstrap-lightbox.min.js",
                "~/Areas/Beta/js/vendor/angular-strap.min.js",
                "~/Areas/Beta/js/vendor/angular-strap.tpl.min.js",
                "~/Areas/Beta/js/vendor/select.min.js",
                "~/Areas/Beta/js/vendor/switchery.min.js",
                "~/Areas/Beta/js/vendor/ng-switchery.js",
                "~/Scripts/SweetAlert/sweetalert2.min.js",
                "~/Scripts/SweetAlert/angular-sweet-alert.js"));

            bundles.Add(new ScriptBundle("~/bundles/js/greenhouse").Include(
                "~/Scripts/GreenHouse/common.js",
                "~/Scripts/GreenHouse/translator.js",
                "~/Scripts/GreenHouse/common-directives.js").Versioned());

            bundles.Add(new ScriptBundle("~/bundles/js/betatest").Include(
                "~/Areas/BetaTest/Scripts/BetaTestController.js"
            ));
            bundles.Add(new ScriptBundle("~/bundles/js/betatest-feedback").Include(
                "~/Content/scripts/excellentexport.min.js",
                "~/Areas/BetaTest/Scripts/BetaTestFeedbacksController.js"));
            //Hoang Vu FeedBack

            bundles.Add(new ScriptBundle("~/bundles/js/feedback-service").Include(
                "~/Areas/User/Scripts/Services/FeedBackService.js"));

            bundles.Add(new ScriptBundle("~/bundles/js/feedback-ctrl").Include(
                "~/Areas/User/Scripts/Controllers/feedback-ctrl.js"));

            bundles.Add(new ScriptBundle("~/bundles/js/admin-feedback-ctrl").Include(
                "~/Areas/BetaTest/Scripts/BetaTestFeedbacksController.js"));
            // End FeedBack
            bundles.Add(new ScriptBundle("~/bundles/js/about").Include(
                "~/Areas/About/Content/js/modernizr-custom.js",
                "~/Areas/About/Content/js/jquery-2.2.3.min.js",
                "~/Areas/About/Content/js/main.js",
                "~/Areas/About/Content/Scripts/AboutController.js"
            ).Versioned());


            bundles.Add(new ScriptBundle("~/bundles/js/main").Include(
                "~/Areas/Beta/js/greenhouseApp.js",
                "~/Areas/regitUI/js/modules/defs.js",
                "~/Areas/regitUI/js/modules/regit.ui.js",
                "~/Areas/regitUI/js/modules/notifications.js",
                "~/Areas/regitUI/js/modules/calendar.js",
                "~/Areas/regitUI/js/modules/windows.js",
                "~/Areas/regitUI/js/modules/interactions.js",
                "~/Areas/Beta/js/regitMain.js",
                "~/Areas/Beta/js/controllers/CalendarCtrl.js",
                "~/Areas/Beta/js/controllers/EventEditorCtrl.js",
                "~/Areas/Beta/js/controllers/MsgCtrl.js",
                "~/Areas/Beta/js/controllers/ActivityCtrl.js"
            //,
            //"~/Areas/User/Scripts/Services/FeedBackService.js",
            //"~/Areas/User/Scripts/Controllers/feedback-ctrl.js"
            ).Versioned());


            bundles.Add(new ScriptBundle("~/bundles/js/individual").Include(
                "~/Areas/Beta/js/modules/vault.js",
                "~/Areas/Beta/js/modules/docs.js",
                "~/Areas/Beta/js/regitApp.js",
                "~/Areas/Beta/js/controllers/AccountCtrl.js",
                "~/Areas/Beta/js/controllers/BusinessPageCtrl.js",
                "~/Areas/Beta/js/controllers/VaultManagerCtrl.js",
                "~/Areas/Beta/js/controllers/DelegationCtrl.js",
                "~/Areas/Beta/js/controllers/VaultAddFormCtrl.js",
                "~/Areas/Beta/js/controllers/VaultFamilyCtrl.js",
                "~/Areas/Beta/js/controllers/VaultDocumentCtrl.js"
            ).Versioned());
            bundles.Add(new ScriptBundle("~/bundles/js/greenhouseBusiness").Include(
                "~/Areas/User/Scripts/Services/BusinessAccountService.js"
            ).Versioned());

            bundles.Add(new ScriptBundle("~/bundles/js/business").Include(
                "~/Areas/Beta/js/regitBusiness.js",
                "~/Areas/Beta/js/modules/analytics.js",
                "~/Areas/Beta/js/controllers/InteractionManagerCtrl.js"
            ).Versioned());

            bundles.Add(new ScriptBundle("~/bundles/js/business/interaction/editor").Include(
                "~/Areas/Beta/js/controllers/InteractionEditorCtrl.js",
                "~/Areas/Beta/js/controllers/FormEditorCtrl.js",
                "~/Areas/Beta/js/controllers/InteractionPreviewCtrl.js"
            ).Versioned());

            bundles.Add(new ScriptBundle("~/bundles/js/phone-input").Include(
                "~/Content/scripts/intlTelInput.min.js",
                //"~/Content/scripts/utils.js",
                "~/Content/scripts/international-phone-number.js").Versioned());

            #endregion


            bundles.Add(new StyleBundle("~/User/css").Include(
                "~/Areas/User/Content/layout.css",
                "~/Areas/User/Content/Dashboard.css"
            ).Versioned());


            bundles.Add(new StyleBundle("~/User/trouble-css").Include(
                "~/Areas/Beta/css/trouble.css"
            ).Versioned());

            bundles.Add(new StyleBundle("~/User/trouble-controller").Include(
                "~/Areas/User/Scripts/Controllers/TroubleController.js"
            ).Versioned());

            bundles.Add(new StyleBundle("~/User/login").Include(
                "~/Areas/User/Content/login.css"
            ).Versioned());

            bundles.Add(new ScriptBundle("~/bundles/login-ctrl").Include(
                "~/Areas/User/Scripts/Controllers/login-ctrl.js"
            ).Versioned());

            bundles.Add(new StyleBundle("~/User/signup/css").Include(
                "~/Areas/User/Content/signup.css"
            ).Versioned());

            bundles.Add(new StyleBundle("~/User/signup/controller").Include(
                "~/Areas/User/Scripts/Controllers/sign-up-ctrl.js"
            ).Versioned());

            bundles.Add(new ScriptBundle("~/bundles/user-dashboard-service").Include(
                "~/Areas/User/Scripts/Services/DashBoardService.js"
            ).Versioned());

            bundles.Add(new ScriptBundle("~/bundles/user-activity-log-service").Include(
                "~/Areas/User/Scripts/Services/ActivityLogServices.js"
            ).Versioned());


            bundles.Add(new ScriptBundle("~/bundles/user-home-ctrl").Include(
                "~/Areas/User/Scripts/Controllers/user-home-ctrl.js"
            ).Versioned());

            bundles.Add(new ScriptBundle("~/bundles/user-post-new-feed").Include(
                "~/Areas/User/Scripts/Controllers/post-new-feed-ctrl.js"
            ).Versioned());

            bundles.Add(new ScriptBundle("~/bundles/user-layout-ctrl").Include(
                "~/Areas/User/Scripts/Controllers/user-layout-ctrl.js"
            ).Versioned());

            bundles.Add(new ScriptBundle("~/bundles/user-service").Include(
                "~/Areas/User/Scripts/Services/UserModule.js",
                "~/Areas/User/Scripts/Services/AuthorizationService.js",
                "~/Areas/User/Scripts/Services/UserManagementService.js",
                "~/Areas/User/Scripts/Services/NetworkService.js",
                "~/Areas/User/Scripts/Services/DataService.js"
            ).Versioned());

            #region Blue Project

            bundles.Add(new ScriptBundle("~/bundles/comment").Include(
                "~/Areas/regitUI/js/angular-strap.min.js",
                "~/Areas/regitUI/js/angular-strap.tpl.min.js"
            ).Versioned());
            bundles.Add(new ScriptBundle("~/bundles/ui-grid").Include(
                "~/Content/BlueContent/Themes/Default/Js/ui-grid.min.js"
            ));
            bundles.Add(new ScriptBundle("~/bundles/draganddrop").Include(
                "~/Content/BlueContent/Themes/Default/Js/draganddrop.min.js"
            ));
            bundles.Add(new ScriptBundle("~/bundles/SMSauthencation").Include(
                "~/Areas/User/Scripts/Services/SmSAuthencationService.js"
            ).Versioned());
            bundles.Add(new ScriptBundle("~/bundles/CommentService").Include(
                "~/Areas/User/Scripts/Services/CommentService.js"
            ).Versioned());
            bundles.Add(new ScriptBundle("~/bundles/CountryCityService").Include(
                "~/Areas/User/Scripts/Services/CountryCityService.js"
            ).Versioned());
            bundles.Add(new ScriptBundle("~/bundles/AlertServices").Include(
                "~/Areas/User/Scripts/Services/alertService.js"
            ).Versioned());
            bundles.Add(new ScriptBundle("~/bundles/CampaignService").Include(
                "~/Areas/User/Scripts/Services/CampaignService.js"
            ).Versioned());
            bundles.Add(new ScriptBundle("~/bundles/smsauthencation-ctrl").Include(
                "~/Areas/User/Scripts/Controllers/sms-authencation-ctrl.js"
            ));
            bundles.Add(new ScriptBundle("~/bundles/smsauthencation-ctrl").Include(
                "~/Areas/User/Scripts/Controllers/sms-authencation-ctrl.js"
            ).Versioned());
            // vault information
            bundles.Add(new ScriptBundle("~/bundles/information-vault-ctrl").Include(
                "~/Areas/User/Scripts/Controllers/Vault/information-vault-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/personal-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/address-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/financial-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/visaCard-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/masterCard-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/paypal-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/governmentid-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/BirthCertificate-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/DriverLicense-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/HealthCard-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/MedicalCard-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/PermanentCard-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/socialCard-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/taxID-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/passportid-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/family-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/membership-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/employment-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/education-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/others-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/document-ctrl.js"
            ).Versioned());
            bundles.Add(new ScriptBundle("~/bundles/search-vault").Include(
                "~/Areas/User/Scripts/Controllers/Vault/Search/search-vault-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/Search/search-contact-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/Search/search-address-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/Search/search-governmentid-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/Search/search-others-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/Search/search-financial-ctrl.js",
                "~/Areas/User/Scripts/Controllers/Vault/Search/search-vault-form-ctrl.js"
            ).Versioned());

            bundles.Add(new ScriptBundle("~/bundles/InformationVaultService").Include(
                "~/Areas/User/Scripts/Services/InformationVaultService.js",
                "~/Areas/User/Scripts/Services/DocumentVaultService.js",
                "~/Areas/User/Scripts/Services/VaultService.js"
            ).Versioned());
            // end vault information

            bundles.Add(new ScriptBundle("~/bundles/dialog-confirm-ctrl").Include(
                "~/Areas/User/Scripts/Controllers/dialog-confirm-ctrl.js"
            ).Versioned());

            bundles.Add(new ScriptBundle("~/bundles/BusinessUserSystem").Include(
                "~/Areas/User/Scripts/Controllers/BusinessUserSytem/AnalyticsCampaignController.js"
                , "~/Areas/User/Scripts/Controllers/BusinessUserSytem/AnalyticsCustomerListController.js"
                , "~/Areas/User/Scripts/Controllers/BusinessUserSytem/IndividualBusinessController.js"
                , "~/Areas/User/Scripts/Controllers/BusinessUserSytem/TransactionController.js"
                , "~/Areas/User/Scripts/Controllers/BusinessUserSytem/CustomerManagementController.js"
            ).Versioned());

            bundles.Add(new ScriptBundle("~/bundles/EventCalendar").Include(
                "~/Areas/User/Scripts/Controllers/Event/CalendarCtrl.js",
                "~/Areas/User/Scripts/Controllers/Event/EventEditorCtrl.js"
            ).Versioned());

            bundles.Add(new ScriptBundle("~/bundles/NewFeeds").Include(
                "~/Areas/User/Scripts/Controllers/NewFeeds/RegistrationOnNewFeedController.js"
            ).Versioned());

            bundles.Add(new ScriptBundle("~/bundles/Campaign").Include(
                //"~/Areas/User/Scripts/Controllers/Campaign/AdvertisingCampaignPreviewController.js"
                "~/Areas/User/Scripts/Controllers/Campaign/InsertAdvertisingCampaignController.js"
                , "~/Areas/User/Scripts/Controllers/Campaign/InsertRegistrationCampaignController.js"
                , "~/Areas/User/Scripts/Controllers/Campaign/InsertSRFICampaignController.js"
                //, "~/Areas/User/Scripts/Controllers/Campaign/InsertRegistrationCampaignPreviewController.js"
                , "~/Areas/User/Scripts/Controllers/Campaign/InsertRegistrationformCampaignController.js"
                , "~/Areas/User/Scripts/Controllers/Campaign/ManagerCampaignController.js"
                , "~/Areas/User/Scripts/Controllers/Campaign/CampaignListController.js"
                , "~/Areas/User/Scripts/Controllers/Campaign/RegistrationCampaignDetailController.js"
                , "~/Areas/User/Scripts/Controllers/Campaign/EventsCampaignController.js"
                , "~/Areas/User/Scripts/Controllers/Campaign/InsertEventCampaignController.js"
                , "~/Areas/User/Scripts/Controllers/Campaign/ApprovedAdvertisingCampaignController.js"
                , "~/Areas/User/Scripts/Controllers/Campaign/ApprovedRegistrationCampaignController.js"
                , "~/Areas/User/Scripts/Controllers/Campaign/ApprovedSRFICampaignController.js"
                , "~/Areas/User/Scripts/Controllers/Campaign/ApprovedEventCampaignController.js"
            ).Versioned());


            bundles.Add(new ScriptBundle("~/bundles/country-city-ctrl").Include(
                "~/Areas/User/Scripts/Controllers/country-city-ctrl.js"
            ).Versioned());

            bundles.Add(new ScriptBundle("~/bundles/ActivityLogService").Include(
                "~/Areas/User/Scripts/Services/ActivityLogService.js"
            ).Versioned());

            bundles.Add(new ScriptBundle("~/bundles/sms-authencation-popup-ctrl").Include(
                "~/Areas/User/Scripts/Controllers/sms-authencation-popup-ctrl.js"
            ).Versioned());

            bundles.Add(new ScriptBundle("~/bundles/activity-log-ctrl").Include(
                "~/Areas/User/Scripts/Controllers/activity-log-ctrl.js"
            ).Versioned());

            #endregion


            bundles.Add(new ScriptBundle("~/bundles/user/account-settings-ctrl").Include(
                "~/Content/scripts/jquery.accordion.js",
                "~/Areas/User/Scripts/Controllers/account-settings-ctrl.js",
                "~/Areas/User/Scripts/Controllers/edit-profile-ctrl.js",
                "~/Areas/User/Scripts/Controllers/privacy-settings-ctrl.js",
                "~/Areas/User/Scripts/Controllers/activity-log-settings-ctrl.js",
                "~/Areas/User/Scripts/Controllers/manage-login-password-ctrl.js",
                "~/Areas/User/Scripts/Controllers/manage-one-regit-account-ctrl.js",
                "~/Areas/User/Scripts/Controllers/close-suspend-settings-ctrl.js",
                "~/Areas/User/Scripts/Controllers/notification-settings-ctrl.js"
            ).Versioned());

            bundles.Add(new ScriptBundle("~/bundles/user/network-ctrl").Include(
                "~/Content/scripts/jquery.accordion.js",
                "~/Areas/User/Scripts/Controllers/NetworkController.js",
                "~/Areas/User/Scripts/Controllers/manage-network-ctrl.js",
                "~/Areas/User/Scripts/Controllers/relationship-settings-ctrl.js").Versioned());

            bundles.Add(new ScriptBundle("~/bundles/test/vaultTest").Include(
                "~/Scripts/AngularJS/angular.js",
                "~/Scripts/AngularJS/angular-sanitize.min.js",
                "~/Scripts/AngularJS/angular-cookies.min.js",
                "~/Scripts/AngularJS/angular-route.min.js",
                "~/Scripts/AngularJS/angular-touch.min.js",
                "~/Scripts/AngularJS/angular-resource.min.js",
                "~/Areas/regitUI/js/angular-simple-logger.js",
                "~/Areas/regitUI/js/angular-google-maps.min.js",
                "~/Areas/BetaTest/Scripts/VuTestVaultService.js",
                "~/Areas/BetaTest/Scripts/VuTestVaultCtrl.js"
            ));
        }
    }
}