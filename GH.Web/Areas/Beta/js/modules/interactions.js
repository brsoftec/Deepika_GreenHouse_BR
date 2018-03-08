angular.module('interactions', ['angular-momentjs'])
    .constant('interactionTypeNames', ['Advertisement', 'Registration', 'Event', 'SRFI'])
    .constant('interactionTypeAbbrs', ['Ad', 'Reg', 'Evt', 'SRFI'])
    .constant('interactionStatusNames', ['Pending', 'Active', 'Inactive', 'Expired'])
    .constant('interactions', [])
    .config(function (interactions, interactionTypeNames, interactionStatusNames) {
        function generateSampleCampaigns() {
            var businessId = '01';
            var campaign = {
                "_id": '01',
                "Role": '',
                "CampaignId": '01',
                "BusinessId": "01",
                "CampaignName": "Interaction #01",
                "DataType": "InUsed",
                "CampaignType": "Advertisement",
                "Description": 'Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, ' +
                'totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. ' +
                'Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, ' +
                'sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt.',
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
                "ModifiedDate": new Date(),
                "participators": 0

            };
            interactions.push(campaign);
            for (var i = 2; i < 10; i++) {
                campaign = angular.extend({}, campaign);
                campaign.CampaignId = i.toString();
                campaign.BusinessId = i.toString();
                campaign.CampaignName = 'Interaction #' + i;
                campaign.Description = 'Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt.';
                campaign.CampaignType = interactionTypeNames[Math.floor(Math.random() * interactionTypeNames.length) % interactionTypeNames.length];
                campaign.CampaignStatus = interactionStatusNames[Math.floor(Math.random() * interactionStatusNames.length) % interactionStatusNames.length].toLowerCase();
                if (campaign.CampaignType === 'Event') {
                    campaign.EventStartDate = new Date();
                    campaign.EventEndDate = new Date();
                    campaign.EventLocation = 'Ho Chi Minh City';
                    campaign.EventTheme = 'Festive'
                }
                campaign.participators = Math.floor(Math.random() * 5000);
                interactions.push(campaign);
            }
            // Generate sample drafts
            for (i = 1; i < 4; i++) {
                campaign = angular.extend({}, campaign);
                campaign.Role = 'Draft';
                campaign.CampaignId = 'd' + i;
                campaign.CampaignName = 'Interaction Draft #' + i;
                campaign.CampaignType = interactionTypeNames[Math.floor(Math.random() * interactionTypeNames.length) % interactionTypeNames.length];
                campaign.CampaignStatus = null;
                interactions.push(campaign);
            }
            // Generate sample templates
            for (i = 1; i < 6; i++) {
                campaign = angular.extend({}, campaign);
                campaign.Role = 'Template';
                campaign.CampaignId = 't' + i;
                campaign.CampaignName = 'Interaction Template #' + i;
                campaign.Description = 'Description of Template #' + i;
                campaign.CampaignType = interactionTypeNames[Math.floor(Math.random() * interactionTypeNames.length) % interactionTypeNames.length];
                campaign.CampaignStatus = null;
                interactions.push(campaign);
            }
        }

        generateSampleCampaigns();
        //  Insert code to pull campaigns from API here
    })
    .service('interactionService', function (interactions) {
        return {
            getList: function () {
                return interactions;
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
                angular.forEach(interactions, function (campaign) {
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
                    interactions.push(campaign);
                }
                //  Push campaign list to database...
                var success = true;
                if (angular.isFunction(callback)) {
                    callback.call(null, {success: success});
                }
            }
        };
    })
    .directive('campaignIndicator', function (interactionTypeNames, interactionTypeAbbrs) {
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
                var typeIndex = $.inArray(type, interactionTypeNames);
                if (typeIndex >= 0) {
                    $scope.abbr = interactionTypeAbbrs[typeIndex];
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



