# 🏗️ Enterprise Architecture & Design Patterns

## 1. System Architecture Layers

```
┌──────────────────────────────────────────────────────────┐
│          Presentation Layer (Client)                      │
│     React 18 + Redux Toolkit + TypeScript                │
│     (Web, Mobile-Responsive, PWA-ready)                  │
└────────────────────┬─────────────────────────────────────┘
                     │ HTTP/REST + WebSocket
┌────────────────────▼─────────────────────────────────────┐
│         API Gateway / BFF Layer                           │
│  • Request routing to microservices                       │
│  • Authentication/Authorization                          │
│  • Rate limiting & throttling                            │
│  • Request/Response transformation                       │
│  • Aggregation of multiple service calls                 │
└────────────────────┬─────────────────────────────────────┘
                     │
        ┌────────────┼────────────┬──────────────┐
        │            │            │              │
┌───────▼──┐ ┌──────▼──┐ ┌──────▼──┐ ┌────────▼──┐
│  User    │ │  Post   │ │ Message │ │ Notification
│ Service  │ │ Service │ │ Service │ │ Service
│          │ │         │ │         │ │
│ • Auth   │ │ • CRUD  │ │ • Send  │ │ • Queue
│ • Profile│ │ • Feed  │ │ • History│ │ • Process
│ • Follow │ │ • Search│ │ • RealTime│ │ • Deliver
└────┬─────┘ └────┬────┘ └────┬────┘ └────┬──────┘
     │            │           │           │
     └────────────┼───────────┼───────────┘
                  │
      ┌───────────┴───────────┐
      │   Data Access Layer    │
      ├───────────────────────┤
      │ • Repository Pattern  │
      │ • Unit of Work        │
      │ • ORM (EF Core)       │
      └───────────┬───────────┘
                  │
      ┌───────────┴───────────┐
      │   Persistence Layer    │
      ├───────────────────────┤
      │ • SQL Server          │
      │ • Redis (Cache)       │
      │ • RabbitMQ (Queue)    │
      │ • Elasticsearch       │
      └───────────────────────┘
```

---

## 2. Design Patterns Used

### 2.1 Repository Pattern

```csharp
// Abstraction
public interface IUserRepository
{
    Task<User> GetByIdAsync(int id);
    Task<User> GetByEmailAsync(string email);
    Task<List<User>> GetFollowersAsync(int userId);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}

// Implementation
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    public async Task<User> GetByIdAsync(int id)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<List<User>> GetFollowersAsync(int userId)
    {
        return await _context.UserFollows
            .Where(uf => uf.FollowingId == userId)
            .Include(uf => uf.Follower)
            .AsNoTracking()
            .Select(uf => uf.Follower)
            .ToListAsync();
    }
}

// Dependency Injection
services.AddScoped<IUserRepository, UserRepository>();
```

### 2.2 Unit of Work Pattern

```csharp
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IPostRepository Posts { get; }
    ICommentRepository Comments { get; }
    
    Task<int> SaveChangesAsync();
    Task<bool> BeginTransactionAsync();
    Task<bool> CommitAsync();
    Task<bool> RollbackAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IUserRepository _userRepository;
    private IPostRepository _postRepository;

    public IUserRepository Users => 
        _userRepository ??= new UserRepository(_context);
    
    public IPostRepository Posts => 
        _postRepository ??= new PostRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

// Usage in service
public class PostService
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task CreatePostAsync(CreatePostRequest request)
    {
        var post = new Post { Content = request.Content, UserId = request.UserId };
        await _unitOfWork.Posts.AddAsync(post);
        await _unitOfWork.SaveChangesAsync();
    }
}
```

### 2.3 CQRS (Command Query Responsibility Segregation)

```csharp
// Queries (Read operations)
public class GetUserFeedQuery : IRequest<List<PostDto>>
{
    public int UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetUserFeedQueryHandler : IRequestHandler<GetUserFeedQuery, List<PostDto>>
{
    private readonly IPostRepository _postRepository;
    
    public async Task<List<PostDto>> Handle(GetUserFeedQuery request, CancellationToken ct)
    {
        var posts = await _postRepository.GetFeedAsync(request.UserId, request.PageNumber, request.PageSize);
        return posts.MapToDto();
    }
}

// Commands (Write operations)
public class CreatePostCommand : IRequest<PostDto>
{
    public int UserId { get; set; }
    public string Content { get; set; }
}

public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, PostDto>
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<PostDto> Handle(CreatePostCommand request, CancellationToken ct)
    {
        var post = new Post { UserId = request.UserId, Content = request.Content };
        await _unitOfWork.Posts.AddAsync(post);
        await _unitOfWork.SaveChangesAsync();
        return post.MapToDto();
    }
}

// Usage in controller
public class PostsController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost]
    public async Task<IActionResult> Create(CreatePostCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
}
```

### 2.4 Dependency Injection & IoC Container

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();

// Register Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();

// Register MediatR for CQRS
builder.Services.AddMediatR(typeof(Program));

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Register Validators
builder.Services.AddFluentValidation(config =>
    config.RegisterValidatorsFromAssembly(typeof(Program).Assembly));

// Register Logging
builder.Services.AddSerilog();

var app = builder.Build();
```

---

## 3. Data Access Layer (EF Core Best Practices)

### 3.1 DbContext Configuration

```csharp
public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<UserFollow> UserFollows { get; set; }
    public DbSet<Message> Messages { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Bio).HasMaxLength(500);
            
            // Relationships
            entity.HasMany(e => e.Posts)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.Followers)
                .WithOne(uf => uf.Following)
                .HasForeignKey(uf => uf.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // UserFollow self-reference
        modelBuilder.Entity<UserFollow>(entity =>
        {
            entity.HasKey(e => new { e.FollowerId, e.FollowingId });
            entity.HasIndex(e => e.CreatedAt).IsDescending();
            
            entity.HasOne(uf => uf.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(uf => uf.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Post configuration with shadow properties
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.CreatedAt }).IsDescending(false, true);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => "UpdatedAt").HasDefaultValueSql("GETUTCDATE()");
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Set audit fields
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is AuditableEntity && e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            ((AuditableEntity)entry.Entity).UpdatedAt = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
```

### 3.2 Generic Repository

```csharp
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.AsNoTracking().ToListAsync();

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.AsNoTracking().Where(predicate).ToListAsync();

    public async Task AddAsync(T entity)
        => await _dbSet.AddAsync(entity);

    public void Update(T entity)
        => _dbSet.Update(entity);

    public void Delete(T entity)
        => _dbSet.Remove(entity);
}
```

---

## 4. Service Layer Architecture

### 4.1 Layered Service Design

```csharp
// Domain Service (Business Logic)
public interface IPostService
{
    Task<PostDto> CreatePostAsync(CreatePostRequest request);
    Task<PostDto> GetPostAsync(int id);
    Task<PagedResult<PostDto>> GetFeedAsync(int userId, int page, int size);
    Task LikePostAsync(int postId, int userId);
    Task UnlikePostAsync(int postId, int userId);
}

public class PostService : IPostService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;
    private readonly ILogger<PostService> _logger;

    public async Task<PostDto> CreatePostAsync(CreatePostRequest request)
    {
        _logger.LogInformation("Creating post for user {UserId}", request.UserId);
        
        var post = new Post 
        { 
            UserId = request.UserId, 
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Posts.AddAsync(post);
        await _unitOfWork.SaveChangesAsync();

        // Invalidate cache
        await _cache.RemoveAsync($"feed:{request.UserId}");

        _logger.LogInformation("Post created with Id {PostId}", post.Id);
        return _mapper.Map<PostDto>(post);
    }

    public async Task<PagedResult<PostDto>> GetFeedAsync(int userId, int page, int size)
    {
        var cacheKey = $"feed:{userId}:{page}";
        
        // Try cache first
        var cachedFeed = await _cache.GetAsync<PagedResult<PostDto>>(cacheKey);
        if (cachedFeed != null)
            return cachedFeed;

        // Get user's following list
        var following = await _unitOfWork.Users
            .GetFollowingAsync(userId);

        var followingIds = following.Select(u => u.Id).ToList();
        followingIds.Add(userId); // Include own posts

        // Get posts from following
        var posts = await _unitOfWork.Posts.GetByUsersAsync(
            followingIds, 
            page, 
            size);

        var result = new PagedResult<PostDto>
        {
            Items = _mapper.Map<List<PostDto>>(posts.Items),
            TotalCount = posts.TotalCount
        };

        // Cache for 5 minutes
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }
}
```

---

## 5. API Design & Versioning

### 5.1 RESTful API Standards

```csharp
// Consistent API structure
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class PostsController : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostDto>> GetPost(int id)
    {
        var post = await _postService.GetPostAsync(id);
        if (post == null)
            return NotFound();
        return Ok(post);
    }

    [HttpPost]
    [Authorize] // Require authentication
    [ProducesResponseType(typeof(PostDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PostDto>> CreatePost(CreatePostRequest request)
    {
        var result = await _postService.CreatePostAsync(request);
        return CreatedAtAction(nameof(GetPost), new { id = result.Id }, result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PostDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PostDto>>> GetFeed(
        [FromQuery] int page = 1, 
        [FromQuery] int size = 20)
    {
        var userId = User.GetUserId(); // From claims
        var result = await _postService.GetFeedAsync(userId, page, size);
        return Ok(result);
    }
}
```

### 5.2 API Versioning Strategy

```csharp
// In Program.cs
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("version"),
        new HeaderApiVersionReader("X-API-Version"));
});

// Usage
// GET /api/v1/posts (v1.0)
// GET /api/v2/posts (v2.0 - new features)
```

---

## 6. Error Handling & Validation

### 6.1 Custom Exception Handling

```csharp
public class ApiException : Exception
{
    public int StatusCode { get; set; }
    public string ErrorCode { get; set; }

    public ApiException(int statusCode, string message, string errorCode = null) 
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode ?? "INTERNAL_ERROR";
    }
}

public class NotFoundException : ApiException
{
    public NotFoundException(string resource, int id) 
        : base(404, $"{resource} with id {id} not found", "NOT_FOUND") { }
}

public class ValidationException : ApiException
{
    public Dictionary<string, string[]> Errors { get; set; }

    public ValidationException(Dictionary<string, string[]> errors) 
        : base(400, "One or more validation errors occurred.", "VALIDATION_ERROR")
    {
        Errors = errors;
    }
}
```

### 6.2 Global Exception Middleware

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, 
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new { message = exception.Message, errors = new object() };

        context.Response.StatusCode = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            ValidationException ve => 
            {
                response = new { message = exception.Message, errors = ve.Errors };
                return StatusCodes.Status400BadRequest;
            },
            _ => StatusCodes.Status500InternalServerError
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}

// Register in Program.cs
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

### 6.3 Fluent Validation

```csharp
public class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CreatePostRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .Length(1, 5000).WithMessage("Content must be between 1 and 5000 characters");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Invalid user ID");
    }
}
```

---

## 7. Caching Strategy

### 7.1 Redis Caching

```csharp
public interface ICacheService
{
    Task<T> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
}

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;

    public async Task<T> GetAsync<T>(string key)
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(key);
        return value.IsNull ? default : JsonSerializer.Deserialize<T>(value.ToString());
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var db = _redis.GetDatabase();
        var json = JsonSerializer.Serialize(value);
        await db.StringSetAsync(key, json, expiration);
    }
}

// Cache invalidation patterns
public class PostService
{
    public async Task UpdatePostAsync(UpdatePostRequest request)
    {
        var post = await _unitOfWork.Posts.GetByIdAsync(request.Id);
        post.Content = request.Content;
        
        await _unitOfWork.SaveChangesAsync();
        
        // Invalidate related caches
        await _cache.RemoveAsync($"post:{request.Id}");
        await _cache.RemoveByPatternAsync($"feed:*");
    }
}
```

---

## 8. Real-Time Communication with SignalR

```csharp
public class NotificationHub : Hub
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly INotificationService _notificationService;

    public async Task SendNotification(int userId, Notification notification)
    {
        await _hubContext.Clients
            .User(userId.ToString())
            .SendAsync("ReceiveNotification", notification);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.GetUserId();
        if (userId.HasValue)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }
        await base.OnConnectedAsync();
    }
}

// Frontend (React)
useEffect(() => {
    const connection = new HubConnectionBuilder()
        .withUrl("https://api.example.com/notificationHub")
        .withAutomaticReconnect()
        .build();

    connection.on("ReceiveNotification", (notification) => {
        dispatch(addNotification(notification));
    });

    connection.start();
}, []);
```

---

**Version:** 1.0.0  
**Created:** January 2, 2026  
**Status:** Enterprise Architecture Standard
