using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
    public static class ExStorageHelper
    {
        private readonly static StorageFolder localCache = ApplicationData.Current.LocalCacheFolder;
        private readonly static StorageFolder localState = ApplicationData.Current.LocalFolder;
        private readonly static StorageFolder temp = ApplicationData.Current.TemporaryFolder;

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
