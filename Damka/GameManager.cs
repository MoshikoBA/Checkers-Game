using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Media;
using System.IO;

namespace DamkaApp
{
    public class GameManager
    {
        private Point m_CurrentSourceToolCoordinate;
        private Point m_CurrentDestinationToolCoordinate;
        private ButtonTool m_LastToolEat;
        private int m_CurrentPlayerTurn;
        private int m_EeatenIndexTool;
        private Timer m_ComputerTimer = new Timer();
        private SoundPlayer m_MoveSound;
        private SoundPlayer m_RoundOverSound;
        private SoundPlayer m_ErrorSound;
        private SoundPlayer m_CaptureSound;

        public GameManager()
        {
            InitProperties();
            initSoundStreams();
        }

        public Point SourceToolCoordinate
        {
            get
            {
                return m_CurrentSourceToolCoordinate;
            }

            set
            {
                m_CurrentSourceToolCoordinate = value;
            }
        }

        public Point DestinationToolCoordinate
        {
            get
            {
                return m_CurrentDestinationToolCoordinate;
            }

            set
            {
                m_CurrentDestinationToolCoordinate = value;
            }
        }

        public ButtonTool LastToolEat
        {
            get
            {
                return m_LastToolEat;
            }

            set
            {
                m_LastToolEat = value;
            }
        }

        public int PlayerTurn
        {
            get
            {
                return m_CurrentPlayerTurn;
            }

            set
            {
                m_CurrentPlayerTurn = value;
            }
        }

        public int EeatenIndexTool
        {
            get
            {
                return m_EeatenIndexTool;
            }

            set
            {
                m_EeatenIndexTool = value;
            }
        }

        public Timer ComputerTimer
        {
            get
            {
                return m_ComputerTimer;
            }
        }

        public SoundPlayer MoveSound
        {
            get
            {
                return m_MoveSound;
            }
        }

        public SoundPlayer RoundOverSound
        {
            get
            {
                return m_RoundOverSound;
            }
        }

        public SoundPlayer ErrorSound
        {
            get
            {
                return m_ErrorSound;
            }
        }

        public SoundPlayer CaptureSound
        {
            get
            {
                return m_CaptureSound;
            }
        }

        public void InitProperties()
        {
            m_CurrentSourceToolCoordinate = new Point(-1, -1);
            m_CurrentDestinationToolCoordinate = new Point(-1, -1);
            m_LastToolEat = null;
            m_CurrentPlayerTurn = 0;
            m_EeatenIndexTool = -1;
            m_ComputerTimer.Interval = 1200;
        }

        private void initSoundStreams()
        {
            Stream moveSoundStream = Properties.Resources.move;
            m_MoveSound = new SoundPlayer(moveSoundStream);

            Stream roundOverSoundStream = Properties.Resources.over;
            m_RoundOverSound = new SoundPlayer(roundOverSoundStream);

            Stream errorSoundStream = Properties.Resources.error;
            m_ErrorSound = new SoundPlayer(errorSoundStream);

            Stream captureSoundStream = Properties.Resources.capture;
            m_CaptureSound = new SoundPlayer(captureSoundStream);
        }
   }
}
