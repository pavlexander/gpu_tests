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

        // random bools in 2d array
        public static bool[,] In4_2;
        public static byte[,] In4_2_bytes;

        // flattened random bools
        public static bool[] In4_3;
        public static byte[] In4_3_bytes;

        // core params
        public const int InputCount = 800000; // 850_628, 1_245_456
        public const int Height = 3000; // 5_000, 20_000
        public const int Width = 200;
        public static int DoubleRandomRange = 3;

        private static SecureRandom _rand = new SecureRandom();

        static DataGenerator()
        {
            In1 = new int[InputCount];
            In2 = new int[InputCount];

            for (int i = 0; i < In1.Length; i++)
            {
                In1[i] = GetRandomInRangeInt(0, Width); //_rand.Next(0, Width);
                In2[i] = GetRandomInRangeInt(0, Width); // _rand.Next(0, Width);
            }

            In3 = new double[Height];
            for (int i = 0; i < In3.Length; i++)
            {
                In3[i] = GetRandomInRange(0, DoubleRandomRange);
            }

            //Console.WriteLine();
            //Console.WriteLine(In1.Sum());
            //Console.WriteLine(In2.Sum());
            //Console.WriteLine(In3.Sum());
            //Console.WriteLine();

            In4_2 = new bool[Height, Width];
            In4_2_bytes = new byte[Height, Width];
            PopulateBools();

            In4_3 = new bool[Height * Width];
            //System.Buffer.BlockCopy(In4_2, 0, In4_3, 0, sizeof(bool) * In4_2.Length);
            for (int row = 0; row < Height; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    In4_3[row * Width + col] = In4_2[row,col];
                }
            }

            In4_3_bytes = new byte[In4_3.Length];
            for (int i = 0; i < In4_3.Length; i++)
            {
                if (In4_3[i])
                {
                    In4_3_bytes[i] = 1;
                }
            }

            int trueCount = 0;
            for (var i = 0; i < In4_3.Length; i++)
            {
                if (In4_3_bytes[i] == 1)
                {
                    trueCount++;
                }
            }

            //Console.WriteLine();
            //Console.WriteLine(trueCount);
            //Console.WriteLine();
        }

        private static void PopulateBools()
        {
            for (int i = 0; i < In4_2.GetLength(0); i++)
            {
                for (int y = 0; y < In4_2.GetLength(1); y++)
                {
                    int randomOneOrZero = GetRandomInRangeInt(0, 2); //_rand.Next(2);
                    In4_2[i, y] = (randomOneOrZero == 1); // 0 or 1
                    In4_2_bytes[i, y] = In4_2[i, y] == true ? (byte)1 : (byte)0;
                }
            }
        }
        
        private static int GetRandomInRangeInt(int min, int max)
        {
            return _rand.Next(min, max);
        }

        private static double GetRandomInRange(int min, int max)
        {
            return _rand.NextDouble() * (max - min) + min;
        }
    }
}
