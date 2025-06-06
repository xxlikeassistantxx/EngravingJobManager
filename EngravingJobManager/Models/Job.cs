// Add this using statement if it's not already there, though explicit qualification below is key
// using SQLite; // This helps but explicit qualification for attributes is safer for ambiguity

using System.Collections.Generic; // Keep this
// Remove 'using System.ComponentModel.DataAnnotations.Schema;' if it was added by mistake.
// Remove 'using System.Diagnostics.CodeAnalysis;' if it was added by mistake.


namespace EngravingJobManager.Models
{
    // Explicitly use SQLite.TableAttribute
    [SQLite.Table("Jobs")] // MODIFIED HERE
    public class Job
    {
        // SQLite.PrimaryKey and SQLite.AutoIncrement are usually specific enough
        // but if PrimaryKeyAttribute or AutoIncrementAttribute also became ambiguous,
        // you'd qualify them as SQLite.PrimaryKeyAttribute, SQLite.AutoIncrementAttribute
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int Id { get; set; }

        // Explicitly use SQLite.NotNullAttribute
        [SQLite.NotNull] // MODIFIED HERE (assuming this is line 14 from the error)
        public string Title { get; set; }

        public string CustomerName { get; set; }
        public string CustomerOrganization { get; set; }
        public string PhoneNumber { get; set; }
        public string Details { get; set; }

        public DateTime DateCreated { get; set; }

        [SQLite.Ignore] // MODIFIED HERE (Good practice to qualify all SQLite attributes)
        public List<PhotoItem> Photos { get; set; } = new List<PhotoItem>();

        public bool IsDeleted { get; set; }

        public Job()
        {
            DateCreated = DateTime.Now;
        }
    }
}