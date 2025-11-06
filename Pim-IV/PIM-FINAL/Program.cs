using Supabase;
using Microsoft.EntityFrameworkCore;
using PIM_FINAL.Data;

var builder = WebApplication.CreateBuilder(args);

// Initialize and register Supabase client (reads SUPABASE_URL and SUPABASE_KEY from env)
builder.Services.AddSingleton<Supabase.Client>(sp =>
{
    var url = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? builder.Configuration["SUPABASE_URL"];
    var key = Environment.GetEnvironmentVariable("SUPABASE_KEY") ?? builder.Configuration["SUPABASE_KEY"];
    var options = new Supabase.SupabaseOptions
    {
        AutoConnectRealtime = true
    };

    var client = new Supabase.Client(url, key, options);
    // InitializeAsync is asynchronous; block here to ensure the client is ready.
    client.InitializeAsync().GetAwaiter().GetResult();
    return client;
});

// Register EF Core PIMContext using Npgsql connection string
builder.Services.AddDbContext<PIMContext>(options =>
    options.UseNpgsql(builder.Configuration["SUPABASE_DB_CONNECTION"]));

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Departamento}/{action=Index}/{id?}");

app.Run();