using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

//create a trail class
//add a new point in the next position

namespace CurveAgents
{
    class PointAgent
    {
        public List<PointAgent> AllAgents;
        public List<int> NeighbourIds;
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

        public Point3d Position;
        public Vector3d Velocity;
        private Vector3d Accelaration;



        public PointAgent(Point3d _position, Vector3d _velocity)
        {
            Position = _position;
            Velocity = _velocity;
        }

        private Vector3d CalculateAcceleration(Point3d _position, Point3d _target) {
            Vector3d _accelaration = _target - _position;
            _accelaration.Unitize();
            return _accelaration;
        }
        
        private Vector3d CalculateAcceleration2() {
            Vector3d _accelaration = new Vector3d();

            if (NeighbourIds.Count > 0) {
                _accelaration = CalculateAttract() + CalculateRepel() + CalculateAlign();
                _accelaration.Unitize();
            }
                return _accelaration;
        }

        public Vector3d CalculateAttract() {
            Vector3d _attract = new Vector3d();
                PointAgent targetAgent = AllAgents[NeighbourIds[0]];
                Vector3d move = targetAgent.Position - Position;
                if (AttractMin < move.Length && move.Length <= AttractMax) {
                    move.Unitize();
                    _attract = move * AttractWeight;
            }
            return _attract;
        }

        public Vector3d CalculateRepel()
        {
            Vector3d _repel = new Vector3d();
                PointAgent targetAgent = AllAgents[NeighbourIds[0]];
                Vector3d move = Position - targetAgent.Position;
                if (RepelMin <= move.Length && move.Length <= RepelMax)
                {
                    move.Unitize();
                    _repel = move * RepelWeight;
                }
            return _repel;
        }

        public Vector3d CalculateAlign()
        {
            Vector3d _align = new Vector3d();
            PointAgent targetAgent = AllAgents[NeighbourIds[0]];
            Vector3d move = targetAgent.Velocity;
            Vector3d move2 = targetAgent.Position - Position;
            if (AlignMin <= move2.Length && move2.Length < AlignMax){
                    move.Unitize();
                    _align = move * AlignWeight;
                }
            return _align;
        }

        public void CalculateVelocity(){
            //Accelaration = CalculateAcceleration(Position, new Point3d(0,0,0));
            Accelaration = CalculateAcceleration2();
            Velocity = Velocity + (Accelaration * Steering);
            Velocity.Unitize();
        }

        public void CalculatePosition() {
            CalculateVelocity();
            Position = Position + Velocity;
        }

        public void Update()
        {
            CalculateVelocity();
//            UpdatePosition();
        }

    }
}
