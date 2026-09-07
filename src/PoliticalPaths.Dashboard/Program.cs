using PoliticalPaths.Dashboard.Services;
using PoliticalPaths.Application;
using PoliticalPaths.Infrastructure;
using PoliticalPaths.Importers.Raw;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddApplication();
builder.Services.AddRawImporters();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<DashboardQueryService>();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();
