namespace BE.Patterns.Factory
{
    public class HorrorStory : IStoryCategory
    {
        public override bool Validate()
        {
            // Horror stories might need age verification
            return !string.IsNullOrEmpty(Title) && Description?.Contains("18+") == false;
        }

        public override string GetSpecialFeature()
        {
            return "Warning: May contain scary content. Age restriction: 16+";
        }
    }
}
