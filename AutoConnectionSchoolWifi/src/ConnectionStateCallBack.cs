using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoConnectionSchoolWifi.src
{
    interface ConnectionStateCallBack
    {
        void StateChange(String state);
    }
}
