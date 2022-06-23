using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Game.Chess;
using Game.Players;
using UI;

namespace Game{
    public class GameManager : MonoBehaviour {

        public TypePlayers TypeWhitePlayer;
        public TypePlayers TypeBlackPlayer;
        public bool Export;
        public bool RenderBoard;

        public BoardUI boardUI;

        private Player whitePlayer;
        private Player blackPlayer;
        private Board board;
        private List<Move> gameMoves;

        private Board.BoardState result;
        private int requestsToDraw;

        public void Start() {
            this.gameMoves = new List<Move>();
            this.whitePlayer = this.CreatePlayer(TypeWhitePlayer, Piece.White);
            this.blackPlayer = this.CreatePlayer(TypeBlackPlayer, Piece.Black);

            if(!RenderBoard) {
                boardUI.transform.localScale = new Vector3(0, 0, 0);
            }

            this.NewGame();
        }

        public void Update() {
            if(result != Board.BoardState.Playing)
                return;

            if(this.board.TimeOf == Piece.White) {
                this.whitePlayer.Update();
            } else {
                this.blackPlayer.Update();
            }
        }

        public void NewGame() {
            if(whitePlayer == null || blackPlayer == null)
                throw new ArgumentNullException("Some player has not been defined");
            
            this.board = Board.CreateInitialPosition();
            this.boardUI.SetNewPosition(this.board);

            this.whitePlayer.SetBoard(this.board, this.boardUI);
            this.blackPlayer.SetBoard(this.board, this.boardUI);

            this.gameMoves.Clear();
            this.requestsToDraw = 0;
            this.result = Board.BoardState.Playing;
            this.notifyPlayerToMove();
        }

        private Player CreatePlayer(TypePlayers type, byte color) {
            if(type == TypePlayers.Human) {
                return new HumanPlayer(color, (move) => { this.PlayerChoseMove(move, color); },
                    () => { this.PlayerRequestDraw(color); },
                    () => { this.PlayerAdmitDefeat(color); });
            }
            return new AIPlayer(color, (move) => { this.PlayerChoseMove(move, color); },
                () => { this.PlayerRequestDraw(color); },
                () => { this.PlayerAdmitDefeat(color); });
        }

        private void PlayerChoseMove(Move move, byte color) {
            if(this.result != Board.BoardState.Playing)
                return;

            if(this.board.TimeOf == color) {
                this.board.TryMakeMove(move);
                if(this.board.TimeOf != color) {
                    this.gameMoves.Add(move);
                    this.requestsToDraw = 0;
                }
            }
            this.notifyPlayerToMove();
        }

        private void PlayerRequestDraw(byte color) {
            int mask = (color == Piece.White) ? 1 : 2;
            requestsToDraw |= mask;
            if(requestsToDraw == 3) {
                this.result = Board.BoardState.Draw;
                this.notifyResultToPlayers();
            }
        }

        private void PlayerAdmitDefeat(byte color) {
            this.result = color == Piece.White ? Board.BoardState.BlackWin : Board.BoardState.WhiteWin;
            this.notifyResultToPlayers();
        }

        private void notifyPlayerToMove() {
            this.result = this.board.GetBoardState();
            if(this.result == Board.BoardState.Playing) {
                if(this.board.TimeOf == Piece.White)
                    this.whitePlayer.NotifyTurnToMove();
                else
                    this.blackPlayer.NotifyTurnToMove();
            } else {
                this.notifyResultToPlayers();
            }
        }

        private void notifyResultToPlayers() {
            this.whitePlayer.NotifyResult(this.result);
            this.blackPlayer.NotifyResult(this.result);

            if(Export) 
                ExportGame();
        }

        private void ExportGame() {
            // implement in another day;
        }
    }
}

