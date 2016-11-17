using SimulatedSignals.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SimulatedSignals.Controllers
{
    public class Tit735Controller : ApiController
    {
        public Result Get()
        {
            float value = 18;//40 + (float)(new Random()).NextDouble() * 100;
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
