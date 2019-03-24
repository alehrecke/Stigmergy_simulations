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
        public RTree rTree;

        public Polyline AgentPolyline;


        public List<double> totalWeights;
        public List<Vector3d> totalWeightedDirections;



        public StigmergyAgent(Point3d _startingPoint)
        {
            AgentPolyline = new Polyline();
            AgentPolyline.Add(_startingPoint);
            AgentPolyline.Add(new Point3d(_startingPoint.X, _startingPoint.Y, 20));

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

            for(int i = 0; i < AgentPolyline.Count; i++) {
                totalWeights.Add(0);
                totalWeightedDirections.Add(new Vector3d());
            }
        }

        public void RandomForce()
        {
            //make a random vector

            for (int i = 0; i < AgentPolyline.Count; i++) {

                Random rand = new Random();
                double x = rand.Next(0,100);
                double y = rand.Next(0,100);

                Vector3d move = new Vector3d(x,y,0);
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
            for (int i = 0; i < AgentPolyline.Count; i++)
            {
                Point3d thisPoint = AgentPolyline[i];
                List<Point3d> neighbourPoints = new List<Point3d>();

                rTree.Search(new Sphere(thisPoint, AttractionMax), (sender, args) => { neighbourPoints.Add(AllPoints[args.Id]); });

                if (neighbourPoints.Count > 0) {

                    Point3d first = neighbourPoints[0];
                    Point3d second = neighbourPoints[1];


                    List<Point3d> orderedPoints = ClosestPoints(thisPoint, neighbourPoints, (int)NeighbourCount);

                    if (NeighbourCount > 1) {
                        Point3d targetPoint = orderedPoints[1];
                        Vector3d move = targetPoint - thisPoint;

                        if (RepelMin <= move.Length && move.Length <= RepelMax) {
                            move.Unitize();
                            totalWeights[i] += 1;
                            totalWeightedDirections[i] += move * -1;
                        }

                        if (AttractionMin < move.Length && move.Length <= AttractionMax) {
                            move.Unitize();
                            totalWeights[i] += 1;
                            totalWeightedDirections[i] += move * 1;
                        }

                        //add the align force here
                        if (AlignMin < move.Length && move.Length <= AlignMax){
                           
                            move.Unitize();
                            totalWeights[i] += 1;
                            totalWeightedDirections[i] += move * 1;
                        }


                    }



                }




            }
        }


        public Point3d ClosestPoint(Point3d testPoint, List<Point3d> pointCloud)
        {
            Point3d closestPoint = new Point3d(99999,99999,99999);
            double closestDistance = 99999;

            foreach (Point3d item in pointCloud){
                double distance = (item - testPoint).Length;

                if (distance < closestDistance) {
                    closestDistance = distance;
                    closestPoint = item;
                }
            }
            return closestPoint;
        }


        //have this return a tuple containing the tangent as well as the point
        public List<Point3d> ClosestPoints(Point3d testPoint, List<Point3d> pointCloud, int n)
        {
            List<Point3d> closePoints = new List<Point3d>();
            for (int i = 0; i < n; i++) {
                Point3d closestPoint = ClosestPoint(testPoint, pointCloud);
                closePoints.Add(closestPoint);
                pointCloud.Remove(closestPoint);
            }
            return closePoints;
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
