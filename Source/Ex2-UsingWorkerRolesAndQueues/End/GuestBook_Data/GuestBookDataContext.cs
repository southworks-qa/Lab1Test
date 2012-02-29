using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuestBook_Data
{
    public class GuestBookDataContext
        : Microsoft.WindowsAzure.StorageClient.TableServiceContext
    {
        public GuestBookDataContext(string baseAddress, Microsoft.WindowsAzure.StorageCredentials credentials)
            : base(baseAddress, credentials)
        { }

        public IQueryable<GuestBookEntry> GuestBookEntry
        {
            get
            {
                return this.CreateQuery<GuestBookEntry>("GuestBookEntry");
            }
        }
    }
}
