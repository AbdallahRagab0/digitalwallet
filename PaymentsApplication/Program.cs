var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();


// Session
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;

    options.Cookie.SameSite = SameSiteMode.None;

    options.Cookie.SecurePolicy =
        CookieSecurePolicy.Always;
});


// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowHtml",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});



var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}



app.UseHttpsRedirection();


app.UseRouting();


// CORS
app.UseCors("AllowHtml");


// Session
app.UseSession();


app.UseAuthorization();


app.MapStaticAssets();


app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}"
)
.WithStaticAssets();


app.Run();