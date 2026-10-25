namespace SenseNotes.Api;

public class MlClient(HttpClient http)
{
    public async Task<MlResult> AnalyzeAsync(string text)
    {
        var r = await http.PostAsJsonAsync("/analyze", new { text });
        r.EnsureSuccessStatusCode();
        return (await r.Content.ReadFromJsonAsync<MlResult>())!;
    }
}
