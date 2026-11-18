namespace ContentSafetyDemo.Services
{
    public interface IIntentDetector
    {
        bool IsHateIntent(string text);
        bool IsSexualIntent(string text);
        bool IsSelfHarmIntent(string text);
        bool IsSeriousViolence(string text);
        bool IsMildAggression(string text);
    }
}
