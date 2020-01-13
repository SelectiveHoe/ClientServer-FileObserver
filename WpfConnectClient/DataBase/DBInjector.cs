using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace WpfConnectClient.DataBase
{
    public class DBInjector : DbContext 
    {
        public DbSet<ServerItem> Servers { get; set; }
        public DbSet<DBDownItem> DBDownItems { get; set; }

        public DBInjector() : base("conStr")
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DBDownItem>()
                .HasRequired<ServerItem>(s => s.ServerItem)
                .WithMany(g => g.DBDownItems)
                .HasForeignKey<int>(s => s.ServerItemId);


            Database.SetInitializer(new Initializer(modelBuilder));
            //Database.SetInitializer<DBInjector>(null);
            //base.OnModelCreating(modelBuilder);


        }
    }
}
