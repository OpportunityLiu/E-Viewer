using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient.Launch
{
    public abstract class LaunchResult
    {
        internal LaunchResult() { }
    }

    public enum GalleryLaunchStatus
    {
        Default,
        Image,
        Torrent
    }

    public sealed class GalleryLaunchResult : LaunchResult
    {
        public Gallery Gallery
        {
            get;
        }

        public int CurrentIndex
        {
            get;
        }

        public GalleryLaunchStatus Status
        {
            get;
        }

        internal GalleryLaunchResult(Gallery g, int index, GalleryLaunchStatus status)
        {
            this.Gallery = g;
            this.CurrentIndex = index;
            this.Status = status;
        }
    }

    public sealed class SearchLaunchResult : LaunchResult
    {
        internal SearchLaunchResult(SearchResult data)
        {
            Data = data;
        }

        public SearchResult Data
        {
            get;
        }
    }
}
