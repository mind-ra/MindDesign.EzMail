using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EzMail.Lib;
using EzMail.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace EzMail.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IEzMailService mailService;

        public IndexModel(IEzMailService mailService, 
            ILogger<IndexModel> logger)
        {
            this.mailService = mailService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var body = await mailService.Renderer.RenderViewToStringAsync("/Views/EzMail/TestMessage.cshtml", "Angelo");

            await mailService.SendMailAsync("oggetto", body,
                fromAddress: "Mind <info@minddesign.it>",
                toAddresses: new string[] { "Angelo Rotta <angelo.rotta@minddesign.it>" },
                replyAddresses: new string[] { "<info@minddesign.it>" },
                ccAddresses: new string[] { "m.martino@minddesign.it" },
                bccAddresses: new string[] { "Sandra <sandra.l@minddesign.it>" });

            //await mailService.SendSimpleMessage(new Dictionary<string, string> {
            //    { "Nome", "Angelo" },
            //    { "Cognome", "Rotta" },
            //    { "Messaggio", "Messggio di test" }
            //});

            // In reality, you would have this on a POST and pass along user input and not just have the Confirm Account link be the Index page… but #Demoware
            //await _registerAccountService.Register("testmctestyface@contoso.com", Url.Page("./Index"));
            return Page();
        }
    }
}
