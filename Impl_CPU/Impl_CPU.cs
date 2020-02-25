using CommonLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProccessorImplementation
{
    public class Impl_CPU : TestsBase
    {
        public Impl_CPU() : base("CPU Parallel loop", false) {

        }

        public override void Dispose()
        {
            
        }

        public override void Init()
        {
            
        }

        public override void Proccess()
        {
            //for (int index = 0; index < DataGenerator.InputCount; index++) {
            Parallel.For(0, DataGenerator.InputCount, index => {
                bool isTrue = false;
                int varA = DataGenerator.In1[index]; // column (from width)
                int varB = DataGenerator.In2[index];

                // itterate through 2-dimensional array
                double calculatable = 0;
                bool isLastFirstCondition = false;
                for (int row = 0; row < DataGenerator.Height; row++)
                {
                    if (isTrue)
                    {
                        // get 2d index in flattened array
                        int idx = DataGenerator.Width * row + varA; // width * row + col

                        if (!DataGenerator.In4_3[idx])
                            continue;

                        calculatable = calculatable + DataGenerator.In3[row];
                        isTrue = false;

                        isLastFirstCondition = true; // if last is condition 1 - then true
                    }
                    else
                    {
                        int idx = DataGenerator.Width * row + varB;

                        if (!DataGenerator.In4_3[idx])
                            continue;

                        calculatable = calculatable - DataGenerator.In3[row];
                        isTrue = true;

                        isLastFirstCondition = false;
                    }
                }

                results[index] = isLastFirstCondition;
                base.calculatables[index] = calculatable;
            });
            //}
        }
    }
}
