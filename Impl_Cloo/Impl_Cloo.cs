using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Cloo;
using CommonLibrary;

// https://dhruba.wordpress.com/2012/10/14/opencl-cookbook-hello-world-using-c-cloo-host-binding/
namespace ProccessorImplementation
{
    public class Impl_Cloo : TestsBase
    {
        public Impl_Cloo() : base("Cloo (OpenCL)", true)
        {

        }

        private ComputeKernel kernel;
        private ComputeCommandQueue queue;

        private ComputeBuffer<byte> result_dev;
        private ComputeBuffer<double> resultCalc_dev;

        private ComputeBuffer<int> input1_dev;
        private ComputeBuffer<int> input2_dev;
        private ComputeBuffer<double> input3_dev;
        private ComputeBuffer<byte> input4_dev;
        //ComputeBuffer<int> input5_dev;

        public override void Init()
        {
            // pick first platform
            ComputePlatform platform = ComputePlatform.Platforms[0];

            // create context with all gpu devices
            ComputeContext context = new ComputeContext(ComputeDeviceTypes.Gpu, new ComputeContextPropertyList(platform), null, IntPtr.Zero);

            // create a command queue with first gpu found
            queue = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);

            // load opencl source
            StreamReader streamReader = new StreamReader("main.cl");
            string clSource = streamReader.ReadToEnd();
            streamReader.Close();

            // create program with opencl source
            ComputeProgram program = new ComputeProgram(context, clSource);

            // compile opencl source
            program.Build(null, null, null, IntPtr.Zero);
            
            result_dev = new ComputeBuffer<byte>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.AllocateHostPointer, results.Length);
            resultCalc_dev = new ComputeBuffer<double>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.AllocateHostPointer, calculatables.Length);

            // init input parameters
            input1_dev = new ComputeBuffer<int>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, DataGenerator.In1);
            input2_dev = new ComputeBuffer<int>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, DataGenerator.In2);
            input3_dev = new ComputeBuffer<double>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, DataGenerator.In3);
            input4_dev = new ComputeBuffer<byte>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, DataGenerator.In4_3_bytes);
            //input5_dev = new ComputeBuffer<int>(context, ComputeMemoryFlags.ReadOnly, DataFeeder.width);
            
            // load chosen kernel from program
            kernel = program.CreateKernel("Proccess");

            int i = 0;
            kernel.SetMemoryArgument(i++, result_dev);
            kernel.SetMemoryArgument(i++, resultCalc_dev);
            kernel.SetMemoryArgument(i++, input1_dev);
            kernel.SetMemoryArgument(i++, input2_dev);
            kernel.SetMemoryArgument(i++, input3_dev);
            kernel.SetMemoryArgument(i++, input4_dev);
            kernel.SetValueArgument(i++, DataGenerator.Width);
            kernel.SetValueArgument(i++, DataGenerator.Height);
        }

        public override void Proccess()
        {
            // execute kernel
            queue.Execute(kernel, null, new long[] { DataGenerator.InputCount }, null, null);
            queue.Finish();

            /*
            short[] results2 = new short[this.results.Length];
            GCHandle arrCHandle = GCHandle.Alloc(results2, GCHandleType.Pinned);
            queue.Read(result_dev, true, 0, DataFeeder.GetInputCount(), arrCHandle.AddrOfPinnedObject(), events);
            */
            
            //bool[] results2 = new bool[DataFeeder.GetInputCount()];
            queue.ReadFromBuffer(result_dev, ref resultsBytes, true, null);
            queue.ReadFromBuffer(resultCalc_dev, ref calculatables, true, null);
            //queue.ReadFromBuffer()
            /*
            bool[] arrC = new bool[5];
            GCHandle arrCHandle = GCHandle.Alloc(arrC, GCHandleType.Pinned);
            queue.Read<bool>(result_dev, true, 0, 5, arrCHandle.AddrOfPinnedObject(), null);
            */
            // wait for completion
            //queue.Finish();

            //kernel.Dispose();
            //queue.Dispose();
            //context.Dispose();
        }

        public void ProccessOld()
        {
            // pick first platform
            ComputePlatform platform = ComputePlatform.Platforms[0];

            // create context with all gpu devices
            ComputeContext context = new ComputeContext(ComputeDeviceTypes.Gpu, new ComputeContextPropertyList(platform), null, IntPtr.Zero);

            // create a command queue with first gpu found
            ComputeCommandQueue queue = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);

            // load opencl source
            StreamReader streamReader = new StreamReader("1.cl");
            string clSource = streamReader.ReadToEnd();
            streamReader.Close();

            // create program with opencl source
            ComputeProgram program = new ComputeProgram(context, clSource);

            // compile opencl source
            program.Build(null, null, null, IntPtr.Zero);

            // load chosen kernel from program
            ComputeKernel kernel = program.CreateKernel("sum");

            // create a ten integer array and its length
            float[] a = new float[] { 1.1f, 2.3f };
            float[] b = new float[] { 3.6f, 4.9f };

            // allocate a memory buffer with the message (the int array)
            ComputeBuffer<float> aBuffer = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, a);
            ComputeBuffer<float> bBuffer = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, b);

            kernel.SetMemoryArgument(0, aBuffer);
            kernel.SetMemoryArgument(1, bBuffer);

            // execute kernel
            queue.ExecuteTask(kernel, null);

            // wait for completion
            queue.Finish();

            queue.ReadFromBuffer<float>(aBuffer, ref a, true, null);

            for (int i = 0; i < a.Length; i++)
            {
                Console.WriteLine(a[i]);
            }

            /*
            // create a ten integer array and its length
            int[] message = new int[] { 1, 2, 3, 4, 5 };
            int messageSize = message.Length;

            // allocate a memory buffer with the message (the int array)
            ComputeBuffer<int> messageBuffer = new ComputeBuffer<int>(context,
            ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, message);

            kernel.SetMemoryArgument(0, messageBuffer); // set the integer array
            kernel.SetValueArgument(1, messageSize); // set the array size

            // execute kernel
            queue.ExecuteTask(kernel, null);

            // wait for completion
            queue.Finish();*/
        }
        
        public override void Dispose()
        {
            input1_dev.Dispose();
            input2_dev.Dispose();
            input3_dev.Dispose();
            input4_dev.Dispose();
            result_dev.Dispose();
            kernel.Dispose();
            kernel = null;
        }
    }
}
