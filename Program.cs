using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TodoApi; // ודא שזה המרחב השמות הנכון
using Microsoft.OpenApi.Models;
using DotNetEnv;



// טען את משתני הסביבה מקובץ .env
Env.Load();

// יצירת תשתית להגדרת כל השירותים והתצורות ליצירת האפליקציה
var builder = WebApplication.CreateBuilder(args);

// הוספת שירותי DbContext עם מחרוזת החיבור
var connectionString = Environment.GetEnvironmentVariable("ToDoDB");
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// הוספת שירות CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// הוספת Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Todo API", Version = "v1" });
});

var app = builder.Build();

// הפעלת מדיניות CORS
app.UseCors("AllowAllOrigins");

// הפעלת Swagger
app.UseSwagger();
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API V1");
    c.RoutePrefix = string.Empty; // אם תרצה שה-Swagger UI יהיה בדף הראשי
});
//7.	git push -u origin master
//6.	git remote add origin https://github.com/YourUsername/RepositoryName.git
// Route לשליפת כל המשימות
app.MapGet("/tasks", async (ToDoDbContext db) =>
{
    return await db.Items.ToListAsync();
})
.WithName("GetAllTasks") // הוספת שם למסלול
.Produces<List<Item>>(StatusCodes.Status200OK); // הוספת תוצאה אפשרית
app.MapGet("/",()=>"AuthServer API is running");
// Route להוספת משימה
app.MapPost("/tasks", async (ToDoDbContext db, Item item) =>
{
    if (item == null || string.IsNullOrWhiteSpace(item.Name))
    {
        return Results.BadRequest("Task name is required.");
    }

    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/tasks/{item.Id}", item);
})
.WithName("AddTask")
.Produces<Item>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest); // הוספת אפשרות לתגובה רעה


app.MapPut("/updateTask/{id}", async (int id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item.IsComplete == false)
    {
        item.IsComplete = true;
    }
    else
    {
        item.IsComplete = false;
    }
    db.Items.Update(item);
    await db.SaveChangesAsync();
    return item;
});
//ssh-keygen -t rsa -b 4096 -C "your_email@example.com"
//git remote set-url origin https://github.com/rachel67835/repository.git



// Route למחיקת משימה
app.MapDelete("/tasks/{id}", async (ToDoDbContext db, int id) =>
{
    var item = await db.Items.FindAsync(id);

    if (item is null) return Results.NotFound();

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("DeleteTask")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

app.Run();
