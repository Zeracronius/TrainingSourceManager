using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingSourceManager.Data
{
    internal class DataContext : DbContext
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public DbSet<Source> Sources { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured == false)
            {
                optionsBuilder.UseSqlite(@"Data Source=SourceData.db");
                optionsBuilder.LogTo((string x) => System.Diagnostics.Debug.WriteLine(x));
                base.OnConfiguring(optionsBuilder);
            }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<File>()
                .HasOne<FileData>("FileData")
                .WithOne()
                .HasForeignKey<FileData>(x => x.FileId);

            modelBuilder.Entity<File>()           
                .HasIndex(x => x.Name)
                .IsUnique(true);
        }

    }
}
