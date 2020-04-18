using IronBox.AntiSQLi.Core.Models;
using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace IronBox.AntiSQLi.Core.Common
{
    internal static partial class AntiSQLiCommon
    {

        

        public static void ParameterizeAndLoadQuery<DbParameterType>(DbCommand dbCommand, String queryText, params Object[] queryTextArgs) where DbParameterType : DbParameter
        {
            // Input validation
            AntiSQLiCommon.ValidateQueryTextAndArgsThrowEx(queryText, queryTextArgs);

            // Parse arguments, and do substitution
            IEnumerable<DbParameterType> parsedParameters;
            if (!TryConvertObjectsToDbParameterCollection<DbParameterType>(out parsedParameters, queryTextArgs))
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
            if (!TryParameterizeQueryText<DbParameterType>(queryText, parsedParameters, out parameterizedQueryText))
            {
                throw new AntiSQLiException(Resources.AntiSQLiResource.Error_UnableToParameterizeQuery);
            }

            // Set the underlying command object (query text and command type), along with the 
            // parsed parameters
            dbCommand.CommandText = parameterizedQueryText;
            dbCommand.CommandType = System.Data.CommandType.Text;
            dbCommand.Parameters.Clear();
            dbCommand.Parameters.AddRange(parsedParameters.ToArray());
        }


        //-------------------------------------------------------------------------
        /// <summary>
        ///     Attempts to convert a given array of objects into a 
        ///     collection of data parameters of type TDbParameter
        /// </summary>
        /// <typeparam name="DbParameterType"></typeparam>
        /// <param name="parameterCollection"></param>
        /// <param name="queryTextArgs">Objects to convert</param>
        /// <returns>
        ///     Returns true on success, false otherwise
        /// </returns>
        //-------------------------------------------------------------------------
        public static bool TryConvertObjectsToDbParameterCollection<DbParameterType>(
            out IEnumerable<DbParameterType> parameterCollection,
            params Object[] queryTextArgs) where DbParameterType : DbParameter
        {

            try
            {
                List<DbParameterType> results = new List<DbParameterType>();

                // Iterate through each parameter and try to convert to 
                // db parameter with name, type and value properly specified
                foreach (var o in queryTextArgs)
                {
                    // Create a new db parameter, name it and object
                    DbParameterType paramRef = Activator.CreateInstance<DbParameterType>();
                    DbParameter dbParam = paramRef as DbParameter ?? throw new AntiSQLiException(Resources.AntiSQLiResource.Error_ObjToDbParameterConversionError);
                    dbParam.ParameterName = "@AntiSQLiParam" + results.Count();
                    dbParam.Value = o;

                    // The type of the parameter must be defined, if unparseable then
                    // assume a string value and call the object's .ToString()
                    DbType parsedDbType;
                    if (!GetDbTypeForObject<DbParameterType>(o, out parsedDbType))
                    {
                        parsedDbType = DbType.String;
                        dbParam.Value = o?.ToString() ?? null;
                    }
                    dbParam.DbType = parsedDbType;

                    // Done, add the completed db parameter to the final results
                    results.Add(paramRef);
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

        private static bool GetDbTypeForObject<DbParameterType>(Object Obj, out DbType Dtype)
        {
            try
            {
                // Create a new db parameter, assign temp name and value
                // Then read the parsed dbtype
                DbParameterType paramRef = (DbParameterType)Activator.CreateInstance(typeof(DbParameterType), "temp", Obj);
                DbParameter dbParam = paramRef as DbParameter ?? throw new AntiSQLiException(Resources.AntiSQLiResource.Error_UnableToAttainDbTypeForObject);
                Dtype = dbParam.DbType;
                return (true);
            }
            catch (Exception e)
            {
                Dtype = DbType.Object;
                return (false);
            }
        }


        public static bool TryParameterizeQueryText<DbParameterType>(String queryText,
            IEnumerable<DbParameterType> parameterCollection, out String parameterizedQueryText) where DbParameterType : DbParameter
        {
            try
            {
                // Build a list of the parameter names (example: @p1, @p2, ...
                List<String> parameterNames = new List<String>();
                parameterCollection.ToList().ForEach(x => parameterNames.Add(x.ParameterName));

                // Apply format token assignment to queryText using parameter names, if not 
                // enought parameters String.Format will throw an exception
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


    
}
