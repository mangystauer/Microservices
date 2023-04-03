using Microsoft.EntityFrameworkCore;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.SyncDataServices;
using PlatformService.SyncDataServices.Http;


var builder = WebApplication.CreateBuilder(args);
bool envProd = false;
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



if(builder.Environment.IsProduction())
{
    System.Console.WriteLine("---> Using Server Db");
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("PlatformsConn")));
    envProd = true;
}
else
{
    System.Console.WriteLine("---> Using in memory database");
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("InMem"));
}


builder.Services.AddScoped<IPlatformRepo, PlatformRepo>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<ICommandDataClient, HttpCommandDataClient>();
builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();
builder.Services.AddGrpc();

System.Console.WriteLine($"Command service endpoint {builder.Configuration.GetValue<string>("CommandService")}");
var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<GrpcPlatformService>();

app.MapGet("/protos/platforms.proto", async context => 
{
await context.Response.WriteAsync(File.ReadAllText("/Protos/platforms.proto"));
});

PrepDb.PrepPopulation(app, envProd);
app.Run();


