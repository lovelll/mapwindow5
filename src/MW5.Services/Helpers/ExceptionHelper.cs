﻿using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace MW5.Services.Helpers
{
    internal static class ExceptionHelper
    {
        public static string ExceptionToString(this Exception ex)
        {
            string msg = string.Empty;
            ExceptionToString(ex, ref msg);
            return msg;
        }

        private static void ExceptionToString(Exception ex, ref string msg)
        {
            if (!string.IsNullOrWhiteSpace(msg))
            {
                msg += Environment.NewLine + Environment.NewLine;
            }

            msg += "Description: " + ex.Message + Environment.NewLine + Environment.NewLine;
            msg += "Stack trace: " + Environment.NewLine + ex.StackTrace;

            if (ex.InnerException != null)
            {
                ExceptionToString(ex.InnerException, ref msg);
            }
        }
        
        internal static string Report(Exception ex)
        {
            if (ex is ReflectionTypeLoadException)
            {
                return ReportReflectionException(ex as ReflectionTypeLoadException);
            }

            return string.Empty;
        }

        private static string ReportReflectionException(ReflectionTypeLoadException ex)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Exception exSub in ex.LoaderExceptions)
            {
                sb.AppendLine(exSub.Message);
                if (exSub is FileNotFoundException)
                {
                    FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                    if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                    {
                        sb.AppendLine("Fusion Log:");
                        sb.AppendLine(exFileNotFound.FusionLog);
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
