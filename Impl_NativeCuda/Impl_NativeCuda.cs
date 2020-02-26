using CLRLib;
using CommonLibrary;
using System;

namespace ProccessorImplementation
{
    public class Impl_NativeCuda : TestsBase
    {
        private CudaWrapper _wrapper;

        public Impl_NativeCuda() : base("Native CUDA (CLR wrapper)", true)
        {

        }

        public override void Init()
        {
            _wrapper = new CudaWrapper();
        }

        public override void Dispose()
        {
            _wrapper = null;
        }

        public override void Proccess()
        {
            int result = _wrapper.Execute(resultsBytes, ref calculatables, DataGenerator.In1, DataGenerator.In2, DataGenerator.In3, DataGenerator.In4_3_bytes, DataGenerator.InputCount, DataGenerator.Width, DataGenerator.Height);

            if (result != 0)
            {
                Console2.WriteLineRed("Kernel did not return a success status code. Exception?");
            }
        }
    }
}
