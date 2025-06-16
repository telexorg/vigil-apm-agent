using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VigilAgent.Api.Helpers;

namespace VigilAgent.Api.IServices
{
    public interface IAIService
    {
        Task<string> GenerateResponse(string message, string systemMessage, TelemetryTask blogDto);
    }
}
