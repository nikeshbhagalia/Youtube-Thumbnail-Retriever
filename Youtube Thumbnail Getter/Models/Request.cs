namespace Youtube_Thumbnail_Getter.Models
{
    public class Request
    {
        public HttpMethod? Method { get; set; }

        public string PostData { get; set; }

        public string Url { get; set; }
    }
}