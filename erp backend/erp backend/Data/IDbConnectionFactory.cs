using System.Data;

namespace erp_backend.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
