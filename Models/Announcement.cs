﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Runtime.Serialization;

namespace EngrLink.Models
{
    [Table("announcements")]
    public class Announcement : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("announcement_message")]
        public string Announcements { get; set; }

        [Column("student_announcements")]
        public bool ForStud { get; set; }

        [Column("faculty_announcements")]
        public bool ForFac { get; set; }

        [Column("program")]
        public string Program { get; set; }
    }
}