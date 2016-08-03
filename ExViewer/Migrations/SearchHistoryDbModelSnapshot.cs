using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using ExViewer.Database;

namespace ExViewer.Migrations
{
    [DbContext(typeof(SearchHistoryDb))]
    partial class SearchHistoryDbModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431");

            modelBuilder.Entity("ExViewer.Database.SearchHistory", b =>
                {
                    b.Property<DateTimeOffset>("Time");

                    b.Property<string>("Content");

                    b.HasKey("Time");

                    b.ToTable("SearchHistorySet");
                });
        }
    }
}
