using Microsoft.EntityFrameworkCore;
using StockCristales.Models.Entidades;

namespace StockCristales.Data
{
    public class StockCristalesContext : DbContext
    {
        public StockCristalesContext(
            DbContextOptions<StockCristalesContext> options)
            : base(options)
        {
        }


        public DbSet<Cristal> Cristales { get; set; }
        public DbSet<TipoCristal> TiposCristal { get; set; }
        public DbSet<MarcaCristal> MarcasCristal { get; set; }
        public DbSet<ColorCristal> ColoresCristal { get; set; }
        public DbSet<OrigenCristal> OrigenesCristal { get; set; }

        public DbSet<MarcaVehiculo> MarcasVehiculo { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }

        public DbSet<RelacionCristalTipoCristal> RelacionesCristalTipoCristal { get; set; }

        public DbSet<CaracteristicaCristal> CaracteristicasCristal { get; set; }

        public DbSet<RelacionCristalCaracteristica>
            RelacionesCristalCaracteristica
        { get; set; }

        public DbSet<RelacionCristalVehiculo>
            RelacionesCristalVehiculo
        { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CLAVES PRIMARIAS

            modelBuilder.Entity<Cristal>()
                .HasKey(x => x.IdCristal);

            modelBuilder.Entity<TipoCristal>()
                .HasKey(x => x.IdTipoCristal);

            modelBuilder.Entity<MarcaCristal>()
                .HasKey(x => x.IdMarcaCristal);

            modelBuilder.Entity<ColorCristal>()
                .HasKey(x => x.IdColorCristal);

            modelBuilder.Entity<OrigenCristal>()
                .HasKey(x => x.IdOrigenCristal);

            modelBuilder.Entity<MarcaVehiculo>()
                .HasKey(x => x.IdMarcaVehiculo);

            modelBuilder.Entity<Vehiculo>()
                .HasKey(x => x.IdVehiculo);

            modelBuilder.Entity<CaracteristicaCristal>()
                .HasKey(x => x.IdCaracteristicaCristal);

            modelBuilder.Entity<RelacionCristalCaracteristica>()
                .HasKey(x => x.IdRelacionCristalCaracteristica);

            modelBuilder.Entity<RelacionCristalVehiculo>()
                .HasKey(x => x.IdRelacionCristalVehiculo);

            modelBuilder.Entity<RelacionCristalTipoCristal>()
                .HasKey(x => x.IdRelacionCristalTipoCristal);



            // VEHICULO -> MARCA VEHICULO

            modelBuilder.Entity<Vehiculo>()
                .HasOne(x => x.MarcaVehiculo)
                .WithMany()
                .HasForeignKey(x => x.IdMarcaVehiculo)
                .OnDelete(DeleteBehavior.Restrict);


            // CRISTAL <-> TIPO CRISTAL

            modelBuilder.Entity<RelacionCristalTipoCristal>()
                .HasOne(x => x.Cristal)
                .WithMany(x => x.RelacionesTiposCristal)
                .HasForeignKey(x => x.IdCristal);

            modelBuilder.Entity<RelacionCristalTipoCristal>()
                .HasOne(x => x.TipoCristal)
                .WithMany(x => x.RelacionesCristales)
                .HasForeignKey(x => x.IdTipoCristal);


            // CRISTAL -> MARCA

            modelBuilder.Entity<Cristal>()
                .HasOne(x => x.MarcaCristal)
                .WithMany()
                .HasForeignKey(x => x.IdMarcaCristal)
                .OnDelete(DeleteBehavior.Restrict);


            // CRISTAL -> COLOR

            modelBuilder.Entity<Cristal>()
                .HasOne(x => x.ColorCristal)
                .WithMany()
                .HasForeignKey(x => x.IdColorCristal)
                .OnDelete(DeleteBehavior.Restrict);


            // CRISTAL -> ORIGEN

            modelBuilder.Entity<Cristal>()
                .HasOne(x => x.OrigenCristal)
                .WithMany()
                .HasForeignKey(x => x.IdOrigenCristal)
                .OnDelete(DeleteBehavior.Restrict);

            // CRISTAL -> TIPO CRISTAL
            modelBuilder.Entity<RelacionCristalTipoCristal>()
                .HasOne(x => x.Cristal)
                .WithMany(x => x.RelacionesTiposCristal)
                .HasForeignKey(x => x.IdCristal);

            modelBuilder.Entity<RelacionCristalTipoCristal>()
                .HasOne(x => x.TipoCristal)
                .WithMany(x => x.RelacionesCristales)
                .HasForeignKey(x => x.IdTipoCristal);


            // CRISTAL <-> CARACTERISTICA

            modelBuilder.Entity<RelacionCristalCaracteristica>()
                .HasOne(x => x.Cristal)
                .WithMany(x => x.RelacionesCaracteristicas)
                .HasForeignKey(x => x.IdCristal);

            modelBuilder.Entity<RelacionCristalCaracteristica>()
                .HasOne(x => x.CaracteristicaCristal)
                .WithMany(x => x.RelacionesCristales)
                .HasForeignKey(x => x.IdCaracteristicaCristal);


            // CRISTAL <-> VEHICULO

            modelBuilder.Entity<RelacionCristalVehiculo>()
                .HasOne(x => x.Cristal)
                .WithMany(x => x.RelacionesVehiculos)
                .HasForeignKey(x => x.IdCristal);

            modelBuilder.Entity<RelacionCristalVehiculo>()
                .HasOne(x => x.Vehiculo)
                .WithMany(x => x.RelacionesCristales)
                .HasForeignKey(x => x.IdVehiculo);


            // RESTRICCIONES

            modelBuilder.Entity<Cristal>()
                .HasIndex(x => x.Codigo)
                .IsUnique();

            modelBuilder.Entity<RelacionCristalCaracteristica>()
                .HasIndex(x => new
                {
                    x.IdCristal,
                    x.IdCaracteristicaCristal
                })
                .IsUnique();

            modelBuilder.Entity<RelacionCristalVehiculo>()
                .HasIndex(x => new
                {
                    x.IdCristal,
                    x.IdVehiculo,
                    x.AnioDesde
                })
                .IsUnique();

            modelBuilder.Entity<RelacionCristalTipoCristal>()
                .HasIndex(x => new
                {
                    x.IdCristal,
                    x.IdTipoCristal
                })
                .IsUnique();


            // DATOS INICIALES

            modelBuilder.Entity<TipoCristal>().HasData(
                new TipoCristal { IdTipoCristal = 1, Descripcion = "Parabrisas" },
                new TipoCristal { IdTipoCristal = 2, Descripcion = "Luneta" },
                new TipoCristal { IdTipoCristal = 3, Descripcion = "Puerta delantera izquierda" },
                new TipoCristal { IdTipoCristal = 4, Descripcion = "Puerta delantera derecha" },
                new TipoCristal { IdTipoCristal = 5, Descripcion = "Puerta trasera izquierda" },
                new TipoCristal { IdTipoCristal = 6, Descripcion = "Puerta trasera derecha" },
                new TipoCristal { IdTipoCristal = 7, Descripcion = "Custodia izquierda" },
                new TipoCristal { IdTipoCristal = 8, Descripcion = "Custodia derecha" },
                new TipoCristal { IdTipoCristal = 9, Descripcion = "Ventilete delantero izquierdo" },
                new TipoCristal { IdTipoCristal = 10, Descripcion = "Ventilete delantero derecho" },
                new TipoCristal { IdTipoCristal = 11, Descripcion = "Ventilete trasero izquierdo" },
                new TipoCristal { IdTipoCristal = 12, Descripcion = "Ventilete trasero derecho" },
                new TipoCristal { IdTipoCristal = 13, Descripcion = "Techo" }
            );

            modelBuilder.Entity<MarcaCristal>().HasData(
                    new MarcaCristal
                    {
                        IdMarcaCristal = 1,
                        Nombre = "Pilkington"
                    }
                );

            modelBuilder.Entity<ColorCristal>().HasData(
                new ColorCristal { IdColorCristal = 1, Descripcion = "Verde" },
                new ColorCristal { IdColorCristal = 2, Descripcion = "Verde Plus" },
                new ColorCristal { IdColorCristal = 3, Descripcion = "Degradé" },
                new ColorCristal { IdColorCristal = 4, Descripcion = "Azul" },
                new ColorCristal { IdColorCristal = 5, Descripcion = "Gris" },
                new ColorCristal { IdColorCristal = 6, Descripcion = "Incoloro" }
               );




        }
    }
}
