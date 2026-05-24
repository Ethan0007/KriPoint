using KriPoint;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKriPoint(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(o =>
    o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseKriPoint();
app.UseAuthorization();
app.MapControllers();

app.Run();
