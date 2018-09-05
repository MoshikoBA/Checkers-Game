using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Damka;

namespace DamkaApp
{
    public class GameSetting : Form
    {
        private readonly RadioButton[] m_RadioButtonsBoardaSizeOptions = new RadioButton[3];
        private Label labelBoardSize = new Label();
        private Label labelPlayers = new Label();
        private Label labelPlayer1 = new Label();
        private CheckBox checkBoxPlayer2 = new CheckBox();
        private TextBox textBoxPlayer1Name = new TextBox();
        private TextBox textBoxPlayer2Name = new TextBox();
        private Button buttonDone = new Button();
        private byte m_BoardSizeChosen;

        public GameSetting()
        {
            InitializeComponent();
        }

        public RadioButton[] BoardaSizeOptions
        {
            get
            {
                return m_RadioButtonsBoardaSizeOptions;
            }
        }

        public string Player1Name
        {
            get
            {
                if(textBoxPlayer1Name.Text.Length > 10)
                {
                    return textBoxPlayer1Name.Text.Remove(10);
                }
                else
                {
                    return textBoxPlayer1Name.Text;
                }
            }
        }

        public CheckBox Playe2CheckBox
        {
            get
            {
                return checkBoxPlayer2;
            }
        }

        public string Player2Name
        {
            get
            {
                if (textBoxPlayer2Name.Text.Length > 10)
                {
                    return textBoxPlayer2Name.Text.Remove(10);
                }
                else
                {
                    return textBoxPlayer2Name.Text;
                }
            }
        }

        public Button DoneButton
        {
            get
            {
                return buttonDone;
            }
        }

        public byte BoardSizeChosen
        {
            get
            {
                return m_BoardSizeChosen;
            }
        }

        private void InitializeComponent()
        {
            initFormProperties();
            initBoardSizeProperties();
            initBoardSizeOptionsProperties();
            initPlayersProperties();
            initPlayer1Properties();
            initPlayer1Name();
            initPlayer2Name();
            initPlayer2Properties();
            initDoneButton();
        }

        private void initFormProperties()
        {
            this.Width = 300;
            this.Height = 280;
            this.BackColor = Color.WhiteSmoke;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Game Setting";
            this.FormBorderStyle = FormBorderStyle.Fixed3D;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
        }

        private void initBoardSizeProperties()
        {
            labelBoardSize.Text = "Board Size:";
            labelBoardSize.AutoSize = true;
            this.Controls.Add(labelBoardSize);
            labelBoardSize.Left = labelBoardSize.Top = 20;
        }

        private void initBoardSizeOptionsProperties()
        {
            int leftOffset = 40;

            for(int i = 0; i < m_RadioButtonsBoardaSizeOptions.Length; i++)
            {
                m_RadioButtonsBoardaSizeOptions[i] = new RadioButton();
                m_RadioButtonsBoardaSizeOptions[i].AutoSize = true;
                m_RadioButtonsBoardaSizeOptions[i].Top = 40;
                m_RadioButtonsBoardaSizeOptions[i].Left = leftOffset;
                this.Controls.Add(m_RadioButtonsBoardaSizeOptions[i]);
                m_RadioButtonsBoardaSizeOptions[i].Click += new EventHandler(boardSizeOption_Clicked);
                leftOffset += 80;
            }

            m_RadioButtonsBoardaSizeOptions[0].Text = "6 x 6";
            m_RadioButtonsBoardaSizeOptions[1].Text = "8 x 8";
            m_RadioButtonsBoardaSizeOptions[2].Text = "10 x 10";
            m_RadioButtonsBoardaSizeOptions[0].Checked = true;
        }

        private void boardSizeOption_Clicked(object sender, EventArgs e)
        {
            string size = (sender as RadioButton).Text.Substring(0, (sender as RadioButton).Text.IndexOf(' '));
            m_BoardSizeChosen = byte.Parse(size);
        }

        private void initPlayersProperties()
        {
            labelPlayers.Text = "Players:";
            labelPlayers.AutoSize = true;
            this.Controls.Add(labelPlayers);
            labelPlayers.Top = 70;
            labelPlayers.Left = 20;
        }

        private void initPlayer1Properties()
        {
            this.Controls.Add(labelPlayer1);
            labelPlayer1.AutoSize = true;
            labelPlayer1.Text = "Player1:";
            labelPlayer1.Top = 105;
            labelPlayer1.Left = 40;
        }

        private void initPlayer1Name()
        {
            this.Controls.Add(textBoxPlayer1Name);
            textBoxPlayer1Name.Top = 105;
            textBoxPlayer1Name.Left = 120;
            textBoxPlayer1Name.Width = 100;
        }

        private void initPlayer2Name()
        {
            this.Controls.Add(textBoxPlayer2Name);
            textBoxPlayer2Name.Text = "[Computer]";
            textBoxPlayer2Name.Top = 140;
            textBoxPlayer2Name.Left = 120;
            textBoxPlayer2Name.Width = 100;
            textBoxPlayer2Name.Enabled = false;
        }

        private void initPlayer2Properties()
        {
            this.Controls.Add(checkBoxPlayer2);
            checkBoxPlayer2.AutoSize = true;
            checkBoxPlayer2.Text = "Player2:";
            checkBoxPlayer2.Top = 140;
            checkBoxPlayer2.Left = 40;
            checkBoxPlayer2.Checked = false;
            checkBoxPlayer2.Click += new EventHandler(player2_Click);
        }

        private void player2_Click(object sender, EventArgs e)
        {
            if((sender as CheckBox).Checked == true)
            {
                textBoxPlayer2Name.Enabled = true;
                textBoxPlayer2Name.Text = string.Empty;
            }
            else
            {
                textBoxPlayer2Name.Enabled = false;
                textBoxPlayer2Name.Text = "[Computer]";
            }
        }

        private void initDoneButton()
        {
            this.Controls.Add(buttonDone);
            buttonDone.Text = "Done";
            buttonDone.Top = 190;
            buttonDone.Left = textBoxPlayer1Name.Left;
        }
    }
}
