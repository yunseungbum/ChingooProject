using Chingoo.Data;
using Chingoo.Services.Accounts;
using Chingoo.Services.Comments;
using Chingoo.Services.FootballData;
using Chingoo.Services.Communities;
using Chingoo.Services.Home;
using Chingoo.Services.Notices;
using Chingoo.Services.Posts;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Chingoo.Services.Youtube;
using Chingoo.Hubs;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("DefaultConnection")
        )));

// MVC
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// Services
builder.Services.Configure<FootballDataOptions>(builder.Configuration.GetSection("FootballData"));
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IFootballDataService, FootballDataService>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<FootballDataOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
});
builder.Services.AddHttpClient<IYoutubeFeedService, YoutubeFeedService>(client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 ChingooProject/1.0");
    client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");
});
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<INoticeService, NoticeService>();
builder.Services.AddScoped<ICommunityService, CommunityService>();

// 인증
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });

// 권한
builder.Services.AddAuthorization();

var app = builder.Build();

// 에러 처리
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// ⭐ 핵심 (순서 중요)
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();

app.MapHub<ChatHub>("/chatHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

public partial class Program { }


