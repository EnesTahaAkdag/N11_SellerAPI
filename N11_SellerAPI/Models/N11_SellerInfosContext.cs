﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace N11_SellerAPI.Models;

public partial class N11_SellerInfosContext : DbContext
{
    public N11_SellerInfosContext(DbContextOptions<N11_SellerInfosContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Store> Stores { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Store>(entity =>
        {
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Category).HasMaxLength(500);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Mersis)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.RatingScore).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.SellerName).HasMaxLength(500);
            entity.Property(e => e.StoreLink)
                .IsRequired()
                .HasMaxLength(250);
            entity.Property(e => e.StoreName).HasMaxLength(50);
            entity.Property(e => e.StoreScore).HasColumnType("decimal(5, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}