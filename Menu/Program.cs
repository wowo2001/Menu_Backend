using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Menu.Data;
using Menu.Services;

var builder = WebApplication.CreateBuilder(args);

// Load AWS configurations from appsettings.json
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());

// Register AWS DynamoDB service
builder.Services.AddAWSService<IAmazonDynamoDB>();


// Add services to the container.
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IMenuData, MenuData>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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