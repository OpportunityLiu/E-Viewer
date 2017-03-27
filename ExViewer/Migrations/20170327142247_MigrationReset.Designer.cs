using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using ExViewer.Database;

namespace ExViewer.Migrations
{
    [DbContext(typeof(SearchHistoryDb))]
    [Migration("20170327142247_MigrationReset")]
    partial class MigrationReset
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1");

            modelBuilder.Entity("ExViewer.Database.SearchHistory", b =>
                {
                    b.Property<long>("time");

                    b.Property<string>("Content");

                    b.HasKey("time");

                    b.ToTable("SearchHistorySet");
                });
        }
    }
}
