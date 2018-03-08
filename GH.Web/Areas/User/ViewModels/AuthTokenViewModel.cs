using System;
using GH.Core.BlueCode.Entity.AuthToken;

namespace GH.Web.Areas.User.ViewModels
{
    public class AuthTokenViewModel
    {
        public string tokenId { get; set; }
        public string accountId { get; set; }
        public string accessToken { get; set; }
        public DateTime issued { get; set; }
        public DateTime expires { get; set; }
        public double expiresIn { get; set; }
        public AuthClientInfo clientInfo { get; set; }

        public AuthTokenViewModel(AuthToken token)
        {
            tokenId = token.Id.ToString();
            accountId = token.AccountId;
            accessToken = token.AccessToken;
            issued = token.Issued;
            expires = token.Expires;
            clientInfo = token.ClientInfo;

            var left = (token.Expires - DateTime.UtcNow);
            var mins = left.TotalMinutes;
            if (mins <= 0) expiresIn = 0;
            else expiresIn = Math.Round(mins, 2);
        }
    }
}