namespace BE.Patterns.Factory
{
    public class RomanceStory : IStoryCategory
    {
        public override bool Validate()
        {
            // Romance stories might have relationship validation
            return !string.IsNullOrEmpty(Title);
        }

        public override string GetSpecialFeature()
        {
            return "Heartwarming love story, emotional journey";
        }
    }
}
