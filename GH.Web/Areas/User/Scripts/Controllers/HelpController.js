//var myApp = getApp("myApp", true);
(function () {
    var app = angular.module(regitGlobal.ngApp);

    app.filter('trustHtml', function ($sce) {
        return function (text) {
            if (!text) return '';
            return text;
            // return $sce.trustAsHtml(text);
        };
    })
        .filter('searchHighlight', function ($sce) {
            return function (text, query) {
                if (!text) return '';
                return text.replace(new RegExp(query, 'gi'), '<span class="search-highlight">$&</span>');
                //return $sce.trustAsHtml(text.replace(new RegExp(query, 'gi'), '<span class="search-highlight">$&</span>'));
            };
        })

        .filter('helpSearchHighlight', function ($sce) {
            return function (html, query) {
                if (!html) return '';
                return html.replace(new RegExp(query, 'gi'), '<span class="search-highlight">$&</span>');
                // return $sce.trustAsHtml(html.replace(new RegExp(query, 'gi'), '<span class="search-highlight">$&</span>'));

            };
        })
        .controller('HelpController', function ($scope) {
            $scope.navs = [
                {
                    name: 'home',
                    label: 'Home'
                }, {
                    name: 'using',
                    label: 'Regit for Individuals'
                },
                {
                    name: 'business',
                    label: 'Regit for Business'
                },
                {
                    name: 'security',
                    label: 'Security & Safety'
                }
            ];
            $scope.navHome = $scope.navs[0];
            $scope.view = {
                mode: 'home',
                topic: null,
                section: 'home',
                nav: $scope.navHome
            };

            $scope.sections = [
                {
                    name: 'using',
                    label: 'Regit for Individuals',
                    tier: '',
                    description: 'Learn how to create an account. Organize, use and share your information'
                },
                {
                    name: 'business',
                    label: 'Regit for Business',
                    tier: 'business',
                    description: 'Regit integrated communication platform makes it easy for your customers, and employees to notify you of changes to their information. Stop guessing and say goodbye to bad data.'
                },
                {
                    name: 'security',
                    label: 'Security & Safety',
                    tier: '',
                    description: 'How Regit protects your data securely and safely'
                }
            ];
            $scope.topics = [
                {
                    name: 'using',
                    label: 'Using Regit',
                    tier: '',
                    featured: true,
                    section: 'using',
                    subject: '',
                    description: 'Learn how to create an account. Organize, use and share your information'
                }, {
                    name: 'manage-account',
                    label: 'Account Settings',
                    tier: '',
                    featured: true,
                    section: 'using',
                    subject: 'manage-account',
                    description: 'Get help managing your profile and account'
                }, {
                    name: 'delegation',
                    label: 'Delegation',
                    tier: '',
                    featured: true,
                    section: 'using',
                    subject: 'delegation',
                    description: 'Understand delegation rules and how to authorize others'
                }, {
                    name: 'interaction',
                    label: 'Interaction',
                    tier: '',
                    featured: true,
                    section: 'using',
                    subject: 'interaction',
                    description: 'Learn how to interact with other individuals and businesses'
                }
            ];

            $scope.subjects = [
                {
                    name: 'create-account',
                    label: 'Create an account',
                    section: 'using',
                    description: ''
                }, {
                    name: 'manage-account',
                    label: 'Account Settings',
                    section: 'using',
                    description: ''
                },
                {
                    name: 'information-vault',
                    label: 'Information Vault',
                    section: 'using',
                    description: ''
                }, {
                    name: 'search',
                    label: 'Search for information',
                    section: 'using',
                    description: ''
                }, {
                    name: 'sync-share',
                    label: 'Synchronization & Sharing',
                    section: 'using',
                    description: ''
                }, {
                    name: 'delegation',
                    label: 'Delegation',
                    section: 'using',
                    description: ''
                }, {
                    name: 'network',
                    label: 'Network',
                    section: 'using',
                    description: ''
                }, {
                    name: 'interaction',
                    label: 'Interaction',
                    section: 'using',
                    description: ''
                }, {
                    name: 'handshakes',
                    label: 'Manage Handshakes',
                    section: 'using',
                    description: ''
                }, {
                    name: 'security',
                    label: 'Security',
                    section: 'security',
                    description: ''
                }, {
                    name: 'care-information',
                    label: 'Does Regit sell my information?',
                    section: 'security',
                    description: ''
                }, {
                    name: 'business-profile',
                    label: 'Business Profile',
                    section: 'business',
                    description: ''
                },
                {
                    name: 'interaction-module',
                    label: 'Interactions Module',
                    section: 'business',
                    description: ''
                },
                {
                    name: 'workflow',
                    label: 'Workflow',
                    section: 'business',
                    description: ''
                }
            ];

            function initNav() {
                $scope.subjects.forEach(function (subject) {
                    subject.items = [];
                });
                $scope.helpItems.forEach(function (item) {
                    $scope.subjects.forEach(function (subject) {
                        if (subject.name === item.subject) {
                            subject.items.push(item);
                        }
                    });
                });
            }

            $scope.search = {
                query: '',
                searching: false
            };

            $scope.clearSearch = function () {
                $scope.search.query = '';
            };
            $scope.focusSearchInput = function () {
                $('.help-search-input').focus();
            };

            $scope.navTo = function (nav) {
                $scope.view.nav = nav;
                var isHome = nav.name === 'home';
                $scope.view.mode = isHome ? 'home' : 'section';
                switch (nav.name) {
                    case 'home':
                        $scope.focusSearchInput();
                        break;
                    case 'using':
                        $scope.viewSection('using');
                        break;
                    case 'security':
                        $scope.viewSection('security');
                        break;
                    case 'business':
                        $scope.viewSection('business');
                        break;
                }
            };

            if (!Array.prototype.forEach) {
                Array.prototype.forEach = function forEach(callback, thisArg) {
                    if (typeof callback !== 'function') {
                        throw new TypeError(callback + ' is not a function');
                    }
                    var array = this;
                    thisArg = thisArg || this;
                    for (var i = 0, l = array.length; i !== l; ++i) {
                        callback.call(thisArg, array[i], i, array);
                    }
                };
            }

            $scope.navTo($scope.navHome);

            $scope.viewSection = function (sectionName) {
                var activeSection = null;
                $scope.sections.forEach(function (section) {
                    if (section.name === sectionName) {
                        activeSection = section;
                    }
                });
                if (!activeSection) return;
                $scope.view.section = activeSection;
                activeSection.subject = null;
                $scope.view.mode = 'section';
                // $scope.view.nav = null;
                $scope.clearSearch();
            };

            $scope.viewSubject = function (subject) {
                var section = $scope.view.section;
                section.subject = subject;
                $scope.clearSearch();
            };
            $scope.viewSubjectByName = function (subjectName) {
                $scope.subjects.forEach(function (subject) {
                    if (subject.name === subjectName) {
                        $scope.viewSubject(subject);
                    }
                });
            };
            $scope.viewTopic = function (topic) {
                $scope.viewSection(topic.section);
                var subject = topic.subject;
                if (subject) {
                    $scope.viewSubjectByName(subject);
                } else {

                }

            };


            $scope.navToItem = function (item) {
                $scope.subjects.forEach(function (subject) {
                    if (subject.name === item.subject) {
                        $scope.viewSection(subject.section);
                        $scope.viewSubject(subject);
                    }
                });
            };

            $scope.filterFeaturedTopics = function (topic) {
                return !!topic.featured;
            };

            $scope.filterSubjectsByCurrentSection = function (subject) {
                return subject.section === $scope.view.section.name;
            };
            $scope.filterItemsByCurrentSubject = function (item) {
                if (!$scope.view.section.subject) return false;
                return item.subject === $scope.view.section.subject.name;
            };
            $scope.filterItemsBySearch = function (item) {
                var query = $scope.search.query;
                if (!query.length) return false;
                var rg = new RegExp(query, 'i');
                var found = rg.test(item.title);
                if (found) return true;
                return rg.test(item.description);
            };

            $scope.onHelpSearchInput = function () {
                $scope.search.searching = $scope.search.query.length;

                if (!$scope.search.searching) {
                    $scope.view.mode = 'home';
                    return;
                }
                $scope.view.mode = 'search';

            };


            $scope.helpItems = [

                {
                    name: 'overview',
                    subject: 'manage-account',
                    title: 'Account Settings',
                    description: '<p>The account settings is for you to manage your profile, access, notification, and activity log, accounts status. See below to learn more of each.</p>' +
                    '<ul><li><b>Profile</b> – Here you can edit your profile information. Your profile is designed to share information about you to the Regit community. Feel free to share what you like and if you don’t want people to find you, you can always adjust your privacy setting. Note that your profile is distinct from your information vault.</li>' +
                    '<li><b>Manage Access</b> – Here you can reset your password, your Vault PIN, your login ID, Security Questions, and your phone number.</li>' +
                    '<li><b>Notifications</b> – Here you can customize your notification so that you are only alerted of certain notifications important to you.</li>' +
                    '<li><b>Activity Log</b> – Here you can customize what types of activities in your accounts get saved in your activity log.</li>' +
                    '<li><b>Manage Account</b> – In our commitment to give you total control of your information, this includes closing your account. Note that once closed, your account won’t be retrievable, including all the information and activities on the account when it was active.</li>'
                },
                {
                    name: 'forgot-password',
                    subject: 'manage-account',
                    title: 'I forgot my password, what can I do? ',
                    description: '<p>Your login password should: <ul><li>Have been 8 or more characters long</li><li>Include at least one number</li><li>Include at least one upper case letter</li><li>Include at least one lower case letter</li></ul><p>Tip: Check and ensure your CAP LOCK key is not on.</p>' +
                    '<p>You can reset your password by clicking on the “Forgot Password” link. Answer your security question, then select your preferred method to receive the one-time PIN to reset your password. If by email, check your email for the 4-digit PIN. If by phone, a 4-digit PIN will be sent via SMS.</p><p>Tip: Do not use this password for any other account or save it anywhere that is not encrypted.</p>'
                },
                {
                    id: 'id03',
                    subject: 'manage-account',
                    title: 'Reset Account',
                    description: 'If you don\'t remember your registered email address, we will not be able to reset your account for security reasons.'

                }, {
                    subject: 'manage-account',
                    title: 'What happens if someone steals my password and Vault PIN?',
                    description: '<p>If you believe your password has been jeopardized, you should immediately change your account password and PIN.</p>' +
                    '<p>Similarly, if you feel your Vault PIN has been compromised, please login and change your PIN. Note that you cannot change your PIN unless you log in the account.</p>'
                }, {
                    subject: 'manage-account',
                    title: 'When I die, how will my wife or kids get to all my accounts, especially the financial ones?',
                    description: '<p>Unless you delegated the information to your wife and kids, no one would have access to your account and information.</p>' +
                    '<p>Email support@regit.today to inform us of any accounts that should be closed but the user is unable to do so. We will need to ask for supporting information before proceeding for requests of these nature.</p>'
                },
                {
                    id: 'id04',
                    subject: 'manage-account',
                    title: 'Regit does not work.... What should I do?',
                    description: 'Our team is committed to ensuring you have access to your information when you need it. If you encounter issues and unable to find the solution on this page, please contact us.<br><br> In this article, you will find tips and content designed to walk you through the main issues currently being raised by our users. If you have an issue or question that is not listed here, do not worry: it may be covered in another article. Also, you can always reach out to our Support team anytime.',

                },
                {
                    name: 'overview',
                    subject: 'information-vault',
                    title: 'Information Vault',
                    description: '<p>Information Vault is where all your personal information is saved. Only relevant information from the Vault is pulled for each interaction and is only shared once you accept/consent.</p>'
                },
                {
                    name: 'why-information',
                    subject: 'information-vault',
                    title: 'Why should I put my personal information in Regit?',
                    description: '<p>Regit is a secure and convenient way to store and use of your information within your trust network. Security alerts are also sent straight to your device when any of your accounts may be compromised so you can update your old password and stop hackers in their tracks</p><p>If you are worried about putting you’re your information in Regit, learn more about our security by <a href="/about#security">clicking here</a>.</p>'
                },
                // {
                //     id: 'id06',
                //     subject: 'information-vault',
                //     title: 'Why should I put my IDs into Regit?',
                //     description: '<p>Regit can make your Personal Data work for you and fill any field from any form on Regit. This data is securely encrypted just like your login information. <br>The more data you enter in your Regit account, the more effective and smart Regit will be and the more time you will save!</p>' +
                //     '<p>In the IDs section, you can enter your:<ul ><li>ID Card</li><li>Passport</li><li>Driver\'s License</li> <li>Social Security Card</li> </ul > <p><strong>No more mistakes</strong> filling out your passport number when booking a flight! </p><p>Regit will do all the leg work.</p>'
                // },
                {
                    name: 'not-sell-information',
                    subject: 'care-information',
                    title: 'Does Regit sell my information?',
                    description: '<p>Regit does not sell your personal information. Only you can share your information with the people and businesses you trust.</p>' +
                    'We give you total control of your information. All information shared with other businesses and users must be authorized by you.</p>'
                },
                {
                    name: 'store-information',
                    subject: 'care-information',
                    title: 'Where do you store all my information?',
                    description: '<p>Your information is hosted on Microsoft Azure, a highly secured and available cloud service from Microsoft.</p>'
                }, {
                    subject: 'information-vault',
                    title: 'Why do I need to enter my PIN to access my vault?',
                    description: '<p>The PIN is unique to you and meant to be a second layer of authentication so even if someone knows your password, they are unable to view your personal information.</p>'
                }, {
                    subject: 'information-vault',
                    title: 'What is the difference between the vault and my profile information?',
                    description: '<p>Profile information, if made public can be viewed by anyone on the Regit network. The information in the Vault is 100% private to you and the people you have shared with. No public users or businesses can see this information.</p>'
                },
                {
                    subject: 'delegation',
                    title: 'What is delegation?',
                    description: '<p>Delegation is away for you to share your personal information to the people you trust without typing another word, without sharing it over emails or other insecure method, and giving you controls with how long they see that information.</p>'
                },
                {
                    name: 'interactions',
                    subject: 'interaction',
                    title: 'Interactions',
                    description: '<p>Interactions are what we used to describe different ways you engage with other people and businesses. Each interaction has a unique function but all interactions has the same purpose, which is to simplify how we share, update, and exchange personal information with people and businesses that we trust. All interactions are only done with your consent.</p>' +
                    '<p>Some interactions are one way, some are two ways, some are continuous, whereas some are one-time. Some appears on news feed, profile page, notification, and some on chats. Some are designed only for business whereas some are just for individuals. Don’t be confused! It’s all very intuitive.</p>'
                },
                {
                    name: 'handshake',
                    subject: 'interaction',
                    title: 'Request for handshake',
                    description: '<p>Interactions are what we used to describe different ways you engage with other people and businesses. Each interaction has a unique function but all interactions has the same purpose, which is to simplify how we share, update, and exchange personal information with people and businesses that we trust. All interactions are only done with your consent.</p>' +
                    '<p>Some interactions are one way, some are two ways, some are continuous, whereas some are one-time. Some appears on news feed, profile page, notification, and some on chats. Some are designed only for business whereas some are just for individuals. Don’t be confused! It’s all very intuitive.</p>'
                },
                {
                    name: 'request-information',
                    subject: 'interaction',
                    title: 'Request for information',
                    description: '<p>This replaces the repetitive form. Businesses can now send you a direct request for information, which will automatically pull the information from the Vault. No typing or having to look for your information. Just consent and click send. It’s that easy.</p>'
                },
                {
                    name: 'push-form',
                    subject: 'interaction',
                    title: 'Push Form',
                    description: '<p>Want to do a good deed for the day? Help your friends and family get organize by pushing information directly to their Vault. Both you and the recipient can edit before sending/accepting. With the information in the Vault, they can use it for future interactions.</p>' +
                    '<p>There are two types of push forms and is used slightly differently. The push forms in the Vault is a convenient way for you to push your information in the Vault to someone else on your network. This is convenient for families and friends who share information like address, memberships, and accounts. When pushing information, you are given a chance to review and edit the information before sending. Similarly the recipient also has the chance to edit and or deny the information.</p>' +
                    '<p>Push Form as an interaction. You can create a more scalable push form interaction if you find yourself pushing unique information constantly. For example, you may want to push someone their membership information which is unique to them. Rather than pushing from the vault and editing that information before sending, you can use the push form interaction. You can also use this to push to multiple people at once.</p>'
                },
                {
                    name: 'events-calendar',
                    subject: 'interaction',
                    title: 'Events & Calendar',
                    description: '<p>Events interactions are for you to manage personal events that can be linked to your calendar. You can use events interactions to also collection required information for your event, send reminders, and communication prior to the events. No more spreadsheets and fragmented mode of communications.</p>' +
                    '<p>The Regit calendar is designed to be linked with interactions helping you stay organize and on schedule.</p>'
                },
                {
                    subject: 'interaction',
                    title: 'Can I push to another user calendar?',
                    description: '<p>Not yet but in the future you will be able to push other types of information including events and reminders.</p>'
                },
                {
                    subject: 'interaction',
                    title: 'Where do I find my history?',
                    description: '<p>You can find activity history in the activity log.</p>'
                },
                {
                    name: 'manage-handshakes',
                    subject: 'handshakes',
                    title: 'Manage handshakes',
                    description: '<p>On this page, you can view and manage all your sync relationships. You can view what type of information is being shared, and the status of your information with the recipient. Green means that both yours and the business record of you is synced (updated), red means the business has not acknowledge you change, and grey means its paused or active.</p>' +
                    '<p>At any point in time you can pause or terminate a sync relationship. You are in control.</p>'
                },
                {
                    id: 'id07',
                    subject: 'sync-share',
                    title: 'Synchronization and sharing issues on Regit',
                    description: '<p>All your information across all your devices are actively sync if connected to the internet.</p><p>If you are actively connected to the internet, and continue to experience issue, try to clear your cache as old content may be stored on your browser preventing new information to show.</p><p>Depending your browser, go to clear history, select clear cache, close your browser, and restart again.</p>'

                },
                {
                    id: 'id08',

                    subject: 'sync-share',
                    title: 'Some functions are not working',
                    description: '<p>We try our best to bring you the best quality application, for every possible scenario, but if we have missed something, we apologize and please let us know.</p><p>If you are still unable to get Regit to work properly with certain functions, you can use our browser extension to report this site.</p>'

                },
                {
                    id: 'id09',
                    url: '#id09',
                    subject: 'sync-share',
                    title: 'Regit logs out all the time, can I change that?',
                    description: '<p>Yes. You need to disable the Automatic logout option in the <strong>Account Settings > Activity Log > Keep a record of your logging in/out Regit > Yes</strong></p><p>Please note that if you decide to disable automatic logout, you should be careful to log out each time you leave your computer.</p>'

                },
                {
                    id: 'id10',
                    url: '#id10',
                    subject: 'sync-share',
                    title: 'Can several people share the same Regit account?',
                    description: '<p>No. Your information is specific to you. You are in control of how your share your information.</p><p>That said, we understand sometimes you may want to share your information, so we built numerous features for you to share your information without compromising your information, and giving you control.</p><p>Check out the delegation feature which allows you to select what you want to share, and how long you want to share that information for.</p><p>There are other features like emergency and family contact which allow you to continuously share basic information like contact information until the relationship is cancelled.</p>'

                },
                {
                    id: 'id11',
                    url: '#id11',
                    subject: 'sync-share',
                    title: 'Can I access my data on Regit offline?',
                    description: 'We are currently working on that feature. Once ready, we will broadcast to the Regit community.'

                },
                {
                    id: 'id12',
                    url: '#id12',
                    subject: 'sync-share',
                    title: 'Suspending your Regit account',
                    description: '<p>We understand that circumstances changes and to protect your information, we made it possible for you to suspend your accounts.</p><p>Your information is still there but no one will be able to find you or engage with you.All notifications will also be turned off.</p><p>To suspend your account, you will need to be re- authenticated to ensure it is only you who is suspending your account.</p>'
                },
                {
                    id: 'id13',
                    url: '#id13',
                    topic: 'using',
                    subject: 'sync-share',
                    title: 'Delete your account and all your data permanently',
                    description: '<p>Regit is all about giving users total control of their information, this includes if they wish the right to terminate and delete their account.</p><p>Upon the request, we will delete all information on your account within 60 days.<br>The 60 day is designed to be a backup measure just in case you change your mind.</p><p>During the 60 days period, your account act as though it’s in suspended mode. No one can engage with you and all notifications are turned off.</p><p>To terminate your account, you will need to be re-authenticated to ensure it is only you who is terminating your account.</p>'

                },
                {
                    id: 'id14',
                    url: '#id14',
                    topic: 'account',
                    subject: 'create-account',
                    title: 'How do I create a Regit account?',
                    description: '<p>If you don\'t have a Regit account, you can create one in a few steps: </p><ol><li>Go to <a href="http://www.regit.today" target="_blank">www.regit.today</a> </li><li>If you see the signup form, fill out your name, email address or mobile phone number, password. If you don\'t see the form, click <b>Sign Up</b> then fill out the form.</li><li>Click <b>Sign Up</b>.</li><li>You\'ll need to confirm a PIN code. Then you have to answer 3 security questions to finish.</li><li>At last, you may need to add your profile picture and enter some basic informations. </li></ol><p>If you already have a Regit account, you can log into your account by entering your email and password and clicking <b>Log In</b>. <br><br> Note: you must be at least 13 years old to create a Regit account.</p>'

                },
                {
                    id: 'id15',
                    url: '#id15',
                    topic: 'account',
                    subject: 'create-account',
                    title: 'Why am I being asked to add my phone number to my account?',
                    description: 'Security purposes.'

                },
                {
                    id: 'id16',
                    url: '#id16',
                    tier: 'business',
                    subject: 'interaction-module',
                    title: 'Overview of the Interactions Module',
                    description: '<p>The interactions module is where you create campaigns to interact with your customers and get new customers. Every interactions is designed to make it easy for you and the customers to exchange information, eliminating forms, errors, and missed opportunities.</p>' +
                    '<p>The number of interactions available to you depends on your subscription.</p>' +
                    '<ol><li><h5>Broadcast</h5>' +
                    '<p>This is as it sounds. A broadcast sent to your existing customers, instead of over emails which are often ignored, your customers can see it directly on the Regit discovery page in the beautiful format it’s meant to be.</p></li>' +
                    '<li><h5>Events</h5>' +
                    '<p>It\'s identical to broadcast interaction except, it also allow you to specify the required information if any, and the costs to the event.If your customers sign up for the event, it will also be automatically added to their calendar so they won’t forget.The information collected are aggregated into the campaign analytics and can easily be viewed anytime. Event interactions once approved are shown in the users discovery feed.</p>' +
                    '<p>A unique URL and QR code is also created so that you can share this over email or other mediums directly.</p></li>' +
                    '<li><h5>Registrations</h5>' +
                    '<p>Registration interactions are used to collect information required for a particular purposeInformation can be as simple as the customer name or ask complex as a credit card or school application. The goal is to simplify the registration process so that users don’t need to constantly type the same information over and over again which increases changes of transaction abandonment and errors. Registrations interactions once approved are shown in the users discovery feed similar to adverts.</p>' +
                    '<p>A unique URL and QR code is also created so that you can share this over email or other mediums directly.</p></li>' +
                    '<li><h5>Static Request for Information</h5>' +
                    '<p>Static Request for Information (STFI) interaction is identical to registration interaction with the exception that it does not get broadcast into customer’s discovery feed. Instead, SRFI are only shown on the public business page as it’s not meant to be a discovery feature but an automated way for customers to submit information. This is commonly used by doctors, lawyers, agents, advisors and other office type environment where users must fill in a form before getting services.</p>' +
                    '<p>A unique URL and QR code is also created so that you can share this over email or other mediums directly.</p></li>' +
                    '<li><h5>Push to Vault – Business</h5>' +
                    '<p>For businesses that constantly share customer personal information with their customers via emails, papers, and text. A better and more convenient way for the customer is using the Push to Vault. This will ensure that the user directly gets it and allows them to use it in the future conveniently.</p>' +
                    '<p>For example, if user signed up for a membership, you can push their membership details to them so they can refer to it easily without having to look for it.</p>' +
                    '<p>Other use cases includes, pushing work information, medical records, and even non traditional information like tailor size.</p>' +
                    '<p>In the future, we will allow you to push other types of information like events, reminders, meeting invites, and tasks, so all the information is digitized and centralized for the customer, delivering unheard of customer experience.</p>' +
                    '<p>Push to Vault are only sent to a defined user and not shown on the discovery feed.</p></li>' +

                    '<li><h5>Handshakes</h5>' +
                    '<p>A handshake for you and your customers to say in touch so that when they update certain information, your business is notified so you can act accordingly. For example, if a customer change their address, you would be notified of the change in address if address was a handshake field. The update will allow you to keep your information up to date and for you to better serve your customers.</p>' +
                    '<p>Handshakes requires the customers to approve before any information is exchanged. The customer and you also have the right to cancel the handshake anytime and for any reason.' +
                    '<br>The handshake is also a good way for you to know who your customers are. If a customer handshake relationship is cancelled, you know that the customers is no longer interested in your products or services.</p>' +
                    '<p>Be careful to only ask for required information to increase participation from your customers.</p></li>' +
                    '<li><h5>Future development</h5>' +
                    '<h6>Reservation</h6>' +
                    '<p>Reservation is a service that allows businesses to automate their reservation process without having to rely on another service. By integrating with the regit platform, customers also don’t need to rely on traditional methods to make a reservation therefore simplifying the reservation process.</p>' +
                    '<p>By using Regit reservation, you will be able to collect demographic information of your customers so you can better manage your business.</p>' +
                    '<h6>Memberships</h6>' +
                    '<p>The membership module allows you to manage your membership within the Regit platform. Combined with features like handshake and other interactions like adverts will allow you to continuously engage with your customers. No need to buy separate software when you can do it all within one platform.</p>' +
                    '<h6>Public Forms</h6>' +
                    '<p>Public forms are used to create unique customers forms that can be completed anonymously and for users not on the Regit network.</p>'
                    + '</li></ol>'

                },
                {
                    id: 'id25',
                    url: '#id25',
                    tier: 'business',
                    subject: 'workflow',
                    title: 'Workflow overview',
                    description: '<p>The goal of the business workflow is for you to provision access to your account to other colleagues. The types of access depends on the role.' +
                    '</p><p>Access is on a hierarchical basis so if you have access to the highest level, you are able to do tasks that the lowest level can do.' +
                    '</p><p>Approver – Ability to create and approve interactions campaign. This role should be researched for people who have final authority to approve campaigns. Once approved, unless cancelled, the campaign becomes live and is available for the public to see.' +
                    '</p><p>Editor – This role allow users to create campaigns but cannot approve.This segregation of roles ensure that there is a maker and checker ensuring that the quality of content is appropriate and professional.' +
                    '</p><p>Admin – The admin role has the ability to provision access, as editor, approver or even other admins.'

                },
                {
                    id: 'id26',
                    url: '#id26',
                    tier: 'business',
                    subject: 'business-profile',
                    title: 'Business profile page',
                    description: 'The business profile page, if made public, is for the public to find you. There it includes information about your business, including business hours, and contact information where relevant. The public page will also include all your interaction campaigns that are public allowing non existing customers to interact and follow you.'

                },
                // {
                //     name: 'why-secure',
                //     subject: 'security',
                //     title: 'How can I be 100 percent sure that Regit is itself 100 percent secure? ',
                //     description: '<p>To be completely honest, we have no clear answer for you either. No matter what, we will continue to stand by our word and not sell your information and do our utmost best to secure your information. We are well aware that with all things there is no way to know 100 percent.</p><p>There is also no way to be 100 percent sure that your phone company isn\'t listening in to your calls, that your credit company isn\'t laughing at your list of purchases, that your G.P.S. device isn\'t tracking your every move, that your house isn\'t bugged, that the government isn\'t slowly poisoning you, or that aliens aren\'t puppeteering you from distant planets.</p><p>But you could take comfort in knowing that if we were accessing your account, we would be out of business instantly.</p>',
                //
                // },
                {
                    name: 'security',
                    subject: 'security',
                    title: 'Security',
                    description: '<p>Regit wants to simplify how we communicate and exchange personal information with the people and business we trust. However we won’t do it by compromising on security. Regit protects your accounts in a number of ways depending on the usage and your account type:</p>' +
                    '<ol><li><h5>Robust password policy</h5><p>Ensuring that your password is at a minimum standard so it can’t easily be guessed</p></li>' +
                    '<li><h5>Mandatory multi-factor authentication</h5><p>We use a combination of pre-defined and dynamic authentication depending on the functions being accessed</p>' +
                    '<ol><li>SMS OTP for account verification and changes to your password, Vault PIN, and security questions</li>' +
                    '<li>Vault PIN – Unique 4 digit PIN to access your information Vault after you login</li>' +
                    '<li>Security Questions  – Additional layer required before you can reset your account</li>' +
                    '<li>Email PIN – Option for users to reset their account using their email instead of SMS after correctly answering the security question</li>' +
                    '<li>OTP – Dynamic SMS PIN for login authentication</li></ol></li>' +
                    '<li><h5>Encryption of data in flight</h5><p>SSL encryption your data is sent over the Internet</p></li>' +
                    '<li><h5>Encryption of data at rest </h5><p>AES 256 database encryption</p></li>' +
                    '<li><h5>Top Cloud Provider</h5><p>We don’t just use any cloud provider. We use only Microsoft Azure for all services including hosting, storage, and cloud computing. Azure is recognized as one of the most reliable and safest data centres in the world.</p></li>' +
                    '<li><h5>Coming soon</h5><p>We are constantly innovating by finding new ways to deliver the Regit mission and keep your information secure.</p>' +
                    '<ul><li>Biometrics Authentication – coming soon with mobile, mandatory biometric authentication as an additional layer of protecting your information.</li>' +
'<li>IP and Devices monitoring – When you login in an unknown device, we will require you to authenticate yourself with SMS OTP.</li></ul></li>'
                    + '</ol>'

                },
                {
                    name: 'safety',
                    subject: 'security',
                    title: 'Safety',
                    description: '<p>Everyone has a role to play to keep Regit safe for yourself and your network. Below are some common tips to keep your information safe.</p>' +
                    '<ol><li><p>Do not share your password with anyone.</p>' +
                    '<ol><li>For individuals, Regit delegation function allows you to share your information without sharing your accounts.' +
                    '<li>For businesses, Regit workflow allows you to provision difference types of access for your team.</li></ol></li>' +
                    '<li>Change your password periodically.</li>' +
                    '<li>Do not use the same password for all your online accounts</li>' +
                    '<li>Do not use easily guessed password. Our complex password requirement already makes it more challenging to guess but don’t make it easy for people to guess.</li>' +
                    '<li>Always log out, especially when using Regit on public/shared devices.</li>' +
                    '<li>If you noticed something suspicious, please report it to us. You can contact us at <a href="mailto:"support@regit.today">support@regit.today</a></li>' +
                '</ol>'

                },
                {
                    id: 'id20',
                    url: '#id20',
                    topic: 'using',
                    subject: 'search',
                    title: 'How to search in Regit',
                    description: '<p>There are multiple search options in Regit. This is itself a security measures ensuring that sensitive information is not mixed with public information. The search function will get easier and smarter over time, saving you time when you need it most. Just start typing and we will immediately pull up suggestions that will help you find what you are looking for, be it a credential, a purchase, and ID, a contact or just about everything saved in Regit. </p><p>Home Search Bar on the Top Menu Bar – This allows you to search other users and businesses on the network.Only public information is available. </p><p>Information Vault Search Bar – This allows you to search all the information in your Vault or your delegator vault (depending on your access).</p><p>Activity Log Search Bar – This allows you to search all the information in your activity Log only.<br>Network Search on the network page – This allows you to find other people on the network.'

                },
                {
                    id: 'id21',
                    url: '#id21',
                    topic: 'using',
                    subject: 'information-vault',
                    title: 'How to save time by adding your IDs to Regit',
                    description: '<p>Regit can make your Personal Data work for you, and fill any field from any business on regit.<br>The more data you enter in your Regit account, the more effective and smart Regit will be, and the more time you will save!</p><p>To add information to your Vault, click on the Vault icon on the top right of the menu bar. Enter your Vault PIN, select add information and choose the category of information.</p><p>Other people on your trust network may also push information to your Vault to save you time from having to enter it yourself. Before any information is push to your vault, you must authorize it to prevent unintentional information being added to your Vault.</p><p>The last way which information could be added to your Vault is via your super delegatee or people who have write access to your Vault. ' +
                    ' These are people you have authorized previously to have read and write access to your Vault.'

                },
                {
                    id: 'id22',
                    url: '#id22',
                    topic: 'using',
                    subject: 'information-vault',
                    title: 'Why are some of my ID information written in a different colour? ',
                    description: 'This is to show you when any of your information are expired, meaning that their date of validity has passed such as your passport or memberships. You will be notified in advance when information comes due but in case you missed the notification, the different colours would make it easier to notice when you’re in the Vault. ',

                },
                {
                    id: 'id23',
                    url: '#id23',
                    topic: 'delegation',
                    subject: 'delegation',
                    title: 'Why use delegation?',
                    description: '<p>Sharing data in emails or texts is not secure or convenient.</p><p>Each time you send a password or any sensitive information by email, it is copied in plain text in the sent folder on your computer as well as in the sent folder on all other devices connected to your mailbox.It is also copied on your email provider servers and on your recipient’s email provider servers (which may actually include several different locations and data centers) and finally on your recipient’s computers or devices.</p><p>Using instant messaging apps is also insecure, as the information is usually not encrypted and is sent in plain text on the network.This means that it can be read by the company running the messaging service, and conversations are also generally saved in the history by these applications.</p><p>If you share using Regit delegation, then you are sharing your data in a secured and encrypted platform and not sending it back and forth around the internet.</p>',

                },
                {
                    id: 'id24',
                    url: '#id24',
                    topic: 'delegation',
                    subject: 'delegation',
                    title: 'How to share your information using delegation',
                    description: '<p>To share your information with someone, go to your information vault and select delegation. You must add the person to your trust network before you can set select the recipient. This is to ensure that you don’t accidently select someone you didn’t intended. When delegating, there are few roles you can choose from depending on your needs and preference.</p><ol><li>Normal:  Used for sharing your information with your delegatee without allowing your delegatee to see your information. This is great for getting someone to help you do things like register for an event, and sign up for services.Users don’t see your information and any missing required information is sent back to you to complete.</li><li>Super: Allows your delegate to see and edit your information. Use this wisely and only to those you trust with your life.</li><li>Custom: Allow you to customize what you want type of information you want to share and whether they can make changes on your behalf. If you select read; your delegate will is only allow to see the information.If you select write; your delegate can add and change your information for you. </li><li>Emergency: Is a standardized role that you can assign to someone to help you in cases of an emergency. Information available to your delegate once they accept is, your basic contact information, your passport, health card, insurance policy, your blood type, known allergies, and your designated emergency contact. These are critical information that you would want the delegatee to know so you can get the service you need without delay.</li></ol><p>Duration: You can select the start and end date.This is great for one- off delegation because you’re not available during a period or that you’re travelling with friends and want to share your information with them in case of emergency. Whatever the purpose, this feature allows you to better control your information. You can also delegate to someone outside of Regit by sending them an invitation to join the network. We keep your information safe by keeping all the content within the platform and not sharing it over email or the public web. The user must therefore sign up and be on Regit before any information is shared. The delegatee is not obligated to accept the relationship. The delegatee may reject the request if they don’t want to take on the responsibility. Once the relationship is accepted, you can manage your delegation relationship within the information Vault page.</p>'

                },
                {
                    name: 'network',
                    subject: 'network',
                    title: 'Network',
                    description: '<p>Your personal network is split between your normal and trust network. Trust network are those you have delegated information to and those that are designated as emergency contacts. You can assign anyone to be on your trust network. We are working on additional features that will simplify how you engage with your trust and non-trust network.</p>'
                },
                {
                    name: 'emergency-conact',
                    subject: 'network',
                    title: 'Emergency Network',
                    description: '<p>If you ever fill in a form that request for emergency contacts, you have two problem. One is you don’t fill in their information correctly (ie. their name, phone number, and address). Second, you never update this field when your emergency contact changes or their information changes. By designating someone as an emergency contact on Regit, and registering using this contact when required, you will never have to worry about filling in their information correctly, and not updating their information.</p>'
                },
                {
                    name: 'business-network',
                    subject: 'network',
                    title: 'Business Network',
                    description: '<p>These are businesses that you are following or interact with. Once you interact with the business, you are automatically following that business. You can unfollow anytime. </p>'
                }

            ];


            initNav();


        });


})();
