using SimulatedSignals.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SimulatedSignals.Controllers
{
    public class Pit709Controller : ApiController
    {
        public Result Get()
        {
            float value = 50;// 400 + (float)(new Random()).NextDouble() * 1000;
            return new Result()
            {
                Timestamp = DateTime.Now,
                Value = value,
                UnitsAbbreviation = "",
                Good = true,
                Questionable = false,
                Substituted = false
            };
        }
    }
}
