-- ============================================================================
-- Story Reading System - Simplified Database Schema
-- Design Patterns: Factory, Singleton, Observer, Strategy
-- ============================================================================

USE master;
GO

-- Drop database if exists
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'StoryReaderDB')
BEGIN
    ALTER DATABASE StoryReaderDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE StoryReaderDB;
END
GO

-- Create database
CREATE DATABASE StoryReaderDB;
GO

USE StoryReaderDB;
GO

-- ============================================================================
-- TABLE: Users
-- Quản lý người dùng cơ bản
-- ============================================================================
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    
    INDEX IX_Users_Username (Username),
    INDEX IX_Users_Email (Email)
);
GO

-- ============================================================================
-- TABLE: Categories
-- FACTORY PATTERN: Các thể loại truyện
-- ============================================================================
CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
);
GO

-- ============================================================================
-- TABLE: Stories
-- FACTORY PATTERN: Truyện được tạo bởi Factory theo CategoryId
-- ============================================================================
CREATE TABLE Stories (
    StoryId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    Description NTEXT NULL,
    CoverImage NVARCHAR(500) NULL,
    AuthorId INT NOT NULL,
    CategoryId INT NOT NULL, -- FACTORY: Thể loại để StoryFactory tạo đối tượng phù hợp
    ViewCount INT NOT NULL DEFAULT 0,
    AverageRating DECIMAL(3,2) NOT NULL DEFAULT 0.00,
    TotalRatings INT NOT NULL DEFAULT 0,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Ongoing', -- Ongoing, Completed, Paused
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    
    CONSTRAINT FK_Stories_Users FOREIGN KEY (AuthorId) 
        REFERENCES Users(UserId) ON DELETE NO ACTION,
    CONSTRAINT FK_Stories_Categories FOREIGN KEY (CategoryId)
        REFERENCES Categories(CategoryId) ON DELETE NO ACTION,
    
    INDEX IX_Stories_AuthorId (AuthorId),
    INDEX IX_Stories_CategoryId (CategoryId),
    INDEX IX_Stories_ViewCount (ViewCount)
);
GO

-- ============================================================================
-- TABLE: Chapters
-- Chương truyện
-- ============================================================================
CREATE TABLE Chapters (
    ChapterId INT IDENTITY(1,1) PRIMARY KEY,
    StoryId INT NOT NULL,
    Title NVARCHAR(255) NOT NULL,
    ChapterNumber INT NOT NULL,
    Content NTEXT NULL,
    ViewCount INT NOT NULL DEFAULT 0,
    PublishedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    
    CONSTRAINT FK_Chapters_Stories FOREIGN KEY (StoryId) 
        REFERENCES Stories(StoryId) ON DELETE CASCADE,
    
    CONSTRAINT UQ_Chapters_StoryId_ChapterNumber UNIQUE (StoryId, ChapterNumber),
    INDEX IX_Chapters_StoryId (StoryId),
    INDEX IX_Chapters_PublishedAt (PublishedAt)
);
GO

-- ============================================================================
-- TABLE: ReadingProgress
-- SINGLETON PATTERN: Mỗi user chỉ có 1 instance duy nhất
-- ============================================================================
CREATE TABLE ReadingProgress (
    ProgressId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL UNIQUE, -- SINGLETON: Mỗi user chỉ có 1 record
    CurrentStoryId INT NULL,
    CurrentChapterId INT NULL,
    LastReadPosition INT NOT NULL DEFAULT 0, -- Vị trí scroll
    TotalStoriesRead INT NOT NULL DEFAULT 0,
    TotalChaptersRead INT NOT NULL DEFAULT 0,
    LastReadAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_ReadingProgress_Users FOREIGN KEY (UserId) 
        REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_ReadingProgress_Stories FOREIGN KEY (CurrentStoryId)
        REFERENCES Stories(StoryId) ON DELETE NO ACTION,
    CONSTRAINT FK_ReadingProgress_Chapters FOREIGN KEY (CurrentChapterId)
        REFERENCES Chapters(ChapterId) ON DELETE NO ACTION,
    
    CONSTRAINT UQ_ReadingProgress_UserId UNIQUE (UserId), -- SINGLETON constraint
    INDEX IX_ReadingProgress_UserId (UserId)
);
GO

-- ============================================================================
-- TABLE: ReadingModes
-- STRATEGY PATTERN: Chế độ đọc (Ban ngày, Ban đêm, Cuộn, Lật trang)
-- ============================================================================
CREATE TABLE ReadingModes (
    ModeId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL UNIQUE, -- STRATEGY: Mỗi user có 1 cấu hình chế độ đọc
    Theme NVARCHAR(20) NOT NULL DEFAULT 'Day', -- Day, Night
    NavigationMode NVARCHAR(20) NOT NULL DEFAULT 'Scroll', -- Scroll, Flip
    FontSize INT NOT NULL DEFAULT 16,
    FontFamily NVARCHAR(50) NOT NULL DEFAULT 'Arial',
    LineHeight DECIMAL(3,1) NOT NULL DEFAULT 1.5,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_ReadingModes_Users FOREIGN KEY (UserId)
        REFERENCES Users(UserId) ON DELETE CASCADE,

    CONSTRAINT UQ_ReadingModes_UserId UNIQUE (UserId),
    INDEX IX_ReadingModes_UserId (UserId)
);
GO

-- ============================================================================
-- TABLE: Bookmarks
-- Đánh dấu chương đang đọc để tiếp tục lần sau
-- ============================================================================
CREATE TABLE Bookmarks (
    BookmarkId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    StoryId INT NOT NULL,
    ChapterId INT NOT NULL,
    ScrollPosition INT NOT NULL DEFAULT 0,
    LastReadAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Bookmarks_Users FOREIGN KEY (UserId)
        REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_Bookmarks_Stories FOREIGN KEY (StoryId)
        REFERENCES Stories(StoryId) ON DELETE NO ACTION,
    CONSTRAINT FK_Bookmarks_Chapters FOREIGN KEY (ChapterId)
        REFERENCES Chapters(ChapterId) ON DELETE NO ACTION,

    CONSTRAINT UQ_Bookmarks_UserId_StoryId UNIQUE (UserId, StoryId),
    INDEX IX_Bookmarks_UserId (UserId),
    INDEX IX_Bookmarks_StoryId (StoryId)
);
GO

-- ============================================================================
-- TABLE: Comments
-- Bình luận truyện
-- ============================================================================
CREATE TABLE Comments (
    CommentId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    StoryId INT NOT NULL,
    Content NTEXT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_Comments_Users FOREIGN KEY (UserId)
        REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_Comments_Stories FOREIGN KEY (StoryId)
        REFERENCES Stories(StoryId) ON DELETE NO ACTION,

    INDEX IX_Comments_UserId (UserId),
    INDEX IX_Comments_StoryId (StoryId),
    INDEX IX_Comments_CreatedAt (CreatedAt)
);
GO

-- ============================================================================
-- TABLE: Ratings
-- Đánh giá truyện
-- ============================================================================
CREATE TABLE Ratings (
    RatingId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    StoryId INT NOT NULL,
    Score INT NOT NULL CHECK (Score >= 1 AND Score <= 5),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Ratings_Users FOREIGN KEY (UserId)
        REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_Ratings_Stories FOREIGN KEY (StoryId)
        REFERENCES Stories(StoryId) ON DELETE NO ACTION,

    CONSTRAINT UQ_Ratings_UserId_StoryId UNIQUE (UserId, StoryId),
    INDEX IX_Ratings_UserId (UserId),
    INDEX IX_Ratings_StoryId (StoryId)
);
GO

-- ============================================================================
-- TABLE: Notifications
-- OBSERVER PATTERN: Thông báo khi có chương mới
-- ============================================================================
CREATE TABLE Notifications (
    NotificationId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    StoryId INT NOT NULL,
    ChapterId INT NULL, -- NULL nếu thông báo không liên quan đến chương cụ thể
    Message NVARCHAR(500) NOT NULL,
    Type NVARCHAR(50) NOT NULL DEFAULT 'NewChapter', -- NewChapter, System
    IsRead BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId)
        REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_Notifications_Stories FOREIGN KEY (StoryId)
        REFERENCES Stories(StoryId) ON DELETE NO ACTION,
    CONSTRAINT FK_Notifications_Chapters FOREIGN KEY (ChapterId)
        REFERENCES Chapters(ChapterId) ON DELETE NO ACTION,

    INDEX IX_Notifications_UserId_IsRead (UserId, IsRead),
    INDEX IX_Notifications_CreatedAt (CreatedAt)
);
GO

-- ============================================================================
-- TABLE: StoryFollowers
-- OBSERVER PATTERN: User theo dõi truyện để nhận thông báo
-- ============================================================================
CREATE TABLE StoryFollowers (
    FollowId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    StoryId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_StoryFollowers_Users FOREIGN KEY (UserId)
        REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_StoryFollowers_Stories FOREIGN KEY (StoryId)
        REFERENCES Stories(StoryId) ON DELETE NO ACTION,

    CONSTRAINT UQ_StoryFollowers_UserId_StoryId UNIQUE (UserId, StoryId),
    INDEX IX_StoryFollowers_UserId (UserId),
    INDEX IX_StoryFollowers_StoryId (StoryId)
);
GO

-- ============================================================================
-- SEED DATA: Categories (cho Factory Pattern)
-- ============================================================================
SET IDENTITY_INSERT Categories ON;

INSERT INTO Categories (CategoryId, Name, IsActive) VALUES
(1, N'Hành Động', 1),
(2, N'Kinh Dị', 1),
(3, N'Lãng Mạn', 1),
(4, N'Trinh Thám', 1),
(5, N'Khoa Học Viễn Tưởng', 1),
(6, N'Hài Hước', 1);

SET IDENTITY_INSERT Categories OFF;
GO

-- ============================================================================
-- TRIGGERS
-- ============================================================================

-- Trigger: Update Story Rating khi có rating mới
CREATE TRIGGER trg_UpdateStoryRating
ON Ratings
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AffectedStories TABLE (StoryId INT);

    INSERT INTO @AffectedStories
    SELECT DISTINCT StoryId FROM inserted
    UNION
    SELECT DISTINCT StoryId FROM deleted;

    UPDATE s
    SET
        s.AverageRating = ISNULL((
            SELECT AVG(CAST(r.Score AS DECIMAL(3,2)))
            FROM Ratings r
            WHERE r.StoryId = s.StoryId
        ), 0),
        s.TotalRatings = (
            SELECT COUNT(*)
            FROM Ratings r
            WHERE r.StoryId = s.StoryId
        ),
        s.UpdatedAt = GETUTCDATE()
    FROM Stories s
    INNER JOIN @AffectedStories a ON s.StoryId = a.StoryId;
END;
GO

-- Trigger: OBSERVER PATTERN - Gửi thông báo khi có chương mới
CREATE TRIGGER trg_NotifyNewChapter
ON Chapters
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Gửi thông báo cho tất cả followers của truyện
    INSERT INTO Notifications (UserId, StoryId, ChapterId, Message, Type, IsRead, CreatedAt)
    SELECT 
        sf.UserId,
        i.StoryId,
        i.ChapterId,
        N'Chương mới "' + i.Title + N'" vừa được cập nhật!',
        'NewChapter',
        0,
        GETUTCDATE()
    FROM inserted i
    INNER JOIN StoryFollowers sf ON i.StoryId = sf.StoryId
    WHERE i.IsActive = 1;
END;
GO

-- ============================================================================
-- STORED PROCEDURES
-- ============================================================================

-- Procedure: FACTORY PATTERN - Lấy story theo category
CREATE PROCEDURE sp_GetStoriesByCategory
    @CategoryId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.StoryId,
        s.Title,
        s.Description,
        s.CoverImage,
        s.ViewCount,
        s.AverageRating,
        s.TotalRatings,
        s.Status,
        c.Name AS CategoryName,
        u.Username AS AuthorName
    FROM Stories s
    INNER JOIN Categories c ON s.CategoryId = c.CategoryId
    INNER JOIN Users u ON s.AuthorId = u.UserId
    WHERE s.CategoryId = @CategoryId 
      AND s.IsActive = 1
      AND c.IsActive = 1
    ORDER BY s.ViewCount DESC, s.CreatedAt DESC;
END;
GO

-- Procedure: SINGLETON PATTERN - Lấy hoặc tạo ReadingProgress cho user
CREATE PROCEDURE sp_GetOrCreateReadingProgress
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Kiểm tra xem đã có chưa
    IF NOT EXISTS (SELECT 1 FROM ReadingProgress WHERE UserId = @UserId)
    BEGIN
        -- Tạo mới nếu chưa có (SINGLETON initialization)
        INSERT INTO ReadingProgress (UserId, LastReadAt, CreatedAt)
        VALUES (@UserId, GETUTCDATE(), GETUTCDATE());
    END

    -- Trả về instance duy nhất
    SELECT * FROM ReadingProgress WHERE UserId = @UserId;
END;
GO

-- Procedure: OBSERVER PATTERN - Theo dõi truyện
CREATE PROCEDURE sp_FollowStory
    @UserId INT,
    @StoryId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Thêm vào danh sách theo dõi
    IF NOT EXISTS (
        SELECT 1 FROM StoryFollowers 
        WHERE UserId = @UserId AND StoryId = @StoryId
    )
    BEGIN
        INSERT INTO StoryFollowers (UserId, StoryId, CreatedAt)
        VALUES (@UserId, @StoryId, GETUTCDATE());

        SELECT 'Success' AS Status, 'Followed story successfully' AS Message;
    END
    ELSE
    BEGIN
        SELECT 'Already Following' AS Status, 'You are already following this story' AS Message;
    END
END;
GO

-- Procedure: STRATEGY PATTERN - Cập nhật chế độ đọc
CREATE PROCEDURE sp_UpdateReadingMode
    @UserId INT,
    @Theme NVARCHAR(20),
    @NavigationMode NVARCHAR(20),
    @FontSize INT = NULL,
    @FontFamily NVARCHAR(50) = NULL,
    @LineHeight DECIMAL(3,1) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Kiểm tra xem đã có chưa
    IF NOT EXISTS (SELECT 1 FROM ReadingModes WHERE UserId = @UserId)
    BEGIN
        -- Tạo mới
        INSERT INTO ReadingModes (
            UserId, Theme, NavigationMode, 
            FontSize, FontFamily, LineHeight, CreatedAt
        )
        VALUES (
            @UserId, @Theme, @NavigationMode,
            ISNULL(@FontSize, 16),
            ISNULL(@FontFamily, 'Arial'),
            ISNULL(@LineHeight, 1.5),
            GETUTCDATE()
        );
    END
    ELSE
    BEGIN
        -- Cập nhật
        UPDATE ReadingModes
        SET
            Theme = @Theme,
            NavigationMode = @NavigationMode,
            FontSize = ISNULL(@FontSize, FontSize),
            FontFamily = ISNULL(@FontFamily, FontFamily),
            LineHeight = ISNULL(@LineHeight, LineHeight),
            UpdatedAt = GETUTCDATE()
        WHERE UserId = @UserId;
    END

    -- Trả về cấu hình hiện tại
    SELECT * FROM ReadingModes WHERE UserId = @UserId;
END;
GO

-- ============================================================================
-- VIEWS
-- ============================================================================

-- View: Danh sách truyện với thông tin đầy đủ
CREATE VIEW vw_StoriesWithDetails AS
SELECT
    s.StoryId,
    s.Title,
    s.Description,
    s.CoverImage,
    s.ViewCount,
    s.AverageRating,
    s.TotalRatings,
    s.Status,
    s.CreatedAt,
    c.CategoryId,
    c.Name AS CategoryName,
    u.UserId AS AuthorId,
    u.Username AS AuthorName,
    (SELECT COUNT(*) FROM Chapters ch WHERE ch.StoryId = s.StoryId AND ch.IsActive = 1) AS TotalChapters,
    (SELECT MAX(ch.PublishedAt) FROM Chapters ch WHERE ch.StoryId = s.StoryId AND ch.IsActive = 1) AS LatestChapterDate
FROM Stories s
INNER JOIN Categories c ON s.CategoryId = c.CategoryId
INNER JOIN Users u ON s.AuthorId = u.UserId
WHERE s.IsActive = 1 AND c.IsActive = 1;
GO

-- ============================================================================
-- DATABASE SCHEMA COMPLETE
-- ============================================================================
PRINT '============================================================';
PRINT 'Story Reader Database - Simplified Schema Created!';
PRINT '============================================================';
PRINT 'Total Tables: 11';
PRINT '';
PRINT 'Design Patterns Implemented:';
PRINT '  ✓ Factory Pattern: Categories, Stories';
PRINT '    - StoryFactory tạo đối tượng story theo CategoryId';
PRINT '    - sp_GetStoriesByCategory để lấy stories theo category';
PRINT '';
PRINT '  ✓ Singleton Pattern: ReadingProgress';
PRINT '    - Mỗi user chỉ có 1 instance duy nhất (UNIQUE constraint)';
PRINT '    - sp_GetOrCreateReadingProgress đảm bảo singleton';
PRINT '';
PRINT '  ✓ Observer Pattern: Notifications, StoryFollowers';
PRINT '    - Followers nhận thông báo tự động khi có chương mới';
PRINT '    - trg_NotifyNewChapter trigger gửi notifications';
PRINT '    - sp_FollowStory để theo dõi truyện';
PRINT '';
PRINT '  ✓ Strategy Pattern: ReadingModes';
PRINT '    - Chế độ đọc: Day/Night, Scroll/Flip';
PRINT '    - sp_UpdateReadingMode để thay đổi strategy';
PRINT '';
PRINT 'Core Features:';
PRINT '  ✓ Hiển thị danh sách truyện theo thể loại';
PRINT '  ✓ Đọc truyện với nhiều chế độ';
PRINT '  ✓ Đánh dấu chương đang đọc (Bookmarks)';
PRINT '  ✓ Bình luận và đánh giá truyện';
PRINT '  ✓ Thông báo tự động khi có chương mới';
PRINT '============================================================';
GO