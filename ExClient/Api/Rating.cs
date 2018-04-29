using ExClient.Galleries;
using ExClient.Galleries.Rating;
using Newtonsoft.Json;
using System;

namespace ExClient.Api
{
    internal class RatingRequest : GalleryRequest<RatingResponse>
    {
        public RatingRequest(Gallery gallery, Score rating)
            : base(gallery)
        {
            if (!rating.IsDefined())
            {
                throw new ArgumentOutOfRangeException(nameof(rating));
            }

            this.Rating = rating;
        }

        public override string Method => "rategallery";

        [JsonProperty("rating")]
        public Score Rating { get; }
    }

    internal class RatingResponse : ApiResponse
    {
        [JsonProperty("rating_avg")]
        public double AverageScore { get; set; }
        [JsonProperty("rating_usr")]
        public double UserScore { get; set; }
        [JsonProperty("rating_cnt")]
        public int RatingCount { get; set; }
        [JsonProperty("rating_cls")]
        public string RatingImageClass { get; set; }
    }
}
