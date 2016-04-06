using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HydroTech_RCS.Autopilots.Modules
{
    using UnityEngine;
    using HydroTech_FC;
    using ASAS;

    public class CalculatorVesselInfoBasic
    {
        protected bool _Editor = false;
        public bool Editor
        {
            get { return _Editor; }
            set
            {
                if (value)
                {
                    TargetVessel = null;
                    partList = EditorLogic.SortedShipList;
                    transformRight = EditorLogic.startPod.transform.right;
                    transformDown = EditorLogic.startPod.transform.forward;
                    transformForward = EditorLogic.startPod.transform.up;
                }
                else
                    partList = null;
                _Editor = value;
            }
        }

        protected Vessel _TargetVessel = null;
        public Vessel TargetVessel
        {
            get { return _TargetVessel; }
            set
            {
                if (Editor)
                    return;
                if (value != null)
                    partList = value.parts;
                else
                    partList = null;
                _TargetVessel = value;
            }
        }
        protected Vector3 transformRight = new Vector3();
        protected Vector3 transformDown = new Vector3();
        protected Vector3 transformForward = new Vector3();

        public void SetVessel(Vessel targetVessel)
        {
            Editor = false;
            TargetVessel = targetVessel;
            transformRight = TargetVessel.ReferenceTransform.right;
            transformDown = TargetVessel.ReferenceTransform.forward;
            transformForward = TargetVessel.ReferenceTransform.up;
        }

        protected List<Part> partList = null;

        protected float _Mass = 0.0F;
        public float Mass
        {
            get { return _Mass; }
            protected set { _Mass = value; }
        }
        protected Vector3 CoM = new Vector3();
        public Matrix3x3 MoI = new Matrix3x3();

        protected static float PartMass(Part part) { return part.mass + part.GetResourceMass(); }

        protected virtual void Calculate()
        {
            Mass = 0;
            MoI.Reset();
            Vector3 MassPos = new Vector3();
            foreach (Part p in partList)
            {
                if (p.physicalSignificance != Part.PhysicalSignificance.NONE)
                {
                    Mass += PartMass(p);
                    MassPos += p.Rigidbody.worldCenterOfMass * PartMass(p);
                }
            }
            CoM = MassPos / Mass;
            foreach (Part p in partList)
            {
                Vector3 r = SwitchTransformCalculator.VectorTransform(
                    p.Rigidbody.worldCenterOfMass - CoM,
                    transformRight,
                    transformDown,
                    transformForward
                    );
                if (p.physicalSignificance != Part.PhysicalSignificance.NONE)
                {
                    MoI.m00 += (r.y * r.y + r.z * r.z) * PartMass(p);
                    MoI.m11 += (r.z * r.z + r.x * r.x) * PartMass(p);
                    MoI.m22 += (r.x * r.x + r.y * r.y) * PartMass(p);
                    float Ixy = -r.x * r.y * PartMass(p);
                    MoI.m01 += Ixy;
                    MoI.m10 += Ixy;
                    float Iyz = -r.y * r.z * PartMass(p);
                    MoI.m12 += Iyz;
                    MoI.m21 += Iyz;
                    float Izx = -r.z * r.x * PartMass(p);
                    MoI.m20 += Izx;
                    MoI.m02 += Izx;
                }
            }
        }

        public virtual void OnUpdate(Vessel targetVessel)
        {
            SetVessel(targetVessel);
            Calculate();
        }

        public virtual void OnEditorUpdate()
        {
            Editor = true;
            Calculate();
        }
    }
}