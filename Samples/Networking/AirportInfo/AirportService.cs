using System;
using System.Text;
using Android.Net.Http;
using Java.Io;
using Org.Apache.Http.Client.Methods;
using Org.Json;

namespace AirportInfo
{
    internal static class AirportService
    {
        /// <summary>
        /// Call the FAA Airport service to get status information for airport with the given code.
        /// </summary>
        public static AirportStatus GetStatus(string iataCode)
        {
            var content = PerformRequest(iataCode);
            if (content == null)
                return null;
            var status = new JSONObject(content);
            return new AirportStatus(status);
        }

        /// <summary>
        /// Perform the low level request and return the resulting content.
        /// </summary>
        private static string PerformRequest(string iataCode)
        {
            var uri = string.Format("http://services.faa.gov/airport/status/{0}?format=application/json", iataCode);
            var client = AndroidHttpClient.NewInstance("AirportInfo");
            var request = new HttpGet(uri);
            try
            {
                var response = client.Execute(request);
                var content = response.GetEntity().GetContent();
                var reader = new BufferedReader(new InputStreamReader(content));
                var builder = new StringBuilder();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    builder.Append(line);
                }
                return builder.ToString();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
