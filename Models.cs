using FluentValidation;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SenseNotes.Api;

public class MlSentiment { public string Label { get; set; } = ""; public double Score { get; set; } }

public class MlResult
{
    public MlSentiment Sentiment { get; set; } = new();
    public string Summary { get; set; } = "";
    public List<string> Keywords { get; set; } = new();
}

public class Note
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string Title { get; set; } = "";
    public string Text { get; set; } = "";
    public MlResult? Analysis { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public record NoteRequest(string Title, string Text);

public class NoteRequestValidator : AbstractValidator<NoteRequest>
{
    public NoteRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Text).NotEmpty().MaximumLength(5000);
    }
}
