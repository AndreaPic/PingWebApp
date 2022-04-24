using DistributedLoopDetector;
using Microsoft.Extensions.Http;
using PingWebApp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor().AddLogging().AddHttpClient();

//Line below activate distributed loop detection
builder.Services.AddDistributedLoopDetector(); // <-----

builder.Services.AddControllersWithViews(options =>
{
});

builder.Services.AddControllers();
// Add services to the container.
builder.Services.AddRazorPages();

//uncomment code below to use distributed cache for loop detection
/*
builder.Services.AddDistributedMemoryCache(); // <-----
builder.Services.AddStackExchangeRedisCache(options => // <-----
{ 
    options.Configuration = builder.Configuration.GetConnectionString("DistributedLoopRedisConStr");
    options.InstanceName = "redis-dloopd-dev-weu-001";
});
*/

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//uncomment line below to use distributed cache for loop detection
//app.UseDistributedCacheForLoopDetector("andrea-dev-italy-ping"); // <-----

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.MapControllers();

app.MapRazorPages();

app.Run();
