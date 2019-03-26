using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace CurveAgents
{
    class StigmergyAgent
    {
        public bool Grow;
        public double XExtents;
        public double YExtents;
        public int MaxVertexCount;
        public int NeighbourCount;
        public double RepelMin;
        public double RepelMax;
        public double RepelWeight;
        public double AttractionMin;
        public double AttractionMax;
        public double AttractionWeight;
        public double AlignMin;
        public double AlignMax;
        public double AlignWeight;
        public double SegmentLength;
        public List<Point3d> AllPoints;
        public List<Vector3d> AllTangents;
        public RTree rTree;
        public List<Point3d> AgentList;
        public List<Vector3d> AgentDirections;
        public List<LineCurve> AgentSegments;
        public List<double> TotalWeights;
        /*
        add in desired velocity vs current velocity.
        add acceleration to velocity.

            acceleration = .....
            velocity = velocity + accelaration;
            set velocity total length
            position = position + velocity;

        */



        public StigmergyAgent(Point3d _startingPoint, Vector3d _randomvector){
            AgentList = new List<Point3d>();
            AgentDirections = new List<Vector3d>();
            TotalWeights = new List<double>();
            AgentList.Add(_startingPoint);
            AgentDirections.Add(_randomvector);
            AgentList.Add(_startingPoint + _randomvector);
            AgentDirections.Add(new Vector3d());
            TotalWeights.Add(0);
            TotalWeights.Add(0);
        }

        public void Update(){
            CalculateVelocity();
            AddVertice();
            TorusSpace();
            DisplaySegments();
            RestrictLength();
        }

        private void CalculateVelocity()
        {
                int lastIndex = AgentList.Count - 1;
                Point3d thisPoint = AgentList[lastIndex];
                List<int> neighbourIds = new List<int>();

                rTree.Search(new Sphere(thisPoint, AlignMax), (sender, args) => {
                    if (AgentList.Contains(AllPoints[args.Id]) == false) { neighbourIds.Add(args.Id);}
                });

                if (neighbourIds.Count > 0 && NeighbourCount > 1){

                    List<int> orderedIds = ClosestIds(thisPoint, neighbourIds, NeighbourCount);

                    if (orderedIds.Count > 1)
                    {
                    Point3d targetPoint = AllPoints[orderedIds[0]];
                    Vector3d targetTangent = AverageTangent(orderedIds, NeighbourCount);

                    Vector3d move = targetPoint - thisPoint;
                        
                        if (RepelMin <= move.Length && move.Length <= RepelMax){
                            move.Unitize();
                            TotalWeights[lastIndex] += RepelWeight;
                            AgentDirections[lastIndex] += move * RepelWeight;
                        }
                        
                        if (AttractionMin < move.Length && move.Length <= AttractionMax){
                            move.Unitize();
                            TotalWeights[lastIndex] += AttractionWeight;
                            AgentDirections[lastIndex] += move * AttractionWeight;
                        }
                        
                        if (AlignMin < move.Length && move.Length <= AlignMax){
                            targetTangent.Unitize();
                            TotalWeights[lastIndex] += AlignWeight;
                            AgentDirections[lastIndex] += targetTangent * AlignWeight;
                        }
                    }
                }
            }

        private void AddVertice()
        {
            if (Grow) {
                int lastIndex = AgentList.Count - 1;
                Point3d lastPoint = AgentList[lastIndex];


                Vector3d lastVector = AgentDirections[lastIndex] / TotalWeights[lastIndex];



                lastVector = lastVector * SegmentLength;
                Point3d newPoint = lastPoint + lastVector;
                AgentList.Add(newPoint);
                AgentDirections.Add(new Vector3d());
                TotalWeights.Add(0);
            }
            }

        private void DisplaySegments()
        {
            AgentSegments = new List<LineCurve>();

            for (int i = 0; i < AgentList.Count - 1; i++){
                double distance = (AgentList[i] - AgentList[i + 1]).Length;
                if (distance >= XExtents*0.5 || distance >= YExtents) { continue; }
                else { AgentSegments.Add(new LineCurve(AgentList[i], AgentList[i + 1])); }
            }
        }

        private void RestrictLength()
        {
            if(MaxVertexCount < AgentList.Count){
                AgentList.RemoveAt(0);
                AgentDirections.RemoveAt(0);
                TotalWeights.RemoveAt(0);
            }
        }

        private void TorusSpace()
        {
            int lastIndex = AgentList.Count - 1;
            Point3d lastPoint = AgentList[lastIndex];

            if( lastPoint.X > XExtents ) { AgentList[lastIndex] = new Point3d( 0, lastPoint.Y, 0); }
            if (lastPoint.X < 0) { AgentList[lastIndex] = new Point3d(XExtents, lastPoint.Y, 0); }
            if (lastPoint.Y > YExtents) { AgentList[lastIndex] = new Point3d(lastPoint.X, 0, 0); }
            if (lastPoint.Y < 0) { AgentList[lastIndex] = new Point3d(lastPoint.X, YExtents, 0); }
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

        public Vector3d AverageTangent(List<int> orderedIds, int numberOfNeighbours){

            Vector3d sum = new Vector3d();
            Vector3d average = new Vector3d();
            
            if (numberOfNeighbours <= orderedIds.Count) {
                for (int i = 0; i < numberOfNeighbours; i++) { sum += AllTangents[orderedIds[i]]; }
                average = sum / numberOfNeighbours;
            }
            else{
                for (int i = 0; i < orderedIds.Count; i++) { average += AllTangents[orderedIds[i]]; }
                average = sum / orderedIds.Count;
            }
            return average;
        }

    }
}
