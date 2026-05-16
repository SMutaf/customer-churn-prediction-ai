using CustomerAI.Core.Interfaces;
using CustomerAI.API.Hubs;
using CustomerAI.API.Realtime;
using CustomerAI.Data.Context;
using CustomerAI.Data.Repositories;
using CustomerAI.Services.Concrete;
using CustomerAI.Services.Interfaces;
using CustomerAI.Services.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddDbContext<CustomerAiDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICustomerBehaviorService, CustomerBehaviorService>();
builder.Services.AddScoped<IFeatureExtractionService, FeatureExtractionService>();
builder.Services.AddScoped<ICoreRiskEngine, CoreRiskEngine>();
builder.Services.AddScoped<IFinalRiskDecisionService, FinalRiskDecisionService>();
builder.Services.AddScoped<ISegmentAssignmentService, SegmentAssignmentService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IAnalyticsRealtimeNotifier, SignalRAnalyticsRealtimeNotifier>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddHttpClient<IPythonApiService, PythonApiService>(client =>
{
    client.BaseAddress = new Uri("http://127.0.0.1:5000");
});

builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters()
                .AddValidatorsFromAssemblyContaining<CreateCustomerValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.UseMiddleware<CustomerAI.API.Middleware.GlobalExceptionMiddleware>();
app.MapControllers();
app.MapHub<AnalyticsHub>("/hubs/analytics");
app.Run();
