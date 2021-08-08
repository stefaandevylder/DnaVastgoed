using DnaVastgoed.Models;
using PostmarkDotNet;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DnaVastgoed.Managers {

    public class PostmarkManager {

        private readonly PostmarkClient _client;

        public PostmarkManager(string apiKey) {
            _client = new PostmarkClient(apiKey);
        }

        /// <summary>
        /// Send the property to the right subscribers.
        /// </summary>
        /// <param name="property">The new property</param>
        /// <param name="subscribers">The subscribers</param>
        public async Task sendMail(DnaProperty property, IEnumerable<Subscriber> subscribers) {
            ICollection<TemplatedPostmarkMessage> messages = new List<TemplatedPostmarkMessage>();

            foreach (Subscriber sub in subscribers) {
                var message = new TemplatedPostmarkMessage();

                message.From = "info@dnavastgoed.be";
                message.MessageStream = "broadcast";
                message.To = sub.Email;

                message.TemplateId = 24345379;
                message.TemplateModel = new {
                    Fullname = sub.Firstname + " " + sub.Lastname,
                    Link = property.URL
                };

                messages.Add(message);
            }

            await _client.SendMessagesAsync(messages.ToArray());
        }

    }

}
