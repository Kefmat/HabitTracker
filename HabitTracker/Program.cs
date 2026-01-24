using HabitTracker.Components;
using HabitTracker.Data;
using HabitTracker.Services;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Service-laget bør være Scoped fordi det bruker DbContext (som også er Scoped).
builder.Services.AddScoped<HabitService>();

// Registrer AI-klient: ekte AI hvis OPENAI_API_KEY finnes, ellers fallback.
// OpenAI .NET SDK anbefaler at klientene er thread-safe og kan registreres som singleton.
builder.Services.AddSingleton<ICoachClient>(sp =>
{
    var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    if (string.IsNullOrWhiteSpace(apiKey))
        return new RuleBasedCoachClient();

    // Velg modell her (enkelt å endre senere).
    // Du kan starte med "gpt-4o" som i SDK-eksempel.
    var model = "gpt-4o";
    var chatClient = new ChatClient(model: model, apiKey: apiKey);

    return new OpenAiCoachClient(chatClient);
});

builder.Services.AddScoped<CoachService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
