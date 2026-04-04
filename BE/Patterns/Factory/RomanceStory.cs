namespace BE.Patterns.Factory
{
    public class RomanceStory : IStoryCategory
    {
        public override string Validate()
        {
            if (string.IsNullOrEmpty(Title)) return "Tên truyện ngôn tình không được để trống!";
            return null;
        }

        public override string GetSpecialFeature()
        {
            return "Heartwarming love story, emotional journey";
        }
    }
}
