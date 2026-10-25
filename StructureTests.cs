using SenseNotes.Api;
using Xunit;

public class StructureTests
{
    [Fact]
    public void Lru_EvictsLeastRecentlyUsed()
    {
        var c = new LruCache<string, int>(2);
        c.Put("a", 1); c.Put("b", 2);
        c.Get("a");                 // "b" is now the oldest
        c.Put("c", 3);
        Assert.Equal(0, c.Get("b"));
        Assert.Equal(1, c.Get("a"));
        Assert.Equal(3, c.Get("c"));
    }

    [Fact]
    public void Trie_FindsByPrefix()
    {
        var t = new Trie();
        t.Insert("cat", "1"); t.Insert("car", "2");
        Assert.Equal(2, t.Search("ca").Count);
        Assert.Single(t.Search("cat"));
        Assert.Empty(t.Search("dog"));
    }

    [Fact]
    public void Trie_RemoveIdDropsAllMatches()
    {
        var t = new Trie();
        t.IndexText("meeting notes", "1");
        t.RemoveId("1");
        Assert.Empty(t.Search("meet"));
        Assert.Empty(t.Search("not"));
    }
}
