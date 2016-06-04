using System;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient
{
    /// <summary>
    /// 用于操作缓存文件夹的辅助类
    /// </summary>
    internal static class StorageHelper
    {
        private readonly static StorageFolder localCache = ApplicationData.Current.LocalCacheFolder;
        private readonly static StorageFolder localState = ApplicationData.Current.LocalFolder;
        private readonly static StorageFolder temp = ApplicationData.Current.TemporaryFolder;

        public static StorageFolder LocalCache => localCache;
        public static StorageFolder LocalState => localState;
        public static StorageFolder Temporary => temp;

        public static IAsyncOperation<StorageItemThumbnail> GetIconOfExtension(string extension)
        {
            return Run(async token =>
            {
                var dummy = await temp.CreateFileAsync("dummy." + extension, CreationCollisionOption.OpenIfExists);
                return await dummy.GetThumbnailAsync(ThumbnailMode.SingleItem);
            });
        }

        public static IAsyncOperation<StorageFile> TryGetFileAsync(this StorageFolder folder, string name)
        {
            return Run(async token => (StorageFile)await folder.TryGetItemAsync(name));
        }
        public static IAsyncOperation<StorageFolder> TryGetFolderAsync(this StorageFolder folder, string name)
        {
            return Run(async token => (StorageFolder)await folder.TryGetItemAsync(name));
        }

        public static IAsyncOperation<StorageFile> SaveFileAsync(this StorageFolder folder, string fileName, IBuffer buffer)
        {
            return Run(async token =>
            {
                var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteBufferAsync(file, buffer);
                return file;
            });
        }

        public static IAsyncOperation<StorageFile> LoadFileAsync(string folderName, string fileName)
        {
            return Run(async token =>
            {
                var folder = await localCache.TryGetFolderAsync(folderName);
                if(folder == null)
                    return null;
                var file = await folder.TryGetFileAsync(fileName);
                return file;
            });
        }
    }
}
