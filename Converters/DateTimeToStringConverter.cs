using Microsoft.UI.Xaml.Data;
using System;
using System.Globalization;

namespace EngrLink.Converters // Correct the namespace to match your project structure
{
    public class DateTimeToStringConverter : IValueConverter
    {
        public string Format { get; set; } = "yyyy-MM-dd HH:mm"; // Default format

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dateTime)
            {
                // Handle DateTime
                return dateTime.ToString(Format, CultureInfo.InvariantCulture);
            }
            else if (value is DateTimeOffset dateTimeOffset)
            {
                // Handle DateTimeOffset, convert it to DateTime first
                return dateTimeOffset.DateTime.ToString(Format, CultureInfo.InvariantCulture);
            }

            return string.Empty; // Return an empty string if the value is not DateTime or DateTimeOffset
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
