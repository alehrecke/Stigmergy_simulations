using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;



namespace MeshGrowth
{
    public class GH_AgentCurves_02 : GH_Component
    {
        List<StigmergyAgent> agents;

        public GH_AgentCurves_02() : base("Stigmergy", "Stigmergy", "AgentCurves", "AgentCurves", "AgentCurves")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("StartingPoints", "StartingPoints", "StartingPoints", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Reset", "Reset", "Reset", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Grow", "Grow", "Grow", GH_ParamAccess.item);
            pManager.AddCurveParameter("Extents", "Extents", "Extents", GH_ParamAccess.list);
            pManager.AddNumberParameter("MaxVertexCount", "MaxVertexCount", "MaxVertexCount", GH_ParamAccess.item);
            pManager.AddNumberParameter("NeighbourCount", "NeighbourCount", "NeighbourCount", GH_ParamAccess.item);
            pManager.AddNumberParameter("RepelMin", "RepelMin", "RepelMin", GH_ParamAccess.item);
            pManager.AddNumberParameter("RepelMax", "RepelMax", "RepelMax", GH_ParamAccess.item);
            pManager.AddNumberParameter("AttractionMin", "AttractionMin", "AttractionMin", GH_ParamAccess.item);
            pManager.AddNumberParameter("AttractionMax", "AttractionMax", "AttractionMax", GH_ParamAccess.item);
            pManager.AddNumberParameter("AlignMin", "AlignMin", "AlignMin", GH_ParamAccess.item);
            pManager.AddNumberParameter("AlignMax", "AlignMax", "AlignMax", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves", "Curves", "Curves", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> outPutCurves = new List<Curve>();
            RTree rTree = new RTree();
            List<Point3d> allPoints = new List<Point3d>();
            List<Vector3d> allTangents = new List<Vector3d>();

            List<Point3d> iStartingPoints = new List<Point3d>();

            bool iReset = false;
            bool iGrow = false;
            List<Curve> iExtents = new List<Curve>();
            double iMaxVertexCount = 0.0;
            double iNeighbourCount = 0.0;
            double iRepelMin = 0.0;
            double iRepelMax = 0.0;
            double iAttractionMin = 0.0;
            double iAttractionMax = 0.0;
            double iAlignMin = 0.0;
            double iAlignMax = 0.0;

            DA.GetDataList("StartingPoints", iStartingPoints);
            DA.GetData("Reset", ref iReset);
            DA.GetData("Grow", ref iGrow);
            DA.GetDataList("Extents", iExtents);
            DA.GetData("MaxVertexCount", ref iMaxVertexCount);
            DA.GetData("NeighbourCount", ref iNeighbourCount);
            DA.GetData("RepelMin", ref iRepelMin);
            DA.GetData("RepelMax", ref iRepelMax);
            DA.GetData("AttractionMin", ref iAttractionMin);
            DA.GetData("AttractionMax", ref iAttractionMax);
            DA.GetData("AlignMin", ref iAlignMin);
            DA.GetData("AlignMax", ref iAlignMax);


            if (iReset || agents == null) {
                agents = new List<StigmergyAgent>();
                for (int i = 0; i < iStartingPoints.Count; i++) { agents.Add(new StigmergyAgent(iStartingPoints[i])); }
            }

            for (int i = 0; i < agents.Count; i++)
            {
                Polyline thisPolyline = agents[i].AgentPolyline;
                outPutCurves.Add(thisPolyline.ToNurbsCurve());

                //get all the point on all the curves
                //get all the tangents at those points
                for (int j = 0; j < thisPolyline.Count; j++) {

                    Point3d thisPoint = thisPolyline[j];
                    allPoints.Add(thisPoint);

                    double t = thisPolyline.ClosestParameter(thisPoint);
                    Vector3d thisTangent = thisPolyline.TangentAt(t);
                    allTangents.Add(thisTangent);
                }



            }

            for (int i = 0; i < allPoints.Count; i++) { rTree.Insert(allPoints[i], i); }

            for (int i = 0; i < agents.Count; i++) {
                agents[i].AllPoints = allPoints;
                agents[i].rTree = rTree;

                agents[i].Grow = iGrow;
                agents[i].Extents = iExtents;
                agents[i].MaxVertexCount = iMaxVertexCount;
                agents[i].NeighbourCount = iNeighbourCount;
                agents[i].RepelMin = iRepelMin;
                agents[i].RepelMax = iRepelMax;
                agents[i].AttractionMin = iAttractionMin;
                agents[i].AttractionMax = iAttractionMax;
                agents[i].AlignMin = iAlignMin;
                agents[i].AlignMax = iAlignMax;

                agents[i].Update();
            }


            DA.SetDataList("Curves", outPutCurves);
        }

        protected override System.Drawing.Bitmap Icon { get { return null; } }

        public override Guid ComponentGuid { get { return new Guid("1a709b5d-5618-4447-80f5-17238fbf466f"); } }

    }
}