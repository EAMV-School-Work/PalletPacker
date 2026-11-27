using Microsoft.EntityFrameworkCore;
using PalletPacker.Infrastructure.Data;
using PalletPacker.Infrastructure.Imports;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add db context
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    options.UseSqlServer(connectionString);
});

// Add mock data loader and synchronizer services
builder.Services.AddSingleton<MockDataJsonLoader>();
builder.Services.AddScoped<MockDataSynchronizer>();

var app = builder.Build();

// Migrate DB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    const int maxAttempts = 10;
    var delay = TimeSpan.FromSeconds(3);

    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await db.Database.MigrateAsync();

            app.Logger.LogInformation(
                "Database migrations completed successfully.");

            break;
        }
        catch (Microsoft.Data.SqlClient.SqlException)
            when (attempt < maxAttempts)
        {
            app.Logger.LogWarning(
                "SQL Server is not ready. Retrying ({Attempt}/{MaxAttempts})...",
                attempt,
                maxAttempts);

            await Task.Delay(delay);
        }
    }
}

// Synchronize mock data
using (var scope = app.Services.CreateScope())
{
    var synchronizer = scope.ServiceProvider
        .GetRequiredService<MockDataSynchronizer>();

    var wasUpdated = await synchronizer.SynchronizeAsync();

    app.Logger.LogInformation(
        wasUpdated
            ? "Order and element data were synchronized from JSON files."
            : "Order and element data already match JSON files.");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();