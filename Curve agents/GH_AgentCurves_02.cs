using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;



namespace MeshGrowth
{
    public class GH_AgentCurves_02 : GH_Component
    {
        List<CurveAgent> agents;

        public GH_AgentCurves_02() : base("Stigmergy", "Stigmergy", "AgentCurves", "AgentCurves", "AgentCurves")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("StartingPoints", "StartingPoints", "StartingPoints", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Reset", "Reset", "Reset", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Grow", "Grow", "Grow", GH_ParamAccess.item);
            pManager.AddBoxParameter("Extents", "Extents", "Extents", GH_ParamAccess.item);
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
            List<Point3d> iStartingPoints = new List<Point3d>();
            bool iReset = false;
            bool iGrow = false;
            Curve iExtents;
            double iMaxVertexCount = 0.0;
            double iNeighbourCount = 0.0;
            double iRepelMin = 0.0;
            double iRepelMax = 0.0;
            double iAttractionMin = 0.0;
            double iAttractionMax = 0.0;
            double AlignMin = 0.0;
            double AlignMax = 0.0;


        }

        protected override System.Drawing.Bitmap Icon { get { return null; } }

        public override Guid ComponentGuid { get { return new Guid("1a709b5d-5618-4447-80f5-17238fbf466f"); } }

    }
}