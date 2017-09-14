using ExClient.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExClient.Launch
{
    internal abstract class SearchHandlerBase : UriHandler
    {
        protected AdvancedSearchOptions GetAdvancedSearchOptions(UriHandlerData data)
        {
            var advanced = new AdvancedSearchOptions();
            foreach (var item in data.Queries)
            {
                var b = item.Value.QueryValueAsBoolean();
                switch (item.Key)
                {
                case "f_sname":
                    advanced.SearchName = b;
                    break;
                case "f_stags":
                    advanced.SearchTags = b;
                    break;
                case "f_sdesc":
                    advanced.SearchDescription = b;
                    break;
                case "f_storr":
                    advanced.SearchTorrentFilenames = b;
                    break;
                case "f_sto":
                    advanced.GalleriesWithTorrentsOnly = b;
                    break;
                case "f_sdt1":
                    advanced.SearchLowPowerTags = b;
                    break;
                case "f_sdt2":
                    advanced.SearchDownvotedTags = b;
                    break;
                case "f_sh":
                    advanced.ShowExpungedGalleries = b;
                    break;
                case "f_sr":
                    advanced.SearchMinimumRating = b;
                    break;
                case "f_srdd":
                    advanced.MinimumRating = item.Value.QueryValueAsInt32();
                    break;
                case "skip_mastertags":
                    advanced.SkipMasterTags = b;
                    break;
                }
            }
            return advanced;
        }

        protected Category GetCategory(UriHandlerData data)
        {
            var category = Category.Unspecified;
            foreach (var item in data.Queries)
            {
                var b = item.Value.QueryValueAsBoolean();
                switch (item.Key)
                {
                case "f_doujinshi":
                    if (b) category |= Category.Doujinshi;
                    break;
                case "f_manga":
                    if (b) category |= Category.Manga;
                    break;
                case "f_artistcg":
                    if (b) category |= Category.ArtistCG;
                    break;
                case "f_gamecg":
                    if (b) category |= Category.GameCG;
                    break;
                case "f_western":
                    if (b) category |= Category.Western;
                    break;
                case "f_non-h":
                    if (b) category |= Category.NonH;
                    break;
                case "f_imageset":
                    if (b) category |= Category.ImageSet;
                    break;
                case "f_cosplay":
                    if (b) category |= Category.Cosplay;
                    break;
                case "f_asianporn":
                    if (b) category |= Category.AsianPorn;
                    break;
                case "f_misc":
                    if (b) category |= Category.Misc;
                    break;
                }
            }
            return category;
        }
    }
}
