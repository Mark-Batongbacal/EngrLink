using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace EngrLink.Models
{
    [Table("announcements")] // Make sure this matches your Supabase table name
    public class Announcement : BaseModel // Inherit from BaseModel
    {
        [PrimaryKey("id", false)] // Adjust 'false' to 'true' if your ID is auto-incrementing
        public int Id { get; set; }

        [Column("title")]
        public string Title { get; set; }   

        [Column("content")]
        public string Content { get; set; }

        [Column("created_at")] // Make sure this matches your Supabase column name
        public DateTime CreatedAt { get; set; }

        // Add other columns as needed, with [Column("your_column_name")] attributes
    }
}