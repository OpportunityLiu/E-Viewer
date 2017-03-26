using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using EhTagTranslatorClient.Model;
using ExClient;

namespace EhTagTranslatorClient.Migrations
{
    [DbContext(typeof(TranslateDb))]
    partial class TranslateDbModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1");

            modelBuilder.Entity("EhTagTranslatorClient.Record", b =>
                {
                    b.Property<int>("Namespace");

                    b.Property<string>("Original");

                    b.Property<string>("IntroductionRaw");

                    b.Property<string>("TranslatedRaw");

                    b.Property<string>("TranslatedStr");

                    b.HasKey("Namespace", "Original");

                    b.ToTable("Table");
                });
        }
    }
}
