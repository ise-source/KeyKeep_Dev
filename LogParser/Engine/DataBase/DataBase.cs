using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.SQLite;
using System.Data.SQLite.EF6;
using System.IO;
using System.Threading.Tasks;
using LogParser.Engine.Classes;
using System.Collections.ObjectModel;

namespace LogParser.Engine.DataBase
{
    public class DataBaseContext : DbContext
    {
        public DataBaseContext(string connectionString) : base(CreateConnection(connectionString), true)
        {
            if (!Database.Exists())
            {
                Database.SetInitializer(new CreateDatabaseIfNotExists<DataBaseContext>());
            }
        }
        private static DbConnection CreateConnection(string connectionString) => new SQLiteConnection(connectionString);

        public DbSet<Data> Data { get; set; }
    }

    public class Configuration : DbConfiguration
    {
        public Configuration()
        {
            SetProviderFactory("System.Data.SQLite", SQLiteFactory.Instance);
            SetProviderFactory("System.Data.SQLite.EF6", SQLiteFactory.Instance);

            var ProviderServices = (DbProviderServices)SQLiteProviderFactory.Instance.GetService(typeof(DbProviderServices));

            SetProviderServices("System.Data.SQLite", ProviderServices);
            SetProviderServices("System.Data.SQLite.EF6", ProviderServices);
        }
    }
    public class DataManager
    {
        private static DataBaseContext context;
        private static object _object = new object();
        private static readonly string ConnectionString = $"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BD.db")}";

        public static DataBaseContext GetContext()
        {
            lock (_object)
            {
                if (context == null)
                    context = new DataBaseContext(ConnectionString);

                return context;
            }
        }

        public static async Task<ObservableCollection<Data>> LoaDataAsync()
        {
            return new ObservableCollection<Data>(await GetContext().Data.ToListAsync());
        }

        public static void AddData(Data data)
        {
            context.Data.Add(data);
            Handler.AllData.Add(data);
        }

        public static void RemoveData(Data data)
        {
            context.Data.Remove(data);
            Handler.AllData.Remove(data);
        }

        public static async Task SaveBD()
        {
            try
            {
                await GetContext().SaveChangesAsync();
            }
            catch { Notify.Send("Сохранение неудачно!", TypeNotify.Error); }
        }
    }

    public class Data
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public bool IsFavorit { get; set; } = false;
        public System.DateTime DateCreate { get; set; }
        public Nullable<System.DateTime> DateUpdate { get; set; }
        public string WebSite { get; set; }
        public string Icon { get; set; }
    }
}
