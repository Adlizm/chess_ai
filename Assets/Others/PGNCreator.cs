using System;

using Core.Chess;

namespace Others {
    class PGNCreator {

        static public string Create(Move[] moves, Game.GameState result) {
            Game game = Game.CreateInitialPosition();
            string pgn = "";

            for(int i = 0; i < moves.Length; i++) {
                if(i % 2 == 0) 
                    pgn += (game.Rounds + ". ");

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
                    msn = "e.p" + msn;
                    break;
                case Move.PROMOTION_QUEEN:
                    msn = "=D" + msn;
                    break;
                case Move.PROMOTION_TOWER:
                    msn = "=T" + msn;
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

            char col = (char) ('a' + (move.target % 8));
            char row = (char) ('1' + (7 - move.target/8));
            msn = col + row + msn;

            if(game[move.target].IsPiece || move.type == Move.EN_PASSANT) {
                msn = "x" + msn;
                if(game[move.selected].IsPawn) {
                    char pawnCol = (char) ('a' + (move.selected % 8));
                    return pawnCol + msn;
                }
            }

            msn = OriginMoveString(game, move) + msn;
            return msn;
        }

        static private string PieceString(Game game, Move move) {
            Piece piece = game[move.selected];
            byte type = piece.Type;
            switch(type) {
                case Piece.Queen:
                    return "D";
                case Piece.Tower:
                    return "T";
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
                if(selected == other) {
                    foreach(Move validMove in game.GetValidPieceMoves((byte) i)) {
                        if(validMove.target == move.target) {
                            if(validMove.selected % 8 != move.selected % 8) {
                                char col = (char) ('a' + (move.selected % 8));
                                return str + col;
                            } else {
                                char row = (char) ('1' + (7 - (move.selected / 8)));
                                return str + row;
                            }
                        }
                    }
                }
            }
            return str;
        }


    }
}
