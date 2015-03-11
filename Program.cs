using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace ConsoleApplication1
{
    class Program
    {
        static WebBrowser webBrowser1 = new WebBrowser();
        static bool endFlag = false;
        static Thread loopThread;
        //static Thread loopThread2;
        static string url;
        static string number_of_friends = "";
        static string latest_post = "";
        static string join_date = "";
        static string wanted_post;
        //static string outfilename = "D:\\test.html";

        [STAThread]
        static void Main(string[] args)
        {
            //url = Console.ReadLine();
            wanted_post = args[1];
            url = args[0];
            //Console.Write("Url got: ");
            //Console.WriteLine(url);
            if(url.Contains("facebook")) runBrowserThread_facebook(url);
            else runBrowserThread_renren(url);
        }

        private static void runBrowserThread_facebook(string url)
        {
            var th = new Thread(() =>
            {
                webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(facebook_complete_handler);
                webBrowser1.Navigate(url);
                Application.Run();
            });
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
        }

        private static void runBrowserThread_renren(string url)
        {
            var th = new Thread(() =>
            {
                webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(renren_complete_handler);
                string newurl = url.Replace(":", "%3A");
                newurl = newurl.Replace("/", "%2F");
                newurl = "http://www.renren.com/SysHome.do?origURL=" + newurl;
                webBrowser1.Navigate(newurl);
                Application.Run();
            });
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
        }

        static void renren_complete_handler(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            HtmlElement login_form = webBrowser1.Document.GetElementById("loginForm");//look for the login form
            if(login_form != null)//do log in
            {
                HtmlElementCollection login_elements = webBrowser1.Document.GetElementsByTagName("input");
                foreach (HtmlElement login_element in login_elements)
                {
                    if (login_element.Name == "email") //look for the email field
                    {
                        login_element.SetAttribute("value", "yetanotheranacct@yahoo.com"); //set the password
                    }
                    if (login_element.Name == "password") //look for the password field
                    {
                        login_element.SetAttribute("value", "fwu1cisczf3f4"); //set the password
                    }
                }
                login_form.InvokeMember("submit");
            }
            else
            {
                if(webBrowser1.Url.ToString() == url)
                {
                    HtmlDocument document = webBrowser1.Document;
                    string myhtml = "";
                    HtmlElementCollection hc = document.GetElementsByTagName("html");
                    foreach (HtmlElement he in hc)
                    {
                        myhtml += he.InnerHtml;
                    }
                    string outfilename = "D:\\test.html";
                    System.IO.File.WriteAllText(outfilename, myhtml);
                    int temp = myhtml.IndexOf("<H4><SPAN>");
                    int temp2;
                    if (temp == -1)
                    {
                        number_of_friends = "0";
                    }
                    else
                    {
                        temp2 = myhtml.IndexOf("</SPAN>", temp);
                        number_of_friends = myhtml.Substring(temp + 10, temp2 - temp - 10);
                    }
                    
                    temp = myhtml.IndexOf("tlNavData");
                    temp2 = myhtml.IndexOf(";", temp);
                    join_date = myhtml.Substring(temp + 12, temp2 - temp -10);
                    temp = join_date.LastIndexOf("year");
                    string year = join_date.Substring(temp+6,4);
                    temp = join_date.LastIndexOf("[");
                    temp2 = join_date.LastIndexOf("]");
                    string month = join_date.Substring(temp+1, temp2 - temp - 3);
                    if(month.Contains(","))
                    {
                        temp = month.LastIndexOf(",");
                        month = month.Substring(temp + 1);
                    }

                    join_date = year + "-" + month;

                    foreach(HtmlElement link in webBrowser1.Document.GetElementsByTagName("A"))
                    {
                        if(link.GetAttribute("href").Contains("http://status.renren.com/status/"))
                        {
                            webBrowser1.Invoke(new Action(() =>
                            {
                                webBrowser1.Navigate(link.GetAttribute("href"));
                            }));
                        }
                    }
                }
                else
                {
                    HtmlDocument document = webBrowser1.Document;
                    string myhtml = "";
                    HtmlElementCollection hc = document.GetElementsByTagName("html");
                    foreach (HtmlElement he in hc)
                    {
                        myhtml += he.InnerHtml;
                    }

                    foreach (HtmlElement item in webBrowser1.Document.GetElementsByTagName("LI"))
                    {
                        if(item.GetAttribute("className") == "ugc-list-item")
                        {
                            string temp_html = item.InnerHtml;
                            string outfilename = "D:\\test.html";
                            System.IO.File.WriteAllText(outfilename, temp_html);

                            int temp = temp_html.IndexOf("<P>");
                            int temp2 = temp_html.IndexOf("</P>", temp);
                            latest_post = temp_html.Substring(temp + 3, temp2 - temp - 3);
                            bool has_emoticon = latest_post.Contains("<IMG");
                            while(has_emoticon)
                            {
                                temp = latest_post.IndexOf("<IMG ");
                                temp2 = latest_post.IndexOf(">",temp);
                                latest_post = latest_post.Remove(temp,temp2-temp+1);
                                has_emoticon = latest_post.Contains("<IMG ");
                            }
                            bool has_link = latest_post.Contains("<A ");
                            while (has_link)
                            {
                                temp = latest_post.IndexOf("<A ");
                                temp2 = latest_post.IndexOf("</A>", temp);
                                latest_post = latest_post.Remove(temp, temp2 - temp + 4);
                                has_link = latest_post.Contains("<A ");
                            }
                            break;
                        }
                    }
                }

                if(number_of_friends != "" && latest_post != "" && join_date != "")
                {
                    //` " \ ( ) | $ # & * ; ' < >
                    int index = latest_post.IndexOf("&amp;");
                    while (index != -1)
                    {
                        latest_post = latest_post.Remove(index, 5);
                        index = latest_post.IndexOf("&amp;");
                    }

                    index = latest_post.IndexOf("&lt;");
                    while (index != -1)
                    {
                        latest_post = latest_post.Remove(index, 4);
                        index = latest_post.IndexOf("&lt;");
                    }

                    index = latest_post.IndexOf("&gt;");
                    while (index != -1)
                    {
                        latest_post = latest_post.Remove(index, 4);
                        index = latest_post.IndexOf("&gt;");
                    }

                    index = latest_post.IndexOf(' ');
                    while(index != -1)
                    {
                        latest_post = latest_post.Remove(index,1);
                        index = latest_post.IndexOf(' ');
                    }

                    index = latest_post.IndexOf(':');
                    while (index != -1)
                    {
                        latest_post = latest_post.Remove(index, 1);
                        index = latest_post.IndexOf(':');
                    }

                    index = latest_post.IndexOf('?');
                    while (index != -1)
                    {
                        latest_post = latest_post.Remove(index, 1);
                        index = latest_post.IndexOf('?');
                    }

                    index = latest_post.IndexOf('`');
                    while (index != -1)
                    {
                        latest_post = latest_post.Remove(index, 1);
                        index = latest_post.IndexOf('`');
                    }

                    index = latest_post.IndexOf('"');
                    while (index != -1)
                    {
                        latest_post = latest_post.Remove(index, 1);
                        index = latest_post.IndexOf('"');
                    }

                    index = latest_post.IndexOf('\\');
                    while (index != -1)
                    {
                        latest_post = latest_post.Remove(index, 1);
                        index = latest_post.IndexOf('\\');
                    }

                    index = latest_post.IndexOf('(');
                    while (index != -1)
                    {
                        latest_post = latest_post.Remove(index, 1);
                        index = latest_post.IndexOf('(');
                    }

                    index = latest_post.IndexOf(')');
                    while (index != -1)
                    {
                        latest_post = latest_post.Remove(index, 1);
                        index = latest_post.IndexOf(')');
                    }

                    index = latest_post.IndexOf('|');
                    while (index != -1)
                    {
                        latest_post = latest_post.Remove(index, 1);
                        index = latest_post.IndexOf('|');
                    }

                    index = latest_post.IndexOf('$');
                    while (index != -1)
                    {
                        latest_post = latest_post.Remove(index, 1);
                        index = latest_post.IndexOf('$');
                    }

                    index = latest_post.IndexOf('#');
                    while (index != -1)
                    {
                        latest_post = latest_post.Remove(index, 1);
                        index = latest_post.IndexOf('#');
                    }

                    index = latest_post.IndexOf('*');
                    while (index != -1)
                    {
                        latest_post = latest_post.Remove(index, 1);
                        index = latest_post.IndexOf('*');
                    }

                    index = latest_post.IndexOf(';');
                    while (index != -1)
                    {
                        latest_post = latest_post.Remove(index, 1);
                        index = latest_post.IndexOf(';');
                    }

                    index = latest_post.IndexOf('\'');
                    while (index != -1)
                    {
                        latest_post = latest_post.Remove(index, 1);
                        index = latest_post.IndexOf('\'');
                    }

                    //Console.WriteLine("Has {0} friends",number_of_friends);
                    //Console.WriteLine("Latest post is: '{0}'", latest_post);
                    //Console.WriteLine("Join date is: {0}", join_date);
                    if(wanted_post == latest_post)
                    {
                        Console.WriteLine("YES");
                    }
                    else
                    {
                        Console.WriteLine("NO");
                    }
                    Application.Exit();
                }
            }
            
        }

        static void facebook_complete_handler(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            HtmlElement login_form = webBrowser1.Document.GetElementById("login_form");//look for the form
            if (login_form != null)
            {
                HtmlElementCollection login_elements = webBrowser1.Document.GetElementsByTagName("input");
                foreach (HtmlElement login_element in login_elements)
                {
                    if (login_element.Name == "email") //look for the email field
                    {
                        login_element.SetAttribute("value", "yyksgj@yahoo.com"); //set the password
                    }
                    if (login_element.Name == "pass") //look for the password field
                    {
                        login_element.SetAttribute("value", "r4GDLaFUy9"); //set the password
                    }
                }
                login_form.InvokeMember("submit");
            }
            else
            {
                //if (webBrowser1.Url.ToString() == url)
                //{
                    if (loopThread == null)
                    {
                        loopThread = new Thread(loopScroll);
                        loopThread.Start();
                    }
                //}
                /*else if(webBrowser1.Url.ToString() == (url + "\friends"))
                {
                    if (loopThread2 == null)
                    {
                        loopThread2 = new Thread(loopScroll2);
                        loopThread2.Start();
                    }
                }*/
            }

            /*HtmlDocument document = webBrowser1.Document;
            string myhtml = "";
            HtmlElementCollection hc = document.GetElementsByTagName("html");
            foreach (HtmlElement he in hc)
            {
                myhtml += he.InnerHtml;
            }
            string outfilename = "D:\\test.html";
            System.IO.File.WriteAllText(outfilename, myhtml);*/

            //Application.ExitThread();   // Stops the thread
        }

        static private void loopScroll()
        {
            while (endFlag != true)
            {
                Thread.Sleep(300);
                //HtmlDocument document;// = webBrowser1.Document;
                //HtmlElement body;// = document.Body;
                webBrowser1.Invoke(new Action(() =>
                {
                    HtmlElement body = webBrowser1.Document.Body;
                    body.ScrollIntoView(false);

                    HtmlDocument document = webBrowser1.Document;
                    HtmlElementCollection hc = document.GetElementsByTagName("html");

                    foreach(HtmlElement item in webBrowser1.Document.GetElementsByTagName("a"))
                    {
                        if(item.InnerHtml != null)
                        {
                            if (item.InnerHtml.Contains("Show Older Stories")) item.InvokeMember("click");
                        }
                    }
                    foreach (HtmlElement he in hc)
                    {
                        if (he.InnerHtml.Contains("https://fbstatic-a.akamaihd.net/rsrc.php/v2/yg/r/A2sk6_j-Y1m.png"))
                        {
                            endFlag = true;
                        }
                    }
                }));
            }
            webBrowser1.Invoke(new Action(() =>
            {
                string myhtml = "";
                //HtmlElementCollection hc = webBrowser1.Document.GetElementsByTagName("html");
                //foreach (HtmlElement he in hc)
                //{
                //    myhtml += he.InnerHtml;
                //}
                foreach (HtmlElement unit in webBrowser1.Document.GetElementsByTagName("div"))
                {
                    if (unit.GetAttribute("className") == "_4-u2 mbm _5jmm _5pat _5v3q")
                    {
                        myhtml = unit.InnerHtml;
                        //Console.WriteLine("found");
                        int temp = myhtml.IndexOf("<p>");
                        int temp2 = myhtml.IndexOf("</p>");
                        if(latest_post == "")
                        {
                            if (temp == -1) latest_post = "";
                            else
                            {
                                latest_post = myhtml.Substring(temp + 3, temp2 - temp - 3);
                            }
                            //Console.WriteLine(latest_post);
                        }
                        if (myhtml.Contains("<abbr title=") && (!myhtml.Contains("<div class=\"_6q0 _1n8r\"><div class=\"_5m-\"><i class=\"_5n0\"></i></div></div>")))
                        {
                           string month;
                           string year;
                           temp = myhtml.IndexOf("<abbr title=");
                           temp = myhtml.IndexOf(",", temp);
                           temp2 = myhtml.IndexOf(",", temp + 1);
                           month = myhtml.Substring(temp + 2, temp2 - temp - 2);
                           temp = temp2;
                           temp2 = myhtml.IndexOf("at", temp);
                           year = myhtml.Substring(temp + 1, temp2 - temp - 1);
                           join_date = month + "," + year;
                        }
                    }
                }
                //endFlag = false;
                //` " \ ( ) | $ # & * ; ' < >

                int index = latest_post.IndexOf("<br> ");
                while(index != -1)
                {
                    latest_post = latest_post.Remove(index, 5);
                    index = latest_post.IndexOf("<br> ");
                }

                index = latest_post.IndexOf("&amp;");
                while (index != -1)
                {
                    latest_post = latest_post.Remove(index, 5);
                    index = latest_post.IndexOf("&amp;");
                }

                index = latest_post.IndexOf("&lt;");
                while (index != -1)
                {
                    latest_post = latest_post.Remove(index, 4);
                    index = latest_post.IndexOf("&lt;");
                }

                index = latest_post.IndexOf("&gt;");
                while (index != -1)
                {
                    latest_post = latest_post.Remove(index, 4);
                    index = latest_post.IndexOf("&gt;");
                }

                index = latest_post.IndexOf(' ');
                while (index != -1)
                {
                    latest_post = latest_post.Remove(index, 1);
                    index = latest_post.IndexOf(' ');
                }

                index = latest_post.IndexOf('`');
                while (index != -1)
                {
                    latest_post = latest_post.Remove(index, 1);
                    index = latest_post.IndexOf('`');
                }

                index = latest_post.IndexOf(':');
                while (index != -1)
                {
                    latest_post = latest_post.Remove(index, 1);
                    index = latest_post.IndexOf(':');
                }

                index = latest_post.IndexOf('?');
                while (index != -1)
                {
                    latest_post = latest_post.Remove(index, 1);
                    index = latest_post.IndexOf('?');
                }

                index = latest_post.IndexOf('"');
                while (index != -1)
                {
                    latest_post = latest_post.Remove(index, 1);
                    index = latest_post.IndexOf('"');
                }

                index = latest_post.IndexOf('\\');
                while (index != -1)
                {
                    latest_post = latest_post.Remove(index, 1);
                    index = latest_post.IndexOf('\\');
                }

                index = latest_post.IndexOf('(');
                while (index != -1)
                {
                    latest_post = latest_post.Remove(index, 1);
                    index = latest_post.IndexOf('(');
                }

                index = latest_post.IndexOf(')');
                while (index != -1)
                {
                    latest_post = latest_post.Remove(index, 1);
                    index = latest_post.IndexOf(')');
                }

                index = latest_post.IndexOf('|');
                while (index != -1)
                {
                    latest_post = latest_post.Remove(index, 1);
                    index = latest_post.IndexOf('|');
                }

                index = latest_post.IndexOf('$');
                while (index != -1)
                {
                    latest_post = latest_post.Remove(index, 1);
                    index = latest_post.IndexOf('$');
                }

                index = latest_post.IndexOf('#');
                while (index != -1)
                {
                    latest_post = latest_post.Remove(index, 1);
                    index = latest_post.IndexOf('#');
                }

                index = latest_post.IndexOf('*');
                while (index != -1)
                {
                    latest_post = latest_post.Remove(index, 1);
                    index = latest_post.IndexOf('*');
                }

                index = latest_post.IndexOf(';');
                while (index != -1)
                {
                    latest_post = latest_post.Remove(index, 1);
                    index = latest_post.IndexOf(';');
                }

                index = latest_post.IndexOf('\'');
                while (index != -1)
                {
                    latest_post = latest_post.Remove(index, 1);
                    index = latest_post.IndexOf('\'');
                }

                if (wanted_post == latest_post)
                {
                    Console.WriteLine("YES");
                }
                else
                {
                  Console.WriteLine("NO");
                }
                //Console.WriteLine("Program ends");
                Application.Exit();
                
                //Console.WriteLine("Has {0} friends", number_of_friends);
                //Console.WriteLine("Latest post is: '{0}'", latest_post);
                //Console.WriteLine("Join date is: {0}", join_date);
            }));
        }

        /*static private void loopScroll2()
        {
            webBrowser1.Invoke(new Action(() =>
            {
                HtmlDocument document = webBrowser1.Document;
                string myhtml = "";
                HtmlElementCollection hc = document.GetElementsByTagName("html");
                foreach (HtmlElement he in hc)
                {
                    myhtml += he.InnerHtml;
                }
                string outfilename = "D:\\test.html";
                System.IO.File.WriteAllText(outfilename, myhtml);
            }));
        }*/

    }
}
