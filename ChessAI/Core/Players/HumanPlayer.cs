using Core.Chess;

namespace Core.Players {
    public class HumanPlayer : Player {
        public HumanPlayer(byte color, ChoseMove chose, RequestDraw draw, AdmitDefeat defeat)
            : base(color, chose, draw, defeat) {
        }

        override public void NotifyTurnToMove() {
            
        }

        override public void NotifyResult(Game.GameState result) {
            string resultString = "";
            resultString = result == Game.GameState.Draw ? "Draw" : resultString;
            resultString = result == Game.GameState.WhiteWin ? "White Win!" : resultString;
            resultString = result == Game.GameState.BlackWin ? "Black Win!" : resultString;
            System.Console.WriteLine(resultString);
        }
    }
}