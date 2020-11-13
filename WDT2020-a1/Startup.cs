using System;
using WDT2020_a1.Controller;
using WDT2020_a1.Model;
using WDT2020_a1.View;

namespace WDT2020_a1
{
    public class Startup
    {
        public void Run()
        {
            var engine = new Model.AppEngine();
            var callback = new Callback(engine);

            callback.Run();

        }
    }
}