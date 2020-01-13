using SQLite.CodeFirst;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfConnectClient.DataBase
{
    public class Initializer : SqliteDropCreateDatabaseWhenModelChanges<DBInjector>//SqliteCreateDatabaseIfNotExists<DBInjector>
    {
        public Initializer(DbModelBuilder modelBuilder) : base(modelBuilder)
        {


        }

        protected override void Seed(DBInjector context)
        {

        }
    }
}
