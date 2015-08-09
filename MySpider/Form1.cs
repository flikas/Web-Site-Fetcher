using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MySpider
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            MySpider.Site.Initialize("http://www.kanunu8.com/book2/10734/index.html");
            Spider.Initialize(1);
            InitListView();
            Spider.spiders.CollectionChanged += spiders_CollectionChanged;
        }

        void InitListView()
        {
            this.listView1.Items.Clear();
            foreach (Spider sp in Spider.spiders)
            {
                sp.PropertyChanged += sp_PropertyChanged;
                ListViewItem lvi = this.listView1.Items.Add(sp.Percentage.ToString());
                lvi.SubItems.Add(sp.CurrentUri != null ? sp.CurrentUri.ToString() : "");
            }
        }

        void spiders_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            InitListView();
        }

        void sp_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Spider sp = (Spider)sender;
            int idx = Spider.spiders.IndexOf(sp);
            this.listView1.Items[idx].SubItems[0].Text = sp.Percentage.ToString();
            this.listView1.Items[idx].SubItems[1].Text = sp.CurrentUri != null ? sp.CurrentUri.ToString() : "";
            this.listView1.Refresh();
        }
    }
}
