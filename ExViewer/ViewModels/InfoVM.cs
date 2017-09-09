using ExClient.Status;
using ExClient;
using Opportunity.MvvmUniverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opportunity.MvvmUniverse.Commands;

namespace ExViewer.ViewModels
{
    public class InfoVM : ViewModelBase
    {
        public InfoVM()
        {
            this.RefreshStatus = new AsyncCommand(() => Status.RefreshAsync().AsTask(), () => Status != null);
            this.RefreshTaggingStatistics = new AsyncCommand(() => TaggingStatistics.RefreshAsync().AsTask(), () => TaggingStatistics != null);
        }

        public UserStatus Status => Client.Current.UserStatus;

        public TaggingStatistics TaggingStatistics => Client.Current.TaggingStatistics;

        public AsyncCommand RefreshStatus { get; }

        public AsyncCommand RefreshTaggingStatistics { get; }
    }
}
