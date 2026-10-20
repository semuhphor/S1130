using S1130.Site;
using S1130.Site.Machines;
using S1130.Site.Sessions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection(SiteOptions.Section));
builder.Services.AddSingleton<MachineManager>();
builder.Services.AddHostedService<IdleMachineReaper>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseSessionCookie();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapMachineEndpoints();

app.Run();

public partial class Program { }

