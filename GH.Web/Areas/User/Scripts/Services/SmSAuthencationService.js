var _userApp = getApp('UserModule');
//OpenPopupAuthencationSMS
_userApp.factory('SmSAuthencationService', ['$http', '$q', '$cookies', '$interval', '$timeout', '$uibModal', function ($http, $q, $cookies, $interval, $timeout, $uibModal) {

    var GetQuerystring = function (name, url) {
        if (!url) url = window.location.href;
        name = name.replace(/[\[\]]/g, "\\$&");
        var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
            results = regex.exec(url);
        if (!results) return null;
        if (!results[2]) return '';
        return decodeURIComponent(results[2].replace(/\+/g, " "));
    }
    return {
            OpenPopupAuthencationPIN: function (okCallback, cancelCallback) {
                $http.post('/Api/Account/IsCheckPinVault', null)
           .success(function (isauthencation) {
               if (!isauthencation) {
                   var modalInstance = $uibModal.open({
                       templateUrl: '/Areas/User/Views/Shared/Template/PINAuthencation.html',
                       controller: 'PinAuthencationPoPupController',
                       size: "",
                       resolve: {
                       }
                   });
                   modalInstance.result.then(function (status) {
                       if (status == "OK") {
                           if (okCallback) okCallback();
                       }
                   }, function (status) {
                       if (cancelCallback) cancelCallback();
                   });
               }
               else {
                   if (okCallback) okCallback();
               }
           })
        },
            OpenPopupAuthencationSMS:function(okCallback, cancelCallback){
            $http.post('/Api/Account/Verify/IsSMSAuthenticated', null)
           .success(function (isauthencation) {
                  if (!isauthencation)
                  {
                      var modalInstance = $uibModal.open({
                          templateUrl: '/Areas/User/Views/Shared/Template/SMSAuthencation.html',
                          controller: 'SMSAuthencationPoPupController',
                          size: "",
                          resolve: {
                          }
                      });
                      modalInstance.result.then(function (status) {
                          if (status == "OK") {
                              if(okCallback) okCallback();
                          }
                      }, function (status) {
                          if (cancelCallback) cancelCallback();
                      });
                  }
                  else
                  {
                      if (okCallback) okCallback();
                  }
           })
            },
            OpenPopupAuthencationSMSEmail: function (okCallback, cancelCallback) {
                $http.post('/Api/Account/Verify/IsSMSAuthenticated', null)
               .success(function (isauthencation) {
                   if (!isauthencation) {
                       var modalInstance = $uibModal.open({
                           templateUrl: '/Areas/User/Views/Shared/Template/SMSEmailAuthencation.html',
                           controller: 'SMSEmailAuthencationPoPupController',
                           size: "",
                           resolve: {
                           }
                       });
                       modalInstance.result.then(function (status) {
                           if (status == "OK") {
                               if (okCallback) okCallback();
                           }
                       }, function (status) {
                           if (cancelCallback) cancelCallback();
                       });
                   }
                   else {
                       if (okCallback) okCallback();
                   }
               })
            },
            OpenPopupAuthencationSMSNoSession: function (phonenumber,okCallback, cancelCallback) {
                var modalInstance = $uibModal.open({
                    templateUrl: '/Areas/User/Views/Shared/Template/SMSAuthencation.html',
                    controller: 'SMSAuthencationPoPupNoSessionController',
                    size: "",

                    
                    resolve: {
                        registerPopup: function () {
                            return {
                                newphonenumber: phonenumber
                               
                                //,currentBusinessId: businessId
                                //,currentCampaignType: campaignType
                            };
                        }
                    }
                   
                });
                modalInstance.result.then(function (status) {
                    if (status == "OK") {
                        if (okCallback) okCallback();
                    }
                }, function (status) {
                    if (cancelCallback) cancelCallback();
                });
            },

            OpenRedirect: function (typeRedirect) {
                $http.post('/Api/Account/IsCheckPinVault', null)
                .success(function (isauthencation) {
                  if (typeRedirect == "Vault") {
                    //  if (!isauthencation)
                    //     window.location.href = "/User/SMSAuthencation?TypeRedirect=Vault";
                    //else
                        window.location.href = "/VaultInformation/Index";
                  }
                  else if (typeRedirect == "Delegate")
                    {
                    
                      //if (!isauthencation)
                      //    window.location.href = "/User/SMSAuthencation?TypeRedirect=Delegate";
                      //else
                          window.location.href = "/DelegationManager/Index";
                  }
              })
            },
            CallbackOkAuthencationSMS: function () {
                typeRedirect = GetQuerystring("TypeRedirect")
                if (typeRedirect=="Vault")
                    window.location.href = "/VaultInformation/Index";
                else if (typeRedirect == "Delegate")
                    window.location.href = "/DelegationManager/Index";
           },
            CallbackCancelAuthencationSMS: function () {
                typeRedirect = GetQuerystring("TypeRedirect")
                if (typeRedirect == "Vault")
                    window.location.href = "/";
                else if (typeRedirect == "Delegate")
                    window.location.href = "/";
           }
    }
}])
