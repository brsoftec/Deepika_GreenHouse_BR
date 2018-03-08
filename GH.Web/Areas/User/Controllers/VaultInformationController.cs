using System.Web.Mvc;

namespace GH.Web.Areas.User.Controllers
{
    [Authorize]
    public class VaultInformationController : BaseController
    {

        // GET: User/User
        public ActionResult Index()
        {
            AccountController acc = new AccountController();

            if(!acc.IsCheckPinVault())
            {
                return Redirect("/User/SMSAuthencation?TypeRedirect=Vault");
            }
          
           
            return View();
        }


        public ActionResult Index2()
        {
            return View();
        }

    }
}