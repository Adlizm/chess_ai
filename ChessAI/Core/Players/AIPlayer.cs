using Core.Chess;

namespace Core.Players {
    public class AIPlayer : Player {
        public AIPlayer(byte color, ChoseMove chose, RequestDraw draw, AdmitDefeat defeat)
            : base(color, chose, draw, defeat) {

        }

        override public void NotifyTurnToMove() {

        }

        override public void NotifyResult(Game.GameState result) {

        }
    }
}