using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpExample
{
    class Sender
    {
        private HttpClient httpClient;

        public Sender()
        {
            httpClient = new HttpClient();
        }

        public async Task<string> SendPostRequest(string password, string algorithm)
        {
            var request = new Dictionary<string, string>()
            {
                { "textToHash", password },
                { "hash-algorithm", algorithm }
            };

            string response = await Send(request);
            return GetTheHashCode(response);
        }

        private async Task<string> Send(Dictionary<string, string> data)
        {
            var content = new FormUrlEncodedContent(data);
            var response = await httpClient.PostAsync("http://www.md5online.pl/", content);
            var responseString = await response.Content.ReadAsStringAsync();

            return responseString;
        }

        private string GetTheHashCode(string htmlContent)
        {
            string openTag = "<span id='result-md5'>";
            string closeTag = "</span>";

            int openTagPosition = htmlContent.IndexOf(openTag);
            openTagPosition += openTag.Length;

            var substring = htmlContent.Substring(openTagPosition);

            int closeTagPosition = substring.IndexOf(closeTag);
            string password = substring.Substring(0, closeTagPosition);

            return password;

            /** 
             * Match result = Regex.Match(htmlContent, @"<span id='result-md5'>([A-Za-z0-9\-]+)</span>");
             */
        }
    }
}
