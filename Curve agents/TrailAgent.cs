using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace CurveAgents
{
    class TrailAgent
    {
        public RTree rtree;
        public List<PointAgent> AllAgents;
        private List<int> NeighbourIds;

        public int TrailLength;
        public int NeighbourCount;
        public double SearchRange;
        public double xExtents;
        public double yExtents;

        public double AttractMin;
        public double AttractMax;
        public double AttractWeight;
        public double RepelMin;
        public double RepelMax;
        public double RepelWeight;
        public double AlignMin;
        public double AlignMax;
        public double AlignWeight;
        public double Steering;

        public List<PointAgent> Trail;
        public List<LineCurve> TrailSegments;

        public TrailAgent(Point3d _position, Vector3d _velocity)
        {
            Trail = new List<PointAgent>();
            Trail.Add(new PointAgent(_position, _velocity));
        }

        public void Update()
        {
            SearchTree();
            PassInValues();
            AddAgent();
            TorusSpace();
            RestrictLength();
            DisplaySegments();
        }

        private void SearchTree()
        {
            Point3d lastPoint = Trail[Trail.Count - 1].Position;
            NeighbourIds = new List<int>();

            rtree.Search(new Sphere(lastPoint, 50), (sender, args) => { if (Trail.Contains(AllAgents[args.Id]) == false) { NeighbourIds.Add(args.Id); } });

            if (NeighbourIds.Count >= NeighbourCount) { NeighbourIds = ClosestIds(lastPoint, NeighbourIds, NeighbourCount); }
            else { NeighbourIds = ClosestIds(lastPoint, NeighbourIds, NeighbourIds.Count); }
        }

        public int ClosestId(Point3d testPoint, List<int> pointIds)
        {
            double closestDistance = 99999;
            int closestId = 99999;

            for (int i = 0; i < pointIds.Count; i++)
            {

                double distance = (AllAgents[pointIds[i]].Position - testPoint).Length;

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
                int closestId = ClosestId(testPoint, pointIds);
                if (closestId != 99999)
                {
                    closeIds.Add(closestId);
                    pointIds.Remove(closestId);
                }
            }
            return closeIds;
        }

        private void PassInValues()
        {
            for (int i = 0; i < Trail.Count; i++)
            {
                Trail[i].NeighbourIds = NeighbourIds;
                Trail[i].AllAgents = AllAgents;
                Trail[i].AttractMin = AttractMin;
                Trail[i].AttractMax = AttractMax;
                Trail[i].AttractWeight = AttractWeight;
                Trail[i].RepelMin = RepelMin;
                Trail[i].RepelMax = RepelMax;
                Trail[i].RepelWeight = RepelWeight;
                Trail[i].AlignMin = AlignMin;
                Trail[i].AlignMax = AlignMax;
                Trail[i].AlignWeight = AlignWeight;
                Trail[i].Steering = Steering;
            }
        }

        private void AddAgent()
        {

            int lastIndex = Trail.Count - 1;
            PointAgent thisAgent = Trail[lastIndex];

            thisAgent.Update();
            Point3d newPosition = thisAgent.Position + thisAgent.Velocity;
            Vector3d newVelocity = thisAgent.Velocity;

            Trail.Add(new PointAgent(newPosition, newVelocity));
            //update agents velocity.
            //new position = prev position + this updated velocity
            //add new agent (new position, this updated velocity)
        }

        private void TorusSpace()
        {
            int lastIndex = Trail.Count - 1;
            Point3d lastPosition = Trail[lastIndex].Position;

            if (lastPosition.X > xExtents) { Trail[lastIndex].Position = new Point3d(0, lastPosition.Y, 0); }
            if (lastPosition.X < 0) { Trail[lastIndex].Position = new Point3d(xExtents, lastPosition.Y, 0); }
            if (lastPosition.Y > yExtents) { Trail[lastIndex].Position = new Point3d(lastPosition.X, 0, 0); }
            if (lastPosition.Y < 0) { Trail[lastIndex].Position = new Point3d(lastPosition.X, yExtents, 0); }
        }

        private void RestrictLength()
        {
            if (TrailLength < Trail.Count) { Trail.RemoveAt(0); }
        }

        private void DisplaySegments()
        {
            TrailSegments = new List<LineCurve>();

            for (int i = 0; i < Trail.Count - 1; i++)
            {
                double distance = (Trail[i].Position - Trail[i + 1].Position).Length;
                if (distance >= xExtents * 0.9 || distance >= yExtents * 0.9) { continue; }
                else { TrailSegments.Add(new LineCurve(Trail[i].Position, Trail[i + 1].Position)); }
            }
        }
    }
}
