using Microsoft.EntityFrameworkCore;
using PortalSugestao.Domain.Entities;

namespace PortalSugestao.Infrastructure.Data;

public class PortalSugestaoDbContext : DbContext
{
    public PortalSugestaoDbContext(DbContextOptions<PortalSugestaoDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Sugestao> Sugestoes => Set<Sugestao>();
    public DbSet<Voto> Votos => Set<Voto>();
    public DbSet<Comentario> Comentarios => Set<Comentario>();
    public DbSet<NotificacaoLog> NotificacaoLogs => Set<NotificacaoLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.ErpUserId).IsUnique();
            entity.Property(u => u.Nome).HasMaxLength(200).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.Property(u => u.Empresa).HasMaxLength(200);
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasIndex(c => c.Nome).IsUnique();
            entity.Property(c => c.Nome).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Sugestao>(entity =>
        {
            entity.Property(s => s.Titulo).HasMaxLength(200).IsRequired();
            entity.Property(s => s.Descricao).HasMaxLength(4000).IsRequired();
            entity.Property(s => s.ResultadoEsperado).HasMaxLength(2000).IsRequired();

            entity.HasOne(s => s.Categoria)
                .WithMany(c => c.Sugestoes)
                .HasForeignKey(s => s.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Autor)
                .WithMany(u => u.Sugestoes)
                .HasForeignKey(s => s.AutorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Moderador)
                .WithMany()
                .HasForeignKey(s => s.ModeradorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Voto>(entity =>
        {
            // Um usuário só pode ter um voto ativo por sugestão (regra de negócio 7.2).
            entity.HasIndex(v => new { v.SugestaoId, v.UsuarioId }).IsUnique();

            entity.HasOne(v => v.Sugestao)
                .WithMany(s => s.Votos)
                .HasForeignKey(v => v.SugestaoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(v => v.Usuario)
                .WithMany(u => u.Votos)
                .HasForeignKey(v => v.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Comentario>(entity =>
        {
            entity.Property(c => c.Texto).HasMaxLength(2000).IsRequired();

            entity.HasOne(c => c.Sugestao)
                .WithMany(s => s.Comentarios)
                .HasForeignKey(c => c.SugestaoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Usuario)
                .WithMany(u => u.Comentarios)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<NotificacaoLog>(entity =>
        {
            entity.HasOne(n => n.Usuario)
                .WithMany()
                .HasForeignKey(n => n.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(n => n.Sugestao)
                .WithMany()
                .HasForeignKey(n => n.SugestaoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
