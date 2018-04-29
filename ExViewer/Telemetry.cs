using ExClient.Galleries;
using ExClient.Search;
using ExViewer.ViewModels;
using ExViewer.Views;
using Newtonsoft.Json;
using Opportunity.Helpers.Universal;
using Opportunity.MvvmUniverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Email;
using Windows.ApplicationModel.Resources.Core;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.Core;
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
                dp.RunAsync((DispatchedHandler)addInfo).GetAwaiter().GetResult();
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
                if (exception.Data is null)
                {
                    sb.AppendLine($"{indentStr}Data: null");
                }
                else if (exception.Data.Count == 0)
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
                sb.AppendLine($"Package: {Package.Current.Id.FullName}");
                sb.AppendLine($"FrameStack:");
                foreach (var item in RootControl.RootController.Frame.BackStack)
                {
                    sb.AppendLine($"  {item.SourcePageType}, {item.Parameter}");
                }
                sb.AppendLine($"  >> {RootControl.RootController.Frame.CurrentSourcePageType}");
                foreach (var item in RootControl.RootController.Frame.ForwardStack)
                {
                    sb.AppendLine($"  {item.SourcePageType}, {item.Parameter}");
                }
                switch (RootControl.RootController.Frame?.Content)
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
                q.Append(item.Key);
                q.Append('=');
                q.Append(item.Value);
                q.Append(", ");
            }
            await EmailManager.ShowComposeNewEmailAsync(new EmailMessage
            {
                Subject = "Crash log for ExViewer",
                To =
                {
                    new EmailRecipient("opportunity@live.in", "Opportunity"),
                },
                Attachments =
                {
                    new EmailAttachment(LOG_FILE, logFile),
                },
                Body = $@"


Please check following infomation and attchments, and remove anything that you wouldn't like to send.
--------------------
-- Account Info --
UserId: {ExClient.Client.Current.UserId}
Config: {JsonConvert.SerializeObject(ExClient.Client.Current.Settings)}
-- Package Info --
PackageFullName: {Package.Current.Id.FullName}
PackageVersion: {Package.Current.Id.Version.ToVersion()}
PackageArchitecture: {Package.Current.Id.Architecture}
PackageNeedsRemediation: {Package.Current.Status.NeedsRemediation}
-- Device Info --
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
SystemSku: {eascdi.SystemSku}
--------------------
"
            });
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

        private static void AddtionalInfo(StringBuilder sb, GalleryPage gp)
        {
            if (gp.ViewModel is null)
            {
                return;
            }
            var pv = gp.Descendants<Windows.UI.Xaml.Controls.Pivot>("pv").FirstOrDefault();
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
            var fv = ip.Descendants<Windows.UI.Xaml.Controls.FlipView>("fv").FirstOrDefault();
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
            sb.AppendLine($"Gallery: Type={g.GetType()}, ID={g.ID}, Token={g.Token:x10}");
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
