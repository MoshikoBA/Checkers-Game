using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Damka;

namespace Damka
{
    public class Player
    {
        public delegate void ChangeStatusDelegate();

        public enum ePlayerType
        {
            HUMAN,
            BOT
        }

        public enum eDirection
        {
            DOWN_TO_UP = -1,
            UP_TO_DOWN = 1
        }

        public enum ePlayerStatus
        {
            ACTIVE,
            QUIT,
            CAN_NOT_MOVE
        }

        private readonly string m_Name;
        private readonly Tool[] m_Tools;
        private readonly eDirection m_Direction;
        private readonly ePlayerType m_Type;
        private readonly char m_ToolsSign;
        private int m_Score = 0;
        private ePlayerStatus m_Status = ePlayerStatus.ACTIVE;

        public event EventHandler AfterPlay;

        public event ChangeStatusDelegate m_ChangeStatusDelegate;

        public Player(string i_Name, eDirection i_Direction, ePlayerType i_Type, byte i_NumberOfTools, char i_ToolsSign)
        {
            m_Name = i_Name;
            m_Direction = i_Direction;
            m_Type = i_Type;
            m_Tools = new Tool[i_NumberOfTools];
            m_ToolsSign = i_ToolsSign;
        }

        public string Name
        {
            get
            {
                return m_Name;
            }

            private set
            {
                Name = value;
            }
        }

        public Tool[] Tools
        {
            get
            {
                return m_Tools;
            }
        }

        public char ToolsSign
        {
            get
            {
                return m_ToolsSign;
            }
        }

        public int Score
        {
            get
            {
                return m_Score;
            }

            set
            {
                m_Score = value;
            }
        }

        public ePlayerType Type
        {
            get
            {
                return m_Type;
            }
        }

        public eDirection Direction
        {
            get
            {
                return m_Direction;
            }
        }

        public ePlayerStatus Status
        {
            get
            {
                return m_Status;
            }

            set
            {
                m_Status = value;
                if(value != ePlayerStatus.ACTIVE)
                {
                    notifyChangeStatusObservers();
                }
            }
        }

        public bool IsMyTool(int i_IndextTool, Point i_ToolLocation)
        {
            return i_IndextTool >= 0 && m_Tools[i_IndextTool] != null && m_Tools[i_IndextTool].Coordinate.X == i_ToolLocation.X &&
                m_Tools[i_IndextTool].Coordinate.Y == i_ToolLocation.Y;
        }

        public bool IsAnyToolCanEat()
        {
            bool isToolCanEat = false;

            foreach (Tool currnetTool in Tools)
            {
                if (currnetTool != null && currnetTool.EatMoves.Count > 0)
                {
                    isToolCanEat = true;
                    break;
                }
            }

            return isToolCanEat;
        }

        public int GetNumberOfToolOnBoard()
        {
            int numberOfToolsOnBoard = 0;

            foreach (Tool tool in m_Tools)
            {
                if (tool != null)
                {
                    numberOfToolsOnBoard++;
                }
            }

            return numberOfToolsOnBoard;
        }

        public void Play(byte[,] i_Board, Point i_SourceCoordinate, Point i_DestinationCoordinate)
        {
            byte indexTool;

            if (m_Status == ePlayerStatus.ACTIVE)
            {
                indexTool = i_Board[i_SourceCoordinate.X, i_SourceCoordinate.Y];
                i_Board[i_SourceCoordinate.X, i_SourceCoordinate.Y] = 0;
                i_Board[i_DestinationCoordinate.X, i_DestinationCoordinate.Y] = indexTool;
                m_Tools[(indexTool - 1) % m_Tools.Length].Coordinate = i_DestinationCoordinate;
                if (m_Direction == eDirection.UP_TO_DOWN)
                {
                    if (m_Tools[(indexTool - 1) % m_Tools.Length].Coordinate.X == Math.Sqrt(i_Board.Length) - 1)
                    {
                        m_Tools[(indexTool - 1) % m_Tools.Length].IsKing = true;
                        m_Tools[(indexTool - 1) % m_Tools.Length].Sign = 'K';
                    }
                }

                if (m_Direction == eDirection.DOWN_TO_UP)
                {
                    if (m_Tools[(indexTool - 1) % m_Tools.Length].Coordinate.X == 0)
                    {
                        m_Tools[(indexTool - 1) % m_Tools.Length].IsKing = true;
                        m_Tools[(indexTool - 1) % m_Tools.Length].Sign = 'U';
                    }
                }
            }

            AfterPlay.Invoke(this, new EventArgs());
        }

        protected virtual void notifyChangeStatusObservers()
        {
            if(m_ChangeStatusDelegate != null)
            {
                m_ChangeStatusDelegate.Invoke();
            }
        }
    }
}
