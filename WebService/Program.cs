using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ServicesLibrary.Groups.Services;
using ServicesLibrary.Users;
using SharedLibrary.Contexts;

var builder = WebApplication.CreateBuilder(args);

#region Logging
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());
#endregion
builder.Services.AddControllers();

#region ConnectionStrings
builder.Services.AddDbContext<UserContext>(options =>
options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
?? "Data Source=users.db"));
#endregion

#region Library Services
builder.Services.AddScoped<AddUserService>();
builder.Services.AddScoped<GetUserService>();
builder.Services.AddScoped<GetGroupService>();
builder.Services.AddScoped<EditUserService>();
builder.Services.AddScoped<DeleteUserService>();
builder.Services.AddScoped<UserCountService>();
builder.Services.AddScoped<GroupPermissionService>();
#endregion

#region Web Service Registrations
builder.Services.AddCors(options =>
{
options.AddPolicy("AllowBlazorWasm",
policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
});
builder.Services.AddOpenApi(); 
#endregion

var app = builder.Build();

#region Database Intializer 
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UserContext>();
    context.Database.EnsureCreated();
} 
#endregion

#region MiddleWare
app.UseSerilogRequestLogging();
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;

        if (exception != null)
        {
            Log.Error(exception, "Unhandled exception");
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
    });
});
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseCors("AllowBlazorWasm");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers(); 
#endregion

app.Run();
