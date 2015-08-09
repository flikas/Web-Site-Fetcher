using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.XPath;

namespace MySpider
{
    enum ResourceType { Page, Resource }

    class Resource
    {
        public Resource(Uri resUri, Page parent)
        {
            this.Url = resUri;
            this.Content = null;
            this.Loaded = false;
            this.Parent = parent;
        }
        public Uri Url { get; protected set; }
        public byte[] Content { get; protected set; }
        public string ContentType { get; protected set; }
        public bool Loaded { get; protected set; }
        public ResourceType Type { get; protected set; }
        public Page Parent { get; protected set; }

        public event EventHandler LoadOK;

        public void Load(byte[] content, string contentType)
        {
            if (this.Content == null)
            {
                this.Content = content;
                this.ContentType = contentType;
                if (contentType.Contains("html"))
                {
                    this.Type = ResourceType.Page;
                }
                else
                {
                    this.Type = ResourceType.Resource;
                }
                this.Loaded = true;
                this.LoadOK(this, new EventArgs());
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void SaveToDisk()
        {
            Debug.WriteLine("SAVE:: {0}, {1}, {2}", this.ContentType, this.Content.Length, this.Url.ToString());
        }
    }

    class Page : Resource
    {
        public Page(Resource r)
            : base(r.Url, r.Parent)
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
                        //Debug.Print("GotLink:{0}", xn_attr.Value);
                        this.Resources.Add(new Resource(new Uri(this.Url, xn_attr.Value), this));
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
