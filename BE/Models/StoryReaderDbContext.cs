using Microsoft.EntityFrameworkCore;

namespace BE.Models
{
    public class StoryReaderDbContext : DbContext
    {
        public StoryReaderDbContext(DbContextOptions<StoryReaderDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Story> Stories { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<ReadingProgress> ReadingProgresses { get; set; }
        public DbSet<ReadingMode> ReadingModes { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<StoryFollower> StoryFollowers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ================================================================
            // User Configuration
            // ================================================================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // ================================================================
            // Category Configuration - FACTORY PATTERN
            // ================================================================
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(e => e.IsActive);
            });

            // ================================================================
            // Story Configuration - FACTORY PATTERN
            // ================================================================
            modelBuilder.Entity<Story>(entity =>
            {
                entity.HasIndex(e => e.AuthorId);
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.ViewCount);

                entity.HasOne(e => e.Author)
                    .WithMany(u => u.Stories)
                    .HasForeignKey(e => e.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Stories)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================================================================
            // Chapter Configuration
            // ================================================================
            modelBuilder.Entity<Chapter>(entity =>
            {
                entity.HasIndex(e => e.StoryId);
                entity.HasIndex(e => e.PublishedAt);
                entity.HasIndex(e => new { e.StoryId, e.ChapterNumber }).IsUnique();

                entity.HasOne(e => e.Story)
                    .WithMany(s => s.Chapters)
                    .HasForeignKey(e => e.StoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ================================================================
            // ReadingProgress Configuration - SINGLETON PATTERN
            // ================================================================
            modelBuilder.Entity<ReadingProgress>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();

                entity.HasOne(e => e.User)
                    .WithOne(u => u.ReadingProgress)
                    .HasForeignKey<ReadingProgress>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.CurrentStory)
                    .WithMany()
                    .HasForeignKey(e => e.CurrentStoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CurrentChapter)
                    .WithMany()
                    .HasForeignKey(e => e.CurrentChapterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================================================================
            // ReadingMode Configuration - STRATEGY PATTERN
            // ================================================================
            modelBuilder.Entity<ReadingMode>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();

                entity.HasOne(e => e.User)
                    .WithOne(u => u.ReadingMode)
                    .HasForeignKey<ReadingMode>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ================================================================
            // Bookmark Configuration
            // ================================================================
            modelBuilder.Entity<Bookmark>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.StoryId);
                entity.HasIndex(e => new { e.UserId, e.StoryId }).IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Bookmarks)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Story)
                    .WithMany(s => s.Bookmarks)
                    .HasForeignKey(e => e.StoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Chapter)
                    .WithMany(c => c.Bookmarks)
                    .HasForeignKey(e => e.ChapterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================================================================
            // Comment Configuration
            // ================================================================
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.StoryId);
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Story)
                    .WithMany(s => s.Comments)
                    .HasForeignKey(e => e.StoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================================================================
            // Rating Configuration
            // ================================================================
            modelBuilder.Entity<Rating>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.StoryId);
                entity.HasIndex(e => new { e.UserId, e.StoryId }).IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Ratings)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Story)
                    .WithMany(s => s.Ratings)
                    .HasForeignKey(e => e.StoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================================================================
            // Notification Configuration - OBSERVER PATTERN
            // ================================================================
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.IsRead });
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Story)
                    .WithMany(s => s.Notifications)
                    .HasForeignKey(e => e.StoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Chapter)
                    .WithMany(c => c.Notifications)
                    .HasForeignKey(e => e.ChapterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================================================================
            // StoryFollower Configuration - OBSERVER PATTERN
            // ================================================================
            modelBuilder.Entity<StoryFollower>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.StoryId);
                entity.HasIndex(e => new { e.UserId, e.StoryId }).IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.StoryFollowers)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Story)
                    .WithMany(s => s.Followers)
                    .HasForeignKey(e => e.StoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================================================================
            // Seed Data
            // ================================================================
            // Users
            modelBuilder.Entity<User>().HasData(
                new User { UserId = 1, Username = "author_admin", Email = "author@test.com", PasswordHash = "hash123" },
                new User { UserId = 2, Username = "reader_01", Email = "reader@test.com", PasswordHash = "hash123" }
            );

            // Categories (for Factory Pattern)
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "Hành Động", IsActive = true },
                new Category { CategoryId = 2, Name = "Kinh Dị", IsActive = true },
                new Category { CategoryId = 3, Name = "Lãng Mạn", IsActive = true },
                new Category { CategoryId = 4, Name = "Trinh Thám", IsActive = true }
            );

            // Stories
            modelBuilder.Entity<Story>().HasData(
                new Story
                {
                    StoryId = 1,
                    Title = "Huyền Thoại Sát Thủ",
                    AuthorId = 1,
                    CategoryId = 1,
                    CoverImage = "https://picsum.photos/200/300?random=1",
                    Description = "Truyện hành động hấp dẫn.",
                    ViewCount = 1000,
                    AverageRating = 4.5m,
                    TotalRatings = 1
                },
                new Story
                {
                    StoryId = 2,
                    Title = "Tiếng Vọng Đêm Khuya",
                    AuthorId = 1,
                    CategoryId = 2,
                    CoverImage = "https://picsum.photos/200/300?random=2",
                    Description = "Truyện kinh dị kịch tính.",
                    ViewCount = 500,
                    AverageRating = 4.0m,
                    TotalRatings = 1
                }
            );

            // Seed Chapters
            modelBuilder.Entity<Chapter>().HasData(
                new Chapter {
                    ChapterId = 1,
                    StoryId = 1,
                    ChapterNumber = 1,
                    Title = "Sự coi thường này, thật quen thuộc",
                    Content = """
                        <div>"Điều gì sẽ xảy ra với con người sau khi chết?”</div>
                        <div>“Tôi không biết vì tôi đã chết bao giờ đâu.”</div>
                        <div>Tôi thản nhiên trả lời người bạn của mình, người đột nhiên hỏi tôi một câu như vậy khi chúng tôi đang uống rượu.</div>
                        <div>Đó chưa bao giờ là một chủ đề mà tôi dành nhiều tâm trí để suy nghĩ. Suy cho cùng, việc mài sắc lưỡi kiếm thêm một chút có vẻ là cách sử dụng thời gian tốt hơn là nghiền ngẫm những điều như thế.</div>
                        <div>“Người ta nói rằng một số người sẽ được tái sinh.”</div>
                        <div>“À, vậy thì, tôi hy vọng kiếp sau mình được sinh ra trong một gia đình bình thường. Tôi muốn sống một đời lặng lẽ.”</div>
                        <div>Hắn cười ngắn ngủi trước lời nói của tôi về việc muốn có một cuộc sống yên bình, rồi hỏi lại.</div>
                        <div>“Cậu nghiêm túc chứ?"</div>
                        <div>"Ù."</div>
                        <div>“Nhiều người đã đang phải chịu đựng thảm họa rồi. Nếu cậu hành động, sẽ còn nhiều người hơn nữa phải chết.”</div>
                        <div>"Tôi không quan tâm.”</div>
                        <div>“Tôi không nhận ra người bạn vui vẻ của mình lại mang trong lòng nhiều đau đớn đến thế.”</div>
                        <div>“Ai mà chẳng có một vài quá khứ đau thương."</div>
                        <div>Hắn gật đầu đồng ý, rồi nâng ly lên.</div>
                        <div>“Hãy đi săn quái vật lần nữa khi tất cả chuyện này kết thúc nhé.”</div>
                        <div>“Vậy thì tìm cho tôi một đối thủ xứng tầm đi.”</div>
                        <div>Hắn cười khẩy, cạn chén trong một hơi rồi đặt ly xuống.</div>
                        <div>“Chúc may mắn. Tôi có nên cầu nguyện cho cậu không?"</div>
                        <div>“Tôi không tin vào thần linh. Tôi chỉ tin vào thứ này thôi.”</div>
                        <div>Tôi lắc lắc thanh kiếm của mình và cười, khiến hắn lắc đầu ngán ngẩm khi đứng dậy.</div>
                        <div>"Tạm biệt. Tôi sẽ không đi xa đâu."</div>
                        <div>“Cứ như cậu từng đi xa không bằng.”</div>
                        <div>Xoẹt.</div>
                        <div>Một vòng xoáy đen xuất hiện, và cơ thể hắn bị hút vào đó, biến mất khỏi tầm mắt.</div>
                        <div>“Quả là một kỹ năng tiện lợi.”</div>
                        <div>Còn lại một mình, tôi nâng ly.</div>
                        <div>Một ly, hai ly, ba ly.</div>
                        <div>Ký ức quá khứ lại ùa về.</div>
                        <div>'Tôi hối hận.'</div>
                        <div>Lãnh địa Ferdium nằm ở phía bắc của Vương quốc Ritania.</div>
                        <div>Đó là một vùng đất nghèo nàn và hoang vằng nằm ở biên giới vương quốc, liên tục phải chiến đấu với những kẻ man di.</div>
                        <div>Tôi sinh ra là người thừa kế của lãnh địa đó.</div>
                        <div>'Tôi đã thật thảm hại.'</div>
                        <div>Tôi sống cả đời chỉ toàn những lời phàn nàn, liên tục so sánh hoàn cảnh của mình với những đứa trẻ quý tộc khác.</div>
                        <div>Sự so sánh nảy sinh lòng tự ti.</div>
                        <div>Sự tự ti bộc phát thành những hành động liều lĩnh, dẫn đến tai nạn; những người khác liên tục chỉ trỏ và chế giễu tôi.</div>
                        <div>Một tên đốn mạt, một kẻ điên, một bậc thầy kiếm thuật tự kỷ ...</div>
                        <div>Tôi đã sống qua đủ loại danh hiệu lăng mạ cho đến khi cuối cùng, tôi bỏ trốn khỏi gia đình trong sự nhục nhã.</div>
                        <div>Nhiều năm trôi qua khi tôi lang thang với tư cách là lính đánh thuê.</div>
                        <div>Có lẽ tôi đã may mắn, nhưng tôi đã xoay sở để sống sót dù đã lăn lộn qua vô số chiến trường.</div>
                        <div>Khi tôi có được kỹ năng, cận kề cái chết hết lần này đến lần khác, danh tiếng của tôi lớn dần-và nỗi nhớ nhà cũng vậy.</div>
                        <div>'Tôi đã nghĩ mọi thứ sẽ ổn nều mình trở về gia đình vào lúc đó.'</div>
                        <div>Với sự hổi hận và tội lỗi về những ngày tháng tuổi trẻ ngu ngốc, tôi nghĩ mình có thể trở về nhà và giúp ích rất nhiều cho gia đình.</div>
                        <div>Nhưng ...</div>
                        <div>Vào thời điểm tôi trở về, gia đình và điền trang của tôi đã biến thành tro bụi.</div>
                        <div>Tôi không thể làm gì được. Tất cả những gì tôi làm là chạy trốn.</div>
                        <div>Tôi phải lần trốn, vứt bỏ cả cái tên quý tộc của mình, lo sợ những tổn hại tiềm tàng mà mình có thể phải đối mặt.</div>
                        <div>'Tôi phải trở nên mạnh mẽ hơn.'</div>
                        <div>Một mục tiêu mới trỗi dậy trong tôi.</div>
                        <div>Tôi đã chịu đựng nhiều năm đau đớn khổ sở, mài giũa bản thân như một lưỡi kiếm. Tôi chiến đấu không ngừng nghỉ chống lại vô số tai ương tàn phá lục địa.</div>
                        <div>Đến một lúc nào đó, mọi người bắt đầu gọi tôi bằng một cái tên mới. Vua Lính Đánh Thuê.</div>
                        <div>Và cuối cùng, tôi đứng trong hàng ngũ bảy người quyền năng nhất thế giới, ở vị trí vinh quang được biết đến là Thất Cường Lục Địa.</div>
                        <div>Vào lúc đó, tôi không thiếu thứ gì trong đời, với vô số thuộc hạ, danh tiếng vô song và kỹ năng để chứng minh tất cả.</div>
                        <div>'Nhưng bấy nhiêu vẫn là chưa đủ.'</div>
                        <div>Tuy nhiên, tôi luôn cảm thấy một cơn khát không thể thỏa mãn.</div>
                    """
                },
                new Chapter {
                    ChapterId = 2,
                    StoryId = 1,
                    ChapterNumber = 2,
                    Title = "Tôi sẽ không để nó xảy ra lần thứ hai",
                    Content = """
                        <div>"Anh trai?"</div>
                        <div>Khi Ghislain đột nhiên nắm lấy mặt cô và đôi vai anh bắt đầu run lên, Elena lộ ra vẻ mặt hơi sợ hãi.</div>
                        <div>Đó là bởi vì anh trai cô thuộc mẫu người có thể nổi khùng và làm điều gì đó điên rồ bất cứ lúc nào.</div>
                        <div>“Hả? Ô, không, không có gì đâu. Nhưng chà, lâu thật rồi nhỉ!”</div>
                        <div>Ghislain dang rộng hai tay, vẻ mặt đây xúc động.</div>
                        <div>Cái chết của Elena từng là một ký ức đau đớn ám ảnh anh suốt cả cuộc đời. Được thấy em gái còn sống, anh cảm thấy một niềm vui khôn xiết trào dâng trong lông ngực.</div>
                        <div>Anh không bộc lộ cảm xúc bằng lời nói. Đúng với danh hiệu Vua lính đánh thuê, anh luôn thể hiện bản thân một cách trực diện và mạnh mẽ qua hành động.</div>
                        <div>"Elena!"</div>
                        <div>Khi Ghislain tiến lại gần với vòng tay rộng mở, khuôn mặt Elena thoáng chốc tái nhợt.</div>
                        <div>"S-Sao vậy?"</div>
                        <div>"Anh nhớ em quá!"</div>
                        <div>“Nhưng mấy hôm trước em mới gặp anh mà ... Khoan đã! Sao anh lại hành động kỳ lạ thế này? Đừng có lại gần đây!”</div>
                        <div>Chộp!</div>
                        <div>Ghislain ôm chặt lấy Elena, nhắm mắt lại. Một cảm xúc mãnh liệt đến mức suýt làm anh rơi nước mắt bao trùm lấy toàn bộ cơ thể.</div>
                        <div>"Eo ôi! Sao tự dưng anh lại gây ám ảnh thế hả?"</div>
                        <div>Elena thực sự bối rối.</div>
                        <div>Sự thật là, cô và Ghislain không có mối quan hệ tốt đẹp gì cho lắm.</div>
                        <div>Bị thôi thúc bởi mặc cảm tự ti, Ghislain luôn nóng nảy và khiến những người xung quanh cảm thấy mệt mỏi. Chẳng đời nào anh lại tỏ ra trìu mến với em gái mình như vậy.</div>
                        <div>“Đây là trò đùa gì thế? Anh lại đang âm mưu chuyện gì nữa đây?”</div>
                        <div>Elena vặn mình, đây Ghislain ra.</div>
                        <div>Ngay khi cô định buông thêm một lời chất vấn, cô sững người lại khi nhìn vào khuôn mặt anh trai mình.</div>
                        <div>Ánh mắt dịu dàng, một nụ cười chất chứa nỗi nhớ mong khôn tả.</div>
                        <div>Đó là một biểu cảm của Ghislain mà cô chưa từng thấy bao giờ, và trong khoảnh khắc, nó khiên Elena cảm thây nghẹn ngào.</div>
                        <div>Cô không hiếu tại sao mình lại có cảm giác này.</div>
                        <div>'Sao anh ấy lại hành động thế nhỉ? Lại gây chuyện gì nữa à? Và sao mắt anh ấy lại tự dưng rưng rưng thế kia?'</div>
                        <div>Elena nhìn Ghislain đây nghi ngờ. Phía bên kia, anh vẫn đang mỉm cười rạng rỡ, như thể không còn gì hạnh phúc hơn.</div>
                        <div>Dù không biết lý do, nhưng nụ cười đó vào lúc này thật sự rất chân thành.</div>
                        <div>'Anh ấy hơi giông ngày xưa nhỉ?'</div>
                        <div>Khi cha của họ luôn đi chinh chiến xa nhà, và sau khi mẹ họ qua đời, hai anh em đã nương tựa vào nhau.</div>
                        <div>Tuy nhiên, thời gian trôi qua và khi Ghislain trở thành một kẻ đểu cáng, mối quan hệ của họ đã dần trở nên xa cách.</div>
                        <div>Khi Elena nheo mắt và tiếp tục nhìn chằm chẳm vào anh, Ghislain hằng giọng.</div>
                        <div>“E hèm, anh chỉ là vui khi gặp em thôi. Mà thôi, em vào phòng anh làm gì thế?"</div>
                        <div>"Chà."</div>
                        <div>Elena nhìn anh sững sờ, như thể không tin vào những gì mình vừa nghe.</div>
                        <div>Vài ngày trước khi cô đến thăm, Ghislain đâu có phản ứng như thế này.</div>
                        <div>- Cút đi. Đừng có lảng vảng trước mặt tao làm mất hứng. Tao thấy sự hiện diện của mày cực kỳ khó chịu.</div>
                    """
                },
                new Chapter { 
                    ChapterId = 3,
                    StoryId = 2,
                    ChapterNumber = 1,
                    Title = "Ngôi nhà ma ám đang suy tàn",
                    Content = """
                        <div>[Đây là lần đầu tiên tôi tới thăm Nhà Ma nhàm chán tới vậy.]</div>
                        <div>[Đạo cụ quá giả tạo; tôi không thây sợ chút nào. Nói ra thì, toàn bộ cứ như trò hề ấy.J</div>
                        <div>Những ngưoi thuc te nhu chung ta duong nhien khong co gi phai so ca! Ma quy lam gi co that!</div>
                        <div>[ Tao ghét phải nhắc lại, nhưng tao đã nói rồi mà. Lẽ ra tụi mình nên ở lại ký túc xá; Tao sắp lên level trên game online rồi.]</div>
                        <div>Nhóm học sinh thất vọng căn nhăn trước Ngôi Nhà Ma của Tây thành phố Cửu Giang trước khi rời đi trên xe mô-tô. Trần Ca, tay cầm sấp tờ rơi Nhà Ma, lắc đầu chán nản khi nhìn thấy vậy.</div>
                        <div>Nghệ thuật dọa người là một kỹ năng, nhưng với các bộ phim kinh dị, ngưỡng sợ hãi của nhiêu công dân hiện đại đã được nới rộng. Một chuyến đi Nhà Ma giờ chẳng khác gì đi dạo sân sau.</div>
                        <div>[Ông chủ!]</div>
                        <div>Một giọng trong trẻo cất lên từ đăng sau anh. Trần Ca quay lại và thấy một 'Zombie' mảnh khảnh trong bộ trang phục y tá chạy ra từ Nhà Ma trong cơn giận dữ.</div>
                        <div>[ Sao vậy, tiểu Uyển?] tên thật của zombie là Từ Uyền; cô ấy là một trong những diễn viên tạm thời được thuê bởi Nhà Ma.</div>
                        <div>[Mấy tên lưu manh lúc nãy, tụi nó dám lợi dụng em!] cô gái rít lên trong khi nghiến răng, nắm đấm nắm chặt.</div>
                        <div>Ra là, chỉ để phàn nàn ...</div>
                        <div>[ Thật là tổi tệ; tụi nó còn không để cho zombie yên.] Là ông chủ, tất nhiên Trần Ca ở phe tiểu Uyền. [Lát nữa, anh sẽ nói quản lý công viên đưa lên video giám sát.</div>
                        <div>[ Cái đó hông cần đâu. Khi nhận ra ý đồ, nấm đấm em đã bay thằng vào mặt tụi nó rồi. Từ Uyến chỉ vào vệt máu ở góc trang phục roi tự hào nói, [Đây, chỗ này không phải máu giả.</div>
                        <div>[ Tốt, tốt, con gái nên học cách tự vệ.] Trần Ca lau đi mồ hôi lạnh trên trán. Khi anh quay sang nhìn mặt trời lặng, anh nói, [Giờ chắc đã tới lúc dọn dẹp rồi nhỉ. Chúng ta có lẽ sẽ không có thêm khách nữa đâu, nên báo mọi người giúp anh hôm nay chúng ta tan ca sớm.]</div>
                        <div>Thế nhưng, anh nhận ra cô gái hóa trang zombie này không hề có ý định di chuyển.</div>
                        <div>[ Có chuyện gì nữa à?]</div>
                        <div>[Ông chủ ... ] Từ Uyển do dự trước khi chậm rãi kéo ra hai lá thư từ trong túi. Đây là đơn nghỉ việc của Đào Minh và tiểu Ngụy. Anh đã là ông chủ tốt với họ, nên họ không dám tận tay đưa mà lại nhờ em chuyển lời cho anh.]</div>
                        <div>[Họ nghỉ việc à?] Trần Ca hỏi câu hiển nhiên trong khi nhận lá thư, rồi anh nói, [Mọi người đều có giấc mơ riêng để theo đuổi, vậy đi. Tiểu Uyển, em có thể đi nếu không còn chuyện gì thêm.]</div>
                        <div>[Ok, em sẽ đi tẩy trang trước.</div>
                        <div>Sau khi cô zombie nhỏ bé ấy rời đi, Trần Ca lặng lẽ thắp điếu thuốc. Nữa năm trước, khi bố mẹ anh biến mất một cách bí ẩn, thứ duy nhất họ để lại cho anh là Ngôi Nhà Ma này. Để lưu giữ ký ức về họ, Trần Ca đã nghỉ việc để tập trung quản lý Nhà Ma này.</div>
                        <div>Than ôi, thời gian đã đổi thay. Mặc dù thuộc thể loại kinh điển, vẫn có sự cạnh tranh lớn giữa những Ngôi Nhà Ma, và cũng có vô số sự hạn chế. Một khung cảnh đáng sợ sẽ mất đi nhân tố sợ hãi sau trãi nghiệm đầu tiên, nhưng cập nhật liên tục sẽ đòi hỏi rất nhiều tiền và tài nguyên.</div>
                        <div>Bắt đầu từ vài tuần trước, Nhà Ma đã trong tình trạng báo động đỏ; thu nhập từ tiền vé hằng ngày chỉ vừa đủ để trả tiền điện nước.</div>
                        <div>[Mình tự hỏi mình sẽ trụ được bao lâu.]</div>
                        <div>Sau khi dập điều thuốc, khi Trần Ca chuẩn bị quay trở lại Nhà Ma, một ông trú trung niên mặc đồng phục Công Viên Tân Thế Kỷ đi lại chỗ anh. Khi anh thấy ông, Trần Ca tăng tốc độ như chuột thấy mèo.</div>
                        <div>[ Con nghĩ con có thế giả bộ không thấy chú à?] ông chú trung niên năm chặt vai Trần Ca. [Hôm nay, chúng ta chắc chắn cần phải nói chuyện. Con nợ tiền thuê chỗ với tiền điện nước hai tháng rồi. Cấp trên đang sờ gáy chú để đòi tiền đó, nên đưa đây!</div>
                        <div>[Chú Từ, không phải là con không muốn trả chú, nhưng con thật sự không có gì để trả chú cả. Chú cho con thêm một tháng được không?]</div>
                        <div>[Con nói câu này với chú tháng trước roi!</div>
                        <div>[Con hứa, lần này sẽ là lần cuối! Trần Ca thành thật hứa trong khi vỗ ngực.</div>
                        <div>[Mọi người đã không còn ưa chuộng Nhà Ma nữa. Nghe lời chú, cứng đầu không có ích lợi gì đâu.] Khi chú Từ thấy mấy lá thư trên tay Trần Ca, lực nắm trên vai Trần Ca giảm dần. [Con còn trẻ; con vẫn có thể bắt đầu lại từ đầu với nghề khác, sao con lại tự hành hạ bản thân vậy?]</div>
                        <div>[Chú Từ, con biết chú có ý tốt cho con, nhưng Ngôi Nhà Ma này mang ý nghĩa đặc biệt với con. Con đoán rằng con vẫn chưa muốn buông đi ký ức cuối của ba mẹ con.] Trần Ca thốt ra bằng giọng nhỏ như thể sợ người khác nghe thấy.</div>
                        <div>Là quản lý công viên, ông biết về sự biến mất của bố mẹ Trần Ca. Ông không trả lời ngay. Sau vài giây, ông thở dài và nói, [Được rồi, chú hiểu cảm giác của con. Chú sẽ cố hết sức để nói với ban quản lý để xem họ có có thể cho con thêm vài tuần không.]</div>
                        <div>[Cảm ơn, chú Từ!]</div>
                        <div>[Đừng cảm ơn vội, con nên đảm bảo số lượng vé bán tăng không thì kết cục cũng không đổi đâu.]</div>
                        <div>Sau khi tiễn quản lý công viên đi khỏi, Trần Ca trở lại Nhà Ma để bắt đầu bảo dưỡng trang bị và dọn dẹp hằng ngày.</div>
                        <div>[Máu giả trong phòng bảo trì sắp hết; mình phải mua thêm mẻ mới.</div>
                        <div>[Nếu hành lang chỉnh nghiêng qua một chút, thì sẽ tạo điểm mù tốt hơn để dọa khách.]</div>
                        <div>[A chết, con rối này hỏng mất rồi; mình phải sửa nó sau.</div>
                        <div>[F*ck! Bóng đèn tuan trước mình lap ở đây đau roi? Thang nào trộm mất !? ]</div>
                        <div>Trong mất người ngoài, anh là chủ Ngôi Nhà Ma, một nhà doanh nghiệp trẻ theo một cách nào đó, nhưng chỉ bản thân Trần Ca biết sự khó khăn trong việc duy trì một Ngôi Nhà Ma. Nhà Ma là một hình thức giải trí. Đặt trong một môi trường đáng sợ, cơ thể lẫn tâm lý của một người sẽ được duy trì trong trạng thái cao độ, nhưng khi sì trét được giải tỏa, nó sẽ dẫn đến sự nhẹ nhõm và sự thỏa mãn; nó cũng giống mát xa theo một nghĩa.</div>
                        <div>Thường thường, chủ yếu các Ngôi Nhà Ma đều chỉ có một mẹo chuyển để có thể thu hút thêm nhiều mẻ khách mới. Nhà Ma cố định ở một chỗ như của Trần Ca phải có độ nổi tiếng cao để thu hút khách, không thì họ sẽ khó sống sót được lâu. Việc anh đã cẩm cự được lâu như vậy, theo một cách, đã là kỳ tích.</div>
                        <div>Kéo lê con rối bị hỏng, Trần Ca đi vào phòng bảo trì. Anh đã học Thiết Kế Đồ Chơi ở đại học, mọi máy móc và bẫy dùng trong Nhà Ma đều là tự tay anh thiết kế và lắp ráp. Quá trình bảo trì, bao gồm may vá và sơn lại, đều khô khan và lặp lại.</div>
                        <div>[ Vẫn còn thiếu máu giả. Nếu mình nhớ không lầm, trong gác xếp vẫn còn tồn kho.] Nhà Ma được tách làm ba lầu; lầu thứ nhất và thứ hai là khung cảnh đáng sợ, trong khi lầu ba là kho lưu trữ.</div>
                        <div>Đẩy cánh cửa gỗ, ngoài khói gỗ vụn và bụi, có vô số các loại nguyên liệu và vật liệu không dùng tới bỏ lại bởi bố mẹ của Trần Ca khi họ còn quản lý Nhà Ma.</div>
                        <div>Không muốn đối mặt với quá khứ, Trần Ca hiếm khi lên đây.</div>
                        <div>[Giờ nhớ lại, đã gần nữa năm trôi qua rồi.]</div>
                        <div>Ngắm nhìn vô số thiết bị, Trần Ca nhớ lại tuổi thơ của anh. Thời gian đó, gia đình anh đã điều hành một Ngôi Nhà Ma di động, nên anh có cơ hội đi khắp nước với bố mẹ anh. Khi họ bận rộn, họ sẽ để Trần Ca một mình ở hậu trường đồng hành với vô số loại ma quỷ, nên Trần Ca đã được rèn luyện sự gan dạ từ khi còn nhỏ.</div>
                        <div>Xét cho cùng thì, khi bạn bè trang lứa đang chơi với Lego và xêp hình, thì anh lại chạy lung tung với cái đâu người giả.</div>
                        <div>[Đó là những ký ức quá giá.</div>
                        <div>Trần Ca lang thang không mục đích cho tới khi tìm được cái hộp gỗ chứa đựng những di vật của bố mẹ anh. Bên trong là chiếc điện thoại màu đen và một con bút bê thô sơ. Con búp bê này là đồ chơi đầu tiên mà anh làm khi còn nhỏ, nhưng anh không có ký ức nào về chiếc điện thoại. Cả hai đều được tìm thấy trong bệnh viện bỏ hoang ở miền quê, và lý do tại sao bố mẹ anh lại đi đến đó giữa đêm khuya, tới cảnh sát còn không thể đưa anh câu trả lời.</div>
                        <div>[ Cả hai người đang ở đâu?] Trần Ca nhặt con búp bê lên và nhéo cái má phinh phính của nó. Thở dài, anh tự nhủ, [Mình nên đi tìm đống máu giả đó, neu mình không song sót qua mua, mình sẽ thật sự tạm biệt Ngôi Nhà Ma này.</div>
                        <div>Trần Ca đang tự nói với bản thân, nhưng anh nói tới đây, chiếc điện thoại đen, từ nãy giờ im lăng trong hộp, đột nhiên ánh lên một ánh đèn mờ lạnh lẽo.</div>
                        <div>[ Chuyện gì đang xảy ra vậy? Công nghệ đen hay hiện tượng siêu nhiên? Nêu chuyện này xảy ra với người khác, chắc hằn người đó đã vừa chạy vừa la làng rồi, song, phản ứng của Trần Ca bình tĩnh hơn nhiều. Anh cầm chiếc điện thoại lên và kiểm tra nó gần hơn.</div>
                        <div>[ Lạ thật. Mình đã thử mở cái điện thoại này hơn cả trăm lần, nhưng không tác dụng, tại sao hôm nay nó lại tự mở lên? Cái điện thoại này được tìm thấy ở nơi bố mẹ mình mất tích, có khi nào họ biết mình đang gặp rắc rối nên liên lạc để giúp mình?]</div>
                        <div>Trần Ca trượt mở điện thoại, giao diện chính chỉ có hình nền màu đen, và chỉ có duy nhất một app hiện diện. Icon của app có hình dạng Ngôi Nhà Ma.</div>
                        <div>[Khoan đã ... Cái này nhìn quen quen, y hệt trước cong chính Ngôi Nhà Ma của mình]</div>
                        <div>Trong khi cau mày, Trần Ca chạm mở app, và với một hàng chữ nhìn như máu xuất hiện trên màn hình - Bạn có tin trên đời này có Ma?</div>
                        <div>Nói một cách khách quan, đây là câu hỏi triết học trừu tượng; đối với học sinh khoa kỹ sư như Trần Ca, câu hỏi này hầu như không thể trả lời.</div>
                        <div>[ Chắc là có?] Trần Ca lẩm bẩm, vài giây sau, một dòng chữ mới hiện lên màn hình.</div>
                        <div>Cái bạn tin chính là câu trả lời. Từ lúc này trở đi, bạn sẽ chính thức là chủ nhân mới của Ngôi Nhà Ma. Tất nhiên, đây không phải là thứ đáng để ăn mừng. Trước khi kết thúc hướng dẫn, xin hãy nghe lời khuyên cuối của tôi: tự sát là hình vi hèn nhát nhất, hãy cố hết sức để sống sót! ]</div>
                        <div>[ Cái gì là cái gì? Nhưng cái cách hành văn này hơi giống ba của mình.]</div>
                        <div>Trần Ca chạm vào app lần nữa, một cửa sổ mới xuất hiện.</div>
                        <div>Ngôi Nhà Ma Am phía Tây Cửu Giang</div>
                        <div>Trạng thái: Sắp đóng cửa</div>
                        <div>Danh tiếng tôt: Không</div>
                        <div>Khách thăm hằng ngày: Bốn</div>
                        <div>Khách thăm hằng tháng: Mười</div>
                        <div>Đội ngũ Ma và Xác Sông của tôi: Không có</div>
                        <div>Dụng cụ của tôi: Không có</div>
                        <div>Danh hiện đã mở khóa: Không có</div>
                        <div>Khung cảnh sẵn có: [Mảnh Set]:</div>
                        <div>- Đêm của người chết-</div>
                        <div>Đạo cụ kinh khủng, diễn viên không được đào tạo, không có cốt truyện đọc được và phi logic. Mức độ đáng sợ: 0*</div>
                        <div>-Minh hôn [Hôn Lễ Âm Giới]</div>
                        <div>- Cặp đôi bị chia lìa ở Địa Giới, trói buộc với nhau vĩnh viễn ở Âm Giới; sẽ chia cùng nấm mồ, theo đuổi hạnh phúc trong cái chết. Mức độ đáng sợ: 0,5</div>
                        <div>Khung cảnh có thể mở khóa:</div>
                        <div>- Án mạng giữa đêm</div>
                        <div>- Một bệnh nhân tâm thần nguy hiểm đi lang thang khắp căn hộ bỏ hoang. Kéo và Búa trong tay, hằn chỉ quanh quấn bên ngoài cửa phòng bạn thôi. Mức độ đáng sợ: 1+</div>
                        <div>- Sảnh bệnh thứ Ba</div>
                        <div>- Có nhiều tiếng động không thể giải thích phát ra từ bệnh viện bỏ hoang này mỗi đêm. Là một phóng viên báo chí, nhiệm vụ của bạn là giải mã bí ẩn đen tối này. Mức độ đáng sợ: 3*</div>
                        <div>- Xe tang bị ám</div>
                        <div>- Lên xe tang vác theo quan tài, nếu không thể trốn thoát trong vòng một giờ, bạn sẽ vĩnh viễn kẹt trong xe tang. Mức độ đáng sợ: 2+</div>
                        <div>Nhiệm vụ hàng ngày: Hoàn thành nhiệm vụ hàng ngày được cung cấp bởi Ngôi Nhà Ma để mở khóa thêm nhiều khung cảnh đáng sợ. Phần thưởng sẽ tương ứng với độ khó của nhiệm vụ.</div>
                        <div>Điều kiện mở rộng Nhà Ma: Khách thăm hàng tháng hơn 100. Danh tiếng tốt hơn 60 phần trăm. (Sau 3 lần mở rộng, Nhà Ma sẽ nâng cấp thành Mê Cung Ám Ảnh.)</div>
                        <div>Vòng quay vận rủi (Tiêu dùng điểm sợ hãi được tạo bởi khách thăm để quay): Luật lệ của sự sống và cái chết chưa bao giờ là quyền phán quyết của con người; Vận May và Điềm Rủi chỉ cách nhau một li. Chúng tôi có Trái Linh Hồn để tăng tuổi thọ bạn và các Bóng Ma Tai Họa đầy lòng thù hận!</div>
                        <div>Chức năng khác: Chưa mở khóa.</div>
                    """
                },
                new Chapter {
                    ChapterId = 4,
                    StoryId = 2,
                    ChapterNumber = 2,
                    Title = "Nhiệm vụ hằng ngày kỳ lạ",
                    Content = """
                        <div>App có Icon Ngôi Nhà Ma, nhìn tương tự các game quản lý trên điện thoại; tuy nhiên, thay vì quản lý khách sạn, công viên thủy sinh hay cửa hàng thú nuôi, nó lại là quản lý Nhà Ma.</div>
                        <div>Trần Ca nhìn chằm chằm vào màn hình và tự hỏi. Tại sao chiếc điện thoại để lại bởi bố mẹ mình lại có cái app kì lạ này?</div>
                        <div>Anh nhìn qua giao diện của app, tất cả thông tin đều trùng khớp với hoàn cảnh Ngôi Nhà Ma của anh. Việc này tạo cho Trần Ca cảm giác kỳ lạ, cứ như quản lý Nhà Ma trong game không khác với cái mà anh đang quản lý hiện tại.</div>
                        <div>Cả hai đều hết thời và đối mặt với nguy cơ phải đóng cửa; đơn giản là có quá nhiều điểm tương đồng giữa cả hai.</div>
                        <div>[ Có thể nào game này được tạo ra dựa trên Nhà Ma của mình? Vậy có nghĩa là nếu có sự thay đổi trong game, hiện thực cũng bị ảnh hưởng?] Trần Ca lẩm bẩm.</div>
                        <div>Trần Ca tiếp tục đọc; khung cảnh hiện tại của Nhà Ma, Đêm của người chết, bị chỉ trích tàn nhẫn. Thậm chí khung cảnh Minh Hôn đã từng lên báo chỉ được đánh giá 0,5x .</div>
                        <div>[Nếu Minh Hôn chỉ được 0.5 , tưởng tượng đến độ đáng sợ của các khung cảnh đang chờ mở khóa cũng đủ làm mình nổi da gà.] Anh thử ấn vào mục tùy chọn, một cửa số hiện lên màn hình, báo rằng anh nần phải hoàn thành một số nhiệm vụ hằng ngày nhất định nếu muốn mở khóa thêm khung cảnh khác.</div>
                        <div>[ Có vẻ như giải pháp là nhiệm vụ hằng ngày, chỉ khi hoàn thành nhiệm vụ thì mình mới được mở khóa thêm khung cảnh. Bằng cách mở khóa thêm khung cảnh, mình sẽ có thể thu hút thêm khách hàng, cũng như mở rộng Nhà Ma. ] Trần Ca là người chơi mobile game hăng hái, anh nhanh chóng năm bắt luật lệ của game-tỷ lệ hoàn thành nhiệm vụ hằng ngày sẽ ảnh hưởng tới sự phát triển của Ngôi Nhà Ma.</div>
                        <div>Sau khi ấn vào Nhiệm vụ hằng ngày, có ba nhiệm vụ hiện lên:</div>
                        <div>Nhiệm vụ dễ: Có ba yếu tố chính để dựng lên một Ngôi Nhà Ma tốt- Cốt truyện, Khung cảnh và Tâm trạng. Một Ngôi Nhà Ma không có côt truyện là Nhà Ma không có sức sống, xin hãy hoàn thành bối cảnh cho hai khung cảnh, Đêm của người chết và Minh Hôn.</div>
                        <div>Nhiệm vụ thường: Sửa chữa toàn bộ ma-nơ-canh trong Nhà Ma trước nửa đêm.</div>
                        <div>Nhiệm vụ Ác Mộng: Tôi biết bạn vẫn chưa hoàn toàn tin vào sự tồn tại của Ma quỷ; nếu vậy, hay chúng ta chơi một trò chơi? Sự thật sẽ sáng tỏ trước mắt bạn.</div>
                        <div>Nhiệm vụ hằng ngày sẽ tái tạo mỗi ngày vào nửa đêm. Người dùng chỉ được chọn một nhiệm vụ mỗi ngày, phần thưởng sẽ tương ứng với độ khó của nhiệm vụ.</div>
                        <div>(Cảnh báo! Độ khó nhiệm vụ tỷ lệ thuận với độ nguy hiểm, xin hãy chọn cần thận!)</div>
                        <div>Sau khi đọc kỹ nhiệm vụ, Trần Ca há hốc ngạc nhiên. [Nhiệm vụ trong game phải hoàn thành trong đời thực mới được? Không phải đây là bằng chứng hoàn hảo cho việc game có thể ảnh hưởng thực tế !?</div>
                        <div>Để thử nghiệm, anh quyết định chọn một nhiệm vụ. Bởi vì phần thưởng được trao tương ứng với độ khó và anh chỉ có thể chọn một mỗi ngày, để có phần thưởng tốt nhất, anh phải chọn nhiệm vụ khó nhất. Tuy nhiên, cảnh báo đính kèm bên dưới nhiệm làm Trần Ca hơi lo lắng.</div>
                        <div>[Đây là lựa chọn khó. Mô tả của nhiệm vụ Ác Mộng quá mơ hổ; nghe như bẩy ây. Sao mình không bắt đâu bằng nhiệm vụ Thường? Sửa chữa toàn bộ đạo cụ hơi khó nhưng không phải bất khả thi.]</div>
                        <div>Trần Ca là người động tay động chân, sau khi quyết định, anh bắt đâu hành động. Túm lấy hộp dụng cụ và xô đựng máu giả, anh bắt đầu kiểm tra mọi ma-nơ-canh trong Nhà Ma.</div>
                        <div>Đêm đã xuống. Để tiết kiệm điện, Trần Ca thậm chí không bật đèn hành lang. Kẹp lấy đèn pin trong nách, Trần Ca lượn khắp Nhà Ma rộng lớn, sửa chữa toàn bộ ma-cơ-canh cần thiết.</div>
                        <div>Nếu có ai thấy được, chắc họ sẽ bị dọa tới mức phải gọi cảnh sát.</div>
                        <div>[Mình không nghĩ rang có qua nhiều ma-nơ-canh cần phải bảo trì tới vậy; Mình nên tăng tốc thôi!</div>
                        <div>Lúc 11:45 tối, Trần Ca nhận được thông báo hoàn thành nhiệm vụ từ điện thoại. [Bạn đã hoàn thành nhiệm vụ Thường. Để tâm đến các chi tiết sẽ đóng góp cho không khí đáng sợ. Chúc mừng, bạn đã nhận được phần thưởng nhiệm vụ- Nhạc Nền, Black Friday.]</div>
                        <div>[Khoan đã, hình như Black Friday là nhạc bị cấm ở nước ngoài? Theo tin đồn, nó có khả năng kỳ quái khiến cho người nghe có xu hướng tự sát, và bản gốc đã thất lạc từ lâu.] Trần Ca nhận ra trong mục dụng cụ có hình một chiếc CD. [Phần thưởng nhiệm vụ gì thế này, đừng nói toàn bộ đống này là chơi khăm?]</div>
                        <div>Anh ấn vào hình CD, một giai điệu mà anh chưa từng nghe vang lên tai. Giai điệu đẩy sự u ám, buồn rầu và cô đơn. Trần Ca cảm thấy thế giới quanh anh đang rời rạc ra, và anh đang đứng trên hành lang dài bất tận.</div>
                        <div>Khi bài hát kết thúc, lưng anh thấm đẫm mồ hôi. Anh mừng vì mình không chọn chế độ lặp, không thì anh đã bị kẹt bởi ảnh hưởng của giai điệu mãi rồi.</div>
                        <div>[F*ck, đây là thật! Đây là bài hát nguyên bản! ]</div>
                        <div>đời thật. Điều này đem lại cho Trần Ca một tia hi vọng. Anh dừng nhạc và lưu lại cần thận. Sau khi giải quyết mọi thứ, Trần Ca trở về phòng nghỉ để nghỉ ngơi.</div>
                        <div>Năm trên giường, mặc dù mệt, nhưng anh không cảm thấy buồn ngủ chút nào. Sau cùng thì, những thứ anh vừa trãi nghiệm cần chút thời gian để tiêu hóa.</div>
                        <div>Không hay biết, đã qua nửa đêm, và Trần Ca vẫn đang nhìn vô định lên trần nhà.</div>
                        <div>[Không ngủ được tí nào! Chán chường, anh lôi điện thoại đen ra. Nửa đêm đã qua, nên sẽ có nhiệm vụ hàng ngày mới chứ nhỉ?]</div>
                        <div>Anh mở app, và như đã mong đợi, có vài thay đổi ở mục Nhiệm vụ hàng ngày.</div>
                        <div>Nhiệm vụ Dễ: Nếu bạn muốn đem lại cho khách thăm những trãi nghiệm đáng sợ, thì bạn phải để tâm tới chuỗi sự kiện và nhịp độ của họ trong Nhà Ma. Dọa họ quá sớm sẽ khiến khách thăm mất đi hứng thú, đề nghị bạn nên lắp đặt một số thiết bị nhận dạng âm thanh hay máy quay để theo dõi tiên độ của khách thăm.</div>
                        <div>Nhiệm vụ Thường: một cây làm chăng lên non; một Ngôi Nhà Ma tôt cân được vận hành bởi đội ngũ nhân viên tốt. Chiêu mộ thêm nhân tài để trợ giúp cho hành trình của bạn.</div>
                        <div>Nhiệm vụ Ác Mộng: Tôi biết bạn vẫn chưa hoàn toàn tin vào sự tôn tại của Ma quỷ; nều vậy, hay chúng ta chơi một trò chơi? Sự thật sẽ sáng tỏ trước mắt bạn.</div>
                        <div>Nhiệm vụ hằng ngày sẽ tái tạo mỗi ngày vào giửa đêm. Người dùng chỉ được chọn một nhiệm vụ mỗi ngày, phần thưởng sẽ tương ứng với độ khó của nhiệm vụ.</div>
                        <div>(Cảnh báo! Độ khó nhiệm vụ tỷ lệ thuận với độ nguy hiểm, xin hãy chọn cẩn thận!)</div>
                        <div>Ba nhiệm vụ mới có chút trở ngại trong kê hoạch của anh.</div>
                        <div>Nhiệm vụ Dễ là lắp thêm máy quay trong Nhà Ma; cái này có thể thực hiện bằng tiền, nhưng vấn để là ... Ngân sách hiện tại của Trần Ca có hơi hạn chế.</div>
                        <div>Nhiệm vụ Thường cũng không dễ dàng lắm. Hai nhân viên dày dặn, người đã vượt qua bao sóng gió cùng anh, vừa nghỉ việc. Và thậm chí nếu anh tuyển thêm nhân viên lúc này, sẽ mất vài ngày để hoàn thành huấn luyện. Khi nhân viên mới có thể phụ giúp Nhà Ma, chỗ này chắc cũng đã dẹp mất rồi.</div>
                        <div>Do nhiệm vụ Dễ và nhiệm vụ Thường đều không thể làm được, Trần Ca dán mắt vào nhiệm vụ cuối cùng.</div>
                        <div>[Nếu nhiệm vụ càng khó phần thì thưởng càng tốt, mình có nên thử nhiệm vụ Ác Mộng không nhỉ?]</div>
                    """
                }
                    
            );

            // Seed Follower (Test Observer)
            modelBuilder.Entity<StoryFollower>().HasData(
                new StoryFollower { FollowId = -1, UserId = 2, StoryId = 1, CreatedAt = DateTime.UtcNow }
            );

            // Seed ReadingProgress (Test Singleton - Tiến trình đọc duy nhất)
            modelBuilder.Entity<ReadingProgress>().HasData(
                new ReadingProgress
                {
                    ProgressId = -1,
                    UserId = 2,
                    CurrentStoryId = 1,
                    CurrentChapterId = 1,
                    LastReadPosition = 150,
                    TotalStoriesRead = 1,
                    TotalChaptersRead = 1,
                    LastReadAt = DateTime.UtcNow
                }
            );

            // Seed ReadingMode (Test Strategy - Lưu cấu hình đọc của User)
            modelBuilder.Entity<ReadingMode>().HasData(
                new ReadingMode
                {
                    ModeId = -1,
                    UserId = 2,
                    Theme = "Night",
                    NavigationMode = "Scroll",
                    FontSize = 18,
                    FontFamily = "Georgia",
                    LineHeight = 1.8m
                }
            );
        }
    }
}
