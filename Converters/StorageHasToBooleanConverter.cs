using AOEMatchDataProvider.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AOEMatchDataProvider.Converters
{
    public class StorageHasToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IStorageService)
                throw new InvalidOperationException("Value isn't implementing IStorageService");

            if (parameter.GetType() != typeof(string))
                throw new InvalidOperationException("Parameter (storage key) has to be string");

            if ((value as IStorageService).Has(parameter as string))
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
