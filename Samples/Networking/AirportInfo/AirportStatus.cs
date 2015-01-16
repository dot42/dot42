using Org.Json;

namespace AirportInfo
{
    /// <summary>
    /// Wrapper for a status retrieved from the FAA airport status service.
    /// </summary>
    internal class AirportStatus
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        internal AirportStatus(JSONObject source)
        {
            Delay = source.GetBoolean("delay");
            IataCode = source.GetString("IATA");
            State= source.GetString("state");
            City = source.GetString("city");
            Name = source.GetString("name");

            var weather = source.GetJSONObject("weather");
            Temperature = weather.GetString("temp");
            Wind = weather.GetString("wind");
        }

        // Airport
        public bool Delay { get; private set; }
        public string IataCode { get; private set; }
        public string State { get; private set; }
        public string City { get; private set; }
        public string Name { get; private set; }

        // Weather
        public string Temperature { get; private set; }
        public string Wind { get; private set; }
    }
}
