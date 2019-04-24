using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //SpeedTester speedTester = new SpeedTester(Test);
            //speedTester.RunTest();
            Benchmark Benchmark = new Benchmark();
            Benchmark.Main();
            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }
        static int iterations = 10000000; // Give it a reasonable default
        static ArrayList list; // This is what we'll check

        [Benchmark]
        public static void Simple()
        {
            int count = iterations;

            ArrayList al = new ArrayList();
            for (int i = 0; i < count; i++)
                al.Add("hello");
            // Set the result
            list = al;
        }

        [Benchmark]
        public static void RightSizing()
        {
            int count = iterations;

            ArrayList al = new ArrayList(count);
            for (int i = 0; i < count; i++)
                al.Add("hello");
            // Set the result
            list = al;
        }
    }
}
