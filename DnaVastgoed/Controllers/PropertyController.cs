using AngleSharp.Html.Parser;
using DnaVastgoed.Data.Repositories;
using DnaVastgoed.Managers;
using DnaVastgoed.Models;
using DnaVastgoed.Network;
using ImmoVlanAPI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DnaVastgoed.Controllers {

    [ApiController]
    [Route("[controller]")]
    public class PropertyController : ControllerBase {

        public IConfiguration Configuration;

        private readonly PostmarkManager _postmarkManager;
        private readonly PostalCodeManager _postalCodeManager;

        private readonly PropertyRepository _propertyRepository;
        private readonly SubscriberRepository _subscriberRepository;

        public PropertyController(IConfiguration configuration, PostmarkManager postmarkManager, PostalCodeManager postalCodeManager,
            PropertyRepository propertyRepository, SubscriberRepository subscriberRepository) {
            Configuration = configuration;

            _postmarkManager = postmarkManager;
            _postalCodeManager = postalCodeManager;

            _propertyRepository = propertyRepository;
            _subscriberRepository = subscriberRepository;
        }

        /// <summary>
        /// Gets a list of all properties in the database.
        /// </summary>
        /// <returns>A list of DnaProperty objects</returns>
        [HttpGet]
        public ActionResult<IEnumerable<DnaProperty>> GetProperties() {
            return Ok(_propertyRepository.GetAll());
        }

        /// <summary>
        /// Scrape the BaseURL for a list of all properties. After that
        /// it will scrape each URL and save it in the database.
        /// </summary>
        /// <returns>A log list of what happend during this action (To debug)</returns>
        [HttpGet("scrape")]
        public ActionResult<IEnumerable<string>> Scrape() {
            ICollection<string> logs = new List<string>();
            IEnumerable<string> links = ParseJson(Configuration["BaseURL"], "/wp-json/wp/v2/property?per_page=100&orderby=date");

            HtmlParser parser = new HtmlParser();

            foreach (string link in links) {
                var client = new RestClient(link);
                var request = new RestRequest(Method.GET);
                var queryResult = client.Execute(request);

                DnaProperty property = new DnaProperty(
                    parser.ParseDocument(queryResult.Content),
                    link,
                    Configuration["BaseURL_Replace"],
                    Configuration["BaseURL"]);

                logs.Add(AddOrUpdateProperty(property));
            }

            _propertyRepository.SaveChanges();

            return Ok(logs);
        }

        /// <summary>
        /// Gets all properties from the database, checks if the property
        /// has to be uploaded to Immovlan or not. If true, it will upload
        /// the property and set the status so it will not upload twice.
        /// </summary>
        /// <param name="staging">Wether it has to be send to the production servers or debug servers</param>
        /// <returns>A log list of what happend during this action (To debug)</returns>
        [HttpGet("immovlan")]
        public async Task<ActionResult<IEnumerable<string>>> UploadToImmovlan(bool staging = true) {
            ICollection<string> logs = new List<string>();
            ICollection<DnaProperty> propertiesUploaded = new List<DnaProperty>();
            ImmoVlanClient immovlanClient = new ImmoVlanClient(Configuration["ImmoVlan:BusinessEmail"],
                Configuration["ImmoVlan:TechincalEmail"], int.Parse(Configuration["ImmoVlan:SoftwareId"]),
                Configuration["ImmoVlan:ProCustomerId"], Configuration["ImmoVlan:SoftwarePassword"], staging);

            if (staging) logs.Add("Staging is on.");

            foreach (DnaProperty property in _propertyRepository.GetAll()) {
                if (property.UploadToImmovlan) {
                    property.UploadToImmovlan = false;

                    var result = new ImmoVlanProperty(property).Publish(immovlanClient);
                    logs.Add($"UPLOADED: {property.Name} ({property.Images.Count()} images) with result {result.Content}");
                    propertiesUploaded.Add(property);
                }
            }

            _propertyRepository.SaveChanges();

            await _postmarkManager.sendUploadedImmoVlan(propertiesUploaded);

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
            IEnumerable<Subscriber> subscribers = _subscriberRepository.GetAllActive();

            foreach (DnaProperty property in _propertyRepository.GetAll()) {
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

            _propertyRepository.SaveChanges();

            return Ok(logs);
        }

        /// <summary>
        /// Reset all statuses to upload to immovlan.
        /// </summary>
        /// <param name="apiKey">The API admin key</param>
        /// <returns>A log list of what happend during this action (To debug)</returns>
        [HttpGet("resetstatus")]
        public ActionResult<IEnumerable<string>> ResetStatuses(string apiKey) {
            if (apiKey != Configuration["ApiKey"])
                return BadRequest("API key does not exist.");
            
            ICollection<string> logs = new List<string>();

            foreach (DnaProperty property in _propertyRepository.GetAll()) {
                property.UploadToImmovlan = true;
                logs.Add($"Status reset for {property.Name}");
            }

            _propertyRepository.SaveChanges();

            return Ok(logs);
        }

        /// <summary>
        /// Suspend all properties.
        /// </summary>
        /// <param name="apiKey">The API admin key</param>
        /// <returns>A log list of what happend during this action (To debug)</returns>
        [HttpGet("suspend/all")]
        public ActionResult<IEnumerable<string>> SuspendAllProperties(string apiKey) {
            if (apiKey != Configuration["ApiKey"])
                return BadRequest("API key does not exist.");

            ICollection<string> logs = new List<string>();
            ImmoVlanClient immovlanClient = new ImmoVlanClient(Configuration["ImmoVlan:BusinessEmail"],
                Configuration["ImmoVlan:TechincalEmail"], int.Parse(Configuration["ImmoVlan:SoftwareId"]),
                Configuration["ImmoVlan:ProCustomerId"], Configuration["ImmoVlan:SoftwarePassword"]);

            foreach (DnaProperty property in _propertyRepository.GetAll()) {
                immovlanClient.SuspendProperty(property.Id.ToString());
                logs.Add($"Property {property.Name} suspended.");
            }

            _propertyRepository.RemoveAll();
            _propertyRepository.SaveChanges();

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
            var request = new RestRequest(req, Method.GET);

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
        private string AddOrUpdateProperty(DnaProperty property) {
            DnaProperty propertyFound = _propertyRepository.GetByURL(property.URL);

            if (propertyFound == null) {
                if (property.Price != null) {
                    property.UploadToImmovlan = true;
                    property.SendToSubscribers = true;

                    _propertyRepository.Add(property);

                    return $"ADDED: Property {property.Name}";
                } else {
                    return $"NO PRICE: Property {property.Name}";
                }
            } else {
                if (!property.Equals(propertyFound)) {
                    propertyFound.UploadToImmovlan = true;

                    propertyFound.Name = property.Name;
                    propertyFound.Type = property.Type;
                    propertyFound.Status = property.Status;
                    propertyFound.Description = property.Description;
                    propertyFound.Location = property.Location;
                    propertyFound.Price = property.Price;
                    propertyFound.Energy = property.Energy;
                    propertyFound.LotArea = property.LotArea;
                    propertyFound.LivingArea = property.LivingArea;
                    propertyFound.Rooms = property.Rooms;
                    propertyFound.Bedrooms = property.Bedrooms;
                    propertyFound.Bathrooms = property.Bathrooms;
                    propertyFound.EPCNumber = property.EPCNumber;
                    propertyFound.KatastraalInkomen = property.KatastraalInkomen;
                    propertyFound.OrientatieAchtergevel = property.OrientatieAchtergevel;
                    propertyFound.Elektriciteitskeuring = property.Elektriciteitskeuring;
                    propertyFound.Bouwvergunning = property.Bouwvergunning;
                    propertyFound.StedenbouwkundigeBestemming = property.StedenbouwkundigeBestemming;
                    propertyFound.Verkavelingsvergunning = property.Verkavelingsvergunning;
                    propertyFound.Dagvaarding = property.Dagvaarding;
                    propertyFound.Verkooprecht = property.Verkooprecht;
                    propertyFound.RisicoOverstroming = property.RisicoOverstroming;
                    propertyFound.AfgebakendOverstromingsGebied = property.AfgebakendOverstromingsGebied;
                    propertyFound.Images = property.Images;

                    return $"UPDATED: Property {property.Name}";
                } else {
                    return $"ALREADY EXISTS: Property {property.Name}";
                }
            }
        }
    }
}
