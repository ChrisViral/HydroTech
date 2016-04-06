using System.Collections.Generic;
using HydroTech_FC;
using HydroTech_RCS.Constants.Autopilots.Landing;
using UnityEngine;

namespace HydroTech_RCS.Autopilots.Modules
{
    public class DetectorGroundContact
    {
        public enum Direction
        {
            CENTER,
            NORTH,
            SOUTH,
            WEST,
            EAST
        }

        protected const float radiusAltAsl = Position.Gcd.radiusAltAsl;
        protected const float radiusMin = Position.Gcd.radiusMin;

        protected const float physicsContactDistanceAdd = Position.Gcd.physicsContactDistanceAdd;
        protected float altAsl;
        protected Dictionary<Direction, float> distance = new Dictionary<Direction, float>();
        public bool terrain;

        protected Vessel vessel;

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

        public float SlopeNorth
        {
            get { return HMaths.Atan((this.DistCenter - this.DistNorth) / this.Radius); }
        }

        public float SlopeSouth
        {
            get { return HMaths.Atan((this.DistCenter - this.DistSouth) / this.Radius); }
        }

        public float SlopeWest
        {
            get { return HMaths.Atan((this.DistCenter - this.DistWest) / this.Radius); }
        }

        public float SlopeEast
        {
            get { return HMaths.Atan((this.DistCenter - this.DistEast) / this.Radius); }
        }

        protected CelestialBody MainBody
        {
            get { return this.vessel.mainBody; }
        }

        protected Vector3 CoM
        {
            get { return this.vessel.CoM; }
        }

        public float Radius
        {
            get { return HMaths.Max(this.altAsl * radiusAltAsl, radiusMin); }
        }

        protected Vector3 Up
        {
            get { return (this.CoM - this.MainBody.position).normalized; }
        }

        protected Vector3 North
        {
            get { return this.MainBody.transform.up; }
        }

        protected Vector3 South
        {
            get { return -this.North; }
        }

        protected Vector3 West
        {
            get { return HMaths.CrossProduct(this.Up, this.North); }
        }

        protected Vector3 East
        {
            get { return -this.West; }
        }

        public DetectorGroundContact()
        {
            this.distance.Add(Direction.CENTER, 0.0F);
            this.distance.Add(Direction.NORTH, 0.0F);
            this.distance.Add(Direction.SOUTH, 0.0F);
            this.distance.Add(Direction.WEST, 0.0F);
            this.distance.Add(Direction.EAST, 0.0F);
        }

        public float Distance(Direction dir)
        {
            return this.distance[dir];
        }

        public float Slope(Direction dir)
        {
            return HMaths.Atan((this.DistCenter - this.distance[dir]) / this.Radius);
        }

        public virtual void OnUpdate(Vessel v, float heightOffset, bool slope)
        {
            this.vessel = v;
            this.altAsl = (float)this.MainBody.GetAltitude(this.CoM) - heightOffset;
            float res;
            if (GroundContactDetect(this.CoM, this.altAsl, out res))
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
        }

        // Following methods learned from MechJeb
        protected bool PhysicsContactMethod(Vector3 origin, Vector3 direction, float maxDistance, // the max distance of detection, to pass into Physics.Raycast
                                            out float result)
        {
            RaycastHit sfc;
            if (Physics.Raycast(origin, direction, out sfc, maxDistance, 1 << 15))
            {
                result = sfc.distance;
                return true;
            }
            result = 0.0F;
            return false;
        }

        protected bool MainBodyTerrainMethod(Vector3 origin, out float result)
        {
            if (this.MainBody.pqsController != null)
            {
                result = (float)(this.altAsl - (this.MainBody.pqsController.GetSurfaceHeight(QuaternionD.AngleAxis(this.MainBody.GetLongitude(origin), Vector3d.down) * QuaternionD.AngleAxis(this.MainBody.GetLatitude(origin), Vector3d.forward) * Vector3d.right) - this.MainBody.pqsController.radius));
                return true;
            }
            result = 0.0F;
            return false;
        }

        protected bool GroundContactDetect(Vector3 origin, float defaultResult, out float result)
        {
            if (PhysicsContactMethod(origin, (this.MainBody.position - origin).normalized, this.altAsl + physicsContactDistanceAdd, out result)) { return true; }
            if (MainBodyTerrainMethod(origin, out result)) { return true; }
            result = defaultResult;
            return false;
        }

        protected float GroundContactDetect(Vector3 origin)
        {
            float res;
            GroundContactDetect(origin, 0.0F, out res);
            return res;
        }
    }

    public class TrueAltitudeDetector : DetectorGroundContact
    {
        public override void OnUpdate(Vessel v, float heightOffset, bool slope)
        {
            base.OnUpdate(v, heightOffset, slope);
            for (Direction dir = Direction.CENTER; dir <= Direction.EAST; dir++) { if ((this.distance[dir] > this.altAsl) && this.MainBody.ocean) { this.distance[dir] = this.altAsl; } }
        }
    }
}