using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace EngrLink.Models
{
    [Table("Student_Subjects")]
    public class Subjects : BaseModel
    {
        [PrimaryKey("record_id", true)]
        public int Eme { get; set; }

        [Column("student_id")]
        public int Id { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("Subject")]
        public string Subject { get; set; }

        [Column("grade")]
        public int Grade { get; set; }

        [Column("passed")]
        public bool Remarks { get; set; }
        
        [Column("units")]
        public int Units { get; set; }
    }

    public class IdParser
    {
        public int Id { get; set; }
    }
}
