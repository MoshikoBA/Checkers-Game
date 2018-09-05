using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Damka
{
    public class Checkers
    {
        public enum eStatus
        {
            PLAYER_1_WIN = 0,
            PLAYER_2_WIN,
            TIE,
            RUNNING,
            GAME_OVER
        }

        public const byte k_NumberOfPlayers = 2;
        public const int k_CanNotMove = -1;
        public const int k_CanNotEat = -1;

        private readonly Player[] m_Players;
        private readonly byte[,] m_Board;
        private readonly byte m_BoardSize;
        private readonly byte m_NumberOfToolForPlayer;
        private eStatus m_Status;

        public event EventHandler DeleteToolFromBoard;

        public event EventHandler StatusChanged;

        public Checkers(byte i_BoardSize, Player[] i_Players, byte i_NumberOfToolForPlayer)
        {
            m_BoardSize = i_BoardSize;
            m_Board = new byte[i_BoardSize, i_BoardSize];
            m_Players = i_Players;
            m_Players[0].m_ChangeStatusDelegate += CheckWhoWin;
            m_Players[1].m_ChangeStatusDelegate += CheckWhoWin;
            m_Status = eStatus.RUNNING;
            m_NumberOfToolForPlayer = i_NumberOfToolForPlayer;
        }

        public byte BoardSize
        {
            get
            {
                return m_BoardSize;
            }
        }

        public byte[,] Board
        {
            get
            {
                return m_Board;
            }
        }

        public Player[] Players
        {
            get
            {
                return m_Players;
            }
        }

        public eStatus Status
        {
            get
            {
                return m_Status;
            }

            set
            {
                m_Status = value;
                if(value != eStatus.RUNNING)
                {
                    OnStatusChanged();
                }
            }
        }

        public byte NumberOfToolsForEachPlayer
        {
            get
            {
                return m_NumberOfToolForPlayer;
            }
        }

        public void CheckWhoWin()
        {
            switch (m_Players[0].Status)
            {
                case Player.ePlayerStatus.QUIT:
                    m_Status = eStatus.PLAYER_2_WIN;
                    break;
                case Player.ePlayerStatus.CAN_NOT_MOVE:
                    if (m_Players[1].Status == Player.ePlayerStatus.CAN_NOT_MOVE)
                    {
                        m_Status = eStatus.TIE;
                    }
                    else
                    {
                        m_Status = eStatus.PLAYER_2_WIN;
                    }

                    break;
                case Player.ePlayerStatus.ACTIVE:
                    m_Status = eStatus.PLAYER_1_WIN;
                    break;
            }

            OnStatusChanged();
        }

        public string GetRoundStetus()
        {
            string roundStatus = string.Empty;
            switch (m_Status)
            {
                case eStatus.PLAYER_1_WIN:
                    roundStatus = string.Format("{0} Win.", m_Players[0].Name);
                    break;
                case eStatus.PLAYER_2_WIN:
                    roundStatus = string.Format("{0} Win.", m_Players[1].Name);
                    break;
                case eStatus.TIE:
                    roundStatus = string.Format("Tie");
                    break;
            }

            return roundStatus;
        }

        public bool IsValidEatMove(Point i_OriginalSourceTool, Point i_UserSourceInput, Point i_Destination, int i_CurrentPlayer, ref int io_EatenOtherPlayerToolIndex)
        {
            Tool currentTool = m_Players[i_CurrentPlayer].Tools[(m_Board[i_OriginalSourceTool.X, i_OriginalSourceTool.Y] - 1) % m_NumberOfToolForPlayer];

            return i_OriginalSourceTool.Equals(i_UserSourceInput) && currentTool.isMyEatenMove(i_Destination, ref io_EatenOtherPlayerToolIndex);
        }

        public bool IsValidPlayerMove(int i_CurrentPlayerIndex, Point i_SourceCoordinate, Point i_DestinatonCoordinate, ref int io_EatenOtherPlayerToolIndex)
        {
            Tool currentPlayerTool;
            bool isValidMove = false;
            int toolIndexOnBoard = k_CanNotEat;

                toolIndexOnBoard = (m_Board[i_SourceCoordinate.X, i_SourceCoordinate.Y] - 1) % m_NumberOfToolForPlayer;
                if (m_Players[i_CurrentPlayerIndex].IsMyTool(toolIndexOnBoard, i_SourceCoordinate))
                {
                    currentPlayerTool = m_Players[i_CurrentPlayerIndex].Tools[toolIndexOnBoard];

                    if (currentPlayerTool.isMyEatenMove(i_DestinatonCoordinate, ref io_EatenOtherPlayerToolIndex))
                    {
                        isValidMove = true;
                    }
                    else if (!m_Players[i_CurrentPlayerIndex].IsAnyToolCanEat() &&
                                (currentPlayerTool.IsMyRightMove(i_DestinatonCoordinate) ||
                                currentPlayerTool.IsMyLeftMove(i_DestinatonCoordinate)))
                    {
                        isValidMove = true;
                    }
                }

            return isValidMove;
        }

        private bool canPlayerQuit(int i_PlayerTurn)
        {
            bool canQuit = false;

            if (m_Players[i_PlayerTurn].GetNumberOfToolOnBoard() < m_Players[(i_PlayerTurn + 1) % k_NumberOfPlayers].GetNumberOfToolOnBoard())
            {
                canQuit = true;
            }

            return canQuit;
        }

        public void DeleteEatenTool(int i_CurrentPlayerTurn, int i_EatenOtherPlayerToolIndex)
        {
            int rowBoardOfTool = m_Players[i_CurrentPlayerTurn].Tools[i_EatenOtherPlayerToolIndex].Coordinate.X;
            int colBoardOfTool = m_Players[i_CurrentPlayerTurn].Tools[i_EatenOtherPlayerToolIndex].Coordinate.Y;

            m_Board[rowBoardOfTool, colBoardOfTool] = 0;
            m_Players[i_CurrentPlayerTurn].Tools[i_EatenOtherPlayerToolIndex] = null;
            DeleteToolFromBoard.Invoke(this, new DeletedToolEventArg(rowBoardOfTool, colBoardOfTool));
        }

        private void initBoard()
        {
            byte toolIndex = 1;

            Array.Clear(m_Board, 0, m_BoardSize * m_BoardSize);

            for (int i = 0; i < (m_BoardSize - 2) / 2; i++)
            {
                for (int j = 0; j < m_BoardSize; j++)
                {
                    if ((i % 2 == 0 && j % 2 == 1) || (i % 2 == 1 && j % 2 == 0))
                    {
                        m_Board[i, j] = (byte)(toolIndex + m_NumberOfToolForPlayer);
                        if (j % 2 == 0)
                        {
                            m_Board[m_BoardSize - i - 1, j + 1] = toolIndex;
                        }
                        else
                        {
                            m_Board[m_BoardSize - i - 1, j - 1] = toolIndex;
                        }

                        toolIndex++;
                    }
                }
            }
        }

        public void ResetRound()
        {
            initBoard();
            m_Players[0].Status = Player.ePlayerStatus.ACTIVE;
            m_Players[1].Status = Player.ePlayerStatus.ACTIVE;
            initPlayersTools();
            UpdatePlayerToolsMoves(0);
        }

        private void initPlayersTools()
        {
            int indexOfTool = 0;

            for (int i = 0; i < (m_BoardSize - 2) / 2; i++)
            {
                for (int j = 0; j < m_BoardSize; j++)
                {
                    if ((i % 2 == 0 && j % 2 == 1) || (i % 2 == 1 && j % 2 == 0))
                    {
                        m_Players[1].Tools[indexOfTool] = new Tool(i, j, 'O');
                        if (j % 2 == 0)
                        {
                            m_Players[0].Tools[indexOfTool] = new Tool(m_BoardSize - i - 1, j + 1, 'X');
                        }
                        else
                        {
                            m_Players[0].Tools[indexOfTool] = new Tool(m_BoardSize - i - 1, j - 1, 'X');
                        }

                        indexOfTool++;
                    }
                }
            }
        }

        public void GetMoveByComputer(ref Point io_SourceCoordinate, ref Point io_DestinationCoordinate, ref int io_EatenOtherPlayerToolIndex)
        {
            if (getMaxLengthEatingPath(ref io_SourceCoordinate, ref io_DestinationCoordinate, m_Players[1].Tools, ref io_EatenOtherPlayerToolIndex) == 0)
            {
                if (!canRunAwayFromEating(ref io_SourceCoordinate, ref io_DestinationCoordinate))
                {
                    if (!isAnyToolCanBeKingInTheNextMove(ref io_SourceCoordinate, ref io_DestinationCoordinate))
                    {
                        if (!isAnyToolCanMoveWithoutBeEaten(ref io_SourceCoordinate, ref io_DestinationCoordinate))
                        {
                            chooseRandomMoveForComputer(ref io_SourceCoordinate, ref io_DestinationCoordinate);
                        }
                    }
                }
            }
        }

        private int getMaxLengthEatingPath(ref Point io_SourceCoordinate, ref Point io_DestinationCoordinate, Tool[] i_ToolsToCheck, ref int io_EatenOtherPlayerToolIndex)
        {
            int maxEatingLengthPath = 0;
            List<int> maxArrayOfEatingLengthPathPerTool = new List<int>();
            int indextOfMaxLenghtOfEatingLength = 0;
            Tool tempTool;

            foreach (Tool currentTool in i_ToolsToCheck)
            {
                if (currentTool != null)
                {
                    for (int i = 0; i < currentTool.EatMoves.Count; i++)
                    {
                        tempTool = new Tool(currentTool.EatMoves[i], '$');
                        maxArrayOfEatingLengthPathPerTool.Add(getMaxLengthEatingPathPerTool(tempTool));
                    }

                    indextOfMaxLenghtOfEatingLength = findIndexOfMaxValue(maxArrayOfEatingLengthPathPerTool);
                    if (indextOfMaxLenghtOfEatingLength > -1 && maxArrayOfEatingLengthPathPerTool[indextOfMaxLenghtOfEatingLength] > maxEatingLengthPath)
                    {
                        maxEatingLengthPath = maxArrayOfEatingLengthPathPerTool[indextOfMaxLenghtOfEatingLength];
                        io_DestinationCoordinate = currentTool.EatMoves[indextOfMaxLenghtOfEatingLength];
                        io_SourceCoordinate = currentTool.Coordinate;
                        io_EatenOtherPlayerToolIndex = currentTool.EatenOtherPlayerToolIndex[indextOfMaxLenghtOfEatingLength];
                    }

                    maxArrayOfEatingLengthPathPerTool.Clear();
                }
            }

            return maxEatingLengthPath;
        }

        private int findIndexOfMaxValue(List<int> i_Array)
        {
            int maxValue = -1, indexMaxValue = -1;
            for (int i = 0; i < i_Array.Count; i++)
            {
                if (i_Array[i] > maxValue)
                {
                    maxValue = i_Array[i];
                    indexMaxValue = i;
                }
            }

            return indexMaxValue;
        }

        private int getMaxLengthEatingPathPerTool(Tool i_SourceTool)
        {
            List<Tool> eatMoveQueue = new List<Tool>();
            Tool toolOfQueue, tempTool;
            int maxLenght = 1;
            int[] direction = new int[] { 1, (int)m_Players[1].Direction };

            eatMoveQueue.Add(i_SourceTool);
            while (eatMoveQueue.Count > 0)
            {
                tempTool = eatMoveQueue[0];
                eatMoveQueue.RemoveAt(0);
                for (int i = 0; i < 2; i++)
                {
                    direction[0] *= -1;
                    direction[1] = (int)m_Players[1].Direction;
                    updateToolMoves(tempTool, 1, direction);
                    if (tempTool.IsKing)
                    {
                        direction[1] *= -1;
                        updateToolMoves(tempTool, 1, direction);
                    }
                }

                if (tempTool.EatMoves.Count > 0)
                {
                    maxLenght++;
                    foreach (Point current in tempTool.EatMoves)
                    {
                        toolOfQueue = new Tool(current, '@');
                        eatMoveQueue.Add(toolOfQueue);
                    }
                }
            }

            return maxLenght;
        }

        public void ContinuingEatingByComputer(Tool i_CurrentTool, ref Point io_Destination, ref int io_EatenOtherPlayerToolIndex)
        {
            Tool[] onlySourcrToolArray = new Tool[1] { i_CurrentTool };
            Point source = i_CurrentTool.Coordinate;

            getMaxLengthEatingPath(ref source, ref io_Destination, onlySourcrToolArray, ref io_EatenOtherPlayerToolIndex);
        }

        private bool canRunAwayFromEating(ref Point io_SourceCoordinate, ref Point io_DestinationCoordinate)
        {
            bool canRunAwayFromEating = false;

            foreach (Tool currentTool in m_Players[1].Tools)
            {
                if (currentTool != null && currentTool.CanBeEaten)
                {
                    if (tryRunAwayAndNotBeEatenInTheNextMove(currentTool, ref io_SourceCoordinate, ref io_DestinationCoordinate))
                    {
                        canRunAwayFromEating = true;
                        break;
                    }
                }
            }

            return canRunAwayFromEating;
        }

        private bool tryRunAwayAndNotBeEatenInTheNextMove(Tool i_CurrentTool, ref Point io_SourceCoordinate, ref Point io_DestinatinCoordinate)
        {
            bool isToolCanRunAway = false;

            foreach (Point leftNextMoveCoordinate in i_CurrentTool.LeftMoves)
            {
                if (!isToolEatenInTheNextMove(leftNextMoveCoordinate))
                {
                    io_SourceCoordinate = i_CurrentTool.Coordinate;
                    io_DestinatinCoordinate = leftNextMoveCoordinate;
                    isToolCanRunAway = true;
                    break;
                }
            }

            if (!isToolCanRunAway)
            {
                foreach (Point rightNextMoveCoordinate in i_CurrentTool.RightMoves)
                {
                    if (!isToolEatenInTheNextMove(rightNextMoveCoordinate))
                    {
                        io_SourceCoordinate = i_CurrentTool.Coordinate;
                        io_DestinatinCoordinate = rightNextMoveCoordinate;
                        isToolCanRunAway = true;
                        break;
                    }
                }
            }

            return isToolCanRunAway;
        }

        private bool isAnyToolCanBeKingInTheNextMove(ref Point io_SourceCoordinate, ref Point io_DestinationCoordinate)
        {
            bool beAKingInTheNextMove = false;

            foreach (Tool currentTool in m_Players[1].Tools)
            {
                if (!beAKingInTheNextMove && currentTool != null)
                {
                    foreach (Point nextLeftMoveCoordinate in currentTool.LeftMoves)
                    {
                        if (!currentTool.IsKing && nextLeftMoveCoordinate.X == m_BoardSize - 1)
                        {
                            io_SourceCoordinate = currentTool.Coordinate;
                            io_DestinationCoordinate = nextLeftMoveCoordinate;
                            beAKingInTheNextMove = true;
                            break;
                        }
                    }

                    if (!beAKingInTheNextMove)
                    {
                        foreach (Point nextRightMoveCoordinate in currentTool.RightMoves)
                        {
                            if (!currentTool.IsKing && nextRightMoveCoordinate.X == m_BoardSize - 1)
                            {
                                io_SourceCoordinate = currentTool.Coordinate;
                                io_DestinationCoordinate = nextRightMoveCoordinate;
                                beAKingInTheNextMove = true;
                                break;
                            }
                        }
                    }
                }
            }

            return beAKingInTheNextMove;
        }

        private bool isToolEatenInTheNextMove(Point i_NextMove)
        {
            Tool nextMove = new Tool(i_NextMove, 'a');
            int[] directionsMove = new int[] { 1, (int)m_Players[1].Direction };

            for (int i = 0; i < 2; i++)
            {
                directionsMove[0] *= -1;
                directionsMove[1] = (int)m_Players[1].Direction;
                updateToolMoves(nextMove, 1, directionsMove);
            }

            return nextMove.CanBeEaten;
        }

        public bool UpdatePlayerToolsMoves(int i_CurrentPlayer)
        {
            int[] nextToolMoveDirections = new int[] { 1, (int)m_Players[i_CurrentPlayer].Direction };
            int facotrForYCoordinateToMoveLeftOrRight = -1;
            bool isToolCanMove = false;

            foreach (Tool currentTool in m_Players[i_CurrentPlayer].Tools)
            {
                if (currentTool != null)
                {
                    currentTool.ResetMoves();
                    for (int i = 0; i < 2; i++)
                    {
                        nextToolMoveDirections[0] *= facotrForYCoordinateToMoveLeftOrRight;
                        updateToolMoves(currentTool, i_CurrentPlayer, nextToolMoveDirections);
                        if (currentTool.IsKing)
                        { ////For king tool we have to check 4 possible move options
                            nextToolMoveDirections[1] *= -1;
                            updateToolMoves(currentTool, i_CurrentPlayer, nextToolMoveDirections);
                        }
                    }

                    if (!isToolCanMove)
                    {
                        if (currentTool.EatMoves.Count > 0 || currentTool.LeftMoves.Count > 0
                            || currentTool.RightMoves.Count > 0)
                        {
                            isToolCanMove = true;
                        }
                    }
                }
            }

            return isToolCanMove;
        }

        private void updateToolMoves(Tool i_CurrentTool, int i_CurrentPlayer, int[] i_NextToolMoveDirectionsArray)
        {
            byte tempXCoordinateForNextMove = (byte)(i_CurrentTool.Coordinate.X + i_NextToolMoveDirectionsArray[1]);
            byte tempYCoordinateForNextMove = (byte)(i_CurrentTool.Coordinate.Y + i_NextToolMoveDirectionsArray[0]);
            int tempValueToolInBoard;
            Point tempToolCoordinatesForNextMove;

            if (tempYCoordinateForNextMove < m_BoardSize && tempYCoordinateForNextMove >= 0 && tempXCoordinateForNextMove < m_BoardSize && tempXCoordinateForNextMove >= 0)
            {
                tempValueToolInBoard = m_Board[tempXCoordinateForNextMove, tempYCoordinateForNextMove];
                tempToolCoordinatesForNextMove = new Point((int)tempXCoordinateForNextMove, (int)tempYCoordinateForNextMove);
                if (tempValueToolInBoard == 0)
                {
                    if (i_NextToolMoveDirectionsArray[0] > 0)
                    {
                        i_CurrentTool.RightMoves.Add(tempToolCoordinatesForNextMove);
                    }
                    else
                    {
                        i_CurrentTool.LeftMoves.Add(tempToolCoordinatesForNextMove);
                    }

                    updateIfToolCanBeEatenInBack(i_CurrentTool, (i_CurrentPlayer + 1) % k_NumberOfPlayers, i_NextToolMoveDirectionsArray);
                }
                else
                {
                    if (!m_Players[i_CurrentPlayer].IsMyTool((tempValueToolInBoard - 1) % m_NumberOfToolForPlayer, tempToolCoordinatesForNextMove))
                    {
                        updateIfToolCanBeEatenInFront(i_CurrentTool, i_NextToolMoveDirectionsArray);
                        updateEatToolsMoves(i_CurrentTool, i_CurrentPlayer, i_NextToolMoveDirectionsArray, tempToolCoordinatesForNextMove);
                    }
                }
            }
        }

        private void updateEatToolsMoves(Tool i_CurrentTool, int i_CurrentPlayer, int[] i_NextToolMoveDirectionsArray, Point i_TempToolCoordinatesForNextMove)
        {
            byte tempXCoordinateForNextMove = (byte)(i_TempToolCoordinatesForNextMove.X + i_NextToolMoveDirectionsArray[1]);
            byte tempYCoordinateForNextMove = (byte)(i_TempToolCoordinatesForNextMove.Y + i_NextToolMoveDirectionsArray[0]);

            if (tempYCoordinateForNextMove < m_BoardSize && tempYCoordinateForNextMove >= 0 &&
                tempXCoordinateForNextMove < m_BoardSize && tempXCoordinateForNextMove >= 0)
            {
                if (m_Board[tempXCoordinateForNextMove, tempYCoordinateForNextMove] == 0)
                {
                    i_CurrentTool.EatenOtherPlayerToolIndex.Add((m_Board[i_TempToolCoordinatesForNextMove.X, i_TempToolCoordinatesForNextMove.Y] - 1) % m_NumberOfToolForPlayer);
                    i_TempToolCoordinatesForNextMove.X = tempXCoordinateForNextMove;
                    i_TempToolCoordinatesForNextMove.Y = tempYCoordinateForNextMove;
                    i_CurrentTool.EatMoves.Add(i_TempToolCoordinatesForNextMove);
                }
            }
        }

        private void updateIfToolCanBeEatenInFront(Tool i_CurrentTool, int[] i_NextToolMoveDirectionsArray)
        {
            byte tempXCoordinateForNextMove = (byte)(i_CurrentTool.Coordinate.X + (i_NextToolMoveDirectionsArray[1] * -1));
            byte tempYCoordinateForNextMove = (byte)(i_CurrentTool.Coordinate.Y + (i_NextToolMoveDirectionsArray[0] * -1));

            if (tempYCoordinateForNextMove < m_BoardSize && tempYCoordinateForNextMove >= 0 &&
                tempXCoordinateForNextMove < m_BoardSize && tempXCoordinateForNextMove >= 0)
            {
                if (m_Board[tempXCoordinateForNextMove, tempYCoordinateForNextMove] == 0)
                {
                    i_CurrentTool.CanBeEaten = true;
                }
            }
        }

        private void updateIfToolCanBeEatenInBack(Tool i_CurrentTool, int i_SecondPlayerIndex, int[] i_NextToolMoveDirectionsArray)
        {
            byte tempXCoordinateForNextMove = (byte)(i_CurrentTool.Coordinate.X + (i_NextToolMoveDirectionsArray[1] * -1));
            byte tempYCoordinateForNextMove = (byte)(i_CurrentTool.Coordinate.Y + (i_NextToolMoveDirectionsArray[0] * -1));
            int indexToolOnBoard;
            Point locationOfOtherTool;

            if (tempYCoordinateForNextMove < m_BoardSize && tempYCoordinateForNextMove >= 0 &&
                tempXCoordinateForNextMove < m_BoardSize && tempXCoordinateForNextMove >= 0)
            {
                locationOfOtherTool = new Point(tempXCoordinateForNextMove, tempYCoordinateForNextMove);
                indexToolOnBoard = (m_Board[tempXCoordinateForNextMove, tempYCoordinateForNextMove] - 1) % m_NumberOfToolForPlayer;
                if (indexToolOnBoard >= 0)
                {
                    if (m_Players[i_SecondPlayerIndex].IsMyTool(indexToolOnBoard, locationOfOtherTool))
                    {
                        if (m_Players[i_SecondPlayerIndex].Tools[indexToolOnBoard].IsKing)
                        {
                            i_CurrentTool.CanBeEaten = true;
                        }
                    }
                }
            }
        }

        public void UpdatePlayerScore()
        {
            int player1ToolsValue = 0, player2ToolsValue = 0, roundValue = 0;

            if (m_Status != eStatus.TIE)
            {
                player1ToolsValue = getPlayerToolsValue(0);
                player2ToolsValue = getPlayerToolsValue(1);
                roundValue = Math.Abs(player1ToolsValue - player2ToolsValue);

                if (m_Status == eStatus.PLAYER_1_WIN)
                {
                    m_Players[0].Score += roundValue;
                }
                else
                {
                    m_Players[1].Score += roundValue;
                }
            }
        }

        private int getPlayerToolsValue(int i_PlayerIndex)
        {
            int playerToolValue = 0;

            foreach (Tool tool in m_Players[i_PlayerIndex].Tools)
            {
                if (tool != null)
                {
                    if (tool.IsKing)
                    {
                        playerToolValue += Tool.k_KingValue;
                    }
                    else
                    {
                        playerToolValue += Tool.k_SimpleToolValue;
                    }
                }
            }

            return playerToolValue;
        }

        private void chooseRandomMoveForComputer(ref Point io_SourceCoordinate, ref Point io_DestinationCoordinate)
        {
            List<Point> allPossibleMovesSource = new List<Point>();
            List<Point> allPossibleMovesDestination = new List<Point>();
            int randomMoveIndex;
            Random randomMoves = new Random();

            foreach (Tool currentTool in m_Players[1].Tools)
            {
                if (currentTool != null)
                {
                    foreach (Point nextLeftMoveCoordinate in currentTool.LeftMoves)
                    {
                        allPossibleMovesSource.Add(currentTool.Coordinate);
                        allPossibleMovesDestination.Add(nextLeftMoveCoordinate);
                    }

                    foreach (Point nextRightMoveCoordinate in currentTool.RightMoves)
                    {
                        allPossibleMovesSource.Add(currentTool.Coordinate);
                        allPossibleMovesDestination.Add(nextRightMoveCoordinate);
                    }
                }
            }

            randomMoveIndex = randomMoves.Next(allPossibleMovesSource.Count);
            io_SourceCoordinate = allPossibleMovesSource[randomMoveIndex];
            io_DestinationCoordinate = allPossibleMovesDestination[randomMoveIndex];
        }

        private bool isAnyToolCanMoveWithoutBeEaten(ref Point io_SourceCoordinate, ref Point io_DestinationCoordinate)
        {
            bool isAnyToolCanMoveWithoutBeEaten = false;
            byte originalBoardValue;

            for (int i = 0; i < m_Players[1].Tools.Length; i++)
            {
                Tool computerTool = m_Players[1].Tools[i];

                if (computerTool != null)
                {
                    originalBoardValue = m_Board[computerTool.Coordinate.X, computerTool.Coordinate.Y];
                    m_Board[computerTool.Coordinate.X, computerTool.Coordinate.Y] = 0;
                    foreach (Point tempNextMove in computerTool.LeftMoves)
                    {
                        if (!isToolEatenInTheNextMove(tempNextMove))
                        {
                            io_SourceCoordinate = computerTool.Coordinate;
                            io_DestinationCoordinate = tempNextMove;
                            isAnyToolCanMoveWithoutBeEaten = true;
                            break;
                        }
                    }

                    foreach (Point tempNextMove in computerTool.RightMoves)
                    {
                        if (!isToolEatenInTheNextMove(tempNextMove))
                        {
                            io_SourceCoordinate = computerTool.Coordinate;
                            io_DestinationCoordinate = tempNextMove;
                            isAnyToolCanMoveWithoutBeEaten = true;
                            break;
                        }
                    }

                    m_Board[computerTool.Coordinate.X, computerTool.Coordinate.Y] = originalBoardValue;
                }
            }

            return isAnyToolCanMoveWithoutBeEaten;
        }

        protected virtual void OnStatusChanged()
        {
            if(StatusChanged != null)
            {
                StatusChanged.Invoke(this, new EventArgs());
            }
        }
    }
}
