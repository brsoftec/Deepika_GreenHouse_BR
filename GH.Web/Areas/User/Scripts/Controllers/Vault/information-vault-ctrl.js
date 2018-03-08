var myApp = getApp("myApp", true);

myApp.getController('InformationVaultController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'SmSAuthencationService', 'ConfirmDialogService', 'VaultService',

function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, InformationVaultService, SmSAuthencationService, ConfirmDialogService, _vaultService) {
    var vm = this;
    var isAuthencation = false;
    var vaultinformation;

    this.getVaultInformation = function () {
        $http.post('/Api/Account/IsCheckPinVault', null)
           .success(function (isauthencation) {
               isAuthencation = isauthencation;
               if (isAuthencation == false) {
                   window.location.href = "/User/SMSAuthencation?TypeRedirect=Vault";
               }
               var userId = "";
               InformationVaultService.GetVaultInformation(userId).then(function (response) {
                   InformationVaultService.VaultInformation = response.VaultInformation;
                   vm.AlphaRedirectTask("main_form.html");
                   //vm.teamplateVault = "/Areas/User/Views/VaultInformation/content_html/main_form.html";
               }, function (errors) {
               });
           });

    }
    //$rootScope.GetData = function()
    //{
    //    var vm = new Object();
    //    _vaultService.GetVault(vm).then(function (rs) {
    //        dataInformation = rs.VaultInformation;
    //        vm.AlphaRedirectTask("main_form.html");
    //        vm.teamplateVault = "/Areas/User/Views/VaultInformation/content_html/main_form.html";
    //    }, function (errors) {
    //        swal('Error', errors, 'error');
    //    });
    //}

    vm.ToDate = new Date();

    this.AlphaRedirectTask = function (name) {
        if (name == 'main_form.html') {
            vm.showVaultMain = true;
        }
        else { vm.showVaultMain = false; }

        vm.teamplateVault = "/Areas/User/Views/VaultInformation/content_html/" + name;

        //address
        vm.current_address = "/Areas/User/Views/VaultInformation/content_html/address/current_address.html";
        vm.delivery_address = "/Areas/User/Views/VaultInformation/content_html/address/delivery_address.html";
        vm.billing_address = "/Areas/User/Views/VaultInformation/content_html/address/billing_address.html";
        vm.mailing_address = "/Areas/User/Views/VaultInformation/content_html/address/mailing_address.html";
        vm.pobox = "/Areas/User/Views/VaultInformation/content_html/address/pobox.html";

        //financial
        vm.bank_account = "/Areas/User/Views/VaultInformation/content_html/financial/bank_account.html";
        vm.bank_card = "/Areas/User/Views/VaultInformation/content_html/financial/bank_card.html";
        vm.master_card = "/Areas/User/Views/VaultInformation/content_html/financial/master_card.html";
        vm.visa_card = "/Areas/User/Views/VaultInformation/content_html/financial/visa_card.html";
        vm.paypal = "/Areas/User/Views/VaultInformation/content_html/financial/paypal.html";
        vm.insurance = "/Areas/User/Views/VaultInformation/content_html/financial/insurance.html";
        vm.investment = "/Areas/User/Views/VaultInformation/content_html/financial/investment.html";

        //government
        vm.birthday_certificate = "/Areas/User/Views/VaultInformation/content_html/government/birthday_certificate.html";
        vm.custom = "/Areas/User/Views/VaultInformation/content_html/government/custom.html";
        vm.driver_license_card = "/Areas/User/Views/VaultInformation/content_html/government/driver_license_card.html";
        vm.passport = "/Areas/User/Views/VaultInformation/content_html/government/passport.html";
        vm.health_card = "/Areas/User/Views/VaultInformation/content_html/government/health_card.html";

        vm.medical_card = "/Areas/User/Views/VaultInformation/content_html/government/medical_beneﬁt_card.html";
        vm.social_card = "/Areas/User/Views/VaultInformation/content_html/government/social_card.html";

        vm.permanent_card = "/Areas/User/Views/VaultInformation/content_html/government/permanent_card.html";
        vm.nationalid = "/Areas/User/Views/VaultInformation/content_html/government/nationalid.html";
        vm.taxid = "/Areas/User/Views/VaultInformation/content_html/government/taxid.html";

        //relationship emergency
        vm.family = "/Areas/User/Views/VaultInformation/content_html/relationship/family.html";
        vm.pet = "/Areas/User/Views/VaultInformation/content_html/relationship/pet.html";
        vm.emergency = "/Areas/User/Views/VaultInformation/content_html/relationship/emergency.html";

        //MAIN
        vm.main_basic = "/Areas/User/Views/VaultInformation/content_html/main/_basic.html";
        vm.main_contact = "/Areas/User/Views/VaultInformation/content_html/main/_contact.html";
        vm.main_address = "/Areas/User/Views/VaultInformation/content_html/main/_address.html";

        //address main
        vm._current_address = "/Areas/User/Views/VaultInformation/content_html/main/_address/_current_address.html";
        vm._delivery_address = "/Areas/User/Views/VaultInformation/content_html/main/_address/_delivery_address.html";
        vm._billing_address = "/Areas/User/Views/VaultInformation/content_html/main/_address/_billing_address.html";
        vm._mailing_address = "/Areas/User/Views/VaultInformation/content_html/main/_address/_mailing_address.html";
        vm._pobox = "/Areas/User/Views/VaultInformation/content_html/main/_address/_pobox.html";


        vm.main_financial = "/Areas/User/Views/VaultInformation/content_html/main/_financial.html";
        //financial main
        vm._bank_account = "/Areas/User/Views/VaultInformation/content_html/main/_financial/_bank_account.html";
        vm._bank_card = "/Areas/User/Views/VaultInformation/content_html/main/_financial/_bank_card.html";
        vm._master_card = "/Areas/User/Views/VaultInformation/content_html/main/_financial/_master_card.html";
        vm._visa_card = "/Areas/User/Views/VaultInformation/content_html/main/_financial/_visa_card.html";
        vm._paypal = "/Areas/User/Views/VaultInformation/content_html/main/_financial/_paypal.html";
        vm._insurance = "/Areas/User/Views/VaultInformation/content_html/main/_financial/_insurance.html";
        vm._investment = "/Areas/User/Views/VaultInformation/content_html/main/_financial/_investment.html";


        vm.main_governmentID = "/Areas/User/Views/VaultInformation/content_html/main/_governmentID.html";

        //government
        vm._birthday_certificate = "/Areas/User/Views/VaultInformation/content_html/main/_government/_birthday_certificate.html";
        vm._custom = "/Areas/User/Views/VaultInformation/content_html/main/_government/_custom.html";
        vm._driver_license_card = "/Areas/User/Views/VaultInformation/content_html/main/_government/_driver_license_card.html";
        vm._health_card = "/Areas/User/Views/VaultInformation/content_html/main/_government/_health_card.html";
        vm._medical_card = "/Areas/User/Views/VaultInformation/content_html/main/_government/_medical_beneﬁt_card.html";

        vm._passport = "/Areas/User/Views/VaultInformation/content_html/main/_government/_passport.html";

        vm._social_card = "/Areas/User/Views/VaultInformation/content_html/main/_government/_social_card.html";
        vm._permanent_card = "/Areas/User/Views/VaultInformation/content_html/main/_government/_permanent_card.html";
        vm._nationalid = "/Areas/User/Views/VaultInformation/content_html/main/_government/_nationalid.html";
        vm._taxid = "/Areas/User/Views/VaultInformation/content_html/main/_government/_taxid.html";
        //end government


        vm.main_membership = "/Areas/User/Views/VaultInformation/content_html/main/_membership.html";
        vm.main_family = "/Areas/User/Views/VaultInformation/content_html/main/_family.html";
        vm.main_pet = "/Areas/User/Views/VaultInformation/content_html/main/_pet.html";
        vm.main_emergency = "/Areas/User/Views/VaultInformation/content_html/main/_emergency.html";

        vm.main_employment = "/Areas/User/Views/VaultInformation/content_html/main/_employment.html";
        vm.main_education = "/Areas/User/Views/VaultInformation/content_html/main/_education.html";
        vm.main_others = "/Areas/User/Views/VaultInformation/content_html/main/_others.html";

    }

    this.initializeController = function () {
        vm.showVaultMain = true;
        this.getVaultInformation();
    }
    $scope.$on('UpdateVaultInformation', function () {

        vm.showVaultMain = true;
        var userId = "";
        InformationVaultService.GetVaultInformation(userId).then(function (response) {
            InformationVaultService.VaultInformation = response.VaultInformation;
            $rootScope.ruserid = response.VaultInformation.userId;
            vm.AlphaRedirectTask("main_form.html");
            //vm.teamplateVault = "/Areas/User/Views/VaultInformation/content_html/main_form.html";
            $rootScope.$broadcast('GetBasicInformation');
        }, function (errors) {
        });


    })


}])


//Vu
var _vaultInformation;
myApp.getController('VaultController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'SmSAuthencationService', 'ConfirmDialogService', 'VaultService',

function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, InformationVaultService, SmSAuthencationService, ConfirmDialogService, _vaultService) {
    // Get Vault
    $scope.vault = {};
    $scope.GetVault = function () {
        $http.post('/Api/Account/Verify/IsSMSAuthenticated', null)
           .success(function (isauthencation) {
               isAuthencation = isauthencation;
               if (isAuthencation == false) {
                   window.location.href = "/User/SMSAuthencation?TypeRedirect=Vault";
               }
               var vm = new Object();
               _vaultService.GetVault(vm).then(function (rs) {
                   _vaultInformation = rs.VaultInformation;
                   $scope.vault = rs;
                   $scope.test2 = _vaultInformation;

               }, function (errors) {
                   swal('Error', errors, 'error');
               });
           });
    }
    $scope.GetVault();

    // Update Vault
    $scope.UpdateVault = function () {
        var vm = new Object();
        vm.vaultString = _vaultInformation;
        vm.userId = _vaultInformation._id;
        var vm = new Object();
        _vaultService.UpdateVault(vm).then(function (rs) {
            _vaultInformation = rs.VaultInformation;

        }, function (errors) {
            swal('Error', errors, 'error');
        });

    }

    $scope.$on('GetUpdateVault', function () {
        $scope.UpdateVault();
    })

    //
    $scope.IncludeForm = function (name) {
        var pform = "/Areas/User/Views/VaultInformation/templates/" + name;
        return pform;

    }

    //


}])



// 5 Office Email
myApp.getController('TestOfficeEmailController',
    ['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService',
        function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService) {

            // var BasicInfo = _vaultInformation.contact;
            $scope.test = {};
            $scope.InitData = function () {
                $scope.test = _vaultInformation;
            }

            $scope.InitData();

        }]);







