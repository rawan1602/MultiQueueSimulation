using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MultiQueueTesting;
using MultiQueueModels;
using System.IO;

namespace MultiQueueSimulation
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ReadFromFiles readFromFiles = new ReadFromFiles();
            SimulationSystem system = new SimulationSystem();
            string file = Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory().ToString()).ToString()) + "\\TestCases\\TestCase1.txt";
            readFromFiles.loadfortest(system, file);
            
            //Console.WriteLine(readFromFiles.RandomValues(system.InterarrivalDistribution, 85));
            //string result = TestingManager.Test(system, Constants.FileNames.TestCase1);
            //MessageBox.Show(result);
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            //Console.WriteLine(system.SelectionMethod);
        }
    }
}
