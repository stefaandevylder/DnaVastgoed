using DnaVastgoed.Models;
using Microsoft.EntityFrameworkCore;

namespace DnaVastgoed.Data {

    public class ApplicationDbContext : DbContext {

        public DbSet<DnaProperty> Properties { get; set; }
        public DbSet<Subscriber> Subscribers { get; set; }

        /// <summary>
        /// Configure the database.
        /// </summary>
        /// <param name="options">Any context options</param>
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=../Database/DnaVastgoedDatabase.db");
    }

}
