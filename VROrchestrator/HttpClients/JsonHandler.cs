using System;
using System.Text.Json;
using CSharpFunctionalExtensions;

namespace VRNotifier.Services
{
    public static class JsonHandler
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        public static Result<T> Deserialize<T>(string toDeserialize)
        {
            try
            {
                var result = JsonSerializer.Deserialize<T>(toDeserialize, JsonSerializerOptions);
                return Result.Success(result);
            }
            catch (JsonException)
            {
                return Result.Failure<T>("Serialization failed - invalid JSON");
            }
            catch (ArgumentNullException)
            {
                return Result.Failure<T>("Serialization failed - JSON was null");
            }
        }

        public static string Serialize<T>(T toSerialize)
        {
            return JsonSerializer.Serialize(toSerialize, JsonSerializerOptions);
        }
    }
}