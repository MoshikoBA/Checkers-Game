using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace Damka
{
    public class Tool
    {
        public const byte k_KingValue = 4;
        public const byte k_SimpleToolValue = 1;

        private readonly List<Point> m_RightMoves = new List<Point>();
        private readonly List<Point> m_LeftMoves = new List<Point>();
        private readonly List<Point> m_EatMoves = new List<Point>();
        private readonly List<int> m_EatenOtherPlayerToolsIndexes = new List<int>();
        private Point m_Coordinate;
        private bool m_IsKing = false;
        private char m_Sign;
        private bool m_CanBeEaten = false;
        public EventHandler BecameKing;

        public Tool(Point i_ToolCoordinate, char i_ToolSign)
        {
            m_Coordinate = i_ToolCoordinate;
            m_Sign = i_ToolSign;
        }

        public Tool(int i_Row, int i_Col, char i_ToolSign)
        {
            m_Coordinate.X = i_Row;
            m_Coordinate.Y = i_Col;
            m_Sign = i_ToolSign;
        }

        public Point Coordinate
        {
            get
            {
                return m_Coordinate;
            }

            set
            {
                m_Coordinate = value;
            }
        }

        public bool IsKing
        {
            get
            {
                return m_IsKing;
            }

            set
            {
                if(!m_IsKing)
                {
                    OnBecameKing();
                    m_IsKing = value;
                } 
            }
        }

        public char Sign
        {
            get
            {
                return m_Sign;
            }

            set
            {
                m_Sign = value;
            }
        }

        public bool CanBeEaten
        {
            get
            {
                return m_CanBeEaten;
            }

            set
            {
                m_CanBeEaten = value;
            }
        }

        public List<Point> RightMoves
        {
            get
            {
                return m_RightMoves;
            }
        }

        public List<Point> LeftMoves
        {
            get
            {
                return m_LeftMoves;
            }
        }

        public List<Point> EatMoves
        {
            get
            {
                return m_EatMoves;
            }
        }

        public List<int> EatenOtherPlayerToolIndex
        {
            get
            {
                return m_EatenOtherPlayerToolsIndexes;
            }
        }

        public bool IsMyRightMove(Point i_Direction)
        {
            bool isMyRightMove = false;

            foreach (Point rightMove in m_RightMoves)
            {
                if (rightMove.Equals(i_Direction))
                {
                    isMyRightMove = true;
                    break;
                }
            }

            return isMyRightMove;
        }

        public bool IsMyLeftMove(Point i_Direction)
        {
            bool isMyLeftMove = false;

            foreach (Point leftMove in m_LeftMoves)
            {
                if (leftMove.Equals(i_Direction))
                {
                    isMyLeftMove = true;
                    break;
                }
            }

            return isMyLeftMove;
        }

        public bool isMyEatenMove(Point i_DestinationEat, ref int io_EatenOtherPlayerToolIndex)
        {
            bool isMyEatenMove = false;
            for (int i = 0; i < EatMoves.Count; i++)
            {
                if (m_EatMoves[i].Equals(i_DestinationEat))
                {
                    io_EatenOtherPlayerToolIndex = m_EatenOtherPlayerToolsIndexes[i];
                    isMyEatenMove = true;
                    break;
                }
            }

            return isMyEatenMove;
        }

        public void ResetMoves()
        {
            m_RightMoves.Clear();
            m_LeftMoves.Clear();
            m_EatMoves.Clear();
            m_EatenOtherPlayerToolsIndexes.Clear();
            m_CanBeEaten = false;
        }

        protected virtual void OnBecameKing()
        {
            if(BecameKing != null)
            {
                BecameKing.Invoke(this, new EventArgs());
            }
        }
    }
}
