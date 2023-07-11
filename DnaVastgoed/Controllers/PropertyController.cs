using AngleSharp.Html.Parser;
using DnaVastgoed.Data.Repositories;
using DnaVastgoed.Managers;
using DnaVastgoed.Models;
using DnaVastgoed.Network;
using ImmoVlanAPI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using RestSharp;
using SpottoAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DnaVastgoed.Controllers {

    [ApiController]
    [Route("[controller]")]
    public class PropertyController : ControllerBase {

        public IConfiguration Configuration;

        private readonly PostmarkManager _postmarkManager;
        private readonly PostalCodeManager _postalCodeManager;
        private readonly CoordinatesManager _coordinatesManager;

        private readonly PropertyRepository _propertyRepository;
        private readonly SubscriberRepository _subscriberRepository;

        public PropertyController(IConfiguration configuration, PostmarkManager postmarkManager, PostalCodeManager postalCodeManager,
            CoordinatesManager coordinatesManager, PropertyRepository propertyRepository, SubscriberRepository subscriberRepository) {
            Configuration = configuration;

            _postmarkManager = postmarkManager;
            _postalCodeManager = postalCodeManager;
            _coordinatesManager = coordinatesManager;

            _propertyRepository = propertyRepository;
            _subscriberRepository = subscriberRepository;
        }

        /// <summary>
        /// Gets a list of all properties in the database.
        /// </summary>
        /// <returns>A list of DnaProperty objects</returns>
        [HttpGet]
        public ActionResult<IEnumerable<DnaProperty>> GetProperties(int? id = null) {
            if (!id.HasValue)
                return Ok(_propertyRepository.GetAll());

            return Ok(_propertyRepository.GetById(id.Value));
        }

        /// <summary>
        /// Scrape the BaseURL for a list of all properties. After that
        /// it will scrape each URL and save it in the database.
        /// </summary>
        /// <returns>A log list of what happend during this action (To debug)</returns>
        [HttpGet("scrape")]
        public async Task<ActionResult<IEnumerable<string>>> Scrape(int page = 1) {
            ICollection<string> logs = new List<string>();
            IEnumerable<string> links = ParseJson(Configuration["BaseURL"], $"/wp-json/wp/v2/property?per_page=100&page={page}&orderby=date");

            HtmlParser parser = new HtmlParser();

            foreach (string link in links) {
                var client = new RestClient(link);
                var request = new RestRequest();
                var queryResult = client.Get(request);

                DnaProperty property = new DnaProperty(
                    parser.ParseDocument(queryResult.Content),
                    link,
                    Configuration["BaseURL_Replace"],
                    Configuration["BaseURL"]);

                string logResult = await AddOrUpdateProperty(property);
                logs.Add(logResult);
            }

            await _propertyRepository.SaveChanges();

            return Ok(logs);
        }        
        
        /// <summary>
        /// Scrape the BaseURL for a list of all properties. After that
        /// it will scrape each URL and save it in the database.
        /// </summary>
        /// <returns>A log list of what happend during this action (To debug)</returns>
        [HttpGet("fetch-coordinates")]
        public async Task<ActionResult<IEnumerable<string>>> FetchCoordinates(string apiKey) {
            if (apiKey != Configuration["ApiKey"])
                return BadRequest("API key is not correct.");

            ICollection<string> logs = new List<string>();
            IEnumerable<DnaProperty> properties = await _propertyRepository.GetAll();

            foreach (DnaProperty property in properties) {
                if (string.IsNullOrWhiteSpace(property.CoordinatesLng) || string.IsNullOrWhiteSpace(property.CoordinatesLat)) {
                    var coordinates = await _coordinatesManager.GetCoordinatesFromAddress(property.Location);
                    property.CoordinatesLat = coordinates.Lat;
                    property.CoordinatesLng = coordinates.Lng;

                    // The API we use has a rate limit of 2 requests every 1 second.
                    Thread.Sleep(1000);
                    logs.Add($"FETCHED: {coordinates.Lat} and {coordinates.Lng} for {property.Name}");
                } else {
                    logs.Add($"ALREADY FETCHED, skipping: {property.Name}");
                }
            }

            await _propertyRepository.SaveChanges();
            return Ok(logs);
        }

        /// <summary>
        /// Gets all properties from the database, checks if the property
        /// has to be uploaded to Immovlan or not. If true, it will upload
        /// the property and set the status so it will not upload twice.
        /// </summary>
        /// <returns>A log list of what happend during this action (To debug)</returns>
        [HttpGet("immovlan")]
        public async Task<ActionResult<IEnumerable<string>>> UploadToImmovlan() {
            ICollection<string> logs = new List<string>();
            ImmoVlanClient immovlanClient = new ImmoVlanClient(Configuration["ImmoVlan:BusinessEmail"],
                Configuration["ImmoVlan:TechincalEmail"], int.Parse(Configuration["ImmoVlan:SoftwareId"]),
                Configuration["ImmoVlan:ProCustomerId"], Configuration["ImmoVlan:SoftwarePassword"], false);

            IEnumerable<DnaProperty> allPropertiesToUpload = await _propertyRepository.GetAll();
            allPropertiesToUpload = allPropertiesToUpload.Where(p => p.UploadToImmovlan);

            foreach (DnaProperty property in allPropertiesToUpload) {
                DnaImmoVlanProperty imovlanProperty = new DnaImmoVlanProperty(property);

                RestResponse result = await imovlanProperty.Publish(immovlanClient);
                logs.Add($"UPLOADED: {property.Name} ({property.Images.Count} images) with result {result.Content}");

                property.UploadToImmovlan = false;
            }

            await _propertyRepository.SaveChanges();

            if (allPropertiesToUpload.Count() > 0) {
                _ = _postmarkManager.sendUploadedImmoVlan(allPropertiesToUpload);
            }

            return Ok(logs);
        }

        /// <summary>
        /// Reset all statuses to upload to immovlan.
        /// </summary>
        /// <param name="apiKey">The API admin key</param>
        /// <returns>A log list of what happend during this action (To debug)</returns>
        [HttpGet("immovlan/resetstatus")]
        public async Task<ActionResult<IEnumerable<string>>> ResetStatusesImmovlan(string apiKey) {
            if (apiKey != Configuration["ApiKey"])
                return BadRequest("API key is not correct.");

            ICollection<string> logs = new List<string>();
            IEnumerable<DnaProperty> properties = await _propertyRepository.GetAll();

            foreach (DnaProperty property in properties) {
                property.UploadToImmovlan = true;
                logs.Add($"Status reset for {property.Name}");
            }

            await _propertyRepository.SaveChanges();

            return Ok(logs);
        }

        /// <summary>
        /// Suspend one property.
        /// </summary>
        /// <param name="apiKey">The API admin key</param>
        /// <param name="id">The ID of the property to suspend</param>
        /// <returns>A log list of what happend during this action (To debug)</returns>
        [HttpGet("immovlan/suspend")]
        public async Task<ActionResult<IEnumerable<string>>> SuspendImmovlanProperty(string apiKey, int id) {
            if (apiKey != Configuration["ApiKey"])
                return BadRequest("API key is not correct.");

            ICollection<string> logs = new List<string>();
            ImmoVlanClient immovlanClient = new ImmoVlanClient(Configuration["ImmoVlan:BusinessEmail"],
                Configuration["ImmoVlan:TechincalEmail"], int.Parse(Configuration["ImmoVlan:SoftwareId"]),
                Configuration["ImmoVlan:ProCustomerId"], Configuration["ImmoVlan:SoftwarePassword"]);

            DnaProperty property = await _propertyRepository.GetById(id);

            await immovlanClient.SuspendProperty(property.Id.ToString());
            logs.Add($"Property {property.Name} suspended.");

            property.UploadToImmovlan = false;

            await _propertyRepository.SaveChanges();

            return Ok(logs);
        }

        /// <summary>
        /// Suspend all properties.
        /// </summary>
        /// <param name="apiKey">The API admin key</param>
        /// <returns>A log list of what happend during this action (To debug)</returns>
        [HttpGet("immovlan/suspend/all")]
        public async Task<ActionResult<IEnumerable<string>>> SuspendAllImmovlanProperties(string apiKey) {
            if (apiKey != Configuration["ApiKey"])
                return BadRequest("API key is not correct.");

            ICollection<string> logs = new List<string>();
            ImmoVlanClient immovlanClient = new ImmoVlanClient(Configuration["ImmoVlan:BusinessEmail"],
                Configuration["ImmoVlan:TechincalEmail"], int.Parse(Configuration["ImmoVlan:SoftwareId"]),
                Configuration["ImmoVlan:ProCustomerId"], Configuration["ImmoVlan:SoftwarePassword"]);
            IEnumerable<DnaProperty> properties = await _propertyRepository.GetAll();

            foreach (DnaProperty property in properties) {
                await immovlanClient.SuspendProperty(property.Id.ToString());
                logs.Add($"Property {property.Name} suspended.");

                property.UploadToImmovlan = true;
            }

            await _propertyRepository.SaveChanges();

            return Ok(logs);
        }

        /// <summary>
        /// Gets all properties from the database, checks if the property
        /// has to be uploaded to Spotto or not. If true, it will upload
        /// the property and set the status so it will not upload twice.
        /// </summary>
        /// <returns>A log list of what happend during this action (To debug)</returns>
        [HttpGet("spotto")]
        public async Task<ActionResult<IEnumerable<string>>> UploadToSpotto() {
            ICollection<string> logs = new List<string>();
            SpottoClient spottoClient = new SpottoClient(Configuration["Spotto:SubscriptionKey"], Configuration["Spotto:PartnerId"], false);

            IEnumerable<DnaProperty> propertiesToUpload = await _propertyRepository.GetAll();
            propertiesToUpload = propertiesToUpload.Where(p => p.UploadToSpotto);

            foreach (DnaProperty property in propertiesToUpload) {
                DnaSpottoProperty spottoProperty = new DnaSpottoProperty(property);

                var result = await spottoProperty.Publish(spottoClient);
                logs.Add($"UPLOADED: {property.Name} ({property.Images.Count} images) with result {result.Content}");

                property.UploadToSpotto = false;
            }

            await _propertyRepository.SaveChanges();

            // SEND EMAIL

            return Ok(logs);
        }

        /// <summary>
        /// Reset all statuses to upload to spotto.
        /// </summary>
        /// <param name="apiKey">The API admin key</param>
        /// <returns>A log list of what happend during this action (To debug)</returns>
        [HttpGet("spotto/resetstatus")]
        public async Task<ActionResult<IEnumerable<string>>> ResetStatusesSpotto(string apiKey) {
            if (apiKey != Configuration["ApiKey"])
                return BadRequest("API key is not correct.");

            ICollection<string> logs = new List<string>();
            IEnumerable<DnaProperty> properties = await _propertyRepository.GetAll();

            foreach (DnaProperty property in properties) {
                if (property.Price != null || property.Status != "Realisatie") {
                    property.UploadToSpotto = true;
                    logs.Add($"Status reset for {property.Name}");
                } else {
                    logs.Add($"Ignored {property.Name} (Price null or realisatie)");
                }
            }

            await _propertyRepository.SaveChanges();

            return Ok(logs);
        }

        /// <summary>
        /// Gets all properties from the database, checks if the property
        /// has to be sent to subscribers or not. If it has to be send, it will
        /// get all active subscribers from database, checks if the property
        /// is in the correct radius, the right price, correct status and type.
        /// If so, an email will be sent containing the property.
        /// </summary>
        /// <returns>A log list of what happend during this action (To debug)</returns>
        [HttpGet("mail")]
        public async Task<ActionResult<IEnumerable<string>>> SendNewPropertiesMail() {
            ICollection<string> logs = new List<string>();
            IEnumerable<Subscriber> subscribers = await _subscriberRepository.GetAllActive();
            IEnumerable<DnaProperty> properties = await _propertyRepository.GetAll();

            foreach (DnaProperty property in properties) {
                if (property.SendToSubscribers) {
                    property.SendToSubscribers = false;

                    ICollection<Subscriber> subscribersToSend = new List<Subscriber>();

                    logs.Add($"Found property {property.Name} to send.");

                    foreach (Subscriber sub in subscribers) {
                        if (_postalCodeManager.FindNearby(sub.Postalcode, sub.RadiusInKM).Contains(property.GetLocation()[2])
                            && property.GetPrice() >= sub.MinPrice 
                            && property.GetPrice() <= sub.MaxPrice
                            && property.Status == sub.Status
                            && property.Type == sub.Type) {
                            subscribersToSend.Add(sub);
                        }
                    }

                    if (subscribersToSend.Count() > 0) {
                        logs.Add($"Sending mail to {subscribersToSend.Count()} subscribers.");
                        await _postmarkManager.sendMail(property, subscribersToSend);
                    } else {
                        logs.Add($"No matching subscribers found to send email to.");
                    }
                }
            }

            await _propertyRepository.SaveChanges();

            return Ok(logs);
        }

        /// <summary>
        /// First we need to parse all possible properties 
        /// from our wordpress Homeo theme. This uses
        /// the BASE URL (wordpress json) and just retreives all
        /// links. The reason we just cant use this for everything
        /// is because the location is not within this information.
        ///
        /// This information does not contain custom fields.
        /// </summary>
        /// <param name="url">The URL we are going to parse</param>
        /// <param name="req">The request parameter</param>
        /// <returns>An enumerable of all property links</returns>
        private IEnumerable<string> ParseJson(string url, string req) {
            ICollection<string> links = new List<string>();

            var client = new RestClient(url);
            var request = new RestRequest(req, Method.Get);

            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };

            var queryResult = client.Execute(request);
            var myJson = JArray.Parse(queryResult.Content);

            foreach (JObject item in myJson) {
                string link = item["link"].ToString().Replace(Configuration["BaseURL_Replace"], Configuration["BaseURL"]);

                links.Add(link);
            }

            return links;
        }

        /// <summary>
        /// Adds property in the database and checks other versions,
        /// if a new one is found, it will be updated.
        /// Also keeps track of the statuses. This will mark if it has
        /// to be uploaded to Immovlan or has to be used to send emails.
        /// </summary>
        /// <param name="property">The property to insert</param>
        /// <returns>The content of the result</returns>
        private async Task<string> AddOrUpdateProperty(DnaProperty scrapedProperty) {
            DnaProperty databaseProperty = await _propertyRepository.GetByURL(scrapedProperty.URL);

            if (databaseProperty == null) {
                if (scrapedProperty.Price != null) {
                    scrapedProperty.UploadToImmovlan = true;
                    scrapedProperty.UploadToSpotto = true;
                    scrapedProperty.SendToSubscribers = true;

                    var coordinates = await _coordinatesManager.GetCoordinatesFromAddress(scrapedProperty.Location);
                    scrapedProperty.CoordinatesLat = coordinates.Lat;
                    scrapedProperty.CoordinatesLng = coordinates.Lng;

                    _propertyRepository.Add(scrapedProperty);

                    return $"ADDED: Property {scrapedProperty.Name}";
                } else {
                    return $"NO PRICE: Property {scrapedProperty.Name}";
                }
            } else {
                if (!databaseProperty.Equals(scrapedProperty)) {
                    databaseProperty.UploadToImmovlan = true;
                    databaseProperty.UploadToSpotto = true;

                    databaseProperty.Name = scrapedProperty.Name;
                    databaseProperty.Type = scrapedProperty.Type;
                    databaseProperty.Status = scrapedProperty.Status;
                    databaseProperty.Description = scrapedProperty.Description;
                    databaseProperty.Location = scrapedProperty.Location;

                    if (!string.IsNullOrWhiteSpace(scrapedProperty.Price)) {
                        databaseProperty.Price = scrapedProperty.Price;
                    }

                    databaseProperty.Energy = scrapedProperty.Energy;
                    databaseProperty.LotArea = scrapedProperty.LotArea;
                    databaseProperty.LivingArea = scrapedProperty.LivingArea;
                    databaseProperty.Rooms = scrapedProperty.Rooms;
                    databaseProperty.Bedrooms = scrapedProperty.Bedrooms;
                    databaseProperty.Bathrooms = scrapedProperty.Bathrooms;
                    databaseProperty.EPCNumber = scrapedProperty.EPCNumber;
                    databaseProperty.KatastraalInkomen = scrapedProperty.KatastraalInkomen;
                    databaseProperty.OrientatieAchtergevel = scrapedProperty.OrientatieAchtergevel;
                    databaseProperty.Elektriciteitskeuring = scrapedProperty.Elektriciteitskeuring;
                    databaseProperty.Bouwvergunning = scrapedProperty.Bouwvergunning;
                    databaseProperty.StedenbouwkundigeBestemming = scrapedProperty.StedenbouwkundigeBestemming;
                    databaseProperty.Verkavelingsvergunning = scrapedProperty.Verkavelingsvergunning;
                    databaseProperty.Dagvaarding = scrapedProperty.Dagvaarding;
                    databaseProperty.Verkooprecht = scrapedProperty.Verkooprecht;
                    databaseProperty.RisicoOverstroming = scrapedProperty.RisicoOverstroming;
                    databaseProperty.AfgebakendOverstromingsGebied = scrapedProperty.AfgebakendOverstromingsGebied;
                    databaseProperty.BuildingYear = scrapedProperty.BuildingYear;
                    databaseProperty.Voorkooprecht = scrapedProperty.Voorkooprecht;
                    databaseProperty.GScore = scrapedProperty.GScore;
                    databaseProperty.PScore = scrapedProperty.PScore;
                    
                    /*if (!databaseProperty.EqualsImages(scrapedProperty)) {
                        _propertyRepository.RemoveAllImagesFromProperty(databaseProperty.Id);

                        databaseProperty.Images = scrapedProperty.Images;
                    }*/

                    return $"UPDATED: Property {scrapedProperty.Name}";
                } else {
                    return $"ALREADY EXISTS: Property {scrapedProperty.Name}";
                }
            }
        }
    }
}
