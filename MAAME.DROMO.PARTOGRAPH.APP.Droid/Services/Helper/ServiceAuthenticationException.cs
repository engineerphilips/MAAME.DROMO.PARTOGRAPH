using System;
using System.Collections.Generic;
using System.Text;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services.Helper
{
    internal class ServiceAuthenticationException : Exception
    {
        public string Content { get; }

        public ServiceAuthenticationException()
        {
        }

        public ServiceAuthenticationException(string content)
        {
            Content = content;
        }
    }
}
