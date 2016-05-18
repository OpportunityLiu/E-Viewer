using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using Windows.Web.Http;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;


namespace ExClient
{
    public class ReCaptcha
    {
        static Uri recaptchaUri = new Uri("https://www.google.com/recaptcha/api/noscript?k=6LdtfgYAAAAAALjIPPiCgPJJah8MhAUpnHcKF8u_");
        static Uri imageBaseUri = new Uri("https://www.google.com/recaptcha/api/");

        public static IAsyncOperation<ReCaptcha> Get()
        {
            return Run(async token =>
            {
                using(var c = new HttpClient())
                {
                    var g = c.GetStringAsync(recaptchaUri);
                    token.Register(g.Cancel);
                    var h = await g;
                    var html = new HtmlDocument();
                    html.LoadHtml(h);
                    var recaptcha_challenge_field = html.GetElementbyId("recaptcha_challenge_field").GetAttributeValue("value", "");
                    var imgUri = html.DocumentNode.Descendants("img").Single().GetAttributeValue("src", "");
                    return new ReCaptcha(recaptcha_challenge_field, imgUri);
                }
            });
        }

        private ReCaptcha(string recaptchaChallengeField, string imageUri)
        {
            this.recaptchaChallengeField = recaptchaChallengeField;
            ImageUri = new Uri(imageBaseUri, imageUri);
        }

        //recaptcha_challenge_field
        private readonly string recaptchaChallengeField;

        public Uri ImageUri
        {
            get;
        }

        //recaptcha_response_field
        public IAsyncAction Submit(string response)
        {
            return Run(async token =>
            {
                using(var c = new HttpClient())
                {
                    var message = new Dictionary<string, string>()
                    {
                        ["recaptcha_challenge_field"] = recaptchaChallengeField,
                        ["recaptcha_response_field"] = response
                    };
                    var post = c.PostAsync(recaptchaUri, new HttpFormUrlEncodedContent(message));
                    token.Register(post.Cancel);
                    var res = await post;
                    var str = await res.Content.ReadAsStringAsync();
                    var html = new HtmlDocument();
                    html.LoadHtml(str);
                    var ans = html.DocumentNode.Descendants("textarea").SingleOrDefault();
                    if(ans == null)
                        throw new ArgumentException("The captcha was not entered correctly. Please try again.", nameof(response));
                    Answer = ans.InnerText;
                }
            });
        }

        internal string Answer
        {
            get;
            private set;
        }
    }
}
