using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BaiduShopTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private static bool isFinish = false;
        private static int loadingStatus = 0;
        private void Form1_Load(object sender, EventArgs e)
        {

            loadingStatus = 1;
            webBrowser1.Dock = DockStyle.Fill;
            webBrowser1.ScriptErrorsSuppressed = true;
            while (loadingStatus != 0)
            {
                isFinish = false;
                webBrowser1.Navigate("https://zhidao.baidu.com/shop/lottery");
                while (!isFinish)
                {
                    Application.DoEvents(); // 等待本次加载完毕才执行下次循环. 
                }
            }
            System.Threading.Thread.Sleep(5000);
            Application.Exit();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                if (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
                    return;

                HtmlElementCollection InsideEle = webBrowser1.Document.GetElementsByTagName("div");
                HtmlElement DivMachineControl = null;
                if (InsideEle != null && InsideEle.Count > 0)
                {
                    foreach (HtmlElement div in InsideEle)
                    {
                        if (div.GetAttribute("className").Equals("control-btn-collection one-try-item"))
                        {
                            DivMachineControl = div;
                            break;
                        }
                    }
                }

                HtmlElement clickEle = null;
                string freeTips = string.Empty;
                HtmlElementCollection elems = DivMachineControl.GetElementsByTagName("p");
                HtmlElementCollection spans = DivMachineControl.GetElementsByTagName("span");
                if (spans[0].InnerText.Contains("抽奖"))
                {
                    clickEle = spans[0];
                }

                bool isFree = false;
                bool hasStopFlag=false;

                foreach (HtmlElement elem in elems)
                {
                    if (elem.GetAttribute("className").Equals("action-coins"))
                    {
                        if (elem.Style.Equals("visibility: visible;"))
                        {
                            hasStopFlag=true;
                        }
                    }
                    if (elem.GetAttribute("className").Equals("action-ps"))
                    {
                        if (elem.Style.Equals("visibility: visible;"))
                        {
                            isFree = true;
                            freeTips = elem.InnerText;
                            break;
                        }
                        if (elem.InnerText == "??分钟后免费")
                        {
                            if (hasStopFlag)
                            {
                                isFree = false;
                                break;
                            }
                            else
                            {
                                isFree = true;
                                freeTips = elem.InnerText;
                                break;
                            }
                        }
                    }
                }
                if (clickEle != null)
                {
                    if (isFree)
                    {
                        WriteLogToFile(DivMachineControl.InnerHtml);
                        if (!string.IsNullOrEmpty(freeTips))
                        {
                            var waitTimeStr = freeTips.Replace("后免费", "");
                            if (waitTimeStr == "NaN" || waitTimeStr == "??分钟")
                            {
                                clickEle.InvokeMember("click");
                                System.Threading.Thread.Sleep(5000);

                            }
                            else if (waitTimeStr.Contains("分"))
                            {
                                int waitTime = int.Parse(waitTimeStr.Substring(0, waitTimeStr.IndexOf("分钟"))) * 60 + int.Parse(waitTimeStr.Replace("秒", "").Substring(waitTimeStr.IndexOf("分钟") + 2));
                                if (waitTime > 0)
                                {
                                    System.Threading.Thread.Sleep(waitTime * 1000);
                                    clickEle.InvokeMember("click");
                                }
                            }
                        }
                        isFinish = true;
                        loadingStatus = 2;
                        return;
                    }
                    else
                    {
                        WriteLogToFile(DateTime.Now.ToShortDateString() + "今日已领完，明天再来！");
                    }
                }
                else
                {
                    WriteLogToFile(DateTime.Now.ToShortDateString() + "网页有所变化，找不到点击入口了！");
                }
                isFinish = true;
                loadingStatus = 0;
                return;
            }
            catch (Exception ex)
            {
                WriteLogToFile("主程序出错，需要修改调试！！！" + ex.ToString());
                webBrowser1.Hide();
            }
        }

        public static void WriteLogToFile(string msg)
        {
            string filePath = "C:\\Users\\Administrator\\Documents\\Visual Studio 2013\\Projects\\BaiduShop.txt";
            try
            {
                using (StreamWriter sw = new StreamWriter(filePath, true))
                {
                    sw.WriteLine(msg);
                    sw.Flush();
                    sw.Close();
                    sw.Dispose();
                }
            }
            catch (IOException e)
            {
                using (StreamWriter sw = new StreamWriter(filePath, true))
                {
                    sw.WriteLine("异常：" + e.Message);
                    sw.WriteLine("时间：" + DateTime.Now.ToString("yyy-MM-dd HH:mm:ss"));
                    sw.WriteLine("**************************************************");
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                    sw.Dispose();
                }
            }
        }
    }
}
