using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Core.Chess;

namespace Others {
    public class PGNLoader {
        static public string RemoveCommentsAndLabels(string pgn) {
            for(int i = 0; i < pgn.Length; i++) {
                char c = pgn[i];
                int length = 0;
                switch(c) {
                    case '[':
                        for(int j = i + 1; j < pgn.Length; j++) {
                            length++;
                            if(pgn[j] == ']')
                                break;
                        }
                        pgn = pgn.Remove(i, length + 1);
                        i--;
                        break;
                    case '{':
                        for(int j = i + 1; j < pgn.Length; j++) {
                            length++;
                            if(pgn[j] == ']')
                                break;
                        }
                        pgn = pgn.Remove(i, length + 1);
                        i--;
                        break;
                    case ';':
                        for(int j = i + 1; j < pgn.Length; j++) {
                            length++;
                            if(pgn[j] == ']')
                                break;
                        }
                        pgn = pgn.Remove(i, length + 1);
                        i--;
                        break;
                }
            }
            return pgn;
        }
        static public (List<Move>, Game.GameState) Load(string pgn) {
            string pgnClear = RemoveCommentsAndLabels(pgn);
            string[] words = pgnClear.Replace("\n", " ").Split(' ');

            List<string> entrys = new List<string>();

            Game.GameState result = Game.GameState.Playing;
            for(int i = 0; i < words.Length; i++) {
                string entry = words[i].Trim();
                if(entry.Length == 0)
                    continue;

                if(entry.Contains("1-0")) {
                    result = Game.GameState.WhiteWin;
                    break;
                } else if(entry.Contains("0-1")) {
                    result = Game.GameState.BlackWin;
                    break;
                } else if(entry.Contains("1/2-1/2")) {
                    result = Game.GameState.Draw;
                    break;
                }


                var match = Regex.Match(entry, @"[0-9]+[.]");
                if(match.Success) 
                    entry = entry.Replace(match.Value, "");
                
                if(!string.IsNullOrEmpty(entry)) {
                    entrys.Add(entry);
                }
            }
            var moves = EntrysToMoves(entrys);
            return (moves, result);
            
        }
        
        static public List<Move> EntrysToMoves(List<string> entrys) {
            List<Move> moves = new List<Move>();
            Game game = Game.CreateInitialPosition();

            foreach(string entry in entrys) {
                Move move = EntryToMove(game, entry);
                moves.Add(move);
            }
            return moves;
        }
        static public Move EntryToMove(Game game, string entry) {
            string symbol = entry;

            symbol = RemoveCheckNotation(symbol);
            var target = 0;
            var selected = 0;

            byte moveType = SymbolToEspecialMoveType(symbol, out symbol); //Return moveType and remove spacial notations;
            if(moveType == Move.BIG_ROOK || moveType == Move.LIT_ROOK) {
                //Console.WriteLine("Traing Converting Rooks");

                selected = game.TimeOf == Piece.White ? 60 : 4;
                target = (moveType == Move.BIG_ROOK) ? 
                                (game.TimeOf == Piece.White ? 58 : 2) :
                                (game.TimeOf == Piece.White ? 62 : 6);

                return TestMoveFind(game, new Move((byte) selected, (byte) target, moveType), entry);
            }

            var (trow, tcol) = SymbolToTarget(symbol, out symbol);
            var (srow, scol) = (0, 0);

            target = trow * 8 + tcol;
            
            if(IsEnpassantBySymbol(game, trow, tcol, symbol)) {
                //Console.WriteLine("Traing Converting En passant. Symbol = {0}", symbol);

                symbol = RemoveCaptureNotation(symbol);
                scol = entry[0] - 'a';
                srow = game.TimeOf == Piece.White ? 3 : 4;
                selected = srow * 8 + scol;
                return TestMoveFind(game, new Move((byte) selected, (byte) target, Move.EN_PASSANT), entry);
            }

            symbol = RemoveCaptureNotation(symbol);
           
;
            byte pieceType = 0;
            if(symbol.Length == 0) {
                //Console.WriteLine("Traing Converting Pawn move without capture. Symbol = {0}", symbol);

                pieceType = Piece.Pawn;
                selected = SelectedByPieceTarget(game, pieceType, target);

                moveType = Math.Abs((selected / 8) - trow) == 2 ? Move.DOUBLE_STEP_PAWN : moveType;

                return TestMoveFind(game, new Move((byte) selected, (byte) target, moveType), entry);
            } else {
                pieceType = SymbolToPieceType(symbol.Substring(0, 1));

                if(symbol.Length == 1) {
                    if(pieceType == Piece.Pawn) {
                        //Console.WriteLine("Traing Converting Pawn move with capture. Symbol = {0}", symbol);

                        selected = SelectedByPawnColTarget(game, entry[0] - 'a', target);
                        return TestMoveFind(game, new Move((byte) selected, (byte) target, moveType), entry);
                    } else {
                        //Console.WriteLine("Traing Converting Pieces. Symbol = {0}", symbol);

                        selected = SelectedByPieceTarget(game, pieceType, target);
                        return TestMoveFind(game, new Move((byte) selected, (byte) target, moveType), entry);
                    }
                } else { //Length == 2
                    //Console.WriteLine("Traing Converting Pieces with ambiguos. Symbol = {0}", symbol);
                    char sCR = entry[1];
                    if(sCR >= 'a' && sCR <= 'h') {
                        scol = sCR - 'a';
                        selected = SelectedByPieceCol(game, pieceType, scol);
                    } else {
                        srow = 7 - (sCR - '1');
                        selected = SelectedByPieceRow(game, pieceType, srow);
                    }
                    return TestMoveFind(game, new Move((byte) selected, (byte) target, moveType), entry);
                }
            }
        }
       
        static private Move TestMoveFind(Game game, Move move, string entry) {
            if(!game.TryMakeMove(move)) 
                throw new Exception(string.Format("Entry \"{0}\" cannot convert in a valid move", entry));
            return move;
        }

        static private string RemoveCheckNotation(string symbol) {
            return symbol.Replace("+", "").Replace("#", "");
        }
        static private string RemoveCaptureNotation(string symbol) {
            return symbol.Replace("x", "");
        }

        static private byte SymbolToPieceType(string symbol) {
            switch(symbol) {
                case "Q":
                    return Piece.Queen;
                case "R":
                    return Piece.Tower;
                case "B":
                    return Piece.Bishop;
                case "N":
                    return Piece.Knight;
                case "K":
                    return Piece.King;
            }
            return Piece.Pawn;
        }
        static private byte SymbolToEspecialMoveType(string symbol, out string outsymbol) {
            if(symbol.Contains("O-O-O") || symbol.Contains("OOO")) {
                outsymbol = "";
                return Move.BIG_ROOK;
            }
            if(symbol.Contains("O-O") || symbol.Contains("OO")) {
                outsymbol = "";
                return Move.LIT_ROOK;
            }


            if(symbol.Contains("=Q")) {
                outsymbol = symbol.Replace("=Q", "");
                return Move.PROMOTION_QUEEN;
            }
            if(symbol.Contains("=R")) {
                outsymbol = symbol.Replace("=R", "");
                return Move.PROMOTION_TOWER;
            }
            if(symbol.Contains("=B")) {
                outsymbol = symbol.Replace("=B", "");
                return Move.PROMOTION_BISHOP;
            }
            if(symbol.Contains("=N")) {
                outsymbol = symbol.Replace("=N", "");
                return Move.PROMOTION_KNIGHT;
            }

            if(symbol.Contains("e.p")) {
                outsymbol = symbol.Replace("e.p", "").Trim();
                return Move.EN_PASSANT;
            }

            outsymbol = symbol;
            return Move.NORMAL;
        }
       
        static private bool IsEnpassantBySymbol(Game game, int trow, int tcol, string symbol) {
            //Console.WriteLine("Testing En passant: {0}", symbol);
            if(!game.EnPassant.valid || game.EnPassant.column != tcol || symbol.Length != 2)
                return false;

            int enPassantRowTarget = game.TimeOf == Piece.White ? 2 : 5;
            if(trow != enPassantRowTarget)
                return false;

            return symbol[1] == 'x' && symbol[0] >= 'a' && symbol[0] <= 'h';
        }
        
        static private (int, int) SymbolToTarget(string symbol, out string outsymbol) {
            var length = symbol.Length;
            var rowChar = symbol[length - 1];
            var colChar = symbol[length - 2];

            var row = 7 - (rowChar - '1');
            var col = colChar - 'a';

            outsymbol = symbol.Substring(0, length - 2);
            return (row, col);
        }

        static private int SelectedByPieceTarget(Game game, byte pieceType, int target, int[] movesOffSets, int[] steps) {
            List<int> possiblesSelecteds = new List<int>();

            for(int j = 0; j < movesOffSets.Length; j++) {
                int moveOffSet = movesOffSets[j];
                int step = steps[j];

                for(int i = 1; i <= step; i++) {
                    byte selected = (byte) (target + i * moveOffSet);
                    Piece pieceSelected = game[selected];
                    if(pieceSelected.IsPiece) {
                        if(pieceSelected.Type == pieceType && pieceSelected.Color == game.TimeOf)
                            possiblesSelecteds.Add(selected);
                        break;
                    }
                    
                }
            }
            if(possiblesSelecteds.Count == 0)
                return 255; //Error
            if(possiblesSelecteds.Count == 1)
                return possiblesSelecteds[0];
            else { // Some pieces selected can't move
                foreach(var seleceted in possiblesSelecteds) {
                    foreach(var move in game.GetValidPieceMoves((byte) seleceted))
                        if(move.target == target)
                            return seleceted;
                }
                return possiblesSelecteds[0];
            }
        }
        static private int SelectedByPieceTarget(Game game, byte pieceType, int target) {
            var tcol = target % 8;
            var trow = target / 8;

            if(pieceType == Piece.Pawn) {
                var step = game.TimeOf == Piece.White ? 8 : -8;
                var selected = target + step;
                if(game[selected].IsPawn && game.TimeOf == game[selected].Color)
                    return selected;
                selected += step;
                return selected;
            }

            if(pieceType == Piece.Knight) {
                List<int> possiblesSelecteds = new List<int>();

                int[] offsetRow = new int[8] { 2, 2, 1, 1, -1, -1, -2, -2 };
                int[] offsetCol = new int[8] { 1, -1, 2, -2, 2, -2, 1, -1 };

                for(int i = 0; i < 8; i++) {
                    int selectedRow = trow + offsetRow[i];
                    int selectedCol = tcol + offsetCol[i];
                    if(selectedRow >= 0 && selectedRow < 8 && selectedCol >= 0 && selectedCol < 8) {
                        int selected = selectedRow * 8 + selectedCol;
                        Piece selectedPiece = game[selected];
                        if(selectedPiece.IsKnight && selectedPiece.Color == game.TimeOf)
                            possiblesSelecteds.Add(selected);
                    }
                }

                if(possiblesSelecteds.Count == 0)
                    return 255; //Error
                if(possiblesSelecteds.Count == 1)
                    return possiblesSelecteds[0];
                else { // Some kinght selected can't move
                    foreach(var seleceted in possiblesSelecteds) {
                        if(game.GetValidPieceMoves((byte) seleceted).Count != 0)
                            return seleceted;
                    }
                    return possiblesSelecteds[0];
                }
            }


            int left = target % 8, right = 7 - left;
            int top = target / 8, bottom = 7 - top;

            if(pieceType == Piece.Tower) {
                int[] steps = new int[4] { left, right, top, bottom };
                return SelectedByPieceTarget(game, pieceType, target, MoveGenerator.TOWER_MOVES_OFFSETS, steps);
            }


            int lefttop     = Math.Min(left, top);
            int righttop    = Math.Min(right, top);
            int leftbottom  = Math.Min(left, bottom);
            int righbottom  = Math.Min(right, bottom);

            if(pieceType == Piece.Bishop) {
                int[] steps = new int[4] { lefttop,   righttop, leftbottom, righbottom };
                return SelectedByPieceTarget(game, pieceType, target, MoveGenerator.BISHOP_MOVES_OFFSETS, steps);
            }
            if(pieceType == Piece.Queen) {
                int[] steps = new int[8] { left, right, top, bottom, lefttop, righttop, leftbottom, righbottom };
            
                return SelectedByPieceTarget(game, pieceType, target, MoveGenerator.QUEEN_MOVES_OFFSETS, steps);
            }
            if(pieceType == Piece.King) {
                int[] steps = new int[8] { 
                    left   >= 1 ? 1 : 0,
                    right  >= 1 ? 1 : 0,
                    top    >= 1 ? 1 : 0,
                    bottom >= 1 ? 1 : 0,
                    lefttop    >= 1 ? 1 : 0,
                    righttop   >= 1 ? 1 : 0,
                    leftbottom >= 1 ? 1 : 0,
                    righbottom >= 1 ? 1 : 0,
                };
                return SelectedByPieceTarget(game, pieceType, target, MoveGenerator.QUEEN_MOVES_OFFSETS, steps);
            }

            return 255; //Error
        }
        static private int SelectedByPawnColTarget(Game game, int scol, int target) {
            var trow = target / 8;
            var srow = game.TimeOf == Piece.White ? trow + 1: trow - 1;

            return srow * 8 + scol;
        }
        static private int SelectedByPieceCol(Game game, byte pieceType, int col) {
            Piece piece = new Piece((byte) (game.TimeOf | pieceType));
            for(int i = (byte) col; i < 64; i += 8)
                if(piece == game[i])
                    return i;
            return 255; //Error
        }
        static private int SelectedByPieceRow(Game game, byte pieceType, int row) {
            Piece piece = new Piece((byte) (game.TimeOf | pieceType));
            for(int i = (byte) (row * 8), step = 0; step < 8; i++, step++)
                if(piece == game[i])
                    return i;
            return 255; //Error
        }
    }
}
