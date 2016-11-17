using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimulatedSignals.Models
{
    public class Result
    {
        public DateTime Timestamp { get; set; }
        public float Value { get; set; }
        public string UnitsAbbreviation { get; set; }
        public bool Good { get; set; }
        public bool Questionable { get; set; }
        public bool Substituted { get; set; }

    }
}