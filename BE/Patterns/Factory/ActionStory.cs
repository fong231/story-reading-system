namespace BE.Patterns.Factory
{
    public class ActionStory : IStoryCategory
    {
        public override bool Validate()
        {
            // Action stories might need action tag validation
            return !string.IsNullOrEmpty(Title) && Title.Length >= 3;
        }

        public override string GetSpecialFeature()
        {
            return "High-paced action scenes, combat sequences";
        }
    }
}
