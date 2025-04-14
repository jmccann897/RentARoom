using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using RentARoom.DataAccess.Data;
using RentARoom.DataAccess.Repository;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.Hubs;
using RentARoom.Models;
using RentARoom.Services.IServices;
using RentARoom.Utility;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Azure;
using Azure.Identity;
using System.Globalization;
using Microsoft.AspNetCore.Localization;


var builder = WebApplication.CreateBuilder(args);
// Ensure User Secrets are loaded in Development mode
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}
else
{
    // Retrieve the Key Vault URI from an app setting or environment variable.
    // For example, set "KeyVaultUri" in your production configuration.
    var keyVaultUri = builder.Configuration["KeyVaultUri"];
    if (!string.IsNullOrEmpty(keyVaultUri))
    {
        builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
    }
}

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()//AddEFStores defines Db linked to identity
    .AddDefaultTokenProviders();//Needed for email token generation no longer using basic identity

//logging errors trying to trace identity issues
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString).EnableSensitiveDataLogging());

// Needed to redirect with identity pages ***MUST BE AFTER ADDIDENTITY***
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDeniedPath";
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Unique DI
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IAzureBlobService, AzureBlobService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddTransient<ITravelTimeService, TravelTimeService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<IPropertyViewService, PropertyViewService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IMapService, MapService>();
builder.Services.AddSingleton<IHttpClientWrapper, HttpClientWrapper>();


builder.Services.AddRazorPages(); // needed for identity
builder.Services.AddControllersWithViews();

// adding signal r - https://learn.microsoft.com/en-us/aspnet/core/tutorials/signalr?view=aspnetcore-8.0&tabs=visual-studio
// https://learn.microsoft.com/en-us/aspnet/core/signalr/hubcontext?view=aspnetcore-2.2

// SignalR
builder.Services.AddSignalR();

//https://medium.com/@nizzola.dev/how-to-solve-jsonexception-a-possible-object-cycle-was-detected-9a349439c3cd
//https://learn.microsoft.com/en-us/ef/core/querying/related-data/serialization
builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["AzureBlobStorage:blob"]!, preferMsi: true);
    clientBuilder.AddQueueServiceClient(builder.Configuration["AzureBlobStorage:queue"]!, preferMsi: true);
});

var app = builder.Build();

// Chat
app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// For formatting currency in Azure
var cultureInfo = new CultureInfo("en-GB");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultureInfo),
    SupportedCultures = new List<CultureInfo> { cultureInfo },
    SupportedUICultures = new List<CultureInfo> { cultureInfo }
};

app.UseRequestLocalization(localizationOptions);

app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages(); // needed for identity which use razor pages
app.MapHub<ChatHub>("/chatHub"); // needed for signalR - chat
app.MapHub<PropertyViewHub>("/propertyViewHub"); // needed for signalR - propertyViews
app.MapHub<NotificationHub>("/notificationHub"); // needed for signalR - chatmessage notifications

//Route for MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{area=User}/{controller=Home}/{action=Index}/{id?}");

// Seed data
var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

// Check if Db present or not
var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
dbContext.Database.Migrate();

try
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    

    await dbContext.SeedDataAsync(roleManager, userManager);
}
catch (Exception ex)
{
    // Handle errors
    Console.WriteLine($"An error occurred during database seeding: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
    }
}

//starts the app
app.Run();
