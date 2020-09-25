using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace EzMail.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var model = new Dictionary<string, string> {
                { "Nome", "Angelo" },
                { "Cognome", "Rotta" },
            };
            
            return View("/Views/EzMail/SimpleMessage.cshtml", model);
        }
    }
}
