using System;
using System.Configuration;
using analyticsLibrary.library;
using System.Diagnostics;


namespace analyticsLibrary.dbObjects
{
    internal static class dbConnection
    {
        public enum loginType { server, dsn, tns }
        internal static void primeLogin (login dbLogin, loginType type){
            Console.WriteLine("{0} login information set for the {1} connection.",
                type == loginType.server ? dbLogin.domain : dbLogin.dsn, type.ToString());
        }
        
        internal static login primeLogin(string server, string domain, string userId, string password)
        {
            return new login()
            {
                server   = server,
                domain   = domain,
                userId   = userId,
                password = password,
            };
        }

        internal static login primeLogin(string dsn, string userId, string password)
        {
            return new login()
            {
                dsn      = dsn,
                userId   = userId,
                password = password,
            };
        }

        internal static login primeLogin(string server, string serviceName, string userId, string password, int port)
        {
            return new login()
            {
                server      = server,
                port        = port,
                serviceName = serviceName,
                userId      = userId,
                password    = password,
            };
        }

        
        internal static login getLogin(ref login sessionLogin, string name, string server, string domain)
        {
            login dbLogin = getLogin(ref sessionLogin, () => userNamePassword.getUserNamePasswordServer(name, server, domain));
            return dbLogin;
        }

        internal static login getLoginDsn(ref login sessionLogin, string name, string dsn)
        {
            login dbLogin = getLogin(ref sessionLogin, () => userNamePassword.getUserNamePasswordDsn(name, dsn));
            return dbLogin;
        }

        internal static login getLoginOracle(ref login sessionLogin, string name, string server, string service, int port)
        {
            login dbLogin = getLogin(ref sessionLogin, () => userNamePassword.getUserNamePasswordOracle(name, server, service, port));
            return dbLogin;
        }

        private static login getLogin(ref login sessionLogin, Func<login> loginMethod)
        {
            login dbLogin;
            if (sessionLogin == null)
            {
                dbLogin = loginMethod();
                if (dbLogin.sessionPersist) sessionLogin = dbLogin;
            }
            else
            {
                dbLogin = sessionLogin;
            }
            return dbLogin;
        }
    }
}
