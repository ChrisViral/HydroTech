using System.Collections.Generic;
using UnityEngine;

namespace HydroTech.Autopilots.Calculators
{
    public class GroundContactCalculator
    {
        public enum Direction
        {
            CENTER,
            NORTH,
            SOUTH,
            WEST,
            EAST
        }

        #region Constants
        private const float radiusAltASL = 0.01f;
        private const float radiusMin = 1;
        private const float physicsContactDistanceAdd = 1000;
        #endregion

        #region Fields
        private float altASL;
        private readonly Dictionary<Direction, float> distance;
        public bool terrain;
        private Vessel vessel;
        #endregion

        #region Properties
        public float DistCenter
        {
            get { return this.distance[Direction.CENTER]; }
            protected set { this.distance[Direction.CENTER] = value; }
        }

        public float DistNorth
        {
            get { return this.distance[Direction.NORTH]; }
            protected set { this.distance[Direction.NORTH] = value; }
        }

        public float DistSouth
        {
            get { return this.distance[Direction.SOUTH]; }
            protected set { this.distance[Direction.SOUTH] = value; }
        }

        public float DistWest
        {
            get { return this.distance[Direction.WEST]; }
            protected set { this.distance[Direction.WEST] = value; }
        }

        public float DistEast
        {
            get { return this.distance[Direction.EAST]; }
            protected set { this.distance[Direction.EAST] = value; }
        }

        public float SlopeNorth => Mathf.Atan((this.DistCenter - this.DistNorth) / this.Radius);

        public float SlopeSouth => Mathf.Atan((this.DistCenter - this.DistSouth) / this.Radius);

        public float SlopeWest => Mathf.Atan((this.DistCenter - this.DistWest) / this.Radius);

        public float SlopeEast => Mathf.Atan((this.DistCenter - this.DistEast) / this.Radius);

        private CelestialBody MainBody => this.vessel.mainBody;

        private Vector3 CoM => this.vessel.CoM;

        public float Radius => Mathf.Max(this.altASL * radiusAltASL, radiusMin);

        private Vector3 Up => (this.CoM - this.MainBody.position).normalized;

        private Vector3 North => this.MainBody.transform.up;

        private Vector3 South => -this.North;

        private Vector3 West => -this.East;

        private Vector3 East => Vector3.Cross(this.Up, this.North);
        #endregion

        #region Constructors
        public GroundContactCalculator()
        {
            this.distance = new Dictionary<Direction, float>(5)
            #region Values
            {
                { Direction.CENTER, 0 },
                { Direction.NORTH,  0 },
                { Direction.SOUTH,  0 },
                { Direction.WEST,   0 },
                { Direction.EAST,   0 }
            };
            #endregion
        }
        #endregion

        #region Methods
        public float Distance(Direction dir)
        {
            return this.distance[dir];
        }

        public float Slope(Direction dir)
        {
            return Mathf.Atan((this.DistCenter - this.distance[dir]) / this.Radius);
        }

        public void OnUpdate(Vessel v, float heightOffset, bool slope)
        {
            this.vessel = v;
            this.altASL = (float)this.MainBody.GetAltitude(this.CoM) - heightOffset;
            float res;
            if (GroundContactDetect(this.CoM, this.altASL, out res))
            {
                this.DistCenter = res;
                this.DistNorth = GroundContactDetect(this.CoM + (this.North * this.Radius));
                this.DistSouth = GroundContactDetect(this.CoM + (this.South * this.Radius));
                this.DistWest = GroundContactDetect(this.CoM + (this.West * this.Radius));
                this.DistEast = GroundContactDetect(this.CoM + (this.East * this.Radius));
                this.terrain = true;
            }
            else
            {
                this.DistCenter = res;
                this.terrain = false;
            }

            for (int i = 0; i < 5; i++)
            {
                Direction dir = (Direction)i;
                if (this.distance[dir] > this.altASL && this.MainBody.ocean) { this.distance[dir] = this.altASL; }
            }
        }

        //From MechJeb
        private bool DistanceToGround(Vector3 origin, Vector3 direction, float maxDistance, out float result)
        {
            RaycastHit hit;
            if (Physics.Raycast(origin, direction, out hit, maxDistance, 1 << 15))
            {
                result = hit.distance;
                return true;
            }
            result = 0;
            return false;
        }

        //TODO: scrap this pqs bullshit and implement vessel.pqsaltitude
        private bool PQSAltitude(Vector3 origin, out float result)
        {
            if (this.MainBody.pqsController != null)
            {
                result = (float)(this.altASL - (this.MainBody.pqsController.GetSurfaceHeight(QuaternionD.AngleAxis(this.MainBody.GetLongitude(origin), Vector3d.down) * QuaternionD.AngleAxis(this.MainBody.GetLatitude(origin), Vector3d.forward) * Vector3d.right) - this.MainBody.pqsController.radius));
                return true;
            }
            result = 0;
            return false;
        }

        private bool GroundContactDetect(Vector3 origin, float defaultResult, out float result)
        {
            if (DistanceToGround(origin, (this.MainBody.position - origin).normalized, this.altASL + physicsContactDistanceAdd, out result)) { return true; }
            if (PQSAltitude(origin, out result)) { return true; }
            result = defaultResult;
            return false;
        }

        private float GroundContactDetect(Vector3 origin)
        {
            float res;
            GroundContactDetect(origin, 0, out res);
            return res;
        }
        #endregion
    }
}