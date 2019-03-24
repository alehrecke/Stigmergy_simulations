using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace MeshGrowth
{
    class StigmergyAgent
    {

        public bool Grow;
        public List<Curve> Extents;
        public double MaxVertexCount;
        public double NeighbourCount;
        public double RepelMin;
        public double RepelMax;
        public double AttractionMin;
        public double AttractionMax;
        public double AlignMin;
        public double AlignMax;

        public List<Point3d> AllPoints;
        public List<Vector3d> AllTangents;
        public RTree rTree;
        public Polyline AgentPolyline;


        public List<double> totalWeights;
        public List<Vector3d> totalWeightedDirections;

        public StigmergyAgent(Point3d _startingPoint, Vector3d _randomVector)
        {
            Point3d secondPoint = _startingPoint + _randomVector;

            AgentPolyline = new Polyline();
            AgentPolyline.Add(_startingPoint);
            AgentPolyline.Add(secondPoint);
        }

        public void Update()
        {
            Weights();
            //RandomForce();
            CollisionDetection();
            UpdateVertices();
        }

        public void Weights()
        {
            totalWeights = new List<double>();
            totalWeightedDirections = new List<Vector3d>();

            for (int i = 0; i < AgentPolyline.Count; i++)
            {
                totalWeights.Add(0);
                totalWeightedDirections.Add(new Vector3d());
            }
        }

        public void RandomForce()
        {
            //make a random vector

            for (int i = 0; i < AgentPolyline.Count; i++)
            {

                Random rand = new Random();
                double x = rand.Next(0, 100);
                double y = rand.Next(0, 100);

                Vector3d move = new Vector3d(x, y, 0);
                move.Unitize();

                totalWeightedDirections[i] += move * 1;
                totalWeights[i] += 1;
            }
        }

        private void UpdateVertices()
        {
            for (int i = 0; i < AgentPolyline.Count; i++)
            {
                if (totalWeights[i] == 0) { continue; }
                else
                {
                    Vector3d move = totalWeightedDirections[i] / totalWeights[i];
                    AgentPolyline[i] += move;
                }
            }
        }

        private void CollisionDetection()
        {
            //only search from the last point
            for (int i = 0; i < AgentPolyline.Count; i++)
            {

                Point3d thisPoint = AgentPolyline[i];
                List<int> neighbourIds = new List<int>();

                rTree.Search(new Sphere(thisPoint, AttractionMax), (sender, args) => { neighbourIds.Add(args.Id); });

                if (neighbourIds.Count > 0 && NeighbourCount > 1)
                {
                    List<int> orderedIds = ClosestIds(thisPoint, neighbourIds, (int)NeighbourCount);

                    if (orderedIds.Count > 1){

                        Point3d targetPoint = AllPoints[orderedIds[1]];
                        Vector3d targetTangent = AllTangents[orderedIds[1]];
                        Vector3d move = targetPoint - thisPoint;

                        if (RepelMin <= move.Length && move.Length <= RepelMax)
                        {
                            move.Unitize();
                            totalWeights[i] += 1;
                            totalWeightedDirections[i] += move * -1;
                        }

                        if (AttractionMin < move.Length && move.Length <= AttractionMax)
                        {
                            move.Unitize();
                            totalWeights[i] += 1;
                            totalWeightedDirections[i] += move * 1;
                        }

                        if (AlignMin < move.Length && move.Length <= AlignMax)
                        {
                            targetTangent.Unitize();
                            totalWeights[i] += 1;
                            totalWeightedDirections[i] += targetTangent * 1;
                        }
                    }



                }
            }
        }

        private void AddVertice()
        {
            int lastIndex = AgentPolyline.Count - 1;
            Point3d lastPoint = AgentPolyline[lastIndex];
            Vector3d lastVector = totalWeightedDirections[lastIndex] / totalWeights[lastIndex];
            Point3d newPoint = lastPoint + lastVector;

            AgentPolyline.Add(newPoint);
        }


        public int ClosestPoint(Point3d testPoint, List<int> pointIds)
        {
            double closestDistance = 99999;
            int closestId = 99999;

            for (int i = 0; i < pointIds.Count; i++)
            {

                double distance = (AllPoints[pointIds[i]] - testPoint).Length;

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestId = pointIds[i];
                }
            }
            return closestId;
        }

        public List<int> ClosestIds(Point3d testPoint, List<int> pointIds, int n)
        {
            List<int> closeIds = new List<int>();

            for (int i = 0; i < n; i++)
            {
                int closestId = ClosestPoint(testPoint, pointIds);
                closeIds.Add(closestId);
                pointIds.Remove(closestId);
            }
            return closeIds;
        }

        /*
         public List<Vec3D> ClosestPoints(Vec3D a, List<Vec3D> points, int n)
                {
                    List<Vec3D> closePts = new List<Vec3D>();
                    for (int i = 0; i < n; i++)
                    {
                        Vec3D closestPt = ClosestPoint(a, points);
                        closePts.Add(closestPt);
                        points.Remove(closestPt);
                    }
                    return closePts;
                }

                public Vec3D ClosestPoint(Vec3D a, List<Vec3D> points)
                {
                    Vec3D closest = null;
                    float closestDist = 10000000000;

                    foreach (Vec3D p in points)
                    {
                        if ((p - a).magnitude() < closestDist)
                        {
                            closestDist = (p - a).magnitude();
                            closest = p;
                        }
                    }
                    return closest;
                }
        */

    }
}
