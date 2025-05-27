using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace EngrLink.Models
{
    [Table("Faculty_Info")]
    public class Faculty : BaseModel
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("profcode")]
        public string ProfCode { get; set; }

        [Column("program")]
        public string Program { get; set; }
    }
}
