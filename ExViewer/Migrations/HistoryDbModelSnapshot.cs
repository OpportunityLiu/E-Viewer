using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using ExViewer.Database;

namespace ExViewer.Migrations
{
    [DbContext(typeof(HistoryDb))]
    partial class HistoryDbModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.5");

            modelBuilder.Entity("ExViewer.Database.HistoryRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("TimeStamp");

                    b.Property<string>("Title");

                    b.Property<int>("Type");

                    b.Property<string>("uri")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("Title");

                    b.ToTable("HistorySet");
                });
        }
    }
}
