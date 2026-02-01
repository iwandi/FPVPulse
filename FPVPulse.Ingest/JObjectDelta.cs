using Newtonsoft.Json.Linq;
using System.Reflection;
using System;
using System.Collections.Generic;

namespace FPVPulse.Ingest
{
    /*public class JObjectDelta
    {
        static JsonMergeSettings jsonMergeSettings = new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Union,
            MergeNullValueHandling = MergeNullValueHandling.Merge
        };


        static Dictionary<Type, List<PropertyInfo>>? protectedPropertiesCache;

        public static List<PropertyInfo> GetProtected(Type type)
        {
            if(protectedPropertiesCache == null)
            {
                protectedPropertiesCache = new Dictionary<Type, List<PropertyInfo>>();
            }
            else if (protectedPropertiesCache.TryGetValue(type, out var protectedProperties))
            {
                return protectedProperties;
            }
            var @protected = type.GetProperties().Where(p =>
                p.GetCustomAttributes(typeof(Newtonsoft.Json.JsonPropertyAttribute), true)
                .Cast<Newtonsoft.Json.JsonPropertyAttribute>()
                .Any(attr => attr.Required == Newtonsoft.Json.Required.Always)
            ).ToList();
            protectedPropertiesCache[type] = @protected;

            return @protected;
        }

        // Using Reflection and boxing this should be realy slow
        public static bool Diff(JObject original, JObject change, out JObject merged, out JObject diff)
        {
            merged = (JObject)original.DeepClone();
            merged.Merge(change, jsonMergeSettings);


        }
    }*/
}
