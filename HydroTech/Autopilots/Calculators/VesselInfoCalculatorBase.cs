using System.Collections.Generic;
using HydroTech.Utils;
using UnityEngine;

namespace HydroTech.Autopilots.Calculators
{
    public class VesselInfoCalculatorBase
    {
        #region Fields
        public Matrix3x3 moI = new Matrix3x3();
        protected List<Part> partList;
        protected Vector3 CoM, transformDown, transformForward, transformRight;
        #endregion

        #region Properties
        protected bool editor;
        public bool Editor
        {
            get { return this.editor; }
            set
            {
                if (value)
                {
                    this.TargetVessel = null;
                    this.partList = EditorLogic.SortedShipList;
                    //"partList[0]" used to be EditoryLogic.startPod
                    this.transformRight = this.partList[0].transform.right;
                    this.transformDown = this.partList[0].transform.forward;
                    this.transformForward = this.partList[0].transform.up;
                }
                else { this.partList = null; }
                this.editor = value;
            }
        }

        protected Vessel targetVessel;
        public Vessel TargetVessel
        {
            get { return this.targetVessel; }
            set
            {
                if (this.Editor) { return; }
                this.partList = value != null ? value.parts : null;
                this.targetVessel = value;
            }
        }

        protected float mass;
        public float Mass
        {
            get { return this.mass; }
            protected set { this.mass = value; }
        }
        #endregion

        #region Methods
        public void SetVessel(Vessel targetVessel)
        {
            this.Editor = false;
            this.TargetVessel = targetVessel;
            this.transformRight = this.TargetVessel.ReferenceTransform.right;
            this.transformDown = this.TargetVessel.ReferenceTransform.forward;
            this.transformForward = this.TargetVessel.ReferenceTransform.up;
        }
        #endregion

        #region Static methods
        //TODO: change this into a part extension
        protected static float PartMass(Part part)
        {
            return part.mass + part.GetResourceMass();
        }
        #endregion

        #region Virtual methods
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
                    massPos += p.WCoM * PartMass(p);
                }
            }
            this.CoM = massPos / this.Mass;
            foreach (Part p in this.partList)
            {
                Vector3 r = SwitchTransformCalculator.VectorTransform(p.WCoM - this.CoM, this.transformRight, this.transformDown, this.transformForward);
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
        #endregion
    }
}