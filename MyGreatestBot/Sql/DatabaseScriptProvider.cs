using System.IO;

namespace DicordNET.Sql
{
    internal sealed class DatabaseScriptProvider
    {
        internal string LocalStoreDirectory { get; init; } = Directory.GetCurrentDirectory();
        internal string DatabaseName { get; init; } = string.Empty;

        internal DatabaseScriptProvider()
        {

        }

        internal DatabaseScriptProvider(string localStoreDirectory, string databaseName) : this()
        {
            LocalStoreDirectory = localStoreDirectory;
            DatabaseName = databaseName;
        }

        internal string GetDatabaseScript()
        {
            string trimmed_path = LocalStoreDirectory.Replace('/', '\\').TrimEnd('\\');

            if (!Directory.Exists(trimmed_path))
            {
                _ = Directory.CreateDirectory(trimmed_path);
            }

            string database_file_path = $"{trimmed_path}\\{DatabaseName}.mdf";
            string log_file_path = $"{trimmed_path}\\{DatabaseName}_log.ldf";

            if (File.Exists(database_file_path))
            {
                File.Delete(database_file_path);
            }

            if (File.Exists(log_file_path))
            {
                File.Delete(log_file_path);
            }

            string script = $"""
                CREATE DATABASE [{DatabaseName}]
                 CONTAINMENT = NONE
                 ON  PRIMARY 
                ( NAME = N'{DatabaseName}', FILENAME = N'{database_file_path}' , SIZE = 8192KB , FILEGROWTH = 65536KB )
                 LOG ON 
                ( NAME = N'{DatabaseName}_log', FILENAME = N'{log_file_path}' , SIZE = 8192KB , FILEGROWTH = 65536KB )
                 WITH LEDGER = OFF
                GO
                ALTER DATABASE [{DatabaseName}] SET COMPATIBILITY_LEVEL = 160
                GO
                ALTER DATABASE [{DatabaseName}] SET ANSI_NULL_DEFAULT OFF 
                GO
                ALTER DATABASE [{DatabaseName}] SET ANSI_NULLS OFF 
                GO
                ALTER DATABASE [{DatabaseName}] SET ANSI_PADDING OFF 
                GO
                ALTER DATABASE [{DatabaseName}] SET ANSI_WARNINGS OFF 
                GO
                ALTER DATABASE [{DatabaseName}] SET ARITHABORT OFF 
                GO
                ALTER DATABASE [{DatabaseName}] SET AUTO_CLOSE OFF 
                GO
                ALTER DATABASE [{DatabaseName}] SET AUTO_SHRINK OFF 
                GO
                ALTER DATABASE [{DatabaseName}] SET AUTO_CREATE_STATISTICS ON(INCREMENTAL = OFF)
                GO
                ALTER DATABASE [{DatabaseName}] SET AUTO_UPDATE_STATISTICS ON 
                GO
                ALTER DATABASE [{DatabaseName}] SET CURSOR_CLOSE_ON_COMMIT OFF 
                GO
                ALTER DATABASE [{DatabaseName}] SET CURSOR_DEFAULT  GLOBAL 
                GO
                ALTER DATABASE [{DatabaseName}] SET CONCAT_NULL_YIELDS_NULL OFF 
                GO
                ALTER DATABASE [{DatabaseName}] SET NUMERIC_ROUNDABORT OFF 
                GO
                ALTER DATABASE [{DatabaseName}] SET QUOTED_IDENTIFIER OFF 
                GO
                ALTER DATABASE [{DatabaseName}] SET RECURSIVE_TRIGGERS OFF 
                GO
                ALTER DATABASE [{DatabaseName}] SET  DISABLE_BROKER 
                GO
                ALTER DATABASE [{DatabaseName}] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
                GO
                ALTER DATABASE [{DatabaseName}] SET DATE_CORRELATION_OPTIMIZATION OFF 
                GO
                ALTER DATABASE [{DatabaseName}] SET PARAMETERIZATION SIMPLE 
                GO
                ALTER DATABASE [{DatabaseName}] SET READ_COMMITTED_SNAPSHOT OFF 
                GO
                ALTER DATABASE [{DatabaseName}] SET  READ_WRITE 
                GO
                ALTER DATABASE [{DatabaseName}] SET RECOVERY FULL 
                GO
                ALTER DATABASE [{DatabaseName}] SET  MULTI_USER 
                GO
                ALTER DATABASE [{DatabaseName}] SET PAGE_VERIFY CHECKSUM  
                GO
                ALTER DATABASE [{DatabaseName}] SET TARGET_RECOVERY_TIME = 60 SECONDS 
                GO
                ALTER DATABASE [{DatabaseName}] SET DELAYED_DURABILITY = DISABLED 
                GO
                USE [{DatabaseName}]
                GO
                ALTER DATABASE SCOPED CONFIGURATION SET LEGACY_CARDINALITY_ESTIMATION = Off;
                GO
                ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET LEGACY_CARDINALITY_ESTIMATION = Primary;
                GO
                ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 0;
                GO
                ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET MAXDOP = PRIMARY;
                GO
                ALTER DATABASE SCOPED CONFIGURATION SET PARAMETER_SNIFFING = On;
                GO
                ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET PARAMETER_SNIFFING = Primary;
                GO
                ALTER DATABASE SCOPED CONFIGURATION SET QUERY_OPTIMIZER_HOTFIXES = Off;
                GO
                ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET QUERY_OPTIMIZER_HOTFIXES = Primary;
                GO
                USE [{DatabaseName}]
                GO
                IF NOT EXISTS (SELECT name FROM sys.filegroups WHERE is_default=1 AND name = N'PRIMARY') ALTER DATABASE [{DatabaseName}] MODIFY FILEGROUP [PRIMARY] DEFAULT
                GO

                """
            ;

            return script;
        }
    }
}
