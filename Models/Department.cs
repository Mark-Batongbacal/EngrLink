using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace EngrLink.Models
{

    [Table("DepChair_Info")]
    public class DeptChair : BaseModel
    {
        [PrimaryKey("record_id", true)]
        public int Record_Id { get; set; }

        [Column("id")]
        public string Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("program")]
        public string Program { get; set; }

        [Column("password")]
        public string Password { get; set; }

    }
}
