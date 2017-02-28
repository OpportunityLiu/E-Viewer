using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.Web.Http;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;


namespace ExClient
{
    public class ReCaptcha
    {
        static Uri recaptchaUri = new Uri("https://www.google.com/recaptcha/api/noscript?k=6LdtfgYAAAAAALjIPPiCgPJJah8MhAUpnHcKF8u_");
        static Uri imageBaseUri = new Uri("https://www.google.com/recaptcha/api/");

        public static IAsyncOperation<ReCaptcha> FetchAsync()
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
                    var cf = html.GetElementbyId("recaptcha_challenge_field").GetAttributeValue("value", "");
                    var imgUri = html.DocumentNode.Descendants("img").Single().GetAttributeValue("src", "");
                    return new ReCaptcha(cf, imgUri);
                }
            });
        }

        private ReCaptcha(string recaptchaChallengeField, string imageUri)
        {
            this.recaptchaChallengeField = recaptchaChallengeField;
            this.ImageUri = new Uri(imageBaseUri, imageUri);
        }

        //recaptcha_challenge_field
        private readonly string recaptchaChallengeField;

        public Uri ImageUri
        {
            get;
        }

        //recaptcha_response_field
        public IAsyncAction Submit(string result)
        {
            return Run(async token =>
            {
                using(var c = new HttpClient())
                {
                    IEnumerable<KeyValuePair<string, string>> message()
                    {
                        yield return new KeyValuePair<string, string>("recaptcha_challenge_field", this.recaptchaChallengeField);
                        yield return new KeyValuePair<string, string>("recaptcha_response_field", result);
                    }
                    var post = c.PostAsync(recaptchaUri, new HttpFormUrlEncodedContent(message()));
                    token.Register(post.Cancel);
                    var res = await post;
                    var str = await res.Content.ReadAsStringAsync();
                    var html = new HtmlDocument();
                    html.LoadHtml(str);
                    var ans = html.DocumentNode.Descendants("textarea").SingleOrDefault();
                    if(ans == null)
                        throw new ArgumentException(LocalizedStrings.Resources.WrongCaptcha);
                    this.Answer = ans.InnerText;
                }
            });
        }

        public bool ReCaptchaCompleted => this.Answer != null;

        internal string Answer
        {
            get;
            private set;
        }
    }
}
