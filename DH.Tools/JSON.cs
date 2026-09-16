using System.Reflection;
using System.Runtime.Versioning;
using Newtonsoft.Json;

namespace DH.Tools;

/// <summary>
/// Authors  : DH
/// Datetime : 2026-09-03 10:30
/// Describe : JSON Utility
/// </summary>
public sealed class JSON
{
    public static T ReadJSONFile<T>(string FilePath) where T : notnull, new()
    {
        if (File.Exists(FilePath))
        {
            using StreamReader FileReader = new(FilePath);

            JsonSerializer Serializer = new();

            JsonReader Reader = new JsonTextReader(FileReader);

            T JSONValue = Serializer.Deserialize<T>(Reader) ?? default!;

            return JSONValue;
        }
        else
        {
            return default!;
        }
    }

    public static bool WriteJSONToFile<T>(string FilePath, T JSONvalues) where T : notnull, new()
    {
        using StreamWriter SWriter = new(FilePath);

        JsonSerializer Serializer = new();

        JsonWriter JWriter = new JsonTextWriter(SWriter);

        Serializer.Serialize(JWriter, JSONvalues);

        JWriter.Flush();

        JWriter.Close();

        SWriter.Close();

        return true;
    }

    public static string SerializeObjectToJSON<T>(T Entity) where T : notnull, new()
    {
        return JsonConvert.SerializeObject(Entity) ?? string.Empty;
    }

    public static T ReadAssemblyInfo<T>(string AssemblyPath = "") where T : notnull, new()
    {
        string AssemboyBuildTime = default!;

        Assembly? AInfo = default!;

        AInfo = Assembly.GetEntryAssembly();

        if (!string.IsNullOrEmpty(AssemblyPath))
        {
            if (File.Exists(AssemblyPath))
            {
                AssemboyBuildTime = File.GetLastWriteTime(AssemblyPath).ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        if (AInfo is not null)
        {
            T Entity = new();

            string Version = AInfo.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? default!;

            string Company = AInfo.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? default!;

            Entity.GetType().GetProperty("Version")?.SetValue(Entity, Version);

            Entity.GetType().GetProperty("BuildTime")?.SetValue(Entity, AssemboyBuildTime);

            Entity.GetType().GetProperty("Company")?.SetValue(Entity, Company);

            return Entity;
        }
        else
        {
            return default!;
        }
    }
}
