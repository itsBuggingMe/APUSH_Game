using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game.Helpers
{
    internal struct Smoother
    {
        private float _value;
        public float Target { get;set; }
        public Smoother(float value)
        {
            _value = value;
            Target = value;
        }

        public void Approach(float? value = null, float smoothingConstant = 0.2f)
        {
            if(value.HasValue)
                Target = value.Value;
            _value += (Target - _value) * smoothingConstant;
        }

        public static implicit operator float(Smoother s) => s._value;
        public static implicit operator Smoother(float s) => new Smoother(s);
    }

    internal static class FloatExtensions
    {
        public static float Approach(ref this float f, float value, float smoothingConst = 0.2f)
        {
            f += (value - f) * smoothingConst;
            return f;
        }
    }
}
