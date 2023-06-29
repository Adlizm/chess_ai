using System;

using Core.Chess;

namespace Others {
    class PGNCreator {

        private static string[] ROWS_CHARS = new string[] { "1", "2", "3", "4", "5", "6", "7", "8" };
        private static string[] COLS_CHARS = new string[] { "a", "b", "c", "d", "e", "f", "g", "h" };

        static public string Create(Move[] moves, Game.GameState result) {
            Game game = Game.CreateInitialPosition();
            string pgn = "";

            for(int i = 0; i < moves.Length; i++) {
                if(i % 2 == 0) 
                    pgn += ((game.Rounds + 1) + ". ");

                string moveString = MoveToStringNotation(game, moves[i]);
                pgn += moveString + " ";

                game.TryMakeMove(moves[i]);
                if(game.CurrentState != Game.GameState.Playing) {
                    break;
                }
            }
            switch(result) {
                case Game.GameState.WhiteWin:
                    pgn += "1-0";
                    break;
                case Game.GameState.BlackWin:
                    pgn += "0-1";
                    break;
                case Game.GameState.Draw:
                    pgn += "1/2-1/2";
                    break;
            }
            return pgn;
        }


        static public string MoveToStringNotation(Game game, Move move) {
            string msn = "";

            Game copy = game.Copy();
            if(copy.TryMakeMove(move)) {
                if(copy.KingInCheck) {
                    if(copy.GetAllValidMoves().Count == 0)
                        msn = "#";
                    else
                        msn = "+";
                }
            } else {
                throw new Exception("Cannot convert in a PGN, found a invalid move!");
            }

            switch(move.type) {
                case Move.EN_PASSANT:
                    //msn = "e.p" + msn;
                    break;
                case Move.PROMOTION_QUEEN:
                    msn = "=Q" + msn;
                    break;
                case Move.PROMOTION_TOWER:
                    msn = "=R" + msn;
                    break;
                case Move.PROMOTION_BISHOP:
                    msn = "=B" + msn;
                    break;
                case Move.PROMOTION_KNIGHT:
                    msn = "=N" + msn;
                    break;
                case Move.BIG_ROOK:
                    return "O-O-O" + msn;
                case Move.LIT_ROOK:
                    return "O-O" + msn;
            }

            string col = COLS_CHARS[move.target % 8];
            string row = ROWS_CHARS[7 - move.target/8];
            msn = col + row + msn;

            if(game[move.target].IsPiece || move.type == Move.EN_PASSANT) {
                msn = "x" + msn;
                if(game[move.selected].IsPawn) {
                    string pawnCol = COLS_CHARS[move.selected % 8];
                    return pawnCol + msn;
                }
            }
            if(!game[move.selected].IsPawn)
                msn = OriginMoveString(game, move) + msn;
            return msn;
        }

        static private string PieceString(Game game, Move move) {
            Piece piece = game[move.selected];
            byte type = piece.Type;
            switch(type) {
                case Piece.Queen:
                    return "Q";
                case Piece.Tower:
                    return "R";
                case Piece.Bishop:
                    return "B";
                case Piece.Knight:
                    return "N";
                case Piece.King:
                    return "K";
                case Piece.Pawn:
                    return "";
            }
            return "";
        }
        static private string OriginMoveString(Game game, Move move) {
            Piece selected = game[move.selected];
            string str = PieceString(game, move);

            for(int i = 0; i < 64; i++) {
                Piece other = game[i];
                if(selected == other && i != move.selected) {
                    foreach(Move validMove in game.GetValidPieceMoves((byte) i)) {
                        if(validMove.target == move.target) {
                            if(validMove.selected % 8 != move.selected % 8) {
                                return str + COLS_CHARS[move.selected % 8];
                            } else {
                                return str + ROWS_CHARS[7 - move.selected / 8];;
                            }
                        }
                    }
                }
            }
            return str;
        }


    }
}
