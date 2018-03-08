//var myApp = getApp("myApp", true);
var app = angular.module('myApp');

// Create the instant search filter

app.filter('searchFor',
    function() {

        // All filters must return a function. The first parameter
        // is the data that is to be filtered, and the second is an
        // argument that may be passed with a colon (searchFor:searchString)

        return function(arr, searchString) {

            if (!searchString) {
                return arr;
            }

            var result = [];

            searchString = searchString.toLowerCase();

            // Using the forEach helper method to loop through the array
            angular.forEach(arr,
                function(item) {

                    if (item.title.toLowerCase().indexOf(searchString) !== -1) {
                        result.push(item);
                    }

                });

            return result;
        };

    });

app.controller('FAQController',function($scope) {
    $scope.ScrollTo = function (id) {
        $('html, body').animate({
                scrollTop: $('#' + id).offset().top
            },
            1000);
    };

    $scope.openTab = function (id) {
        var tabpills = document.getElementsByClassName("tabpills");
        $(tabpills).removeClass("active"); // remove active on all tab nav-pills
        $('#tab' + id).addClass("active"); // add active on tab

        var tabpane = document.getElementsByClassName("tab-pane");
        $(tabpane).removeClass("active"); // remove active on all tab contents
        $('#' + id).addClass("active"); // add active on tab content
    }
// The controller

    function InstantSearchController($scope) {

        // The data model. These items would normally be requested via AJAX,
        // but are hardcoded here for simplicity. See the next example for
        // tips on using AJAX.

        $scope.tabs = [
            {
                id: 'regit-01',
                display: 'Home',
                image: '',
            },
            {
                id: 'regit-02',
                display: 'Individual',
                image: '',
            },
            {
                id: 'regit-03',
                display: 'Business',
                image: '/Areas/About/Content/img/under-construction.svg',
            },
            {
                id: 'regit-04',
                display: 'Developer',
                image: '/Areas/About/Content/img/under-construction.svg',
            },
            {
                id: 'regit-05',
                display: 'Security & Safety',
                image: '',
            }
        ];

        $scope.categories = [
            {
                catid: 'cat01',
                id: 'id01',
                catTitle: 'General Account',
                tab: 'regit-01'
            },
            {
                catid: 'cat02',
                id: 'id02',
                catTitle: 'General Information',
                tab: 'regit-01'
            },
            {
                catid: 'cat03',
                id: 'id03',
                catTitle: 'Synchronization and Sharing issues on Regit',
                tab: 'regit-01'
            },
            {
                catid: 'cat04',
                id: 'id20',
                catTitle: 'Functions',
                tab: 'regit-05'
            },
            {
                catid: 'cat05',
                id: 'id05',
                catTitle: 'Brainstorming',
                tab: 'regit-05'
            },
            {
                catid: 'cat06',
                id: 'id06',
                catTitle: 'Security',
                tab: 'regit-05'
            }
        ];


        $scope.items = [
            {
                id: 'id01',
                url: '#id01',
                tab: 'regit-01',
                title: 'I forgot my password, what can I do? ',
                long_description: 'You can reset your password by clicking on the “Forgot Password” link. Answer your security question, then select your preferred method to receive the one-time PIN to reset your password. If by email, check your email for the 4-digit pin. If by phone, a 4- digit pin will be sent via SMS. <br><br>Tip: Do not use this password for any other account or save it anywhere that is not encrypted.',

            },
            {
                id: 'id02',
                url: '#id02',
                tab: 'regit-01',
                title: 'I cannot remember my password ',
                long_description: 'Your login password should: <br><br>Have been 8 or more characters long<br>Include at least one number<br>Include at least one upper case letter<br>Include at least one lower case letter<br><br>Tip: Check and ensure your CAP LOCK key is not on',

            },
            {
                id: 'id03',
                url: '#id03',
                tab: 'regit-01',
                title: 'Reset Account',
                long_description: 'If you don\'t remember your registered email address, we will not be able to reset your account for security reasons.',

            },
            {
                id: 'id04',
                url: '#id04',
                tab: 'regit-01',
                title: 'Regit does not work.... What should I do?',
                long_description: 'Our team is committed to ensuring you have access to your information when you need it. If you encounter issues and unable to find the solution on this page, please contact us.<br><br> In this article, you will find tips and content designed to walk you through the main issues currently being raised by our users. If you have an issue or question that is not listed here, do not worry: it may be covered in another article. Also, you can always reach out to our Support team anytime!',

            },
            {
                id: 'id05',
                url: '#id05',
                tab: 'regit-01',
                category: 'General Info',
                title: 'Why should I put my personal information in Regit?',
                long_description: 'Regit is a secure and convenient way to store and use of your information within your trust network. Security alerts are also sent straight to your device when any of your accounts may be compromised so you can update your old password and stop hackers in their tracks. <br>If you are worried about putting you’re your information in Regit, learn more about our security by clicking <a href="#securitysafety" data- toggle="tab">here</a>!',

            },
            {
                id: 'id06',
                url: '#id06',
                tab: 'regit-01',
                title: 'Why should I put my IDs into Regit?',
                long_description: 'Regit can make your Personal Data work for you and fill any field from any form on Regit. <br>This data is securely encrypted just like your login information. <br>The more data you enter in your Regit account, the more effective and smart Regit will be and the more time you will save!<br><br> In the IDs section, you can enter your:<ul ><li>ID Card</li><li>Passport</li><li>Driver\'s License</li> <li>Social Security Card</li> </ul > <strong>No more mistakes</strong> filling out your passport number when booking a flight! <br><br>Regit will do all the leg work.',

            },
            {
                id: 'id07',
                url: '#id07',
                tab: 'regit-01',
                category: 'Synchronization and Sharing issues on Regit',
                title: 'Synchronization and Sharing issues on Regit',
                long_description: 'All your information across all your devices are actively sync if connected to the internet. <br>If you are actively connected to the internet, and continue to experience issue, try to clear your cache as old content may be stored on your browser preventing new information to show.<br>Depending your browser, go to clear history, select clear cache, close your browser, and restart again.',

            },
            {
                id: 'id08',
                url: '#id08',
                tab: 'regit-01',
                title: 'Some functions are not working',
                long_description: 'We try our best to bring you the best quality application, for every possible scenario, but if we have missed something, we apologize and please let us know. <br>If you are still unable to get Regit to work properly with certain functions, you can use our browser extension to report this site!',

            },
            {
                id: 'id09',
                url: '#id09',
                tab: 'regit-01',
                title: 'Regit logs out all the time, can I change that?',
                long_description: 'Yes. You need to disable the Automatic logout option in the <strong>Account Settings > Activity Log > Keep a record of your logging in/out Regit > Yes</strong> <br>Please note that if you decide to disable automatic logout, you should be careful to log out each time you leave your computer.',

            },
            {
                id: 'id10',
                url: '#id10',
                tab: 'regit-01',
                title: 'Can several people share the same Regit account?',
                long_description: 'No. Your information is specific to you. You are in control of how your share your information. <br>That said, we understand sometimes you may want to share your information, so we built numerous features for you to share your information without compromising your information, and giving you control.<br>Check out the delegation feature which allows you to select what you want to share, and how long you want to share that information for. <br>There are other features like emergency and family contact which allow you to continuously share basic information like contact information until the relationship is cancelled.',

            },
            {
                id: 'id11',
                url: '#id11',
                tab: 'regit-01',
                title: 'Can I access my data on Regit offline?',
                long_description: 'We are currently working on that feature. Once ready, we will broadcast to the Regit community.',

            },
            {
                id: 'id12',
                url: '#id12',
                tab: 'regit-01',
                title: 'Suspending your Regit account',
                long_description: 'We understand that circumstances changes and to protect your information, we made it possible for you to suspend your accounts. <br>Your information is still there but no one will be able to find you or engage with you.All notifications will also be turned off.<br>To suspend your account, you will need to be re- authenticated to ensure it is only you who is suspending your account.',

            },
            {
                id: 'id13',
                url: '#id13',
                tab: 'regit-01',
                title: 'Delete your account and all your data permanently',
                long_description: 'Regit is all about giving users total control of their information, this includes if they wish the right to terminate and delete their account. <br>Upon the request, we will delete all information on your account within 60 days.<br>The 60 day is designed to be a backup measure just in case you change your mind.<br>During the 60 days period, your account act as though it’s in suspended mode. No one can engage with you and all notifications are turned off.<br>To terminate your account, you will need to be re- authenticated to ensure it is only you who is terminating your account.',

            },
            {
                id: 'id14',
                url: '#id14',
                tab: 'regit-02',
                title: 'How do I create a Regit account?',
                long_description: 'If you don\'t have a Regit account, you can create one in a few steps: <br><br><br> &gt; Go to <a href="http://www.regit.today" target="_blank">www.regit.today</a> <br>&gt; If you see the signup form, fill out your name, email address or mobile phone number, password. If you don\'t see the form, click <b>Sign Up</b> then fill out the form. <br> &gt;Click <b>Sign Up</b>. <br> &gt;You\'ll need to confirm a PIN code. Then you have to answer 3 security questions to finish. <br> &gt;At last, You may need to add your profile picture and enter some basic informations. <br> If you already have a Regit account, you can log into your account by entering your email and password and clicking <b>Log In</b>. <br><br> Note: you must be at least 13 years old to create a Regit account.',

            },
            {
                id: 'id15',
                url: '#id15',
                tab: 'regit-02',
                title: 'Why am I being asked to add my phone number to my account?',
                long_description: 'Security purposes.',

            },
            {
                id: 'id16',
                url: '#id16',
                tab: 'regit-03',
                title: 'Interactions Module',
                long_description: 'The interactions module is where you create campaigns to interact with your customers and get new customers. Every interactions is designed to make it easy for you and the customers to exchange information, eliminating forms, errors, and missed opportunities.\
                            <br><br>The number of interactions available to you depends on your subscription. \
                            <br>1.	Broadcast\
                            <br>This is as it sounds. An Broadcast  sent to your existing customers, instead of over emails which are often ignored, your customers can see it directly on the Regit discovery page in the beautiful format it’s meant to be.\
                            <br>2.	Events\
                            <br>Its identical to broadcast interaction except, it also allow you to specify the required information if any, and the costs to the event.If your customers sign up for the event, it will also automatically added to their calendar so they won’t forget.The information collected are aggregated into the campaign analytics and can easily be viewed anytime.Events interactions once approved are shown in the users discovery feed.\
                            <br><br>A unique URL and QR code is also created so that you can share this over email or other mediums directly.\
                            <br>3.	Registrations\
                            <br>Registration interactions are used to collect information required for a particular purposeInformation can be as simple as the customer name or ask complex as a credit card or school application. The goal is to simplify the registration process so that users don’t need to constantly type the same information over and over again which increases changes of transaction abandonment and errors. Registrations interactions once approved are shown in the users discovery feed similar to adverts.\
                            <br><br>A unique URL and QR code is also created so that you can share this over email or other mediums directly.\
                            <br>4.	Static Request for information\
                            <br>Static Request for Information (STFI) interaction is identical to registration interaction with the exception that it does not get broadcast into customer’s discovery feed. Instead, SRFI are only shown on the public business page as it’s not meant to be a discovery feature but an automated way for customers to submit information. This is commonly used by doctors, lawyers, agents, advisors and other office type environment where users must fill in a form before getting services.\
                            <br><br>A unique URL and QR code is also created so that you can share this over email or other mediums directly.\
                            <br>5.	Push to Vault – Business\
                            <br>For businesses that constantly share customer personal information with their customers via emails, papers, and text. A better and more convenient way for the customer is using the Push to Vault. This will ensure that the user directly gets it and allows them to use it in the future conveniently.\
                            <br>For example, if user signed up for a membership, you can push their membership details to them so they can refer to it easily without having to look for it.\
                            <br>Other use cases includes, pushing work information, medical records, and even non traditional information like tailor size.\
                            <br>In the future, we will allow you to push other types of information like events, reminders, meeting invites, and tasks, so all the information is digitized and centralize for the customer, delivering unheard of customer experience.\
                            <br>Push to Vault are only sent to a defined user and not shown on the discovery feed.\
                            <br>6.	Sync Interaction\
                            <br>A Sync for you and your customers to say in touch so that when they update certain information, your business is notified so you can action accordingly. For example, if a customer change their address, you would be notified of the change in address if address was a handshake field. The update will allow you to keep your information up to date and for you to better service your customers.\
                            <br>Handshakes requires the customers to approve before any information is exchanged. The customer and you also have the right to cancel the handshake anytime and for any reason.\
                            <br>The handshake is also a good way for you to know who your customers are. If a customer handshake relationship is cancelled, you know that the customers is no longer interested in your products or service. \
                            <br>Be careful to only ask required information to increase participation from your customers.\
                            <br>7.	Reservation interaction (In development)\
                            <br>Reservation is a service that allows businesses to automate their reservation process without having to rely on another service. By integrating with the regit platform, customers also don’t need to rely on traditional methods to make a reservation therefore simplifying the reservation process.\
                            <br>By using regit reservation, you will be able to collect demographic information of your customers so you can better manage your business.\
                            <br>8.	Membership Interaction (in development)\
                            <br>The membership module allows you to manage your membership within the Regit platform. Combined with features like handshake and other interactions like adverts will allow you to continuously engage with your customers. No need to buy separate software when you can do it all within one platform.\
                            <br>9.	Public Forms \
                            <br>Public forms are used to create unique customers forms that can be completed anonymously and for users not on the Regit network. ',

            },
            {
                id: 'id25',
                url: '#id25',
                tab: 'regit-03',
                title: 'Workflow',
                long_description: 'The goal of the business workflow is for you to provision access to your account to other colleagues. The types of access depends on the role.\
                            <br>Access is on a hierarchical basis so if you have access to the highest level, you are able to do tasks that the lowest level can do.\
                            <br>Approver – Ability to create and approve interactions campaign. This role should be researched for people who have final authority to approve campaigns. Once approved, unless cancelled, the campaign becomes live and is available for the public to see.\
                            <br>Editor – This role allow users to create campaigns but cannot approve.This segregation of roles ensure that there is a maker and checker ensuring that the quality of content is appropriate and professional.\
                            <br>Admin – The admin role has the ability to provision access, as editor, approver or even other admins.',

            },
            {
                id: 'id26',
                url: '#id26',
                tab: 'regit-03',
                title: 'Business Profile Page',
                long_description: 'The business profile page, if made public, is for the public to find you. There it includes information about your public, including business hours, and contact information where relevant. The public page will also include all your interaction campaigns that are public allowing non existing customers to interact and follow you.',

            },
            {
                id: 'id18',
                url: '#id18',
                tab: 'regit-05',
                title: 'How can I be 100 percent sure that Regit is itself 100 percent secure? ',
                long_description: 'To be completely honest, we have no clear answer for you either. No matter what, we will continue to stand by our word and not sell your information and do our utmost best to secure your information. We are well aware that with all things there is no way to know 100 percent. <br><br>There is also no way to be 100 percent sure that your phone company isn\'t listening in to your calls, that your credit company isn\'t laughing at your list of purchases, that your G.P.S. device isn\'t tracking your every move, that your house isn\'t bugged, that the government isn\'t slowly poisoning you, or that aliens aren\'t puppeteering you from distant planets. <br>But you could take comfort in knowing that if we were accessing your account, we would be out of business instantly.',

            },
            {
                id: 'id19',
                url: '#id19',
                tab: 'regit-05',
                title: 'How does Regit protect my information?',
                long_description: 'Regit protects your information in a number of ways. \
                                \r<br>1. Robust password policy – ensuring that your password is at a minimum standard so it can’t easily be guessed. \
                                \r<br>2.	Mandatory multi-factor authentication – we use a combination of authentication depending on the use. \
                                \r<br>a.SMS OTP for account verification and changes to your password, Vault PIN, and security questions. \
                                \r<br>b.Vault PIN – a unique 4 digit PIN to access your information Vault after you login. \
                                \r<br>c.Security Password – An additional layer required before you can reset your account. \
                                \r<br>d.Email PIN – an option for users to reset their account using their email instead of SMS after correctly answering the security question. \
                                \r<br>3.	SSL – which encrypts your data when it’s sent over the internet. \
                                \r<br>4.	Database encryption \
                                \r<br>5.	Microsoft Azure – We don’t just use any cloud provider. We use only Microsoft Azure for all services including hosting, storage, and cloud computing. Azure is recognized as one of the most reliable and safest data centres in the world. \
                                \r<br>6.	Biometrics Authentication – coming soon with mobile, mandatory biometric authentication as an additional layer of protecting your information. \
                                \r<br>7.	IP and Devices monitoring – when you login in an unknown device, we will require you to authenticate yourself with SMS OTP. ',

            },
            {
                id: 'id20',
                url: '#id20',
                tab: 'regit-05',
                title: 'How to search in Regit',
                long_description: 'There are multiple search options in Regit. This is itself a security measures ensuring that sensitive information is not mixed with public information. The search function will get easier and smarter over time, saving you time when you need it most. Just start typing and we will immediately pull up suggestions that will help you find what you are looking for, be it a credential, a purchase, and ID, a contact or just about everything saved in Regit. <br>Home Search Bar on the Top Menu Bar – This allows you to search other users and businesses on the network.Only public information is available. <br> Information Vault Search Bar – This allows you to search all the information in your Vault or your delegator vault (depending on your access). <br>Activity Log Search Bar – This allows you to search all the information in your activity Log only.<br>Network Search on the network page – This allows you to find other people on the network.',

            },
            {
                id: 'id21',
                url: '#id21',
                tab: 'regit-05',
                title: 'How to save time by adding your IDs to Regit',
                long_description: 'Regit can make your Personal Data work for you, and fill any field from any business on regit.<br>The more data you enter in your Regit account, the more effective and smart Regit will be, \
                        and the more time you will save!<br>To add information to your Vault, click on the Vault icon on the top right of the menu bar. \
                        Enter your Vault PIN, select add information and choose the category of information.<br>Other people on your trust network may also push information to your Vault to save you time from having to enter it yourself. \
                        Before any information is push to your vault, you must authorize it to prevent unintentional information being added to your Vault.<br>The last way which information could be added to your Vault is via your super delegatee or people who have write access to your Vault. \
                        These are people you have authorized previously to have read and write access to your Vault.',

            },
            {
                id: 'id22',
                url: '#id22',
                tab: 'regit-05',
                title: 'Why are some of my ID information written in a different colour? ',
                long_description: 'This is to show you when any of your information are expired, meaning that their date of validity has passed such as your passport or memberships. \
                            You will be notified in advance when information comes due but in case you missed the notification, the different colours would make it easier to notice when you’re in the Vault. ',

            },
            {
                id: 'id23',
                url: '#id23',
                tab: 'regit-05',
                title: 'Why use Delegation?',
                long_description: 'Sharing data in emails or texts is not secure or convenient.<br>Each time you send a password or any sensitive information by email, it is copied in plain text in the sent folder on your computer as well as in the sent folder on all other devices connected to your mailbox.It is also copied on your email provider servers and on your recipient’s email provider servers (which may actually include several different locations and data centers) and finally on your recipient’s computers or devices.<br>Using instant messaging apps is also insecure, as the information is usually not encrypted and is sent in plain text on the network.This means that it can be read by the company running the messaging service, and conversations are also generally saved in the history by these applications.<br>If you share using Regit delegation, then you are sharing your data in a secured and encrypted platform and not sending it back and forth around the internet.',

            },
            {
                id: 'id24',
                url: '#id24',
                tab: 'regit-05',
                title: 'How to Share your information using Delegate',
                long_description: 'To share your information with someone, go to your information vault and select delegation. You must add the person to your trust network before you can set select the recipient. This is to ensure that you don’t accidently select someone you didn’t intended. When delegating, there are few roles you can choose from depending on your needs and preference.<br>1.	Normal:  Used for sharing your information with your delegatee without allowing your delegatee to see your information. This is great for getting someone to help you do things like register for an event, and sign up for services.Users don’t see your information and any missing required information is sent back to you to complete.<br>2.	Super: Allows your delegate to see and edit your information. Use this wisely and only to those you trust with your life.<br>3.	Custom: Allow you to customize what you want type of information you want to share and whether they can make changes on your behalf. If you select read; your delegate will is only allow to see the information.If you select write; your delegate can add and change your information for you. <br>4.	Emergency: Is a standardized role that you can assign to someone to help you in cases of an emergency. Information available to your delegate once they accept is, your basic contact information, your passport, health card, insurance policy, your blood type, known allergies, and your designated emergency contact. These are critical information that you would want the delegatee to know so you can get the service you need without delay.<br><br>Duration: You can select the start and end date.This is great for one- off delegation because you’re not available during a period or that you’re travelling with friends and want to share your information with them in case of emergency. Whatever the purpose, this feature allows you to better control your information. You can also delegate to someone outside of Regit by sending them an invitation to join the network. We keep your information safe by keeping all the content within the platform and not sharing it over email or the public web. The user must therefore sign up and be on Regit before any information is shared. The delegatee is not obligated to accept the relationship. The delegatee may reject the request if they don’t want to take on the responsibility. Once the relationship is accepted, you can manage your delegation relationship within the information Vault page. ',

            },
            {
                id: 'id17',
                url: '#id17',
                tab: 'regit-04',
                title: '',
                long_description: '',

            }
        ];
    }
});