using MongoDB.Driver;

namespace SenseNotes.Api;

public interface INoteRepository
{
    Task AddAsync(Note note);
    Task<Note?> GetAsync(string id);
    Task<List<Note>> GetPageAsync(int page, int size);
    Task<List<Note>> GetByIdsAsync(IEnumerable<string> ids);
    Task UpdateAsync(Note note);
    Task<bool> DeleteAsync(string id);
}

public class MongoNoteRepository(IMongoClient client) : INoteRepository
{
    private readonly IMongoCollection<Note> _col =
        client.GetDatabase("sensenotes").GetCollection<Note>("notes");

    public Task AddAsync(Note n) => _col.InsertOneAsync(n);

    public async Task<Note?> GetAsync(string id) => await _col.Find(n => n.Id == id).FirstOrDefaultAsync();

    public Task<List<Note>> GetPageAsync(int page, int size) =>
        _col.Find(_ => true).SortByDescending(n => n.CreatedAt).Skip((page - 1) * size).Limit(size).ToListAsync();

    public Task<List<Note>> GetByIdsAsync(IEnumerable<string> ids) =>
        _col.Find(n => ids.Contains(n.Id!)).SortByDescending(n => n.CreatedAt).ToListAsync();

    public Task UpdateAsync(Note n) => _col.ReplaceOneAsync(x => x.Id == n.Id, n);

    public async Task<bool> DeleteAsync(string id) => (await _col.DeleteOneAsync(n => n.Id == id)).DeletedCount > 0;
}
