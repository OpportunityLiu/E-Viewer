using EhTagClient.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EhTagClient.Migrations
{
    [DbContext(typeof(TagDb))]
    partial class TagDbModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1");

            modelBuilder.Entity("EhTagClient.TagRecord", b =>
                {
                    b.Property<int>("TagId");

                    b.Property<string>("TagConetnt");

                    b.Property<int>("TagNamespace");

                    b.HasKey("TagId");

                    b.ToTable("TagTable");
                });
        }
    }
}
