using System;
using Microsoft.Extensions.Configuration;
using WDT2020_a1.Controller;
using WDT2020_a1.Model;

namespace WDT2020_a1
{
    class Program
    {
        static void Main(string[] args)
        {
            new Startup().Run();
        }
    }
}
