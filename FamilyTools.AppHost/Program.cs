using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var sql = builder.AddSqlServer("FamilyTools", port: 14329)
    .WithEndpoint(name: "sqlEndpoint", targetPort: 14330)
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("easycompta");

//var backgroundService = builder.AddProject<Projects.FamilyTools_BackgroundService>("BackgroundService");


var migration = builder.AddProject<FamilyTools_MigrationService>("migrations")
    .WithReference(sql)
    .WaitFor(sql);

var easyComptaAPI = builder
    .AddProject<FamilyTools_EasyCompta>("EasyComptaAPI")
    .WithReference(sql)
    .WithReference(migration)
    .WaitForCompletion(migration)
    //.WithReference(backgroundService)
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.AddNpmApp("webfrontend", "../FamilyTools.Web")
    .WithReference(easyComptaAPI)
    .WaitFor(easyComptaAPI)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();


builder.Build().Run();