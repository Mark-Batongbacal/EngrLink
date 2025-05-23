﻿using System;
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

    public class StudentViewModel
    {
        public Student Student2 { get; set; }

        public string NameStatus => $"{Student2.Name} - {(Student2.Regular ? "Regular" : "Irregular")}";
    }


    [Table("Student_Info")]
    public class Student : BaseModel
    {
        [PrimaryKey("id", true)]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("address")]
        public string Address { get; set; }

        [Column("contact")]
        public string Contact { get; set; }

        [Column("year")]
        public string Year { get; set; }

        [Column("program")]
        public string Program { get; set; }

        [Column("fees")]
        public int? Fees { get; set; }

        [Column("total_fees")]
        public int? Total { get; set; }

        [Column("enrolled")]
        public bool Enrolled { get; set; }

        [Column("paid")]
        public bool Paid { get; set; }

        [Column("regular")]
        public bool Regular { get; set; }

        [Column("bday")]
        public string Birthday { get; set; }

        [Column("password")]
        public string Password { get; set; }
    }
}


