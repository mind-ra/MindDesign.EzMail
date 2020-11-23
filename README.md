# MindDesign.EzMail

**EzMail** is a simple Razor Class Library designed to simplify the sending of email in .Net Core 3.0 apps.

---

### Installation

**EzMail** is on [NuGet](https://www.nuget.org/packages/MindDesign.EzMail/)

``` powershell
Install-Package MindDesign.EzMail
```

Minimum Requirements: **.NET Core 3.0**.

---

### Initialization

**EzMail** must be configured inside <code>appsettings.json</code> and initialized as a service inside <code>Startup.cs</code>.

If <code>DebugData.Active</code> is <code>true</code> EzMail overwrite all receivers with <code>DebugData.Email</code>.

```json
"Ezmail": {
    "DebugData": {
      "Active": true,
      "Email": "debug-email@domain.it"
    },
    "SmtpParameters": {
      "Host": "smtp.domain.it",
      "Port": 25,
      "Username": "your-username@domain.it",
      "Password": "your-password",
      "EnableSSL": false
    }
  }
```

```csharp
public class Startup
{
    ...
    public void ConfigureServices(IServiceCollection services)
    {
        ...
        services.AddEzMailServices();
        ...
    }
...
}
```

---

### Usage

After initialization you can get the service in a Controller or in aRazor Pages with Dependecy injection.

```csharp

public class IndexModel : PageModel
{
    private readonly IEzMailService _mailService;
    public IndexModel(IEzMailService mailService)
    {
        _mailService = mailService;
    }
    public async Task<IActionResult> OnGetAsync()
    {
        var model = new Dictionary<string, string>() {
            { "Name" , "Angelo"},
            { "Email" , "angelo@domain.it"}
        };

        var body = await _mailService.Renderer.RenderViewToStringAsync("/Views/EzMail/EzSimpleMessage.cshtml", model);

        await _mailService.SendMailAsync(
            subject: "subject",
            body: body, 
            fromAddress: "from@domain.it",
            toAddresses: new string[] { "Angelo <to@domain.it>", "second@domain.it" },
            replyAddresses: new string[] { "replyTo@domain.it" },
            ccAddresses: new string[] { "cc@domain.it" },
            bccAddresses: new string[] { "Bcc <bcc@domain.it>" });
        
        return Page();
    }
}
```
---

## Integrated Views

You can overwrite the integrated views, to personalize the layout of your email, or you can use them as provided.

The views provided are:

```
/Views/EzMail/EzSimpleMessage.cshtml

/Views/EzMail/_EzMailLayout.cshtml
/Views/EzMail/_EzMailHeader.cshtml
/Views/EzMail/_EzMailFooter.cshtml
```

#### EzSimpleMessage.cshtml

```html
@model IDictionary<string, string>
@{
    var odd = true;
}

<table border="0" cellpadding="0" cellspacing="0" style="border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;">
    <tr>
        <td colspan="2" class="content-block" style="font-family: sans-serif; vertical-align: middle; padding: 5px; font-size: 24px; text-align:center">
            Messaggio semplice
        </td>
    </tr>
    @foreach (var kvp in Model)
    {
        <tr>
            <td class="content-block" style="font-family: sans-serif; font-weight:bold; vertical-align: middle; padding: 5px; font-size: 16px; width:1%; @(odd ? "background:#e3e3e3;":"background:#f3f3f3;")">
                @kvp.Key
            </td>
            <td class="content-block" style="font-family: sans-serif; vertical-align: middle; padding: 5px; font-size: 16px; @(odd ? "background:#e3e3e3;":"background:#f3f3f3;")">
                @kvp.Value
            </td>
        </tr>
        odd = !odd;
    }
</table>
```

#### _EzMailLayout.cshtml

``` html
<!doctype html>
<html>
<head>
    <meta name="viewport" content="width=device-width">
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
    <title>Simple Transactional Email</title>
    <style>
        /* -------------------------------------
            RESPONSIVE AND MOBILE FRIENDLY STYLES
        ------------------------------------- */
        @@media only screen and (max-width: 620px) {
            table[class=body] h1 {
                font-size: 28px !important;
                margin-bottom: 10px !important;
            }

            table[class=body] p,
            table[class=body] ul,
            table[class=body] ol,
            table[class=body] td,
            table[class=body] span,
            table[class=body] a {
                font-size: 16px !important;
            }

            table[class=body] .wrapper,
            table[class=body] .article {
                padding: 10px !important;
            }

            table[class=body] .content {
                padding: 0 !important;
            }

            table[class=body] .container {
                padding: 0 !important;
                width: 100% !important;
            }

            table[class=body] .main {
                border-left-width: 0 !important;
                border-radius: 0 !important;
                border-right-width: 0 !important;
            }

            table[class=body] .btn table {
                width: 100% !important;
            }

            table[class=body] .btn a {
                width: 100% !important;
            }

            table[class=body] .img-responsive {
                height: auto !important;
                max-width: 100% !important;
                width: auto !important;
            }
        }

        /* -------------------------------------
            PRESERVE THESE STYLES IN THE HEAD
        ------------------------------------- */
        @@media all {
            .ExternalClass {
                width: 100%;
            }

                .ExternalClass,
                .ExternalClass p,
                .ExternalClass span,
                .ExternalClass font,
                .ExternalClass td,
                .ExternalClass div {
                    line-height: 100%;
                }

            .apple-link a {
                color: inherit !important;
                font-family: inherit !important;
                font-size: inherit !important;
                font-weight: inherit !important;
                line-height: inherit !important;
                text-decoration: none !important;
            }

            #MessageViewBody a {
                color: inherit;
                text-decoration: none;
                font-size: inherit;
                font-family: inherit;
                font-weight: inherit;
                line-height: inherit;
            }
        }
    </style>
</head>
<body class="" style="background-color: #f6f6f6; font-family: sans-serif; -webkit-font-smoothing: antialiased; font-size: 14px; line-height: 1.4; margin: 0; padding: 0; -ms-text-size-adjust: 100%; -webkit-text-size-adjust: 100%;">
    <span class="preheader" style="color: transparent; display: none; height: 0; max-height: 0; max-width: 0; opacity: 0; overflow: hidden; mso-hide: all; visibility: hidden; width: 0;">This is preheader text. Some clients will show this text as a preview.</span>
    <table border="0" cellpadding="0" cellspacing="0" class="body" style="border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; background-color: #f6f6f6;">
        <tr>
            <td style="font-family: sans-serif; font-size: 14px; vertical-align: top;">&nbsp;</td>
            <td class="container" style="font-family: sans-serif; font-size: 14px; vertical-align: top; display: block; Margin: 0 auto; max-width: 580px; padding: 10px; width: 580px;">
                <div class="content" style="box-sizing: border-box; display: block; Margin: 0 auto; max-width: 580px; padding: 10px;">

                    @await Html.PartialAsync("./_EzMailHeader.cshtml")

                    <!-- START CENTERED WHITE CONTAINER -->
                    <table class="main" style="border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; background: #ffffff; border-radius: 3px;">

                        <!-- START MAIN CONTENT AREA -->
                        <tr>
                            <td class="wrapper" style="font-family: sans-serif; font-size: 14px; vertical-align: top; box-sizing: border-box; padding: 20px;">
                                <table border="0" cellpadding="0" cellspacing="0" style="border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;">
                                    <tr>
                                        <td style="font-family: sans-serif; font-size: 14px; vertical-align: top;">
                                            @RenderBody()
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>

                        <!-- END MAIN CONTENT AREA -->
                    </table>

                    @await Html.PartialAsync("./_EzMailFooter.cshtml")
                    <!-- END CENTERED WHITE CONTAINER -->
                </div>
            </td>
            <td style="font-family: sans-serif; font-size: 14px; vertical-align: top;">&nbsp;</td>
        </tr>
    </table>
</body>
</html>
```

#### _EzMailHeader.cshtml

``` html
<!-- START HEADER -->
<div class="header" style="clear: both; margin-bottom: 10px; text-align: center; width: 100%;">
    <table border="0" cellpadding="0" cellspacing="0" style="border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;">
        <tr>
            <td class="content-block" style="font-family: sans-serif; vertical-align: top; padding-bottom: 10px; padding-top: 10px; font-size: 32px; text-align: center;">
                YOUR HEADER
            </td>
        </tr>
    </table>
</div>
<!-- END HEADER -->
```

#### _EzMailFooter.cshtml

``` html
<!-- START FOOTER -->
<div class="footer" style="clear: both; Margin-top: 10px; text-align: center; width: 100%;">
    <table border="0" cellpadding="0" cellspacing="0" style="border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;">
        <tr>
            <td class="content-block" style="font-family: sans-serif; vertical-align: top; padding-bottom: 10px; padding-top: 10px; font-size: 12px; color: #999999; text-align: center;">
                <span class="apple-link" style="color: #999999; font-size: 12px; text-align: center;">YOUR FOOTER DATA</span>
            </td>
        </tr>
        <tr>
            <td class="content-block powered-by" style="font-family: sans-serif; vertical-align: top; padding-bottom: 10px; padding-top: 10px; font-size: 12px; color: #999999; text-align: center;">
                Powered by <a href="https://github.com/mind-ra/MindDesign.EzMail" style="color: #999999; font-size: 12px; text-align: center; text-decoration: none;">EzMail</a>.
            </td>
        </tr>
    </table>
</div>
<!-- END FOOTER -->
```
