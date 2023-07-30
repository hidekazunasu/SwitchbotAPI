var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Serve static files from wwwroot folder.
app.UseStaticFiles();
// Enable routing middleware to match incoming requests to an endpoint.
app.UseRouting();

app.UseHttpsRedirection();
app.UseEndpoints(x => x.MapControllers());
app.Run();