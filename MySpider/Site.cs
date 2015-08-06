using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySpider
{
    class Page
    {
        internal Page()
        {
            LinkedPages = new List<Page>();
        }
        public List<Page> LinkedPages { get; private set; }
        public Uri Url { get; set; }
    }
    class Site : Page
    {
        public Site(Uri rootPage) : base() { }

    }
}
