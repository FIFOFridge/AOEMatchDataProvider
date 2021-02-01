using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AOEMatchDataProvider.Converters.Logic
{
    //allows to "pipe" (output value is passed as input value for each next converting operation) value with multiple converters
    //based on: https://stackoverflow.com/a/48503354
    public class ConverterPipe : List<IValueConverter>, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object result = null;
            IValueConverter lastValueConverter = this.First();
            object lastInput = value;
            int lastIndex = 0;

            try
            {
                result = this.Aggregate(value, (current, converter) =>
                {
                    lastInput = current;
                    lastValueConverter = converter;
                    lastIndex++;

                    var converted = converter.Convert(current, targetType, parameter, culture);
                    return converted;
                });
            } 
            catch(Exception e)
            {
                if (
                    e is StackOverflowException ||
                    e is ThreadAbortException ||
                    e is AccessViolationException
                    )
                {
                    throw e; //rethrow without wrapping
                }

                //processing exception
                throw new ConverterPipeException(
                    $"Exception occurred while performing conversion: converter {lastValueConverter.GetType()}, converter input: {lastInput}, converter index: {lastIndex}",
                    e
                    );
            }

            return result;
        }

        //todo: implement general method for pipe processing
        //void Process(object initialValue, IEnumerable<IValueConverter> valueConverters, string modeDescriptor)
        //{

        //}

        //todo: implement convert back
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConverterPipeException : Exception
    {
        public ConverterPipeException()
        {
        }

        public ConverterPipeException(string message) : base(message)
        {
        }

        public ConverterPipeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ConverterPipeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
