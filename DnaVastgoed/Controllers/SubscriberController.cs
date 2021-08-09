using DnaVastgoed.Data.Repositories;
using DnaVastgoed.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace DnaVastgoed.Controllers {

    [ApiController]
    [Route("[controller]")]
    public class SubscriberController : ControllerBase {

        private readonly SubscriberRepository _subscriberRepository;

        public SubscriberController(SubscriberRepository subscriberRepository) {
            _subscriberRepository = subscriberRepository;
        }

        /// <summary>
        /// Get basic information about subscribers, this is public info.
        /// </summary>
        /// <returns>Public basic subscriber information</returns>
        [HttpGet]
        public ActionResult<string> GetInformation() {
            int activeSubscribers = _subscriberRepository.GetAllActive().Count();
            return Ok($"There are {activeSubscribers} active subscribers.");
        }

        /// <summary>
        /// Add a new subscriber to the email list.
        /// </summary>
        /// <param name="subscriber">The subscriber object</param>
        /// <returns>The status</returns>
        [HttpPost("add")]
        public ActionResult<string> AddSubscriber([FromForm] Subscriber subscriber) {
            if (_subscriberRepository.Get(subscriber.Email) != null) {
                return BadRequest("Already exists");
            }

            subscriber.Suppressed = null;

            _subscriberRepository.Add(subscriber);
            _subscriberRepository.SaveChanges();

            return Ok($"Added new subscriber: {subscriber.Email}");
        }

        /// <summary>
        /// Delete a subscriber from the mailing list.
        /// </summary>
        /// <param name="webhook">The webhook event</param>
        /// <returns>The status</returns>
        [HttpPost("delete")]
        public ActionResult<string> RemoveSubscriber(PostmarkWebhook webhook) {
            Subscriber subscriber = _subscriberRepository.Get(webhook.Recipient);

            if (subscriber == null)
                return NotFound();

            if (webhook.SuppressSending) {
                subscriber.Suppressed = DateTime.Now;
            } else {
                subscriber.Suppressed = null;
            }

            _subscriberRepository.SaveChanges();

            return Ok($"Removed subscriber: ${subscriber.Email}");
        }
    }
}
