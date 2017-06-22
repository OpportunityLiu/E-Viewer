using EhWikiClient.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EhWikiClient.Migrations
{
    [DbContext(typeof(WikiDb))]
    [Migration("20170503025602_AddTagType")]
    partial class AddTagType
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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
