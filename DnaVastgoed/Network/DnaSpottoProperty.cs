using DnaVastgoed.Models;
using RestSharp;
using SpottoAPI;
using SpottoAPI.Models;
using SpottoAPI.Models.Property;
using SpottoAPI.Models.Resource;
using SpottoAPI.Models.Transaction;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DnaVastgoed.Network {

    public class DnaSpottoProperty {

        private readonly DnaProperty _prop;

        public DnaSpottoProperty(DnaProperty prop) {
            _prop = prop;
        }

        /// <summary>
        /// Create and publish a new Spotto property.
        /// </summary>
        /// <param name="client">The Spotto client</param>
        /// <returns>The response</returns>
        public async Task<RestResponse> Publish(SpottoClient client) {
            TransactionType transactionType = GetTransactionType();

            SpottoListing listing = new SpottoListing(
                new SpottoProperty() {
                    Type = GetPropertyType(),
                    SubType = GetPropertySubType(),
                    Descriptions = new List<Description> {
                        new Description(DescriptionType.Title, "NL", _prop.Name),
                        new Description(DescriptionType.DetailedDescription, "NL", _prop.Description),
                    },
                    Location = _prop.Location != null ? new LocationInfo {
                        Address = new AddressInfo {
                            Street = _prop.GetLocation()[0],
                            StreetNumber = _prop.GetLocation()[1],
                            MunicipalityPostalCode = _prop.GetLocation()[2],
                            MunicipalityName = _prop.GetLocation()[3],
                            TwoLetterIsoCountryCode = "BE"
                        }
                    } : null
                },
                new SpottoTransaction() {
                    Type = transactionType,
                    AvailabilityStatusType = AvailabilityStatusType.Available,
                    HidePriceDetails = _prop.Price == null,
                    ContactInfo = new ContactInfo {
                        ContactReference = "D&A Vastgoed",
                        ContactPerson = new ContactPerson {
                            Email = "info@dnavastgoed.be",
                            Name = "D&A Vastgoed",
                            PhoneNumber = "03 776 19 22",
                            PictureUrl = "https://dnavastgoed.be/wp-content/uploads/2020/09/Logo-7x7-PNG.png"
                        }
                    },
                    SaleTypeInfo = transactionType == TransactionType.Sale && _prop.Price != null ? new SaleTypeInfo {
                        Price = (double?)_prop.GetPrice()
                    } : null,
                    RentTypeInfo = transactionType == TransactionType.Rent && _prop.Price != null ? new RentTypeInfo {
                        Price = (double?)_prop.GetPrice()
                    } : null
                },
                new SpottoResource() {
                    Images = _prop.Images.Select(dnaImage => {
                        return new Image {
                            FileName = dnaImage.Url,
                            FileType = ".jpg",
                            Url = dnaImage.Url
                        };
                    }).ToList(),
                    PublicationInfo = new PublicationInfo {
                        BrokerWebsiteUrl = "https://dnavastgoed.be/"
                    }
                }
             );

            return await client.CreatePublication(listing, _prop.Id.ToString());
        }

        /// <summary>
        /// Gets the TransactionType of a property.
        /// </summary>
        /// <returns>The correct TransactionType</returns>
        private TransactionType GetTransactionType() {
            switch (_prop.Type) {
                case "Verkocht": return TransactionType.Sale;
                case "Verhuurd": return TransactionType.Rent;
                case "Realisatie": return TransactionType.Sale;
                case "Te Koop": return TransactionType.Sale;
                case "Te Huur": return TransactionType.Rent;
            }

            return TransactionType.Sale;
        }

        /// <summary>
        /// Gets the proper property type.
        /// </summary>
        /// <returns>An Spotto property type</returns>
        private PropertyType GetPropertyType() {
            switch (_prop.Type) {
                case "Woning": return PropertyType.House;
                case "Huis": return PropertyType.House;
                case "Appartement": return PropertyType.Apartment;
                case "Studio": return PropertyType.Apartment;
                case "Assistentiewoning": return PropertyType.Other;
                case "Industrieel/Commercieel": return PropertyType.Industrial;
                case "Grond": return PropertyType.Land;
                case "Garage": return PropertyType.Garage;
                case "Gemeubeld Appartement/Expats": return PropertyType.Apartment;
            }

            return PropertyType.Other;
        }

        /// <summary>
        /// Gets the proper property subtype.
        /// </summary>
        /// <returns>An Spotto property subtype</returns>
        private PropertySubType GetPropertySubType() {
            switch (_prop.Type) {
                case "Studio": return PropertySubType.Studio;
                case "Assistentiewoning": return PropertySubType.ServiceFlat;
                case "Industrieel/Commercieel": return PropertySubType.CommercialSpace;
                case "Garage": return PropertySubType.Garage;
            }

            return PropertySubType.Other;
        }

    }

}
