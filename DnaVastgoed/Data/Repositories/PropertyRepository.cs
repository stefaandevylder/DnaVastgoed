using DnaVastgoed.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DnaVastgoed.Data.Repositories {

    public class PropertyRepository {

        private readonly ApplicationDbContext _context;
        private readonly DbSet<DnaProperty> _properties;
        private readonly DbSet<DnaPropertyImage> _images;

        public PropertyRepository(ApplicationDbContext context) {
            _context = context;
            _properties = context.Properties;
            _images = context.PropertyImages;
        }

        /// <summary>
        /// Get a property by id.
        /// </summary>
        /// <param name="id">The property id</param>
        /// <returns>The found property object or null</returns>
        public async Task<DnaProperty> GetById(int id) {
            return await _properties.FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Get a property by name and location.
        /// </summary>
        /// <param name="url">The property url</param>
        /// <returns>The found property object or null</returns>
        public async Task<DnaProperty> GetByURL(string url) {
            return await _properties.FirstOrDefaultAsync(p => p.URL == url);
        }

        /// <summary>
        /// Get a list of all properties.
        /// </summary>
        /// <returns>An enumerable of all properties</returns>
        public async Task<IEnumerable<DnaProperty>> GetAll() {
            return await _properties.Include(p => p.Images).ToListAsync();
        }

        /// <summary>
        /// Add a new property to the database.
        /// </summary>
        /// <param name="property">The property to add</param>
        public async void Add(DnaProperty property) {
            await _properties.AddAsync(property);
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
        public void RemoveAllImagesFromProperty(int propertyId) {
            var imagesToRemove = _properties.FirstOrDefault(p => p.Id == propertyId).Images;
            _images.RemoveRange(imagesToRemove);
        }

        /// <summary>
        /// Remove all properties from the database.
        /// </summary>
        public async void RemoveAll() {
            var propertiesToRemove = await GetAll();
            _properties.RemoveRange(propertiesToRemove);
        }

        /// <summary>
        /// Save all changes.
        /// </summary>
        public async Task SaveChanges() {
            await _context.SaveChangesAsync();
        }

    }
}
