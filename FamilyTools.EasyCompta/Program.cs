using System.Text.Json.Serialization;

using FamilyTools.Data.Context;
using FamilyTools.EasyCompta.Business;
using FamilyTools.EasyCompta.IBusiness;
using FamilyTools.EasyCompta.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddSqlServerDbContext<EasyComptaContext>("easycompta");

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddScoped<IUserBusiness, UserBusiness>();
builder.Services.AddScoped<ITemplateBusiness, TemplateBusiness>();
builder.Services.AddScoped<IAccountEnterBusiness, AccountEnterBusiness>();
builder.Services.AddScoped<IAccountTagBusiness, AccountTagBusiness>();
builder.Services.AddScoped<IAccountPageBusiness, AccountPageBusiness>();
builder.Services.AddScoped<IAccountLineBusiness, AccountLineBusiness>();
builder.Services.AddScoped<IImportCSVBusiness, ImportCSVBusiness>();

builder.Services.AddHostedService<CSVConvertService>();
builder.Services.AddSingleton<IBackgroundCSVConvert>(_ =>
{
    if (!int.TryParse(builder.Configuration["QueueCapacity"], out var queueCapacity))
    {
        queueCapacity = 100;
    }

    return new DefaultBackgroundTaskQueue(queueCapacity);
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Ignore les cycles au lieu de throw
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        // Optionnel: rendre le JSON plus lisible
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();



app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    //using (var scope = app.Services.CreateScope())
    //{
    //    var context = scope.ServiceProvider.GetRequiredService<EasyComptaContext>();
    //    context.Database.EnsureCreated();
    //    context.EnsureSeedData().GetAwaiter().GetResult();
    //}
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days.
    // You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Run();
