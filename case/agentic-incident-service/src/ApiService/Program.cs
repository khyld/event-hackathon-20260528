var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p => p
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin());
});

var app = builder.Build();

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/incidents", () => new[] { new { id = "INC-1", title = "Sample" } });

app.Run();

public partial class Program { }
