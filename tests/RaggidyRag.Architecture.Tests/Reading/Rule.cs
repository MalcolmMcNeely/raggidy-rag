using YamlDotNet.Serialization;

namespace RaggidyRag.Architecture.Tests.Reading;

public sealed class Rule
{
    readonly Dictionary<string, object?> settings;

    Rule(Dictionary<string, object?> settings) => this.settings = settings;

    public static Rule Read(string path)
    {
        var text = File.ReadAllText(path);
        var open = text.LastIndexOf("```yaml", StringComparison.Ordinal);
        if (open < 0)
        {
            throw new InvalidOperationException(path + " ends with no yaml block");
        }

        var start = text.IndexOf('\n', open) + 1;
        var close = text.IndexOf("```", start, StringComparison.Ordinal);
        var yaml = text[start..close];
        var parsed = new DeserializerBuilder().Build().Deserialize<Dictionary<string, object?>>(yaml);
        return new Rule(parsed ?? new Dictionary<string, object?>());
    }

    public string String(string key) => settings.TryGetValue(key, out var value) && value is string text
        ? text
        : throw new InvalidOperationException(key + " is not set");

    public int Int(string key) => int.Parse(String(key), System.Globalization.CultureInfo.InvariantCulture);

    public bool Bool(string key) => bool.Parse(String(key));

    public IReadOnlyList<string> Strings(string key) =>
        settings.TryGetValue(key, out var value) ? ToStrings(value) : [];

    public IReadOnlyDictionary<string, string> Map(string key) =>
        ToMap(key).ToDictionary(pair => pair.Key, pair => pair.Value?.ToString() ?? "");

    public IReadOnlyDictionary<string, IReadOnlyList<string>> Lists(string key) =>
        ToMap(key).ToDictionary(pair => pair.Key, pair => ToStrings(pair.Value));

    IEnumerable<KeyValuePair<string, object?>> ToMap(string key)
    {
        if (!settings.TryGetValue(key, out var value) || value is null)
        {
            return [];
        }

        if (value is Dictionary<object, object?> map)
        {
            return map.Select(pair => new KeyValuePair<string, object?>(pair.Key.ToString()!, pair.Value));
        }

        throw new InvalidOperationException(key + " is not a map");
    }

    static IReadOnlyList<string> ToStrings(object? value) => value switch
    {
        null => [],
        List<object?> list => list.Select(item => item?.ToString() ?? "").ToList(),
        string one => [one],
        _ => throw new InvalidOperationException(value + " is not a list"),
    };
}
