using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Models
{
    internal static class ModelDeserializationHelper
    {
        internal static object ConstcructModel(string JSON, string modelType)
        {
            var currentDomain = AppDomain.CurrentDomain;

            Type targetType = null;

            foreach (var assembly in currentDomain.GetAssemblies())
            {
                targetType = assembly.GetType(modelType);

                if (targetType != null)
                    break;
            }

            //dynamicly create model instance based by its type name
            object modelInstance = Activator.CreateInstance(targetType);

            if (!(modelInstance is ISerializableModel))
                throw new InvalidOperationException($"Model: {modelType} don't implement ISerializableModel interface.");// + modelType);

            //execute ISerializable method to fill/reconstruct specified model from JSON string
            return (modelInstance as ISerializableModel).FromJSON(JSON);

            ////allow to skip ISerializableModel for primitive types
            //if (targetType.IsPrimitive)
            //    return JsonConvert.DeserializeObject<dynamic>(JSON);

            //throw new InvalidOperationException($"Model: {modelType} don't implement ISerializableModel interface.");// + modelType);
        }
    }


}
