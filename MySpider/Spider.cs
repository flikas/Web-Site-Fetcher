
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace MySpider
{
    class Spider : INotifyPropertyChanged
    {
        public static ObservableCollection<Spider> spiders = new ObservableCollection<Spider>();

        public static void Initialize(int count)
        {
            for (int i = 0; i < count; i++)
                spiders.Add(new Spider());
            StartAll();
        }

        public static void StartAll()
        {
            foreach (Spider sp in spiders)
            {
                sp.StartWork();
            }
        }

        public static void StopAll()
        {
            foreach (Spider sp in spiders)
            {
                sp.StopWork();
            }
        }

        public static int Count
        {
            get { return spiders.Count; }
            set
            {
                int delta = value - spiders.Count;
                if (delta > 0)
                {
                    for (int i = 0; i < delta; i++)
                    {
                        Spider sp = new Spider();
                        spiders.Add(sp);
                        sp.StartWork();
                    }
                }
                else
                {
                    for (int i = 0; i < -delta; i++)
                    {
                        spiders[i].StopWork();
                        spiders.RemoveAt(i);
                    }
                }
            }
        }
        
        #region Instance Members

        protected Spider() { }

        protected BackgroundWorker bgworker;

        public void StartWork()
        {
            if (bgworker == null)
            {
                bgworker = new BackgroundWorker();
                bgworker.WorkerReportsProgress = true;
                bgworker.DoWork += bgworker_DoWork;
                bgworker.ProgressChanged += bgworker_ProgressChanged;
                bgworker.RunWorkerCompleted += bgworker_RunWorkerCompleted;
                bgworker.RunWorkerAsync();
            }
        }

        public void StopWork()
        {
            bgworker.CancelAsync();
        }

        struct SpiderWorkProgress
        {
            public Resource Resource;
            public Exception Exception;
            public long CurrentRead;
            public long ContentLength;
            public SpiderWorkProgress(Resource res) { Resource = res; Exception = null; CurrentRead = 0; ContentLength = 0; }
        }

        void bgworker_DoWork(object sender, DoWorkEventArgs e)
        {
            Resource res;
            byte[] buf;
            WebResponse resp = null;
            const int bufsize = 1024;

            Debug.WriteLine("Spider work begin.");
            while ((res = Site.Instance.Dequeue()) != null && !bgworker.CancellationPending)
            {
                SpiderWorkProgress swp = new SpiderWorkProgress(res);
                bgworker.ReportProgress(0, swp);
                try
                {
                    HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(res.Url);
                    if (res.Parent != null) req.Referer = res.Parent.Url.ToString();
                    req.UserAgent = "Mozilla/5.0 (Windows NT 6.0; U; en; rv:1.8.1)";
                    resp = req.GetResponse();
                    Stream s = resp.GetResponseStream();
                    MemoryStream ms = new MemoryStream();
                    using (BinaryReader tr = new BinaryReader(s))
                    {
                        do
                        {
                            buf = tr.ReadBytes(bufsize);
                            ms.Write(buf, 0, buf.Length);
                            swp.ContentLength = resp.ContentLength;
                            swp.CurrentRead = ms.Position;
                            int percent = swp.ContentLength > 0 ? (int)(swp.CurrentRead * 100 / swp.ContentLength) : 0;
                            bgworker.ReportProgress(percent, swp);
                        } while (buf.Length > 0);
                    }
                    res.Load(ms.ToArray(), resp.ContentType);
                }
                catch (NotSupportedException) { }
                catch (Exception ex)
                {
                    swp.Exception = ex;
                    bgworker.ReportProgress(0, swp);
                }
                finally
                {
                    if (resp != null) resp.Close();
                }
                System.Threading.Thread.Sleep(10000);
            }
        }

        void bgworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            SpiderWorkProgress swp = (SpiderWorkProgress)e.UserState;
            this.CurrentUri = swp.Resource.Url.ToString();
            this.RequestUri = swp.Resource.Parent != null ? swp.Resource.Parent.Url.ToString() : "";
            this.Percentage = e.ProgressPercentage;

            if (swp.Exception != null)
            {
                Debug.WriteLine("Spider read failed: {0}, {1}", swp.Exception.ToString(), swp.Resource.Url);
                Site.Instance.NewResource(swp.Resource);
            }
            else
            {
                //Debug.WriteLine("Spider work progress:: {0}/{1} {2}%, {3}", swp.CurrentRead, swp.ContentLength, this.Percentage, swp.Resource.Url);
            }
        }

        void bgworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.bgworker = null;
            this.Percentage = 0;
            this.CurrentUri = "";
            this.RequestUri = "";
            Debug.WriteLine("Spider work done.");
        }

        private string currentUri;
        public string CurrentUri
        {
            get { return currentUri; }
            protected set { currentUri = value; NotifyPropertyChanged(); }
        }

        private string requestUri;
        public string RequestUri
        {
            get { return requestUri; }
            protected set { requestUri = value; NotifyPropertyChanged(); }
        }

        private int percentage;
        public int Percentage
        {
            get { return percentage; }
            protected set { percentage = value; NotifyPropertyChanged(); }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
