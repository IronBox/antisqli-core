using System;


namespace IronBox.AntiSQLi.Core.Models
{
    public class AntiSQLiException : Exception
    {
        public AntiSQLiException(String message) : base(message)
        {

        }
    }
}
