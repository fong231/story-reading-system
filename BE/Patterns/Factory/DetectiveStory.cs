namespace BE.Patterns.Factory
{
    public class DetectiveStory : IStoryCategory
    {
        public override bool Validate()
        {
            // Detective stories might need mystery elements
            return !string.IsNullOrEmpty(Title) && Description?.Length > 50;
        }

        public override string GetSpecialFeature()
        {
            return "Mystery solving, clues and investigation";
        }
    }
}
