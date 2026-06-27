using System.Text.Json.Serialization;
using Plugin.LocalNotification.Core.Models;

namespace TimeClockApp.Utilities;

[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals)]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(NotificationRequest))]
internal partial class AppJsonContext : JsonSerializerContext
{
}
