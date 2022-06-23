using System;

using Game.Chess;

namespace Game.Players {
    public class AIPlayer : Player {
        public AIPlayer(byte color, Player.ChoseMove chose, Player.RequestDraw draw, Player.AdmitDefeat defeat)
            : base(color, chose, draw, defeat) {

        }

        override public void Update() {

        }

        override public void NotifyTurnToMove() {

        }

        override public void NotifyResult(Board.BoardState result) {

        }
    }
}