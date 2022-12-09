using CommonLibrary;
using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.VectorTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// https://algoslaves.wordpress.com/2013/08/25/nvidia-cuda-hello-world-in-managed-c-and-f-with-use-of-managedcuda/
// https://github.com/kunzmi/managedCuda/wiki/Setup-a-managedCuda-project
// https://stackoverflow.com/questions/15030461/how-to-make-cuda-dll-that-can-be-used-in-c-sharp-application
namespace ProccessorImplementation
{
    public class Impl_ManagedCuda : TestsBase
    {
        static CudaKernel myKernel;

        // init input parameters
        CudaDeviceVariable<int> input1_dev;
        CudaDeviceVariable<int> input2_dev;
        CudaDeviceVariable<double> input3_dev;
        CudaDeviceVariable<byte> input4_dev;

        CudaDeviceVariable<byte> result_dev;
        CudaDeviceVariable<double> resultCalc_dev;

        public Impl_ManagedCuda() : base("Managed CUDA", true)
        {

        }

        public override void Init()
        {
            int blockSize = 1024; // 256

            int N = DataGenerator.InputCount;
            CudaContext cntxt = new CudaContext();
            CUmodule cumodule = cntxt.LoadModule(@"kernel.cubin");
            myKernel = new CudaKernel("proccess", cumodule, cntxt);
            //myKernel.GridDimensions = (N + 255) / 256;
            //myKernel.BlockDimensions = Math.Min(N, 256);
            myKernel.GridDimensions = (N + (blockSize - 1)) / blockSize;
            myKernel.BlockDimensions = blockSize;

            // https://softwarehut.com/blog/general-purpose-computing-gpu-net-world-part-1/
            //https://stackoverflow.com/questions/2392250/understanding-cuda-grid-dimensions-block-dimensions-and-threads-organization-s
            //myKernel.GridDimensions = new dim3(1, 1, 1);
            //myKernel.BlockDimensions = new dim3(16, 16);

            // init input parameters
            input1_dev = new CudaDeviceVariable<int>(DataGenerator.In1.Length);
            input2_dev = new CudaDeviceVariable<int>(DataGenerator.In2.Length);
            input3_dev = new CudaDeviceVariable<double>(DataGenerator.In3.Length);
            input4_dev = new CudaDeviceVariable<byte>(DataGenerator.In4_3_bytes.Length);

            result_dev = new CudaDeviceVariable<byte>(resultsBytes.Length);
            resultCalc_dev = new CudaDeviceVariable<double>(calculatables.Length);

            // copy input parameters
            input1_dev.CopyToDevice(DataGenerator.In1);
            input2_dev.CopyToDevice(DataGenerator.In2);
            input3_dev.CopyToDevice(DataGenerator.In3);
            input4_dev.CopyToDevice(DataGenerator.In4_3_bytes);

            // init output parameters
            //result_dev = new CudaDeviceVariable<bool>(results.Length);

            //myKernel.SetConstantVariable("width", DataGenerator.Width);
            //myKernel.SetConstantVariable("inputCount", N);
            //myKernel.SetConstantVariable("height", DataGenerator.Height);
        }

        public override void Dispose()
        {
            input1_dev.Dispose();
            input2_dev.Dispose();
            input3_dev.Dispose();
            input4_dev.Dispose();
            result_dev.Dispose();
            resultCalc_dev.Dispose();
            myKernel = null;
        }

        public override void Proccess()
        {
            // run CUDA method
            myKernel.Run(
                result_dev.DevicePointer,
                resultCalc_dev.DevicePointer,
                input1_dev.DevicePointer, 
                input2_dev.DevicePointer, 
                input3_dev.DevicePointer, 
                input4_dev.DevicePointer,
                DataGenerator.InputCount,
                DataGenerator.Width,
                DataGenerator.Height
                );

            // copy return to host
            result_dev.CopyToHost(resultsBytes);
            resultCalc_dev.CopyToHost(calculatables);
        }
    }
}
