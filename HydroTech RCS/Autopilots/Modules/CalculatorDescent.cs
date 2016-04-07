using HydroTech_FC;

namespace HydroTech_RCS.Autopilots.Modules
{
    public class CalculatorDescent
    {
        public enum Behaviour
        {
            IDLE,
            NORMAL,
            BRAKE
        }

        public enum Indicator
        {
            WARP,
            SAFE,
            OK,
            DANGER
        }

        protected const float t1 = 60.0F;
        protected const float completeLockHeightDiff = 100.0F;
        protected const float ratioWarp = -0.1F;
        protected const float ratioSafe = 0.5F;
        protected const float ratioOk = 0.8F;
        protected float g;
        protected float h0;

        protected float statusV, statusH;

        protected float thrRate = 1.0F;
        protected float twr;
        protected float v0;

        protected float AMax
        {
            get { return this.twr - this.g; }
        }

        protected float Alpha
        {
            get { return this.AMax / t1; }
        }

        public float ThrRate
        {
            get
            {
                if (this.thrRate < 0.0F) { return 0.0F; }
                return this.thrRate > 1.0F ? 1.0F : this.thrRate;
            }
        }

        public Behaviour CurrentBehaviour
        {
            get { return GetBehaviour(this.thrRate); }
        }

        public Indicator CurrentIndicator
        {
            get { return GetIndicator(this.thrRate); }
        }

        protected Behaviour GetBehaviour(float aRatio)
        {
            if (aRatio < 0.0F) { return Behaviour.IDLE; }
            return aRatio > 1.0F ? Behaviour.BRAKE : Behaviour.NORMAL;
        }

        protected Indicator GetIndicator(float aRatio)
        {
            if (aRatio < ratioWarp) { return Indicator.WARP; }
            if (aRatio < ratioSafe) { return Indicator.SAFE; }
            return aRatio < ratioOk ? Indicator.OK : Indicator.DANGER;
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
            if (v < this.v0 + ((this.AMax * t1) / 2)) { return HMaths.SqRt((2 * (v - this.v0)) / this.Alpha); }
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
            if (this.statusV <= v0) { this.thrRate = -1.0F; }
            else
            {
                Calculate();
            }
        }
    }
}