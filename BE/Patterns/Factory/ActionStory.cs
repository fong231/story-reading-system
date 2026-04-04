namespace BE.Patterns.Factory
{
    public class ActionStory : IStoryCategory
    {
        public override string Validate()
        {
            if (string.IsNullOrEmpty(Title)) return "Tên truyện hành động không được để trống!";
            if (Title.Length < 3) return "Tên truyện hành động phải có ít nhất 3 ký tự!";
            return null;
        }

        public override string GetSpecialFeature()
        {
            return "High-paced action scenes, combat sequences";
        }
    }
}
