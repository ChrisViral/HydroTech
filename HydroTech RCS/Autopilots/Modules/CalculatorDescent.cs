using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Autopilots.Modules
{
    using HydroTech_FC;

    public class CalculatorDescent
    {
        protected const float t1 = 60.0F;
        protected float h0;
        protected float v0;
        protected float g;
        protected float twr;
        protected float aMax { get { return twr - g; } }
        protected float alpha { get { return aMax / t1; } }

        protected float status_v, status_h;
        protected const float completeLockHeightDiff = 100.0F;

        protected float thrRate = 1.0F;
        public float ThrRate
        {
            get
            {
                if (thrRate < 0.0F)
                    return 0.0F;
                else if (thrRate > 1.0F)
                    return 1.0F;
                else
                    return thrRate;
            }
        }

        public enum BEHAVIOUR { IDLE, NORMAL, BRAKE }
        public BEHAVIOUR behaviour { get { return GetBehaviour(thrRate); } }
        protected BEHAVIOUR GetBehaviour(float a_ratio)
        {
            if (a_ratio < 0.0F)
                return BEHAVIOUR.IDLE;
            else if (a_ratio > 1.0F)
                return BEHAVIOUR.BRAKE;
            else
                return BEHAVIOUR.NORMAL;
        }

        public enum INDICATOR { WARP, SAFE, OK, DANGER }
        public INDICATOR indicator { get { return GetIndicator(thrRate); } }
        protected const float Ratio_WARP = -0.1F;
        protected const float Ratio_SAFE = 0.5F;
        protected const float Ratio_OK = 0.8F;
        protected INDICATOR GetIndicator(float a_ratio)
        {
            if (a_ratio < Ratio_WARP)
                return INDICATOR.WARP;
            else if (a_ratio < Ratio_SAFE)
                return INDICATOR.SAFE;
            else if (a_ratio < Ratio_OK)
                return INDICATOR.OK;
            else
                return INDICATOR.DANGER;
        }

        protected float a_t(float t)
        {
            if (t < t1)
                return alpha * t;
            else
                return aMax;
        }
        protected float v_t(float t)
        {
            if (t < t1)
                return v0 + alpha * t * t / 2;
            else
                return v0 - aMax * t1 / 2 + aMax * t;
        }
        protected float h_t(float t)
        {
            if (t < t1)
                return h0 + v0 * t + alpha * t * t * t / 6;
            else
                return h0 + aMax * t1 * t1 / 6 + (v0 - aMax * t1 / 2) * t + aMax * t * t / 2;
        }
        protected float t_v(float v)
        {
            if (v < v0 + aMax * t1 / 2)
                return HMaths.SqRt(2 * (v - v0) / alpha);
            else
                return (v - v0) / aMax + t1 / 2;
        }

        protected void Calculate()
        {
            float t = t_v(status_v);
            float a = a_t(t);
            float h = h_t(t);
            thrRate = (a + g) / twr + (h - status_h) / completeLockHeightDiff;
        }

        public void OnUpdate(float h0, float v0, float g, float twr, float v, float h)
        {
            this.v0 = v0;
            this.g = g;
            this.twr = twr;
            status_v = v;
            status_h = h;
            if (status_v <= v0)
                thrRate = -1.0F;
            else
                Calculate();
        }
    }
}