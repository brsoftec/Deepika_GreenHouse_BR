namespace GH.Core.ViewModels
{
    public class InviteEmailViewModel
    {
        public string category { get; set; }
        public string subject { get; set; }
        public string message { get; set; }
        public string fromName { get; set; }
        public string toName { get; set; }
        public string backLink { get; set; }
    }   
    public class PaymentEmailViewModel
    {
        public string subject { get; set; }
        public string productName { get; set; }
        public string amount { get; set; }
        public string method { get; set; }
        public string transactionId { get; set; }
        public string nextCharge { get; set; }
        public string toName { get; set; }
        public string backLink { get; set; }
    }    
    public class BusinessClaimEmailViewModel
    {
        public string subject { get; set; }
        public string message { get; set; }
        public string ucbName { get; set; }
        public string ucbId { get; set; }
        public string claimerName { get; set; }
        public string claimerPhone { get; set; }
        public string claimerEmail { get; set; }
    }

}