using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExClient.Galleries;
using Newtonsoft.Json;
using ExClient.Galleries.Rating;

namespace ExClient.Api
{
    internal class RatingRequest : GalleryRequest, IRequestOf<RatingRequest, RatingResponse>
    {
        public RatingRequest(Gallery gallery, Score rating)
            : base(gallery)
        {
            if (!rating.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(rating));
            this.Rating = rating;
        }

        public override string Method => "rategallery";

        [JsonProperty("rating")]
        public Score Rating { get; }
    }

    internal class RatingResponse : ApiResponse, IResponseOf<RatingRequest, RatingResponse>
    {
        [JsonProperty("rating_avg")]
        public double AverageRating { get; set; }
        [JsonProperty("rating_usr")]
        public double UserRating { get; set; }
        [JsonProperty("rating_cnt")]
        public int RatingCount { get; set; }
        //[JsonProperty("rating_cls")]
        //public string RatingClass { get; set; }
    }

}
