// Add this using statement if it's not already there for SQLite attributes
// using SQLite; // This helps but explicit qualification for attributes is safer for ambiguity

// Remove 'using System.ComponentModel.DataAnnotations.Schema;' if it was added by mistake.

namespace EngravingJobManager.Models
{
    // Explicitly use SQLite.TableAttribute
    [SQLite.Table("PhotoItems")] // MODIFIED HERE
    public class PhotoItem
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement] // MODIFIED HERE (Good practice)
        public int Id { get; set; }

        public int JobId { get; set; }
        public string PhotoPath { get; set; }
    }
}