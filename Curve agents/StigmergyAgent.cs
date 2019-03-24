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

        //change this to a list of points
        public Polyline AgentPolyline;
        //add a list of tangent vectors


        public List<double> totalWeights;
        public List<Vector3d> totalWeightedDirections;

        //make it so that it works in torus space.
        //make it so that each agent is composed of a list of points, and a list of corresponding vectors.
        //display function makes a line from each point to the next

        public StigmergyAgent(Point3d _startingPoint, Vector3d _randomVector)
        {
            Point3d secondPoint = _startingPoint + _randomVector;
            //replace polyline with list of points
            AgentPolyline = new Polyline();
            AgentPolyline.Add(_startingPoint);
            AgentPolyline.Add(secondPoint);
        }

        public void Update(){
            Weights();
            Velocity();
            //CollisionDetection();
            //UpdateVertices();
            AddVertice();
            RestrictLength();
        }

        public void Weights()
        {
            totalWeights = new List<double>();
            totalWeightedDirections = new List<Vector3d>();

            //replace with list of points
            //change to only deal with the last point in the list
            for (int i = 0; i < AgentPolyline.Count; i++)
            {
                totalWeights.Add(0);
                totalWeightedDirections.Add(new Vector3d());
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
                List<int> neighbourIds = new List<int>();

                rTree.Search(new Sphere(thisPoint, AttractionMax), (sender, args) => {
                    if (AgentPolyline.Contains(AllPoints[args.Id]) == false) { neighbourIds.Add(args.Id); }
                });



                if (neighbourIds.Count > 0 && NeighbourCount > 1)
                {
                    List<int> orderedIds = ClosestIds(thisPoint, neighbourIds, (int)NeighbourCount);

                    if (orderedIds.Count > 1){
                        int value = orderedIds[0];
                        int fullCount = AllPoints.Count;

                        Point3d targetPoint = AllPoints[orderedIds[0]];
                        Vector3d targetTangent = AllTangents[orderedIds[0]];
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

        private void Velocity()
        {
            //replace polyline with list of points.
                int lastIndex = AgentPolyline.Count - 1;
                Point3d thisPoint = AgentPolyline[lastIndex];
                List<int> neighbourIds = new List<int>();

                rTree.Search(new Sphere(thisPoint, AttractionMax), (sender, args) => {
                    //replace polyline with list of points.
                    if (AgentPolyline.Contains(AllPoints[args.Id]) == false) { neighbourIds.Add(args.Id);}

                });

                if (neighbourIds.Count > 0 && NeighbourCount > 1){

                    List<int> orderedIds = ClosestIds(thisPoint, neighbourIds, (int)NeighbourCount);

                    if (orderedIds.Count > 1)
                    {
                        Point3d targetPoint = AllPoints[orderedIds[0]];
                        Vector3d targetTangent = AllTangents[orderedIds[0]];
                        Vector3d move = targetPoint - thisPoint;

                        if (RepelMin <= move.Length && move.Length <= RepelMax)
                        {
                            move.Unitize();
                            totalWeights[lastIndex] += 1;
                            totalWeightedDirections[lastIndex] += move * -1;
                        }

                        if (AttractionMin < move.Length && move.Length <= AttractionMax)
                        {
                            move.Unitize();
                            totalWeights[lastIndex] += 1;
                            totalWeightedDirections[lastIndex] += move * 1;
                        }

                        if (AlignMin < move.Length && move.Length <= AlignMax)
                        {
                            targetTangent.Unitize();
                            totalWeights[lastIndex] += 1;
                            totalWeightedDirections[lastIndex] += targetTangent * 1;
                        }
                    }
                }
        }

        private void AddVertice()
        {
            if (Grow) {
                int lastIndex = AgentPolyline.Count - 1;
                Point3d lastPoint = AgentPolyline[lastIndex];
                Vector3d lastVector = totalWeightedDirections[lastIndex] / totalWeights[lastIndex];
                Point3d newPoint = lastPoint + lastVector;
                AgentPolyline.Add(newPoint);
            }
            }

        private void RestrictLength()
        {
            if(MaxVertexCount < AgentPolyline.Count){
                AgentPolyline.RemoveAt(0);
            }
        }

        private void TorusSpace()
        {
            //if the next position.x > x extents, next position.x == 0
            //
        }

        private void display()
        {


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
                if (closestId != 99999) {
                    closeIds.Add(closestId);
                    pointIds.Remove(closestId);
                }
                }
            return closeIds;
        }

    }
}
