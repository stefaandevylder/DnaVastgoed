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
                        HideLocation = false,
                        Address = new AddressInfo {
                            Street = _prop.GetLocation()[0],
                            StreetNumber = _prop.GetLocation()[1],
                            MunicipalityPostalCode = _prop.GetLocation()[2],
                            MunicipalityName = _prop.GetLocation()[3],
                            TwoLetterIsoCountryCode = "BE"
                        }
                    } : null,
                    EnergyEfficiencyInfo = new EnergyEfficiencyInfo() {
                        EpcScoreInKwhPerSquareMeterPerYear = GetEnergy(),
                        EpcCertificateNumber = _prop.EPCNumber
                    },
                    ConstructionInfo = new ConstructionInfo() {
                        AmountOfBedrooms = !string.IsNullOrWhiteSpace(_prop.Bedrooms) ? int.Parse(_prop.Bedrooms) : null,
                        AmountOfBathrooms = !string.IsNullOrWhiteSpace(_prop.Bathrooms) ? int.Parse(_prop.Bathrooms) : null
                    },
                    FiscalInfo = new FiscalInfo() {
                        CadastralIncomeIndexed = !string.IsNullOrWhiteSpace(_prop.KatastraalInkomen) ? int.Parse(_prop.KatastraalInkomen) : null
                    },
                    ParcelInfo = new ParcelInfo() {
                        OrientationGarden = GetOrientation(),
                        AmountOfTotalPlotSquareMeters = !string.IsNullOrWhiteSpace(_prop.LotArea) ? int.Parse(_prop.LotArea.Split(" ")[0]) : null
                    }
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
                    SaleTypeInfo = transactionType == TransactionType.Sale ? new SaleTypeInfo {
                        Price = _prop.Price != null ? (double?)_prop.GetPrice() : 0
                    } : null,
                    RentTypeInfo = transactionType == TransactionType.Rent ? new RentTypeInfo {
                        Price = _prop.Price != null ? (double?)_prop.GetPrice() : 0
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
            switch (_prop.Status) {
                case "Verkocht": return TransactionType.Sale;
                case "Verhuurd": return TransactionType.Rent;
                case "Te Koop": return TransactionType.Sale;
                case "Te Huur": return TransactionType.Rent;
            }

            return TransactionType.Unknown;
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

        /// <summary>
        /// Get the orientation.
        /// </summary>
        private OrientationType GetOrientation() {
            if (string.IsNullOrWhiteSpace(_prop.OrientatieAchtergevel)) {
                return OrientationType.Unknown;
            }

            switch (_prop.OrientatieAchtergevel.ToLower()) {
                case "n": return OrientationType.North;
                case "no": return OrientationType.NorthEast;
                case "o": return OrientationType.East;
                case "so": return OrientationType.SouthEast;
                case "s": return OrientationType.South;
                case "sw": return OrientationType.SouthWest;
                case "w": return OrientationType.West;
                case "nw": return OrientationType.NorthWest;
            }

            return OrientationType.Unknown;
        }

        /// <summary>
        /// Gets the energy score.
        /// </summary>
        private int? GetEnergy() {
            try {
                if (_prop.Energy == null || _prop.Energy == "")
                    return 0;
                return int.Parse(_prop.Energy.Split(" ")[0]);
            } catch {
                return 0;
            }
        }

    }

}
