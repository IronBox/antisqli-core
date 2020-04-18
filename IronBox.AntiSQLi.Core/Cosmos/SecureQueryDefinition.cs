using IronBox.AntiSQLi.Core.Common;
using IronBox.AntiSQLi.Core.Models;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Documents = Microsoft.Azure.Documents;

namespace IronBox.AntiSQLi.Core.Cosmos
{
    public static class SecureQueryDefinition
    {

        //---------------------------------------------------------------------
        /// <summary>
        ///     Creates and parameterizes an instance of Microsoft.Azure.Cosmos.QueryDefinition 
        ///     using the given queryText and args to mitigate risk from SQLi attacks
        /// </summary>
        /// <param name="queryText">Query text to use</param>
        /// <param name="args">Arguments to map and parameterize</param>
        /// <returns>
        ///     Returns a QueryDefinition instance that is been instantiated
        ///     with the given queryText and parameterized
        /// </returns>
        //---------------------------------------------------------------------
        public static QueryDefinition Create(String queryText, params Object[] args)
        {
            // Input validation
            AntiSQLiCommon.ValidateQueryTextAndArgsThrowEx(queryText, args);

            // Parameterize given arguments
            IEnumerable<Documents.SqlParameter> parsedParameters;
            if (!AntiSQLiCommon.TryConvertObjectsToDbParameterCollection(out parsedParameters, args))
            {
                throw new AntiSQLiException(Resources.AntiSQLiResource.Error_UnableToParseArguments);
            }

            // If no parameters parsed, then stop execution, may not be safe to continue
            if (parsedParameters.Count() == 0)
            {
                throw new AntiSQLiException(Resources.AntiSQLiResource.Error_NoParametersParsed);
            }

            // Substitute the queryText formatters (i.e. {0} .. {N}) with the
            // proper parameter names (i.e., @AntiSQLiParam1 ... @AntiSQLiParamN, 
            // where N is the number of elements in the args params
            String parameterizedQueryText;
            if (!AntiSQLiCommon.TryParameterizeQueryText(queryText, parsedParameters, out parameterizedQueryText))
            {
                throw new AntiSQLiException(Resources.AntiSQLiResource.Error_UnableToParameterizeQuery);
            }

            // Create final QueryDefinition object and add parameters
            QueryDefinition finalQueryDefinition = new QueryDefinition(parameterizedQueryText);
            foreach (var p in parsedParameters)
            {
                finalQueryDefinition = finalQueryDefinition.WithParameter(p.Name, p.Value);
            }
            return (finalQueryDefinition);
        }
    }
}
