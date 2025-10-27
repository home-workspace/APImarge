    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using PdfMergeApi.Services;

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers().AddNewtonsoftJson();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "PDF Merger API", Version = "v1" });
    });


// Register services (SOLID: dependency inversion, single responsibility)
builder.Services.AddScoped<IFileStorageService, TempFileStorageService>();
    builder.Services.AddScoped<IDocumentService, PdfDocumentService>();

    var app =  builder.Build();

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


app.UseHttpsRedirection();
    app.MapControllers();

    app.Run();
