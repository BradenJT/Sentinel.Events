using Sentinel.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSentinelServices(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

var app = builder.Build();

// Seed database in development
if (app.Environment.IsDevelopment())
{
    await Sentinel.Api.SeedData.Initialize(app.Services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Tenant resolution middleware (must be before controllers)
app.UseMiddleware<Sentinel.Api.Middleware.TenantMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
