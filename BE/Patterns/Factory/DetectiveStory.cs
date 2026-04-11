namespace BE.Patterns.Factory
{
    public class DetectiveStory : IStoryCategory
    {
        public override string Validate()
        {
            if (string.IsNullOrEmpty(Title)) return "Tên truyện trinh thám không được để trống!";
            if (Description?.Length <= 50) return "Truyện trinh thám cần mô tả chi tiết hơn (ít nhất 50 ký tự) để gợi mở tình tiết!";
            return null;
        }

        public override string GetSpecialFeature()
        {
            return "Mystery solving, clues and investigation";
        }
    }
}
