///Author: Christian Basubi
/// Warning: This code contains 0% caffeine but 100% functionality 

using EcfrAnalyzerByCB; 


var builder = WebApplication.CreateBuilder(args);

// Register our custom EcfrService and HttpClient
builder.Services.AddHttpClient<EcfrAnalyzerByCB.EcfrService>();

// Register default controller support
builder.Services.AddControllers();

// enabling Razor Pages 
builder.Services.AddRazorPages();

var app = builder.Build();

// for any CSS or static files
app.UseStaticFiles();
// No need for Swagger or HTTPS for this take-home test
app.UseAuthorization();

// Map all controllers
app.MapControllers();

// Register Razor Pages
app.MapRazorPages(); 

app.MapGet("/", () => "👋 Welcome to Christian Basubi's eCFR API. Try /ui to view the metrics UI");


app.Run();

