using Microsoft.EntityFrameworkCore;
using SchoolApp.BL;
using SchoolApp.BL.Configuration;
using SchoolApp.DAL;
using SchoolApp.DAL.Data;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDataAccess(builder.Configuration);
builder.Services.AddBusinessLogic();
builder.Services.Configure<GradeScaleOptions>(builder.Configuration.GetSection(GradeScaleOptions.SectionName));
var app = builder.Build();
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<SchoolDbContext>();
//    await db.Database.EnsureCreatedAsync(); // Inicio rápido; para producción utilizar migraciones.
//}
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Students}/{action=Index}/{id?}");
app.Run();
