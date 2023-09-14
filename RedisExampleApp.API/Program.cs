using Microsoft.EntityFrameworkCore;
using RedisExampleApp.API.Models;
using RedisExampleApp.API.Repositories;
using RedisExampleApp.API.Service;
using RedisExampleApp.Cache;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IProductRepository>(sp =>
{
    var appDbContext = sp.GetRequiredService<AppDbContext>();

    var productRepository = new ProductRepository(appDbContext);

    var redisService = sp.GetRequiredService<RedisService>();
    return new ProductRepositoryWithCacheDecorator(productRepository, redisService);
});

builder.Services.AddScoped<IProductService, ProductService>();


builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase("myDatabase");
});
builder.Services.AddSingleton<RedisService>(sp =>  //bunu yapýnca artýk herhangi bir class ýn ctor unda kullanabiliriz.
{
    return new RedisService(builder.Configuration["CacheOptions:Url"]);
});

builder.Services.AddSingleton<IDatabase>(sp =>
{
    var redisService = sp.GetRequiredService<RedisService>();
    return redisService.GetDb(0);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope()) //Inememory e özel created iþlemi.. DAtabase her seferinde sýfýrdan oluþturur
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
