using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace IntegradorP.Models;

public partial class BaseDatosProyectoContext : DbContext
{
    public BaseDatosProyectoContext()
    {
    }

    public BaseDatosProyectoContext(DbContextOptions<BaseDatosProyectoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Deporte> Deportes { get; set; }

    public virtual DbSet<Empresa> Empresas { get; set; }

    public virtual DbSet<Instalacion> Instalacions { get; set; }

    public virtual DbSet<Perfil> Perfils { get; set; }

    public virtual DbSet<Recurso> Recursos { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<ReservaRecurso> ReservaRecursos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<UsuarioPerfil> UsuarioPerfils { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=JECV;Initial Catalog=BaseDatosProyecto;Integrated Security=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Deporte>(entity =>
        {
            entity.HasKey(e => e.DeporteId).HasName("PK__Deporte__A8A8687B7E25BF2C");

            entity.ToTable("Deporte");

            entity.Property(e => e.DeporteId).HasColumnName("DeporteID");
            entity.Property(e => e.NombreDep)
                .HasMaxLength(64)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.EmpresaId).HasName("PK__Empresa__7B9F2136A04D7965");

            entity.ToTable("Empresa");

            entity.Property(e => e.EmpresaId).HasColumnName("EmpresaID");
            entity.Property(e => e.NombreEmp)
                .HasMaxLength(32)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Instalacion>(entity =>
        {
            entity.HasKey(e => e.InstalacionId).HasName("PK__Instalac__63F5EA95CC29C5D9");

            entity.ToTable("Instalacion");

            entity.Property(e => e.InstalacionId).HasColumnName("InstalacionID");
            entity.Property(e => e.DeporteId).HasColumnName("DeporteID");
            entity.Property(e => e.DescripcioIns)
                .HasMaxLength(82)
                .IsUnicode(false);
            entity.Property(e => e.DireccionIns)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.NombreIns)
                .HasMaxLength(64)
                .IsUnicode(false);

            entity.HasOne(d => d.Deporte).WithMany(p => p.Instalacions)
                .HasForeignKey(d => d.DeporteId)
                .HasConstraintName("FK__Instalaci__Depor__33D4B598");
        });

        modelBuilder.Entity<Perfil>(entity =>
        {
            entity.HasKey(e => e.PerfilId).HasName("PK__Perfil__0C005B66650D2089");

            entity.ToTable("Perfil");

            entity.Property(e => e.PerfilId).HasColumnName("PerfilID");
            entity.Property(e => e.DescripcionPer)
                .HasMaxLength(82)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Recurso>(entity =>
        {
            entity.HasKey(e => e.RecursoId).HasName("PK__Recurso__82F2B1A4D037E6C9");

            entity.ToTable("Recurso");

            entity.Property(e => e.RecursoId).HasColumnName("RecursoID");
            entity.Property(e => e.DescripcionRec)
                .HasMaxLength(82)
                .IsUnicode(false);
            entity.Property(e => e.NombreRec)
                .HasMaxLength(64)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.ReservaId).HasName("PK__Reserva__C39937032AAC4655");

            entity.ToTable("Reserva");

            entity.Property(e => e.ReservaId).HasColumnName("ReservaID");
            entity.Property(e => e.Fecha).HasColumnType("date");
            entity.Property(e => e.InstalacionId).HasColumnName("InstalacionID");
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

            entity.HasOne(d => d.Instalacion).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.InstalacionId)
                .HasConstraintName("FK__Reserva__Instala__34C8D9D1");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__Reserva__Usuario__35BCFE0A");
        });

        modelBuilder.Entity<ReservaRecurso>(entity =>
        {
            entity.HasKey(e => e.Rrid).HasName("PK__ReservaR__E3054D13DFD28552");

            entity.ToTable("ReservaRecurso");

            entity.Property(e => e.Rrid).HasColumnName("RRID");
            entity.Property(e => e.RecursoId).HasColumnName("RecursoID");

            entity.HasOne(d => d.Recurso).WithMany(p => p.ReservaRecursos)
                .HasForeignKey(d => d.RecursoId)
                .HasConstraintName("FK__ReservaRe__Recur__36B12243");

            entity.HasOne(d => d.Reserva).WithMany(p => p.ReservaRecursos)
                .HasForeignKey(d => d.ReservaId)
                .HasConstraintName("FK__ReservaRe__Reser__37A5467C");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UsuarioId).HasName("PK__Usuario__2B3DE798BD6866E0");

            entity.ToTable("Usuario");

            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");
            entity.Property(e => e.CedulaUsu)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.EmailUsu)
                .HasMaxLength(82)
                .IsUnicode(false);
            entity.Property(e => e.NombreUsu)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<UsuarioPerfil>(entity =>
        {
            entity.HasKey(e => e.Upid).HasName("PK__UsuarioP__B6A76C77549CBB8B");

            entity.ToTable("UsuarioPerfil");

            entity.Property(e => e.Upid).HasColumnName("UPID");
            entity.Property(e => e.EmpresaId).HasColumnName("EmpresaID");
            entity.Property(e => e.PerfilId).HasColumnName("PerfilID");
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

            entity.HasOne(d => d.Empresa).WithMany(p => p.UsuarioPerfils)
                .HasForeignKey(d => d.EmpresaId)
                .HasConstraintName("FK__UsuarioPe__Empre__38996AB5");

            entity.HasOne(d => d.Perfil).WithMany(p => p.UsuarioPerfils)
                .HasForeignKey(d => d.PerfilId)
                .HasConstraintName("FK__UsuarioPe__Perfi__398D8EEE");

            entity.HasOne(d => d.Usuario).WithMany(p => p.UsuarioPerfils)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__UsuarioPe__Usuar__3A81B327");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
