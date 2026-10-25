using FluentValidation;
using MongoDB.Driver;
using SenseNotes.Api;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5000");

builder.Services.AddControllers();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));
// No Mongo:Conn setting = run with in-memory storage (no database needed).
var mongoConn = builder.Configuration["Mongo:Conn"];
if (string.IsNullOrWhiteSpace(mongoConn))
    builder.Services.AddSingleton<INoteRepository, InMemoryNoteRepository>();
else
{
    builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConn));
    builder.Services.AddSingleton<INoteRepository, MongoNoteRepository>();
}
builder.Services.AddSingleton(new LruCache<string, MlResult>(100));
builder.Services.AddSingleton<Trie>();
builder.Services.AddScoped<IValidator<NoteRequest>, NoteRequestValidator>();
builder.Services.AddHttpClient<MlClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Ml:BaseUrl"] ?? "http://localhost:8000");
    c.Timeout = TimeSpan.FromMinutes(2);
});

var app = builder.Build();
app.UseCors();
app.MapControllers();

// Rebuild the search index from saved notes on startup.
var trie = app.Services.GetRequiredService<Trie>();
foreach (var n in await app.Services.GetRequiredService<INoteRepository>().GetPageAsync(1, 1000))
    trie.IndexText(n.Title + " " + n.Text, n.Id!);

app.Run();
