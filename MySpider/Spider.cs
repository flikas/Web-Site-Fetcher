
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace MySpider
{
    class Spider
    {
        public Spider() { }

        public void FetchPage(Resource res)
        {
            WebRequest req = HttpWebRequest.Create(res.Url);
            using (WebResponse resp = req.GetResponse())
            {
                Stream s = resp.GetResponseStream();
                BinaryReader tr = new BinaryReader(s);
                byte[] buf = new byte[s.Length];
                tr.Read(buf, 0, (int)s.Length);
                res.Load(buf, resp.ContentType);
            }
        }
    }
}
