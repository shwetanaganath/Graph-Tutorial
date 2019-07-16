using graph_tutorial.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace graph_tutorial.Controllers
{
    public class SecurityController : Controller
    {
        // GET: Security
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<ActionResult> ChangePassword()
        {

            var passwordUpdated = await GraphHelper.UpdatePassword();

            ViewBag.PasswordChange = passwordUpdated;
            return View();

        }
    }
}