using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace ExViewer
{
    public class NavigationManager
    {
        private static readonly Dictionary<SystemNavigationManager, NavigationManager> dic = new Dictionary<SystemNavigationManager, NavigationManager>();

        private readonly SystemNavigationManager snm;

        public NavigationManager(SystemNavigationManager snm)
        {
            snm.BackRequested += this.Snm_BackRequested;
            this.snm = snm;
        }

        private void Snm_BackRequested(object sender, BackRequestedEventArgs e)
        {
            for (var i = this.listeners.Count - 1; i >= 0; i--)
            {
                var item = this.listeners[i];
                item.Invoke(this, e);
                if (e.Handled)
                    return;
            }
        }

        public AppViewBackButtonVisibility AppViewBackButtonVisibility
        {
            get => this.snm.AppViewBackButtonVisibility;
            set => this.snm.AppViewBackButtonVisibility = value;
        }

        private readonly List<EventHandler<BackRequestedEventArgs>> listeners = new List<EventHandler<BackRequestedEventArgs>>();

        public event EventHandler<BackRequestedEventArgs> BackRequested
        {
            add
            {
                if (value != null)
                    this.listeners.Add(value);
            }
            remove
            {
                var idx = this.listeners.LastIndexOf(value);
                if (idx != -1)
                    this.listeners.RemoveAt(idx);
            }
        }

        public static NavigationManager GetForCurrentView()
        {
            var snm = SystemNavigationManager.GetForCurrentView();
            if (dic.TryGetValue(snm, out var r))
                return r;
            return dic[snm] = new NavigationManager(snm);
        }
    }
}
