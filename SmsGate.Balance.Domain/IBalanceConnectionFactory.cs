using ServiceStack.Data;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.PostgreSQL;

namespace SmsGate.Balance.Domain
{
    public interface IBalanceConnectionFactory : IDbConnectionFactory
    {
        
    }
    
    public class BalanceConnectionFactory : OrmLiteConnectionFactory, IBalanceConnectionFactory
    {
        public BalanceConnectionFactory(string s) : base(s)
        {
        }
        
        public BalanceConnectionFactory(string s, PostgreSqlDialectProvider provider) : base(s, provider)
        {
        }
    }
}