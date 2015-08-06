using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySpider
{
    class Task
    {
        public Task(Uri destUri, Uri requestUri)
        {
            this.DestUri = destUri;
            this.RequestUri = requestUri;
            if (DestUri.IsBaseOf(RequestUri))
                Relativity = 1;
            else
                Relativity = 0;
        }
        
        public Uri DestUri { get; private set; }
        public Uri RequestUri { get; private set; }
        public int Relativity { get; private set; }
    }
}
