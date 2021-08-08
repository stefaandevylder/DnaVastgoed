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
        /// Get a property by ID.
        /// </summary>
        /// <param name="propertyName">The property name</param>
        /// <returns>The found property object or null</returns>
        public DnaProperty Get(string propertyName) {
            return _properties.FirstOrDefault(p => p.Name.ToLower() == propertyName.ToLower());
        }

        /// <summary>
        /// Get a list of all properties.
        /// </summary>
        /// <returns>An enumerable of all properties</returns>
        public IEnumerable<DnaProperty> GetAll() {
            return _properties.ToList();
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
        /// Save all changes.
        /// </summary>
        public void SaveChanges() {
            _context.SaveChanges();
        }

    }
}
