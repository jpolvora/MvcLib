using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Text;

namespace MvcLib.Common
{
    public static class ExceptionHelpers
    {
        public static void Raise(this Exception ex, string msg)
        {
            if (ex == null)
                return;

            LogEvent.Raise(msg ?? ex.Message, ex);
        }


        public static string LoopException(this Exception exception, bool stack = true, StringBuilder sb = null)
        {
            if (exception == null)
                return "";

            if (sb == null)
                sb = new StringBuilder();

            sb.AppendFormat("Exception Type: {0}", exception.GetType().Name).AppendLine();

            sb.AppendFormat("Exception Message: {0}", exception.Message).AppendLine();

            var dbEntityValidationException = exception as DbEntityValidationException;
            if (dbEntityValidationException != null)
            {
                foreach (var dbEntityValidationResult in dbEntityValidationException.EntityValidationErrors)
                {
                    foreach (var dbValidationError in dbEntityValidationResult.ValidationErrors)
                    {
                        sb.AppendFormat("Property: {0}, Error: {1}", dbValidationError.PropertyName, dbValidationError.ErrorMessage)
                            .AppendLine();
                    }
                }
            }

            var updateException = exception as DbUpdateException;
            if (updateException != null)
            {
                foreach (var stateEntry in updateException.Entries)
                {
                    foreach (var propertyName in stateEntry.CurrentValues.PropertyNames)
                    {
                        var value = stateEntry.CurrentValues[propertyName];
                        if (value == null)
                        {
                            value = "[NULL]";
                        }

                        if (value is byte[])
                        {
                            var byter = value as byte[];
                            value = "byte[{0}]".Fmt(byter.Length);
                        }

                        sb.AppendFormat("Property: {0}, Value: {1}", propertyName, value).AppendLine();
                    }
                }
            }

            if (stack)
            {
                sb.AppendFormat("StackTrace: {0}", exception.StackTrace).AppendLine();
            }

            if (exception.InnerException != null)
            {
                sb.AppendLine(exception.InnerException.LoopException(stack, sb)).AppendLine();
            }

            return sb.ToString();
        }
    }
}