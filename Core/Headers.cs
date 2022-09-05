namespace Mu.Core;

internal sealed class Headers : IDictionary<string, string>
{
    private IDictionary<string, string> backing = new Dictionary<string, string>()
    {
        ["alg"] = "HS256",
        ["typ"] = "JWT",
    };

    public string Algorithm { get => backing["alg"]; set => backing["alg"] = value; }
    public string Type { get => backing["typ"]; set => backing["typ"] = value; }
    public string EventType { get => backing["event"]; set => backing["event"] = value; }

    public string this[string key] { get => backing[key]; set => backing[key] = value; }

    public ICollection<string> Keys => backing.Keys;

    public ICollection<string> Values => backing.Values;

    public int Count => backing.Count();

    public bool IsReadOnly => backing.IsReadOnly;

    public void Add(string key, string value) => backing.Add(key, value);

    public void Add(KeyValuePair<string, string> item) => backing.Add(item);

    public void Clear() => backing.Clear();

    public bool Contains(KeyValuePair<string, string> item) => backing.Contains(item);

    public bool ContainsKey(string key) => backing.ContainsKey(key);

    public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) => backing.CopyTo(array, arrayIndex);

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => backing.GetEnumerator();

    public bool Remove(string key) => backing.Remove(key);

    public bool Remove(KeyValuePair<string, string> item) => backing.Remove(item);

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value) => backing.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator() => backing.GetEnumerator();

    // public static explicit operator Headers(Dictionary<string, string> dictionary) 
    // {
    //     var headers = new Headers();
    //     headers.backing = dictionary;
    //     return headers;
    // }

    public static implicit operator Headers(Dictionary<string, string> dictionary) 
    {
        var headers = new Headers();
        headers.backing = dictionary;
        return headers;
    }
}
