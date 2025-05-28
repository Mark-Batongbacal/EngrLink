using EngrLink.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel; // Added for ObservableCollection

namespace EngrLink.Main_Window.Department_Chairman.SubPages;

public sealed partial class ShowGrades : Page
{
    public class StudentProfileModel : INotifyPropertyChanged
    {
        private string _name;
        private string _id;
        private string _program;
        private string _year;
        private double _gwa;
        private string _period; // Added Period for consistency
        private string _profileImageUrl;

        public string Name { get => _name; set { _name = value; OnPropertyChanged(nameof(Name)); } }
        public string Id { get => _id; set { _id = value; OnPropertyChanged(nameof(Id)); } }
        public string Program { get => _program; set { _program = value; OnPropertyChanged(nameof(Program)); } }
        public string Year { get => _year; set { _year = value; OnPropertyChanged(nameof(Year)); } }
        public double GWA { get => _gwa; set { _gwa = value; OnPropertyChanged(nameof(GWA)); } }
        public string Period { get => _period; set { _period = value; OnPropertyChanged(nameof(Period)); } }
        public string ProfileImageUrl { get => _profileImageUrl; set { _profileImageUrl = value; OnPropertyChanged(nameof(ProfileImageUrl)); } }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Changed to ObservableCollection for better UI updates
    public ObservableCollection<IndivSubjectView> SubjectViews { get; set; } = new ObservableCollection<IndivSubjectView>();
    public StudentProfileModel StudentProfile { get; set; } = new();
    private int _currentStudentId; // To store the student ID for later use

    public ShowGrades()
    {
        InitializeComponent();
        this.DataContext = this;
        StudentsListView.ItemsSource = SubjectViews;
        StudentProfile.Period = "midterm"; // Default period for editing, assuming midterm
    }

    protected async override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is int studentId)
        {
            _currentStudentId = studentId;
            Debug.WriteLine($"Navigated with Student ID: {_currentStudentId}");
            await LoadStudentAndGradesData(_currentStudentId);

        }
        else
        {
            Debug.WriteLine("Navigation parameter is not an integer or is null.");
            // Optionally, navigate back or show an error
        }
    }

    private async System.Threading.Tasks.Task LoadStudentAndGradesData(int studentId)
    {
        var client = App.SupabaseClient;

        // Load Student Profile
        try
        {
            var studentResponse = await client
                .From<Student>()
                .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, studentId)
                .Get();

            var student = studentResponse.Models.FirstOrDefault();
            if (student != null)
            {
                StudentProfile.Name = student.Name;
                StudentProfile.Program = student.Program;
                StudentProfile.Year = student.Year.ToString(); // Assuming Year is int in Student model
                StudentProfile.Id = student.Id.ToString();
                StudentProfile.ProfileImageUrl = student.ProfileImageUrl;

                Debug.WriteLine($"Student Profile Loaded: Name={StudentProfile.Name}, Program={StudentProfile.Program}");

                if (!string.IsNullOrEmpty(StudentProfile.ProfileImageUrl))
                {
                    try
                    {
                        Uri imageUri = new Uri(StudentProfile.ProfileImageUrl);
                        BitmapImage bitmapImage = new BitmapImage(imageUri);
                        StudentProfileImage.Source = bitmapImage;
                        Debug.WriteLine($"Successfully loaded image from: {StudentProfile.ProfileImageUrl}");
                    }
                    catch (UriFormatException ex)
                    {
                        Debug.WriteLine($"Invalid image URL for student {StudentProfile.Id}: {StudentProfile.ProfileImageUrl} - {ex.Message}");
                        StudentProfileImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error loading image for student {StudentProfile.Id}: {ex.Message}");
                        StudentProfileImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));
                    }
                }
                else
                {
                    StudentProfileImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));
                    Debug.WriteLine($"No profile image URL found for student {StudentProfile.Id}. Displaying placeholder.");
                }
            }
            else
            {
                Debug.WriteLine($"No student found with ID: {studentId}");
                // Handle case where student is not found (e.g., show message, navigate back)
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading student profile: {ex.Message}");
            // Handle database or network errors
        }

        // Load Grades
        try
        {
            var gradesResponse = await client
                .From<IndivSubject>()
                .Filter("student_id", Supabase.Postgrest.Constants.Operator.Equals, studentId)
                .Order("subject", Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();

            SubjectViews.Clear(); // Clear existing items before adding new ones
            foreach (var sub in gradesResponse.Models)
            {
                // For the "ShowGrades" page (for editing), ensure grades are editable
                // and set the initial period (e.g., "midterm" or the default for editing)
                SubjectViews.Add(new IndivSubjectView { Sub = sub, Period = StudentProfile.Period, IsEditable = true });
            }
            RecalculateGWA(); // Calculate GWA after loading all subjects
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading grades: {ex.Message}");
            // Handle database or network errors
        }
    }


    private async void GradePeriodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GradePeriodComboBox.SelectedItem is ComboBoxItem selectedItem &&
            selectedItem.Tag is string periodTag)
        {
            StudentProfile.Period = periodTag; // Update the StudentProfile's period
            // Instead of just calling UpdateDisplayedGrades, refresh the entire data
            // This will reload all subjects with the correct grades for the selected period
            await LoadStudentAndGradesData(_currentStudentId);
        }
    }

    private void GradeInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox && textBox.DataContext is IndivSubjectView subjectView)
        {
            string input = textBox.Text;
            int cursorPosition = textBox.SelectionStart;

            // 1. Clean the input: remove non-digits
            string cleanedInput = new string(input.Where(char.IsDigit).ToArray());

            // Prevent infinite loop if cleaning causes TextChanged again
            if (textBox.Text != cleanedInput)
            {
                textBox.Text = cleanedInput;
                // Adjust cursor position to account for removed characters
                textBox.SelectionStart = Math.Min(cursorPosition, textBox.Text.Length);
                return; // Exit and wait for the new TextChanged event with cleaned text
            }

            // 2. Handle empty string after cleaning
            if (string.IsNullOrEmpty(cleanedInput))
            {
                SetSubjectGradeAndRemarks(subjectView, 0, false); // Set to 0/Failed
                RecalculateGWA();
                return;
            }

            int grade;
            bool gradeSetToModel = false; // Flag to track if the grade was successfully set to the model

            if (int.TryParse(cleanedInput, out grade))
            {
                if (cleanedInput.Length == 1)
                {
                    if (grade < 6) // User cannot input 5 or lower as a single digit
                    {
                        textBox.Text = string.Empty; // Clear the text box
                        SetSubjectGradeAndRemarks(subjectView, 0, false); // Reset model grade
                    }
                    else
                    {
                        // If first digit is 6-9, it's a valid start, but not a complete grade.
                        // Do not clear the textbox, but also do NOT update the model's grade with this partial input.
                        // The model's grade remains 0 (or whatever it was previously) until a full valid grade is entered.
                        SetSubjectGradeAndRemarks(subjectView, 0, false); // Explicitly ensure model is 0 for partial invalid
                    }
                }
                else if (cleanedInput.Length == 2)
                {
                    if (grade >= 65 && grade <= 99) // Valid two-digit grades
                    {
                        SetSubjectGradeAndRemarks(subjectView, grade, grade >= 75);
                        gradeSetToModel = true;
                    }
                    else
                    {
                        textBox.Text = string.Empty; // Clear if outside 65-99 for two digits
                        SetSubjectGradeAndRemarks(subjectView, 0, false); // Reset model grade
                    }
                }

                else // Any other length (e.g., >3 digits)
                {
                    textBox.Text = string.Empty; // Clear for invalid length
                    SetSubjectGradeAndRemarks(subjectView, 0, false); // Reset model grade
                }
            }
            else
            {
                // This handles cases where `cleanedInput` is not a valid integer (e.g., too large for int, though cleanedInput should prevent non-digits)
                textBox.Text = string.Empty;
                SetSubjectGradeAndRemarks(subjectView, 0, false);
            }

            // Recalculate GWA after any potential grade change or clearing
            RecalculateGWA();

            // Restore cursor position only if the textbox was not cleared
            if (!string.IsNullOrEmpty(textBox.Text))
            {
                textBox.SelectionStart = cursorPosition;
            }
            else
            {
                // If cleared, set cursor to start
                textBox.SelectionStart = 0;
            }
        }
    }


    private void RecalculateGWA()
    {
        double totalUnits = 0;
        double totalWeightedGrades = 0;

        foreach (var view in SubjectViews)
        {
            // Use the DisplayedGrade from the view, which dynamically shows midterm or final
            double grade = view.DisplayedGrade;

            // Only include subjects with a grade entered for GWA calculation
            // Assuming 0 means not yet graded or invalid input for GWA purposes.
            // If 0 is a valid failing grade to be included in GWA, remove `grade > 0` check.
            if (grade > 0)
            {
                totalUnits += view.Sub.Units;
                totalWeightedGrades += grade * view.Sub.Units;
            }
        }

        StudentProfile.GWA = totalUnits > 0
            ? Math.Round(totalWeightedGrades / totalUnits, 2)
            : 0;
    }

    /// <summary>
    /// Saves all current grades in SubjectViews to the Supabase database based on the selected period.
    /// </summary>
    /// <returns>True if all grades were saved successfully, false if any error occurred.</returns>
    public async System.Threading.Tasks.Task<bool> SaveGradesToDatabase()
    {
        var client = App.SupabaseClient;
        bool hasError = false;

        foreach (var view in SubjectViews)
        {
            try
            {
                // Determine which grade and remarks to save based on the currently selected period
                if (StudentProfile.Period == "final")
                {
                    await client
                        .From<IndivSubject>()
                        .Where(x => x.Eme == view.Sub.Eme)
                        .Set(x => x.Grade_F, view.Sub.Grade_F)
                        .Set(x => x.Remarks_F, view.Sub.Grade_F >= 75) // Update final remarks based on final grade
                        .Update();
                    Debug.WriteLine($"Updated subject {view.Sub.Subject} (Final Grade) with grade {view.Sub.Grade_F}");
                }
                else // Assuming "midterm" or any other period defaults to Grade
                {
                    await client
                        .From<IndivSubject>()
                        .Where(x => x.Eme == view.Sub.Eme)
                        .Set(x => x.Grade, view.Sub.Grade)
                        .Set(x => x.Remarks, view.Sub.Grade >= 75) // Update midterm remarks based on midterm grade
                        .Update();
                    Debug.WriteLine($"Updated subject {view.Sub.Subject} (Midterm Grade) with grade {view.Sub.Grade}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating subject {view.Sub.Subject}: {ex.Message}");
                hasError = true;
            }
        }
        return hasError;
    }


    private async void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            button.IsEnabled = false; // Disable button to prevent multiple clicks
        }

        // Call the new centralized save function
        bool hasError = await SaveGradesToDatabase();

        // After saving, reload the data to ensure the remarks are updated in the UI
        await LoadStudentAndGradesData(_currentStudentId);

        RecalculateGWA(); // Recalculate GWA after reloading, in case any grades were set to 0 or changed.

        var dialog = new ContentDialog
        {
            Title = hasError ? "Some Errors Occurred" : "Grades Saved Successfully",
            Content = hasError
                ? "Some grades may not have been saved. Please check the debug output or try again."
                : "All grades have been successfully saved to the database.",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot // Essential for ContentDialog to appear correctly in WinUI 3
        };

        await dialog.ShowAsync();

        if (button != null)
        {
            button.IsEnabled = true; // Re-enable button
        }
    }


    // Helper method to reduce duplication
    private void SetSubjectGradeAndRemarks(IndivSubjectView subjectView, int gradeValue, bool remarksValue)
    {
        // Only update if the value is actually different to avoid unnecessary PropertyChanged events
        if (subjectView.Period == "final")
        {
            if (subjectView.Sub.Grade_F != gradeValue || subjectView.Sub.Remarks_F != remarksValue)
            {
                subjectView.Sub.Grade_F = gradeValue;
                subjectView.Sub.Remarks_F = remarksValue;
                subjectView.OnPropertyChanged(nameof(subjectView.DisplayedGrade));
                subjectView.OnPropertyChanged(nameof(subjectView.RemarksText));
            }
        }
        else // Midterm
        {
            if (subjectView.Sub.Grade != gradeValue || subjectView.Sub.Remarks != remarksValue)
            {
                subjectView.Sub.Grade = gradeValue;
                subjectView.Sub.Remarks = remarksValue;
                subjectView.OnPropertyChanged(nameof(subjectView.DisplayedGrade));
                subjectView.OnPropertyChanged(nameof(subjectView.RemarksText));
            }
        }
    }
}