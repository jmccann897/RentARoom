using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentARoom.DataAccess.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options=>options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false) //switch when deploying to have emails confirmed
//    .AddRoles<IdentityRole>() //Needed for roles
//    .AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.AddControllersWithViews();

var app = builder.Build();

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

//app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.MapRazorPages();

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
