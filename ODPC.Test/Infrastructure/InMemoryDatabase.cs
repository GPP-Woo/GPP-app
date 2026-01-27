using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ODPC.Data;

namespace ODPC.Test.Infrastructure
{
    public static class InMemoryDatabase
    {
        public static OdpcDbContext GetDbContext() => new InMemoryOdpcDbContext(new SqliteConnection("DataSource=:memory:"));

        private sealed class InMemoryOdpcDbContext : OdpcDbContext
        {
            private readonly SqliteConnection _sqliteConnection;

            public InMemoryOdpcDbContext(SqliteConnection sqliteConnection) : base(new DbContextOptionsBuilder<OdpcDbContext>().UseSqlite(sqliteConnection).Options)
            {
                _sqliteConnection = sqliteConnection;
                _sqliteConnection.Open();
                Database.EnsureCreated();
            }

            public override void Dispose()
            {
                _sqliteConnection.Close();
                _sqliteConnection.Dispose();
                base.Dispose();
            }

            public override async ValueTask DisposeAsync()
            {
                await _sqliteConnection.CloseAsync();
                await _sqliteConnection.DisposeAsync();
                await base.DisposeAsync();
            }
        }
    }
}
