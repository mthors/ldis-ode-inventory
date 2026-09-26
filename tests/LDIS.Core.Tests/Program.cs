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
                Console.WriteLine("--- Milestone 1 Tests ---");
                var m1Tests = new DatabaseInitializationTests();
                m1Tests.RunAllTests();

                Console.WriteLine();
                Console.WriteLine("--- Milestone 2 Product Management Tests ---");
                var m2Tests = new Milestone2ProductTests();
                m2Tests.RunAllTests();

                Console.WriteLine();
                Console.WriteLine("--- Milestone 3 Stock Operations Tests ---");
                var m3Tests = new Milestone3StockOperationTests();
                m3Tests.RunAllTests();

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("=================================================");
                Console.WriteLine(" ALL MILESTONE 1, 2 & 3 TESTS PASSED SUCCESSFULLY");
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
