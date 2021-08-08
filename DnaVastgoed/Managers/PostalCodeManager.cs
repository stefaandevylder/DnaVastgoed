using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DnaVastgoed.Managers {

    public class PostalCodeManager {

        private readonly List<PostalCode> postalCodes;

        public PostalCodeManager() {
            string belgium = File.ReadAllText("./Data/Files/PostalCodes.json");

            postalCodes = JsonConvert.DeserializeObject<List<PostalCode>>(belgium);
        }

        /// <summary>
        /// Finds the nearby postalcodes in given radius.
        /// </summary>
        /// <param name="postalCode">The postalcode to search from</param>
        /// <param name="radiusInKM">The radius in kilometers to search</param>
        /// <returns>A list of the postalcodes within distance</returns>
        public IEnumerable<string> FindNearby(string postalCode, double radiusInKM) {
            PostalCode pc = postalCodes.FirstOrDefault(p => p.Zip == postalCode);
            ICollection<string> codes = new List<string>();

            if (pc != null) {
                foreach (PostalCode c in postalCodes) {
                    double distance = DistanceTo(pc.Lat, pc.Lng, c.Lat, c.Lng);

                    if (distance <= radiusInKM) {
                        codes.Add(c.Zip);
                    }
                }
            }

            return codes;
        }

        /// <summary>
        /// Calculates the distance between two points on earth in km.
        /// </summary>
        /// <param name="lat1">Latitude postition 1</param>
        /// <param name="lon1">Longitude postition 1</param>
        /// <param name="lat2">Latitude position 2</param>
        /// <param name="lon2">Longitude position 2</param>
        /// <returns>The distance between those points on earth</returns>
        private double DistanceTo(double lat1, double lon1, double lat2, double lon2) {
            double rlat1 = Math.PI * lat1 / 180;
            double rlat2 = Math.PI * lat2 / 180;
            double theta = lon1 - lon2;
            double rtheta = Math.PI * theta / 180;
            double dist =
                Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            return dist * 1.609344;
        }
    }

    public class PostalCode {

        public string Zip { get; set; }
        public string City { get; set; }
        public double Lng { get; set; }
        public double Lat { get; set; }

    }
}
