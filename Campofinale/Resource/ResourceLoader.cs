using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Campofinale.Resource
{
    public class ResourceLoader
    {

        public static void LoadTableCfg()
        {
            var tableCfgTypes = GetAllTableCfgTypes();

            foreach (var type in tableCfgTypes)
            {
                var attr = type.GetCustomAttribute<TableCfgTypeAttribute>();
                string json = ReadJsonFile(attr.Name);
                FieldInfo field = GetResourceField(type);
                if (field != null && json.Length > 0)
                {
                    object deserializedData = DeserializeJson(json, field.FieldType, type);
                    field.SetValue(null, deserializedData);
                    //Logger.Print($"Loaded {attr.Name} into {field.Name}");
                }
            }
        }
        private static object DeserializeJson(string json, Type fieldType, Type valueType)
        {
            if (fieldType.IsGenericType)
            {
                var genericTypeDef = fieldType.GetGenericTypeDefinition();

                if (genericTypeDef == typeof(Dictionary<,>))
                {
                    var keyType = fieldType.GetGenericArguments()[0]; // String
                    var valType = fieldType.GetGenericArguments()[1]; // Il tipo desiderato
                    return JsonConvert.DeserializeObject(json, typeof(Dictionary<,>).MakeGenericType(keyType, valType));
                }
                else if (genericTypeDef == typeof(List<>))
                {
                    return JsonConvert.DeserializeObject(json, typeof(List<>).MakeGenericType(valueType));
                }
            }

            // Oggetto singolo
            return JsonConvert.DeserializeObject(json, valueType);
        }
        public static string ReadJsonFile(string path)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch (Exception e)
            {
                Logger.PrintError($"Error occured while loading {path} Err: {e.Message}");
                ResourceManager.missingResources = true;
                return "";
            }

        }

        public static List<Type> GetAllTableCfgTypes()
        {
            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttribute<TableCfgTypeAttribute>() != null)
                .ToList();
        }
        public static FieldInfo GetResourceField(Type type)
        {
            var resourceManagerType = typeof(ResourceManager);
            var fields = resourceManagerType.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var field in fields)
            {
                var fieldType = field.FieldType;

                if (fieldType.IsGenericType)
                {
                    var genericTypeDef = fieldType.GetGenericTypeDefinition();

                    // Controlla se è un Dictionary<TKey, TValue> e se TValue è del tipo richiesto
                    if (genericTypeDef == typeof(Dictionary<,>))
                    {
                        var valueType = fieldType.GetGenericArguments()[1]; // Ottiene TValue
                        if (valueType == type)
                        {
                            return field;
                        }
                    }
                    // Controlla se è una List<T> e se T è del tipo richiesto
                    else if (genericTypeDef == typeof(List<>) && fieldType.GetGenericArguments()[0] == type)
                    {
                        return field;
                    }
                }
                else
                {
                    // Se il campo non è una collezione ma è direttamente del tipo richiesto
                    if (fieldType == type)
                    {
                        return field;
                    }
                }
            }

            return null; // Nessun campo trovato
        }
    }
}
