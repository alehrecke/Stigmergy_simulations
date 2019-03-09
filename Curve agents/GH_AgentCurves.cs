using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;



namespace MeshGrowth
{
    public class GH_AgentCurves : GH_Component
    {
        List<CurveAgent> agents;

        public GH_AgentCurves() : base("AgentCurves", "AgentCurves", "AgentCurves", "McMuffin", "AgentCurves")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("StartingPoints", "StartingPoints", "StartingPoints", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Reset", "Reset", "Reset", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Grow", "Grow", "Grow", GH_ParamAccess.item);
            pManager.AddBoxParameter("SiteExtents", "SiteExtents", "SiteExtents", GH_ParamAccess.item);
            pManager.AddNumberParameter("MaxVertexCount", "MaxVertexCount", "MaxVertexCount", GH_ParamAccess.item);
            pManager.AddNumberParameter("NeighbourCount", "NeighbourCount", "NeighbourCount", GH_ParamAccess.item);
            pManager.AddNumberParameter("AttractionLength", "AttractionLength", "AttractionLength", GH_ParamAccess.item);
            pManager.AddNumberParameter("AttractionMin", "AttractionMin", "AttractionMin", GH_ParamAccess.item);
            pManager.AddNumberParameter("AttractionMax", "AttractionMax", "AttractionMax", GH_ParamAccess.item);
            pManager.AddNumberParameter("VerticalLength", "VerticalLength", "VerticalLength", GH_ParamAccess.item);
            pManager.AddNumberParameter("FlowLength", "FlowLength", "FlowLength", GH_ParamAccess.item);
            pManager.AddNumberParameter("FlowRange", "FlowRange", "FlowRange", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves", "Curves", "Curves", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> allNurbsCurves = new List<Curve>();
            RTree rTree = new RTree();

            List<Point3d> iStartingPoints = new List<Point3d>();
            bool iReset = false;
            bool iGrow = false;
            Box iSiteExtents = new Box();
            double iMaxVertexCount = 0;
            double iNeighbourCount = 0;
            double iAttractionLength = 0.0;
            double iAttractionMin = 0.0;
            double iAttractionMax = 0.0;
            double iVerticalLength = 0.0;
            double iFlowLength = 0.0;
            double iFlowRange = 0.0;

            DA.GetDataList("StartingPoints", iStartingPoints);
            DA.GetData("Reset", ref iReset);
            DA.GetData("Grow", ref iGrow);
            DA.GetData("SiteExtents", ref iSiteExtents);
            DA.GetData("MaxVertexCount", ref iMaxVertexCount);
            DA.GetData("NeighbourCount", ref iNeighbourCount);
            DA.GetData("AttractionLength", ref iAttractionLength);
            DA.GetData("AttractionMin", ref iAttractionMin);
            DA.GetData("AttractionMax", ref iAttractionMax);
            DA.GetData("VerticalLength", ref iVerticalLength);
            DA.GetData("FlowLength", ref iFlowLength);
            DA.GetData("FlowRange", ref iFlowRange);

            if (agents == null || iReset)
            {
                agents = new List<CurveAgent>();
                foreach (Point3d item in iStartingPoints) { agents.Add(new CurveAgent(item)); }
            }

            for (int i = 0; i < agents.Count; i++)
            {
                allNurbsCurves.Add(agents[i].AgentPolyline.ToNurbsCurve());
                rTree.Insert(agents[i].AgentPolyline.ToNurbsCurve().GetBoundingBox(false), i);
            }

                for (int i = 0; i < agents.Count; i ++) {

                agents[i].iGrow = iGrow;
                agents[i].iSiteExtents = iSiteExtents;
                agents[i].iMaxVertexCount = (int)iMaxVertexCount;
                agents[i].iNeighbourCount = (int)iNeighbourCount;
                agents[i].iAttractionLength = iAttractionLength;
                agents[i].iAttractionMin = iAttractionMin;
                agents[i].iAttractionMax = iAttractionMax;
                agents[i].iVerticalLength = iVerticalLength;
                agents[i].iFlowLength = iFlowLength;
                agents[i].iFlowRange = iFlowRange;
                agents[i].CurveIndex = i;
                agents[i].allNurbsCurves = allNurbsCurves;
                agents[i].rTree = rTree;
                agents[i].Update();
            }

            DA.SetDataList("Curves", allNurbsCurves);
        }

        protected override System.Drawing.Bitmap Icon { get { return null; } }

        public override Guid ComponentGuid { get { return new Guid("8f45cb92-e6ce-43df-979c-de5cde52f97d"); } }

    }
}
