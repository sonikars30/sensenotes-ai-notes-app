using System.Collections.Concurrent;

namespace SenseNotes.Api;

/// <summary>Keeps notes in memory so the app runs with no database. Notes are lost when the API stops.</summary>
public class InMemoryNoteRepository : INoteRepository
{
    private readonly ConcurrentDictionary<string, Note> _notes = new();

    public Task AddAsync(Note n)
    {
        n.Id ??= Guid.NewGuid().ToString("N");
        _notes[n.Id] = n;
        return Task.CompletedTask;
    }

    public Task<Note?> GetAsync(string id) => Task.FromResult(_notes.GetValueOrDefault(id));

    public Task<List<Note>> GetPageAsync(int page, int size) =>
        Task.FromResult(_notes.Values.OrderByDescending(n => n.CreatedAt).Skip((page - 1) * size).Take(size).ToList());

    public Task<List<Note>> GetByIdsAsync(IEnumerable<string> ids) =>
        Task.FromResult(ids.Select(i => _notes.GetValueOrDefault(i)).OfType<Note>().OrderByDescending(n => n.CreatedAt).ToList());

    public Task UpdateAsync(Note n) { _notes[n.Id!] = n; return Task.CompletedTask; }

    public Task<bool> DeleteAsync(string id) => Task.FromResult(_notes.TryRemove(id, out _));
}
