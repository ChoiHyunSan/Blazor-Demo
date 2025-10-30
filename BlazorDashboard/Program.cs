using BlazorDashboard.Components;
using BlazorDashboard.Services;
using Dapper;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Npgsql DataSource ���
var cs = builder.Configuration.GetConnectionString("Postgres");

// Npgsql DataSource ���� �� DI �����̳ʿ� ���
var dataSource = new NpgsqlDataSourceBuilder(cs).Build();
builder.Services.AddSingleton<NpgsqlDataSource>(dataSource);

// NoteService ���
builder.Services.AddSingleton<NoteService>();
builder.Services.AddSingleton<AdvancedNotesService>();
builder.Services.AddSingleton<NewService>();
builder.Services.AddSingleton<UserMailService>();

DefaultTypeMap.MatchNamesWithUnderscores = true;

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
