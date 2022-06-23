using System;

using Game.Chess;
using UI;

namespace Game.Players {
    public enum TypePlayers {
        Human, AI
    }

    public abstract class Player {
        protected Board board;
        protected BoardUI boardUI;
        private byte color;

        public delegate void ChoseMove(Move move);
        public delegate void RequestDraw();
        public delegate void AdmitDefeat();

        protected ChoseMove choseMove;
        protected RequestDraw requestDraw;
        protected AdmitDefeat admitDefeat;

        public Player(byte color, ChoseMove chose, RequestDraw draw, AdmitDefeat defeat) {
            this.color = color;    
            this.choseMove = chose;
            this.requestDraw = draw;
            this.admitDefeat = defeat;
        }

        public void SetBoard(Board board, BoardUI boardUI) {
            this.board = board;
            this.boardUI = boardUI;
        }

        abstract public void NotifyTurnToMove();

        abstract public void NotifyResult(Board.BoardState result);

        abstract public void Update();
    }
}