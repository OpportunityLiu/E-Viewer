using EhWikiClient.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EhWikiClient.Migrations
{
    [DbContext(typeof(WikiDb))]
    partial class WikiDbModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1");

            modelBuilder.Entity("EhWikiClient.Record", b =>
                {
                    b.Property<string>("Title");

                    b.Property<string>("Description");

                    b.Property<bool>("IsValid");

                    b.Property<string>("Japanese");

                    b.Property<int>("Type");

                    b.Property<long>("lastUpdate");

                    b.HasKey("Title");

                    b.ToTable("Table");
                });
        }
    }
}
