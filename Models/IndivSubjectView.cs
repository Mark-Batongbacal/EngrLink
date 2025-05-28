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
using System.Diagnostics;

namespace EngrLink.Models
{
    public class IndivSubjectView : INotifyPropertyChanged
    {
        private IndivSubject _sub;
        private double _displayedGrade;
        private string _period = "midterm"; // Changed default to "midterm" for consistency with ShowGrades

        public IndivSubject Sub
        {
            get => _sub;
            set
            {
                if (_sub != value)
                {
                    _sub = value;
                    OnPropertyChanged(nameof(Sub));
                    // Call UpdateDisplayedGrade here to ensure it's set when Sub changes
                    UpdateDisplayedGrade();
                    OnPropertyChanged(nameof(RemarksText));
                }
            }
        }

        public string Period
        {
            get => _period;
            set
            {
                if (_period != value)
                {
                    _period = value;
                    // Ensure displayed grade is updated when the period changes
                    UpdateDisplayedGrade();
                    OnPropertyChanged(nameof(Period));
                    OnPropertyChanged(nameof(RemarksText));
                }
            }
        }

        public void UpdateDisplayedGrade()
        {
            if (Sub == null)
            {
                DisplayedGrade = 0; // Default or handle null Sub
                return;
            }
            // Ensure DisplayedGrade setter is used, which updates _displayedGrade and triggers PropertyChanged
            DisplayedGrade = Period == "final" ? Sub.Grade_F : Sub.Grade;
        }

        public string RemarksText
        {
            get
            {
                if (Sub == null) return "N/A"; // Handle null Sub
                return Period == "final"
                    ? (Sub.Remarks_F == true ? "Passed" : "Failed")
                    : (Sub.Remarks == true ? "Passed" : "Failed");
            }
        }

        public double DisplayedGrade
        {
            get => _displayedGrade;
            set
            {
                if (_displayedGrade != value)
                {
                    _displayedGrade = value;
                    if (Sub != null)
                    {
                        if (Period == "final")
                            Sub.Grade_F = (int)value;
                        else
                            Sub.Grade = (int)value;
                    }
                    OnPropertyChanged(nameof(DisplayedGrade));
                    OnPropertyChanged(nameof(RemarksText)); // Ensure remarks update when grade changes
                }
            }
        }

        public bool IsEditable { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) =>
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

        [Column("passed_f")]
        public bool Remarks_F { get; set; }
    }
}