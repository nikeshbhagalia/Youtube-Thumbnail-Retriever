using GetYoutubeThumbnail.Tests.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HttpMethod = GetYoutubeThumbnail.Tests.Models.HttpMethod;

namespace GetYoutubeThumbnail.Tests
{
    public class GetYoutubeThumbnailsByChannelTests
    {
        private const string ThumbnailBaseUrl = @"https://img.youtube.com/vi/videoId/mqdefault.jpg";
        private const string ShortsBaseUrl = @"https://www.youtube.com/shorts/";
        private const string BodyTagName = "body";
        private const string VideoAnchorTagXpath = @"//a[@id = 'video-title']";
        private const string DestinationPath = @"D:\Thumbnails\";
        private const string PerformanceKey = "performance";
        private const string YoutubeBrowseApiBaseUrl = @"https://www.youtube.com/youtubei/v1/browse?key=";
        private const string ChromeBinaryPath = @"GoogleChromePortable\App\Chrome-bin\chrome.exe";

        private IWebDriver _driver;

        [SetUp]
        public void Setup()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.SetLoggingPreference(PerformanceKey, LogLevel.Info);
            chromeOptions.BinaryLocation = ChromeBinaryPath;
            chromeOptions.AddArguments("--headless");

            _driver = new ChromeDriver(chromeOptions);
        }

        [TearDown]
        public void TearDown()
        {
            _driver.Quit();
        }

        [TestCase(@"https://www.youtube.com/PewDiePie/videos?view=0&sort=da&flow=grid")]
        public async Task GetThumbnails(string youtubeChannelVideosUrl)
        {
            _driver.Url = youtubeChannelVideosUrl;

            _driver.Manage().Logs.GetLog(PerformanceKey);
            var bodyElement = _driver.FindElement(By.CssSelector(BodyTagName));
            bodyElement.SendKeys(Keys.Control + Keys.End);

            string apiUrlWithKey = null;
            YoutubePostRequestBody postRequestBody = null;
            SearchLogsForApiRequest(ref apiUrlWithKey, ref postRequestBody);

            var anchorElements = _driver.FindElements(By.XPath(VideoAnchorTagXpath));
            var thumbnailDictionary = new Dictionary<int, string>();

            var index = 0;
            while (index < anchorElements.Count)
            {
                var anchorElement = anchorElements[index];
                var href = anchorElement.GetAttribute("href");
                var videoId = href.Substring(href.LastIndexOf("v=") + 2);
                var thumbnailUrl = ThumbnailBaseUrl.Replace(nameof(videoId), videoId);

                index++;

                thumbnailDictionary.Add(index, thumbnailUrl);
            }

            _driver.Quit();

            if (postRequestBody != null)
            {
                var restClient = new RestClient();
                var request = new RestRequest(apiUrlWithKey, Method.POST, DataFormat.Json);
                request.AddJsonBody(JsonConvert.SerializeObject(postRequestBody));
                var youtubeResponse = restClient.Execute<YoutubeResponse>(request).Data;

                while (youtubeResponse != null)
                {
                    var continuationItems = youtubeResponse.OnResponseReceivedActions.Single().AppendContinuationItemsAction.ContinuationItems;
                    string continuationToken = null;
                    foreach (var continuationItem in continuationItems)
                    {
                        var videoId = continuationItem.GridVideoRenderer?.VideoId;
                        if (videoId != null)
                        {
                            var thumbnailUrl = ThumbnailBaseUrl.Replace(nameof(videoId), videoId);
                            index++;
                            thumbnailDictionary.Add(index, thumbnailUrl);
                        }

                        if (continuationItem.ContinuationItemRenderer?.ContinuationEndpoint?.ContinuationCommand?.Token != null)
                        {
                            continuationToken = continuationItem.ContinuationItemRenderer.ContinuationEndpoint.ContinuationCommand.Token;
                        }
                    }

                    if (continuationToken is null)
                    {
                        youtubeResponse = null;
                    }
                    else
                    {
                        postRequestBody.Continuation = continuationToken;
                        request = new RestRequest(apiUrlWithKey, Method.POST, DataFormat.Json);
                        request.AddJsonBody(JsonConvert.SerializeObject(postRequestBody));
                        youtubeResponse = restClient.Execute<YoutubeResponse>(request).Data;
                    }
                }
            }

            var tasks = new List<Task>();
            for (var i = 1; i <= thumbnailDictionary.Count; i++)
            {
                tasks.Add(Download(i, thumbnailDictionary[i]));
                if (i % 100 == 0 || i == thumbnailDictionary.Count)
                {
                    await Task.WhenAll(tasks);
                    tasks.Clear();
                }
            }
        }

        private async Task Download(int number, string thumbnailUrl)
        {
            using (var webClient = new WebClient())
            {
                await webClient.DownloadFileTaskAsync(new Uri(thumbnailUrl), $"{DestinationPath}{number}.jpg");
            }
        }

        private void SearchLogsForApiRequest(ref string apiUrlWithKey, ref YoutubePostRequestBody postRequestBody)
        {
            var startTime = DateTime.Now;
            while (DateTime.Now.Subtract(startTime).Seconds < 5)
            {
                var logs = _driver.Manage().Logs.GetLog(PerformanceKey);
                foreach (var log in logs)
                {
                    var message = JsonConvert.DeserializeObject<ChromeLog>(log.Message).Message;

                    if (message.Method == "Network.requestWillBeSent" && message.Params?.Request?.Method == HttpMethod.Post && message.Params.Request.Url.Contains(YoutubeBrowseApiBaseUrl))
                    {
                        apiUrlWithKey = message.Params.Request.Url;
                        postRequestBody = JsonConvert.DeserializeObject<YoutubePostRequestBody>(message.Params.Request.PostData);
                        return;
                    }
                }
            }
        }
    }
}
