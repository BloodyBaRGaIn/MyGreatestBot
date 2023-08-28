using Microsoft.Data.SqlClient;

namespace MyGreatestBot.Sql
{
    internal sealed class ConnectionStringBuilder
    {
        private readonly SqlConnectionStringBuilder builder = new()
        {
            IntegratedSecurity = true,
            PersistSecurityInfo = false,
            Pooling = false,
            MultipleActiveResultSets = false,
            ConnectTimeout = 5,
            Encrypt = false,
            TrustServerCertificate = true,
            CommandTimeout = 0,
            ConnectRetryCount = 1,
            ConnectRetryInterval = 5,
        };

        internal ConnectionStringBuilder(string data_source)
        {
            builder.DataSource = data_source;
        }

        internal ConnectionStringBuilder(string data_source, string catalog) : this(data_source)
        {
            builder.InitialCatalog = catalog;
        }

        internal string Build()
        {
            return ToString();
        }

        public override string ToString()
        {
            return builder.ToString();
        }
    }
}
