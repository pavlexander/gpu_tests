using CommonLibrary;
using ILGPU;
using ILGPU.Backends;
using ILGPU.Backends.PTX;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ILGPU.Algorithms;
using ILGPU.Runtime.OpenCL;
using System.IO;

// http://www.ilgpu.net/Documentation
namespace ProccessorImplementation
{
    public class Impl_ILGPU : TestsBase
    {
        private MemoryBuffer1D<int, Stride1D.Dense> input1_dev;
        private MemoryBuffer1D<int, Stride1D.Dense> input2_dev;
        private MemoryBuffer1D<double, Stride1D.Dense> input3_dev;
        private MemoryBuffer1D<byte, Stride1D.Dense> input4_dev;

        private MemoryBuffer1D<byte, Stride1D.Dense> result_dev;
        private MemoryBuffer1D<double, Stride1D.Dense> resultCalc_dev;

        private Context context;

        private CudaAccelerator accelerator;

        private Action<Index1D,
                    ArrayView<byte>,
                    ArrayView<double>,
                    ArrayView<int>,
                    ArrayView<int>,
                    ArrayView<double>,
                    ArrayView<byte>,
                    int> myKernel;


        public Impl_ILGPU() : base("ILGU (CUDA)", true)
        {

        }

        private static string GetInfoString(Accelerator a)
        {
            StringWriter infoString = new StringWriter();
            a.PrintInformation(infoString);
            return infoString.ToString();
        }

        public override void Init()
        {
            GeneratePTX(); // alternative approach through this?

            //context = new Context();
            //accelerator = new CudaAccelerator(context);
            context = Context.CreateDefault();

            /*
            // Prints all CPU accelerators.
            Console.WriteLine("\nListing CPU accelerators:");
            foreach (CPUDevice d in context.GetCPUDevices())
            {
                using CPUAccelerator accelerator = (CPUAccelerator)d.CreateAccelerator(context);
                Console.WriteLine(accelerator);
                Console.WriteLine(GetInfoString(accelerator));
            }

            // Prints all Cuda accelerators.
            Console.WriteLine("\nListing Cuda accelerators:");
            foreach (Device d in context.GetCudaDevices())
            {
                using Accelerator accelerator = d.CreateAccelerator(context);
                Console.WriteLine(accelerator);
                Console.WriteLine(GetInfoString(accelerator));
            }

            // Prints all OpenCL accelerators.
            Console.WriteLine("\nListing OpenCL accelerators:");
            foreach (Device d in context.GetCLDevices())
            {
                using Accelerator accelerator = d.CreateAccelerator(context);
                Console.WriteLine(accelerator);
                Console.WriteLine(GetInfoString(accelerator));
            }
            //var sth = context.GetCLDevices();*/

            accelerator = context.CreateCudaAccelerator(0);

            var methodInfo = typeof(Impl_ILGPU).GetMethod(nameof(MyKernel), BindingFlags.Public | BindingFlags.Static);
            myKernel = accelerator.LoadAutoGroupedStreamKernel<Index1D,
                    ArrayView<byte>,
                    ArrayView<double>,
                    ArrayView<int>,
                    ArrayView<int>,
                    ArrayView<double>,
                    ArrayView<byte>,
                    int
                    >(MyKernel);

            // Allocate some memory
            input1_dev = accelerator.Allocate1D<int>(DataGenerator.In1.Length);
            input2_dev = accelerator.Allocate1D<int>(DataGenerator.In2.Length);
            input3_dev = accelerator.Allocate1D<double>(DataGenerator.In3.Length);
            input4_dev = accelerator.Allocate1D<byte>(DataGenerator.In4_3_bytes.Length);

            // init output parameters
            result_dev = accelerator.Allocate1D<byte>(resultsBytes.Length);
            resultCalc_dev = accelerator.Allocate1D<double>(calculatables.Length);

            //input1_dev.CopyFrom(DataGenerator.In1, 0, 0, DataGenerator.In1.Length);
            //input2_dev.CopyFrom(DataGenerator.In2, 0, 0, DataGenerator.In2.Length);
            //input3_dev.CopyFrom(DataGenerator.In3, 0, 0, DataGenerator.In3.Length);
            //input4_dev.CopyFrom(DataGenerator.In4_3_bytes, 0, 0, DataGenerator.In4_3_bytes.Length);

            input1_dev.CopyFromCPU(DataGenerator.In1);
            input2_dev.CopyFromCPU(DataGenerator.In2);
            input3_dev.CopyFromCPU(DataGenerator.In3);
            input4_dev.CopyFromCPU(DataGenerator.In4_3_bytes);
        }

        public override void Dispose()
        {
            // dispose in the oposite order as init

            input1_dev.Dispose();
            input2_dev.Dispose();
            input3_dev.Dispose();
            input4_dev.Dispose();

            result_dev.Dispose();
            resultCalc_dev.Dispose();

            accelerator.Dispose();
            context.Dispose();
        }

        public void GeneratePTX()
        {
            // test compilation
            /*using (var context = Context.CreateDefault()) // new Context()
            {
                //context.EnableAlgorithms();

                using (Backend b = new PTXBackend(context, PTXArchitecture.SM_50, PTXInstructionSet.ISA_50, TargetPlatform.X64))
                {
                    //using (var unit = context.CreateCompileUnit(b, CompileUnitFlags.None))
                    //{
                    //  var compiledKernel = b.Compile(
                    //      unit,
                    //      typeof(GPU_test_ILGPU).GetMethod(nameof(MyKernel), BindingFlags.Public | BindingFlags.Static));
                    //  
                    //  System.IO.File.WriteAllBytes("MyKernel.ptx", compiledKernel.GetBuffer());

                    var methods = typeof(Impl_ILGPU).GetMethods(BindingFlags.Static | BindingFlags.Public);
                    var method = methods.FirstOrDefault(f => f.Name == nameof(MyKernel));//.GetMethod(nameof(MyKernel), BindingFlags.Static);
                    var compiledKernel = b.Compile(method, default);

                    var ptxKernel = compiledKernel as PTXCompiledKernel;
                    System.IO.File.WriteAllText("MyKernel.ptx", ptxKernel.PTXAssembly);
                    //}
                }
            }*/
        }

        public static void MyKernel(
                    Index1D index,
                    ArrayView<byte> results,
                    ArrayView<double> resultsCalc,
                    ArrayView<int> in1,
                    ArrayView<int> in2,
                    ArrayView<double> in3,
                    //ArrayView2D<byte> in4
                    ArrayView<byte> in4,
                    int width
                    )
        {
            bool isTrue = false;
            int varA = in1[index];
            int varB = in2[index];

            double calculatable = 0;
            byte isLastFirstCondition = 0;
            for (int row = 0; row < in3.Length; row++)
            {
                if (isTrue == true)
                {
                    //var i2 = new Index2(row, varA);
                    //var bo = in4[i2];

                    int idx = width * row + varA;
                    if (in4[idx] == 0)
                    {
                        continue;
                    }

                    calculatable = calculatable + in3[row];
                    isTrue = false;

                    isLastFirstCondition = 1;
                }
                else
                {
                    //var i2 = new Index2(row, varB);
                    //var bo = in4[i2];

                    int idx = width * row + varB;
                    if (in4[idx] == 0)
                    {
                        continue;
                    }

                    calculatable = calculatable - in3[row];
                    isTrue = true;

                    isLastFirstCondition = 0;
                }
            }

            results[index] = isLastFirstCondition;
            resultsCalc[index] = calculatable;
        }

        public static void MyKernel2(
            Index1D index, // The global thread index (1D in this case)
            ArrayView<int> dataView, // A view to a chunk of memory (1D in this case)
            int constant) // A sample uniform constant
        {
            dataView[index] = index + constant;
        }

        // https://github.com/m4rs-mt/ILGPU.Samples/blob/master/Src/SharedMemory/Program.cs
        // http://www.ilgpu.net/Documentation
        public override void Proccess()
        {
            myKernel((int)input1_dev.Length, result_dev.View, resultCalc_dev.View, input1_dev.View, input2_dev.View, input3_dev.View, input4_dev.View, DataGenerator.Width);

            // Wait for the kernel to finish...
            accelerator.Synchronize();

            // Resolve data
            //resultsBytes = result_dev.GetAsArray();
            //calculatables = resultCalc_dev.GetAsArray();

            result_dev.CopyToCPU(resultsBytes);
            resultCalc_dev.CopyToCPU(calculatables);
            //resultsBytes = result_dev.GetAsArray1D();
            //calculatables = resultCalc_dev.GetAsArray1D();
        }

        public void ProccessOld()
        {
            // Create the required ILGPU context
            /*using (var context = new Context())
            {
                using (var accelerator = new CudaAccelerator(context)) // test with CPUAccelerator
                {
                    var methodInfo = typeof(Impl_ILGPU).GetMethod(nameof(MyKernel), BindingFlags.Public | BindingFlags.Static);
                    var myKernel = accelerator.LoadAutoGroupedStreamKernel<Index,
                            ArrayView<byte>,
                            ArrayView<double>,
                            ArrayView<int>,
                            ArrayView<int>,
                            ArrayView<double>,
                            ArrayView<byte>,
                            int
                            >(MyKernel);
                    // Allocate some memory
                    var input1_dev = accelerator.Allocate<int>(DataGenerator.In1.Length);
                    var input2_dev = accelerator.Allocate<int>(DataGenerator.In2.Length);
                    var input3_dev = accelerator.Allocate<double>(DataGenerator.In3.Length);
                    //var input4_dev = accelerator.Allocate<byte>(DataGenerator.In4_2.GetLength(0), DataGenerator.In4_2.GetLength(1));
                    var input4_dev = accelerator.Allocate<byte>(DataGenerator.In4_3_bytes.Length);

                    // init output parameters
                    var result_dev = accelerator.Allocate<byte>(resultsBytes.Length);
                    var resultCalc_dev = accelerator.Allocate<double>(calculatables.Length);

                    input1_dev.CopyFrom(DataGenerator.In1, 0, 0, DataGenerator.In1.Length);
                    input2_dev.CopyFrom(DataGenerator.In2, 0, 0, DataGenerator.In2.Length);
                    input3_dev.CopyFrom(DataGenerator.In3, 0, 0, DataGenerator.In3.Length);
                    //input4_dev.CopyFrom(DataFeeder.In4_2_bytes, new Index2(), new Index2(DataFeeder.In4_2_bytes.GetLength(0), 0), new Index2(1, 2));
                    //input4_dev.CopyFrom(DataGenerator.In4_2_bytes, Index2.Zero, Index2.Zero, input4_dev.Extent);
                    input4_dev.CopyFrom(DataGenerator.In4_3_bytes, 0, 0, DataGenerator.In4_3_bytes.Length);

                    myKernel(input1_dev.Length, result_dev.View, resultCalc_dev.View, input1_dev.View, input2_dev.View, input3_dev.View, input4_dev.View, DataGenerator.Width);

                    // Wait for the kernel to finish...
                    accelerator.Synchronize();

                    // Resolve data
                    resultsBytes = result_dev.GetAsArray();
                    calculatables = resultCalc_dev.GetAsArray();
                }
            }*/
        }
    }
}