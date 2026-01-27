using Microsoft.EntityFrameworkCore;
using ServicesLibrary.Groups.Services;
using ServicesLibrary.Users;
using SharedLibrary.Contexts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure SQLite
builder.Services.AddDbContext<UserContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=users.db"));

// Register services
builder.Services.AddScoped<AddUserService>();
builder.Services.AddScoped<EditUserService>();
builder.Services.AddScoped<DeleteUserService>();
builder.Services.AddScoped<UserCountService>();
builder.Services.AddScoped<GroupPermissionService>();

// Configure CORS for Blazor WebAssembly
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorWasm",
        policy => policy
            .WithOrigins(
                "https://localhost:7166",  // WebInterface HTTPS
                "http://localhost:5132")   // WebInterface HTTP
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UserContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowBlazorWasm");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
