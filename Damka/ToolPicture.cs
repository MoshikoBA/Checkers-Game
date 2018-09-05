using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Damka;

namespace DamkaApp
{
    public class ButtonTool : Button
    {
        private Point m_Coordinate;
        private int m_ToolIndex = 0;
        private bool m_IsClicked = false;
        private Color m_OriginalBackColor;

        public ButtonTool(Point i_Coordinate)
        {
            m_Coordinate = i_Coordinate;
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

        public int Index
        {
            get
            {
                return m_ToolIndex;
            }

            set
            {
                m_ToolIndex = value;
            }
        }

        public bool IsClicked
        {
            get
            {
                return m_IsClicked;
            }

            set
            {
                if (value)
                {
                    this.BackColor = Color.Yellow;
                }
                else
                {
                    this.BackColor = m_OriginalBackColor;
                }

                m_IsClicked = value;
            }
        }

        public Color OriginalBackColor
        {
            get
            {
                return m_OriginalBackColor;
            }

            set
            {
                m_OriginalBackColor = value;
            }
        }
    }
}