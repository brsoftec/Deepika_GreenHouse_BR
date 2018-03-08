angular.module('analytics', ['angular-momentjs'])
    .factory('analyticsService', function ($http, $q, $moment) {
        var apiUrl = '/Api/BusinessUserSystemService/GetChartDataByCampaign';
        var apiUrlBus = '/Api/BusinessUserSystemService/GetChartDataByBus';
        return {
            getSampleUsers: function (start) {
                var users = [];
                var numUsers = 1000;
                var sampleCountries = [{
                    name: 'Vietnam',
                    cities: ['Ho Chi Minh', 'Hanoi', 'Da Nang', 'Hue', 'Hai Phong', 'Can Tho']
                },
                    {
                        name: 'Singapore',
                        cities: ['Singapore', 'Alexandra', 'Ama Keng', 'Banla Tengeh', 'Boon Lay', 'Bedoc']
                    },
                    {
                        name: 'Canada',
                        cities: ['Ottawa', 'Edmonton', 'Victoria', 'Toronto', 'Quebec City']
                    }
                ];
                //  Generate random interaction keywords
                var count = chance.natural({
                    min: 1,
                    max: 5
                });
                var keywords = [];
                for (var i = 0; i < count; i++) {
                    keywords.push(chance.word());
                }

                users.splice(0);
                var from = $moment(start);
                var numDays = $moment().diff(from, 'days');
                for (i = 0; i < numUsers; i++) {
                    var days = Math.floor(Math.random() * numDays);
                    var when = $moment(from);
                    if (i == 0) {
                        days = 0;
                    } else if (i == numUsers - 1) {
                        days = numDays;
                    }
                    when.add(days, 'days');
                    var country = sampleCountries[Math.floor(Math.random() * sampleCountries.length)];
                    var gender = chance.gender();
                    var age = chance.age();
                    var dob = chance.birthday({ year: 2016 - age });
                    var user = {
                        id: chance.hash({ length: 10 }),
                        FirstName: chance.first({ gender: gender }),
                        LastName: chance.last({ gender: gender }),
                        Gender: gender,
                        DOB: dob,
                        Email: chance.email(),
                        Country: country.name,
                        City: country.cities[Math.floor(Math.random() * country.cities.length)],
                        JoinedDate: when.format(),
                        keywords: chance.pickset(keywords, chance.natural({
                            min: 0,
                            max: keywords.length
                        }))
                    };
                    users.push(user);
                }
                return users;
            },
            getUsers: function (id, start, byBusiness) {
                var users = [];
                var deferred = $q.defer();
                // $http.get('js/analytics-data.json')
                $http({
                    method: 'POST',
                    url: byBusiness ? apiUrlBus : apiUrl,
                    data: byBusiness ? {
                        BusId: id,
                        Startdate: $moment(start).format('YYYY-MM-DD')
                    } : {
                        CamapignId: id,
                        Startdate: $moment(start).format('YYYY-MM-DD')
                    }
                })
                    .then(function (response) {
                        users = response.data.Data;
                        deferred.resolve(users);
                    }, function (response) {
                        deferred.reject(response);
                    });
                return deferred.promise;
            },

            getTagCloud: function (interactionId) {
                //  Generate random user tags
                var count = chance.natural({
                    min: 40,
                    max: 70
                });
                var tags = [];
                for (var i = 0; i < count; i++) {
                    tags.push({
                        text: chance.word(),
                        weight: chance.integer({
                            min: 1,
                            max: 100
                        })
                    });
                }
                return tags;
            }
        }
    })
    .filter('shortenId', function () {
        return function (id) {
            return id.substring(0, 8) + '...';
        };
    })
     .filter('gridValue', function ($moment, rgu) {
         return function (value) {
             //var returnvalue="";
           
             if (angular.isUndefined(value))
                 return ''; //<span class="vault-value-meta">(No value)</span>';
             if (angular.isArray(value))
                 return value.join(', ');
             
             var parsedDate = rgu.parseDate(value);
             if (parsedDate && (value.search("/")>=0 || value.search("-")>=0)) {
                 var datestr = $moment(parsedDate.date).format('YYYY/MM/DD');
                 if (datestr === 'Invalid date') {
                     datestr = '';
                 }
                 return datestr;
             }
             if (angular.isObject(value)) {
                 if (value.hasOwnProperty('country') && value.hasOwnProperty('city')) {
                     var country = value.country || '', city = value.city || '';
                     var text = city;
                     if (country && city) {
                         text += ', ';
                     }
                     text += country;
                     return text;
                 }
                 if (value.hasOwnProperty('address') && value.address.hasOwnProperty('address')) {
                     return value.address.address;
                 }
                 // return '<span class="vault-value-meta">(Data)</span>'
                 return '';

             }
             return value;
         };
    });



