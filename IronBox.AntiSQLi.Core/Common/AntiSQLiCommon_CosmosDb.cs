using Microsoft.Azure.Documents;
using IronBox.AntiSQLi.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Documents = Microsoft.Azure.Documents;
using Microsoft.Azure.Cosmos;

namespace IronBox.AntiSQLi.Core.Common
{
    internal static partial class AntiSQLiCommon
    {

        #region querydefinition

        public static void ParameterizeAndLoadQuery(QueryDefinition queryDefinition, String queryText, params Object[] queryTextArgs)
        {
            // Input validation
            ValidateQueryTextAndArgsThrowEx(queryText, queryTextArgs);

            // Parse arguments into db parameter collection
            IEnumerable<Documents.SqlParameter> parsedParameters;
            if (!TryConvertObjectsToDbParameterCollection(out parsedParameters, queryTextArgs))
            {
                throw new AntiSQLiException("Unable to parse parameters");
            }

            // Substitute the queryText formatters (i.e. {0} .. {N}) with the
            // proper parameter names (i.e., @AntiSQLiParam1 ... @AntiSQLiParamN, 
            // where N is the number of elements in the args params
            String parameterizedQueryText;
            if (!TryParameterizeQueryText(queryText, parsedParameters, out parameterizedQueryText))
            {
                throw new AntiSQLiException("Unable to parameterize query text");
            }

            

        }

        #endregion



        #region sqlqueryspec

        public static void ParameterizeAndLoadQuery(SqlQuerySpec sqsObj, String queryText, params Object[] queryTextArgs) 
        {
            // Input validation
            ValidateQueryTextAndArgsThrowEx(queryText, queryTextArgs);

            // Parse arguments into parameter collection
            IEnumerable<Documents.SqlParameter> parsedParameters;
            if (!TryConvertObjectsToDbParameterCollection(out parsedParameters, queryTextArgs))
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
            if (!TryParameterizeQueryText(queryText, parsedParameters, out parameterizedQueryText))
            {
                throw new AntiSQLiException(Resources.AntiSQLiResource.Error_UnableToParameterizeQuery);
            }

            // Assign parameterized query and parameters
            sqsObj.QueryText = parameterizedQueryText;
            sqsObj.Parameters.Clear();
            parsedParameters.ToList().ForEach(x => sqsObj.Parameters.Add(x));
        }

        
        public static bool TryConvertObjectsToDbParameterCollection(
            out IEnumerable<Documents.SqlParameter> parameterCollection,
            params Object[] queryTextArgs) 
        {

            try
            {
                List<Documents.SqlParameter> results = new List<Documents.SqlParameter>();

                // Iterate through each arg and convert to SqlParameter
                foreach (var o in queryTextArgs)
                {
                    // Unlike in relation SQL databases, SqlParameters in DocumentDb/Cosmos do 
                    // not have types associated with them
                    // https://docs.microsoft.com/fr-fr/dotnet/api/microsoft.azure.documents.sqlparameter?view=azure-dotnet
                    Documents.SqlParameter p = new SqlParameter("@AntiSQLiParam" + results.Count(), o);
                    results.Add(p);
                }
                parameterCollection = results;
                return true;
            }
            catch (Exception)
            {
                parameterCollection = null;
                return false;
            }
        }


        public static bool TryParameterizeQueryText(String queryText,
            IEnumerable<Documents.SqlParameter> parameterCollection, out String parameterizedQueryText)
        {
            try
            {
                // Build a list of the parameter names (example: @p1, @p2, ...
                // Certainly can use .Select but documentation doesn't specify anything
                // about keeping items in order, can't rely on implementation to preserve this
                List<String> parameterNames = new List<string>();
                parameterCollection.ToList().ForEach(x => parameterNames.Add(x.Name));

                // Apply the formatting, if there is a mismatch in the number
                // of formatters and actual parameters, String.Format will 
                // toss an exception
                parameterizedQueryText = String.Format(queryText, parameterNames.ToArray());
                return (true);
            }
            catch (Exception)
            {
                parameterizedQueryText = null;
                return (false);
            }
        }
        
    }

    #endregion

}
