namespace BE.Patterns.Factory
{
    public class HorrorStory : IStoryCategory
    {
        public override string Validate()
        {
            if (string.IsNullOrEmpty(Title)) return "Tên truyện kinh dị không được để trống!";
            if (Description?.Contains("18+") == true) return "Truyện kinh dị hiện chưa hỗ trợ nhãn 18+ trực tiếp trong mô tả!";
            return null;
        }

        public override string GetSpecialFeature()
        {
            return "Warning: May contain scary content. Age restriction: 16+";
        }
    }
}
