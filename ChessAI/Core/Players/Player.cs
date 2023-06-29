using Core.Chess;

namespace Core.Players {
    public enum TypePlayers {
        Human, AI
    }

    public abstract class Player {
        protected Game game;
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
        public void SetGame(Game game) {
            this.game = game;
        }
        public byte Color {
            get => this.color;
        }

        abstract public void NotifyTurnToMove();

        abstract public void NotifyResult(Game.GameState result);
    }
}