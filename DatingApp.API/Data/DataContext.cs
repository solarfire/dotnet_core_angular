using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DataContext : DbContext // Derive from Entity Framework Core Class
    {
        /**
        Call the options form the class we are deriving from, so add  : base (options)
        */
        public DataContext(DbContextOptions<DataContext> options) : base (options)
        {
        }

        /**
        Tell the DataContext about our entities
        Value - Type / Entity
        Values - Name of Table
        */
        public DbSet<Value> Values { get; set; }
    }
}