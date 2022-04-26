using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Youtube_Thumbnail_Getter.Models;

namespace Youtube_Thumbnail_Getter
{
    public class Program
    {
        private const string YoutubeChannelVideosUrl = @"https://www.youtube.com/c/Sidemen/videos?view=0&sort=da&flow=grid";
        private const string ThumbnailBaseUrl = @"https://img.youtube.com/vi/videoId/mqdefault.jpg";
        private const string SpinnerTagName = "paper-spinner";
        private const string BodyTagName = "body";
        private const string VideoAnchorTagXpath = @"//a[@id = 'video-title']";
        private const string DestinationPath = @"D:\Thumbnails\";

        public static async Task Main(string[] args)
        {
            var start = DateTime.Now;
            var chromeOptions = new ChromeOptions();
            chromeOptions.SetLoggingPreference("performance", LogLevel.All);
            //chromeOptions.AddArguments("--headless");

            var driver = new ChromeDriver(chromeOptions);

            driver.Navigate().GoToUrl(YoutubeChannelVideosUrl);

            driver.Manage().Logs.GetLog("performance");
            var bodyElement = driver.FindElement(By.CssSelector(BodyTagName));
            bodyElement.SendKeys(Keys.Control + Keys.End);

            //var startTime = DateTime.Now;
            //string requestId = null;
            //while (DateTime.Now.Subtract(startTime).Seconds < 5)
            //{
            //    var logs = driver.Manage().Logs.GetLog("performance");
            //    foreach (var log in logs)
            //    {
            //        var message = JsonConvert.DeserializeObject<ChromeLog>(log.Message).Message;
            //        //var logRequestId = message.SelectToken("message.params.requestId")?.ToString();

            //        //if (!string.IsNullOrEmpty(requestId))
            //        //{
            //        //    var status = message.SelectToken("message.params.response.status")?.ToString();
            //        //    if (network == "Network.responseReceived" && status == "200" && requestId == logRequestId)
            //        //    {
            //        //        return;
            //        //    }
            //        //}

            //        if (message.Method == "Network.requestWillBeSent" && message?.Params?.Request?.Method == HttpMethod.Post)
            //        {
            //            var test = "";
            //            //requestId = logRequestId;
            //        }
            //    }
            //}

            var spinnerElements = FindSpinnerElements(driver);
            while (spinnerElements.Count != 0)
            {
                bodyElement.SendKeys(Keys.Control + Keys.End);
                spinnerElements = FindSpinnerElements(driver);
            }

            var anchorElements = driver.FindElements(By.XPath(VideoAnchorTagXpath));
            var thumbnailDictionary = new Dictionary<int, string>();

            for (var index = 0; index < anchorElements.Count; index++)
            {
                var anchorElement = anchorElements[index];
                var href = anchorElement.GetAttribute("href");
                var videoId = href.Substring(href.LastIndexOf("v=") + 2);
                var thumbnailUrl = ThumbnailBaseUrl.Replace(nameof(videoId), videoId);

                thumbnailDictionary.Add(index + 1, thumbnailUrl);
            }

            driver.Quit();
            //await Task.WhenAll(thumbnailDictionary.Select(thumbnailKvp => Download(thumbnailKvp.Key, thumbnailKvp.Value)));
            var end = DateTime.Now - start;
        }

        private static ReadOnlyCollection<IWebElement> FindSpinnerElements(IWebDriver driver) =>
            driver.FindElements(By.TagName(SpinnerTagName));

        private static async Task Download(int number, string thumbnailUrl)
        {
            using (var webClient = new WebClient())
            {
                await webClient.DownloadFileTaskAsync(new Uri(thumbnailUrl), $"{DestinationPath}{number}.jpg");
            }
        }
    }
}
