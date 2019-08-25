﻿using GameInterface.Cells;
using MCTProcon30Protocol.Methods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCTProcon30Protocol;

namespace GameInterface.GameManagement
{
    public class GameData
    {
        public byte FinishTurn { get; set; } = 60;
        public int TimeLimitSeconds { get; set; } = 5;

        private MainWindowViewModel viewModel;
        private System.Random rand = new System.Random();
        //---------------------------------------
        //ViewModelと連動させるデータ(画面上に現れるデータ)
        private Cell[,] cellDataValue = null;
        public Cell[,] CellData
        {
            get => cellDataValue;
            private set
            {
                cellDataValue = value;
                viewModel.CellData = value;
            }
        }
        private Agent[] agents;
        public Agent[] Agents
        {
            get => agents;
            set {
                agents = value;
                for (int i = 0; i < agents.Length; ++i)
                {
                    viewModel.AgentViewModels[i].Data = agents[i];
                }
            }
        }
        private int[] playerScores = new int[2];
        public int[] PlayerScores
        {
            get => playerScores;
            set
            {
                playerScores = value;
                viewModel.PlayerScores = value;
            }
        }
        //----------------------------------------
        public int SecondCount { get; set; }
        public bool IsGameStarted { get; set; } = false;
        public bool IsNextTurnStart { get; set; } = true;
        public bool IsAutoSkipTurn { get; set; }
        public bool IsPause { get; set; }
        public int NowTurn { get; set; }
        public int BoardHeight { get; private set; }
        public int BoardWidth { get; private set; }
        public int SelectPosAgent { get; set; }

        public int AgentsCount { get; set; } = 2;

        public GameSettings.SettingStructure CurrentGameSettings { get; set; }

        public List<Decision>[] Decisions = new List<Decision>[2];
        
        public GameData(MainWindowViewModel _viewModel)
        {
            viewModel = _viewModel;
            agents = _viewModel.AgentViewModels.Select(x => x.Data).ToArray();
        }

        public void InitGameData(GameSettings.SettingStructure settings)
        {
            CurrentGameSettings = settings;
            SecondCount = 0;
            NowTurn = 1;
            FinishTurn = settings.Turns;
            TimeLimitSeconds = settings.LimitTime;
            IsAutoSkipTurn = settings.IsAutoSkip;

            if(settings.BoardCreation == GameSettings.BoardCreation.JsonFile)
            {
                settings.BoardWidth = (byte)settings.JsonCell.GetLength(0);
                settings.BoardHeight = (byte)settings.JsonCell.GetLength(1);
            }

            InitCellData(settings);
            InitAgents(settings);
        }

        void InitCellData(GameSettings.SettingStructure settings)
        {
            if (settings.BoardCreation == GameSettings.BoardCreation.Random)
            {
                BoardHeight = settings.BoardHeight;
                BoardWidth = settings.BoardWidth;

                //水平方向か垂直方向のどちらかを対称にするフラグをランダムに立てる
                bool isVertical;
                bool isHorizontal;
                do
                {
                    isVertical = rand.Next(2) == 1 ? true : false;
                    isHorizontal = rand.Next(2) == 1 ? true : false;
                } while (!isVertical && !isHorizontal);

                int randWidth, randHeight;
                randWidth = (isHorizontal) ? (BoardWidth + 1) / 2 : BoardWidth;
                randHeight = (isVertical) ? (BoardHeight + 1) / 2 : BoardHeight;

                CellData = new Cell[BoardWidth, BoardHeight];
                for (int i = 0; i < BoardWidth; i++)
                {
                    for (int j = 0; j < BoardHeight; j++)
                    {
                        if (i < randWidth && j < randHeight)
                        {
                            //10%の確率で値を0以下にする
                            if (rand.Next(1, 100) > 10)
                                CellData[i, j] = new Cell(rand.Next(1, 14));
                            else
                                CellData[i, j] = new Cell(rand.Next(-14, 0));
                        }
                        else if (j >= randHeight)
                            CellData[i, j] = new Cell(CellData[i, BoardHeight - 1 - j].Score);
                        else
                            CellData[i, j] = new Cell(CellData[BoardWidth - 1 - i, j].Score);
                    }
                }
            }
            else
            {
                CellData = settings.JsonCell;
                BoardWidth = CellData.GetLength(0);
                BoardHeight = CellData.GetLength(1);
            }
        }

        void InitAgents(GameSettings.SettingStructure settings)
        {
            /*
            配置は
            0 2
            3 1
            */
            int[] agentsX = new int[4];
            int[] agentsY = new int[4];
            agentsX[0] = agentsX[3] = rand.Next(1, BoardWidth / 2 - 1);
            agentsY[0] = agentsY[2] = rand.Next(1, BoardHeight / 2 - 1);
            agentsX[2] = agentsX[1] = BoardWidth - 1 - agentsX[0];
            agentsY[3] = agentsY[1] = BoardHeight - 1 - agentsY[0];
            for (int i = 0; i < App.PlayersCount; i++)
            {
                Agents[i].playerNum = i;
                Agents[i].Point = new Point((byte)agentsX[i], (byte)agentsY[i]);
                CellData[agentsX[i], agentsY[i]].AreaState_ =
                    i == 0 ? TeamColor.Area1P : TeamColor.Area2P;

                CellData[agentsX[i], agentsY[i]].AgentState =
                    i == 0 ? TeamColor.Area1P : TeamColor.Area2P;
            }
        }
    }
}
