angular.module('defs', [])
    .config(function () {

    })
    .factory('defLists', function ($http) {
        var languages = [];
        var movies = [];
        var songs = [];
        var tvShows = [];

        $http.get('js/languages.json').then(function (response) {
            languages = response.data;
        });
        $http.get('js/movies.json').then(function (response) {
            movies = response.data;
        });
        $http.get('js/songs.json').then(function (response) {
            songs = response.data.songs;
        });
        $http.get('js/tvshows.json').then(function (response) {
            tvShows = response.data.shows;
        });
        var colors = ["AliceBlue", "AntiqueWhite", "Aqua", "Aquamarine", "Azure", "Beige", "Bisque", "Black", "BlanchedAlmond", "Blue", "BlueViolet", "Brown", "BurlyWood", "CadetBlue", "Chartreuse", "Chocolate", "Coral", "CornflowerBlue", "Cornsilk", "Crimson", "Cyan", "DarkBlue", "DarkCyan", "DarkGoldenRod", "DarkGray", "DarkGrey", "DarkGreen", "DarkKhaki", "DarkMagenta", "DarkOliveGreen", "Darkorange", "DarkOrchid", "DarkRed", "DarkSalmon", "DarkSeaGreen", "DarkSlateBlue", "DarkSlateGray", "DarkSlateGrey", "DarkTurquoise", "DarkViolet", "DeepPink", "DeepSkyBlue", "DimGray", "DimGrey", "DodgerBlue", "FireBrick", "FloralWhite", "ForestGreen", "Fuchsia", "Gainsboro", "GhostWhite", "Gold", "GoldenRod", "Gray", "Grey", "Green", "GreenYellow", "HoneyDew", "HotPink", "IndianRed", "Indigo", "Ivory", "Khaki", "Lavender", "LavenderBlush", "LawnGreen", "LemonChiffon", "LightBlue", "LightCoral", "LightCyan", "LightGoldenRodYellow", "LightGray", "LightGrey", "LightGreen", "LightPink", "LightSalmon", "LightSeaGreen", "LightSkyBlue", "LightSlateGray", "LightSlateGrey", "LightSteelBlue", "LightYellow", "Lime", "LimeGreen", "Linen", "Magenta", "Maroon", "MediumAquaMarine", "MediumBlue", "MediumOrchid", "MediumPurple", "MediumSeaGreen", "MediumSlateBlue", "MediumSpringGreen", "MediumTurquoise", "MediumVioletRed", "MidnightBlue", "MintCream", "MistyRose", "Moccasin", "NavajoWhite", "Navy", "OldLace", "Olive", "OliveDrab", "Orange", "OrangeRed", "Orchid", "PaleGoldenRod", "PaleGreen", "PaleTurquoise", "PaleVioletRed", "PapayaWhip", "PeachPuff", "Peru", "Pink", "Plum", "PowderBlue", "Purple", "Red", "RosyBrown", "RoyalBlue", "SaddleBrown", "Salmon", "SandyBrown", "SeaGreen", "SeaShell", "Sienna", "Silver", "SkyBlue", "SlateBlue", "SlateGray", "SlateGrey", "Snow", "SpringGreen", "SteelBlue", "Tan", "Teal", "Thistle", "Tomato", "Turquoise", "Violet", "Wheat", "White", "WhiteSmoke", "Yellow", "YellowGreen"];
        var foods = [
            {
                label: 'Hamburger',
                group: 'Fast food'
            },
            {
                label: 'Hot dog',
                group: 'Fast food'
            },
            {
                label: 'Fried chicken',
                group: 'Fast food'
            },
            {
                label: 'French steak',
                group: 'European Cuisine'
            },
            {
                label: 'Caviar',
                group: 'European Cuisine'
            },
            {
                label: 'Pizza',
                group: 'Italian food'
            },
            {
                label: 'Spaghetti',
                group: 'Italian food'
            },
            {
                label: 'Macaroni',
                group: 'Italian food'
            },
            {
                label: 'Pasta',
                group: 'Italian food'
            },
            {
                label: 'Tofu',
                group: 'Chinese food'
            },
            {
                label: 'Wanton',
                group: 'Chinese food'
            },
            {
                label: 'Roast duck',
                group: 'Chinese food'
            },
            {
                label: 'Sushi',
                group: 'Japanese food'
            },
            {
                label: 'Ramen',
                group: 'Japanese food'
            },
            {
                label: 'Kimchi',
                group: 'Korean food'
            },
            {
                label: 'Cocoa pot',
                group: 'Thai food'
            },
            {
                label: 'Curry',
                group: 'Indian food'
            },
            {
                label: 'Pho',
                group: 'Vietnamese food'
            },
            {
                label: 'Bun cha',
                group: 'Vietnamese food'
            }
        ];
        var holidays = [
            {
                label: 'New Year',
                group: 'Global'
            },
            {
                label: 'Christmas',
                group: 'Global'
            },
            {
                label: 'Women\'s Day',
                group: 'Global'
            },
            {
                label: 'Independence Day',
                group: 'National'
            },
            {
                label: 'Halloween',
                group: 'Western'
            },
            {
                label: 'Lunar New Year',
                group: 'Asian'
            },
            {
                label: 'Mid Autumn',
                group: 'Asian'
            }
        ];
        var musicGenres = [
            'Pop', 'Rock', 'Blue', 'R&B', 'Country', 'Dance', 'Traditional', 'Classical'
        ];
        var religions = [
            "Christianity", "Buddhism", "Islam", "Hinduism"];
        var docCats = [
            "Passport", "Passport Size Photo", "Birth Certificate", "Driver License", "NRIC ID",
            "Employment Pass", "Membership Card", "Resume", "Credit Card", "Debit Card"
        ];
        return {
            getList: function (name) {
                switch (name) {
                    case 'colors':
                        return colors;
                    case 'food':
                        return foods;
                    case 'holidays':
                        return holidays;
                    case 'languages':
                        return languages;
                    case 'movies':
                        return movies;
                    case 'musicGenres':
                        return musicGenres;
                    case 'songs':
                        return songs;
                    case 'tvShows':
                        return tvShows;
                    case 'religions':
                        return religions;
                    case 'docCats':
                        return docCats;
                }
                return [];
            }
        };
    });