
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

        public void FetchPage(string uri)
        {
            WebRequest req = HttpWebRequest.Create(uri);
            WebResponse resp = req.GetResponse();
            Stream s = resp.GetResponseStream();
            StreamReader tr = new StreamReader(s);
            string buf = tr.ReadToEnd();
            System.Diagnostics.Debug.Print("Content:{0}", buf);
            
            HtmlDocument hd = new HtmlDocument();
            hd.LoadHtml(buf);
            XPathNavigator xn = hd.CreateNavigator();
            //XPathNavigator xn = xd.CreateNavigator();
            while (xn.MoveToFollowing(XPathNodeType.Element))
            {
                if (xn.LocalName == "a")
                {
                    XPathNavigator xn2 = xn.Clone();
                    if(xn.MoveToAttribute("href", xn.NamespaceURI))
                    {
                        System.Diagnostics.Debug.Print("GotLink:{0}", xn.Value);
                        xn.MoveTo(xn2);
                    }
                }
            }
        }
    }
}
