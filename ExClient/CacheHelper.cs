using System;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient
{
    /// <summary>
    /// 用于操作缓存文件夹的辅助类
    /// </summary>
    internal static class CacheHelper
    {
        private readonly static StorageFolder localCache = ApplicationData.Current.LocalCacheFolder;

        public static StorageFolder LocalCache => localCache;

        public static IAsyncOperation<StorageFile> TryGetFileAsync(this StorageFolder folder, string name)
        {
            return Run(async token => (StorageFile)await folder.TryGetItemAsync(name));
        }
        public static IAsyncOperation<StorageFolder> TryGetFolderAsync(this StorageFolder folder, string name)
        {
            return Run(async token => (StorageFolder)await folder.TryGetItemAsync(name));
        }

        public static IAsyncOperation<StorageFile> SaveFileAsync(string folderName, string fileName, IBuffer buffer)
        {
            return Run(async token =>
            {
                var folder = await localCache.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);
                var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteBufferAsync(file, buffer);
                return file;
            });
        }
    }
}
