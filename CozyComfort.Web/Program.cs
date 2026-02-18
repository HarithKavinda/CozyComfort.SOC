var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});



var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// In Development, allow running on plain HTTP without forcing https redirects.
// This avoids mixed-scheme issues with local session cookies and local API calls.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();


// ⭐ RUN → Home (Landing) page load first
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Host address clearly show karanna
Console.WriteLine("");
Console.WriteLine("========================================");
Console.WriteLine("  Frontend: http://localhost:5250");
Console.WriteLine("  Register page open wenne me URL eka");
Console.WriteLine("========================================");
Console.WriteLine("");

app.Run();
