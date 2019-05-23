using ExClient.Galleries;
using ExClient.Search;
using ExViewer.ViewModels;
using ExViewer.Views;
using Newtonsoft.Json;
using Opportunity.Helpers.Universal;
using Opportunity.MvvmUniverse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Email;
using Windows.ApplicationModel.Resources.Core;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ExViewer
{
    public static class Telemetry
    {
        private static long[] keySections = new[]
        {
            1268538959,
            60405,
            18157,
            40680,
            126331606925878,
        };

        public static string AppCenterKey => string.Join("-", keySections.Select(i => i.ToString("x")));

        private const string LOG_FILE = "AppLog.log";

        public static string LogException(Exception ex)
        {
            string toRaw(string value)
            {
                return value.Replace("\"", "\"\"");
            }

            var sb = new StringBuilder();
            sb.AppendLine("--------Exception Info--------");
            addEx(ex, 0);
            sb.AppendLine("--------Other Info--------");
            var dp = CoreApplication.MainView.Dispatcher;
            if (dp.HasThreadAccess)
            {
                addInfo();
            }
            else
            {
                dp.RunAsync(addInfo).GetAwaiter().GetResult();
            }
            var r = sb.ToString();
            log(r);
            return r;

            void addEx(Exception exception, int indent)
            {
                var indentStr = new string(' ', indent);
                sb.AppendLine($"{indentStr}Type: {exception.GetType()}");
                sb.AppendLine($"{indentStr}HResult: 0x{exception.HResult:X8}");
                sb.AppendLine($"{indentStr}HelpLink: {exception.HelpLink}");
                sb.AppendLine($"{indentStr}Message: @\"{toRaw(exception.Message)}\"");
                sb.AppendLine($"{indentStr}DisplayedMessage: @\"{toRaw(exception.GetMessage())}\"");
                sb.AppendLine($"{indentStr}Source: {exception.Source}");
                if (exception.Data.IsNullOrEmpty())
                {
                    sb.AppendLine($"{indentStr}Data: empty");
                }
                else
                {
                    sb.AppendLine($"{indentStr}Data:");
                    foreach (var item in exception.Data.Keys)
                    {
                        var value = exception.Data[item];
                        if (value is null)
                        {
                            sb.AppendLine($"{indentStr}  [{item}]: null");
                        }
                        else
                        {
                            sb.AppendLine($"{indentStr}  [{item}]: Type={value.GetType()}, ToString=@\"{toRaw(value.ToString())}\"");
                        }
                    }
                }
                sb.AppendLine($"{indentStr}StackTrace:");
                foreach (var item in (exception.StackTrace ?? "").Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    sb.Append(indentStr);
                    sb.Append("  ");
                    sb.AppendLine(item);
                }
                if (exception.InnerException is Exception inner)
                {
                    sb.AppendLine($"{indentStr}Inner Exception:");
                    addEx(inner, indent + 2);
                }
                else
                {
                    sb.AppendLine($"{indentStr}Inner Exception: null");
                }
            }

            void addInfo()
            {
                sb.AppendLine($"Package: {Package.Current.Id.FullName} @ {Github.COMMIT}");
                if (!(RootControl.RootController.Frame is Frame frame))
                {
                    return;
                }

                sb.AppendLine($"FrameStack:");
                foreach (var item in frame.BackStack)
                {
                    sb.AppendLine($"  {item.SourcePageType}, {item.Parameter}");
                }
                sb.AppendLine($"  >> {frame.CurrentSourcePageType} <<");
                foreach (var item in frame.ForwardStack)
                {
                    sb.AppendLine($"  {item.SourcePageType}, {item.Parameter}");
                }
                switch (frame.Content)
                {
                case GalleryPage gp:
                    AddtionalInfo(sb, gp);
                    break;
                case ImagePage ip:
                    AddtionalInfo(sb, ip);
                    break;
                case SearchPage sp:
                    AddtionalInfo(sb, sp);
                    break;
                case WatchedPage wp:
                    AddtionalInfo(sb, wp);
                    break;
                case FavoritesPage fp:
                    AddtionalInfo(sb, fp);
                    break;
                case PopularPage pp:
                    AddtionalInfo(sb, pp);
                    break;
                case CachedPage cp:
                    AddtionalInfo(sb, cp);
                    break;
                case SavedPage svp:
                    AddtionalInfo(sb, svp);
                    break;
                default:
                    break;
                }
            }
        }

        private static async void log(string data)
        {
            try
            {
                var time = DateTimeOffset.Now;
                await semaphore.WaitAsync().ConfigureAwait(false);
                if (logFile is null)
                {
                    logFile = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(LOG_FILE, CreationCollisionOption.OpenIfExists);
                }
                await FileIO.AppendTextAsync(logFile, $"[{time:u}]\r\n{data}\r\n\r\n");
            }
            catch
            {
                // ignore
            }
            finally
            {
                semaphore.Release();
            }
        }

        public static async void SendLog()
        {
            try
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
                if (logFile is null)
                {
                    logFile = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(LOG_FILE, CreationCollisionOption.OpenIfExists);
                }
            }
            finally
            {
                semaphore.Release();
            }
            var eascdi = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
            var q = new StringBuilder();
            foreach (var item in ResourceContext.GetForCurrentView().QualifierValues)
            {
                q.Append(item.Key)
                 .Append('=')
                 .Append(item.Value)
                 .Append(", ");
            }
            var s = new StringBuilder();
            foreach (var item in ExClient.Client.Current.Settings.RawSettings)
            {
                s.Append(item.Key)
                 .Append('=')
                 .Append(item.Value)
                 .Append(", ");
            }
            var aInfo = await createStreamRef($@"
UserId: {ExClient.Client.Current.UserId}
Config: {s}");
            var pInfo = await createStreamRef($@"
PackageFullName: {Package.Current.Id.FullName}
PackageVersion: {Package.Current.Id.Version.ToVersion()}
PackageArchitecture: {Package.Current.Id.Architecture}
PackageNeedsRemediation: {Package.Current.Status.NeedsRemediation}
GithubBranch: {Github.BRANCH}
GithubCommit: {Github.COMMIT}");
            var dInfo = await createStreamRef($@"
Qualifiers: {q}
DeviceForm: {AnalyticsInfo.DeviceForm}
DeviceFamily: {AnalyticsInfo.VersionInfo.DeviceFamily}
DeviceFamilyVersion: {ApiInfo.DeviceFamilyVersion}
DeviceId: {eascdi.Id}
OperatingSystem: {eascdi.OperatingSystem}
SystemFirmwareVersion: {eascdi.SystemFirmwareVersion}
SystemHardwareVersion: {eascdi.SystemHardwareVersion}
SystemManufacturer: {eascdi.SystemManufacturer}
SystemProductName: {eascdi.SystemProductName}
SystemSku: {eascdi.SystemSku}");
            var email = new EmailMessage
            {
                Subject = "Crash log for ExViewer",
                To =
                {
                    new EmailRecipient("opportunity@live.in", "Opportunity"),
                },
                Attachments =
                {
                    new EmailAttachment(LOG_FILE, logFile),
                    new EmailAttachment("AccountInfo.log", aInfo),
                    new EmailAttachment("DeviceInfo.log", dInfo),
                    new EmailAttachment("PackageInfo.log", pInfo),
                },
                Body = $@"
Please check attchments, and remove anything that you wouldn't like to send.
"
            };
            await EmailManager.ShowComposeNewEmailAsync(email);

            async Task<RandomAccessStreamReference> createStreamRef(string content)
            {
                var stream = new InMemoryRandomAccessStream();
                await stream.WriteAsync(CryptographicBuffer.ConvertStringToBinary(content, BinaryStringEncoding.Utf8));
                return RandomAccessStreamReference.CreateFromStream(stream);
            }
        }

        private static System.Threading.SemaphoreSlim semaphore = new System.Threading.SemaphoreSlim(1, 1);
        private static StorageFile logFile;

        private static void AddtionalInfo(StringBuilder sb, SavedPage svp)
        {
            if (svp.ViewModel is null)
            {
                return;
            }
            AddtionalInfo(sb, svp.ViewModel);
        }

        private static void AddtionalInfo(StringBuilder sb, CachedPage cp)
        {
            if (cp.ViewModel is null)
            {
                return;
            }
            AddtionalInfo(sb, cp.ViewModel);
        }

        private static void AddtionalInfo(StringBuilder sb, PopularPage pp)
        {
            if (pp.ViewModel is null)
            {
                return;
            }
            AddtionalInfo(sb, pp.ViewModel);
        }

        private static void AddtionalInfo(StringBuilder sb, FavoritesPage fp)
        {
            if (fp.ViewModel is null)
            {
                return;
            }
            AddtionalInfo(sb, fp.ViewModel);
        }

        private static void AddtionalInfo(StringBuilder sb, SearchPage sp)
        {
            if (sp.ViewModel is null)
            {
                return;
            }
            AddtionalInfo(sb, sp.ViewModel);
        }

        private static void AddtionalInfo(StringBuilder sb, WatchedPage wp)
        {
            if (wp.ViewModel is null)
            {
                return;
            }
            AddtionalInfo(sb, wp.ViewModel);
        }

        private static void AddtionalInfo(StringBuilder sb, GalleryPage gp)
        {
            if (gp.ViewModel is null)
            {
                return;
            }
            var pv = gp.Descendants<Pivot>("pv").FirstOrDefault();
            if (pv != null)
            {
                sb.AppendLine($"Pivot: SelectedIndex={pv.SelectedIndex}");
            }
            AddtionalInfo(sb, gp.ViewModel);
        }

        private static void AddtionalInfo(StringBuilder sb, ImagePage ip)
        {
            if (ip.ViewModel is null)
            {
                return;
            }
            var fv = ip.Descendants<FlipView>("fv").FirstOrDefault();
            if (fv != null)
            {
                sb.AppendLine($"FlipView: SelectedIndex={fv.SelectedIndex}");
            }
            AddtionalInfo(sb, ip.ViewModel);
        }

        private static void AddtionalInfo<T>(StringBuilder sb, SearchResultVM<T> srVM)
            where T : SearchResult
        {
            var s = srVM.SearchResult;
            if (s is null)
            {
                return;
            }
            sb.AppendLine($"SearchResult: Type={s.GetType()}, Uri={s.SearchUri}");
        }

        private static void AddtionalInfo<T>(StringBuilder sb, GalleryListVM<T> glVM)
            where T : Gallery
        {
            var gl = glVM.Galleries;
            if (gl is null)
            {
                return;
            }
            sb.AppendLine($"GalleryList: Type={gl.GetType()}, Count={gl.Count}");
            sb.AppendLine($"Gallery: Type={typeof(T)}");
        }

        private static void AddtionalInfo(StringBuilder sb, GalleryVM gVM)
        {
            var g = gVM.Gallery;
            if (g is null)
            {
                return;
            }
            sb.AppendLine($"Gallery: Type={g.GetType()}, ID={g.Id}, Token={g.Token:x10}");
        }

        private static void AddtionalInfo(StringBuilder sb, PopularVM VM)
        {
            var c = VM.Galleries;
            if (c is null)
            {
                return;
            }
            sb.AppendLine($"PopularCollection: Type={c.GetType()}, Count={c.Count}");
        }
    }
}
