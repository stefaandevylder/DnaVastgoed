using DnaVastgoed.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<Subscriber> Get(string email) {
            return await _subscribers.FirstOrDefaultAsync(s => s.Email.ToLower().Trim() == email.ToLower().Trim());
        }

        /// <summary>
        /// Get all active subscribers.
        /// </summary>
        /// <returns>An enumerable of all subscriber objects</returns>
        public async Task<IEnumerable<Subscriber>> GetAllActive() {
            return await _subscribers.Where(s => s.Suppressed == null).ToListAsync();
        }

        /// <summary>
        /// Add a new subscriber to the database.
        /// </summary>
        /// <param name="subscriber">The subscriber object to add</param>
        public async void Add(Subscriber subscriber) {
            await _subscribers.AddAsync(subscriber);
        }

        /// <summary>
        /// Save all changes.
        /// </summary>
        public async void SaveChanges() {
            await _context.SaveChangesAsync();
        }

    }

}
