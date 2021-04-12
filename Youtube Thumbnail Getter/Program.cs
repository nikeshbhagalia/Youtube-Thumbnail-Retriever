using OpenQA.Selenium;
using System;

namespace Youtube_Thumbnail_Getter
{
    class Program
    {
        private const string BaseUrl = @"https://www.youtube.com/c/ChannelNameOrId/videos";
        private const string ChannelNameOrId = "";
        private const string ThumbnailImgTagXpath = @"//img[contains(@src, 'hqdefault')]";

        static void Main(string[] args)
        {
            var driver = new ChromeDriver();
            driver.Navigate().GoToUrl(BaseUrl.Replace(nameof(ChannelNameOrId), ChannelNameOrId));

            var query = driver.FindElements(By.XPath(ThumbnailImgTagXpath));
            //query.Submit();

            //var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            //wait.Until((d) => { return d.Title.ToLower().StartsWith("cheese"); });

            driver.Quit();
        }
    }
}
