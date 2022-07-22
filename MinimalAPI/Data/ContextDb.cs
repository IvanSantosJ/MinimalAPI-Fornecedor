using Microsoft.EntityFrameworkCore;
using MinimalAPI.Models;

namespace MinimalAPI.Data
{
    public class ContextDb : DbContext
    {
        public ContextDb(DbContextOptions<ContextDb> options) : base(options) { }

        public DbSet<Fornecedor> Fornecedores { get; set; }

                protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Fornecedor>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Fornecedor>()
                .Property(p => p.Nome)
                .IsRequired()
                .HasColumnType("varchar(200)");

            modelBuilder.Entity<Fornecedor>()
                .Property(p => p.CNPJ)
                .IsRequired()
                .HasColumnType("varchar(14)");

            modelBuilder.Entity<Fornecedor>()
                .Property(p => p.CNPJ_Formatado)
                .IsRequired()
                .HasColumnType("varchar(18)");

            modelBuilder.Entity<Fornecedor>()
                .ToTable("Fornecedores");

            base.OnModelCreating(modelBuilder);
        }
    }
}
