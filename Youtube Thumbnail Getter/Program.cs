using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;

namespace Youtube_Thumbnail_Getter
{
    class Program
    {
        private const string YoutubeChannelVideosUrl = @"https://www.youtube.com/user/YouTube/videos";
        private const string ThumbnailBaseUrl = @"https://img.youtube.com/vi/videoId/mqdefault.jpg";
        private const string SpinnerTagName = "paper-spinner";
        private const string BodyTagName = "body";
        private const string HrefAttributeName = "href";
        private const string VideoAnchorTagXpath = @"//a[@id = 'video-title']";
        private const string DestinationPath = @"D:\Thumbnails\";
        private const string HeadlessArgument = "--headless";
        private const string VideoIdPrefix = "v=";
        
        private readonly IWebElement _bodyElement;

        static void Main(string[] args)
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments(HeadlessArgument);
            var driver = new ChromeDriver(chromeOptions);
            
            driver.Url = YoutubeChannelVideosUrl;

            ScrollDown();
            
            var spinnerElements = FindSpinnerElements(driver);
            while (spinnerElements.Count != 0)
            {
                ScrollDown();
                spinnerElements = FindSpinnerElements(driver);
            }

            var anchorElements = driver.FindElements(By.XPath(VideoAnchorTagXpath));
            var thumbnailDictionary = new Dictionary<int, string>();

            for (var index = 0; index < anchorElements.Count; index++)
            {
                var anchorElement = anchorElements[index];
                var href = anchorElement.GetAttribute(HrefAttributeName);
                var videoId = href.Substring(href.LastIndexOf(VideoIdPrefix) + 2);
                var thumbnailUrl = ThumbnailBaseUrl.Replace(nameof(videoId), videoId);

                thumbnailDictionary.Add(index + 1, thumbnailUrl);
            }

            driver.Quit();
            
            await Task.WhenAll(thumbnailDictionary.Select(thumbnailKvp => Download(thumbnailKvp.Key, thumbnailKvp.Value)));
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
        
        private static void ScrollDown() =>
            _bodyElement.SendKeys(Keys.Control + Keys.End);
    }
}
