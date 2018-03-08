angular.module('location', ['uiGmapgoogle-maps', 'ngGeolocation'])

    .config(function (uiGmapGoogleMapApiProvider) {
        uiGmapGoogleMapApiProvider.configure({
            key: 'AIzaSyA-2XVNO9Opo0LpuhXD7Al8uG93es0rIGE',
            // v: '3.24',
            libraries: 'places'
        });
    })

    .directive('locationMap', function () {
        return {
            restrict: 'AE',
            templateUrl: '/Areas/regitUI/templates/location-map.html',
            link: function(scope,elem,attrs) {
                scope.locationInputId = attrs['inputid'];
                scope.country = attrs['country'];
            },
            controller: function ($scope, uiGmapGoogleMapApi, $geolocation) {
                $scope.coords = $scope.coords || {
                        latitude: 0,
                        longitude: 0
                    };
                $scope.map = {
                    center: {
                        latitude: $scope.coords.latitude,
                        longitude: $scope.coords.longitude
                    },
                    zoom: 17
                };
                uiGmapGoogleMapApi.then(function (maps) {
                    maps.visualRefresh = true;
                    $scope.maps = maps;
                    // $scope.geocoder = new maps.Geocoder();
                    if (!$scope.coords || !$scope.coords.latitude) {
                        $geolocation.getCurrentPosition({
                            timeout: 60000
                        }).then(function (position) {
                            $scope.centerMapTo(position.coords.latitude, position.coords.longitude);
                        });
                    }
                    var input = document.getElementById($scope.locationInputId);
                    var options = {
                        // types: ['address']
                    };
                    if ($scope.country) {
                        options.componentRestrictions = {country: $scope.country}
                    }
                    var autocomplete = new $scope.maps.places.Autocomplete(input, options);
                    autocomplete.addListener('place_changed', function () {
                        var place = autocomplete.getPlace();
                        if (!place.geometry) return;
                        var location = place.geometry.location;
                        if (location) {
                            $scope.$apply(function () {
                                $scope.centerMapTo(location.lat(), location.lng());
                                $scope.mapRefresh = true;
                                $scope.locationDirty = true;
                                angular.element(input).trigger('input');
                            });
                        }
                    });

                });

                $scope.marker = {
                    id: 0,
                    coords: $scope.coords,
                    options: {draggable: true},
                    events: {
                        position_changed: function (marker, eventName, args) {
                            $scope.coords.latitude = marker.getPosition().lat();
                            $scope.coords.longitude = marker.getPosition().lng();
                            $scope.locationDirty = true;
                        }
                    }
                };

                $scope.centerMapTo = function (latitude, longitude) {
                    $scope.coords.latitude = latitude;
                    $scope.coords.longitude = longitude;

                    $scope.map.center.latitude = latitude;
                    $scope.map.center.longitude = longitude;

                    $scope.marker.coords.latitude = latitude;
                    $scope.marker.coords.longitude = longitude;
                };

                /*        $geolocation.watchPosition({
                 timeout: 60000,
                 maximumAge: 250,
                 enableHighAccuracy: true
                 });*/

                /*            $scope.geocoder.geocode(geocodeOptions, function (results, status) {
                 if (status === 'OK' && results.length > 0) {
                 var location = results[0].geometry.location;
                 console.log(results[0].formatted_address,results[0].partial_match);
                 $scope.coords.latitude = location.lat();
                 $scope.coords.longitude = location.lng();
                 // $scope.map.panTo(location);
                 }
                 })
                 */
                $scope.gmapOptions = {
                    fullscreenControl: true,
                    scrollwheel: false
                };

            }

        };
    });







