using System;

using Core.Chess;
using UI;

namespace Core.Players {
    public class AIPlayer : Player {
        public AIPlayer(byte color, Player.ChoseMove chose, Player.RequestDraw draw, Player.AdmitDefeat defeat)
            : base(color, chose, draw, defeat) {

        }

        override public void Update(BoardUI boardUI) {

        }

        override public void NotifyTurnToMove() {

        }

        override public void NotifyResult(Game.GameState result) {

        }
    }
}