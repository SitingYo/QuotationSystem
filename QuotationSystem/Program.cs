using Microsoft.EntityFrameworkCore;
using QuotationSystem.Data;
using QuotationSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. 註冊資料庫 (使用 SQLite)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. 註冊 Service (DI 注入)
builder.Services.AddScoped<IQuotationService, QuotationService>();

// 3. 註冊 MVC 與 API 控制器
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();

// 4. Swagger 設定
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// 自動執行資料庫遷移
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        // 檢查本地是否有資料庫，若無則根據 Migrations 建立
        context.Database.Migrate(); 
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "資料庫初始化失敗。");
    }
}

// 啟用 Swagger 介面
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 5. 支援 wwwroot 裡的靜態檔案 (CSS, JS, 圖片)
app.UseStaticFiles();

// 6. 設定 MVC 預設路由
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=QuotationView}/{action=Index}/{id?}");

app.MapControllers(); // 保留原本的 API 路由支援

app.Run();
