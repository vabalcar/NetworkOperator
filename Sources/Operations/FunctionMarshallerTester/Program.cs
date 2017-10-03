using NetworkOperator.InteropServices;
using System;
using System.Runtime.InteropServices;

namespace FunctionMarshallerTester
{
    class Program
    {
        const string TESTER_LIB_DLL = "FunctionMarshallerTesterLib.dll";

        [DllImport(TESTER_LIB_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ProcessStringActionDelegate(IntPtr stringDelegate);

        [DllImport(TESTER_LIB_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ProcessIntActionDelegate(IntPtr intDelegate);

        static void Main(string[] args)
        {
            Book book = new Book() { Year = 1930 };
            GCPin<Book> bookPin = new GCPin<Book>();
            bookPin.Pin(book);
            GCPin<string> stringPin = new GCPin<string>();
            stringPin.Pin("hi");
            /*
             *  GCPin<int> intPin = new GCPin<int>();
             *  intPin.Pin(9);
             */
            bookPin.Dispose();
            stringPin.Dispose();

            FunctionMarshaler marshaller = new FunctionMarshaler();
            ProcessIntActionDelegate(marshaller.GetPointer<IntAction>(i => Print(i + 1)));
            ProcessStringActionDelegate(marshaller.GetPointer<StringAction>(s => Print(s)));

            Console.Write("Press any key to close the program . . . ");
            Console.ReadKey();
            Console.WriteLine();

            marshaller.Dispose();
        }
        static void Print<T>(T s) => Console.WriteLine($"In C#: {s}");
    }

    [StructLayout(LayoutKind.Sequential)]
    class Book
    {
        public int Year = 1927;
        //Author a = new Author();
        public string GetName()
        {
            return "book name";
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    class Author
    {
        public int Age = 39;
    }
}
