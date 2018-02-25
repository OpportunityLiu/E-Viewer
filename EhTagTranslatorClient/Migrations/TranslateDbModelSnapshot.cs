using EhTagTranslatorClient.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EhTagTranslatorClient.Migrations
{
    [DbContext(typeof(TranslateDb))]
    partial class TranslateDbModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("EhTagTranslatorClient.Record", b =>
                {
                    b.Property<int>("Namespace");

                    b.Property<string>("Original");

                    b.Property<string>("ExternalLinksRaw");

                    b.Property<string>("IntroductionRaw");

                    b.Property<string>("TranslatedRaw");

                    b.Property<string>("TranslatedStr");

                    b.HasKey("Namespace", "Original");

                    b.ToTable("Table");
                });
        }
    }
}
