using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using EhTagClient.Models;
using ExClient;

namespace EhTagClient.Migrations
{
    [DbContext(typeof(TagDb))]
    [Migration("20170325074042_FirstMigration")]
    partial class FirstMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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
