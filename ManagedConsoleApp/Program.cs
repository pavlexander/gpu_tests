using Newtonsoft.Json;
using ProccessorImplementation;
using System;

namespace ManagedConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start");

            var cpu = new Impl_CPU();
            cpu.RunMainProccessor();
            cpu = null;

            var managedCuda = new Impl_ManagedCuda();
            managedCuda.RunMainProccessor();
            managedCuda = null;

            var cloo = new Impl_Cloo();
            cloo.RunMainProccessor();
            cloo = null;

            var ilgpu = new Impl_ILGPU();
            ilgpu.RunMainProccessor();
            ilgpu = null;
            
            var nativeCuda = new Impl_NativeCuda();
            nativeCuda.RunMainProccessor();
            nativeCuda = null;

            Console.WriteLine("End");
        }
    }
}
