using Microsoft.Extensions.Hosting;
using MindDesign.EzMail;
using MindDesign.EzMail.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EzMail.CLI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var hostBuilder = CreateHostBuilder(args).Build();

            var mailService = (IEzMailService)hostBuilder.Services.GetService(typeof(IEzMailService));

            var model = new Dictionary<string, string>() {
            { "Name" , "Angelo"},
            { "Email" , "angelo@domain.it"}
        };

            var body = await mailService.Renderer.RenderViewToStringAsync("/Views/EzMail/TestMessage.cshtml", model);

            await mailService.SendMailAsync(
                subject: "subject",
                body: body,
                fromAddress: "from@domain.it",
                toAddresses: new string[] { "Angelo <to@domain.it>", "second@domain.it" });
        }



        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddEzMailServices();
            });
    }
}
