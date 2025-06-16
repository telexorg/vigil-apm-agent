using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VigilAgent.Api.Enums
{
    public enum State
    {
        Submitted = 1,
        Working = 2,
        InputRequired = 3,
        Completed = 4,
        Canceled = 5,
        Failed = 6,
        Rejected = 7,
        AuthRequired = 8,
        Unknown = 9,
    }
}
