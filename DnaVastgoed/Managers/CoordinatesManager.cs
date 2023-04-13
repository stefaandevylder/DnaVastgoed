using Newtonsoft.Json.Linq;
using RestSharp;
using System.Threading.Tasks;

namespace DnaVastgoed.Managers {

    public class CoordinatesManager {

        private RestClient Client { get; set; }

        public CoordinatesManager() {
            Client = new RestClient("https://geocode.maps.co");
        }

        /// <summary>
        /// Request a long and lat from free api.
        /// </summary>
        /// <returns>The format from geocode.maps.co</returns>
        public async Task<CoordinatesResponse> GetCoordinatesFromAddress(string address) {
            RestRequest req = new RestRequest("/search");
            req.AddQueryParameter("q", address);

            var response = await Client.ExecuteGetAsync(req);
            var jsonResponse = JArray.Parse(response.Content);

            if (jsonResponse.Count > 0) {
                return new CoordinatesResponse() {
                    Lat = jsonResponse[0]["lat"].ToString(),
                    Lng = jsonResponse[0]["lon"].ToString()
                };
            }

            return new CoordinatesResponse();
        }
    }

    public class CoordinatesResponse {

        public string Lat { get; set; }
        public string Lng { get; set; }

    }
}
