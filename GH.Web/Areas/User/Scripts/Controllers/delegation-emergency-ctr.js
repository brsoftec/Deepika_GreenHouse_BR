//1. Basic Infomation 
myApp.getController('emergencyBasicController',
      ['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'vaultService', 'NetworkService', 'rguNotify', 'VaultService',
          function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, vaultService, _networkService, rguNotify, _vaultService) {

              var vaultData = null;
              $scope.formData = null;
              $scope.TestBasicForm = null;
              var VaultForm = null;
              $scope.countryCity = null;
              $scope.initForm = function (value) {

                  // Init all Form
                  var formVault = {
                      'FormName': $scope.query.groupName,
                      'AccountId': $scope.delegateSearch.FromAccountId
                  }

                  // Begin init all Form
                  _vaultService.GetFormVault(formVault).then(function (response) {
                      vaultData = response;
                      $scope.formData = response;
                      VaultForm = response.value[$scope.query.formName];
                      $scope.TestBasicForm = response.value[$scope.query.formName];
                      $scope.initDataForm();
                  });
                  // End init all Form
              }

              $scope.initSearchVault();
              // End manual push to vault
              $scope.initDataForm = function () {
                  /* Permission */
                  $scope.read = false;
                  $scope.write = false;
                  var lstPermission = [];
                  if ($scope.query.mySelf == false) {
                      if ($scope.delegateSearch.GroupVaultsPermission != undefined || $scope.delegateSearch.GroupVaultsPermission == '') {

                          lstPermission = $scope.delegateSearch.GroupVaultsPermission;
                          for (var i = 0; i < lstPermission.length; i++) {

                              if (lstPermission[i].jsonpath == "basicInformation") {
                                  $scope.read = lstPermission[i].read;
                                  $scope.write = lstPermission[i].write;
                              }
                          };
                      }
                  }
                  else {
                      $scope.read = true;
                      $scope.write = true;
                  }
                  /* End Permission */
                  $scope.IsEdit = false;


                  //Title
                  var _id = "";
                  if (VaultForm._id === undefined || VaultForm._id === null) {
                      VaultForm._id = '';
                  }
                  if (VaultForm.options === undefined || VaultForm.options === null) {
                      VaultForm.options = '';
                  }
                  if (VaultForm.type === undefined || VaultForm.type === null) {
                      VaultForm.type = '';
                  }
                  if (VaultForm.rules === undefined || VaultForm.rules === null) {
                      VaultForm.rules = '';
                  }
                  $scope.field = {
                      '_id': VaultForm._id,
                      'label': VaultForm.label,
                      'value': VaultForm.value,
                      'privacy': VaultForm.privacy,
                      'options': VaultForm.options,
                      'type': VaultForm.type,
                      'rules': VaultForm.rules
                  }
                  if ($scope.field.label == 'City' || $scope.field.label == 'Country') {
                      $scope.countryCity = { country: vaultData.value.country.value, city: vaultData.value.city.value };
                  }

                  $scope.messageBox = "";
                  $scope.alerts = [];
              }

              $scope.Edit = function () {
                  $scope.IsEdit = true;
              }

              $scope.Save = function () {

                  // Save Form Vault
                  VaultForm = $scope.field;
                  vaultData.value[$scope.query.formName] = VaultForm;
                  if ($scope.field.label == 'City' || $scope.field.label == 'Country') {
                      vaultData.value.country.value = $scope.countryCity.country;
                      vaultData.value.city.value = $scope.countryCity.city;
                  }

                  var saveVaultForm = {
                      'AccountId': $scope.delegateSearch.FromAccountId,
                      'FormName': $scope.query.groupName,
                      'FormString': vaultData
                  }

                  _vaultService.UpdateFormVault(saveVaultForm).then(function () {

                      $scope.IsEdit = false;

                      $scope.UpdateDataSearch();

                  }, function (errors) {
                      swal('Error', errors, 'error');
                  })

                  // End Save Form Vault      
              }

              $scope.Cancel = function () {
                  $scope.initSearchVault();
              }

          }])


myApp.getController('emergencyCurrentAddressController',
      ['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'vaultService', 'NetworkService', 'rguNotify', 'VaultService',
          function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, vaultService, _networkService, rguNotify, _vaultService) {
              //InitData
              var VaultForm = null;
              var vaultData = null;
             
              $scope.initFormEmergency = function () {
                  // Init all Form
                  var formVault = {
                      'FormName': 'groupAddress',
                      'AccountId': $scope.getFormEmergency.accountId
                  }

                  // Begin init all Form
                  _vaultService.GetFormVault(formVault).then(function (response) {
                      vaultData = response;
                      VaultForm = response.value[$scope.getFormEmergency.formName];
                     
                  });
                  // End init all Form
              }
              $scope.initFormEmergency();
              // End init data
              $scope.newForm ={
                'IsEdit': false,
                'privacy': true,
                'description': "",
                '_default': true,
                'startDate': new Date(),
                'endDate': '',
                'checkEndDate': false,
                'addressLine': "",
                'addressLine_lat': "",
                'addressLine_lng': "",
                'instruction': "",
                'countryCity': "",
                'state': "",
                'zipCode': "",
                'note': ""

              };
              // == edit == 
              $scope.Save = function () {

                  //address
                
                  if (VaultForm.name == '') {
                      VaultForm.name = VaultForm.label;
                  }
                  VaultForm.name = VaultForm.label;
                  VaultForm.default = $scope.newForm.description

                  var lstFormSave = [];
                  if ($scope.newForm.checkEndDate == false)
                      $scope.newForm.endDate = '';
                  lstFormSave.push({
                      _id: 1,
                      privacy: $scope.newForm.privacy,
                      description: $scope.newForm.description,
                      _default: $scope.newForm._default,
                      startDate: $scope.newForm.startDate,
                      endDate: $scope.newForm.endDate,
                      addressLine: $scope.newForm.addressLine,
                      addressLine_lat: $scope.newForm.addressLine_lat,
                      addressLine_lng: $scope.newForm.addressLine_lng,
                      instruction: $scope.newForm.instruction,
                      country: $scope.newForm.countryCity.country,
                      city: $scope.newForm.countryCity.city,
                      state: $scope.newForm.state,
                      zipCode: $scope.newForm.zipCode,
                      note: $scope.newForm.note

                  });

                  // Save Form Vault countryCity
                  VaultForm.value = lstFormSave;
                  vaultData.value[$scope.getFormEmergency.formName] = VaultForm;

                  var saveVaultForm = {
                      'AccountId': $scope.getFormEmergency.accountId,
                      'FormName': 'groupAddress',
                      'FormString': vaultData
                  }

                  _vaultService.UpdateFormVault(saveVaultForm).then(function () {
                    
                      $rootScope.cancelAddDataEmergency();

                  }, function (errors) {
                      swal('Error', errors, 'error');
                  })

                  // End Save Form Vault      
              }

              $scope.Cancel = function () {
                  $rootScope.cancelAddDataEmergency();
              }

          }])


myApp.getController('emergencyHealthCardController',
      ['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'vaultService', 'NetworkService', 'rguNotify', 'VaultService',
          function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, vaultService, _networkService, rguNotify, _vaultService) {
              //InitData
              var VaultForm = null;
              var vaultData = null;

              $scope.initFormEmergency = function () {
                  // Init all Form
                  var formVault = {
                      'FormName': 'groupGovernmentID',
                      'AccountId': $scope.getFormEmergency.accountId
                  }

                  // Begin init all Form
                  _vaultService.GetFormVault(formVault).then(function (response) {
                      vaultData = response;
                      VaultForm = response.value[$scope.getFormEmergency.formName];
                  });
                  // End init all Form
              }


              $scope.initFormEmergency();
              // End init data
          $scope.newForm =
          {

              'IsEdit': false,
              'privacy': true,
              'description': "",
              '_default': true,
              'firstName': "",
              'middleName': "",
              'lastName': "",
              'cardNumber': "",
              'bloodType': "O",
              'issuedDate': new Date('1960-01-01'),
              'expiryDate': '',
              'indefiniteDate': true,
              'note': "",

          };
              // == edit == 
          $scope.Save = function () {

              //address

              if (VaultForm.name == '') {
                  VaultForm.name = VaultForm.label;
              }
              VaultForm.name = VaultForm.label;
              VaultForm.default = $scope.newForm.description

              var lstFormSave = [];
              if ($scope.newForm.indefiniteDate == false)
                  $scope.newForm.expiryDate = '';
              lstFormSave.push({
                  _id: 1,
                  privacy: $scope.newForm.privacy,
                  description: $scope.newForm.description,
                  _default: $scope.newForm._default,
                  firstName: $scope.newForm.firstName,
                  middleName: $scope.newForm.middleName,
                  lastName: $scope.newForm.lastName,
                  cardNumber: $scope.newForm.cardNumber,
                  bloodType: $scope.newForm.bloodType,
                  issuedDate: $scope.newForm.issuedDate,
                  expiryDate: $scope.newForm.expiryDate,
                 
                  note: $scope.newForm.note

              });

              // Save Form Vault countryCity
              VaultForm.value = lstFormSave;
              vaultData.value[$scope.getFormEmergency.formName] = VaultForm;
              var saveVaultForm = {
                  'AccountId': $scope.getFormEmergency.accountId,
                  'FormName': 'groupGovernmentID',
                  'FormString': vaultData
              }

              _vaultService.UpdateFormVault(saveVaultForm).then(function () {

                  $rootScope.cancelAddDataEmergency();

              }, function (errors) {
                  swal('Error', errors, 'error');
              })

              // End Save Form Vault      
          }

          $scope.Cancel = function () {
              $rootScope.cancelAddDataEmergency();
          }
          }])

myApp.getController('emergencyPassportIDController',
      ['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'vaultService', 'NetworkService', 'rguNotify', 'VaultService',
          function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, vaultService, _networkService, rguNotify, _vaultService) {
              //InitData
              var VaultForm = null;
              var vaultData = null;

              $scope.initFormEmergency = function () {
                  // Init all Form
                  var formVault = {
                      'FormName': 'groupGovernmentID',
                      'AccountId': $scope.getFormEmergency.accountId
                  }

                  // Begin init all Form
                  _vaultService.GetFormVault(formVault).then(function (response) {
                      vaultData = response;
                      VaultForm = response.value[$scope.getFormEmergency.formName];
                  });
                  // End init all Form
              }


              $scope.initFormEmergency();
              // End init data
              $scope.newForm = {
                 
                  'IsEdit': false,
                  'privacy': true,
                  'description': "",
                  '_default': true,
                  'firstName': "",
                  'middleName': "",
                  'lastName': "",

                  'nationality': "",
                  'cardNumber': "",

                  'issuedDate': new Date('1960-01-01'),
                  'expiryDate': '',
                  'indefiniteDate': true,
                  'issuedBy': "",
                  'issuedIn': "",

                  'note': ""

              };

              // == edit == 
              $scope.Save = function () {

                  //address

                  if (VaultForm.name == '') {
                      VaultForm.name = VaultForm.label;
                  }
                  VaultForm.name = VaultForm.label;
                  VaultForm.default = $scope.newForm.description

                  var lstFormSave = [];
                  if ($scope.newForm.indefiniteDate == false)
                      $scope.newForm.expiryDate = '';
                  lstFormSave.push({
                      _id: 1,
                      privacy: $scope.newForm.privacy,
                      description: $scope.newForm.description,
                      _default: $scope.newForm._default,
                      firstName: $scope.newForm.firstName,
                      middleName: $scope.newForm.middleName,
                      lastName: $scope.newForm.lastName,
                      nationality: $scope.newForm.nationality,
                      cardNumber: $scope.newForm.cardNumber,
                      issuedDate: $scope.newForm.issuedDate,
                      expiryDate: $scope.newForm.expiryDate,
                      issuedBy: $scope.newForm.issuedBy,
                      issuedIn: $scope.newForm.issuedIn,
                      note: $scope.newForm.note

                  });




                  // Save Form Vault countryCity
                  VaultForm.value = lstFormSave;
                  vaultData.value[$scope.getFormEmergency.formName] = VaultForm;

                  var saveVaultForm = {
                      'AccountId': $scope.getFormEmergency.accountId,
                      'FormName': 'groupGovernmentID',
                      'FormString': vaultData
                  }

                  _vaultService.UpdateFormVault(saveVaultForm).then(function () {

                      $rootScope.cancelAddDataEmergency();

                  }, function (errors) {
                      swal('Error', errors, 'error');
                  })
              }
                  // End Save Form Vault   
              $scope.Cancel = function () {
                  $rootScope.cancelAddDataEmergency();
              }
          }])


myApp.getController('emergencyAllergiesController',
      ['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'CountryCityService', 'vaultService', 'NetworkService', 'rguNotify', 'VaultService',
          function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService, CountryCityService, vaultService, _networkService, rguNotify, _vaultService) {
              //InitData
              var VaultForm = null;
              var vaultData = null;
              $scope.tess = null;
              $scope.initFormEmergency = function () {
                  // Init all Form
                  var formVault = {
                      'FormName': 'others',
                      'AccountId': $scope.getFormEmergency.accountId
                  }

                  // Begin init all Form
                  _vaultService.GetFormVault(formVault).then(function (response) {
                      vaultData = response;
                      VaultForm = response.value['preference'].value[$scope.getFormEmergency.formName];
                      $scope.tess = response.value['preference'].value[$scope.getFormEmergency.formName];
                  });
                  // End init all Form
              }


              $scope.initFormEmergency();
              // End init data
              $scope.field = {
                  'label': 'Allergies',
                  'value': []
              };

              // == edit == 
              $scope.Save = function () {
                  // Save Form Vault countryCity
                  VaultForm = $scope.field;
                  vaultData.name = vaultData.label;
                  vaultData.value['preference'].value[$scope.getFormEmergency.formName] = VaultForm;

                  var saveVaultForm = {
                      'AccountId': $scope.getFormEmergency.accountId,
                      'FormName': 'others',
                      'FormString': vaultData
                  }

                  _vaultService.UpdateFormVault(saveVaultForm).then(function () {
                      $rootScope.cancelAddDataEmergency();

                  }, function (errors) {
                      swal('Error', errors, 'error');
                  })
              }
              // End Save Form Vault   
              $scope.Cancel = function () {
                  $rootScope.cancelAddDataEmergency();
              }

          }])
