using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace CurveAgents
{
    public class GH_Trails : GH_Component
    {
        List<TrailAgent> AgentTrails;

        public GH_Trails() : base("Trails", "Trails", "Description", "Category", "Subcategory") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Reset", "Reset", "Reset", GH_ParamAccess.item);
            pManager.AddPointParameter("StartingPoints", "StartingPoints", "StartingPoints", GH_ParamAccess.list);
            pManager.AddVectorParameter("StartingVelocities", "StartingVelocities", "StartingVelocities", GH_ParamAccess.list);
            pManager.AddNumberParameter("TrailLength", "TrailLength", "TrailLength", GH_ParamAccess.item);
            pManager.AddNumberParameter("NeighbourCount", "NeighbourCount", "NeighbourCount", GH_ParamAccess.item);
            pManager.AddNumberParameter("SearchRange", "SearchRange", "SearchRange", GH_ParamAccess.item);
            pManager.AddNumberParameter("xExtents", "xExtents", "xExtents", GH_ParamAccess.item);
            pManager.AddNumberParameter("yExtents", "yExtents", "yExtents", GH_ParamAccess.item);
            pManager.AddNumberParameter("AttractMin", "AttractMin", "AttractMin", GH_ParamAccess.item);
            pManager.AddNumberParameter("AttractMax", "AttractMax", "AttractMax", GH_ParamAccess.item);
            pManager.AddNumberParameter("AttractWeight", "AttractWeight", "AttractWeight", GH_ParamAccess.item);
            pManager.AddNumberParameter("RepelMin", "RepelMin", "RepelMin", GH_ParamAccess.item);
            pManager.AddNumberParameter("RepelMax", "RepelMax", "RepelMax", GH_ParamAccess.item);
            pManager.AddNumberParameter("RepelWeight", "RepelWeight", "RepelWeight", GH_ParamAccess.item);
            pManager.AddNumberParameter("AlignMin", "AlignMin", "AlignMin", GH_ParamAccess.item);
            pManager.AddNumberParameter("AlignMax", "AlignMax", "AlignMax", GH_ParamAccess.item);
            pManager.AddNumberParameter("AlignWeight", "AlignWeight", "AlignWeight", GH_ParamAccess.item);
            pManager.AddNumberParameter("Steering", "Steering", "Steering", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Points", "Points", GH_ParamAccess.list);
            pManager.AddVectorParameter("Velocities", "Velocities", "Velocities", GH_ParamAccess.list);
            pManager.AddCurveParameter("Segments", "Segments", "Segments", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RTree rTree = new RTree();
            List<Point3d> allPositions = new List<Point3d>();
            List<Vector3d> allVelocities = new List<Vector3d>();
            List<LineCurve> allSegments = new List<LineCurve>();


            List<PointAgent> allAgents = new List<PointAgent>();

            bool iReset = false;
            List<Point3d> iStartingPoints = new List<Point3d>();
            List<Vector3d> iStartingVelocities = new List<Vector3d>();
            double iTrailLength = 0.0;
            double iNeighbourCount = 0.0;
            double iSearchRange = 0;
            double ixExtents = 0.0;
            double iyExtents = 0.0;
            double iAttractMin = 0.0;
            double iAttractMax = 0.0;
            double iAttractWeight = 0.0;
            double iRepelMin = 0.0;
            double iRepelMax = 0.0;
            double iRepelWeight = 0.0;
            double iAlignMin = 0.0;
            double iAlignMax = 0.0;
            double iAlignWeight = 0.0;
            double iSteering = 0.0;

            DA.GetData("Reset", ref iReset);
            DA.GetDataList("StartingPoints", iStartingPoints);
            DA.GetDataList("StartingVelocities", iStartingVelocities);
            DA.GetData("TrailLength", ref iTrailLength);
            DA.GetData("NeighbourCount", ref iNeighbourCount);
            DA.GetData("SearchRange", ref iSearchRange);
            DA.GetData("xExtents", ref ixExtents);
            DA.GetData("yExtents", ref iyExtents);
            DA.GetData("AttractMin", ref iAttractMin);
            DA.GetData("AttractMax", ref iAttractMax);
            DA.GetData("AttractWeight", ref iAttractWeight);
            DA.GetData("RepelMin", ref iRepelMin);
            DA.GetData("RepelMax", ref iRepelMax);
            DA.GetData("RepelWeight", ref iRepelWeight);
            DA.GetData("AlignMin", ref iAlignMin);
            DA.GetData("AlignMax", ref iAlignMax);
            DA.GetData("AlignWeight", ref iAlignWeight);
            DA.GetData("Steering", ref iSteering);

            //instantiate the list of point agents
            if (AgentTrails == null || iReset)
            {
                AgentTrails = new List<TrailAgent>();
                for (int i = 0; i < iStartingPoints.Count; i++) AgentTrails.Add(new TrailAgent(iStartingPoints[i], iStartingVelocities[i]));
            }

            //pass all of the point agents into the list.
            for (int i = 0; i < AgentTrails.Count; i++)
            {
                List<PointAgent> thisTrail = AgentTrails[i].Trail;
                for (int j = 0; j < thisTrail.Count; j++) { allAgents.Add(thisTrail[j]); }
            }

            //pass all of the positions into the Rtree.
            for (int i = 0; i < allAgents.Count; i++) { rTree.Insert(allAgents[i].Position, i); }

            //Pass in values // update positions.
            for (int i = 0; i < AgentTrails.Count; i++)
            {
                AgentTrails[i].rtree = rTree;
                AgentTrails[i].AllAgents = allAgents;
                AgentTrails[i].TrailLength = (int)iTrailLength;
                AgentTrails[i].NeighbourCount = (int)iNeighbourCount;
                AgentTrails[i].SearchRange = iSearchRange;
                AgentTrails[i].xExtents = ixExtents;
                AgentTrails[i].yExtents = iyExtents;
                AgentTrails[i].AttractMin = iAttractMin;
                AgentTrails[i].AttractMax = iAttractMax;
                AgentTrails[i].AttractWeight = iAttractWeight;
                AgentTrails[i].RepelMin = iRepelMin;
                AgentTrails[i].RepelMax = iRepelMax;
                AgentTrails[i].RepelWeight = iRepelWeight;
                AgentTrails[i].AlignMin = iAlignMin;
                AgentTrails[i].AlignMax = iAlignMax;
                AgentTrails[i].AlignWeight = iAlignWeight;
                AgentTrails[i].Steering = iSteering;
                AgentTrails[i].Update();
            }

            //add positions and velocities to the display lists.
            allPositions = new List<Point3d>();
            allVelocities = new List<Vector3d>();
            allSegments = new List<LineCurve>();

            for (int i = 0; i < AgentTrails.Count; i++)
            {
                List<PointAgent> thisTrail = AgentTrails[i].Trail;
                List<LineCurve> thisTrailSegments = AgentTrails[i].TrailSegments;

                for (int j = 0; j < thisTrail.Count; j++)
                {
                    allPositions.Add(thisTrail[j].Position);
                    allVelocities.Add(thisTrail[j].Velocity);
                }

                for (int j = 0; j < thisTrailSegments.Count; j++)
                {
                    allSegments.Add(thisTrailSegments[j]);
                }
            }

            DA.SetDataList("Points", allPositions);
            DA.SetDataList("Velocities", allVelocities);
            DA.SetDataList("Segments", allSegments);
        }

        protected override System.Drawing.Bitmap Icon { get { return null; } }

        public override Guid ComponentGuid
        {
            get { return new Guid("d2d59a4f-cfe3-4f7b-b3e2-b3cc038a8084"); }
        }
    }
}