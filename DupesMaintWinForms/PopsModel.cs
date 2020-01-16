namespace DupesMaintWinForms
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Data.SqlClient;

    public partial class PopsModel : DbContext
    {
        public PopsModel()
            : base("name=DupesModel")
        {
        }

        public virtual DbSet<CheckSum> CheckSums { get; set; }
        public virtual DbSet<CheckSumDup> CheckSumDups { get; set; }
        public virtual DbSet<DupesAction> DupesActions { get; set; }


        // custom method to insert a new DupesAction row using the stored procedure spDupesAction_ins
        public void DupesAction_ins(DupesAction dupesAction)
        {
            var theFileName         = new SqlParameter("@TheFileName", dupesAction.TheFileName);
            var duplicateOf         = new SqlParameter("@DuplicateOf", dupesAction.DuplicateOf);
            var sHA                 = new SqlParameter("@SHA", dupesAction.SHA);
            var fileExt             = new SqlParameter("@FileExt", dupesAction.FileExt);
            var fileSize            = new SqlParameter("@FileSize", dupesAction.FileSize);
            var fileCreateDt        = new SqlParameter("@FileCreateDt", dupesAction.FileCreateDt);
            var oneDriveRemoved     = new SqlParameter("@OneDriveRemoved", dupesAction.OneDriveRemoved);
            var googlePhotosRemoved = new SqlParameter("@googlePhotosRemoved", dupesAction.GooglePhotosRemoved);

            this.Database.ExecuteSqlCommand("exec spDupesAction_ins @TheFileName, @DuplicateOf, @SHA, @FileExt, @FileSize, @FileCreateDt, @OneDriveRemoved, @googlePhotosRemoved",
                                                                    theFileName, duplicateOf, sHA, fileExt, fileSize, fileCreateDt, oneDriveRemoved, googlePhotosRemoved);
        }


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
