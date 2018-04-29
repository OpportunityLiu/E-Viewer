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
            0x4b9c5e4f,
            0xebf5,
            0x46ed,
            0x9ee8,
            0x72e5de8e0236,
        };

        public static string AppCenterKey => string.Join("-", keySections.Select(i => i.ToString("x")));

        public static string LogException(Exception ex)
        {
            string toRaw(string value)
            {
                return value.Replace("\"", "\"\"");
            }

            var sb = new StringBuilder();
            do
            {
                sb.AppendLine($"Type: {ex.GetType()}");
                sb.AppendLine($"HResult: 0x{ex.HResult:X8}");
                sb.AppendLine($"Message: @\"{toRaw(ex.Message)}\"");
                sb.AppendLine($"DisplayedMessage: @\"{toRaw(ex.GetMessage())}\"");
                sb.AppendLine();
                sb.AppendLine("Data:");
                foreach (var item in ex.Data.Keys)
                {
                    var value = ex.Data[item];
                    if (value is null)
                        sb.AppendLine($"    [{item}]: null");
                    else
                        sb.AppendLine($"    [{item}]: Type={value.GetType()}, ToString=@\"{toRaw(value.ToString())}\"");
                }
                sb.AppendLine("StackTrace:");
                sb.AppendLine(ex.StackTrace);
                ex = ex.InnerException;
                sb.AppendLine("--------Inner Exception--------");
            } while (ex != null);
            sb.AppendLine();
            sb.AppendLine("--------Other Info--------");
            sb.AppendLine($"Page: {RootControl.RootController.CurrentPageName}");
            var dp = CoreApplication.MainView.Dispatcher;
            if (dp.HasThreadAccess)
                addInfo(sb);
            else
                dp.RunAsync(() => addInfo(sb)).GetAwaiter().GetResult();
            var r = sb.ToString();
            log(r);
            return r;
        }

        private static void addInfo(StringBuilder sb)
        {
            var page = RootControl.RootController.Frame?.Content;
            switch (page)
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

        private static async void log(string data)
        {
            try
            {
                var time = DateTimeOffset.Now;
                await semaphore.WaitAsync();
                if (logFile is null)
                    logFile = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync("AppLog", CreationCollisionOption.OpenIfExists);
                await FileIO.AppendTextAsync(logFile, $"----[{time:u}]----------------------------------\r\n{data}\r\n\r\n");
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
                await semaphore.WaitAsync();
                if (logFile is null)
                    logFile = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync("AppLog", CreationCollisionOption.OpenIfExists);
            }
            finally
            {
                semaphore.Release();
            }
            var eascdi = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
            var q = "";
            foreach (var item in ResourceContext.GetForCurrentView().QualifierValues)
            {
                q += $"`{item.Key}`=`{item.Value}` ";
            }
            await EmailManager.ShowComposeNewEmailAsync(new EmailMessage
            {
                Subject = "Crash log for ExViewer",
                To =
                {
                    new EmailRecipient("opportunity@live.in","Opportunity"),
                },
                Attachments =
                {
                    new EmailAttachment("AppLog", logFile),
                },
                Body = $@"
Please check following infomation and remove anything that you wouldn't like to send.
----------
PackageFullName: {Package.Current.Id.FullName}
PackageVersion: {Package.Current.Id.Version.ToVersion()}
PackageArchitecture: {Package.Current.Id.Architecture}
PackageNeedsRemediation: {Package.Current.Status.NeedsRemediation}
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
----------
"
            });
        }

        private static System.Threading.SemaphoreSlim semaphore = new System.Threading.SemaphoreSlim(1, 1);
        private static StorageFile logFile;

        private static void AddtionalInfo(StringBuilder sb, SavedPage svp)
        {
            if (svp.ViewModel is null)
                return;
            AddtionalInfo(sb, svp.ViewModel);
        }

        private static void AddtionalInfo(StringBuilder sb, CachedPage cp)
        {
            if (cp.ViewModel is null)
                return;
            AddtionalInfo(sb, cp.ViewModel);
        }

        private static void AddtionalInfo(StringBuilder sb, PopularPage pp)
        {
            if (pp.ViewModel is null)
                return;
            AddtionalInfo(sb, pp.ViewModel);
        }

        private static void AddtionalInfo(StringBuilder sb, FavoritesPage fp)
        {
            if (fp.ViewModel is null)
                return;
            AddtionalInfo(sb, fp.ViewModel);
        }

        private static void AddtionalInfo(StringBuilder sb, SearchPage sp)
        {
            if (sp.ViewModel is null)
                return;
            AddtionalInfo(sb, sp.ViewModel);
        }

        private static void AddtionalInfo(StringBuilder sb, GalleryPage gp)
        {
            if (gp.ViewModel is null)
                return;
            var pv = gp.Descendants<Windows.UI.Xaml.Controls.Pivot>("pv").FirstOrDefault();
            if (pv != null)
                sb.AppendLine($"Pivot: SelectedIndex={pv.SelectedIndex}");
            AddtionalInfo(sb, gp.ViewModel);
        }

        private static void AddtionalInfo(StringBuilder sb, ImagePage ip)
        {
            if (ip.ViewModel is null)
                return;
            var fv = ip.Descendants<Windows.UI.Xaml.Controls.FlipView>("fv").FirstOrDefault();
            if (fv != null)
                sb.AppendLine($"FlipView: SelectedIndex={fv.SelectedIndex}");
            AddtionalInfo(sb, ip.ViewModel);
        }

        private static void AddtionalInfo<T>(StringBuilder sb, SearchResultVM<T> srVM)
            where T : SearchResult
        {
            var s = srVM.SearchResult;
            if (s is null)
                return;
            sb.AppendLine($"SearchResult: Type={s.GetType()}, Uri={s.SearchUri}");
        }

        private static void AddtionalInfo<T>(StringBuilder sb, GalleryListVM<T> glVM)
            where T : Gallery
        {
            var gl = glVM.Galleries;
            if (gl is null)
                return;
            sb.AppendLine($"GalleryList: Type={gl.GetType()}, Count={gl.Count}");
            sb.AppendLine($"Gallery: Type={typeof(T)}");
        }

        private static void AddtionalInfo(StringBuilder sb, GalleryVM gVM)
        {
            var g = gVM.Gallery;
            if (g is null)
                return;
            sb.AppendLine($"Gallery: Type={g.GetType()}, ID={g.ID}, Token={g.Token:x10}");
        }

        private static void AddtionalInfo(StringBuilder sb, PopularVM VM)
        {
            var c = VM.Galleries;
            if (c is null)
                return;
            sb.AppendLine($"PopularCollection: Type={c.GetType()}, Count={c.Count}");
        }
    }
}
