using EzMail.Lib;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EzMail.Web.Framework
{
    public partial class DemoMailService : IDemoMailService
    {
        private readonly IEzMailService mailService;
        public DemoMailService(IEzMailService mailService)
        {
            this.mailService = mailService;
        }


        public async Task SendSimpleMessage(IDictionary<string, string> model)
        {
            var body = await mailService.Renderer.RenderViewToStringAsync("/Views/EzMail/EzSimpleMessage.cshtml", model);

            await mailService.SendMailAsync("oggetto", body, 
                fromAddress:"Mind <info@minddesign.it>",
                toAddresses: new string[] { "Angelo Rotta <angelo.rotta@minddesign.it>" },
                replyAddresses: new string[] { "<info@minddesign.it>" },
                ccAddresses: new string[] { "m.martino@minddesign.it" },
                bccAddresses: new string[] { "Sandra <sandra.l@minddesign.it>" });
        }
    }

    public interface IDemoMailService
    {
        Task SendSimpleMessage(IDictionary<string, string> model);
    }
}
