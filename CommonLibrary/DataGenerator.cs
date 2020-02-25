using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary
{
    public static class DataGenerator
    {
        public static int[] In1;
        public static int[] In2;

        public static double[] In3;
        public static int DoubleRandomRange = 3;

        // random bools in 2d array
        public static bool[,] In4_2;
        public static byte[,] In4_2_bytes;

        // flattened random bools
        public static bool[] In4_3;
        public static byte[] In4_3_bytes;

        // core params
        public const int InputCount = 850_628; // 1_245_456
        public const int Height = 5_000; // 20_000
        public const int Width = 200;

        private static Random _rand = new Random();

        static DataGenerator()
        {
            In1 = new int[InputCount];
            In2 = new int[InputCount];

            for (int i = 0; i < In1.Length; i++)
            {
                In1[i] = _rand.Next(0, Width);
                In2[i] = _rand.Next(0, Width);
            }

            In3 = new double[Height];
            for (int i = 0; i < In3.Length; i++)
            {
                In3[i] = _rand.NextDouble() * (DoubleRandomRange - 0) + 0;
            }

            In4_2 = new bool[Height, Width];
            In4_2_bytes = new byte[Height, Width];
            PopulateBools();

            In4_3 = new bool[Height * Width];
            System.Buffer.BlockCopy(In4_2, 0, In4_3, 0, sizeof(bool) * In4_2.Length);

            In4_3_bytes = new byte[In4_3.Length];
            for (int i = 0; i < In4_3.Length; i++)
            {
                if (In4_3[i])
                {
                    In4_3_bytes[i] = 1;
                }
            }
        }

        private static void PopulateBools()
        {
            for (int i = 0; i < In4_2.GetLength(0); i++)
            {
                for (int y = 0; y < In4_2.GetLength(1); y++)
                {
                    In4_2[i, y] = (_rand.Next(2) == 0); // 0 or 1
                    In4_2_bytes[i, y] = In4_2[i, y] == true ? (byte)1 : (byte)0;
                }
            }
        }
    }
}
