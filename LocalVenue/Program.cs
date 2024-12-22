using LocalVenue.Web;
using LocalVenue.Core;
using LocalVenue.Core.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.WebComponents;
using LocalVenue.Core.Interfaces;
using LocalVenue.Core.Services;
using LocalVenue.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });
builder.Services.AddAutoMapper(typeof(Program)); //AutoMapper configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "LocalVenue API", Version = "v1" });
    c.EnableAnnotations();
}); // TODO: Investigate if az cli can be used to deploy the app to Azure from pipeline
    //      TODO: Try to add dotnet and postman testing in pipeline
    // TODO: Implement the customer entity and some basic authentication for the API


// Database context setup starts
var connectionStrings = new List<string>();
if (builder.Environment.IsDevelopment()) //pull secrets from local storage or Azure configuration
{
    var venueContextLocal = builder.Configuration.GetConnectionString("VenueContextLocal") ?? throw new ArgumentNullException("VenueContextLocal");
    connectionStrings.Add(venueContextLocal);
}
var venueContext = builder.Configuration.GetConnectionString("VenueContext") ?? throw new ArgumentNullException("VenueContext");
connectionStrings.Add(venueContext);

string? connectionString = null;
bool connected = false;

foreach (var connStr in connectionStrings)
{
    try
    {
        var optionsBuilder = new DbContextOptionsBuilder<VenueContext>();
        optionsBuilder.UseMySql(connStr, ServerVersion.AutoDetect(connStr));

        using (var context = new VenueContext(optionsBuilder.Options))
        {
            if (context.Database.CanConnect())
            {
                connectionString = connStr;
                connected = true;
                break;
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to connect using connection string: {connStr}. Error: {ex.Message}");
    }
}

if (!connected)
{
    throw new Exception("Failed to connect to any database.");
}

builder.Services.AddDbContextFactory<VenueContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.Parse("8.0-mysql"));
});

builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IShowService, ShowService>();
builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
// Database context setup ends

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/404";
        options.Cookie.Name = "CookieAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.MaxAge = TimeSpan.FromDays(1);
        options.Cookie.IsEssential = true;
    });

builder.Services.AddIdentity<Customer, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedEmail = false;

        options.Password.RequiredLength = 3;
        options.Password.RequireUppercase = false;
        options.Password.RequireDigit = false;
        options.Password.RequiredUniqueChars = 0;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<VenueContext>()
    .AddApiEndpoints();

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

//test push in GitHub with organization
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<VenueContext>();
    var retryCount = 5;
    var delay = TimeSpan.FromSeconds(10);

    for (int i = 0; i < retryCount; i++)
    {
        try
        {
            dbContext.Database.Migrate(); //Auto perform migrations
            DbSeeder.UpsertSeed(dbContext); //Auto seed database
            break;
        }
        catch (Microsoft.Data.SqlClient.SqlException)
        {
            if (i == retryCount - 1) throw;
            Thread.Sleep(delay);
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "LocalVenue API V1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "LocalVenue API";
});

app.UseAntiforgery();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<BlazorCookieLoginMiddleware>();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();



app.MapControllers();

app.Run();