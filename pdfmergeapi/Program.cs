    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using PdfMergeApi.Services;


using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configuración para Railway/Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var url = $"http://0.0.0.0:{port}";

builder.WebHost.UseUrls(url);

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PDF Merger API", Version = "v1" });
});

// Register services (SOLID: dependency inversion, single responsibility)
builder.Services.AddScoped<IFileStorageService, TempFileStorageService>();
builder.Services.AddScoped<IDocumentService, PdfDocumentService>();

// Add CORS for production
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Production settings
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Security headers for production
app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Add security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});

app.MapControllers();

// Health check endpoint
app.MapGet("/", () => "PDF Merger API is running!");
app.MapGet("/health", () => new { status = "Healthy", timestamp = DateTime.UtcNow });

app.Run();