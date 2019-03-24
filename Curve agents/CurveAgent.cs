using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace MeshGrowth
{

    class CurveAgent
    {
        public bool iGrow;
        public Box iSiteExtents;
        public int iMaxVertexCount;
        public int iNeighbourCount;
        public double iAttractionLength;
        public double iAttractionMin;
        public double iAttractionMax;
        public double iVerticalLength;
        public double iFlowLength;
        public double iFlowRange;
        public int CurveIndex;
        public List<Curve> allNurbsCurves;
        public RTree rTree;

        private List<Vector3d> TotalWeightedMoves;
        private List<double> TotalWeights;
        public Polyline AgentPolyline;

        public CurveAgent(Point3d _startingPoint)
        {
            AgentPolyline = new Polyline();
            AgentPolyline.Add(_startingPoint);
            AgentPolyline.Add(new Point3d(_startingPoint.X+1, _startingPoint.Y +1, 0));
        }

        public void Update()
        {
            SetWeights();
            CollisionDetection();
            EdgeLength();
            SiteExtents();
            UpdateVertices();
            AddPoint();
        }

        public void SetWeights()
        {
            TotalWeightedMoves = new List<Vector3d>();
            TotalWeights = new List<double>();

            for (int i = 0; i < AgentPolyline.Count; i++) {
                TotalWeightedMoves.Add(new Vector3d(0, 0, 0));
                TotalWeights.Add(0);
            }
        }

        private void CollisionDetection()
        {
            for (int i = 0; i < AgentPolyline.Count; i++)
            {
                List<Curve> neighbours = new List<Curve>();
                List<Point3d> neighbourPoints = new List<Point3d>();
                List<Vector3d> neighbourTangents = new List<Vector3d>() ;
                List<double> distances = new List<double>();

                rTree.Search(new Sphere(AgentPolyline[i], iAttractionMax), (sender, args) => { if (CurveIndex != args.Id) { neighbours.Add(allNurbsCurves[args.Id]); } });

                if (neighbours.Count > 0)
                {
                    var tuple = GetCurveParameters(neighbours, AgentPolyline[i]);
                    neighbourPoints = tuple.Item1;
                    neighbourTangents = tuple.Item2;
                    distances = tuple.Item3;

                    var tuple2 = GetClosestPoints(iNeighbourCount, neighbourPoints, neighbourTangents, distances);
                    Point3d averageNeighbour = tuple2.Item1;
                    Vector3d averageNeighbourTangent = tuple2.Item2;

                    Vector3d toPt = averageNeighbour - AgentPolyline[i];

                    if (toPt.Length > iFlowRange)
                    {
                        averageNeighbourTangent.Unitize();
                        TotalWeightedMoves[i] += averageNeighbourTangent * iFlowLength;
                        TotalWeights[i] += 1;
                    }

                    if (iAttractionMax >= toPt.Length && toPt.Length > iAttractionMin)
                    {
                        toPt.Unitize();
                        TotalWeightedMoves[i] += toPt * iAttractionLength;
                        TotalWeights[i] += 1;
                    }
                    else
                    {
                        toPt.Unitize();
                        TotalWeightedMoves[i] += -1 * toPt * iAttractionLength;
                        TotalWeights[i] += 1;
                    }
                }
                else
                {
                    TotalWeightedMoves[i] += new Vector3d();
                    TotalWeights[i] += 0;
                }
}    
        }

        private void VerticalForce()
        {
            TotalWeightedMoves[AgentPolyline.Count - 1] += new Vector3d(0, 0, iVerticalLength);
            TotalWeights[AgentPolyline.Count - 1] += 1;
        }

        private void EdgeLength()
        {
            for (int i = 0; i < AgentPolyline.Count - 1; i++)
            {

                Vector3d move = AgentPolyline[i + 1] - AgentPolyline[i];

                if (move.Length < iAttractionMin) continue;
                else
                {
                    move = move * (move.Length - iAttractionMin) / move.Length;
                    TotalWeightedMoves[i] += move * 0.5;
                    TotalWeightedMoves[i + 1] -= move * 0.5;
                    TotalWeights[i] += 0.5;
                    TotalWeights[i + 1] += 0.5;
                }
            }
        }

        private void SiteExtents()
        {
            for (int i = 0; i < AgentPolyline.Count; i++) {

               Point3d maximum = iSiteExtents.BoundingBox.Max;
               Point3d minimum = iSiteExtents.BoundingBox.Min;

                if(AgentPolyline[i].X >= maximum.X) { AgentPolyline[i] = new Point3d(maximum.X, AgentPolyline[i].Y, AgentPolyline[i].Z); }
                if (AgentPolyline[i].X <= minimum.X) { AgentPolyline[i] = new Point3d(minimum.X, AgentPolyline[i].Y, AgentPolyline[i].Z); }

                if (AgentPolyline[i].Y >= maximum.Y) { AgentPolyline[i] = new Point3d(AgentPolyline[i].X, maximum.Y, AgentPolyline[i].Z); }
                if (AgentPolyline[i].Y <= minimum.Y) { AgentPolyline[i] = new Point3d(AgentPolyline[i].X, minimum.Y, AgentPolyline[i].Z); }

                if (AgentPolyline[i].Z >= maximum.Z) { AgentPolyline[i] = new Point3d(AgentPolyline[i].X, AgentPolyline[i].Y, maximum.Z); }
                if (AgentPolyline[i].Z <= minimum.Z) { AgentPolyline[i] = new Point3d(AgentPolyline[i].X, AgentPolyline[i].Y, minimum.Z); }

            }

            }

        private void UpdateVertices()
        {
            for (int i = 0; i < AgentPolyline.Count; i++)
            {
                if (TotalWeights[i] == 0) continue;
                else
                {
                    Vector3d move = TotalWeightedMoves[i] / TotalWeights[i];
                    if (i == 0) AgentPolyline[i] += new Vector3d(move.X, move.Y, 0);
                    else AgentPolyline[i] += move;

                }
            }
            }

        private void AddPoint()
        {
            if (iGrow && iMaxVertexCount > AgentPolyline.Count)
            {
                Point3d newPoint = AgentPolyline[AgentPolyline.Count - 1] + TotalWeightedMoves[AgentPolyline.Count - 1];
                AgentPolyline.Add(newPoint);
            }
        }

        public Tuple<List<Point3d>, List<Vector3d>, List<double>> GetCurveParameters(List<Curve> Curves, Point3d testPoint)
        {
            List<Point3d> curvePoints = new List<Point3d>();
            List<Vector3d> curveVectors = new List<Vector3d>();
            List<double> distances = new List<double>();

            foreach (Curve item in Curves)
            {
                double t;
                item.ClosestPoint(testPoint, out t);
                curvePoints.Add(item.PointAt(t));
                curveVectors.Add(item.TangentAt(t));
                distances.Add((item.PointAt(t) - testPoint).Length);
                }
            return Tuple.Create(curvePoints, curveVectors, distances);
        }

        private Tuple<Point3d, Vector3d> GetClosestPoints(int number, List<Point3d> neighbourPoints, List<Vector3d> neighbourTangents, List<double> distances)
        {
            int PointCount = 0;

            List<Point3d> orderedPoints = new List<Point3d>();
            List<Vector3d> orderedTangents = new List<Vector3d>();
            List<double> orderedDistances = new List<double>();

            Point3d ptSum = new Point3d();
            Vector3d vecSum = new Vector3d();

            for (int i = 0; i < neighbourPoints.Count; i++){
                int I = distances.IndexOf(distances.Min());
                orderedDistances.Add(distances[I]);
                orderedPoints.Add(neighbourPoints[I]);
                orderedTangents.Add(neighbourTangents[I]);
                distances.RemoveAt(I);
            }

            for (int i = 0; i < orderedPoints.Count; i++) {
                if (i > number) continue;
                else { PointCount++; ptSum += orderedPoints[i]; vecSum += orderedTangents[i]; }
            }

            return Tuple.Create(ptSum/PointCount, vecSum/PointCount);
        }


    }
}