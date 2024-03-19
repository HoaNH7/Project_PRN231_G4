using BookAPI.Models;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using System.Diagnostics.Metrics;

namespace BookAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Category>("Categories");
            modelBuilder.EntitySet<User>("Users");
            modelBuilder.EntitySet<Book>("Books");
            modelBuilder.EntitySet<Order>("Orders");
            modelBuilder.EntitySet<OrderDetail>("OrderDetails");

            builder.Services.AddControllers().AddOData(option
                => option.Select().OrderBy().Filter().Count().Expand().SetMaxTop(100)
                .AddRouteComponents("odata", modelBuilder.GetEdmModel()));
            builder.Services.AddDbContext<BookStoreContext>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
