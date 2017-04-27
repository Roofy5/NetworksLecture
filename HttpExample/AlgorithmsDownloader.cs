using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace HttpExample
{
    class AlgorithmsDownloader
    {
        private WebRequest request;
        private WebResponse response;

        public AlgorithmsDownloader()
        {
            request = WebRequest.Create("http://www.md5online.pl/");
        }

        public async Task<string[]> GetAlgorithms()
        {
            response = await request.GetResponseAsync();
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);

            string pageContent = await reader.ReadToEndAsync();

            return GetAlgorithmsFromHtml(pageContent);
        }

        private string[] GetAlgorithmsFromHtml(string content)
        {
            List<string> result = new List<string>();

            int index = content.IndexOf("name=\"hash-algorithm\"");
            int indexEnd= -1;
            string substring = content.Substring(index);

            string optionStart = "<option  value=\"";
            string optionEnd = "\">";

            while (index != -1)
            {
                index = substring.IndexOf(optionStart);
                substring = substring.Substring(index + optionStart.Length);
                indexEnd = substring.IndexOf(optionEnd);

                string algoritmName = substring.Substring(0, indexEnd);
                result.Add(algoritmName);
            }

            result.RemoveAt(result.Count - 1);
            return result.ToArray();
        }
    }
}
