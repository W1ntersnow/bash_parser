using System;
using System.Net;
using System.Threading;
using WebApplication2.Models;
using HtmlAgilityPack;
using WebApplication2.DAL;
using System.Collections.Generic;


namespace WebApplication2.HtmlParser
{
    public class Locker {
        public Locker() {
        }
    }
    public abstract class AbstractNetProvider
    {
        protected string url;
        protected string htmlContent;
        protected void GetHtmlContent()
        {
            WebClient client = new WebClient();
            this.htmlContent = client.DownloadString(this.url);
        }

        protected void GetHtmlContent(string page)
        {
            WebClient client = new WebClient();
            this.htmlContent = client.DownloadString(this.url+page);
        }
    }
    public class Dispatcher : AbstractNetProvider
    {
        private int lastPage;
        private int currentPage;
        private int endPage;
        private int count;
        private object locker;
        private Thread[] threadsArray; 
        private PostProducer[] produsersArray;

        public Dispatcher()
        {
            this.locker = new object();
            this.endPage = 1;
            this.url = "http://bash.im/";
            this.count = Environment.ProcessorCount;
            this.GetHtmlContent();
            this.GetData();
            this.threadsArray = new Thread[this.count];
            this.produsersArray = new PostProducer[this.count];
        }

        public int getCurrentPage() {
            lock (this.locker)
            {
                int page = this.currentPage;
                if (this.currentPage == this.endPage)
                    return -1;
                this.currentPage--;
                return page;
            }
        }

        public void GetData()
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(this.htmlContent);
            string html = ""; 
            html = doc.DocumentNode.SelectSingleNode("//div[@id='body']").InnerHtml;
            doc.LoadHtml(html);
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//input[@type='text']")) {
                this.lastPage = node.GetAttributeValue("value", node.GetAttributeValue("max", 0));
                this.currentPage = this.lastPage;
            }
        }

        public string RunThreads() {
            for (int i = 0; i < this.count; i++)
            {
                produsersArray[i] = new PostProducer(this);
                threadsArray[i] = new Thread(new ThreadStart(produsersArray[i].ThreadRun));
            }
            for (int i = 0; i < this.count; i++)
            {
                threadsArray[i].Start();
            }
            for (int i = 0; i < this.count; i++)
            {
                threadsArray[i].Join();
            }
            int runnedCount = threadsArray.Length;
            while (true) {
                for(int i = 0; i < this.count; i++) {
                    if (!threadsArray[i].IsAlive)
                    {
                        runnedCount--;
                    }
                }
                if (runnedCount == 0) {
                    break;
                }
                runnedCount = threadsArray.Length;
            }
            return Convert.ToString(threadsArray.Length) + ' ' + Convert.ToString(runnedCount);
        }
    }

    public class PostProducer : AbstractNetProvider
    {
        private PostContext ctx;
        private HtmlDocument doc;
        private List<BashPost> objs;
        private Dispatcher disp_store;
        private int currentPage;
        public PostProducer(Dispatcher disp)
        {
            this.ctx = new PostContext();
            this.url = "http://bash.im/index/";
            this.disp_store = disp;
            this.doc = new HtmlDocument();
        }

        public void MoveToDb(BashPost post)
        {
            this.ctx.Add(post);
        }

        public void CommitMoves()
        {
            this.ctx.Commit();
        }

        public void ThreadRun()
        {
            while (true) {
                this.currentPage = this.disp_store.getCurrentPage();
                if (this.currentPage == -1)
                {
                    break;
                }
                this.GetHtmlContent(Convert.ToString(this.currentPage));
                this.GetBodyContent();
                this.GatherPosts();
                this.MoveObjToDb();
            }
        }

        private void GetBodyContent() {
            doc.LoadHtml(this.htmlContent);
            this.htmlContent = doc.DocumentNode.SelectSingleNode("//div[@id='body']").InnerHtml;
        }

        public static void RemoveQuotesBreaksSpaces(string str)
        {
            str.Replace("\"", " ");
            str.Replace("\n", " ");
            str.Replace("\t", " ");
            str.Replace("  ", " ");
        }
        private void GatherPosts() {
            doc.LoadHtml(this.htmlContent);
            objs = new List<BashPost>();
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class='quote']"))
            {
                HtmlNode actionsNode = node.SelectSingleNode(".//div[@class='actions']");
                HtmlNode textNode = node.SelectSingleNode(".//div[@class='text']");
                if (actionsNode != null && textNode != null)
                {
                    string rating = actionsNode.SelectSingleNode(".//span[@class='rating']").InnerText;
                    string dt = actionsNode.SelectSingleNode(".//span[@class='date']").InnerText;
                    string title = actionsNode.SelectSingleNode(".//a[@class='id']").InnerText;
                    string textData = textNode.InnerText;
                    int value;
                    if (!int.TryParse(rating, out value))
                    {
                        rating = "0";
                    }
                    objs.Add(new BashPost(textData, title, dt, rating));
                }
                
            }
        }

        private void MoveObjToDb()
        {
            foreach (BashPost post in objs)
                this.MoveToDb(post);
            objs.Clear();
            this.CommitMoves();
        }
    }
}