using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MySpider
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Spider sp = new Spider();
            sp.FetchPage("http://www.kanunu8.com/");
            Application.Run(new Form1());
        }
    }
}
