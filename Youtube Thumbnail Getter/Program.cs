using HtmlAgilityPack;
using OpenQA.Selenium;
using System;
using System.Threading.Tasks;

namespace Youtube_Thumbnail_Getter
{
    class Program
    {
        private const string BaseUrl = @"https://www.youtube.com/c/ChannelNameOrId/videos";
        private const string ChannelNameOrId = "";
        private const string ThumbnailImgTagXpath = "//img[contains(@src, 'hqdefault')]";

        static async Task Main(string[] args)
        {
            //var htmlWeb = new HtmlWeb();
            //var document = htmlWeb.Load(BaseUrl.Replace(nameof(ChannelNameOrId), ChannelNameOrId));
            //var tableRows = document.DocumentNode.SelectNodes(ThumbnailImgTagXpath);
            Console.WriteLine("Hello World!");

            IWebDriver driver = new PhantomJSDriver();
            driver.Navigate().GoToUrl("http://www.google.com/");


            IWebElement query = driver.FindElement(By.Name("q"));
            query.SendKeys("Cheese");
            query.Submit();

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until((d) => { return d.Title.ToLower().StartsWith("cheese"); });

            System.Console.WriteLine("Page title is: " + driver.Title);

            driver.Quit();
        }
    }
}
