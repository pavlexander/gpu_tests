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

// http://www.ilgpu.net/Documentation
namespace ProccessorImplementation
{
    public class Impl_ILGPU : TestsBase
    {
        private MemoryBuffer<int> input1_dev;
        private MemoryBuffer<int> input2_dev;
        private MemoryBuffer<double> input3_dev;
        private MemoryBuffer<byte> input4_dev;

        private MemoryBuffer<byte> result_dev;
        private MemoryBuffer<double> resultCalc_dev;


        public Impl_ILGPU() : base("ILGU (CUDA)", true)
        {

        }

        private Context context;
        private CudaAccelerator accelerator;
        private Action<Index,
                    ArrayView<byte>,
                    ArrayView<double>,
                    ArrayView<int>,
                    ArrayView<int>,
                    ArrayView<double>,
                    ArrayView<byte>,
                    int> myKernel;

        public override void Init()
        {
            GeneratePTX(); // alternative approach through this?

            context = new Context();
            accelerator = new CudaAccelerator(context);

            var methodInfo = typeof(Impl_ILGPU).GetMethod(nameof(MyKernel), BindingFlags.Public | BindingFlags.Static);
            myKernel = accelerator.LoadAutoGroupedStreamKernel<Index,
                    ArrayView<byte>,
                    ArrayView<double>,
                    ArrayView<int>,
                    ArrayView<int>,
                    ArrayView<double>,
                    ArrayView<byte>,
                    int
                    >(MyKernel);

            // Allocate some memory
            input1_dev = accelerator.Allocate<int>(DataGenerator.In1.Length);
            input2_dev = accelerator.Allocate<int>(DataGenerator.In2.Length);
            input3_dev = accelerator.Allocate<double>(DataGenerator.In3.Length);
            input4_dev = accelerator.Allocate<byte>(DataGenerator.In4_3_bytes.Length);

            // init output parameters
            result_dev = accelerator.Allocate<byte>(resultsBytes.Length);
            resultCalc_dev = accelerator.Allocate<double>(calculatables.Length);

            input1_dev.CopyFrom(DataGenerator.In1, 0, 0, DataGenerator.In1.Length);
            input2_dev.CopyFrom(DataGenerator.In2, 0, 0, DataGenerator.In2.Length);
            input3_dev.CopyFrom(DataGenerator.In3, 0, 0, DataGenerator.In3.Length);
            input4_dev.CopyFrom(DataGenerator.In4_3_bytes, 0, 0, DataGenerator.In4_3_bytes.Length);
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
            using (var context = new Context())
            {
                context.EnableAlgorithms();

                using (Backend b = new PTXBackend(context, PTXArchitecture.SM_50, PTXInstructionSet.ISA_50, TargetPlatform.X64))
                {
                    //using (var unit = context.CreateCompileUnit(b, CompileUnitFlags.None))
                    //{
                    /*
                    var compiledKernel = b.Compile(
                        unit,
                        typeof(GPU_test_ILGPU).GetMethod(nameof(MyKernel), BindingFlags.Public | BindingFlags.Static));

                    System.IO.File.WriteAllBytes("MyKernel.ptx", compiledKernel.GetBuffer());
                        */

                    var methods = typeof(Impl_ILGPU).GetMethods(BindingFlags.Static | BindingFlags.Public);
                    var method = methods.FirstOrDefault(f => f.Name == nameof(MyKernel));//.GetMethod(nameof(MyKernel), BindingFlags.Static);
                    var compiledKernel = b.Compile(method, default);

                    var ptxKernel = compiledKernel as PTXCompiledKernel;
                    System.IO.File.WriteAllText("MyKernel.ptx", ptxKernel.PTXAssembly);
                    //}
                }
            }
        }

        public static void MyKernel(
                    Index index,
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
            Index index, // The global thread index (1D in this case)
            ArrayView<int> dataView, // A view to a chunk of memory (1D in this case)
            int constant) // A sample uniform constant
        {
            dataView[index] = index + constant;
        }

        // https://github.com/m4rs-mt/ILGPU.Samples/blob/master/Src/SharedMemory/Program.cs
        // http://www.ilgpu.net/Documentation
        public override void Proccess()
        {
            myKernel(input1_dev.Length, result_dev.View, resultCalc_dev.View, input1_dev.View, input2_dev.View, input3_dev.View, input4_dev.View, DataGenerator.Width);

            // Wait for the kernel to finish...
            accelerator.Synchronize();

            // Resolve data
            resultsBytes = result_dev.GetAsArray();
            calculatables = resultCalc_dev.GetAsArray();
        }

        public void ProccessOld()
        {
            // Create the required ILGPU context
            using (var context = new Context())
            {
                /*
                using (var accelerator = new CPUAccelerator(context))
                {
                    // accelerator.LoadAutoGroupedStreamKernel creates a typed launcher
                    // that implicitly uses the default accelerator stream.
                    // In order to create a launcher that receives a custom accelerator stream
                    // use: accelerator.LoadAutoGroupedKernel<Index, ArrayView<int> int>(...)
                    var myKernel = accelerator.LoadAutoGroupedStreamKernel<Index, ArrayView<int>, int>(MyKernel2);

                    // Allocate some memory
                    using (var buffer = accelerator.Allocate<int>(1024))
                    {
                        // Launch buffer.Length many threads and pass a view to buffer
                        myKernel(buffer.Length, buffer.View, 42);

                        // Wait for the kernel to finish...
                        accelerator.Synchronize();

                        // Resolve data
                        var data = buffer.GetAsArray();
                        // ...
                    }
                }*/

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

                    /*
                    var myKernel = accelerator.LoadAutoGroupedStreamKernel<Action<Index,
                            ArrayView<byte>,
                            ArrayView<int>,
                            ArrayView<int>,
                            ArrayView<double>,
                            ArrayView2D<byte>>>(methodInfo);*/

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

                    //d_in1.Dispose();
                    //d_in1 = null;

                    /*
                    var kernelWithDefaultStream = accelerator.LoadAutoGroupedStreamKernel<
                        Index,
                        ArrayView<bool>,
                        ArrayView<int>,
                        ArrayView<int>,
                        ArrayView<double>,
                        ArrayView2D<bool>
                        >(MyKernel);

                    kernelWithDefaultStream(buffer.Extent, buffer.View, 1);
                    */

                    // Launch buffer.Length many threads and pass a view to buffer
                    //myKernel(d_in1.Length, d_in1.View, 42);

                    // Wait for the kernel to finish...
                    //accelerator.Synchronize();

                    // Resolve data
                    //var data = buffer.GetAsArray();
                    // ...

                }
            }
        }

    }
}