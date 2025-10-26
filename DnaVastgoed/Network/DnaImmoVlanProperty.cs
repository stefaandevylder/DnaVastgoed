using DnaVastgoed.Models;
using ImmoVlanAPI;
using ImmoVlanAPI.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DnaVastgoed.Network {

    public class DnaImmoVlanProperty {

        public readonly DnaProperty Property;

        public DnaImmoVlanProperty(DnaProperty prop) {
            Property = prop;
        }

        /// <summary>
        /// Create and publish a new ImmoVlan property.
        /// </summary>
        /// <param name="client">The Immovlan client</param>
        /// <returns>The response</returns>
        public async Task<RestResponse> Publish(ImmoVlanClient client) {
            Property prop = new Property(Property.Id.ToString(), Property.Id.ToString(), GetCommercialStatus(),
                new Classification(GetTransactionType(), GetPropertyType()),
                new Location(new Address(Property.GetLocation()[2], Property.GetLocation()[0], Property.GetLocation()[1], null, Property.GetLocation()[3])) {
                    IsAddressDisplayed = true
                },
                new Description(Property.Description, Property.Description),
                new FinancialDetails(Property.GetPrice(), PriceType.AskedPrice)) {
                GeneralInformation = new GeneralInformation() {
                    ContactEmail = "info@dnavastgoed.be",
                    ContactPhone = "037761922"
                },
                Certificates = new Certificates() {
                    Epc = new EPC() {
                        EnergyConsumption = GetEnergy().GetValueOrDefault()
                    }
                },
                Attachments = new Attachments() {
                    Pictures = GetPictures(),
                }
            };

            return await client.PublishProperty(prop);
        }

        /// <summary>
        /// Suspend an Immovlan property.
        /// </summary>
        /// <param name="client">The Immovlan client</param>
        /// <returns>The response</returns>
        public async Task<RestResponse> Suspend(ImmoVlanClient client, string softwareId) {
            return await client.SuspendProperty(softwareId);
        }

        /// <summary>
        /// Gets the commercial status of a property.
        /// </summary>
        /// <returns>The correct status</returns>
        private CommercialStatus GetCommercialStatus() {
            return Property.Status.Contains("Verkocht") 
                || Property.Status.Contains("Verhuurd") 
                || Property.Status.Contains("Realisatie") ? CommercialStatus.SOLD : CommercialStatus.ONLINE;
        }

        /// <summary>
        /// Gets the transaction type.
        /// </summary>
        /// <returns>An ImmoVlan transaction type</returns>
        private TransactionType GetTransactionType() {
            return Property.Status.Contains("Te Koop") || Property.Status.Contains("Verkocht") ? TransactionType.SALE : TransactionType.RENT;
        }

        /// <summary>
        /// Gets the proper property type.
        /// </summary>
        /// <returns>An ImmoVlan property type</returns>
        private PropertyType GetPropertyType() {
            switch (Property.Type) {
                case "Woning": return PropertyType.Residence;
                case "Huis": return PropertyType.Residence;
                case "Appartement": return PropertyType.FlatApartment;
                case "Studio": return PropertyType.FlatStudio;
                case "Assistentiewoning": return PropertyType.UnditerminedProperty;
                case "Industrieel/Commercieel": return PropertyType.CommerceBuilding;
                case "Grond": return PropertyType.DevelopmentSite;
                case "Garage": return PropertyType.GarageBuilding;
                case "Gemeubeld Appartement/Expats": return PropertyType.FlatApartment;
            }

            return PropertyType.UnditerminedProperty;
        }

        /// <summary>
        /// Gets the energy score.
        /// </summary>
        private int? GetEnergy() {
            if (!string.IsNullOrWhiteSpace(Property.Energy)) {
                try {
                    return int.Parse(Property.Energy.Split(" ")[0]);
                } catch {
                    return 0;
                }
            }

            return 0;
        }

        /// <summary>
        /// Create a new list of the right picture objects.
        /// </summary>
        /// <returns>An array of picture objects</returns>
        private Picture[] GetPictures() {
            ICollection<Picture> pictures = new List<Picture>();
            var images = Property.Images.Take(25);

            foreach (var item in images.Select((value, i) => new { i, value })) {
                string imageUrl = item.value.Url;
                string encodedImage = EncodeImage(imageUrl);

                if (encodedImage != null) {
                    pictures.Add(new Picture(item.i + 1, encodedImage));
                }
            }

            return pictures.ToArray();
        }

        /// <summary>
        /// Encode an image from the web to a base64.
        /// </summary>
        /// <param name="imageUrl">The url we need to encode</param>
        /// <returns>A base64 in string form</returns>
        private string EncodeImage(string imageUrl) {
            try {
                using WebClient webClient = new WebClient();
                byte[] data = webClient.DownloadData(imageUrl);

                return Convert.ToBase64String(data);
            } catch {
                return null;
            }
        }

    }
}
