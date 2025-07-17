using ProjectManagement.App.Models;
using System;
using System.Dynamic;
using System.Reflection;
using System.Text.Json;

namespace ProjectManagement.App.Helpers
{
    public static class ExpandoObjectExtensions
    {
        public static TaskItem ToTaskItemFromPayload(this ExpandoObject expando)
        {
            // expando: { value: { ...TaskItem fields... }, action: "insert" }
            var dict = (IDictionary<string, object?>)expando;
            if (!dict.TryGetValue("value", out var valueObj)) return new TaskItem();

            if (valueObj == null) return new TaskItem();

            if (valueObj is JsonElement jsonElement)
            {
                // Deserialize to TaskItem

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                };

                try
                {
                    return jsonElement.Deserialize<TaskItem>(options) ?? new TaskItem();
                }
                catch
                {
                    return new TaskItem();
                }
            }

            return new TaskItem();


        }

       
    }
}