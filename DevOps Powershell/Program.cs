using DevOps_Powershell.Interfaces.Services;
using DevOps_Powershell.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.





builder.Services.AddControllers();
builder.Services.AddHttpClient();

builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ISaveProjectExcel,SaveProjectExcelService>();



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
