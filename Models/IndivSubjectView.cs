using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Supabase;
using System.Text.Json.Serialization;
namespace EngrLink.Models
{
    public class IndivSubjectView
    {
        public IndivSubject Sub { get; set; }
        public string RemarksText => Sub?.Remarks == true ? "Passed" : "Failed";
    }

    [Table("Individual_Subjects")]
    public class IndivSubject : BaseModel
    {
        [PrimaryKey("record_id", false)]
        public int Eme { get; set; }

        [Column("student_id")]
        public int Id { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("subject")]
        public string Subject { get; set; }

        [Column("grade")]
        public int Grade { get; set; }

        [Column("passed")]
        public bool Remarks { get; set; }

        [Column("program")]
        public string Program { get; set; }

        [Column("year")]
        public string Year { get; set; }

        [Column("units")]
        public int Units { get; set; }

        [Column("schedule")]
        public string Schedule { get; set; }

        [Column("profcode")]
        public string ProfCode { get; set; }

    }
}