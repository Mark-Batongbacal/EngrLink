using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace EngrLink.Models
{
    [Table("faculty")]
    public class Faculty : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("department")]
        public string Department { get; set; }

        [Column("position")]
        public string Position { get; set; }

        [Column("salary")]
        public decimal? Salary { get; set; }
    }
}
