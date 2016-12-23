using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;

namespace ExViewer.Helpers
{
    public static class ShareHandler
    {
        public static bool IsShareSupported => DataTransferManager.IsSupported();

        public static void Share(TypedEventHandler<DataTransferManager, DataRequestedEventArgs> handler)
        {
            new ShareHandlerStorage(handler);
            DataTransferManager.ShowShareUI();
        }


        private class ShareHandlerStorage
        {
            public ShareHandlerStorage(TypedEventHandler<DataTransferManager, DataRequestedEventArgs> handler)
            {
                this.handler = handler;
                var t = DataTransferManager.GetForCurrentView();
                t.DataRequested += T_DataRequested;
            }

            private TypedEventHandler<DataTransferManager, DataRequestedEventArgs> handler;

            private void T_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
            {
                var d = args.Request.Data;
                d.Properties.Title = Package.Current.DisplayName;
                d.Properties.ApplicationName = Package.Current.DisplayName;
                d.Properties.PackageFamilyName = Package.Current.Id.FamilyName;
                handler?.Invoke(sender, args);
                sender.DataRequested -= T_DataRequested;
            }
        }
    }
}
