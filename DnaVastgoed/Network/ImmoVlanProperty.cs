using DnaVastgoed.Models;
using ImmoVlanAPI;
using ImmoVlanAPI.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DnaVastgoed.Network {

    public class ImmoVlanProperty {

        private readonly DnaProperty _prop;

        public ImmoVlanProperty(DnaProperty prop) {
            _prop = prop;
        }

        /// <summary>
        /// Create and publish a new ImmoVlan property.
        /// </summary>
        /// <param name="client">The Immovlan client</param>
        /// <returns>The response</returns>
        public IRestResponse Publish(ImmoVlanClient client) {
            Property prop = new Property(_prop.Id.ToString(), _prop.Id.ToString(), GetCommercialStatus(),
                new Classification(GetTransactionType(), GetPropertyType()),
                new Location(new Address(_prop.GetLocation()[2], _prop.GetLocation()[0], _prop.GetLocation()[1], null, _prop.GetLocation()[3])) {
                    IsAddressDisplayed = true
                },
                new Description(_prop.Description, _prop.Description),
                new FinancialDetails(_prop.GetPrice(), PriceType.AskedPrice)) {
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

            return client.PublishProperty(prop).Result;
        }

        /// <summary>
        /// Suspend an Immovlan property.
        /// </summary>
        /// <param name="client">The Immovlan client</param>
        /// <returns>The response</returns>
        public IRestResponse Suspend(ImmoVlanClient client, string softwareId) {
            return client.SuspendProperty(softwareId).Result;
        }

        /// <summary>
        /// Gets the commercial status of a property.
        /// </summary>
        /// <returns>The correct status</returns>
        private CommercialStatus GetCommercialStatus() {
            return _prop.Status.Contains("Verkocht") || _prop.Status.Contains("Verhuurd") ? CommercialStatus.SOLD : CommercialStatus.ONLINE;
        }

        /// <summary>
        /// Gets the transaction type.
        /// </summary>
        /// <returns>An ImmoVlan transaction type</returns>
        private TransactionType GetTransactionType() {
            return _prop.Status.Contains("Te Koop") || _prop.Status.Contains("Verkocht") ? TransactionType.SALE : TransactionType.RENT;
        }

        /// <summary>
        /// Gets the proper property type.
        /// </summary>
        /// <returns>An ImmoVlan property type</returns>
        private PropertyType GetPropertyType() {
            switch (_prop.Type) {
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
        /// <returns>The energy score</returns>
        private int? GetEnergy() {
            try {
                if (_prop.Energy == null || _prop.Energy == "")
                    return 0;
                return int.Parse(_prop.Energy.Split(" ")[0]);
            } catch {
                return 0;
            }
        }

        /// <summary>
        /// Create a new list of the right picture objects.
        /// </summary>
        /// <returns>An array of picture objects</returns>
        private Picture[] GetPictures() {
            ICollection<Picture> pictures = new List<Picture>();

            for (int i = 0; i < _prop.Images.Count(); i++) {
                if (i < 31) {
                    string imageUrl = _prop.Images.ToArray()[i].Url;

                    pictures.Add(new Picture(i + 1, EncodeImage(imageUrl)));
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
            using (WebClient webClient = new WebClient()) {
                byte[] data = webClient.DownloadData(imageUrl);

                return Convert.ToBase64String(data);
            }
        }

    }
}
