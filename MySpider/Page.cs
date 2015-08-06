using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.XPath;

namespace MySpider
{
    enum ResourceType { Page, Resource }

    class Resource
    {
        public Resource(Uri resUri)
        {
            this.Url = resUri;
            this.Content = null;
            this.Loaded = false;
        }
        public Uri Url { get; protected set; }
        public byte[] Content { get; protected set; }
        public bool Loaded { get; protected set; }
        public ResourceType Type { get; protected set; }

        public void Load(byte[] content, string contentType)
        {
            if (this.Content == null)
            {
                this.Content = content;
                if (contentType.Contains("html"))
                {
                    this.Type = ResourceType.Page;
                }
                else
                {
                    this.Type = ResourceType.Resource;
                }
                this.Loaded = true;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }

    class Page : Resource
    {
        Page(Resource r)
            : base(r.Url)
        {
            if (!r.Loaded)
            {
                throw new InvalidOperationException();
            }
            this.Resources = new List<Resource>();
            MemoryStream ms = new MemoryStream(r.Content);
            StreamReader rd = new StreamReader(ms);
            this.HtmlContent = rd.ReadToEnd();
            Analyse();
        }

        public string HtmlContent { get; protected set; }
        public List<Resource> Resources { get; protected set; }

        protected void Analyse()
        {
            HtmlDocument hd = new HtmlDocument();
            hd.LoadHtml(this.HtmlContent);
            XPathNavigator xn = hd.CreateNavigator();

            while (xn.MoveToFollowing(XPathNodeType.Element))
            {
                if (xn.LocalName == "a")
                {
                    XPathNavigator xn_attr = xn.Clone();
                    if (xn_attr.MoveToAttribute("href", xn.NamespaceURI))
                    {
                        System.Diagnostics.Debug.Print("GotLink:{0}", xn_attr.Value);
                        this.Resources.Add(new Resource(new Uri(this.Url, xn_attr.Value)));
                    }
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Page)
            {
                return false;
            }
            else
            {
                return ((Page)obj).Url.Equals(this.Url);
            }
        }

        public override int GetHashCode()
        {
            return this.Url.GetHashCode();
        }
    }
}
