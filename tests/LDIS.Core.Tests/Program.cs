using System;

namespace LDIS.Core.Tests
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("=================================================");
            Console.WriteLine(" LDIS Milestone 1: Automated Test Suite");
            Console.WriteLine("=================================================");
            Console.WriteLine();

            try
            {
                var tests = new DatabaseInitializationTests();
                tests.RunAllTests();

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("=================================================");
                Console.WriteLine(" ALL MILESTONE 1 TESTS PASSED SUCCESSFULLY");
                Console.WriteLine("=================================================");
                Console.ResetColor();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("=================================================");
                Console.WriteLine(" TEST RUN FAILED");
                Console.WriteLine(" " + ex.Message);
                Console.WriteLine("=================================================");
                Console.ResetColor();
                return 1;
            }
        }
    }
}
