using DnaVastgoed.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DnaVastgoed.Data.Repositories {

    public class SubscriberRepository {

        private readonly ApplicationDbContext _context;
        private readonly DbSet<Subscriber> _subscribers;

        public SubscriberRepository(ApplicationDbContext context) {
            _context = context;
            _subscribers = context.Subscribers;
        }

        /// <summary>
        /// Get a subscriber object by email.
        /// </summary>
        /// <param name="email">The email to search for</param>
        /// <returns>A subscriber object</returns>
        public Subscriber Get(string email) {
            return _subscribers.FirstOrDefault(s => s.Email.ToLower().Trim() == email.ToLower().Trim());
        }

        /// <summary>
        /// Get all active subscribers.
        /// </summary>
        /// <returns>An enumerable of all subscriber objects</returns>
        public IEnumerable<Subscriber> GetAllActive() {
            return _subscribers.Where(s => s.Suppressed == null).ToList();
        }

        /// <summary>
        /// Add a new subscriber to the database.
        /// </summary>
        /// <param name="subscriber">The subscriber object to add</param>
        public void Add(Subscriber subscriber) {
            _subscribers.Add(subscriber);
        }

        /// <summary>
        /// Save all changes.
        /// </summary>
        public void SaveChanges() {
            _context.SaveChanges();
        }

    }

}
