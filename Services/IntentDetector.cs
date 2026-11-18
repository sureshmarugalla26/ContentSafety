namespace ContentSafetyDemo.Services
{
    public class IntentDetector : IIntentDetector
    {
        public bool IsHateIntent(string text)
        {
            string[] patterns =
            {
            "community people",
            "religion idiots",
            "nationality fools",
            "ethnicity people",
            "group people are dumb"
        };

            return Match(text, patterns);
        }

        public bool IsSexualIntent(string text)
        {
            string[] patterns =
            {
            "talk dirty",
            "naughty",
            "sexy",
            "seductive",
            "be my girlfriend",
            "be my boyfriend",
            "flirt with me",
            "romantic night",
            "kiss you"
        };

            return Match(text, patterns);
        }

        public bool IsSelfHarmIntent(string text)
        {
            string[] patterns =
            {
            "want to die",
            "end my life",
            "hurt myself",
            "give up on life",
            "feel hopeless",
            "nothing helps"
        };

            return Match(text, patterns);
        }

        public bool IsSeriousViolence(string text)
        {
            string[] patterns =
            {
            "kill someone",
            "hurt someone",
            "attack someone",
            "revenge"
        };

            return Match(text, patterns);
        }

        public bool IsMildAggression(string text)
        {
            string[] patterns =
            {
            "punch a wall",
            "smash my computer",
            "break something",
            "throw my laptop"
        };

            return Match(text, patterns);
        }

        private bool Match(string text, string[] patterns)
        {
            return patterns.Any(p => text.Contains(p, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class ContentSafetyResponse
    {
        public List<CategoryResult> CategoriesAnalysis { get; set; }
    }

    //public class CategoryAnalysis
    //{
    //    public string Category { get; set; }
    //    public int Severity { get; set; }
    //}
    public class CategoryResult
    {
        public string Category { get; set; }
        public int Severity { get; set; }
    }
}
