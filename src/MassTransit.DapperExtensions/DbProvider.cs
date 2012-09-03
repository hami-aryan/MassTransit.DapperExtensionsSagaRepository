using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace MassTransit.DESagaRepository
{
    public interface IDbProvider
    {
        IDbConnection Open();
    }
    public class DbProvider
        :IDbProvider
    {
        private string connectionString;
        public DbProvider(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IDbConnection Open()
        {
            var con = new SqlConnection(connectionString);
            con.Open();
            return con;
        }
    }
}
