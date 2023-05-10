﻿// <auto-generated />
using System;
using DnaVastgoed.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DnaVastgoed.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.16");

            modelBuilder.Entity("DnaVastgoed.Models.DnaProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AfgebakendOverstromingsGebied")
                        .HasColumnType("TEXT");

                    b.Property<string>("Bathrooms")
                        .HasColumnType("TEXT");

                    b.Property<string>("Bedrooms")
                        .HasColumnType("TEXT");

                    b.Property<string>("Bouwvergunning")
                        .HasColumnType("TEXT");

                    b.Property<string>("BuildingYear")
                        .HasColumnType("TEXT");

                    b.Property<string>("CoordinatesLat")
                        .HasColumnType("TEXT");

                    b.Property<string>("CoordinatesLng")
                        .HasColumnType("TEXT");

                    b.Property<string>("Dagvaarding")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("EPCNumber")
                        .HasColumnType("TEXT");

                    b.Property<string>("Elektriciteitskeuring")
                        .HasColumnType("TEXT");

                    b.Property<string>("Energy")
                        .HasColumnType("TEXT");

                    b.Property<string>("GScore")
                        .HasColumnType("TEXT");

                    b.Property<string>("KatastraalInkomen")
                        .HasColumnType("TEXT");

                    b.Property<string>("LivingArea")
                        .HasColumnType("TEXT");

                    b.Property<string>("Location")
                        .HasColumnType("TEXT");

                    b.Property<string>("LotArea")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("OrientatieAchtergevel")
                        .HasColumnType("TEXT");

                    b.Property<string>("PScore")
                        .HasColumnType("TEXT");

                    b.Property<string>("Price")
                        .HasColumnType("TEXT");

                    b.Property<string>("RisicoOverstroming")
                        .HasColumnType("TEXT");

                    b.Property<string>("Rooms")
                        .HasColumnType("TEXT");

                    b.Property<bool>("SendToSubscribers")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Status")
                        .HasColumnType("TEXT");

                    b.Property<string>("StedenbouwkundigeBestemming")
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .HasColumnType("TEXT");

                    b.Property<string>("URL")
                        .HasColumnType("TEXT");

                    b.Property<bool>("UploadToImmovlan")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("UploadToSpotto")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Verkavelingsvergunning")
                        .HasColumnType("TEXT");

                    b.Property<string>("Verkooprecht")
                        .HasColumnType("TEXT");

                    b.Property<string>("Voorkooprecht")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Properties");
                });

            modelBuilder.Entity("DnaVastgoed.Models.DnaPropertyImage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("DnaPropertyId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Url")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("DnaPropertyId");

                    b.ToTable("DnaPropertyImage");
                });

            modelBuilder.Entity("DnaVastgoed.Models.Subscriber", b =>
                {
                    b.Property<string>("Email")
                        .HasColumnType("TEXT");

                    b.Property<int>("Bedrooms")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Firstname")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<string>("Lastname")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<int>("MaxPrice")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MinPrice")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Postalcode")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<int>("RadiusInKM")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("Suppressed")
                        .HasColumnType("TEXT");

                    b.Property<string>("Telephone")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("Email");

                    b.ToTable("Subscribers");
                });

            modelBuilder.Entity("DnaVastgoed.Models.DnaPropertyImage", b =>
                {
                    b.HasOne("DnaVastgoed.Models.DnaProperty", null)
                        .WithMany("Images")
                        .HasForeignKey("DnaPropertyId");
                });

            modelBuilder.Entity("DnaVastgoed.Models.DnaProperty", b =>
                {
                    b.Navigation("Images");
                });
#pragma warning restore 612, 618
        }
    }
}
