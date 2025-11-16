using System.Threading.Tasks;
using ABC_Retail.Data;
using ABC_Retail.Models;
using ABC_Retail.SeedData;
using ABC_Retail.Services;
using ABC_Retail.Services.Functions.CustomerFunctions;
using ABC_Retail.Services.Functions.ProductFunctions;
using ABC_Retail.Services.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ABC_Retail
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            //retrivieving the connection strings from appsettings.json
            var storageConnectionString = builder.Configuration.GetConnectionString("storageConnectionString")
                ?? throw new InvalidOperationException("The StorageConnectionString is missing.");

            var sqlConnectionString = builder.Configuration.GetConnectionString("dbConnectionString")
                ?? throw new InvalidOperationException("dbConnectionString is missing.");

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(sqlConnectionString));

            builder.Services.AddIdentity<Users, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;

            })
             .AddEntityFrameworkStores<AppDbContext>()
             .AddDefaultTokenProviders();

            //registering the diferent storage services
            builder.Services.AddSingleton<CustomerService>(sp =>
                new CustomerService(storageConnectionString, "Customer"));

            var blobService = new BlobStorageService(storageConnectionString, "product-photos");
            builder.Services.AddSingleton(blobService);

            // Registering the TableStorageService for the Product information table
            var tableService = new TableStorageService<Product>(storageConnectionString, "ProductInfo");
            builder.Services.AddSingleton(tableService);

            // registering the ProductService itself, injecting the blob and table services
            builder.Services.AddSingleton<ProductService>(sp =>
                new ProductService(storageConnectionString, "ProductInfo", blobService));

            var queueService = new QueueStorageService(storageConnectionString, "order-log-messages");
            builder.Services.AddSingleton(queueService);

            builder.Services.AddSingleton(new FileShareStorageService(storageConnectionString, "contracts"));

            // Register OrderService
            builder.Services.AddScoped<OrderService>(sp =>
            {
                var dbContext = sp.GetRequiredService<AppDbContext>();
                var queueService = sp.GetRequiredService<QueueStorageService>();
                return new OrderService(dbContext, queueService);
            });


            //registering the function services
            builder.Services.AddHttpClient<CustomerFunctionService>();
            builder.Services.AddSingleton<CustomerFunctionService>();

            builder.Services.AddHttpClient<ProductFunctionService>();
            builder.Services.AddSingleton<ProductFunctionService>();

            var app = builder.Build();
            await SeedService.SeedDatabase(app.Services);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
