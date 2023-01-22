using System;
using DiscordbotTest7.Core;

namespace DiscordbotTest7
{
    class Program
    {
        static void Main(string[] args)
        => new Bot().MainAsync().GetAwaiter().GetResult();
    }
}

