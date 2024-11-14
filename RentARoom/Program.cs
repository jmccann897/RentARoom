using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using RentARoom.DataAccess.Data;
using RentARoom.DataAccess.Repository;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.Hubs;
using RentARoom.Utility;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options=>options.UseSqlServer(connectionString));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()//AddEFStores defines Db linked to identity
    .AddDefaultTokenProviders();//Needed for email token generation no longer using basic identity

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

builder.Services.AddRazorPages(); // needed for identity
builder.Services.AddControllersWithViews();

// adding signal r - https://learn.microsoft.com/en-us/aspnet/core/tutorials/signalr?view=aspnetcore-8.0&tabs=visual-studio
// https://learn.microsoft.com/en-us/aspnet/core/signalr/hubcontext?view=aspnetcore-2.2

// Chat
builder.Services.AddSignalR();

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
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages(); // needed for identity which use razor pages
app.MapHub<ChatHub>("/chatHub"); // needed for signalR

//Route for MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{area=User}/{controller=Home}/{action=Index}/{id?}");



#region Role management
////Seeding initial role data
//using (var scope = app.Services.CreateScope()) 
//{
//    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

//    var roles = new[] { "Admin", "Owner", "Renter" };

//    //loop through role and check if exists or not
//    foreach ( var role in roles)
//    {
//        if(!await roleManager.RoleExistsAsync(role))
//        {
//            //create if not present
//            await roleManager.CreateAsync(new IdentityRole(role));
//        }
//    }
//}
////Seeding initial account data
//using (var scope = app.Services.CreateScope())
//{
//    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

//    string email = "admin@admin.com";
//    string password = "Password.01";

//    //Check if user already present
//    if(await userManager.FindByEmailAsync(email) == null)
//    {
//        //Create data for user to input
//        var user = new IdentityUser();
//        user.UserName = email;
//        user.Email = email;
       
//        //Create user
//        await userManager.CreateAsync (user, password);
//        //Add role to user
//        await userManager.AddToRoleAsync(user, "Admin");
//    }
//}
#endregion


//starts the app
app.Run();
