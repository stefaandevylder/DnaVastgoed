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
                    } : null,
                    EnergyEfficiencyInfo = GetEnergy() != null ? new EnergyEfficiencyInfo() {
                        EpcScoreInKwhPerSquareMeterPerYear = GetEnergy(),
                        EpcCertificateNumber = _prop.EPCNumber,
                        EpcLabel = GetEPCLabel()
                    } : null,
                    ConstructionInfo = new ConstructionInfo() {
                        AmountOfBedrooms = !string.IsNullOrWhiteSpace(_prop.Bedrooms) ? int.Parse(_prop.Bedrooms) : null,
                        AmountOfBathrooms = !string.IsNullOrWhiteSpace(_prop.Bathrooms) ? int.Parse(_prop.Bathrooms) : null,
                        ConstructionYear = !string.IsNullOrWhiteSpace(_prop.BuildingYear) ? int.Parse(_prop.BuildingYear) : null,
                    },
                    FiscalInfo = new FiscalInfo() {
                        CadastralIncomeIndexed = GetCadastralincome()
                    },
                    ParcelInfo = new ParcelInfo() {
                        OrientationGarden = GetOrientation(),
                        AmountOfTotalPlotSquareMeters = !string.IsNullOrWhiteSpace(_prop.LotArea) ? int.Parse(_prop.LotArea.Split(" ")[0]) : null,
                        AmountOfBuiltSquareMeters = !string.IsNullOrWhiteSpace(_prop.LivingArea) ? int.Parse(_prop.LivingArea.Split(" ")[0]) : null,
                        ParcelHasPremptionRights = _prop.Voorkooprecht == "Ja",
                        ParcelFloodProneType = GetFloodProneType()
                    },
                    PermitInfo = new PermitInfo() {
                        PermitType = string.IsNullOrWhiteSpace(_prop.Bouwvergunning) ? PermitType.Unknown : PermitType.PermitAvailable,
                        AreaDesignationType = string.IsNullOrWhiteSpace(_prop.StedenbouwkundigeBestemming) ? AreaDesignationType.Unknown : AreaDesignationType.Urban,
                        SubdivisionPermitType = GetSubdivisionalPermitType(),
                        ConstructionOffenseSubpoenaType = string.IsNullOrWhiteSpace(_prop.Dagvaarding) ? ConstructionOffenseSubpoenaType.Unknown : ConstructionOffenseSubpoenaType.None
                    }
                },
                new SpottoTransaction() {
                    Type = transactionType,
                    AvailabilityStatusType = AvailabilityStatusType.Available,
                    HidePriceDetails = string.IsNullOrWhiteSpace(_prop.Price),
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
        private TransactionType GetTransactionType() {
            switch (_prop.Status) {
                case "Verkocht": return TransactionType.Sale;
                case "Verhuurd": return TransactionType.Rent;
                case "Te Koop": return TransactionType.Sale;
                case "Te Huur": return TransactionType.Rent;
            }

            return TransactionType.Sale;
        }

        /// <summary>
        /// Gets the proper property type.
        /// </summary>
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
            if (!string.IsNullOrWhiteSpace(_prop.OrientatieAchtergevel)) {
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
            }

            return OrientationType.Unknown;
        }

        /// <summary>
        /// Gets the energy score.
        /// </summary>
        private int? GetEnergy() {
            if (!string.IsNullOrWhiteSpace(_prop.Energy)) {
                try {
                    return int.Parse(_prop.Energy.Split(" ")[0]);
                } catch {
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the EPC label.
        /// </summary>
        private EpcLabel GetEPCLabel() {
            if (!string.IsNullOrWhiteSpace(_prop.Energy)) {
                string epcLabel = _prop.Energy.Split(" ").Last();

                switch (epcLabel) {
                    case "A": return EpcLabel.A;
                    case "B": return EpcLabel.B;
                    case "C": return EpcLabel.C;
                    case "D": return EpcLabel.D;
                    case "E": return EpcLabel.E;
                    case "F": return EpcLabel.F;
                    case "G": return EpcLabel.G;
                }
            }

            return EpcLabel.Unknown;
        }

        /// <summary>
        /// Gets the cadastralincome.
        /// </summary>
        private int? GetCadastralincome() {
            if (!string.IsNullOrWhiteSpace(_prop.KatastraalInkomen)) {
                try {
                    return int.Parse(_prop.KatastraalInkomen);
                } catch {
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the proper property subtype.
        /// </summary>
        private PermitType GetSubdivisionalPermitType() {
            if (!string.IsNullOrWhiteSpace(_prop.Verkavelingsvergunning)) {
                switch (_prop.Verkavelingsvergunning) {
                    case "Geen": return PermitType.NoPermitAvailable;
                    case "Ja": return PermitType.PermitAvailable;
                    case "Niet beschikbaar": return PermitType.NoPermitAvailable;
                }
            }

            return PermitType.Unknown;
        }

        /// <summary>
        /// Gets the proper floodtype.
        /// </summary>
        private FloodProneType GetFloodProneType() {
            if (!string.IsNullOrWhiteSpace(_prop.RisicoOverstroming)) {
                string flood = _prop.RisicoOverstroming.ToLower();

                if (flood.StartsWith("nee")) {
                    return FloodProneType.NotFloodProne;
                }

                if (flood.StartsWith("ja")) {
                    return FloodProneType.PossiblyFloodProneArea;
                }
            }

            return FloodProneType.Unknown;
        }
    }

}
