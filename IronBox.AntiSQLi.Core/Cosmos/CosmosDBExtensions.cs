using IronBox.AntiSQLi.Core.Common;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Cosmos;
using System;




namespace IronBox.AntiSQLi.Core.Cosmos
{
    public static class CosmosDBExtensions
    {

        /// <summary>
        ///     For Microsoft.Azure.Documents.SqlQuerySpec https://docs.microsoft.com/fr-fr/dotnet/api/microsoft.azure.documents.sqlqueryspec?view=azure-dotnet
        /// </summary>
        /// <param name="sqs"></param>
        /// <param name="queryText"></param>
        /// <param name="queryTextArgs"></param>
        public static void LoadQuerySecure(this SqlQuerySpec sqs, String queryText, params Object[] queryTextArgs)
        {
            AntiSQLiCommon.ParameterizeAndLoadQuery(sqs, queryText, queryTextArgs);
        }

    }
}
