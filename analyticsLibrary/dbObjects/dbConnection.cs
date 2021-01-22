using System;
using System.Configuration;
using analyticsLibrary.library;
using System.Diagnostics;


namespace analyticsLibrary.dbObjects
{
    internal static class dbConnection
    {
        public enum loginType { server, dsn, tns }
        internal static void primeLogin(login dbLogin, loginType type)
        {
            Console.WriteLine("{0} login information set for the {1} connection.",
                type == loginType.server ? dbLogin.domain : dbLogin.dsn, type.ToString());
        }

        internal static login primeLogin(string server, string domain, string userId, string password)
        {
            return new login()
            {
                server = server,
                domain = domain,
                userId = userId,
                password = password,
            };
        }

        internal static login primeLogin(string dsn, string userId, string password)
        {
            return new login()
            {
                dsn = dsn,
                userId = userId,
                password = password,
            };
        }

        internal static login primeLogin(string server, string serviceName, string userId, string password, int port)
        {
            return new login()
            {
                server = server,
                port = port,
                serviceName = serviceName,
                userId = userId,
                password = password,
            };
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
