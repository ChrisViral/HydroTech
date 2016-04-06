using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Autopilots.Modules
{
    using UnityEngine;
    using HydroTech_FC;
    using Constants.Autopilots.Landing;

    public class DetectorGroundContact
    {
        public enum DIRECTION { CENTER, NORTH, SOUTH, WEST, EAST }

        protected Vessel vessel = null;
        public bool terrain = false;
        protected Dictionary<DIRECTION, float> distance = new Dictionary<DIRECTION, float>();

        public float Distance(DIRECTION dir) { return distance[dir]; }
        public float DistCenter
        {
            get { return distance[DIRECTION.CENTER]; }
            protected set { distance[DIRECTION.CENTER] = value; }
        }
        public float DistNorth
        {
            get { return distance[DIRECTION.NORTH]; }
            protected set { distance[DIRECTION.NORTH] = value; }
        }
        public float DistSouth
        {
            get { return distance[DIRECTION.SOUTH]; }
            protected set { distance[DIRECTION.SOUTH] = value; }
        }
        public float DistWest
        {
            get { return distance[DIRECTION.WEST]; }
            protected set { distance[DIRECTION.WEST] = value; }
        }
        public float DistEast
        {
            get { return distance[DIRECTION.EAST]; }
            protected set { distance[DIRECTION.EAST] = value; }
        }

        public float Slope(DIRECTION dir) { return HMaths.Atan((DistCenter - distance[dir]) / Radius); }
        public float SlopeNorth { get { return HMaths.Atan((DistCenter - DistNorth) / Radius); } }
        public float SlopeSouth { get { return HMaths.Atan((DistCenter - DistSouth) / Radius); } }
        public float SlopeWest { get { return HMaths.Atan((DistCenter - DistWest) / Radius); } }
        public float SlopeEast { get { return HMaths.Atan((DistCenter - DistEast) / Radius); } }

        public DetectorGroundContact()
        {
            distance.Add(DIRECTION.CENTER, 0.0F);
            distance.Add(DIRECTION.NORTH, 0.0F);
            distance.Add(DIRECTION.SOUTH, 0.0F);
            distance.Add(DIRECTION.WEST, 0.0F);
            distance.Add(DIRECTION.EAST, 0.0F);
        }

        virtual public void OnUpdate(Vessel v, float heightOffset, bool slope)
        {
            vessel = v;
            altASL = (float)MainBody.GetAltitude(CoM) - heightOffset;
            float res;
            if (GroundContactDetect(CoM, altASL, out res))
            {
                DistCenter = res;
                DistNorth = GroundContactDetect(CoM + North * Radius);
                DistSouth = GroundContactDetect(CoM + South * Radius);
                DistWest = GroundContactDetect(CoM + West * Radius);
                DistEast = GroundContactDetect(CoM + East * Radius);
                terrain = true;
            }
            else
            {
                DistCenter = res;
                terrain = false;
            }
        }

        protected CelestialBody MainBody { get { return vessel.mainBody; } }
        protected Vector3 CoM { get { return vessel.CoM; } }
        protected float altASL = 0.0F;
        protected const float Radius_altASL = Position.GCD.Radius_altASL;
        protected const float Radius_Min = Position.GCD.Radius_Min;
        public float Radius { get { return HMaths.Max(altASL * Radius_altASL, Radius_Min); } }

        protected Vector3 Up { get { return (CoM - MainBody.position).normalized; } }
        protected Vector3 North { get { return MainBody.transform.up; } }
        protected Vector3 South { get { return -North; } }
        protected Vector3 West { get { return HMaths.CrossProduct(Up, North); } }
        protected Vector3 East { get { return -West; } }

        // Following methods learned from MechJeb
        protected bool PhysicsContactMethod(
            Vector3 origin,
            Vector3 direction,
            float maxDistance, // the max distance of detection, to pass into Physics.Raycast
            out float result
            )
        {
            RaycastHit sfc;
            if (Physics.Raycast(origin, direction, out sfc, maxDistance, 1 << 15))
            {
                result = sfc.distance;
                return true;
            }
            else
            {
                result = 0.0F;
                return false;
            }
        }
        protected bool MainBodyTerrainMethod(Vector3 origin, out float result)
        {
            if (MainBody.pqsController != null)
            {
                result = (float)(
                    altASL - (MainBody.pqsController.GetSurfaceHeight(
                        QuaternionD.AngleAxis(MainBody.GetLongitude(origin), Vector3d.down)
                            * QuaternionD.AngleAxis(MainBody.GetLatitude(origin),
                        Vector3d.forward) * Vector3d.right
                        ) - MainBody.pqsController.radius)
                    );
                return true;
            }
            else
            {
                result = 0.0F;
                return false;
            }
        }

        protected const float PhysicsContactDistanceAdd = Position.GCD.PhysicsContactDistanceAdd;
        protected bool GroundContactDetect(Vector3 origin, float defaultResult, out float result)
        {
            if (PhysicsContactMethod(
                    origin,
                    (MainBody.position - origin).normalized,
                    altASL + PhysicsContactDistanceAdd,
                    out result
                ))
                return true;
            else if (MainBodyTerrainMethod(origin, out result))
                return true;
            else
            {
                result = defaultResult;
                return false;
            }
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
            for (DIRECTION dir = DIRECTION.CENTER; dir <= DIRECTION.EAST; dir++)
                if (distance[dir] > altASL && MainBody.ocean)
                    distance[dir] = altASL;
        }
    }
}