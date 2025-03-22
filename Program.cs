using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ✅ הגדרת CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.WithOrigins("https://job-yael.onrender.com")
       
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});
// ✅ הוספת שירותים (Controllers + Swagger)
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
});

var app = builder.Build();

// ✅ Middleware - Swagger
app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        c.RoutePrefix = "swagger"; // אפשר לשנות את הנתיב אם צריך
    });
// ✅ Middleware - קבצים סטטיים + CORS


app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true, // מתיר שליחת כל סוגי הקבצים
    DefaultContentType = "application/octet-stream" // ברירת מחדל
    ,
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "https://job-yael.onrender.com");
    }
});
app.UseCors("AllowAll");
// ✅ Middleware - Routing + Authorization
app.UseRouting();
app.UseAuthorization();

// ✅ מיפוי נתיבים
app.MapControllers();

//app.Run();
app.Run("http://0.0.0.0:8080"); // Ensure the app listens on 8080
