

var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select'], true);
myApp.getController('CountryCityController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService','CountryCityService',
function ($scope, $rootScope, $http, _userManager, _sweetAlert, _authService, alertService, VaultInformationService,CountryCityService) {
 
  $scope.listCountries = CountryCityService.GetCountries();
  $scope.changeCity = function () {
      if($scope.Country!=null)
         $scope.listCities= CountryCityService.GetCitiesByCountryID($scope.Country.ID);
      else
        $scope.listCities = [];
  }
  
  if ($scope.NameCountry && $scope.NameCountry !== "")
  {
      $scope.Country = CountryCityService.FindCountryByName($scope.NameCountry);
      $scope.listCities = CountryCityService.GetCitiesByCountryID($scope.Country.ID);
      if ($scope.NameCity && $scope.NameCity !== "")
      {
          $scope.City = CountryCityService.FindCityByName( $scope.NameCity);
      }
   }

  
}])


