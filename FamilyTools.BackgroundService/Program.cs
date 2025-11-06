//using Familytools.CSVConvert;

//using FamilyTools.CSVConvert;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

//builder.Services.AddSingleton<MonitorLoop>();
//builder.Services.AddHostedService<QueuedHostedService>();
//builder.Services.AddSingleton<IBackgroundTaskQueue>(_ =>
//{
//    if (!int.TryParse(builder.Configuration["QueueCapacity"], out var queueCapacity))
//    {
//        queueCapacity = 2;
//    }

//    return new DefaultBackgroundTaskQueue(queueCapacity);
//});

var host = builder.Build();

//MonitorLoop monitorLoop = host.Services.GetRequiredService<MonitorLoop>()!;
//monitorLoop.StartMonitorLoop();

host.Run();
