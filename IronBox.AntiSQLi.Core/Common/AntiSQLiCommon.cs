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
        //---------------------------------------------------------------------
        /// <summary>
        ///     Helper method to validate that the given query text and 
        ///     arguments array is valid
        /// </summary>
        /// <param name="queryText"></param>
        /// <param name="queryTextArgs"></param>
        //---------------------------------------------------------------------
        public static void ValidateQueryTextAndArgsThrowEx(String queryText, params Object[] queryTextArgs)
        {
            if (String.IsNullOrWhiteSpace(queryText))
            {
                throw new AntiSQLiException(Resources.AntiSQLiResource.Error_InvalidQueryText);
            }
            if ((queryTextArgs == null) || (queryTextArgs.Length == 0))
            {
                throw new AntiSQLiException(Resources.AntiSQLiResource.Error_InvalidQueryArguments);
            }
        }

    }


    
}
