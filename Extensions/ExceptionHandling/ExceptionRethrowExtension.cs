using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Extensions.ExceptionHandling
{
    public static class ExceptionRethrowExtension
    {
        /// <summary>
        /// If exception is kind of "special" one which shouldn't be handled by default
        /// like StackOverflowException or ThreadAbortionException then rethow it
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RethrowIfExceptionCantBeHandled(this Exception exception)
        {
            if(
                exception is StackOverflowException ||
                exception is ThreadAbortException ||
                exception is AccessViolationException
                )
            {
                throw exception; //rethrow
            }

        }
    }
}
