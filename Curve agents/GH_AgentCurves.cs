using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;



namespace CurveAgents
{
    public class GH_AgentCurves : GH_Component
    {
        List<StigmergyAgent> agents;

        public GH_AgentCurves() : base("Stigmergy", "Stigmergy", "AgentCurves", "AgentCurves", "AgentCurves")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("StartingPoints", "StartingPoints", "StartingPoints", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Reset", "Reset", "Reset", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Grow", "Grow", "Grow", GH_ParamAccess.item);
            pManager.AddNumberParameter("XExtents", "XExtents", "XExtents", GH_ParamAccess.item);
            pManager.AddNumberParameter("YExtents", "YExtents", "YExtents", GH_ParamAccess.item);
            pManager.AddNumberParameter("MaxVertexCount", "MaxVertexCount", "MaxVertexCount", GH_ParamAccess.item);
            pManager.AddNumberParameter("NeighbourCount", "NeighbourCount", "NeighbourCount", GH_ParamAccess.item);
            pManager.AddNumberParameter("RepelMin", "RepelMin", "RepelMin", GH_ParamAccess.item);
            pManager.AddNumberParameter("RepelMax", "RepelMax", "RepelMax", GH_ParamAccess.item);
            pManager.AddNumberParameter("RepelWeight", "RepelWeight", "RepelWeight", GH_ParamAccess.item);
            pManager.AddNumberParameter("AttractionMin", "AttractionMin", "AttractionMin", GH_ParamAccess.item);
            pManager.AddNumberParameter("AttractionMax", "AttractionMax", "AttractionMax", GH_ParamAccess.item);
            pManager.AddNumberParameter("AttractionWeight", "AttractionWeight", "AttractionWeight", GH_ParamAccess.item);
            pManager.AddNumberParameter("AlignMin", "AlignMin", "AlignMin", GH_ParamAccess.item);
            pManager.AddNumberParameter("AlignMax", "AlignMax", "AlignMax", GH_ParamAccess.item);
            pManager.AddNumberParameter("AlignWeight", "AlignWeight", "AlignWeight", GH_ParamAccess.item);
            pManager.AddNumberParameter("SegmentLength", "SegmentLength", "SegmentLength", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("segments", "segments", "segments", GH_ParamAccess.list);
            pManager.AddPointParameter("positions", "positions", "positions", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Random rand = new Random();
            List<Point3d> displayPoints = new List<Point3d>();
            RTree rTree = new RTree();
            List<Point3d> allPoints = new List<Point3d>();
            List<Vector3d> allTangents = new List<Vector3d>();
            List<LineCurve> allSegments = new List<LineCurve>();
            List<double> allCounts = new List<double>();

            List<Point3d> iStartingPoints = new List<Point3d>();
            bool iReset = false;
            bool iGrow = false;
            double iXExtents = 0.0;
            double iYExtents = 0.0;
            double iMaxVertexCount = 0.0;
            double iNeighbourCount = 0.0;
            double iRepelMin = 0.0;
            double iRepelMax = 0.0;
            double iRepelWeight = 0.0;
            double iAttractionMin = 0.0;
            double iAttractionMax = 0.0;
            double iAttractionWeight = 0.0;
            double iAlignMin = 0.0;
            double iAlignMax = 0.0;
            double iAlignWeight = 0.0;
            double iSegmentLength = 0.0;

            DA.GetDataList("StartingPoints", iStartingPoints);
            DA.GetData("Reset", ref iReset);
            DA.GetData("Grow", ref iGrow);
            DA.GetData("XExtents", ref iXExtents);
            DA.GetData("YExtents", ref iYExtents);
            DA.GetData("MaxVertexCount", ref iMaxVertexCount);
            DA.GetData("NeighbourCount", ref iNeighbourCount);
            DA.GetData("RepelMin", ref iRepelMin);
            DA.GetData("RepelMax", ref iRepelMax);
            DA.GetData("RepelWeight", ref iRepelWeight);
            DA.GetData("AttractionMin", ref iAttractionMin);
            DA.GetData("AttractionMax", ref iAttractionMax);
            DA.GetData("AttractionWeight", ref iAttractionWeight);
            DA.GetData("AlignMin", ref iAlignMin);
            DA.GetData("AlignMax", ref iAlignMax);
            DA.GetData("AlignWeight", ref iAlignWeight);
            DA.GetData("SegmentLength", ref iSegmentLength);

            if (iReset || agents == null) {
                agents = new List<StigmergyAgent>();
                for (int i = 0; i < iStartingPoints.Count; i++) {

                    //Vector3d randomVector = new Vector3d(rand.Next(100,100), rand.Next(100,100),0);
                    Vector3d randomVector = new Vector3d(rand.Next(-10,100), rand.Next(-10,100),0);
                    randomVector.Unitize();
                    randomVector = randomVector * iSegmentLength;
                    agents.Add(new StigmergyAgent(iStartingPoints[i], randomVector));
                }
            }


            for (int i = 0; i < agents.Count; i++) {
                List<Point3d> thisAgentList = agents[i].AgentList;
                List<Vector3d> thisAgentTangents = agents[i].AgentDirections;
                double thisCount = agents[i].AgentList.Count;

                for (int j = 0; j < thisAgentList.Count; j++) {
                    Point3d thisPoint = thisAgentList[j];
                    Vector3d thisTangent = thisAgentTangents[j];
                    allPoints.Add(thisPoint);
                    allTangents.Add(thisTangent);
                    allCounts.Add(thisCount);
                }
            }

            for (int i = 0; i < allPoints.Count; i++) { rTree.Insert(allPoints[i], i); }

            for (int i = 0; i < agents.Count; i++) {
                agents[i].AllPoints = allPoints;
                agents[i].AllTangents = allTangents;
                agents[i].rTree = rTree;
                agents[i].Grow = iGrow;
                agents[i].XExtents = iXExtents;
                agents[i].YExtents = iYExtents;
                agents[i].MaxVertexCount = (int)iMaxVertexCount;
                agents[i].NeighbourCount = (int)iNeighbourCount;
                agents[i].RepelMin = iRepelMin;
                agents[i].RepelMax = iRepelMax;
                agents[i].RepelWeight = iRepelWeight;
                agents[i].AttractionMin = iAttractionMin;
                agents[i].AttractionMax = iAttractionMax;
                agents[i].AttractionWeight = iAttractionWeight;
                agents[i].AlignMin = iAlignMin;
                agents[i].AlignMax = iAlignMax;
                agents[i].AlignWeight = iAlignWeight;
                agents[i].SegmentLength = iSegmentLength;
                agents[i].Update();
            }

               for (int i = 0; i < agents.Count; i++) {
                List<LineCurve> thisSegmentList = agents[i].AgentSegments;

                for (int j = 0; j < thisSegmentList.Count; j++) {
                    allSegments.Add(thisSegmentList[j]);
                }
            }

            DA.SetDataList("segments", allSegments);
            DA.SetDataList("positions", allPoints);

        }

        protected override System.Drawing.Bitmap Icon { get { return null; } }

        public override Guid ComponentGuid { get { return new Guid("1a709b5d-5618-4447-80f5-17238fbf466f"); } }
    }
}