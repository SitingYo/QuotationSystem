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