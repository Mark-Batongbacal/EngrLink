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

        [Column("year")]
        public string Year { get; set; }

        [Column("schedule")]
        public string Schedule { get; set; }

        [Column("program")]
        public string Program { get; set; }
        public List<ScheduleDetail> ScheduleDetails { get; set; }
    }

    public class ScheduleDetail
    {
        public string Day { get; set; }
        public string Time { get; set; }
        public string Subject { get; set; }

    }
}
