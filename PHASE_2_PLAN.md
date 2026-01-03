# 📋 Phase 2: Social Foundation Implementation Plan

**Timeline:** 8 weeks (Weeks 5-12)  
**Goal:** Transform MiniShop into social platform with user management, authentication, posts, and feed.

---

## Week 1-2: Docker & Testing Infrastructure

### Tasks
- [ ] Create individual Dockerfiles for each service (CatalogService, OrdersService, ApiGateway)
- [ ] Create docker-compose.yml with SQL Server, Redis, RabbitMQ, Elasticsearch
- [ ] Test local Docker setup (`docker-compose up`)
- [ ] Setup xUnit test projects for each service
- [ ] Add Moq and TestContainers for integration tests
- [ ] Create base test fixtures (AppDbContext in memory)
- [ ] Setup code coverage reporting with OpenCover
- [ ] Document Docker local development workflow

### Deliverables
✅ Dockerfile + docker-compose.yml  
⬜ Test projects created  
⬜ CI/CD pipeline template ready

---

## Week 3-4: Authentication & Authorization

### Backend Tasks

**1. Create Auth Service (New Microservice)**
```csharp
// Features
- Register (POST /api/auth/register)
- Login (POST /api/auth/login)
- Refresh Token (POST /api/auth/refresh)
- Logout (POST /api/auth/logout)

// Tech
- JWT tokens (access + refresh)
- bcrypt password hashing
- Role-based access control (Admin, Moderator, User)
```

**2. Update ApiGateway**
```csharp
// Add JWT validation middleware
public class JwtValidationMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (token != null)
        {
            try
            {
                var principal = _tokenService.ValidateToken(token);
                context.User = principal;
            }
            catch { /* Token invalid */ }
        }
        await _next(context);
    }
}

// Add [Authorize] attribute to protected endpoints
```

**3. Protect Endpoints**
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class PostsController : ControllerBase
{
    [Authorize] // Requires valid JWT
    [HttpPost]
    public async Task<IActionResult> CreatePost(CreatePostRequest request)
    {
        var userId = User.GetUserId(); // From JWT claims
        var result = await _postService.CreatePostAsync(userId, request);
        return CreatedAtAction(nameof(GetPost), new { id = result.Id }, result);
    }
}
```

### Frontend Tasks

**1. Create Login/Register Pages**
```tsx
// Components
- LoginForm.tsx
- RegisterForm.tsx
- ForgotPasswordForm.tsx

// Features
- Form validation
- Error handling
- Loading states
- Remember me option
```

**2. Redux Auth Slice**
```tsx
export const authSlice = createSlice({
  name: 'auth',
  initialState: {
    user: null,
    token: null,
    refreshToken: null,
    isLoading: false,
    error: null
  },
  reducers: {
    setToken: (state, action) => {
      state.token = action.payload;
      localStorage.setItem('token', action.payload);
    },
    logout: (state) => {
      state.user = null;
      state.token = null;
      localStorage.removeItem('token');
    }
  },
  extraReducers: (builder) => {
    builder
      .addCase(login.fulfilled, (state, action) => {
        state.token = action.payload.token;
        state.user = action.payload.user;
      })
  }
});
```

**3. Protected Routes**
```tsx
const ProtectedRoute = ({ children }) => {
  const { token } = useAppSelector(state => state.auth);
  return token ? children : <Navigate to="/login" />;
};

<Routes>
  <Route path="/login" element={<LoginPage />} />
  <Route path="/register" element={<RegisterPage />} />
  <Route 
    path="/feed" 
    element={<ProtectedRoute><FeedPage /></ProtectedRoute>} 
  />
</Routes>
```

### Testing Tasks
- [ ] Unit tests for JWT token generation
- [ ] Integration tests for login/register endpoints
- [ ] Unit tests for Redux auth slice
- [ ] E2E test for login → authenticated request → logout flow

### Deliverables
✅ AuthService with JWT  
⬜ Protected API endpoints  
⬜ Login/Register UI  
⬜ Authentication tests (80%+ coverage)

---

## Week 5-6: User Service & Profiles

### Backend Tasks

**1. Create User Service**
```csharp
// Entities
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Bio { get; set; }
    public string ProfileImageUrl { get; set; }
    public List<UserFollow> Followers { get; set; }
    public List<UserFollow> Following { get; set; }
    public List<Post> Posts { get; set; }
}

public class UserFollow
{
    public int Id { get; set; }
    public int FollowerId { get; set; }
    public int FollowingId { get; set; }
    public User Follower { get; set; }
    public User Following { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**2. User Service Implementation**
```csharp
public interface IUserService
{
    Task<UserDto> GetProfileAsync(int userId);
    Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    Task<bool> FollowUserAsync(int followerId, int followingId);
    Task<bool> UnfollowUserAsync(int followerId, int followingId);
    Task<List<UserDto>> GetFollowersAsync(int userId);
    Task<List<UserDto>> GetFollowingAsync(int userId);
    Task<PagedResult<UserDto>> SearchUsersAsync(string query, int page, int size);
}

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;

    public async Task<UserDto> GetProfileAsync(int userId)
    {
        var cacheKey = $"user:{userId}";
        var cached = await _cache.GetAsync<UserDto>(cacheKey);
        if (cached != null) return cached;

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) throw new NotFoundException("User", userId);

        var dto = _mapper.Map<UserDto>(user);
        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromHours(1));
        return dto;
    }

    public async Task<bool> FollowUserAsync(int followerId, int followingId)
    {
        if (followerId == followingId)
            throw new ValidationException("Cannot follow yourself");

        var follow = new UserFollow 
        { 
            FollowerId = followerId, 
            FollowingId = followingId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.UserFollows.AddAsync(follow);
        await _unitOfWork.SaveChangesAsync();

        // Invalidate cache
        await _cache.RemoveAsync($"followers:{followingId}");
        await _cache.RemoveAsync($"following:{followerId}");

        return true;
    }
}
```

**3. User Endpoints**
```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetProfile(int id)
        => Ok(await _userService.GetProfileAsync(id));

    [HttpPut("profile")]
    public async Task<ActionResult<UserDto>> UpdateProfile(UpdateProfileRequest request)
    {
        var userId = User.GetUserId();
        var result = await _userService.UpdateProfileAsync(userId, request);
        return Ok(result);
    }

    [HttpPost("{id}/follow")]
    public async Task<IActionResult> Follow(int id)
    {
        var userId = User.GetUserId();
        await _userService.FollowUserAsync(userId, id);
        return NoContent();
    }

    [HttpDelete("{id}/follow")]
    public async Task<IActionResult> Unfollow(int id)
    {
        var userId = User.GetUserId();
        await _userService.UnfollowUserAsync(userId, id);
        return NoContent();
    }

    [HttpGet("{id}/followers")]
    public async Task<ActionResult<List<UserDto>>> GetFollowers(int id)
        => Ok(await _userService.GetFollowersAsync(id));

    [HttpGet("{id}/following")]
    public async Task<ActionResult<List<UserDto>>> GetFollowing(int id)
        => Ok(await _userService.GetFollowingAsync(id));

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<UserDto>>> SearchUsers(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
        => Ok(await _userService.SearchUsersAsync(query, page, size));
}
```

### Frontend Tasks

**1. Profile Component**
```tsx
// Components
- UserProfile.tsx (Display profile)
- EditProfile.tsx (Edit form)
- FollowButton.tsx (Follow/Unfollow)
- UserCard.tsx (Reusable user card)
- UserList.tsx (Followers/Following lists)

// Redux slice for users
export const userSlice = createSlice({
  name: 'users',
  initialState: { users: {}, loading: false },
  extraReducers: (builder) => {
    builder.addCase(fetchUser.fulfilled, (state, action) => {
      state.users[action.payload.id] = action.payload;
    });
  }
});
```

**2. User Discovery Page**
```tsx
// Features
- Search users by name/username
- Browse user profiles
- Follow/unfollow
- View followers/following lists

// UI
<SearchBar placeholder="Find users..." />
<UserGrid users={users} onFollow={handleFollow} />
<UserModal user={selectedUser} onClose={closeModal} />
```

### Testing Tasks
- [ ] Unit tests for UserService methods
- [ ] Integration tests for follow/unfollow with database
- [ ] API endpoint tests for all user routes
- [ ] Frontend component tests for ProfileCard, FollowButton
- [ ] E2E test for follow user workflow

### Deliverables
✅ User Service with profiles  
✅ Follow/Unfollow functionality  
✅ User discovery UI  
✅ User tests (80%+ coverage)

---

## Week 7-8: Posts & Feed Service

### Backend Tasks

**1. Create Post Service**
```csharp
public class Post
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; }
    public List<string> ImageUrls { get; set; } // JSON array
    public string Visibility { get; set; } = "Public"; // Public, Friends, Private
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public User User { get; set; }
    public List<Like> Likes { get; set; }
    public List<Comment> Comments { get; set; }
}

public interface IPostService
{
    Task<PostDto> CreatePostAsync(int userId, CreatePostRequest request);
    Task<PostDto> GetPostAsync(int postId);
    Task<PagedResult<PostDto>> GetFeedAsync(int userId, int page, int size);
    Task<PagedResult<PostDto>> GetUserPostsAsync(int userId, int page, int size);
    Task<bool> LikePostAsync(int postId, int userId);
    Task<bool> UnlikePostAsync(int postId, int userId);
    Task<CommentDto> AddCommentAsync(int postId, int userId, string content);
    Task DeletePostAsync(int postId, int userId);
}

public class PostService : IPostService
{
    public async Task<PagedResult<PostDto>> GetFeedAsync(int userId, int page, int size)
    {
        // Get user's following list
        var following = await _unitOfWork.Users.GetFollowingAsync(userId);
        var followingIds = following.Select(u => u.Id).ToList();
        followingIds.Add(userId); // Include own posts

        // Get posts with pagination
        var (posts, total) = await _unitOfWork.Posts
            .GetByUsersAsync(followingIds, page, size);

        return new PagedResult<PostDto>
        {
            Items = _mapper.Map<List<PostDto>>(posts),
            TotalCount = total,
            Page = page,
            Size = size
        };
    }

    public async Task<bool> LikePostAsync(int postId, int userId)
    {
        var existingLike = await _unitOfWork.Likes
            .FindAsync(l => l.PostId == postId && l.UserId == userId);

        if (existingLike.Any())
            throw new ValidationException("Post already liked");

        var like = new Like { PostId = postId, UserId = userId };
        await _unitOfWork.Likes.AddAsync(like);
        await _unitOfWork.SaveChangesAsync();

        // Update cache and send notification
        await _cache.RemoveAsync($"post:{postId}:likes");
        return true;
    }
}
```

**2. Post Endpoints**
```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PostsController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PostDto>> CreatePost(CreatePostRequest request)
    {
        var userId = User.GetUserId();
        var result = await _postService.CreatePostAsync(userId, request);
        return CreatedAtAction(nameof(GetPost), new { id = result.Id }, result);
    }

    [HttpGet("feed")]
    public async Task<ActionResult<PagedResult<PostDto>>> GetFeed(
        [FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var userId = User.GetUserId();
        var result = await _postService.GetFeedAsync(userId, page, size);
        return Ok(result);
    }

    [HttpPost("{id}/like")]
    public async Task<IActionResult> LikePost(int id)
    {
        var userId = User.GetUserId();
        await _postService.LikePostAsync(id, userId);
        return NoContent();
    }

    [HttpPost("{id}/comments")]
    public async Task<ActionResult<CommentDto>> AddComment(int id, AddCommentRequest request)
    {
        var userId = User.GetUserId();
        var result = await _postService.AddCommentAsync(id, userId, request.Content);
        return CreatedAtAction(nameof(GetPost), new { postId = id }, result);
    }
}
```

### Frontend Tasks

**1. Feed Components**
```tsx
// Components
- FeedPage.tsx (Main feed)
- PostCard.tsx (Individual post)
- CreatePostModal.tsx (New post form)
- LikeButton.tsx (Like/Unlike)
- CommentSection.tsx (Comments list)
- CommentForm.tsx (Add comment)

// Redux slice for posts
export const postSlice = createSlice({
  name: 'posts',
  initialState: {
    feed: [],
    loading: false,
    hasMore: true
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchFeed.fulfilled, (state, action) => {
        state.feed = action.payload.items;
        state.hasMore = action.payload.hasMore;
      })
      .addCase(createPost.fulfilled, (state, action) => {
        state.feed.unshift(action.payload);
      })
      .addCase(likePost.fulfilled, (state, action) => {
        const post = state.feed.find(p => p.id === action.payload.postId);
        if (post) post.likeCount++;
      });
  }
});
```

**2. Feed UI**
```tsx
export const FeedPage = () => {
  const { feed, loading } = useAppSelector(state => state.posts);
  const dispatch = useAppDispatch();

  useEffect(() => {
    dispatch(fetchFeed({ page: 1, size: 20 }));
  }, []);

  return (
    <div className="feed">
      <CreatePostModal />
      {feed.map(post => (
        <PostCard 
          key={post.id} 
          post={post}
          onLike={(postId) => dispatch(likePost(postId))}
          onComment={(postId, content) => dispatch(addComment(postId, content))}
        />
      ))}
      {loading && <Spinner />}
      <InfiniteScroll 
        dataLength={feed.length}
        next={() => dispatch(fetchFeed({ page: page + 1 }))}
        hasMore={hasMore}
      />
    </div>
  );
};
```

### Testing Tasks
- [ ] Unit tests for PostService (create, like, comment)
- [ ] Integration tests for feed algorithm
- [ ] Load testing for feed endpoint (10K users)
- [ ] Frontend component tests for PostCard, LikeButton
- [ ] E2E test for post creation → like → comment flow

### Deliverables
✅ Post CRUD API  
✅ Feed algorithm  
✅ Like/Comment functionality  
✅ Feed UI with infinite scroll  
✅ Post tests (80%+ coverage)

---

## CI/CD Pipeline Setup (Ongoing)

### GitHub Actions Workflow
```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [develop]

jobs:
  build-test:
    runs-on: ubuntu-latest
    
    services:
      mssql:
        image: mcr.microsoft.com/mssql/server:2019-latest
        env:
          SA_PASSWORD: TestPass@123
          ACCEPT_EULA: Y

    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore --configuration Release
      
      - name: Run tests
        run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
      
      - name: Upload coverage to Codecov
        uses: codecov/codecov-action@v3
        with:
          files: ./coverage.xml
      
      - name: Build Docker image
        run: docker build -t minishop:${{ github.sha }} .
      
      - name: Push to registry
        run: |
          docker login -u ${{ secrets.DOCKER_USER }} -p ${{ secrets.DOCKER_PAT }}
          docker push minishop:${{ github.sha }}
      
      - name: Deploy to staging
        if: github.ref == 'refs/heads/develop'
        run: |
          # Deploy to Azure App Service or similar
          echo "Deploying to staging..."
```

---

## Success Metrics

By end of Phase 2:

- ✅ JWT authentication working
- ✅ User profiles with follow functionality
- ✅ Feed algorithm generating personalized content
- ✅ Like/comment on posts working
- ✅ All APIs tested (80%+ coverage)
- ✅ Docker containers running locally
- ✅ CI/CD pipeline automated
- ✅ Response times < 200ms P95
- ✅ Zero critical security issues
- ✅ Complete documentation updated

---

## Effort Breakdown

| Component | Backend | Frontend | Testing | DevOps | Total |
|-----------|---------|----------|---------|--------|-------|
| Docker & CI/CD | 2 | - | 1 | 3 | **6** |
| Auth Service | 3 | 2 | 2 | - | **7** |
| User Service | 3 | 3 | 2 | - | **8** |
| Post Service | 4 | 4 | 3 | - | **11** |
| **TOTAL** | **12** | **9** | **8** | **3** | **32 days** |

**Timeline:** 8 weeks with team of 4 (parallel work)

---

**Version:** 1.0.0  
**Created:** January 2, 2026  
**Status:** Ready for Implementation
