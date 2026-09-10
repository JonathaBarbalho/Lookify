using Lookify;
using LookifyConsoleTester;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(builder => {
    builder.AddConsole();
});
builder.Services.AddHttpClient();
builder.Services.Configure<LookifyOptions>(builder.Configuration.GetSection("Lookify"));
builder.Services.Configure<LookifyOptions>(options => {
});

builder.Services.AddTransient<LookifyService>();
builder.Services.AddTransient<TestScenarios>();
builder.Services.AddTransient<Tester>();

using var host = builder.Build();

var tester = host.Services.GetRequiredService<Tester>();

await tester.RunAsync();