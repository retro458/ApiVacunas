using System;
using System.Collections.Generic;
using ApiVacunas.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiVacunas.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Alergia> Alergia { get; set; }

    public virtual DbSet<Auditoriausuario> Auditoriausuarios { get; set; }

    public virtual DbSet<Campaniavacunacion> Campaniavacunacions { get; set; }

    public virtual DbSet<Carnetescaneado> Carnetescaneados { get; set; }

    public virtual DbSet<Centrovacunacion> Centrovacunacions { get; set; }

    public virtual DbSet<Certificado> Certificados { get; set; }

    public virtual DbSet<Dispositivousuario> Dispositivousuarios { get; set; }

    public virtual DbSet<Esquemavacuna> Esquemavacunas { get; set; }

    public virtual DbSet<Historialvacuna> Historialvacunas { get; set; }

    public virtual DbSet<Imc> Imcs { get; set; }

    public virtual DbSet<Miembro> Miembros { get; set; }

    public virtual DbSet<Miembroalergia> Miembroalergia { get; set; }

    public virtual DbSet<Recordatorio> Recordatorios { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Vacuna> Vacunas { get; set; }

    public virtual DbSet<Vacunaencentro> Vacunaencentros { get; set; }

    public virtual DbSet<Campaniacentro> CAMPANIA_CENTRO { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDefaultSchema("VACUNAS")
            .UseCollation("USING_NLS_COMP");

        modelBuilder.Entity<Alergia>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SYS_C008371");

            entity.ToTable("ALERGIA");

            entity.HasIndex(e => e.Nombre, "SYS_C008372").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("\"VACUNAS\".\"SEQ_ALERGIA\".\"NEXTVAL\"")
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("NOMBRE");
        });

        modelBuilder.Entity<Auditoriausuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SYS_C008401");

            entity.ToTable("AUDITORIAUSUARIO");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.Campo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CAMPO");
            entity.Property(e => e.FechaCambio)
                .HasDefaultValueSql("SYSDATE ")
                .HasColumnType("DATE")
                .HasColumnName("FECHA_CAMBIO");
            entity.Property(e => e.IdUsuario)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_USUARIO");
            entity.Property(e => e.UsuarioBd)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValueSql("USER ")
                .HasColumnName("USUARIO_BD");
            entity.Property(e => e.ValorAnterior)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("VALOR_ANTERIOR");
            entity.Property(e => e.ValorNuevo)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("VALOR_NUEVO");
        });

        modelBuilder.Entity<Campaniavacunacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SYS_C008389");

            entity.ToTable("CAMPANIAVACUNACION");

            entity.HasIndex(e => e.IdCentro, "IDX_CAMP_CENTRO");

            entity.HasIndex(e => e.IdVacuna, "IDX_CAMP_VACUNA");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("\"VACUNAS\".\"SEQ_CAMPANIAVACUNACION\".\"NEXTVAL\"")
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.Activo)
                .HasColumnType("NUMBER(1)")
                .HasColumnName("ACTIVO")
                 .HasConversion<int>();
            entity.Property(e => e.Fecha)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE")
                .HasColumnName("FECHA");
            entity.Property(e => e.IdCentro)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_CENTRO");
            entity.Property(e => e.IdVacuna)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_VACUNA");
            entity.Property(e => e.Latitud)
                .HasColumnType("NUMBER")
                .HasColumnName("LATITUD");
            entity.Property(e => e.Longitud)
                .HasColumnType("NUMBER")
                .HasColumnName("LONGITUD");
            entity.Property(e => e.Lugar)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("LUGAR");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("NOMBRE");

            entity.HasOne(d => d.IdCentroNavigation).WithMany(p => p.Campaniavacunacions)
                .HasForeignKey(d => d.IdCentro)
                .HasConstraintName("FK_CAMP_CENTRO");

            entity.HasOne(d => d.IdVacunaNavigation).WithMany(p => p.Campaniavacunacions)
                .HasForeignKey(d => d.IdVacuna)
                .HasConstraintName("FK_CAMP_VACUNA");
        });

        modelBuilder.Entity<Carnetescaneado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SYS_C008385");

            entity.ToTable("CARNETESCANEADO");

            entity.HasIndex(e => e.IdMiembro, "IDX_CARNET_MIEMB");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("\"VACUNAS\".\"SEQ_CARNETESCANEADO\".\"NEXTVAL\"")
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.Fecha)
                .HasColumnType("DATE")
                .HasColumnName("FECHA");
            entity.Property(e => e.IdMiembro)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_MIEMBRO");
            entity.Property(e => e.Imagen)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("IMAGEN");
            entity.Property(e => e.NombreClinica)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("NOMBRE_CLINICA");

            entity.HasOne(d => d.IdMiembroNavigation).WithMany(p => p.Carnetescaneados)
                .HasForeignKey(d => d.IdMiembro)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CARNET_MIEMBRO");
        });

        modelBuilder.Entity<Centrovacunacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SYS_C008333");

            entity.ToTable("CENTROVACUNACION");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("\"VACUNAS\".\"SEQ_CENTRO\".\"NEXTVAL\"")
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("1 ")
                .HasColumnType("NUMBER(1)")
                .HasColumnName("ACTIVO")
                 .HasConversion<int>();
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("DIRECCION");
            entity.Property(e => e.Horario)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("HORARIO");
            entity.Property(e => e.Latitud)
                .HasColumnType("NUMBER(10,6)")
                .HasColumnName("LATITUD");
            entity.Property(e => e.Longitud)
                .HasColumnType("NUMBER(10,6)")
                .HasColumnName("LONGITUD");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("NOMBRE");
            entity.Property(e => e.Telefono)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("TELEFONO");
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("TIPO");
        });

        modelBuilder.Entity<Certificado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SYS_C008369");

            entity.ToTable("CERTIFICADO");

            entity.HasIndex(e => e.IdMiembro, "IDX_CERTIFICADO_MIEMB");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("\"VACUNAS\".\"SEQ_CERTIFICADO\".\"NEXTVAL\"")
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.CodigoQr)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("CODIGO_QR");
            entity.Property(e => e.FechaEmision)
                .HasDefaultValueSql("SYSDATE ")
                .HasColumnType("DATE")
                .HasColumnName("FECHA_EMISION");
            entity.Property(e => e.IdMiembro)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_MIEMBRO");
            entity.Property(e => e.UrlPdf)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("URL_PDF");

            entity.HasOne(d => d.IdMiembroNavigation).WithMany(p => p.Certificados)
                .HasForeignKey(d => d.IdMiembro)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CERT_MIEMBRO");
        });

        modelBuilder.Entity<Dispositivousuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SYS_C008365");

            entity.ToTable("DISPOSITIVOUSUARIO");

            entity.HasIndex(e => e.IdUsuario, "IDX_DISPOSITIVO_USR");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("\"VACUNAS\".\"SEQ_DISPOSITIVO\".\"NEXTVAL\"")
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("1 ")
                .HasColumnType("NUMBER(1)")
                .HasColumnName("ACTIVO")
                 .HasConversion<int>();
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("SYSDATE ")
                .HasColumnType("DATE")
                .HasColumnName("FECHA_REGISTRO");
            entity.Property(e => e.IdUsuario)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_USUARIO");
            entity.Property(e => e.Plataforma)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("PLATAFORMA");
            entity.Property(e => e.TokenPush)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("TOKEN_PUSH");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Dispositivousuarios)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DISPOSITIVO_USUARIO");
        });

        modelBuilder.Entity<Esquemavacuna>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SYS_C008327");

            entity.ToTable("ESQUEMAVACUNA");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("\"VACUNAS\".\"SEQ_ESQUEMA\".\"NEXTVAL\"")
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("DESCRIPCION");
            entity.Property(e => e.EdadMinimaDias)
                .HasColumnType("NUMBER")
                .HasColumnName("EDAD_MINIMA_DIAS");
            entity.Property(e => e.IdVacuna)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_VACUNA");
            entity.Property(e => e.IntervaloDias)
                .HasColumnType("NUMBER")
                .HasColumnName("INTERVALO_DIAS");
            entity.Property(e => e.NumeroDosis)
                .HasColumnType("NUMBER")
                .HasColumnName("NUMERO_DOSIS");

            entity.HasOne(d => d.IdVacunaNavigation).WithMany(p => p.Esquemavacunas)
                .HasForeignKey(d => d.IdVacuna)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ESQUEMA_VACUNA");
        });

        modelBuilder.Entity<Historialvacuna>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SYS_C008346");

            entity.ToTable("HISTORIALVACUNA");

            entity.HasIndex(e => e.IdMiembro, "IDX_HISTORIAL_MIEMBRO");

            entity.HasIndex(e => e.IdVacuna, "IDX_HISTORIAL_VACUNA");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("\"VACUNAS\".\"SEQ_HISTORIAL\".\"NEXTVAL\"")
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.DosisNumero)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("DOSIS_NUMERO");
            entity.Property(e => e.FechaAplicacion)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE")
                .HasColumnName("FECHA_APLICACION");
            entity.Property(e => e.IdCentro)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_CENTRO");
            entity.Property(e => e.IdMiembro)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID_MIEMBRO");
            entity.Property(e => e.IdVacuna)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID_VACUNA");
            entity.Property(e => e.Lote)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("LOTE");
            entity.Property(e => e.NombreMedico)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("NOMBRE_MEDICO");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("OBSERVACIONES");
            entity.Property(e => e.ProximaDosis)
                .HasColumnType("DATE")
                .HasColumnName("PROXIMA_DOSIS");

            entity.HasOne(d => d.IdCentroNavigation).WithMany(p => p.Historialvacunas)
                .HasForeignKey(d => d.IdCentro)
                .HasConstraintName("FK_HISTORIAL_CENTRO");

            entity.HasOne(d => d.IdMiembroNavigation).WithMany(p => p.Historialvacunas)
                .HasForeignKey(d => d.IdMiembro)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HISTORIAL_MIEMBRO");

            entity.HasOne(d => d.IdVacunaNavigation).WithMany(p => p.Historialvacunas)
                .HasForeignKey(d => d.IdVacuna)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HISTORIAL_VACUNA");
        });

        modelBuilder.Entity<Imc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SYS_C008382");

            entity.ToTable("IMC");

            entity.HasIndex(e => e.IdMiembro, "IDX_IMC_MIEMBRO");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("\"VACUNAS\".\"SEQ_IMC\".\"NEXTVAL\"")
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.Altura)
                .HasColumnType("NUMBER")
                .HasColumnName("ALTURA");
            entity.Property(e => e.Fecha)
                .HasColumnType("DATE")
                .HasColumnName("FECHA");
            entity.Property(e => e.IdMiembro)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_MIEMBRO");
            entity.Property(e => e.Peso)
                .HasColumnType("NUMBER")
                .HasColumnName("PESO");
            entity.Property(e => e.Resultado)
                .HasComputedColumnSql("ROUND(\"PESO\"/(\"ALTURA\"*\"ALTURA\"),2)", false)
                .HasColumnType("NUMBER")
                .HasColumnName("RESULTADO");

            entity.HasOne(d => d.IdMiembroNavigation).WithMany(p => p.Imcs)
                .HasForeignKey(d => d.IdMiembro)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IMC_MIEMBRO");
        });

        modelBuilder.Entity<Miembro>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SYS_C008317");

            entity.ToTable("MIEMBRO");

            entity.HasIndex(e => e.IdUsuario, "IDX_MIEMBRO_USUARIO");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("\"VACUNAS\".\"SEQ_MIEMBRO\".\"NEXTVAL\"")
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("1 ")
                .HasColumnType("NUMBER(1)")
                .HasColumnName("ACTIVO")
                 .HasConversion<int>();
            entity.Property(e => e.Especie)
                .HasMaxLength(50)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .HasColumnName("ESPECIE");
            entity.Property(e => e.FechaNacimiento)
                .HasColumnType("DATE")
                .HasColumnName("FECHA_NACIMIENTO");
            entity.Property(e => e.FotoUrl)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("FOTO_URL");
            entity.Property(e => e.Genero)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("GENERO");
            entity.Property(e => e.IdUsuario)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_USUARIO");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("NOMBRE");
            entity.Property(e => e.NumeroDocumento)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("NUMERO_DOCUMENTO");
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .HasColumnName("TIPO");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Miembros)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MIEMBRO_USUARIO");
        });

        modelBuilder.Entity<Miembroalergia>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SYS_C008373");

            entity.ToTable("MIEMBROALERGIA");

            entity.HasIndex(e => e.IdAlergia, "IDX_MA_ALERGIA");

            entity.HasIndex(e => e.IdMiembro, "IDX_MA_MIEMBRO");

            entity.HasIndex(e => new { e.IdMiembro, e.IdAlergia }, "UQ_MIEMBRO_ALERGIA").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("\"VACUNAS\".\"SEQ_MIEMBROALERGIA\".\"NEXTVAL\"")
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.IdAlergia)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_ALERGIA");
            entity.Property(e => e.IdMiembro)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_MIEMBRO");

            entity.HasOne(d => d.IdAlergiaNavigation).WithMany(p => p.Miembroalergia)
                .HasForeignKey(d => d.IdAlergia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MA_ALERGIA");

            entity.HasOne(d => d.IdMiembroNavigation).WithMany(p => p.Miembroalergia)
                .HasForeignKey(d => d.IdMiembro)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MA_MIEMBRO");
        });

        modelBuilder.Entity<Recordatorio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SYS_C008356");

            entity.ToTable("RECORDATORIO");

            entity.HasIndex(e => e.IdHistorial, "IDX_RECORDATORIO_HIST");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("\"VACUNAS\".\"SEQ_RECORDATORIO\".\"NEXTVAL\"")
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValueSql("'pendiente' ")
                .HasColumnName("ESTADO");
            entity.Property(e => e.FechaRecordatorio)
                .HasColumnType("DATE")
                .HasColumnName("FECHA_RECORDATORIO");
            entity.Property(e => e.IdHistorial)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_HISTORIAL");
            entity.Property(e => e.Mensaje)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("MENSAJE");
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValueSql("'push' ")
                .HasColumnName("TIPO");

            entity.HasOne(d => d.IdHistorialNavigation).WithMany(p => p.Recordatorios)
                .HasForeignKey(d => d.IdHistorial)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RECORDATORIO_HISTORIAL");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SYS_C008309");

            entity.ToTable("USUARIO");

            entity.HasIndex(e => e.Correo, "SYS_C008310").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("\"VACUNAS\".\"SEQ_USUARIO\".\"NEXTVAL\"")
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.Activo)
                .IsRequired()
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("1 ")
                .HasColumnType("NUMBER(1)")
                .HasColumnName("ACTIVO")
                 .HasConversion<int>();
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .HasColumnName("CORREO");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("SYSDATE ")
                .HasColumnType("DATE")
                .HasColumnName("FECHA_REGISTRO");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("NOMBRE");
            entity.Property(e => e.Password)
                .HasMaxLength(200)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .HasColumnName("PASSWORD");
            entity.Property(e => e.Rol)
                .HasMaxLength(20)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("'usuario' ")
                .HasColumnName("ROL");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("TELEFONO");
        });

        modelBuilder.Entity<Vacuna>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SYS_C008324");

            entity.ToTable("VACUNA");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("\"VACUNAS\".\"SEQ_VACUNA\".\"NEXTVAL\"")
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("1 ")
                .HasColumnType("NUMBER(1)")
                .HasColumnName("ACTIVO")
                 .HasConversion<int>();
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("DESCRIPCION");
            entity.Property(e => e.Fabricante)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("FABRICANTE");
            entity.Property(e => e.ImagenUrl)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("IMAGEN_URL");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("NOMBRE");
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("TIPO");
        });

        modelBuilder.Entity<Vacunaencentro>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SYS_C008338");

            entity.ToTable("VACUNAENCENTRO");

            entity.HasIndex(e => e.IdCentro, "IDX_VC_CENTRO");

            entity.HasIndex(e => e.IdVacuna, "IDX_VC_VACUNA");

            entity.HasIndex(e => new { e.IdCentro, e.IdVacuna }, "UQ_VC_CENTRO_VACUNA").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("\"VACUNAS\".\"SEQ_VACUNA_CENTRO\".\"NEXTVAL\"")
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.Disponible)
                .IsRequired()
                .HasDefaultValueSql("1")
                .HasColumnType("NUMBER(1)")
                .HasColumnName("DISPONIBLE")
                .HasConversion<int>();
                   
            entity.Property(e => e.IdCentro)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_CENTRO");
            entity.Property(e => e.IdVacuna)
                .HasColumnType("NUMBER")
                .HasColumnName("ID_VACUNA");
            entity.Property(e => e.Precio)
                .HasColumnType("NUMBER(10,2)")
                .HasColumnName("PRECIO");

            entity.HasOne(d => d.IdCentroNavigation).WithMany(p => p.Vacunaencentros)
                .HasForeignKey(d => d.IdCentro)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VC_CENTRO");

            entity.HasOne(d => d.IdVacunaNavigation).WithMany(p => p.Vacunaencentros)
                .HasForeignKey(d => d.IdVacuna)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VC_VACUNA");
        });

        modelBuilder.Entity<Campaniacentro>(entity =>
        {
            entity.ToTable("CAMPANIA_CENTRO");

            entity.HasKey(e => e.Id).HasName("e.CampaniaId, e.CentroId");

            entity.Property(e => e.CampaniaId)
               .HasColumnType("NUMBER")
               .HasColumnName("CAMPANIA_ID");

            entity.Property(e => e.CentroId)
                .HasColumnType("NUMBER")
                .HasColumnName("CENTRO_ID");

            entity.HasOne(d => d.IdCampaniaNavigation)
                .WithMany(p => p.Campaniacentros)
                .HasForeignKey(d => d.CampaniaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CC_CAMPANIA");

            entity.HasOne(d => d.IdCentroNavigation)
                .WithMany(p => p.Campaniacentros)
                .HasForeignKey(d => d.CentroId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CC_CENTRO");
        });

        modelBuilder.HasSequence("SEQ_ALERGIA");
        modelBuilder.HasSequence("SEQ_CAMPANIAVACUNACION");
        modelBuilder.HasSequence("SEQ_CARNETESCANEADO");
        modelBuilder.HasSequence("SEQ_CENTRO");
        modelBuilder.HasSequence("SEQ_CERTIFICADO");
        modelBuilder.HasSequence("SEQ_DISPOSITIVO");
        modelBuilder.HasSequence("SEQ_ESQUEMA");
        modelBuilder.HasSequence("SEQ_HISTORIAL");
        modelBuilder.HasSequence("SEQ_IMC");
        modelBuilder.HasSequence("SEQ_MIEMBRO");
        modelBuilder.HasSequence("SEQ_MIEMBROALERGIA");
        modelBuilder.HasSequence("SEQ_RECORDATORIO");
        modelBuilder.HasSequence("SEQ_USUARIO");
        modelBuilder.HasSequence("SEQ_VACUNA");
        modelBuilder.HasSequence("SEQ_VACUNA_CENTRO");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
