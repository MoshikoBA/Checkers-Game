using System;
using System.Collections.Generic;
using System.Text;

namespace Damka
{
    public class DeletedToolEventArg : EventArgs
    {
        private int m_Row;
        private int m_Col;

        public DeletedToolEventArg(int i_Row, int i_Col)
        {
            m_Row = i_Row;
            m_Col = i_Col;
        }

        public int Row
        {
            get { return m_Row; }
        }

        public int Col
        {
            get { return m_Col; }
        }
    }
}
