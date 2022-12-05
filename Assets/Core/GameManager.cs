using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Core.Chess;
using Core.Players;
using UI;

namespace Core{
    public class GameManager : MonoBehaviour {
        public BoardUI boardUI;
        public TypePlayers TypeWhitePlayer;
        public TypePlayers TypeBlackPlayer;

        private Player whitePlayer;
        private Player blackPlayer;
        private Game game;

        private int requestsToDraw;
    
        public void Start() {
            this.whitePlayer = this.CreatePlayer(TypeWhitePlayer, Piece.White);
            this.blackPlayer = this.CreatePlayer(TypeBlackPlayer, Piece.Black);

            this.NewGame();
        }

        public void Update() {
            if(this.game.CurrentState != Game.GameState.Playing)
                return;

            Console.WriteLine(boardUI);
            if(this.game.TimeOf == Piece.White)
                this.whitePlayer.Update(this.boardUI);
            else
                this.blackPlayer.Update(this.boardUI);
        }

        public void NewGame() {
            if(whitePlayer == null || blackPlayer == null)
                throw new ArgumentNullException("Some player has not been defined");
            
            this.game = Game.CreateInitialPosition();

            this.whitePlayer.SetGame(this.game);
            this.blackPlayer.SetGame(this.game);

            this.requestsToDraw = 0;
            this.boardUI.SetNewPosition(this.game.GetPosition(0));
            this.NotifyPlayerToMove();
        }

        private Player CreatePlayer(TypePlayers type, byte color) {
            Player.ChoseMove move = (move) => { this.PlayerChoseMove(move, color); };
            Player.RequestDraw draw = () => { this.PlayerRequestDraw(color); };
            Player.AdmitDefeat defeat = () => { this.PlayerAdmitDefeat(color); };

            if(type == TypePlayers.Human)
                return new HumanPlayer(color, move, draw, defeat);
            return new AIPlayer(color, move, draw, defeat);
        }

        private void PlayerChoseMove(Move move, byte color) {
            if(this.game.CurrentState != Game.GameState.Playing)
                return;

            if(this.game.TimeOf == color) {
                if(this.game.TryMakeMove(move)) 
                    this.requestsToDraw = 0;
            }
            this.boardUI.SetNewPosition(this.game.GetPosition(this.game.TotalPositions - 1));
            this.NotifyPlayerToMove();
        }

        private void PlayerRequestDraw(byte color) {
            int mask = (color == Piece.White) ? 1 : 2;
            this.requestsToDraw |= mask;
            if(requestsToDraw == 3) {
                this.game.CurrentState = Game.GameState.Draw;
                this.NotifyResultToPlayers();
            }
        }

        private void PlayerAdmitDefeat(byte color) {
            if(this.game.CurrentState != Game.GameState.Playing)
                return;
                
            this.game.
            CurrentState = color == Piece.White ? Game.GameState.BlackWin : Game.GameState.WhiteWin;
            this.NotifyResultToPlayers();
        }

        private void NotifyPlayerToMove() {
            if(this.game.CurrentState != Game.GameState.Playing){
                this.NotifyResultToPlayers();
                return;
            }

            if(this.game.TimeOf == Piece.White)
                this.whitePlayer.NotifyTurnToMove();
            else
                this.blackPlayer.NotifyTurnToMove();
            
        }

        private void NotifyResultToPlayers() {
            this.whitePlayer.NotifyResult(this.game.CurrentState);
            this.blackPlayer.NotifyResult(this.game.CurrentState);
        }
    }
}

