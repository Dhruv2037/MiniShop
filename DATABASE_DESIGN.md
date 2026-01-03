# 🗄️ Production Database Design & Schema

## Overview

Comprehensive database design for a Facebook-like social platform, optimized for scale, performance, and maintainability.

---

## 1. Entity-Relationship Diagram (ERD)

```
┌─────────────────┐
│     Users       │
├─────────────────┤
│ Id (PK)         │
│ Email (UNIQUE)  │
│ Username        │
│ PasswordHash    │
│ FirstName       │
│ LastName        │
│ Bio             │
│ ProfileImage    │
│ CoverImage      │
│ IsActive        │
│ CreatedAt       │
│ UpdatedAt       │
└────────┬────────┘
         │
      1:N│ 1:N
     ┌───┴────────┬─────────────┬──────────────┬───────────────┐
     │            │             │              │               │
┌────▼────┐ ┌────▼────┐ ┌─────▼────┐ ┌──────▼──────┐ ┌──────▼─────┐
│  Posts  │ │  Likes  │ │ Comments │ │  Messages   │ │Notifications│
│         │ │         │ │          │ │             │ │             │
│ UserId  │ │ UserId  │ │ UserId   │ │ SenderId    │ │ UserId      │
│ PostId  │ │ PostId  │ │ PostId   │ │ RecipientId │ │ RelatedUser │
└─────────┘ └─────────┘ └──────────┘ │ Content     │ │ RelatedPost │
                                      │ CreatedAt   │ │ Type        │
    ┌──────────────────────────┐      │ IsRead      │ └─────────────┘
    │   UserFollows            │      └─────────────┘
    ├──────────────────────────┤
    │ FollowerId (PK/FK)       │
    │ FollowingId (PK/FK)      │
    │ CreatedAt                │
    └──────────────────────────┘
```

---

## 2. Complete Schema Definition

### Users Table

```sql
CREATE TABLE dbo.Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Email NVARCHAR(255) NOT NULL UNIQUE,
    Username NVARCHAR(128) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    FirstName NVARCHAR(128),
    LastName NVARCHAR(128),
    Bio NVARCHAR(500),
    ProfileImageUrl NVARCHAR(500),
    CoverImageUrl NVARCHAR(500),
    PhoneNumber NVARCHAR(20),
    DateOfBirth DATE,
    City NVARCHAR(100),
    Country NVARCHAR(100),
    Website NVARCHAR(500),
    IsActive BIT NOT NULL DEFAULT 1,
    IsVerified BIT NOT NULL DEFAULT 0,
    LastLogin DATETIME,
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    DeletedAt DATETIME NULL,
    
    -- Indexes
    INDEX IX_Email NONCLUSTERED (Email),
    INDEX IX_Username NONCLUSTERED (Username),
    INDEX IX_IsActive NONCLUSTERED (IsActive),
    INDEX IX_CreatedAt NONCLUSTERED (CreatedAt DESC)
);

-- Audit trigger for UpdatedAt
CREATE TRIGGER Users_UpdatedAt
ON dbo.Users
AFTER UPDATE
AS
BEGIN
    UPDATE dbo.Users
    SET UpdatedAt = GETUTCDATE()
    WHERE Id IN (SELECT Id FROM inserted)
END;
```

### UserFollows Table (Many-to-Many)

```sql
CREATE TABLE dbo.UserFollows (
    Id INT PRIMARY KEY IDENTITY(1,1),
    FollowerId INT NOT NULL,
    FollowingId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    
    -- Constraints
    FOREIGN KEY (FollowerId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (FollowingId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    
    -- Unique constraint to prevent duplicate follows
    CONSTRAINT UQ_UserFollows_Follower_Following 
        UNIQUE NONCLUSTERED (FollowerId, FollowingId),
    
    -- Self-reference check (can't follow yourself)
    CHECK (FollowerId <> FollowingId),
    
    -- Indexes for efficient queries
    INDEX IX_FollowerId NONCLUSTERED (FollowerId),
    INDEX IX_FollowingId NONCLUSTERED (FollowingId),
    INDEX IX_CreatedAt NONCLUSTERED (CreatedAt DESC)
);
```

### Posts Table

```sql
CREATE TABLE dbo.Posts (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    ImageUrls NVARCHAR(MAX), -- JSON array of image URLs
    Visibility NVARCHAR(50) NOT NULL DEFAULT 'Public', -- Public, Friends, Private
    LikeCount INT NOT NULL DEFAULT 0,
    CommentCount INT NOT NULL DEFAULT 0,
    ShareCount INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    DeletedAt DATETIME NULL,
    
    -- Constraints
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    
    -- Indexes
    INDEX IX_UserId NONCLUSTERED (UserId, CreatedAt DESC),
    INDEX IX_Visibility NONCLUSTERED (Visibility),
    INDEX IX_CreatedAt NONCLUSTERED (CreatedAt DESC),
    INDEX IX_LikeCount NONCLUSTERED (LikeCount DESC),
    
    -- Full-text search
    FULLTEXT INDEX IX_FT_Posts_Content ON dbo.Posts(Content)
);
```

### Likes Table

```sql
CREATE TABLE dbo.Likes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    PostId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    
    -- Constraints
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (PostId) REFERENCES dbo.Posts(Id) ON DELETE CASCADE,
    
    -- Unique constraint (one like per user per post)
    CONSTRAINT UQ_Likes_User_Post UNIQUE NONCLUSTERED (UserId, PostId),
    
    -- Indexes
    INDEX IX_PostId NONCLUSTERED (PostId),
    INDEX IX_UserId NONCLUSTERED (UserId)
);

-- Trigger to update Posts.LikeCount
CREATE TRIGGER Likes_UpdatePostCount
ON dbo.Likes
AFTER INSERT, DELETE
AS
BEGIN
    UPDATE dbo.Posts
    SET LikeCount = (SELECT COUNT(*) FROM dbo.Likes WHERE PostId = inserted.PostId)
    WHERE Id IN (SELECT PostId FROM inserted)
END;
```

### Comments Table

```sql
CREATE TABLE dbo.Comments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    PostId INT NOT NULL,
    UserId INT NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    LikeCount INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    DeletedAt DATETIME NULL,
    
    -- Constraints
    FOREIGN KEY (PostId) REFERENCES dbo.Posts(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    
    -- Indexes
    INDEX IX_PostId NONCLUSTERED (PostId, CreatedAt DESC),
    INDEX IX_UserId NONCLUSTERED (UserId),
    INDEX IX_CreatedAt NONCLUSTERED (CreatedAt DESC)
);
```

### Messages Table

```sql
CREATE TABLE dbo.Conversations (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Participant1Id INT NOT NULL,
    Participant2Id INT NOT NULL,
    LastMessageAt DATETIME,
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    
    -- Constraints
    FOREIGN KEY (Participant1Id) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (Participant2Id) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    
    -- Unique constraint (one conversation per pair)
    CONSTRAINT UQ_Conversations_Participants 
        UNIQUE NONCLUSTERED (
            CASE WHEN Participant1Id < Participant2Id THEN Participant1Id ELSE Participant2Id END,
            CASE WHEN Participant1Id < Participant2Id THEN Participant2Id ELSE Participant1Id END
        ),
    
    -- Indexes
    INDEX IX_Participant1 NONCLUSTERED (Participant1Id),
    INDEX IX_Participant2 NONCLUSTERED (Participant2Id),
    INDEX IX_LastMessageAt NONCLUSTERED (LastMessageAt DESC)
);

CREATE TABLE dbo.Messages (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ConversationId INT NOT NULL,
    SenderId INT NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    ImageUrl NVARCHAR(500),
    IsRead BIT NOT NULL DEFAULT 0,
    ReadAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    
    -- Constraints
    FOREIGN KEY (ConversationId) REFERENCES dbo.Conversations(Id) ON DELETE CASCADE,
    FOREIGN KEY (SenderId) REFERENCES dbo.Users(Id) ON DELETE NO ACTION,
    
    -- Indexes
    INDEX IX_ConversationId NONCLUSTERED (ConversationId, CreatedAt DESC),
    INDEX IX_IsRead NONCLUSTERED (IsRead),
    INDEX IX_CreatedAt NONCLUSTERED (CreatedAt DESC)
);
```

### Notifications Table

```sql
CREATE TABLE dbo.Notifications (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Type NVARCHAR(50) NOT NULL, -- Like, Comment, Follow, Message, Share
    RelatedUserId INT,
    RelatedPostId INT,
    RelatedCommentId INT,
    Message NVARCHAR(500),
    IsRead BIT NOT NULL DEFAULT 0,
    ReadAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    
    -- Constraints
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (RelatedUserId) REFERENCES dbo.Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (RelatedPostId) REFERENCES dbo.Posts(Id) ON DELETE SET NULL,
    FOREIGN KEY (RelatedCommentId) REFERENCES dbo.Comments(Id) ON DELETE SET NULL,
    
    -- Indexes
    INDEX IX_UserId NONCLUSTERED (UserId, CreatedAt DESC),
    INDEX IX_IsRead NONCLUSTERED (IsRead),
    INDEX IX_Type NONCLUSTERED (Type),
    INDEX IX_CreatedAt NONCLUSTERED (CreatedAt DESC)
);
```

### Media Table

```sql
CREATE TABLE dbo.Media (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    FileUrl NVARCHAR(500) NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    FileType NVARCHAR(50) NOT NULL, -- image, video, document
    FileSizeBytes BIGINT NOT NULL,
    Width INT,
    Height INT,
    Metadata NVARCHAR(MAX), -- JSON metadata
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    
    -- Constraints
    FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    
    -- Indexes
    INDEX IX_UserId NONCLUSTERED (UserId),
    INDEX IX_FileType NONCLUSTERED (FileType),
    INDEX IX_CreatedAt NONCLUSTERED (CreatedAt DESC)
);
```

---

## 3. Indexing Strategy

### Query Performance Patterns

```
FREQUENTLY QUERIED:
1. Get user posts → Index on (UserId, CreatedAt DESC)
2. Get user followers → Index on (FollowingId, CreatedAt DESC)
3. Get feed → Index on (CreatedAt DESC) + filter by followers
4. Get likes on post → Index on (PostId)
5. Get unread messages → Index on (ConversationId, IsRead)
6. Get unread notifications → Index on (UserId, IsRead)

RARELY QUERIED:
1. Aggregate statistics
2. Historical analysis
3. Bulk updates
```

### Index Maintenance

```sql
-- Weekly index fragmentation check
SELECT 
    OBJECT_NAME(ips.object_id) AS TableName,
    i.name AS IndexName,
    ips.avg_fragmentation_in_percent AS Fragmentation
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
INNER JOIN sys.indexes i ON ips.object_id = i.object_id 
    AND ips.index_id = i.index_id
WHERE ips.avg_fragmentation_in_percent > 10
ORDER BY ips.avg_fragmentation_in_percent DESC;

-- Rebuild fragmented indexes (monthly)
ALTER INDEX ALL ON dbo.Posts REBUILD;
ALTER INDEX ALL ON dbo.UserFollows REBUILD;
```

---

## 4. Partitioning Strategy (For Scale)

```sql
-- Partition Posts by date (for efficient archiving)
CREATE PARTITION FUNCTION PF_PostsDate (DATETIME)
AS RANGE LEFT FOR VALUES (
    '2026-01-01', '2026-02-01', '2026-03-01', '2026-04-01'
);

CREATE PARTITION SCHEME PS_PostsDate
AS PARTITION PF_PostsDate TO ([PRIMARY], [PRIMARY], [PRIMARY], [PRIMARY], [PRIMARY]);

-- Apply to Posts table (on next major version)
-- Move Posts to Posts_Partitioned with partitioned schema
```

---

## 5. Data Retention Policy

### Deletion Rules

```
Entity          | Retention | Rules
----------------+----------+---------------------------------------------------
Users           | Forever   | Soft delete (DeletedAt), anonymize after 1 year
Posts           | Forever   | Soft delete, hard delete after user deletion
Comments        | Forever   | Soft delete, cascade delete on post delete
Likes           | Forever   | Immediate deletion on request
Messages        | 2 years   | Archive to cold storage, hard delete after
Notifications   | 1 year    | Auto-delete read notifications
Media           | Lifetime  | Hard delete when referenced post deleted
User Follows    | Forever   | Immediate deletion on unfollow
```

---

## 6. Backup & Disaster Recovery

### Backup Strategy

```
Backup Type     | Frequency   | Retention | Purpose
----------------+------------+-----------+----------------------------------
Full Backup     | Daily 3am   | 30 days   | Complete restore point
Differential    | Every 6h    | 7 days    | Faster restore
Transaction Log | Every 15min | 7 days    | Point-in-time recovery

Recovery Time Objective (RTO): 1 hour
Recovery Point Objective (RPO): 15 minutes
```

### Backup Testing

```sql
-- Monthly backup restoration test
RESTORE DATABASE [MiniShop_Test]
FROM DISK = 'E:\Backups\MiniShop_full_backup.bak'
WITH REPLACE;

-- Validate data integrity
DBCC CHECKDB ([MiniShop_Test]);
```

---

## 7. Performance Tuning

### Query Optimization

```sql
-- Get user feed (optimized)
SELECT TOP 20
    p.Id, p.Content, p.CreatedAt,
    u.Id, u.Username, u.ProfileImageUrl,
    (SELECT COUNT(*) FROM dbo.Likes WHERE PostId = p.Id) AS LikeCount,
    (SELECT COUNT(*) FROM dbo.Comments WHERE PostId = p.Id) AS CommentCount
FROM dbo.Posts p
INNER JOIN dbo.Users u ON p.UserId = u.Id
WHERE p.UserId IN (
    SELECT FollowingId FROM dbo.UserFollows 
    WHERE FollowerId = @UserId
    UNION ALL
    SELECT @UserId  -- Include own posts
)
AND p.Visibility = 'Public'
ORDER BY p.CreatedAt DESC;

-- Expected execution time: < 100ms
```

### Caching Strategy (Redis)

```
Key Pattern             | TTL    | Purpose
-----------------------+--------+--------------------------------------
user:{id}              | 1 hour | User profile
feed:{id}              | 5 min  | User's feed
post:{id}:likes        | 10 min | Post likes count
post:{id}:comments     | 10 min | Post comments
followers:{id}         | 1 hour | User's followers list
following:{id}         | 1 hour | User's following list
notifications:{id}     | 5 min  | Unread notifications
```

---

## 8. Migration Plan

### Phase 1: Current State (Week 1)
- Products, Orders tables (e-commerce)

### Phase 2: Social Foundation (Weeks 2-3)
- Create Users, UserFollows, Posts
- Migrate old order data to archive

### Phase 3: Engagement (Weeks 4-5)
- Add Likes, Comments, Notifications
- Update indexes

### Phase 4: Messaging (Weeks 6-7)
- Add Conversations, Messages
- Archive old products

### Phase 5: Scale (Weeks 8+)
- Implement partitioning
- Optimize indexes
- Archive historical data

---

## 9. Monitoring & Alerts

```sql
-- Daily health check queries
-- Monitor table sizes
SELECT 
    t.name AS TableName,
    SUM(p.rows) AS RowCount,
    SUM(au.total_pages) * 8 / 1024 AS SizeMB
FROM sys.tables t
INNER JOIN sys.partitions p ON t.object_id = p.object_id
INNER JOIN sys.allocation_units au ON p.partition_id = au.container_id
GROUP BY t.name
ORDER BY SUM(au.total_pages) DESC;

-- Monitor slow queries
SELECT TOP 10
    qs.sql_handle,
    qs.execution_count,
    qs.total_elapsed_time / 1000000 AS TotalElapsedMS,
    qs.total_elapsed_time / qs.execution_count / 1000 AS AvgElapsedMS,
    SUBSTRING(st.text, 1, 100) AS QueryText
FROM sys.dm_exec_query_stats qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) st
ORDER BY qs.total_elapsed_time DESC;
```

---

## 10. Database Change Management

### PR Checklist for Schema Changes

- [ ] Migration script written (up & down)
- [ ] Backward compatible (no breaking changes)
- [ ] Tested in integration environment
- [ ] No data loss risk identified
- [ ] Rollback plan documented
- [ ] Performance impact assessed
- [ ] Documentation updated
- [ ] Code review approved

### Example Migration (EF Core)

```csharp
public partial class AddUserFollows : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserFollows",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                FollowerId = table.Column<int>(nullable: false),
                FollowingId = table.Column<int>(nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false, 
                    defaultValueSql: "GETUTCDATE()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserFollows", x => x.Id);
                table.UniqueConstraint("UQ_UserFollows_Follower_Following", 
                    x => new { x.FollowerId, x.FollowingId });
                table.ForeignKey("FK_UserFollows_Users_FollowerId",
                    x => x.FollowerId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "UserFollows");
    }
}
```

---

**Version:** 1.0.0  
**Created:** January 2, 2026  
**Status:** Production-Ready Design
