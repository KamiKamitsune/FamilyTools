using FamilyTools.Data.Context;
using FamilyTools.EasyCompta.Business;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddSqlServerDbContext<EasyComptaContext>("easycompta");
builder.AddRabbitMQClient(connectionName: "messaging");

builder.Services.AddEasyComptaBusiness();
builder.Services.AddHostedService<FamilyTools.CsvWorker.CsvImportConsumer>();

var host = builder.Build();
host.Run();
