using System;
using System.Text.Json.Serialization;
using Supabase.Extensions;
using Supabase.Postgrest.Models;

namespace EngrLink.Models
{
    public class Announcement : BaseModel
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("for_students")]
        public bool ForStudents { get; set; }

        [JsonPropertyName("for_teachers")]
        public bool ForTeachers { get; set; }
    }
}