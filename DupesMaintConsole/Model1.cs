namespace DupesMaintConsole
{
    using System;
    using System.Data.Entity;
    using System.Data.SqlClient;

    public partial class Model1 : DbContext
    {
        public Model1()
            : base("name=PopsConnection")
        {
        }

        public virtual DbSet<CheckSum> CheckSums { get; set; }
        public virtual DbSet<CheckSumDup> CheckSumDups { get; set; }
        public virtual DbSet<DupesAction> DupesActions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CheckSum>()
                .Property(e => e.SHA)
                .IsUnicode(false);

            modelBuilder.Entity<CheckSum>()
                .Property(e => e.Folder)
                .IsUnicode(false);

            modelBuilder.Entity<CheckSum>()
                .Property(e => e.TheFileName)
                .IsUnicode(false);

            modelBuilder.Entity<CheckSum>()
                .Property(e => e.FileExt)
                .IsUnicode(false);

            modelBuilder.Entity<CheckSum>()
                .Property(e => e.Notes)
                .IsUnicode(false);

            modelBuilder.Entity<CheckSumDup>()
                .Property(e => e.SHA)
                .IsUnicode(false);

            modelBuilder.Entity<CheckSumDup>()
                .Property(e => e.ToDelete)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<DupesAction>()
                .Property(e => e.TheFileName)
                .IsUnicode(false);

            modelBuilder.Entity<DupesAction>()
                .Property(e => e.DuplicateOf)
                .IsUnicode(false);

            modelBuilder.Entity<DupesAction>()
                .Property(e => e.SHA)
                .IsUnicode(false);

            modelBuilder.Entity<DupesAction>()
                .Property(e => e.FileExt)
                .IsUnicode(false);

            modelBuilder.Entity<DupesAction>()
                .Property(e => e.OneDriveRemoved)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<DupesAction>()
                .Property(e => e.GooglePhotosRemoved)
                .IsFixedLength()
                .IsUnicode(false);
        }



    }
}
