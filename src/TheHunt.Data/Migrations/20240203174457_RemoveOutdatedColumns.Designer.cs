﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TheHunt.Data;

#nullable disable

namespace TheHunt.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240203174457_RemoveOutdatedColumns")]
    partial class RemoveOutdatedColumns
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.1");

            modelBuilder.Entity("TheHunt.Data.Models.Competition", b =>
                {
                    b.Property<ulong>("ChannelId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("channel_id");

                    b.Property<ulong>("VerifierRoleId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("role_verifier");

                    b.HasKey("ChannelId");

                    b.ToTable("competitions");
                });

            modelBuilder.Entity("TheHunt.Data.Models.Competition", b =>
                {
                    b.OwnsOne("TheHunt.Data.Models.SheetsRef", "Spreadsheet", b1 =>
                        {
                            b1.Property<ulong>("CompetitionChannelId")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("SheetName")
                                .IsRequired()
                                .HasMaxLength(20)
                                .HasColumnType("TEXT")
                                .HasColumnName("sheet_name");

                            b1.Property<string>("SpreadsheetId")
                                .IsRequired()
                                .HasMaxLength(44)
                                .HasColumnType("TEXT")
                                .HasColumnName("spreadsheet_id");

                            b1.HasKey("CompetitionChannelId");

                            b1.ToTable("competitions");

                            b1.WithOwner()
                                .HasForeignKey("CompetitionChannelId");

                            b1.OwnsOne("TheHunt.Data.Models.SheetsRef+Sheet", "Sheets", b2 =>
                                {
                                    b2.Property<ulong>("SheetsRefCompetitionChannelId")
                                        .HasColumnType("INTEGER");

                                    b2.Property<int>("Items")
                                        .HasColumnType("INTEGER")
                                        .HasColumnName("sheet_items");

                                    b2.Property<int>("Members")
                                        .HasColumnType("INTEGER")
                                        .HasColumnName("sheet_members");

                                    b2.Property<int>("Overview")
                                        .HasColumnType("INTEGER")
                                        .HasColumnName("sheet_overview");

                                    b2.Property<int>("Submissions")
                                        .HasColumnType("INTEGER")
                                        .HasColumnName("sheet_submissions");

                                    b2.HasKey("SheetsRefCompetitionChannelId");

                                    b2.ToTable("competitions");

                                    b2.WithOwner()
                                        .HasForeignKey("SheetsRefCompetitionChannelId");
                                });

                            b1.Navigation("Sheets")
                                .IsRequired();
                        });

                    b.Navigation("Spreadsheet")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
