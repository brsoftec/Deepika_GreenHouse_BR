namespace GH.Core.BlueCode.Entity.Notification
{
    public sealed class EnumNotificationType
    {
        //Sync Email Notification
        public static readonly string NotifyMissingInformationVaultForRegistration = "Notify Missing Information Vault For Registration";


        public static readonly string NotifyBillingFirst = "Billing First";
        public static readonly string NotifyBillingFailed = "Billing Failed";
        public static readonly string NotifyBillingRenew = "Billing Renew";
        public static readonly string NotifyPushToVault = "Push To Vault";
        public static readonly string NotifyPushToVaultAccecpt = "Push To Vault Accecpt";
        public static readonly string NotifyPushToVaultDeny = "Push To Vault Deny";
        public static readonly string NotifyHandShakeVaultChanged = "HandShake Vault Changed";
        public static readonly string NotifyInvitedHandshake = "Invited Handshake";
        public static readonly string NotifyInvitedHandshakeOutsite = "Invited Handshake Outsite";
        public static readonly string NotifyEmailHandshakeRequest = "Handshake Request Email Notification";
        public static readonly string NotifySyncHandshakeToMailOutsite = "Sync Email Notification";
        public static readonly string NotifyRequestdHandshake = "Request Handshake";

        public static readonly string NotifyRegister = "Register Interaction";
        public static readonly string NotifyBusinessUnregister = "Business Unregister";
        
        public static readonly string NotifyJoinHandshake = "Join Handshake";
        public static readonly string NotifyPauseHandshake = "Pause Handshake";
        public static readonly string NotifyResumeHandshake = "Resume Handshake";
        public static readonly string NotifyTerminateHandshake = "Terminate Handshake";
        public static readonly string NotifyTerminateIndividualHandshake = "Terminate Individual Handshake";
        public static readonly string NotifyAcknowledgeHandshake = "Acknowledge Handshake";
        public static readonly string NotifyExpiredDateHandshake = "Expired date Handshake";

        // SRFI
        public static readonly string NotifyInviteSRFI = "Invited SRFI";

        public static readonly string DelegationRequest = "Delegation Request";
        public static readonly string DelegationAccept = "Delegation Accept";
        public static readonly string DelegationActivated = "Delegation Activated";
        
        public static readonly string DelegationDeny = "Delegation Deny";
        public static readonly string DelegationRemove = "Delegation Remove";

        //Network
        public static readonly string InviteFriend = "Invite Friend";
        public static readonly string InviteFriendFollowEmail = "Invite Friend Follow Email";
        public static readonly string CancelFriend = "Cancel Friend";
        public static readonly string AcceptFriend = "Accept Friend";
        public static readonly string DenyFriend = "Deny Friend";
        public static readonly string MoveFriend = "Move Friend";
        public static readonly string RemoveFriend = "Remove Friend";
        public static readonly string UpdateFriend = "Update Friend";

        //Family
        public static readonly string InviteFamily = "Invite Family";
        public static readonly string AcceptFamily = "Accept Family";
       
        //Emergency
        public static readonly string InviteEmergency = "Invite Emergency";
        public static readonly string AcceptEmergency = "Accept Emergency";
        public static readonly string DenyEmergency = "Deny Emergency";
        public static readonly string RemoveEmergency = "Remove Emergency";
        public static readonly string UpdateEmergency = "Update Emergency";
        
        public static readonly string UpdateRelationship = "Update Relationship";
        
        public static readonly string WorkflowRemoveMember = "Workflow Remove Member";
        public static readonly string WorkflowUpdateMember = "Workflow Update Member";
        public static readonly string WorkflowInviteMember = "Workflow Invite Member";
        public static readonly string WorkflowInviteMemberFollowEmail = "Workflow Invite Member Follow Email";
        public static readonly string WorkflowCancelInvitation = "Workflow Cancel Invitation";
        public static readonly string WorkflowAcceptInvitation = "Workflow Accept Invitation";
        public static readonly string WorkflowDenyInvitation = "Workflow Deny Invitation";
        
        public static readonly string InviteEmail = "Invite Email";
        
        public static readonly string InteractionParticipateDelegated = "Interaction Participate Delegated";
        public static readonly string InteractionUnparticipate = "Interaction Unparticipate";
    }
}
