using Microsoft.EntityFrameworkCore;
using PetProject.DataAccess.Data;
using PetProject.DataAccess.Repository;
using PetProject.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// .AddEntityFrameworkStores<ApplicationDbContext>() binds identity tables to entity framework,
// that is all tables needed for identity will be managed with help of specified context.
builder.Services.AddDefaultIdentity<IdentityUser>().AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication always goes before Autorization.

// Checks if user login and password is valid. 
app.UseAuthentication();
// Autorization means that we accessed site, but it depends on our role what 
// features we can access.
app.UseAuthorization();


app.MapRazorPages();
// Default route to action that will be executed on application start.
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();
