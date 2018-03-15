using ExClient.Galleries;
using ExClient.Search;
using ExViewer.ViewModels;
using ExViewer.Views;
using Opportunity.MvvmUniverse;
using System;
using System.Linq;
using System.Text;
using Windows.UI.Xaml.Media;

namespace ExViewer
{
    public static class Telemetry
    {
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
                    if (value == null)
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
            DispatcherHelper.RunAsyncOnUIThread(() =>
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
            }).AsTask().Wait();
            return sb.ToString();
        }

        private static void AddtionalInfo(StringBuilder sb, SavedPage svp)
        {
            if (svp.ViewModel == null)
                return;
            AddtionalInfo(sb, svp.ViewModel);
        }

        private static void AddtionalInfo(StringBuilder sb, CachedPage cp)
        {
            if (cp.ViewModel == null)
                return;
            AddtionalInfo(sb, cp.ViewModel);
        }

        private static void AddtionalInfo(StringBuilder sb, PopularPage pp)
        {
            if (pp.VM == null)
                return;
            AddtionalInfo(sb, pp.VM);
        }

        private static void AddtionalInfo(StringBuilder sb, FavoritesPage fp)
        {
            if (fp.ViewModel == null)
                return;
            AddtionalInfo(sb, fp.ViewModel);
        }

        private static void AddtionalInfo(StringBuilder sb, SearchPage sp)
        {
            if (sp.VM == null)
                return;
            AddtionalInfo(sb, sp.VM);
        }

        private static void AddtionalInfo(StringBuilder sb, GalleryPage gp)
        {
            if (gp.ViewModel == null)
                return;
            var pv = gp.Descendants<Windows.UI.Xaml.Controls.Pivot>("pv").FirstOrDefault();
            if (pv != null)
                sb.AppendLine($"Pivot: SelectedIndex={pv.SelectedIndex}");
            AddtionalInfo(sb, gp.ViewModel);
        }

        private static void AddtionalInfo(StringBuilder sb, ImagePage ip)
        {
            if (ip.ViewModel == null)
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
            if (s == null)
                return;
            sb.AppendLine($"SearchResult: Type={s.GetType()}, Uri={s.SearchUri}");
        }

        private static void AddtionalInfo<T>(StringBuilder sb, GalleryListVM<T> glVM)
            where T : Gallery
        {
            var gl = glVM.Galleries;
            if (gl == null)
                return;
            sb.AppendLine($"GalleryList: Type={gl.GetType()}, Count={gl.Count}");
            sb.AppendLine($"Gallery: Type={typeof(T)}");
        }

        private static void AddtionalInfo(StringBuilder sb, GalleryVM gVM)
        {
            var g = gVM.Gallery;
            if (g == null)
                return;
            sb.AppendLine($"Gallery: Type={g.GetType()}, ID={g.ID}, Token={g.Token:x10}");
        }

        private static void AddtionalInfo(StringBuilder sb, PopularVM VM)
        {
            var c = VM.Galleries;
            if (c == null)
                return;
            sb.AppendLine($"PopularCollection: Type={c.GetType()}, Count={c.Count}");
        }
    }
}
