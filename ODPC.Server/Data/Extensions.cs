using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ODPC.Data
{
    public static class Extensions
    {
        public static bool IsDuplicateException(this DbUpdateException ex)
        {
            return ex.InnerException switch
            {
                PostgresException pgEx => pgEx.SqlState == "23505",
                _ => ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
            };
        }
    }
}
