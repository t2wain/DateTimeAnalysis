// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Hosting;
using DateTimeConsole;
using Microsoft.Extensions.Configuration;

var builder = Host.CreateApplicationBuilder(args);

var fileName = "appsettings.Development.json";
if (File.Exists(fileName))
    builder.Configuration.AddJsonFile(fileName);

var cfg = builder
    .Configuration.GetSection("LicenseUsage")
    .Get<UsageConfig>();

using var host = builder.Build();

var t = new AlgoTest(cfg!);
t.Run();
