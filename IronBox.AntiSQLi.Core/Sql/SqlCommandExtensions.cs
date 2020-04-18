using IronBox.AntiSQLi.Core.Common;
using System;

namespace IronBox.AntiSQLi.Core.Sql
{
    public static class SqlCommandExtensions
    {

        //---------------------------------------------------------------------
        /// <summary>
        ///     Extension to Microsoft.Data.SqlClient to load query with untrusted
        ///     data provided in args parameters safely to mitigate the risk from
        ///     SQL injection attacks
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="queryText"></param>
        /// <param name="queryTextArgs"></param>
        /// <exception cref="IronBox.AntiSQLi.Models.AntiSQLiException">
        ///     Thrown on loading errors
        /// </exception>
        //---------------------------------------------------------------------
        public static void LoadQuerySecure(this Microsoft.Data.SqlClient.SqlCommand sqlCommandObj, String queryText, params Object[] queryTextArgs)
        {
            AntiSQLiCommon.ParameterizeAndLoadQuery<Microsoft.Data.SqlClient.SqlParameter>(sqlCommandObj, queryText, queryTextArgs);   
        }


        //---------------------------------------------------------------------
        /// <summary>
        ///     Extension to System.Data.SqlClient to load query with untrusted
        ///     data provided in args parameters safely to mitigate the risk from
        ///     SQL injection attacks
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="queryText">Query string to execute</param>
        /// <param name="queryTextArgs">Parameters</param>
        //---------------------------------------------------------------------
        public static void LoadQuerySecure(this System.Data.SqlClient.SqlCommand sqlCommandObj, String queryText, params Object[] queryTextArgs)
        {
            AntiSQLiCommon.ParameterizeAndLoadQuery<System.Data.SqlClient.SqlParameter>(sqlCommandObj, queryText, queryTextArgs);
        }


        
        
    }
}
