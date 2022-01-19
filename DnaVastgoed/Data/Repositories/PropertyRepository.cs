using DnaVastgoed.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DnaVastgoed.Data.Repositories {

    public class PropertyRepository {

        private readonly ApplicationDbContext _context;
        private readonly DbSet<DnaProperty> _properties;

        public PropertyRepository(ApplicationDbContext context) {
            _context = context;
            _properties = context.Properties;
        }

        /// <summary>
        /// Get a property by name and location.
        /// </summary>
        /// <param name="url">The property url</param>
        /// <returns>The found property object or null</returns>
        public DnaProperty GetByURL(string url) {
            return _properties.FirstOrDefault(p => p.URL == url);
        }

        /// <summary>
        /// Get a list of all properties.
        /// </summary>
        /// <returns>An enumerable of all properties</returns>
        public IEnumerable<DnaProperty> GetAll() {
            return _properties.Include(p => p.Images);
        }

        /// <summary>
        /// Add a new property to the database.
        /// </summary>
        /// <param name="property">The property to add</param>
        public void Add(DnaProperty property) {
            _properties.Add(property);
        }

        /// <summary>
        /// Remove a property from the database.
        /// </summary>
        /// <param name="property">The property we want to remove</param>
        public void Remove(DnaProperty property) {
            _properties.Remove(property);
        }

        /// <summary>
        /// Remove all properties from the database.
        /// </summary>
        public void RemoveAll() {
            _properties.RemoveRange(GetAll());
        }

        /// <summary>
        /// Save all changes.
        /// </summary>
        public void SaveChanges() {
            _context.SaveChanges();
        }

    }
}
