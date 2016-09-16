using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ScratchConnection;
using System.Threading;
using System.Globalization;

namespace BoardManagerTest
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#if DEBUG
            //Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");     // 強制的に英語にする
            //Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("zh");     // 強制的に中国語にする
            //Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ko");     // 強制的に韓国語にする
#endif
            Application.Run(new Form1());
        }
    }
}
