using System;

namespace GH.Core.ViewModels
{
    public class DisableUserViewModel
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? Until { get; set; }
        public string Reason { get; set; }
        public bool IsEnabled { get; set; }
    }
}