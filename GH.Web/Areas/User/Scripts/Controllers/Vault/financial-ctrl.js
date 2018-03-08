
var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit', 'ui.grid.cellNav'], true);

//4.0 groupFinancial
myApp.getController('GroupFinancialController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter) {

    "use strict";
    var BasicInfo = VaultInformationService.VaultInformation.groupFinancial;
    $scope.InitData = function () {
        $scope.IsEdit = false;

        $scope.group = {
            _label: BasicInfo.label,
            _name: BasicInfo.name,
            _default: BasicInfo.default,
            _privacy: BasicInfo.privacy
        };

        // $scope.list_group = ["Bank account", "Bank card", "Master card", "Visa card", "Paypal", "Investment", "Insurance"];
        $scope.list_group = ["Bank account", "Bank card", "Investment", "Insurance"];
        $scope._select = { _value: '' };
        if ($scope.group._default == '') {
            $scope.group._default = "Bank account";
        }

        if ($scope._select._value == '')
            $scope._select._value = $scope.group._default;
    }
  
    $scope.InitData();
    $scope.Edit = function () {
        $scope.IsEdit = true;
    }

    // == edit == 
    $scope.Save = function () {
        BasicInfo.label = $scope.group._label;
        if ($scope.group._name == '') {
            $scope.group._name = $scope.group._label;
        }
        BasicInfo.name = $scope.group._name;
        BasicInfo.privacy = $scope.group._privacy;
        BasicInfo.default = $scope.group._default;

        // === 
        VaultInformationService.VaultInformation.groupFinancial = BasicInfo;
        VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
            VaultInformationService.VaultInformation._id).then(function (response) {
                $scope.IsEdit = false;

            }, function (errors) {

            });
    }

    $scope.saveVaultInformationOnSuccess = function (response) {
        $scope.IsEdit = false;

    }
    $scope.saveVaultInformationOnError = function (response) {
        alertService.renderSuccessMessage(response.ReturnMessage);
        $scope.messageBox = alertService.returnFormattedMessage();
        $scope.alerts = alertService.returnAlerts();
    }

    $scope.Cancel = function () {
        $scope.InitData();
    }

    $scope.goBack = function () {
        $scope.$location.path('/VaultInformation');
    };
    $scope.closePanel = function () {
        $scope.Cancel();
    };
}])

//4.1 Bank account
myApp.getController('BankAccountController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter) {

    "use strict";
    var BasicInfo = VaultInformationService.VaultInformation.groupFinancial.value.bankAccount;
    $scope.InitData = function () {
        $scope.IsEdit = false;
        $scope.dateTimeNow = new Date().toLocaleString();
        $scope._form = {
            _label: BasicInfo.label,
            _name: BasicInfo.name,
            _default: BasicInfo.default,
            _privacy: BasicInfo.privacy
        };
        //
        $scope._listForm = [];
        $(BasicInfo.value).each(function (index, object) {
            var today = new Date();
            var expiryNo = false;

            // indefinite
            var _indefiniteDate = true;
            if (BasicInfo.value[index].expiryDate == undefined)
                BasicInfo.value[index].expiryDate = "";
            if (BasicInfo.value[index].expiryDate != "")
                _indefiniteDate = false;
            if (BasicInfo.value[index].accountNumber == undefined)
                BasicInfo.value[index].accountNumber = "";
            // End indefinite
            
            
            $scope._listForm.push({
                IsEdit: false,
                privacy: BasicInfo.value[index].privacy,
                description: BasicInfo.value[index].description,
                accountNumber: BasicInfo.value[index].accountNumber,
                bankName: BasicInfo.value[index].bankName,
                bankBranch: BasicInfo.value[index].bankBranch,
                accountType: BasicInfo.value[index].accountType,

                bankAdress: BasicInfo.value[index].bankAdress,
                countryCity: { country: BasicInfo.value[index].country, city: BasicInfo.value[index].city },
                state: BasicInfo.value[index].state,
                zipCode: BasicInfo.value[index].zipCode,
                issuedDate: BasicInfo.value[index].issuedDate,
                expiryDate: BasicInfo.value[index].expiryDate,
                indefiniteDate: _indefiniteDate,
                expiryNo: expiryNo,
                nonExpiry: BasicInfo.value[index].nonExpiry,
                note: BasicInfo.value[index].note,
                _default: BasicInfo.value[index]._default,

            });
            if ($scope._listForm[index].issuedDate != "" && $scope._listForm[index].issuedDate != undefined)
                $scope._listForm[index].issuedDate = new Date($scope._listForm[index].issuedDate);
            else
                $scope._listForm[index].issuedDate = new Date();

            if ($scope._listForm[index].expiryDate != "" && $scope._listForm[index].expiryDate != undefined)
            {
                $scope._listForm[index].expiryDate = new Date($scope._listForm[index].expiryDate);
                if ($scope._listForm[index].expiryDate < today)
                    $scope._listForm[index].expiryNo = true;
            }
               
          
        });


        $scope._new =
           {
               'IsEdit': false,
               'privacy': "",
               'description': "",
               '_default': true,
               'accountNumber': "",
               'bankName': "",
               'bankBranch': "",
               'accountType': "Checking",
               'bankAdress': "",
               'countryCity': "",
               'state': "",
               'zipCode': "",
               'issuedDate': new Date('2000-01-01'),
               'expiryDate': '',
                'indefiniteDate': true,
               'nonExpiry': true,
               'note': "",

           };

        $scope.addNewForm = function (newForm) {
            if (newForm.indefiniteDate == true)
                newForm.expiryDate = '';
            $scope._listForm.push({
                'IsEdit': false,
                'privacy': newForm.privacy,
                'description': newForm.description,
                '_default': newForm._default,

                'accountNumber': newForm.accountNumber,
                'bankName': newForm.bankName,
                'bankBranch': newForm.bankBranch,
                'accountType': newForm.accountType,
                'bankAdress': newForm.bankAdress,
                'countryCity': newForm.countryCity,
                'state': newForm.state,
                'zipCode': newForm.zipCode,
                'issuedDate': newForm.issuedDate,
                'expiryDate': newForm.expiryDate,
                'indefiniteDate': newForm.indefiniteDate,
                'nonExpiry': newForm.nonExpiry,
                'note': newForm.note,

            });

        }
       
        //
        $scope.removeForm = function (_value) {
            var index = $scope._listForm.indexOf(_value);
            swal({
                title: "Are you sure to remove bank account: " + _value.description + " ?",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes",
                cancelButtonText: "No"

            }).then(function () {
                $scope._listForm.splice(index, 1);
                $scope.Save();
                swal(
                    'Removed',
                     _value.description + ' has been removed.',
                    'success'
                  )
            }
            , function (dismiss) {
                if (dismiss === 'cancel') {

                }
            });

        };


        //
        $scope.messageBox = "";
        $scope.alerts = [];

    }
    
    $scope.$on('bankAccount', function () {
        $scope.InitData();
    });
    $scope.InitData();
    $scope.Edit = function () {
        $scope.IsEdit = true;
    }

    // == edit == 
    $scope.Save = function () {

        //address
        BasicInfo.label = $scope._form._label;
        if ($scope._form._name == '') {
            $scope._form._name = $scope._form._label;
        }
        BasicInfo.name = $scope._form._name;
        BasicInfo.privacy = $scope._form._privacy;


        //
        var checkDefault = true;
        $($scope._listForm).each(function (index) {

            if (checkDefault == true && $scope._listForm[index]._default == true && $scope._listForm[index].description != $scope._form._default) {
                $scope._form._default = $scope._listForm[index].description;
                checkDefault = false;
            }

        });

        BasicInfo.default = $scope._form._default;

        //
        var lstFormSave = [];
        $($scope._listForm).each(function (index, object) {
            if (checkDefault == false && $scope._listForm[index].description != $scope._form._default) {
                $scope._listForm[index]._default = false;
            }
            // indefinite
            if ($scope._listForm[index].nonExpiry == true)
                $scope._listForm[index].expiryDate = "";
            // end indefinite
            lstFormSave.push({
                privacy: $scope._listForm[index].privacy,

                description: $scope._listForm[index].description,
                accountNumber: $scope._listForm[index].accountNumber,
                bankName: $scope._listForm[index].bankName,

                bankBranch: $scope._listForm[index].bankBranch,
                accountType: $scope._listForm[index].accountType,
                bankAdress: $scope._listForm[index].bankAdress,
                city: $scope._listForm[index].countryCity.city,
                country: $scope._listForm[index].countryCity.country,
                state: $scope._listForm[index].state,
                zipCode: $scope._listForm[index].zipCode,
                issuedDate: $scope._listForm[index].issuedDate,
                expiryDate: $scope._listForm[index].expiryDate,
                nonExpiry: $scope._listForm[index].nonExpiry,

                note: $scope._listForm[index].note,
                _default: $scope._listForm[index]._default,

            })
            $scope._listForm[index].IsEdit = false;
        });


        // === 
        var groupName = VaultInformationService.VaultInformation.groupFinancial.name;
        if (groupName == "")
            VaultInformationService.VaultInformation.groupFinancial.name = "Financial";

        //Save
        VaultInformationService.VaultInformation.groupFinancial.value.bankAccount = BasicInfo;
        VaultInformationService.VaultInformation.groupFinancial.value.bankAccount.value = lstFormSave;
        VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
            VaultInformationService.VaultInformation._id).then(function (response) {
                $scope.IsEdit = false;
                $scope._new = "";
                 $rootScope.$broadcast('bankAccount');
              

            }, function (errors) {

            });
    }

    $scope.saveVaultInformationOnSuccess = function (response) {
        $scope.IsEdit = false;
        $scope._new = "";
    }
    $scope.saveVaultInformationOnError = function (response) {
        alertService.renderSuccessMessage(response.ReturnMessage);
        $scope.messageBox = alertService.returnFormattedMessage();
        $scope.alerts = alertService.returnAlerts();
    }

    $scope.Cancel = function () {
        $scope.InitData();
    }

    $scope.goBack = function () {
        $scope.$location.path('/VaultInformation');
    };
    $scope.closePanel = function () {
        $scope.Cancel();
    };

}])

//4.2 Bank card 
myApp.getController('BankCardController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter', 'DocumentVaultService',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter, DocumentVaultService) {

    "use strict";
    var BasicInfo = VaultInformationService.VaultInformation.groupFinancial.value.bankCard;
    var _document = VaultInformationService.VaultInformation.document;
    $scope.InitData = function () {
        $scope.IsEdit = false;

        $scope._form = {
            _label: BasicInfo.label,
            _name: BasicInfo.name,
            _default: BasicInfo.default,
            _privacy: BasicInfo.privacy
        };
        //
        $scope._listForm = [];
        var maxindex = 0;

        $(BasicInfo.value).each(function (index, object) {
            if (maxindex < BasicInfo.value[index]._id)
                maxindex = BasicInfo.value[index]._id;
            var _jspath = "/Financial/Bank card/" + BasicInfo.value[index]._id;
            var today = new Date();
            var expiryNo = false;
            // indefinite
            var _indefiniteDate = true;
            if (BasicInfo.value[index].expiryDate == undefined)
                BasicInfo.value[index].expiryDate = "";
            if (BasicInfo.value[index].expiryDate != "")
                _indefiniteDate = false;
            // End indefinite


            $scope._listForm.push({
                _id: BasicInfo.value[index]._id,
                IsEdit: false,
                privacy: BasicInfo.value[index].privacy,
                description: BasicInfo.value[index].description,
                bankName: BasicInfo.value[index].bankName,
                bankBranch: BasicInfo.value[index].bankBranch,
                cardNumber: BasicInfo.value[index].cardNumber,

                bankAdress: BasicInfo.value[index].bankAdress,
                countryCity: { country: BasicInfo.value[index].country, city: BasicInfo.value[index].city },
                state: BasicInfo.value[index].state,
                zipCode: BasicInfo.value[index].zipCode,
                issuedDate: BasicInfo.value[index].issuedDate,
                expiryDate: BasicInfo.value[index].expiryDate,
                indefiniteDate: _indefiniteDate,
                expiryNo: expiryNo,
                nonExpiry: BasicInfo.value[index].nonExpiry,
                note: BasicInfo.value[index].note,
                _default: BasicInfo.value[index]._default,
                docJspath: _jspath

            });
            if ($scope._listForm[index].issuedDate != "" && $scope._listForm[index].issuedDate != undefined)
                $scope._listForm[index].issuedDate = new Date($scope._listForm[index].issuedDate);
            else
                $scope._listForm[index].issuedDate = new Date();
            if ($scope._listForm[index].expiryDate != "" && $scope._listForm[index].expiryDate != undefined) {
                $scope._listForm[index].expiryDate = new Date($scope._listForm[index].expiryDate);
                if ($scope._listForm[index].expiryDate < today)
                    $scope._listForm[index].expiryNo = true;
            }

          
        });

        maxindex = maxindex + 1;
        $rootScope._newJspath = "/Financial/Bank card/" + maxindex;
        $scope._new =
           {
               '_id': maxindex,
               'IsEdit': false,
               'privacy': "",
               'description': "",
               '_default': true,
               'bankName': "",
               'bankBranch': "",
               'cardNumber': "",
               'bankAdress': "",
               'countryCity': "",
               'state': "",
               'zipCode': "",
               'issuedDate': new Date('2000-01-01'),
               'expiryDate': '',
                'indefiniteDate': true,
               'nonExpiry': "",
               'note': "",

           };


        $scope.addNewForm = function (newForm) {
            if (newForm.indefiniteDate == true)
                newForm.expiryDate = '';

            $scope._listForm.push({
                '_id': newForm._id,
                'IsEdit': false,
                'privacy': newForm.privacy,
                'description': newForm.description,
                '_default': newForm._default,

                'bankName': newForm.bankName,
                'bankBranch': newForm.bankBranch,
                'cardNumber': newForm.cardNumber,
                'bankAdress': newForm.bankAdress,
                'countryCity': newForm.countryCity,
                'state': newForm.state,
                'zipCode': newForm.zipCode,
                'issuedDate': newForm.issuedDate,
                'expiryDate': newForm.expiryDate,
                'indefiniteDate': newForm.indefiniteDate,
                'nonExpiry': newForm.nonExpiry,
                'note': newForm.note,

            });

        }

        $scope.removeForm = function (_value) {
            var index = $scope._listForm.indexOf(_value);
            swal({
                title: "Are you sure to remove " + _value.description + " ?",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes",
                cancelButtonText: "No!"

            }).then(function () {
                $scope._listForm.splice(index, 1);
                $scope.Save();
                swal(
                    'Removed',
                     _value.description + ' has been removed.',
                    'success'
                  )
            }
            , function (dismiss) {
                if (dismiss === 'cancel') {

                }
            });

        }

        // Document
        $scope._listdocument = [];
        $(_document.value).each(function (index, object) {

            $scope._listdocument.push({
                _id: _document.value[index]._id,
                IsEdit: false,
                privacy: _document.value[index].privacy,
                name: _document.value[index].name,
                saveName: _document.value[index].saveName,
                category: _document.value[index].category,
                description: _document.value[index].description,
                type: _document.value[index].type,
                uploadDate: _document.value[index].uploadDate,
                jsPath: _document.value[index].jsPath,
                nosearch: _document.value[index].nosearch
            });
        });

        $scope.checkRemoveDoc = false;
        $scope.removeFormDoc = function (_value) {
            $scope.checkRemoveDoc = true;
            var _jsp = "/Financial/Bank card/" + _value._id;
            var model = new Object();
            model.UserID = $rootScope.ruserid;

            for (var i = $scope._listdocument.length - 1; i >= 0; i--) {
                if ($scope._listdocument[i].jsPath == _jsp) {
                    $scope._listdocument.splice(i, 1);
                }
            }

        }
        //
        $scope.messageBox = "";
        $scope.alerts = [];

    }
    $scope.$on('bankCard', function () {
        $scope.InitData();
    });
    $scope.InitData();
    $scope.Edit = function () {
        $scope.IsEdit = true;
    }

    // == edit == 
    $scope.Save = function () {
        BasicInfo.label = $scope._form._label;
        if ($scope._form._name == '') {
            $scope._form._name = $scope._form._label;
        }
        BasicInfo.name = $scope._form._name;
        BasicInfo.privacy = $scope._form._privacy;

        var checkDefault = true;
        $($scope._listForm).each(function (index) {

            if (checkDefault == true && $scope._listForm[index]._default == true && $scope._listForm[index].description != $scope._form._default) {
                $scope._form._default = $scope._listForm[index].description;
                checkDefault = false;
            }

        });

        BasicInfo.default = $scope._form._default;

        //
        var lstFormSave = [];
        $($scope._listForm).each(function (index, object) {
            if (checkDefault == false && $scope._listForm[index].description != $scope._form._default) {
                $scope._listForm[index]._default = false;
            }
            // indefinite
            if ($scope._listForm[index].nonExpiry == true)
                $scope._listForm[index].expiryDate = "";
            // end indefinite
            lstFormSave.push({
                _id: $scope._listForm[index]._id,
                privacy: $scope._listForm[index].privacy,
                description: $scope._listForm[index].description,
                bankName: $scope._listForm[index].bankName,
                bankBranch: $scope._listForm[index].bankBranch,
                cardNumber: $scope._listForm[index].cardNumber,
                bankAdress: $scope._listForm[index].bankAdress,
                city: $scope._listForm[index].countryCity.city,
                country: $scope._listForm[index].countryCity.country,
                state: $scope._listForm[index].state,
                zipCode: $scope._listForm[index].zipCode,
                issuedDate: $scope._listForm[index].issuedDate,
                expiryDate: $scope._listForm[index].expiryDate,
                nonExpiry: $scope._listForm[index].nonExpiry,

                note: $scope._listForm[index].note,
                _default: $scope._listForm[index]._default,

            })
            $scope._listForm[index].IsEdit = false;
        });

        //Document 

        var lstDocSave = [];
        $($scope._listdocument).each(function (index, object) {

            lstDocSave.push({
                _id: $scope._listdocument[index]._id,
                privacy: $scope._listdocument[index].privacy,
                name: $scope._listdocument[index].name,
                SaveName: $scope._listdocument[index].SaveName,
                description: $scope._listdocument[index].description,
                type: $scope._listdocument[index].type,
                category: $scope._listdocument[index].category,
                uploadDate: $scope._listdocument[index].uploadDate,
                jsPath: $scope._listdocument[index].jsPath,
                nosearch: $scope._listdocument[index].nosearch,

            })
            $scope._listdocument[index].IsEdit = false;
        });

        //End document
        // === 
        var groupName = VaultInformationService.VaultInformation.groupFinancial.name;
        if (groupName == "")
            VaultInformationService.VaultInformation.groupFinancial.name = "Financial";

        //Save
        VaultInformationService.VaultInformation.groupFinancial.value.bankCard = BasicInfo;
        VaultInformationService.VaultInformation.groupFinancial.value.bankCard.value = lstFormSave;
        if ($scope.checkRemoveDoc == true) {
            VaultInformationService.VaultInformation.document = _document;
            VaultInformationService.VaultInformation.document.value = lstDocSave;
        }
        VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
            VaultInformationService.VaultInformation._id).then(function (response) {
                $scope.IsEdit = false;
                $scope._new = "";
                $rootScope.$broadcast('bankCard');
              
                $scope.checkRemoveDoc == false;
                $rootScope._newJspath = "";
            }, function (errors) {

            });
    }

    $scope.saveVaultInformationOnSuccess = function (response) {
        $scope.IsEdit = false;
        $scope._new = "";
    }
    $scope.saveVaultInformationOnError = function (response) {
        alertService.renderSuccessMessage(response.ReturnMessage);
        $scope.messageBox = alertService.returnFormattedMessage();
        $scope.alerts = alertService.returnAlerts();
    }

    $scope.Cancel = function () {
        $scope.InitData();
    }

    $scope.goBack = function () {
        $scope.$location.path('/VaultInformation');
    };
    $scope.closePanel = function () {
        $scope.Cancel();
    };

}])

//4.3 Investment
myApp.getController('InvestmentController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter) {

    "use strict";
    var BasicInfo = VaultInformationService.VaultInformation.groupFinancial.value.investment;
    $scope.InitData = function () {
        $scope.IsEdit = false;

        $scope._form = {
            _label: BasicInfo.label,
            _name: BasicInfo.name,
            _default: BasicInfo.default,
            _privacy: BasicInfo.privacy
        };
        $scope._listForm = [];

        $(BasicInfo.value).each(function (index, object) {
            $scope._listForm.push({
                IsEdit: false,
                privacy: BasicInfo.value[index].privacy,
                description: BasicInfo.value[index].description,
                fullName: BasicInfo.value[index].fullName,
                accountType: BasicInfo.value[index].accountType,
                accountNumber: BasicInfo.value[index].accountNumber,
                investmentFirm: BasicInfo.value[index].investmentFirm,
                note: BasicInfo.value[index].note,
                _default: BasicInfo.value[index]._default,

            });

        });


        $scope._new =
           {
               'IsEdit': false,
               'privacy': "",
               'description': "",
               '_default': true,
               'fullName': "",
               'accountType': "",
               'accountNumber': "",
               'investmentFirm': "",
               'note': "",

           };


        $scope.addNewForm = function (newForm) {

            $scope._listForm.push({
                'IsEdit': false,
                'privacy': newForm.privacy,
                'description': newForm.description,
                '_default': newForm._default,
                'fullName': newForm.fullName,
                'accountType': newForm.accountType,
                'accountNumber': newForm.accountNumber,
                'investmentFirm': newForm.investmentFirm,

                'note': newForm.note,

            });
        }

        $scope.removeForm = function (_value) {
            var index = $scope._listForm.indexOf(_value);
            swal({
                title: "Are you sure to remove " + _value.description + " ?",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes",
                cancelButtonText: "No!"

            }).then(function () {
                $scope._listForm.splice(index, 1);
                $scope.Save();
                swal(
                    'Removed',
                     _value.description + ' has been removed.',
                    'success'
                  )
            }
            , function (dismiss) {
                if (dismiss === 'cancel') {

                }
            });
        };
        $scope.messageBox = "";
        $scope.alerts = [];

    }
    $scope.$on('investment', function () {
        $scope.InitData();
    });
    $scope.InitData();
    $scope.Edit = function () {
        $scope.IsEdit = true;
    }

    // == edit == 
    $scope.Save = function () {
        BasicInfo.label = $scope._form._label;
        if ($scope._form._name == '') {
            $scope._form._name = $scope._form._label;
        }
        BasicInfo.name = $scope._form._name;
        BasicInfo.privacy = $scope._form._privacy;
        var checkDefault = true;
        $($scope._listForm).each(function (index) {

            if (checkDefault == true && $scope._listForm[index]._default == true && $scope._listForm[index].description != $scope._form._default) {
                $scope._form._default = $scope._listForm[index].description;
                checkDefault = false;
            }

        });

        BasicInfo.default = $scope._form._default;
        var lstFormSave = [];
        $($scope._listForm).each(function (index, object) {
            if (checkDefault == false && $scope._listForm[index].description != $scope._form._default) {
                $scope._listForm[index]._default = false;
            }
            lstFormSave.push({
                privacy: $scope._listForm[index].privacy,

                description: $scope._listForm[index].description,
                fullName: $scope._listForm[index].fullName,
                accountType: $scope._listForm[index].accountType,
                accountNumber: $scope._listForm[index].accountNumber,
                investmentFirm: $scope._listForm[index].investmentFirm,

                note: $scope._listForm[index].note,
                _default: $scope._listForm[index]._default,

            })
            $scope._listForm[index].IsEdit = false;
        });

        var groupName = VaultInformationService.VaultInformation.groupFinancial.name;
        if (groupName == "")
            VaultInformationService.VaultInformation.groupFinancial.name = "Financial";

        //Save
        VaultInformationService.VaultInformation.groupFinancial.value.investment = BasicInfo;
        VaultInformationService.VaultInformation.groupFinancial.value.investment.value = lstFormSave;
        VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
            VaultInformationService.VaultInformation._id).then(function (response) {
                $scope.IsEdit = false;
                $scope._new = "";
                $rootScope.$broadcast('investment');
               
            }, function (errors) {

            });
    }

    $scope.saveVaultInformationOnSuccess = function (response) {
        $scope.IsEdit = false;
        $scope._new = "";

    }
    $scope.saveVaultInformationOnError = function (response) {
        alertService.renderSuccessMessage(response.ReturnMessage);
        $scope.messageBox = alertService.returnFormattedMessage();
        $scope.alerts = alertService.returnAlerts();
    }

    $scope.Cancel = function () {
        $scope.InitData();
    }

    $scope.goBack = function () {
        $scope.$location.path('/VaultInformation');
    };
    $scope.closePanel = function () {
        $scope.Cancel();
    };

}])

//4.4 Insurance
myApp.getController('InsuranceController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'dateFilter',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, dateFilter) {

    "use strict";
    var BasicInfo = VaultInformationService.VaultInformation.groupFinancial.value.insurance;
    $scope.InitData = function () {
        $scope.IsEdit = false;

        $scope._form = {
            _label: BasicInfo.label,
            _name: BasicInfo.name,
            _default: BasicInfo.default,
            _privacy: BasicInfo.privacy
        };
        //
        $scope._listForm = [];

        $(BasicInfo.value).each(function (index, object) {
            var today = new Date();
            var expiryNo = false;
            // indefinite
            var _indefiniteDate = true;
            if (BasicInfo.value[index].expiryDate == undefined)
                BasicInfo.value[index].expiryDate = "";
            if (BasicInfo.value[index].expiryDate != "")
                _indefiniteDate = false;
            // End indefinite
            $scope._listForm.push({
                IsEdit: false,
                privacy: BasicInfo.value[index].privacy,
                description: BasicInfo.value[index].description,
                insurer: BasicInfo.value[index].insurer,
                insuranceType: BasicInfo.value[index].insuranceType,
                policyNumber: BasicInfo.value[index].policyNumber,
                startDate: BasicInfo.value[index].startDate,
                expiryDate: BasicInfo.value[index].expiryDate,
                indefiniteDate: _indefiniteDate,
                expiryNo: expiryNo,
                note: BasicInfo.value[index].note,
                _default: BasicInfo.value[index]._default,

            });
            if ($scope._listForm[index].startDate != "" && $scope._listForm[index].startDate != undefined)
                $scope._listForm[index].startDate = new Date($scope._listForm[index].startDate);
            else
                $scope._listForm[index].startDate = new Date();

            if ($scope._listForm[index].expiryDate != "" && $scope._listForm[index].expiryDate != undefined) {
                $scope._listForm[index].expiryDate = new Date($scope._listForm[index].expiryDate);
                if ($scope._listForm[index].expiryDate < today)
                    $scope._listForm[index].expiryNo = true;
            }
           
        });

        $scope._new =
           {
               'IsEdit': false,
               'privacy': "",
               'description': "",
               '_default': true,
               'insurer': "",
               'insuranceType': "",
               'policyNumber': "",
               'startDate': new Date('2000-01-01'),
               'expiryDate':'',
               'indefiniteDate': true,
               'note': "",

           };

        $scope.addNewForm = function (newForm) {
            if (newForm.indefiniteDate == true)
                newForm.expiryDate = '';
            $scope._listForm.push({
                'IsEdit': false,
                'privacy': newForm.privacy,
                'description': newForm.description,
                '_default': newForm._default,
                'insurer': newForm.insurer,
                'insuranceType': newForm.insuranceType,
                'policyNumber': newForm.policyNumber,
                'startDate': newForm.startDate,
                'expiryDate': newForm.expiryDate,
                'indefiniteDate': newForm.indefiniteDate,
                'note': newForm.note,
            });

        }

        $scope.removeForm = function (_value) {
            var index = $scope._listForm.indexOf(_value);
            swal({
                title: "Are you sure to remove " + _value.description + " ?",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes",
                cancelButtonText: "No"

            }).then(function () {
                $scope._listForm.splice(index, 1);
                $scope.Save();
                swal(
                    'Removed',
                     _value.description + ' has been removed.',
                    'success'
                  )
            }
            , function (dismiss) {
                if (dismiss === 'cancel') {

                }
            });

        };


        //
        $scope.messageBox = "";
        $scope.alerts = [];
    }
    $scope.$on('insurance', function () {
        $scope.InitData();
    });
    $scope.InitData();
    $scope.Edit = function () {
        $scope.IsEdit = true;
    }

    // == edit == 
    $scope.Save = function () {

        BasicInfo.label = $scope._form._label;
        if ($scope._form._name == '') {
            $scope._form._name = $scope._form._label;
        }
        BasicInfo.name = $scope._form._name;
        BasicInfo.privacy = $scope._form._privacy;

        var checkDefault = true;
        $($scope._listForm).each(function (index) {

            if (checkDefault == true && $scope._listForm[index]._default == true && $scope._listForm[index].description != $scope._form._default) {
                $scope._form._default = $scope._listForm[index].description;
                checkDefault = false;
            }
        });
        BasicInfo.default = $scope._form._default;

        var lstFormSave = [];
        $($scope._listForm).each(function (index, object) {
            if (checkDefault == false && $scope._listForm[index].description != $scope._form._default) {
                $scope._listForm[index]._default = false;
            }
            // indefinite
            if ($scope._listForm[index].indefiniteDate == true)
                $scope._listForm[index].expiryDate = "";
            // end indefinite
            lstFormSave.push({
                privacy: $scope._listForm[index].privacy,

                description: $scope._listForm[index].description,
                insurer: $scope._listForm[index].insurer,

                insuranceType: $scope._listForm[index].insuranceType,
                policyNumber: $scope._listForm[index].policyNumber,
                startDate: $scope._listForm[index].startDate,
                expiryDate: $scope._listForm[index].expiryDate,

                note: $scope._listForm[index].note,
                _default: $scope._listForm[index]._default,

            })
            $scope._listForm[index].IsEdit = false;
        });


        // === 
        var groupName = VaultInformationService.VaultInformation.groupFinancial.name;
        if (groupName == "")
            VaultInformationService.VaultInformation.groupFinancial.name = "Financial";

        VaultInformationService.VaultInformation.groupFinancial.value.insurance = BasicInfo;
        VaultInformationService.VaultInformation.groupFinancial.value.insurance.value = lstFormSave;
        VaultInformationService.SaveVaultInformation(VaultInformationService.VaultInformation,
            VaultInformationService.VaultInformation._id).then(function (response) {
                $scope.IsEdit = false;
                $scope._new = "";
                $rootScope.$broadcast('insurance');
              
            }, function (errors) {

            });
    }

    $scope.saveVaultInformationOnSuccess = function (response) {
        $scope.IsEdit = false;
        $scope._new = "";
    }
    $scope.saveVaultInformationOnError = function (response) {
        alertService.renderSuccessMessage(response.ReturnMessage);
        $scope.messageBox = alertService.returnFormattedMessage();
        $scope.alerts = alertService.returnAlerts();
    }

    $scope.Cancel = function () {
        $scope.InitData();
    }

    $scope.goBack = function () {
        $scope.$location.path('/VaultInformation');
    };
    $scope.closePanel = function () {
        $scope.Cancel();
    };

}]);
