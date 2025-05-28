using Supabase;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
namespace EngrLink.Models
{
    public class IndivSubjectView
    {
        private IndivSubject _sub;
        private double _displayedGrade;

        public IndivSubject Sub
        {
            get => _sub;
            set
            {
                if (_sub != value)
                {
                    _sub = value;
                    OnPropertyChanged(nameof(Sub));
                    OnPropertyChanged(nameof(RemarksText));
                }
            }
        }

        public string RemarksText
        {
            get => Sub?.Remarks == true ? "Passed" : "Failed";
        }

        public double DisplayedGrade
        {
            get => _displayedGrade;
            set
            {
                if (_displayedGrade != value)
                {
                    _displayedGrade = value;
                    OnPropertyChanged(nameof(DisplayedGrade));
                }
            }
        }
        public bool IsEditable { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        [Column("grade_f")]
        public int Grade_F { get; set; }
    }
}