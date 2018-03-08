angular.module("regitApp")
    .controller("LocationMapCtrl", function ($scope, uiGmapGoogleMapApi, $geolocation) {
        $scope.coords = {
            latitude: 10,
            longitude: 106
        };
        $scope.map = {
            center: {
                latitude: 10,
                longitude: 106
            },
            zoom: 17
        };
        uiGmapGoogleMapApi.then(function (maps) {
            maps.visualRefresh = true;
            $scope.maps = maps;
            $scope.geocoder = new maps.Geocoder();

        });
        $scope.marker = {
            id: 0,
            coords: $scope.coords,
            options: {draggable: true},
            events: {
                position_changed: function (marker, eventName, args) {
                    $scope.coords.latitude = marker.getPosition().lat();
                    $scope.coords.longitude = marker.getPosition().lng();
                    // $scope.map.center = $scope.coords;
                }
            }
        };

        $geolocation.getCurrentPosition({
            timeout: 60000
        }).then(function (position) {
            $scope.centerMapTo(position.coords.latitude,position.coords.longitude);
        });

        $scope.centerMapTo = function (latitude, longitude) {
            $scope.coords.latitude = latitude;
            $scope.coords.longitude = longitude;
            $scope.marker.coords.latitude = latitude;
            $scope.marker.coords.longitude = longitude;
            $scope.map.center.latitude = latitude;
            $scope.map.center.longitude = longitude;
        };

        /*        $geolocation.watchPosition({
         timeout: 60000,
         maximumAge: 250,
         enableHighAccuracy: true
         });*/

        /*        $scope.$watchCollection('event.location', function (newValue, oldValue) {
         if (!$scope.geocoder) return;
         var geocodeOptions =
         {
         address: newValue,
         // region: 'vn',
         componentRestrictions: {
         country: 'VN'
         }
         };
         /!*            $scope.geocoder.geocode(geocodeOptions, function (results, status) {
         if (status === 'OK' && results.length > 0) {
         var location = results[0].geometry.location;
         console.log(results[0].formatted_address,results[0].partial_match);
         $scope.coords.latitude = location.lat();
         $scope.coords.longitude = location.lng();
         // $scope.map.panTo(location);
         }
         })*!/
         });*/
        $scope.gmapOptions = {
            fullscreenControl: true,
            scrollwheel: false
        };
        $scope.gmapSearchEvents = {
            places_changed: function (searchBox) {
                var places = searchBox.getPlaces();
                // console.log(places.length);
                if (!places.length || !places[0].geometry) return;

                var location = places[0].geometry.location;
                if (location) {
                    $scope.centerMapTo(location.lat(), location.lng());
                }
            }
        };
        $scope.gmapSearchOptions = {
            // autocomplete: true
        };

    });



