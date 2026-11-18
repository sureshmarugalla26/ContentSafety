namespace ContentSafetyDemo.Services
{
    public interface IContentSafety
    {
        Task<ContentSafetyResponse> AnalyzeText(string text);
    }
}
