using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Windows.Storage.Streams;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;

namespace ExClient
{
    /// <summary>
    /// 用于操作缓存文件夹的辅助类
    /// </summary>
    internal static class CacheHelper
    {
        private static StorageFolder localCache = ApplicationData.Current.LocalCacheFolder;

        public static StorageFolder LocalCache => localCache;

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

        public static IAsyncOperation<StorageFile> SaveStringAsync(string folderName, string fileName, string content)
        {
            return Run(async token =>
            {
                var folder = await localCache.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);
                var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, content);
                return file;
            });
        }

        public static IAsyncOperation<StorageFile> LoadFileAsync(string folderName, string fileName)
        {
            return Run(async token =>
            {
                try
                {
                    var folder = await localCache.GetFolderAsync(folderName);
                    var file = await folder.GetFileAsync(fileName);
                    return file;
                }
                catch(Exception)
                {
                    return null;
                }
            });
        }

        public static IAsyncOperation<string> LoadStringAsync(string folderName, string fileName)
        {
            return Run(async token =>
            {
                var file = await LoadFileAsync(folderName, fileName);
                if(file == null)
                    return null;
                return await FileIO.ReadTextAsync(file);
            });
        }
    }
}
