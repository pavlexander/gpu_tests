using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary
{
    public interface ITestsBase
    {
        void Init();
        void RunMainProccessor();
        void Proccess();
        void Dispose();
    }
}
