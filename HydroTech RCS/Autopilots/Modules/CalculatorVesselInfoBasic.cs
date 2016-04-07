using System.Collections.Generic;
using HydroTech_FC;
using UnityEngine;

namespace HydroTech_RCS.Autopilots.Modules
{
    public class CalculatorVesselInfoBasic
    {
        protected Vector3 coM;
        protected bool editor;

        protected float mass;
        public Matrix3x3 moI = new Matrix3x3();

        protected List<Part> partList;

        protected Vessel targetVessel;
        protected Vector3 transformDown;
        protected Vector3 transformForward;
        protected Vector3 transformRight;

        public bool Editor
        {
            get { return this.editor; }
            set
            {
                if (value)
                {
                    this.TargetVessel = null;
                    this.partList = EditorLogic.SortedShipList;
                    this.transformRight = EditorLogic.startPod.transform.right;
                    this.transformDown = EditorLogic.startPod.transform.forward;
                    this.transformForward = EditorLogic.startPod.transform.up;
                }
                else
                {
                    this.partList = null;
                }
                this.editor = value;
            }
        }

        public Vessel TargetVessel
        {
            get { return this.targetVessel; }
            set
            {
                if (this.Editor) { return; }
                if (value != null) { this.partList = value.parts; }
                else
                {
                    this.partList = null;
                }
                this.targetVessel = value;
            }
        }

        public float Mass
        {
            get { return this.mass; }
            protected set { this.mass = value; }
        }

        public void SetVessel(Vessel targetVessel)
        {
            this.Editor = false;
            this.TargetVessel = targetVessel;
            this.transformRight = this.TargetVessel.ReferenceTransform.right;
            this.transformDown = this.TargetVessel.ReferenceTransform.forward;
            this.transformForward = this.TargetVessel.ReferenceTransform.up;
        }

        protected static float PartMass(Part part)
        {
            return part.mass + part.GetResourceMass();
        }

        protected virtual void Calculate()
        {
            this.Mass = 0;
            this.moI.Reset();
            Vector3 massPos = new Vector3();
            foreach (Part p in this.partList)
            {
                if (p.physicalSignificance != Part.PhysicalSignificance.NONE)
                {
                    this.Mass += PartMass(p);
                    massPos += p.Rigidbody.worldCenterOfMass * PartMass(p);
                }
            }
            this.coM = massPos / this.Mass;
            foreach (Part p in this.partList)
            {
                Vector3 r = SwitchTransformCalculator.VectorTransform(p.Rigidbody.worldCenterOfMass - this.coM, this.transformRight, this.transformDown, this.transformForward);
                if (p.physicalSignificance != Part.PhysicalSignificance.NONE)
                {
                    this.moI.m00 += ((r.y * r.y) + (r.z * r.z)) * PartMass(p);
                    this.moI.m11 += ((r.z * r.z) + (r.x * r.x)) * PartMass(p);
                    this.moI.m22 += ((r.x * r.x) + (r.y * r.y)) * PartMass(p);
                    float ixy = -r.x * r.y * PartMass(p);
                    this.moI.m01 += ixy;
                    this.moI.m10 += ixy;
                    float iyz = -r.y * r.z * PartMass(p);
                    this.moI.m12 += iyz;
                    this.moI.m21 += iyz;
                    float izx = -r.z * r.x * PartMass(p);
                    this.moI.m20 += izx;
                    this.moI.m02 += izx;
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
            this.Editor = true;
            Calculate();
        }
    }
}