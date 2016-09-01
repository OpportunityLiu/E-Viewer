using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace ExViewer
{
    internal static class AccountManager
    {
        public static readonly string AccountName = "e-viewer";

        public static PasswordCredential CurrentCredential
        {
            get
            {
                var pv = new PasswordVault();
                try
                {
                    return pv.FindAllByResource(AccountName).First();
                }
                catch(Exception ex) when(ex.HResult == -2147023728)
                {
                    return null;
                }
            }
            set
            {
                var pv = new PasswordVault();
                var old = CurrentCredential;
                if(old != null)
                {
                    pv.Remove(old);
                }
                if(value != null)
                {
                    pv.Add(value);
                }
            }
        }

        public static PasswordCredential CreateCredential(string userName, string password)
        {
            return new PasswordCredential(AccountName, userName, password);
        }
    }
}
