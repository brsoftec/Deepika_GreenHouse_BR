namespace GH.Core.ViewModels
{
    public class VerifyEmailViewModel
    {
        public VerifyType Type { get; set; }
        public string Message { get; set; }
        public string Email { get; set; }
    }

    public enum VerifyType
    {
        SendMailSuccessfully,
        SessionIsExpired,
        AccountNotFound,
        AccountIsVerified,
        Exception,
        VerifySuccessfully
    }
}