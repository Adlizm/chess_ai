using UnityEngine;

using System;
using System.Collections.Generic;

namespace Core.Chess {
    public static class MoveGenerator {

        static public GameConfig MakeMove(Board board, GameConfig config, Move move) {
            Piece piece = board[move.selected];
            byte colorPiece = piece.Color;

            int rookIndex = piece.Color == Piece.White ? 0 : 2;
            int row = move.selected / 8;
            int col = move.selected % 8;

            config.LastCaptureOrPawnMove++;

            Piece targetPiece = board[move.target];
            if(piece.IsPawn || targetPiece.IsPiece)
                config.LastCaptureOrPawnMove = 0;
            if(targetPiece.IsTower) {
                if(move.target / 8 == 7) { //White Rook
                    if(move.target % 8 == 7)
                        config.SetRook(0, false); //Lit
                    else if(move.target % 8 == 0)
                        config.SetRook(1, false); //Big

                }else if(move.target / 8 == 0){ //Black Rook
                    if(move.target % 8 == 7)
                        config.SetRook(2, false); //Lit
                    else if(move.target % 8 == 0)
                        config.SetRook(3, false); //Big
                }
            }

            if(config.TimeOf == Piece.Black)
                config.Rounds++;

            config.TimeOf = config.TimeOf == Piece.White ? Piece.Black : Piece.White;
            board[move.target] = piece;
            board[move.selected] = new Piece(Piece.None);
            config.EnPassant.valid = false;

            switch(move.type) {
                case Move.NORMAL:
                    if(piece.IsKing) {
                        config.SetRook(rookIndex, false);
                        config.SetRook(rookIndex + 1, false);
                    }else if(piece.IsTower && col == 0) {
                        config.SetRook(rookIndex + 1, false);
                    }else if(piece.IsTower && col == 7) {
                        config.SetRook(rookIndex, false);
                    }
                    break;
                case Move.DOUBLE_STEP_PAWN:
                    config.EnPassant.valid = true;
                    config.EnPassant.column = (byte) (move.selected % 8);
                    break;
                case Move.EN_PASSANT:
                    int rowPassan = piece.IsWhite ? 3 : 4;
                    board[rowPassan * 8 + config.EnPassant.column] = new Piece(Piece.None);
                    break;
                case Move.BIG_ROOK:
                    config.SetRook(rookIndex, false);
                    config.SetRook(rookIndex + 1, false);
                    board[row * 8] = new Piece(Piece.None);
                    board[row * 8 + 3] = new Piece((byte) (Piece.Tower | colorPiece));
                    break;
                case Move.LIT_ROOK:
                    config.SetRook(rookIndex, false);
                    config.SetRook(rookIndex + 1, false);
                    board[row * 8 + 7] = new Piece(Piece.None);
                    board[row * 8 + 5] = new Piece((byte) (Piece.Tower | colorPiece));
                    break;
                case Move.PROMOTION_BISHOP:
                    board[move.target] = new Piece((byte) (Piece.Bishop | colorPiece));
                    break;
                case Move.PROMOTION_KNIGHT:
                    board[move.target] = new Piece((byte) (Piece.Knight | colorPiece));
                    break;
                case Move.PROMOTION_TOWER:
                    board[move.target] = new Piece((byte) (Piece.Tower | colorPiece));
                    break;
                case Move.PROMOTION_QUEEN:
                    board[move.target] = new Piece((byte) (Piece.Queen | colorPiece));
                    break;
            }

            return config;
        }

        static public List<Move> FilterInvalidMoves(Board board, GameConfig config, List<Move> moves) {
            byte enemyColor = config.TimeOf == Piece.White ? Piece.Black : Piece.White;
            byte allyColor = config.TimeOf;

            for(int i = moves.Count - 1; i >= 0 ; i--) {
                Move move = moves[i];

                if(!board[move.selected].isSameColor(config.TimeOf)){
                    moves.RemoveAt(i);
                    continue;
                }

                Board copy = board.Copy();
                GameConfig copyConfig = config;
                switch(move.type) {
                    case Move.BIG_ROOK:
                        copyConfig.TimeOf = enemyColor;
                        if(ExistsAttackInLocal(copy, copyConfig, move.selected, enemyColor) ||
                            ExistsAttackInLocal(copy, copyConfig, move.selected - 1, enemyColor) ||
                            ExistsAttackInLocal(copy, copyConfig, move.selected - 2, enemyColor)) {
                            moves.RemoveAt(i);
                        }
                        break;
                    case Move.LIT_ROOK:
                        copyConfig.TimeOf = enemyColor;
                        if(ExistsAttackInLocal(copy, copyConfig, move.selected, enemyColor) ||
                            ExistsAttackInLocal(copy, copyConfig, move.selected + 1, enemyColor) ||
                            ExistsAttackInLocal(copy, copyConfig, move.selected + 2, enemyColor)) {
                            moves.RemoveAt(i);
                        }
                        break;
                    default:
                        copyConfig = MakeMove(copy, copyConfig, moves[i]);
                        byte localKing = GetKingLocal(copy, allyColor);
                        if(ExistsAttackInLocal(copy, copyConfig, localKing, enemyColor)) {
                            moves.RemoveAt(i);
                        }
                        break;
                }
            }
            
            return moves;
        }

        static public List<Move> GetPieceMoves(Board board, GameConfig config, byte localPiece) {
            Piece piece = board[localPiece];
            byte type = piece.Type;

            List<Move> moves = new List<Move>();
            if(!piece.IsPiece)
                return moves;

            switch(type) {
                case Piece.Pawn:
                    return GetPawnMoves(board, config, localPiece);
                case Piece.Bishop:
                    return GetBishopMoves(board, config, localPiece);
                case Piece.Knight:
                    return GetKnightMoves(board, config, localPiece);
                case Piece.Tower:
                    return GetTowerMoves(board, config, localPiece);
                case Piece.Queen:
                    return GetQueenMoves(board, config, localPiece);
                case Piece.King:
                    return GetKingMoves(board, config, localPiece);
            }
            return moves;
        }

        static public List<Move> GetPawnMoves(Board board, GameConfig config, byte localPiece) {
            List<Move> moves = new List<Move>();
            Piece piece = board[localPiece];

            if(!piece.IsPawn)
                return moves;

            byte offsetTopBottom = (byte) (piece.IsWhite ? -1 : 1);
            byte col = (byte) (localPiece % 8);
            byte row = (byte) (localPiece / 8);
            int enPassantRow = (byte) (piece.IsWhite ? 3 : 4);

            byte targetLocal = (byte) (localPiece + offsetTopBottom * 8);
            byte rowTarget = (byte) (targetLocal / 8);
            bool doubleStep = row == (piece.IsWhite ? 6 : 1);
            bool promotion = rowTarget == 0 || rowTarget == 7;


            Piece pieceForward = board[targetLocal];
            if(!pieceForward.IsPiece) {
                if(promotion) {
                    moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_BISHOP));
                    moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_KNIGHT));
                    moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_TOWER));
                    moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_QUEEN));
                }else {
                    moves.Add(new Move(localPiece, targetLocal, Move.NORMAL));
                }

                if(doubleStep) {
                    byte targetDouble = (byte) (localPiece + offsetTopBottom * 8 * 2);
                    pieceForward = board[targetDouble];
                    if(!pieceForward.IsPiece) {
                        if(piece.IsWhite && row == 6) {

                            moves.Add(new Move(localPiece, targetDouble, Move.DOUBLE_STEP_PAWN));
                        } else if(row == 1) {
                            moves.Add(new Move(localPiece, targetDouble, Move.DOUBLE_STEP_PAWN));
                        }
                    }
                }
            }

            targetLocal = (byte) (localPiece + offsetTopBottom * 8 - 1);
            if(col > 0) {
                pieceForward = board[targetLocal];
                if(piece.isEnemy(pieceForward)) {
                    if(promotion) {
                        moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_BISHOP));
                        moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_KNIGHT));
                        moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_TOWER));
                        moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_QUEEN));
                    }else {
                        moves.Add(new Move(localPiece, targetLocal, Move.NORMAL));
                    }
                }

                if(config.EnPassant.valid && config.EnPassant.column == col - 1 && row == enPassantRow)
                    moves.Add(new Move(localPiece, targetLocal, Move.EN_PASSANT)); 
            }

            targetLocal = (byte) (localPiece + offsetTopBottom * 8 + 1);
            if(col < 7) {
                pieceForward = board[targetLocal];
                if(piece.isEnemy(pieceForward)) {
                    if(promotion) {
                        moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_BISHOP));
                        moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_KNIGHT));
                        moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_TOWER));
                        moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_QUEEN));
                    }else {
                        moves.Add(new Move(localPiece, targetLocal, Move.NORMAL));
                    }
                }

                if(config.EnPassant.valid && config.EnPassant.column == col + 1 && row == enPassantRow)
                    moves.Add(new Move(localPiece, targetLocal, Move.EN_PASSANT));
            }

            return moves;
        }
        static public List<Move> GetBishopMoves(Board board, GameConfig config, byte localPiece) {
            List<Move> moves = new List<Move>();
            Piece piece = board[localPiece];

            if(!piece.IsBishop)
                return moves;

            int left = localPiece % 8, right = 7 - left;
            int top = localPiece / 8, bottom = 7 - top;

            int[] movesOffSets = new int[4] { -9, -7, 7, 9 };
            int[] steps = new int[4] { Math.Min(left, top),   Math.Min(right, top),
                                        Math.Min(left, bottom),Math.Min(right, bottom) };

            for(int j = 0; j < 4; j++) {
                int moveOffSet = movesOffSets[j];
                int step = steps[j];

                for(int i = 1; i <= step; i++) {
                    byte target = (byte) (localPiece + i * moveOffSet);
                    Piece pieceTarget = board[target];
                    if(pieceTarget.IsPiece) {
                        if(piece.isEnemy(pieceTarget)) {
                            moves.Add(new Move(localPiece, target, Move.NORMAL));
                        }
                        break;
                    }
                    moves.Add(new Move(localPiece, target, Move.NORMAL));
                }
            }

            return moves;
        }
        static public List<Move> GetKnightMoves(Board board, GameConfig config, byte localPiece) {
            List<Move> moves = new List<Move>();
            Piece piece = board[localPiece];

            if(!piece.IsKnight)
                return moves;

            int row = localPiece / 8, col = localPiece % 8;

            int[] offsetRow = new int[8] { 2, 2, 1, 1, -1, -1, -2, -2 };
            int[] offsetCol = new int[8] { 1, -1, 2, -2, 2, -2, 1, -1 };

            for(int i = 0; i < 8; i++) {
                int targetRow = row + offsetRow[i];
                int targetCol = col + offsetCol[i];
                if(targetRow >= 0 && targetRow < 8 && targetCol >= 0 && targetCol < 8) {
                    byte targetLocal = (byte) (targetRow * 8 + targetCol);
                    Piece targetPiece = board[targetLocal];
                    if(targetPiece.IsPiece && piece.isAlly(targetPiece))
                        continue;
                    moves.Add(new Move(localPiece, targetLocal, Move.NORMAL));
                }
            }
            return moves;
        }
        static public List<Move> GetTowerMoves(Board board, GameConfig config, byte localPiece) {
            List<Move> moves = new List<Move>();
            Piece piece = board[localPiece];

            if(!piece.IsQueen)
                return moves;

            int left = localPiece % 8, right = 7 - left;
            int top = localPiece / 8, bottom = 7 - top;

            int[] movesOffSets = new int[4] { -1, 1, -8, 8 };
            int[] steps = new int[4] { left, right, top, bottom };

            for(int j = 0; j < 4; j++) {
                int moveOffSet = movesOffSets[j];
                int step = steps[j];

                for(int i = 1; i <= step; i++) {
                    byte target = (byte) (localPiece + i * moveOffSet);
                    Piece pieceTarget = board[target];
                    if(pieceTarget.IsPiece) {
                        if(piece.isEnemy(pieceTarget)) {
                            moves.Add(new Move(localPiece, target, Move.NORMAL));
                        }
                        break;
                    }
                    moves.Add(new Move(localPiece, target, Move.NORMAL));
                }
            }

            return moves;
        }
        static public List<Move> GetQueenMoves(Board board, GameConfig config, byte localPiece) {
            List<Move> moves = new List<Move>();
            Piece piece = board[localPiece];

            if(!piece.IsQueen)
                return moves;

            int left = localPiece % 8, right = 7 - left;
            int top = localPiece / 8, bottom = 7 - top;

            int[] movesOffSets = new int[8] { -1, 1, -8,  8, -9, -7, 7, 9 };
            int[] steps = new int[8] { left, right, top, bottom, 
                Math.Min(left, top), Math.Min(right, top), Math.Min(left, bottom),Math.Min(right, bottom) };

            for(int j = 0; j < 8; j++) {
                int moveOffSet = movesOffSets[j];
                int step = steps[j];

                for(int i = 1; i <= step; i++) {
                    byte target = (byte) (localPiece + i * moveOffSet);
                    Piece pieceTarget = board[target];
                    if(pieceTarget.IsPiece) {
                        if(piece.isEnemy(pieceTarget)) {
                            moves.Add(new Move(localPiece, target, Move.NORMAL));
                        }
                        break;
                    }
                    moves.Add(new Move(localPiece, target, Move.NORMAL));
                }
            }

            return moves;
        }
        static public List<Move> GetKingMoves(Board board, GameConfig config, byte localPiece) {
            List<Move> moves = new List<Move>();
            Piece piece = board[localPiece];

            if(!piece.IsKing)
                return moves;

            byte enemyColor = config.TimeOf == Piece.White ? Piece.Black : Piece.White;
            int rookIndex = piece.Color == Piece.White ? 0 : 2;
            int rowRook = piece.Color == Piece.White ? 7 : 0;
            int row = localPiece / 8; 
            int col = localPiece % 8;

            for(int i = Math.Max(0, row - 1); i <= Math.Min(7, row + 1); i++) {
                for(int j = Math.Max(0, col - 1); j <= Math.Min(7, col + 1); j++) {
                    if(i == row && j == col)
                        continue;

                    byte targetLocal =  (byte) (i * 8 + j);
                    Piece pieceTarget = board[targetLocal];
                    if(!piece.isAlly(pieceTarget)) {
                        moves.Add(new Move(localPiece, targetLocal, Move.NORMAL));
                    }
                }
            }

            if(col == 4 && row == rowRook) {
                if(config.GetRook(rookIndex + 1)) { //Big Rook
                    if( board[localPiece - 1].IsNone && board[localPiece - 2].IsNone && board[localPiece - 3].IsNone && 
                        board[localPiece - 4].IsTower && piece.isAlly(board[localPiece - 4]) ) 
                            moves.Add(new Move(localPiece, (byte) (localPiece - 2), Move.BIG_ROOK));
                }
                if(config.GetRook(rookIndex)) { //Lit Rook
                    if( board[localPiece + 1].IsNone && board[localPiece + 2].IsNone && board[localPiece + 3].IsTower && 
                        piece.isAlly(board[localPiece + 3]) ) 
                            moves.Add(new Move(localPiece, (byte) (localPiece + 2), Move.LIT_ROOK));
                }
            }
            return moves;
        }

        static public byte GetKingLocal(Board board, byte kingColor) {
            for(byte i = 0; i < 64; i++) {
                Piece piece = board[i];
                if(piece.IsKing && piece.isSameColor(kingColor))
                    return i;
            }
            string colorString = kingColor == Piece.White ? "White" : "Black";
            throw new Exception("This board hasn't a king with " + colorString + " color");
        }
        static public bool ExistsAttackInLocal(Board board, GameConfig config, int local, byte colorAttack) {
            for(byte i = 0; i < 64; i++) {
                Piece piece = board[i];
                if(piece.IsPiece && piece.isSameColor(colorAttack)){
                    List<Move> moves = GetPieceMoves(board, config, i);
                    foreach(Move move in moves) {
                        if(move.target == local) 
                            return true;
                        
                    }
                }
            }
            return false;
        }
    }
}