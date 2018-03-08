angular.module('campaigns', ['angular-momentjs'])
    .constant('campaignTypeNames', ['Advertisement', 'Registration', 'Event', 'SRFI'])
    .constant('campaignTypeAbbrs', ['Ad', 'Reg', 'Evt', 'SRFI'])
    .constant('campaignStatusNames', ['Pending', 'Active', 'Inactive', 'Expired'])
    .constant('campaigns', [])
    .config(function (campaigns, campaignTypeNames, campaignStatusNames) {
        function generateSampleCampaigns() {
            var businessId = '01';
            var campaign = {
                "_id": '01',
                "CampaignId": '01',
                "BusinessId": "01",
                "CampaignName": "Campaign #01",
                "DataType": "InUsed",
                "CampaignType": "Advertisement",
                "Description": "",
                "Gender": 'All',
                "FromAge": 10,
                "ToAge": 100,
                "StartDate": new Date(),
                "EndDate": null,
                "LocationType": 0,
                "ContinentId": 0,
                "ContinentName": "",
                "CountryId": null,
                "CountryName": "",
                "RegionId": 0,
                "RegionName": "",
                "CityId": null,
                "CityName": "",
                "Latitude": null,
                "Longitude": null,
                "Budget": 0,
                "UnitBudget": "",
                "FlashCost": "0",
                "IsFlash": false,
                "Members": 0,
                "TargetMembers": 0,
                "DisplayOnBuzFeed": true,
                "AllowCreateQrCode": false,
                "ImagePath": null,
                "UrlLink": null,
                "Tags": null,
                "CampaignStatus": "active",
                "CreatedBy": businessId,
                "ModifiedBy": null,
                "CreatedDate": new Date(),
                "ModifiedDate": new Date()

            };
            campaigns.push(campaign);
            for (var i = 2; i < 10; i++) {
                campaign = angular.extend({}, campaign);
                campaign.CampaignId = i.toString();
                campaign.CampaignName = 'Campaign #' + i;
                campaign.CampaignType = campaignTypeNames[Math.floor(Math.random() * campaignTypeNames.length) % campaignTypeNames.length];
                campaign.CampaignStatus = campaignStatusNames[Math.floor(Math.random() * campaignStatusNames.length) % campaignStatusNames.length].toLowerCase();
                if (campaign.CampaignType === 'Event') {
                    campaign.EventStartDate = new Date();
                    campaign.EventEndDate = new Date();
                    campaign.EventLocation = 'Ho Chi Minh City';
                    campaign.EventTheme = 'Festive'
                }
                campaigns.push(campaign);
            }
            // Generate sample drafts
            for (ii = 1; i < 4; i++) {
                campaign = angular.extend({}, campaign);
                campaign.Role = 'Draft';
                campaign.CampaignId = 'd' + i;
                campaign.CampaignName = 'Campaign Draft #' + i;
                campaign.CampaignType = campaignTypeNames[Math.floor(Math.random() * campaignTypeNames.length) % campaignTypeNames.length];
                campaign.CampaignStatus = null;
                campaigns.push(campaign);
            }
            // Generate sample templates
            for (i = 1; i < 6; i++) {
                campaign = angular.extend({}, campaign);
                campaign.Role = 'Template';
                campaign.CampaignId = 't' + i;
                campaign.CampaignName = 'Campaign Template #' + i;
                campaign.Description = 'Description of Template #' + i;
                campaign.CampaignType = campaignTypeNames[Math.floor(Math.random() * campaignTypeNames.length) % campaignTypeNames.length];
                campaign.CampaignStatus = null;
                campaigns.push(campaign);
            }
        }

        generateSampleCampaigns();
        //  Insert code to pull campaigns from API here
    })
    .service('campaignService', function (campaigns) {
        return {
            getList: function () {
                return campaigns;
            },
            getBlankCampaign: function (type) {
                return {
                    "_id": null,
                    "CampaignId": Date.now().toString(),
                    "BusinessId": "01",
                    "CampaignName": 'New ' + type,
                    "CampaignType": type,
                    "Description": "",
                    "Gender": 'All',
                    "FromAge": null,
                    "ToAge": null,
                    "StartDate": null,
                    "EndDate": null,
                    "LocationType": 0,
                    "Budget": 0,
                    "Duration": 'Daily',
                    "DisplayOnBuzFeed": true,
                    "AllowCreateQrCode": false,
                    "CampaignStatus": null,
                    "CreatedDate": new Date()

                };
            },
            getCampaignById: function (id) {
                var found = null;
                angular.forEach(campaigns, function (campaign) {
                    if (!found && id === campaign.CampaignId) {
                        found = campaign;
                    }
                });
                return found;
            },
            saveCampaign: function (campaign, callback) {
                campaign.ModifiedDate = new Date();
                var found = this.getCampaignById(campaign.CampaignId);
                if (found) {
                    found = angular.extend(found, campaign);
                } else {
                    campaign.CreatedDate = new Date();
                    campaigns.push(campaign);
                }
                //  Push campaign list to database...
                var success = true;
                if (angular.isFunction(callback)) {
                    callback.call(null, {success: success});
                }
            }
        };
    })
    .directive('campaignIndicator', function (campaignTypeNames, campaignTypeAbbrs) {
        function link(scope, element, attrs) {
        }

        return {
            restrict: 'E',
            link: link,
            scope: {
                type: '@',
                status: '@'
            },
            controller: function ($scope) {
                var type = $scope.type, status = $scope.status;
                var typeIndex = $.inArray(type, campaignTypeNames);
                if (typeIndex>=0) {
                    $scope.abbr = campaignTypeAbbrs[typeIndex];
                }
            },
            template: function (el, attr) {
                var statusIndexes = {
                    'active': 0,
                    'pending': 1,
                    'inactive': 2,
                    'expired': 3
                };
                var statusIndex = statusIndexes[status];
                return '<span class="campaign-indicator campaign-type-{{type.toLowerCase()}} '
                    + 'campaign-status-{{status}}">'
                    + '<span class="campaign-indicator-inner">{{abbr}}</span></span>';
            }
        };
    })
    .filter('vaultEntryPath', function () {
        return function (path) {
            if (!path) return '';
            var pathParts = path.split('.');
            pathParts.splice(0, 1);
            pathParts.push('');
            return pathParts.join(' &raquo; ');
        };
    })
    .filter('vaultEntryHighlightLabel', function () {
        return function (label, query) {
            if (!query.length) return label;
            var re = new RegExp(query, 'i');
            return label.replace(re, '<b>$&</b>');
        };
    })
    .filter('formFieldPath', function () {
        return function (path) {
            if (!path) return '';
            var pathParts = path.split('.');
            pathParts.splice(0, 1);
            pathParts.push('');
            return pathParts.join(' &raquo; ');
        };
    })
    .filter('controlTypeName', function () {
        return function (type) {
            var names = {
                textbox: 'Text field',
                textarea: 'Multiline text field',
                date: 'Date picker',
                datecombo: 'Date picker combo',
                datedmy: 'Day/Month/Year picker',
                numinput: 'Number with unit',
                radio: 'Radio button',
                checkbox: 'Checkbox',
                select: 'Dropdown select',
                tagsinput: 'Free tag list',
                smartinput: 'Smart tag list',
                range: 'Range select',
                location: 'Location input',
                address: 'Address input',
                history: 'Recent history',
                'static': 'Static text',
                qa: 'Custom question',
                doc: 'Document file'
            };
            if (names.hasOwnProperty(type))
                return names[type];
            return 'Text field';
        };
    })
    .filter('hiddenField', function () {
        return function (value) {
            var wildCard = '*';
            var text = '';
            for (var i = 0; i < 6; i++) {
                text += wildCard;
            }
            return text;
        };
    })

    // .value('workingInteraction', {})
    .constant('workingInteraction', {})
    .value('workingForm', {});



