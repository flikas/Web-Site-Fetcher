using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySpider
{
    class Site
    {
        private Site() { }

        public static Site Instance { get; private set; }
        public Uri RootUri { get; private set; }
        public HashSet<Page> Pages { get; private set; }
        public Page RootPage { get; private set; }

        protected HashSet<Uri> UriSet { get; private set; }
        protected Queue<Resource> PrimaryQueue;
        protected Queue<Resource> SecondaryQueue;
        protected int processing = 0;

        public static void Initialize(string rootUri)
        {
            Instance = new Site();
            Instance.Pages = new HashSet<Page>();
            Instance.UriSet = new HashSet<Uri>();
            Instance.PrimaryQueue = new Queue<Resource>();
            Instance.SecondaryQueue = new Queue<Resource>();
            Instance.RootUri = new Uri(rootUri);
            Instance.NewResource(new Resource(Instance.RootUri, null));
        }

        public void NewResource(Resource res)
        {
            if (UriSet.Contains(res.Url)) return;
            UriSet.Add(res.Url);

            lock (this)
            {
                if (res.Parent != null && res.Parent.Url.IsBaseOf(res.Url))
                {
                    PrimaryQueue.Enqueue(res);
                }
                else
                {
                    SecondaryQueue.Enqueue(res);
                }
            }
            res.LoadOK += res_LoadOK;
        }

        public Resource Dequeue()
        {
            Resource res = null;

            while (res == null)
            {
                lock (this)
                {
                    try
                    {
                        res = this.PrimaryQueue.Dequeue();
                    }
                    catch (InvalidOperationException)
                    {
                        try
                        {
                            res = this.SecondaryQueue.Dequeue();
                        }
                        catch (InvalidOperationException) { }
                    }
                }
                System.Threading.Thread.Sleep(500);
            }

            return res;
        }

        void res_LoadOK(object sender, EventArgs e)
        {
            Resource res = (Resource)sender;

            lock (this)
            {
                if (res.Type == ResourceType.Page)
                {
                    Page p = new Page(res);
                    if (this.Pages.Add(p))
                    {
                        if (res.Parent == null) this.RootPage = p;
                        foreach (Resource r in p.Resources)
                        {
                            this.NewResource(r);
                        }
                    }
                }
            }
            res.SaveToDisk();
        }
    }
}
