namespace GH.Core.BlueCode.Entity.Campaign
{
    public sealed class EnumCampaignStatus
    {
        public static readonly string Draft = "Draft"; // when save as Draft
        public static readonly string Pending = "Pending"; //when create new, and waiting for approval
        public static readonly string InActive = "InActive"; //after approval, but not active yet
        public static readonly string Active = "Active"; // the campaign is active, being shown to users
        public static readonly string Expired = "Expired"; // the campaign has been passed the duration of advertisement
        public static readonly string Template = "Template"; // when campaign is saved into template
        public static readonly string Remove = "Remove"; // the campaign is active, being shown to users
    }
    public sealed class EnumSRFIStatus
    {
        public static readonly string Invite = "Invite";
        public static readonly string Accept = "Accept";

    }
    public sealed class EnumSRFIInviteType
    {
        public static readonly string Email = "email";
        public static readonly string Member = "member";

    }
}
