using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Media;
using Damka;

namespace DamkaApp
{
    public enum eBoardSize
    {
        SMALL = 6,
        MEDIUM = 8,
        LARGE = 10
    }

    public class DamkaGame : Form
    {
        private const int k_ButtonToolSize = 60;
        private const int k_IconUserSize = 50;
        private readonly Label[] m_LabelPlayersDetails = new Label[2];
        private readonly Label[] m_LabelPlayersScore = new Label[2];
        private readonly GameSetting gameSetting = new GameSetting();
        private readonly GameManager m_GameManager = new GameManager();
        private ButtonTool[,] m_ButtonToolGameTools;
        private Checkers m_DamkaLogic;

        protected override void OnClosing(CancelEventArgs e)
        {
            if (m_DamkaLogic.Players[0].Status == Player.ePlayerStatus.ACTIVE && m_DamkaLogic.Players[1].Status == Player.ePlayerStatus.ACTIVE)
            {
                if (m_DamkaLogic.Players[m_GameManager.PlayerTurn].GetNumberOfToolOnBoard() >= m_DamkaLogic.Players[(m_GameManager.PlayerTurn + 1) % Checkers.k_NumberOfPlayers].GetNumberOfToolOnBoard())
                {
                    m_GameManager.ErrorSound.Play();
                    MessageBox.Show(string.Format("You Can't Quit{0}(You do not have less tools on board than the other player)", Environment.NewLine));
                }
                else
                {
                    DialogResult res = MessageBox.Show(string.Format("{0}, Are you sure you want to quit?", m_DamkaLogic.Players[m_GameManager.PlayerTurn].Name), "Damka", MessageBoxButtons.YesNo);
                    if (res == DialogResult.Yes)
                    {
                        m_DamkaLogic.Players[m_GameManager.PlayerTurn].Status = Player.ePlayerStatus.QUIT;
                    }
                }

                if (m_DamkaLogic.Status != Checkers.eStatus.GAME_OVER)
                {
                    e.Cancel = true;
                }
            }
            else
            {
                e.Cancel = false;
            }
        }

        public ButtonTool[,] BoardTools
        {
            get
            {
                return m_ButtonToolGameTools;
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Damka";
            this.Height = (m_DamkaLogic.BoardSize + 3) * k_ButtonToolSize;
            this.Width = (m_DamkaLogic.BoardSize + 2) * k_ButtonToolSize;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.Fixed3D;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.BackgroundImage = Properties.Resources.wood;
            m_GameManager.ComputerTimer.Tick += new EventHandler(Computer_Turn);
            initBoardTools(m_DamkaLogic.BoardSize);
            initPlayersDetails(m_DamkaLogic);
            initPlayersIcons();
            initPlayersScore();
        }

        private void initPlayersDetails(Checkers i_LogicDamka)
        {
            StringBuilder tempPlayerDetail = new StringBuilder();

            for (int i = 0; i < Checkers.k_NumberOfPlayers; i++)
            {
                m_LabelPlayersDetails[i] = new Label();
                this.Controls.Add(m_LabelPlayersDetails[i]);
                m_LabelPlayersDetails[i].Font = new Font("serif", 12, FontStyle.Bold);
                tempPlayerDetail.Append(i_LogicDamka.Players[i].Name).Append(":");
                m_LabelPlayersDetails[i].Text = tempPlayerDetail.ToString();
                tempPlayerDetail.Remove(0, tempPlayerDetail.Length);
                m_LabelPlayersDetails[i].BackColor = Color.Transparent;
                m_LabelPlayersDetails[i].Top = 10;
                m_LabelPlayersDetails[i].AutoSize = true;
            }

            m_LabelPlayersDetails[0].Left = this.Width / 4;
            m_LabelPlayersDetails[1].Left = (this.Width / 2) + (this.Width / 8);
            m_LabelPlayersDetails[0].BackColor = Color.LightBlue;
        }

        private void initPlayersIcons()
        {
            PictureBox iconUser;

            for (int i = 0; i < Checkers.k_NumberOfPlayers; i++)
            {
                iconUser = new PictureBox();
                if (getPlayer2Type() == Player.ePlayerType.BOT && i == 1)
                {
                    iconUser.BackgroundImage = Properties.Resources.Computer_Icon;
                }
                else
                {
                    iconUser.BackgroundImage = Properties.Resources.Human_Icon;
                }

                iconUser.Height = iconUser.Width = k_IconUserSize;
                iconUser.Top = m_LabelPlayersDetails[i].Bottom;
                iconUser.Left = m_LabelPlayersDetails[i].Left + (m_LabelPlayersDetails[i].Width / 4);
                iconUser.BackgroundImageLayout = ImageLayout.Stretch;
                iconUser.BackColor = Color.Transparent;
                this.Controls.Add(iconUser);
            }
        }

        private void initPlayersScore()
        {
            for (int i = 0; i < Checkers.k_NumberOfPlayers; i++)
            {
                m_LabelPlayersScore[i] = new Label();
                this.Controls.Add(m_LabelPlayersScore[i]);
                m_LabelPlayersScore[i].Font = new Font("serif", 12, FontStyle.Bold);
                m_LabelPlayersScore[i].AutoSize = true;
                m_LabelPlayersScore[i].Text = "0";
                m_LabelPlayersScore[i].BackColor = Color.Transparent;
                m_LabelPlayersScore[i].Top = m_LabelPlayersDetails[i].Top;
                m_LabelPlayersScore[i].Left = m_LabelPlayersDetails[i].Right;
            }
        }

        private void gameSetting_Done(object sender, EventArgs e)
        {
            createLogicDamka();
            InitializeComponent();
            gameSetting.Close();
        }

        private void createLogicDamka()
        {
            byte boardSize = gameSetting.BoardSizeChosen;
            byte numberOfToolForPlayer = (byte)(((boardSize - 2) / 2) * (boardSize / 2));
            Player.ePlayerType player2Type = getPlayer2Type();
            Player[] players = new Player[2];

            players[0] = new Player(gameSetting.Player1Name, Player.eDirection.DOWN_TO_UP, Player.ePlayerType.HUMAN, numberOfToolForPlayer, 'X');
            players[1] = new Player(gameSetting.Player2Name, Player.eDirection.UP_TO_DOWN, player2Type, numberOfToolForPlayer, 'O');
            m_DamkaLogic = new Checkers(boardSize, players, numberOfToolForPlayer);
            m_DamkaLogic.StatusChanged += new EventHandler(GameStatus_Changed);
            m_DamkaLogic.ResetRound();
            addLogicDamkaToolsListenerOfBecameKing();
            players[0].AfterPlay += new EventHandler(player_Played);
            players[1].AfterPlay += new EventHandler(player_Played);
            m_DamkaLogic.DeleteToolFromBoard += new EventHandler(tool_RemovedFromBoard);
            m_DamkaLogic.UpdatePlayerToolsMoves(m_GameManager.PlayerTurn);
        }

        private void addLogicDamkaToolsListenerOfBecameKing()
        {
            for (int i = 0; i < m_DamkaLogic.NumberOfToolsForEachPlayer; i++)
            {
                m_DamkaLogic.Players[0].Tools[i].BecameKing += new EventHandler(Tool_BecameKing);
                m_DamkaLogic.Players[1].Tools[i].BecameKing += new EventHandler(Tool_BecameKing);
            }
        }

        private Player.ePlayerType getPlayer2Type()
        {
            Player.ePlayerType player2Type;

            if (gameSetting.Playe2CheckBox.Checked)
            {
                player2Type = Player.ePlayerType.HUMAN;
            }
            else
            {
                player2Type = Player.ePlayerType.BOT;
            }

            return player2Type;
        }

        private void initBoardTools(byte i_BoardSize)
        {
            int factorTop = this.Height / i_BoardSize;
            int factorLeft = 50;

            m_ButtonToolGameTools = new ButtonTool[m_DamkaLogic.BoardSize, m_DamkaLogic.BoardSize];
            for (int i = 0; i < i_BoardSize; i++)
            {
                for (int j = 0; j < i_BoardSize; j++)
                {
                    m_ButtonToolGameTools[i, j] = new ButtonTool(new Point(i, j));
                    m_ButtonToolGameTools[i, j].Width = k_ButtonToolSize;
                    m_ButtonToolGameTools[i, j].Height = k_ButtonToolSize;
                    m_ButtonToolGameTools[i, j].Top = factorTop;
                    m_ButtonToolGameTools[i, j].Left = factorLeft;
                    m_ButtonToolGameTools[i, j].AutoSize = true;
                    m_ButtonToolGameTools[i, j].FlatStyle = FlatStyle.Flat;
                    m_ButtonToolGameTools[i, j].FlatAppearance.BorderSize = 1;
                    m_ButtonToolGameTools[i, j].FlatAppearance.BorderColor = Color.Maroon;

                    if ((i % 2 == 0 && j % 2 == 0) || (i % 2 != 0 && j % 2 != 0))
                    {
                        m_ButtonToolGameTools[i, j].BackColor = Color.FromArgb(249, 250, 197);
                        m_ButtonToolGameTools[i, j].Enabled = false;
                    }
                    else
                    {
                        if (i <= (i_BoardSize / 2) - 2)
                        {
                            m_ButtonToolGameTools[i, j].BackgroundImage = Properties.Resources.BlackTool;
                        }
                        else if ((i <= i_BoardSize) && i >= ((i_BoardSize / 2) + 1))
                        {
                            m_ButtonToolGameTools[i, j].BackgroundImage = Properties.Resources.WhiteTool;
                        }

                        m_ButtonToolGameTools[i, j].BackgroundImageLayout = ImageLayout.Stretch;
                        m_ButtonToolGameTools[i, j].Click += new EventHandler(tool_Click);
                        m_ButtonToolGameTools[i, j].OriginalBackColor = m_ButtonToolGameTools[i, j].BackColor = Color.FromArgb(123, 79, 16);
                    }

                    factorLeft += k_ButtonToolSize;
                    this.Controls.Add(m_ButtonToolGameTools[i, j]);
                }

                factorTop += k_ButtonToolSize;
                factorLeft = 50;
            }

            initBoardToolsIndex();
        }

        private void initBoardToolsIndex()
        {
            int indexForTool = 1;
            int xFactor = (int)m_DamkaLogic.BoardSize - 1, yFactor = -1;

            for (int i = 0; i < (m_DamkaLogic.BoardSize - 2) / 2; i++)
            {
                for (int j = 0; j < m_DamkaLogic.BoardSize; j++)
                {
                    if ((i % 2 == 0 && j % 2 == 1) || (i % 2 == 1 && j % 2 == 0))
                    {
                        m_ButtonToolGameTools[i, j].Index = indexForTool + m_DamkaLogic.NumberOfToolsForEachPlayer;
                        m_ButtonToolGameTools[i + xFactor, j + yFactor].Index = indexForTool;
                        indexForTool++;
                    }
                }

                xFactor -= 2;
                yFactor *= -1;
            }
        }

        private void tool_Click(object sender, EventArgs e)
        {
            if (m_DamkaLogic.Players[m_GameManager.PlayerTurn].Type == Player.ePlayerType.HUMAN)
            {
                if (!(sender as ButtonTool).IsClicked)
                {
                    if (m_GameManager.LastToolEat != null)
                    {
                        continueEating(sender as ButtonTool);
                    }
                    else
                    {
                        (sender as ButtonTool).IsClicked = true;
                        if (m_GameManager.SourceToolCoordinate.X == -1)
                        {
                            m_GameManager.SourceToolCoordinate = (sender as ButtonTool).Coordinate;
                        }
                        else
                        {
                            getRegularMove(sender as ButtonTool);
                        }
                    }
                }
                else
                {
                    (sender as ButtonTool).BackColor = (sender as ButtonTool).OriginalBackColor;
                    (sender as ButtonTool).IsClicked = false;
                    m_GameManager.SourceToolCoordinate = new Point(-1, -1);
                }
            }
        }

        private void getRegularMove(ButtonTool i_ButtonToolClicked)
        {
            int eatenToolIndex = -1;

            m_GameManager.DestinationToolCoordinate = i_ButtonToolClicked.Coordinate;
            if (m_DamkaLogic.IsValidPlayerMove(m_GameManager.PlayerTurn, m_GameManager.SourceToolCoordinate, m_GameManager.DestinationToolCoordinate, ref eatenToolIndex))
            {
                m_GameManager.EeatenIndexTool = eatenToolIndex;

                resetToolsAfterClick();
                m_DamkaLogic.Players[m_GameManager.PlayerTurn].Play(m_DamkaLogic.Board, m_GameManager.SourceToolCoordinate, m_GameManager.DestinationToolCoordinate);
            }
            else
            {
                m_GameManager.ErrorSound.Play();
                MessageBox.Show("Invalid Move");
            }

            resetToolsAfterClick();
            resetSourceAndDestinationAfterMove();
        }

        private void resetToolsAfterClick()
        {
            if (m_GameManager.SourceToolCoordinate.X != -1 && m_GameManager.DestinationToolCoordinate.X != -1)
            {
                m_ButtonToolGameTools[m_GameManager.SourceToolCoordinate.X, m_GameManager.SourceToolCoordinate.Y].IsClicked = false;
                m_ButtonToolGameTools[m_GameManager.DestinationToolCoordinate.X, m_GameManager.DestinationToolCoordinate.Y].IsClicked = false;
            }
        }

        private void resetSourceAndDestinationAfterMove()
        {
            m_GameManager.SourceToolCoordinate = new Point(-1, -1);
            m_GameManager.DestinationToolCoordinate = new Point(-1, -1);
        }

        private void continueEating(ButtonTool i_ToolClicked)
        {
            int eatenIndex = -1;

            if (i_ToolClicked != m_GameManager.LastToolEat)
            {
                if (!m_GameManager.LastToolEat.IsClicked)
                {
                    m_GameManager.ErrorSound.Play();
                    MessageBox.Show("Invalid Move");
                }
                else
                {
                    m_GameManager.DestinationToolCoordinate = i_ToolClicked.Coordinate;
                    i_ToolClicked.IsClicked = true;
                    if (m_DamkaLogic.IsValidPlayerMove(m_GameManager.PlayerTurn, m_GameManager.SourceToolCoordinate, m_GameManager.DestinationToolCoordinate, ref eatenIndex))
                    {
                        m_GameManager.EeatenIndexTool = eatenIndex;
                        m_DamkaLogic.Players[m_GameManager.PlayerTurn].Play(m_DamkaLogic.Board, m_GameManager.SourceToolCoordinate, m_GameManager.DestinationToolCoordinate);
                    }
                    else
                    {
                        m_GameManager.ErrorSound.Play();
                        MessageBox.Show("Invalid Move");
                    }

                    resetToolsAfterClick();
                    resetSourceAndDestinationAfterMove();
                }
            }
            else
            {
                i_ToolClicked.IsClicked = true;
                m_GameManager.SourceToolCoordinate = m_GameManager.LastToolEat.Coordinate;
                m_GameManager.EeatenIndexTool = -1;
            }
        }

        public void Run()
        {
            gameSetting.DoneButton.Click += new EventHandler(gameSetting_Done);
            gameSetting.ShowDialog();
            if (m_DamkaLogic != null)
            {
                this.ShowDialog();
            }
        }

        private void player_Played(object sender, EventArgs e)
        {
            moveToolOnBoard();
            if (m_GameManager.EeatenIndexTool != -1)
            {
                m_GameManager.LastToolEat = m_ButtonToolGameTools[m_GameManager.DestinationToolCoordinate.X, m_GameManager.DestinationToolCoordinate.Y];
                m_DamkaLogic.DeleteEatenTool((m_GameManager.PlayerTurn + 1) % Checkers.k_NumberOfPlayers, m_GameManager.EeatenIndexTool);
                m_DamkaLogic.UpdatePlayerToolsMoves(m_GameManager.PlayerTurn);
                int indexOfTool = (m_ButtonToolGameTools[m_GameManager.DestinationToolCoordinate.X, m_GameManager.DestinationToolCoordinate.Y].Index - 1) % m_DamkaLogic.NumberOfToolsForEachPlayer;

                if ((sender as Player).Tools[indexOfTool].EatMoves.Count == 0)
                {
                    changePlayerTurn();
                    if (!m_DamkaLogic.UpdatePlayerToolsMoves(m_GameManager.PlayerTurn))
                    {
                        m_DamkaLogic.Players[m_GameManager.PlayerTurn].Status = Player.ePlayerStatus.CAN_NOT_MOVE;
                    }

                    m_GameManager.LastToolEat = null;
                    m_GameManager.EeatenIndexTool = -1;
                    m_DamkaLogic.UpdatePlayerToolsMoves(m_GameManager.PlayerTurn);
                }
            }
            else
            {
                changePlayerTurn();
                if (!m_DamkaLogic.UpdatePlayerToolsMoves(m_GameManager.PlayerTurn))
                {
                    m_DamkaLogic.Players[m_GameManager.PlayerTurn].Status = Player.ePlayerStatus.CAN_NOT_MOVE;
                }
            }

            if (m_DamkaLogic.Players[m_GameManager.PlayerTurn].Type == Player.ePlayerType.BOT)
            {
                m_GameManager.ComputerTimer.Start();
            }
        }

        private void Computer_Turn(object sender, EventArgs e)
        {
            (sender as Timer).Stop();
            getMoveByComputer();
        }

        private void getMoveByComputer()
        {
            Point source = new Point(-1, -1);
            Point destination = new Point(-1, -1);
            int eatenToolIndex = -1;

            if (m_GameManager.LastToolEat != null)
            {
                m_DamkaLogic.ContinuingEatingByComputer(m_DamkaLogic.Players[1].Tools[(m_GameManager.LastToolEat.Index - 1) % m_DamkaLogic.NumberOfToolsForEachPlayer], ref destination, ref eatenToolIndex);
                m_GameManager.SourceToolCoordinate = m_GameManager.LastToolEat.Coordinate;
            }
            else
            {
                m_DamkaLogic.GetMoveByComputer(ref source, ref destination, ref eatenToolIndex);
                m_GameManager.SourceToolCoordinate = source;
            }

            m_GameManager.DestinationToolCoordinate = destination;
            m_GameManager.EeatenIndexTool = eatenToolIndex;
            m_DamkaLogic.Players[m_GameManager.PlayerTurn].Play(m_DamkaLogic.Board, m_GameManager.SourceToolCoordinate, m_GameManager.DestinationToolCoordinate);
            resetSourceAndDestinationAfterMove();
        }

        private void changePlayerTurn()
        {
            m_LabelPlayersDetails[m_GameManager.PlayerTurn].BackColor = Color.Transparent;
            m_GameManager.PlayerTurn = (m_GameManager.PlayerTurn + 1) % Checkers.k_NumberOfPlayers;
            m_LabelPlayersDetails[m_GameManager.PlayerTurn].BackColor = Color.LightBlue;
        }

        private void moveToolOnBoard()
        {
            if (m_GameManager.EeatenIndexTool != -1)
            {
                m_GameManager.CaptureSound.Play();
            }
            else
            {
                m_GameManager.MoveSound.Play();
            }

            m_ButtonToolGameTools[m_GameManager.DestinationToolCoordinate.X, m_GameManager.DestinationToolCoordinate.Y].BackgroundImage = m_ButtonToolGameTools[m_GameManager.SourceToolCoordinate.X, m_GameManager.SourceToolCoordinate.Y].BackgroundImage;
            m_ButtonToolGameTools[m_GameManager.DestinationToolCoordinate.X, m_GameManager.DestinationToolCoordinate.Y].Index = m_ButtonToolGameTools[m_GameManager.SourceToolCoordinate.X, m_GameManager.SourceToolCoordinate.Y].Index;
            m_ButtonToolGameTools[m_GameManager.SourceToolCoordinate.X, m_GameManager.SourceToolCoordinate.Y].BackgroundImage = null;
            m_ButtonToolGameTools[m_GameManager.SourceToolCoordinate.X, m_GameManager.SourceToolCoordinate.Y].Index = 0;
        }

        private void tool_RemovedFromBoard(object sender, EventArgs e)
        {
            m_ButtonToolGameTools[(e as DeletedToolEventArg).Row, (e as DeletedToolEventArg).Col].BackgroundImage = null;
        }

        private void Tool_BecameKing(object sender, EventArgs e)
        {
            if ((sender as Tool).Sign == m_DamkaLogic.Players[0].ToolsSign)
            {
                m_ButtonToolGameTools[m_GameManager.SourceToolCoordinate.X, m_GameManager.SourceToolCoordinate.Y].BackgroundImage = Properties.Resources.WhiteCrown;
            }
            else
            {
                m_ButtonToolGameTools[m_GameManager.SourceToolCoordinate.X, m_GameManager.SourceToolCoordinate.Y].BackgroundImage = Properties.Resources.BlackCrown;
            }
        }

        private void GameStatus_Changed(object sender, EventArgs e)
        {
            Checkers.eStatus gameStatus = (sender as Checkers).Status;

            m_DamkaLogic.UpdatePlayerScore();
            if (gameStatus == Checkers.eStatus.GAME_OVER)
            {
                MessageBox.Show("Game Over. Bye Bye");
                Close();
            }
            else
            {
                endOfRoundCare(gameStatus);
            }
        }

        private void endOfRoundCare(Checkers.eStatus i_GameStatus)
        {
            string winnerPlayerName;

            m_LabelPlayersScore[0].Text = m_DamkaLogic.Players[0].Score.ToString();
            m_LabelPlayersScore[1].Text = m_DamkaLogic.Players[1].Score.ToString();

            m_GameManager.RoundOverSound.Play();
            if (i_GameStatus != Checkers.eStatus.TIE)
            {
                winnerPlayerName = string.Format("{0} Win.", m_DamkaLogic.Players[(int)i_GameStatus].Name);
            }
            else
            {
                winnerPlayerName = "Tie";
            }

            DialogResult result = MessageBox.Show(string.Format("{0}{1}Do you want another round?", winnerPlayerName, Environment.NewLine), "Damka", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                playAnotherRound();
            }
            else
            {
                m_DamkaLogic.Status = Checkers.eStatus.GAME_OVER;
            }
        }

        private void playAnotherRound()
        {
            m_DamkaLogic.ResetRound();
            addLogicDamkaToolsListenerOfBecameKing();
            claerBoard();
            initBoardTools(m_DamkaLogic.BoardSize);
            initPlayersDetails(m_DamkaLogic);
            m_GameManager.InitProperties();
            m_LabelPlayersDetails[0].BackColor = Color.LightBlue;
            m_LabelPlayersDetails[1].BackColor = Color.Transparent;
        }

        private void claerBoard()
        {
            for (int i = 0; i < m_DamkaLogic.BoardSize; i++)
            {
                for (int j = 0; j < m_DamkaLogic.BoardSize; j++)
                {
                    this.Controls.Remove(m_ButtonToolGameTools[i, j]);
                }
            }
        }
    }
}