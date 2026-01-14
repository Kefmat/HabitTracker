using HabitTracker.Components;
using HabitTracker.Data;
using HabitTracker.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Registrerer Razor Components + interaktiv server-rendering (Blazor Server-stil).
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// EF Core + SQLite.
// Connection string ligger i appsettings.json under ConnectionStrings:Default.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Service-laget vårt bør være Scoped fordi det bruker DbContext (som også er Scoped).
builder.Services.AddScoped<HabitService>();

var app = builder.Build();

// Standard pipeline-oppsett.
// I prod bruker vi error handler + HSTS.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// Antiforgery beskytter mot CSRF-angrep i server-rendered scenarier.
app.UseAntiforgery();

// Gir støtte for statiske filer (CSS/JS osv).
app.MapStaticAssets();

// Starter Blazor/Razor Components appen.
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
