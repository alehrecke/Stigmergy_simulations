using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace CurveAgents
{
    public class GH_Points : GH_Component
    {
        List<PointAgent> Agents;

        public GH_Points() : base("Points", "Points", "Description", "Category", "Subcategory") {}

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Reset", "Reset", "Reset", GH_ParamAccess.item);
            pManager.AddPointParameter("StartingPoints", "StartingPoints", "StartingPoints", GH_ParamAccess.list);
            pManager.AddVectorParameter("StartingVelocities", "StartingVelocities", "StartingVelocities", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Points", "Points", GH_ParamAccess.list);
            pManager.AddVectorParameter("Velocities", "Velocities", "Velocities", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            List<Point3d> allPositions = new List<Point3d>();
            List<Vector3d> allVelocities = new List<Vector3d>();
            bool iReset = false;
            List<Point3d> iStartingPoints = new List<Point3d>();
            List<Vector3d> iStartingVelocities = new List<Vector3d>();

            DA.GetData("Reset", ref iReset);
            DA.GetDataList("StartingPoints", iStartingPoints);
            DA.GetDataList("StartingVelocities", iStartingVelocities);

            //instantiate the list of point agents
            if (Agents == null || iReset){
                Agents = new List<PointAgent>();
                for (int i = 0; i < iStartingPoints.Count; i++){
                    Agents.Add(new PointAgent(iStartingPoints[i], iStartingVelocities[i]));
                }
            }

            //update positions
            for (int i = 0; i < Agents.Count; i++){ Agents[i].CalculatePosition(); }

            //add positions and velocities to the display list.
            for (int i = 0; i < Agents.Count; i++) {
                allPositions.Add(Agents[i].Position);
                allVelocities.Add(Agents[i].Velocity);
            }

            DA.SetDataList("Points", allPositions);
            DA.SetDataList("Velocities", allVelocities);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("54e0c782-27bd-4cbf-92c2-c6b8275638c6"); }
        }
    }
}