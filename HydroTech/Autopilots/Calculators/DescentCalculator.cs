using UnityEngine;

namespace HydroTech.Autopilots.Calculators
{
    public class DescentCalculator
    {
        public enum DescentBehaviour
        {
            IDLE,
            NORMAL,
            BRAKE
        }

        public enum DescentIndicator
        {
            WARP,
            SAFE,
            OK,
            DANGER
        }

        #region Constants
        protected const float t1 = 60;
        protected const float completeLockHeightDiff = 100;
        protected const float ratioWarp = -0.1f, ratioSafe = 0.5f, ratioOk = 0.8f;
        #endregion

        #region Fields
        protected float g, twr, h0, v0;
        protected float statusV, statusH;
        #endregion

        #region Properties
        protected float AMax => this.twr - this.g;

        protected float Alpha => this.AMax / t1;

        protected float thrRate = 1;
        public float ThrRate
        {
            get
            {
                if (this.thrRate < 0) { return 0; }
                return this.thrRate > 1 ? 1 : this.thrRate;
            }
        }

        public DescentBehaviour Behaviour => GetBehaviour(this.thrRate);

        public DescentIndicator Indicator => GetIndicator(this.thrRate);
        #endregion

        #region Methods
        protected DescentBehaviour GetBehaviour(float aRatio)
        {
            if (aRatio < 0) { return DescentBehaviour.IDLE; }
            return aRatio > 1 ? DescentBehaviour.BRAKE : DescentBehaviour.NORMAL;
        }

        protected DescentIndicator GetIndicator(float aRatio)
        {
            if (aRatio < ratioWarp) { return DescentIndicator.WARP; }
            if (aRatio < ratioSafe) { return DescentIndicator.SAFE; }
            return aRatio < ratioOk ? DescentIndicator.OK : DescentIndicator.DANGER;
        }

        protected float a_t(float t)
        {
            if (t < t1) { return this.Alpha * t; }
            return this.AMax;
        }

        protected float v_t(float t)
        {
            if (t < t1) { return this.v0 + ((this.Alpha * t * t) / 2); }
            return (this.v0 - ((this.AMax * t1) / 2)) + (this.AMax * t);
        }

        protected float h_t(float t)
        {
            if (t < t1) { return this.h0 + (this.v0 * t) + ((this.Alpha * t * t * t) / 6); }
            return this.h0 + ((this.AMax * t1 * t1) / 6) + ((this.v0 - ((this.AMax * t1) / 2)) * t) + ((this.AMax * t * t) / 2);
        }

        protected float t_v(float v)
        {
            if (v < this.v0 + ((this.AMax * t1) / 2)) { return Mathf.Sqrt((2 * (v - this.v0)) / this.Alpha); }
            return ((v - this.v0) / this.AMax) + (t1 / 2);
        }

        protected void Calculate()
        {
            float t = t_v(this.statusV);
            float a = a_t(t);
            float h = h_t(t);
            this.thrRate = ((a + this.g) / this.twr) + ((h - this.statusH) / completeLockHeightDiff);
        }

        public void OnUpdate(float h0, float v0, float g, float twr, float v, float h)
        {
            this.v0 = v0;
            this.g = g;
            this.twr = twr;
            this.statusV = v;
            this.statusH = h;
            if (this.statusV <= v0) { this.thrRate = -1; }
            else { Calculate(); }
        }
        #endregion
    }
}