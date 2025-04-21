using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;


namespace EngrLink.Models
{
    [Table("Student_Info")]
    public class Student : BaseModel
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("address")]
        public string Address { get; set; }

        [Column("contact")]
        public string Contact { get; set; }

        [Column("year")]
        public int Year { get; set; }

        [Column("program")]
        public string Program { get; set; }

        [Column("fees")]
        public int Fees { get; set; }

        [Column("enrolled")]
        public bool Enrolled { get; set; }

        [Column("paid")]
        public bool Paid { get; set; }

        [Column("regular")]
        public bool Regular { get; set; }

        [Column("bday")]
        public string Birthday { get; set; }
    }
}


