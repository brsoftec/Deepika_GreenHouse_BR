var _userApp = getApp('UserModule');

_userApp.factory('CountryCityService', ['$http', '$q', '$cookies', '$interval','$timeout', function ($http, $q, $cookies, $interval, $timeout) {
   
    return {
        InitData: function (cityName, countryName) {
          var deferer = $q.defer();
          var parentthis = this;
            $http.post('/api/LocationService/GetAllCountries', { CountryName: countryName })
            .success(function (response) {
                parentthis.Countries = response.Countries;
                if (countryName != null && countryName != undefined) {
                    parentthis.Country = parentthis.Findobjectbyname(countryName, parentthis.Countries);
                    if (parentthis.Country != null) {
                        parentthis.Cities = response.Cities;
                        if (cityName != null)
                            parentthis.City = parentthis.Findobjectbyname(cityName, parentthis.Cities);
                    }
                }
                deferer.resolve();
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            });
          return deferer.promise;;
        },
        City:null,
        Country:null,

        Findobjectbyname:function(name,list)
        {
          var object=new Object();
             $(list).each(function(index){
            if(list[index].Name==name)
              {  
                object=list[index];
                return;
              }  
        })

        return object;
        },
        FindCountryByName:function(name){
           var countries=this.GetCountries();
           return this.Findobjectbyname(name, countries);
        },
         FindCityByName:function(name){
             return this.Findobjectbyname(name, this.Cities);
        },     

        Countries: [],
        Cities:[],
        GetCountries: null,
        GetCitiesByCountryID: function (countryCode) {
             var deferer = $q.defer();

            $http.post('/api/LocationService/GetCitiesById', { CountryCode: countryCode })
                .success(function (response) {
                    deferer.resolve(response);
                }).error(function (errors, status) {
                    __promiseHandler.Error(errors, status, deferer);
                });
            return deferer.promise;
        }
        
    }
}])
