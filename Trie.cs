using System.Text.RegularExpressions;

namespace SenseNotes.Api;

/// <summary>Prefix index: each node keeps the ids of every note with a word passing through it.</summary>
public class Trie
{
    private class Node
    {
        public Dictionary<char, Node> Kids = new();
        public HashSet<string> Ids = new();
    }

    private readonly Node _root = new();
    private readonly object _lock = new();

    public void Insert(string word, string id)
    {
        lock (_lock)
        {
            var n = _root;
            foreach (var c in word)
            {
                if (!n.Kids.TryGetValue(c, out var next)) n.Kids[c] = next = new Node();
                n = next;
                n.Ids.Add(id);
            }
        }
    }

    public void IndexText(string text, string id)
    {
        foreach (var w in Regex.Split(text, @"\W+").Where(w => w.Length > 2))
            Insert(w.ToLowerInvariant(), id);
    }

    public List<string> Search(string prefix)
    {
        lock (_lock)
        {
            var n = _root;
            foreach (var c in prefix.ToLowerInvariant())
                if (!n.Kids.TryGetValue(c, out n)) return new();
            return n.Ids.ToList();
        }
    }

    /// <summary>Used when a note is edited or deleted so search never returns stale matches.</summary>
    public void RemoveId(string id) { lock (_lock) Remove(_root, id); }

    private static void Remove(Node n, string id)
    {
        n.Ids.Remove(id);
        foreach (var kid in n.Kids.Values) Remove(kid, id);
    }
}
