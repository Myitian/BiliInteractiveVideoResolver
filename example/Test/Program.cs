using LibBiliInteractiveVideo.Execution.Compilation;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            foreach (var arg in new NamedNativeActionEnumerator<double>(Console.ReadLine()))
            {
                Console.WriteLine(arg.ToString());
            }
        }
    }
}
