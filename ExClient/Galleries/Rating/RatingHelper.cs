using ExClient.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ExClient.Galleries.Rating
{
    internal static class RatingHelper
    {
        public static IAsyncOperation<RatingResponse> RatingAsync(Gallery gallery, Score rating)
        {
            var reqInfo = new RatingRequest(gallery, rating);
            return reqInfo.GetResponseAsync(true);
        }
    }
}
