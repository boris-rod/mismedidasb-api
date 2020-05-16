﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MismeAPI.Data;

namespace MismeAPI.Data.Migrations
{
    [DbContext(typeof(MismeContext))]
    partial class MismeContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("MismeAPI.Data.Entities.Answer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Order")
                        .HasColumnType("int");

                    b.Property<int>("QuestionId")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("TitleEN")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("TitleIT")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<int>("Weight")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("QuestionId");

                    b.ToTable("answer");
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.CompoundDish", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Image")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("ImageMimeType")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Name")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("compounddish");
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.Concept", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Codename")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Description")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("DescriptionEN")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("DescriptionIT")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Image")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Instructions")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("InstructionsEN")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("InstructionsIT")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Title")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("TitleEN")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("TitleIT")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.ToTable("concept");
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.ContactUs", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Body")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Priority")
                        .HasColumnType("int");

                    b.Property<bool>("Read")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Subject")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("contactus");
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.Device", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Token")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("device");
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.Dish", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<double>("Calcium")
                        .HasColumnType("double");

                    b.Property<double>("Calories")
                        .HasColumnType("double");

                    b.Property<double>("Carbohydrates")
                        .HasColumnType("double");

                    b.Property<double>("Cholesterol")
                        .HasColumnType("double");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<double>("Fat")
                        .HasColumnType("double");

                    b.Property<double>("Fiber")
                        .HasColumnType("double");

                    b.Property<double>("FolicAcid")
                        .HasColumnType("double");

                    b.Property<string>("Image")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("ImageMimeType")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<double>("Iron")
                        .HasColumnType("double");

                    b.Property<bool>("IsCaloric")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsFruitAndVegetables")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsProteic")
                        .HasColumnType("tinyint(1)");

                    b.Property<double>("Magnesium")
                        .HasColumnType("double");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("NameEN")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("NameIT")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<double>("Niacin")
                        .HasColumnType("double");

                    b.Property<double>("Phosphorus")
                        .HasColumnType("double");

                    b.Property<double>("Potassium")
                        .HasColumnType("double");

                    b.Property<double>("Proteins")
                        .HasColumnType("double");

                    b.Property<double>("Ribofla")
                        .HasColumnType("double");

                    b.Property<double>("Sodium")
                        .HasColumnType("double");

                    b.Property<double>("Thiamine")
                        .HasColumnType("double");

                    b.Property<double>("VitaminA")
                        .HasColumnType("double");

                    b.Property<double>("VitaminB12")
                        .HasColumnType("double");

                    b.Property<double>("VitaminB6")
                        .HasColumnType("double");

                    b.Property<double>("VitaminC")
                        .HasColumnType("double");

                    b.Property<double>("Zinc")
                        .HasColumnType("double");

                    b.HasKey("Id");

                    b.ToTable("dish");
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.DishCompoundDish", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("CompoundDishId")
                        .HasColumnType("int");

                    b.Property<int>("DishId")
                        .HasColumnType("int");

                    b.Property<int>("DishQty")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CompoundDishId");

                    b.HasIndex("DishId");

                    b.ToTable("dishcompounddish");
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.DishTag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("DishId")
                        .HasColumnType("int");

                    b.Property<int>("TagId")
                        .HasColumnType("int");

                    b.Property<DateTime>("TaggedAt")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("DishId");

                    b.HasIndex("TagId");

                    b.ToTable("dishtag");
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.Eat", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("EatType")
                        .HasColumnType("int");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("eat");
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.EatDish", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("DishId")
                        .HasColumnType("int");

                    b.Property<int>("EatId")
                        .HasColumnType("int");

                    b.Property<int>("Qty")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DishId");

                    b.HasIndex("EatId");

                    b.ToTable("eatdish");
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.GeneralContent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Content")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("ContentEN")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("ContentIT")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<int>("ContentType")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("generalcontent");
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.Poll", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("ConceptId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Description")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("DescriptionEN")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("DescriptionIT")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("HtmlContent")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("HtmlContentEN")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("HtmlContentIT")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<bool>("IsReadOnly")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("NameEN")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("NameIT")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<int>("Order")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ConceptId");

                    b.ToTable("poll");
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.Question", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Order")
                        .HasColumnType("int");

                    b.Property<int>("PollId")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("TitleEN")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("TitleIT")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.HasIndex("PollId");

                    b.ToTable("question");
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.Reminder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Body")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("BodyEN")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("BodyIT")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("CodeName")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("CronExpression")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Title")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("TitleEN")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("TitleIT")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.ToTable("reminder");
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.Result", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("CodeName")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("ConceptName")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Text")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("TextEN")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("TextIT")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.ToTable("result");
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.Setting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Name")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.ToTable("setting");
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.ToTable("tag");
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.Tip", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Content")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("ContentEN")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("ContentIT")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("PollId")
                        .HasColumnType("int");

                    b.Property<int>("TipPosition")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("PollId");

                    b.ToTable("tip");
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime?>("ActivatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Avatar")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("AvatarMimeType")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("DisabledAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<DateTime?>("LastLoggedIn")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("MarkedForDeletion")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Phone")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<bool>("TermsAndConditionsAccepted")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("VerificationCode")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("user");
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.UserAnswer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("AnswerId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AnswerId");

                    b.HasIndex("UserId");

                    b.ToTable("useranswer");
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.UserConcept", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CompletedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("ConceptId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ConceptId");

                    b.HasIndex("UserId");

                    b.ToTable("userconcept");
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.UserSetting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("SettingId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("Value")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.HasIndex("SettingId");

                    b.HasIndex("UserId");

                    b.ToTable("usersetting");
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.UserToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("AccessToken")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<DateTimeOffset>("AccessTokenExpiresDateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("ClientName")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("ClientType")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("ClientVersion")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("DeviceBrand")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("DeviceModel")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("OS")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("OSPlatform")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("OSVersion")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("RefreshToken")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<DateTimeOffset>("RefreshTokenExpiresDateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("usertoken");
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.Answer", b =>
                {
                    b.HasOne("MismeAPI.Data.Entities.Question", "Question")
                        .WithMany("Answers")
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.CompoundDish", b =>
                {
                    b.HasOne("MismeAPI.Data.Entities.User", "CreatedBy")
                        .WithMany("CompoundDishs")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.ContactUs", b =>
                {
                    b.HasOne("MismeAPI.Data.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.Device", b =>
                {
                    b.HasOne("MismeAPI.Data.Entities.User", null)
                        .WithMany("Devices")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.DishCompoundDish", b =>
                {
                    b.HasOne("MismeAPI.Data.Entities.CompoundDish", "CompoundDish")
                        .WithMany("DishCompoundDishes")
                        .HasForeignKey("CompoundDishId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MismeAPI.Data.Entities.Dish", "Dish")
                        .WithMany("DishCompoundDishes")
                        .HasForeignKey("DishId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.DishTag", b =>
                {
                    b.HasOne("MismeAPI.Data.Entities.Dish", "Dish")
                        .WithMany("DishTags")
                        .HasForeignKey("DishId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MismeAPI.Data.Entities.Tag", "Tag")
                        .WithMany("DishTags")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.Eat", b =>
                {
                    b.HasOne("MismeAPI.Data.Entities.User", "User")
                        .WithMany("Eats")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.EatDish", b =>
                {
                    b.HasOne("MismeAPI.Data.Entities.Dish", "Dish")
                        .WithMany("EatDishes")
                        .HasForeignKey("DishId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MismeAPI.Data.Entities.Eat", "Eat")
                        .WithMany("EatDishes")
                        .HasForeignKey("EatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.Poll", b =>
                {
                    b.HasOne("MismeAPI.Data.Entities.Concept", "Concept")
                        .WithMany("Polls")
                        .HasForeignKey("ConceptId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.Question", b =>
                {
                    b.HasOne("MismeAPI.Data.Entities.Poll", "Poll")
                        .WithMany("Questions")
                        .HasForeignKey("PollId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.Tip", b =>
                {
                    b.HasOne("MismeAPI.Data.Entities.Poll", "Poll")
                        .WithMany("Tips")
                        .HasForeignKey("PollId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.UserAnswer", b =>
                {
                    b.HasOne("MismeAPI.Data.Entities.Answer", "Answer")
                        .WithMany()
                        .HasForeignKey("AnswerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MismeAPI.Data.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.UserConcept", b =>
                {
                    b.HasOne("MismeAPI.Data.Entities.Concept", "Concept")
                        .WithMany("UserConcepts")
                        .HasForeignKey("ConceptId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MismeAPI.Data.Entities.User", "User")
                        .WithMany("UserConcepts")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.UserSetting", b =>
                {
                    b.HasOne("MismeAPI.Data.Entities.Setting", "Setting")
                        .WithMany()
                        .HasForeignKey("SettingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MismeAPI.Data.Entities.User", "User")
                        .WithMany("UserSettings")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MismeAPI.Data.Entities.UserToken", b =>
                {
                    b.HasOne("MismeAPI.Data.Entities.User", "User")
                        .WithMany("UserTokens")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
