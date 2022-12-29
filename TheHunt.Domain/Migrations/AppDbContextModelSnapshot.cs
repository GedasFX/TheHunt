﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TheHunt.Domain;

#nullable disable

namespace TheHunt.Domain.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("TheHunt.Domain.Models.Competition", b =>
                {
                    b.Property<decimal>("ChannelId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("channel_id");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("end_date");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("start_date");

                    b.Property<decimal>("SubmissionChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("submission_channel_id");

                    b.HasKey("ChannelId");

                    b.ToTable("competitions");
                });

            modelBuilder.Entity("TheHunt.Domain.Models.Competition", b =>
                {
                    b.OwnsOne("TheHunt.Domain.Models.SpreadsheetReference", "Spreadsheet", b1 =>
                        {
                            b1.Property<decimal>("CompetitionChannelId")
                                .HasColumnType("numeric(20,0)");

                            b1.Property<int>("ItemsSheet")
                                .HasColumnType("integer")
                                .HasColumnName("sheet_items");

                            b1.Property<int>("MembersSheet")
                                .HasColumnType("integer")
                                .HasColumnName("sheet_members");

                            b1.Property<string>("SpreadsheetId")
                                .IsRequired()
                                .HasColumnType("text")
                                .HasColumnName("spreadsheet_id");

                            b1.Property<int>("SubmissionsSheet")
                                .HasColumnType("integer")
                                .HasColumnName("sheet_submissions");

                            b1.HasKey("CompetitionChannelId");

                            b1.ToTable("competitions");

                            b1.WithOwner()
                                .HasForeignKey("CompetitionChannelId");
                        });

                    b.Navigation("Spreadsheet")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
