using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Implementation
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            MagnetController myMC = new MagnetController();
            myMC.initialise();
        }
    }
}

