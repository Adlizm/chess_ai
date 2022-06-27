using System;
using System.Collections;
using System.Collections.Generic;

using Game.Chess;

namespace Others {
    public class PGNLoader {

        static public void Load(string pgn, out List<Move> moves, out Board.BoardState result) {
            string pgnClear = RemoveCommentsAndLabels(pgn);
            string[] words = pgnClear.Replace("\n", " ").Split(' ');

            Board board = null;
            List<string> entrys = new List<string>();
            bool resultExist = false;


            result = Board.BoardState.Draw;
            for(int i = 0; i < words.Length; i++) {
                string entry = words[i].Trim();

                if(entry.Contains(".") && !entry.Contains("e.p")) {
                    continue;
                }
                if(entry.Contains("1-0")) {
                    result = Board.BoardState.WhiteWin;
                    resultExist = true;
                    break;
                } else if(entry.Contains("0-1")) {
                    result = Board.BoardState.BlackWin;
                    resultExist = true;
                    break;
                } else if (entry.Contains("1/2-1/2")) {
                    result = Board.BoardState.Draw;
                    resultExist = true;
                    break;
                }

                if(!string.IsNullOrEmpty(entry)) {
                    entrys.Add(entry);
                }
            }

            moves = EntrysToMoves(entrys, out board);
            var state = board.GetBoardState();

            if(state != Board.BoardState.Playing) {
                if(resultExist) {
                    if(state != result)
                        throw new Exception("Result does not match current board state");
                } else {
                    throw new Exception("Game result not explicit");
                }
            } else {
                result = board.GetBoardState();
            }
        }

        static private List<Move> EntrysToMoves(List<string> entrys, out Board board) {
            List<Move> moves = new List<Move>();    
            board = Board.CreateInitialPosition();

            foreach(string entry in entrys) {
                Move move = EntryToMove(board, entry);
                if(board.TryMakeMove(move)) {
                    moves.Add(move);
                }
            }
            return moves;
        }

        static private string RemoveCommentsAndLabels(string pgn) {
            for(int i = 0; i < pgn.Length; i++) {
                char c = pgn[i];
                switch(c) {
                    case '[':
                        for(int j = i + 1; j < pgn.Length; j++) {
                            if(pgn[j] == ']') {
                                pgn = pgn.Remove(i, j - i);
                                break;
                            }
                        }
                        break;
                    case '{':
                        for(int j = i + 1; j < pgn.Length; j++) {
                            if(pgn[j] == '}') {
                                pgn = pgn.Remove(i, j - i);
                                break;
                            }
                        }
                        break;
                    case ';':
                        for(int j = i + 1; j < pgn.Length; j++) {
                            if(pgn[j] == '\n') {
                                pgn = pgn.Remove(i, j - i);
                                break;
                            }
                        }
                        break;
                }
            }
            return pgn;
        }

        static public Move EntryToMove(Board board, string entry) {
            List<Move> valids = board.GetAllValidMoves();

            foreach(Move valid in valids) {
                string msn = PGNCreator.MoveToStringNotation(board, valid);
                if(msn.Equals(entry))
                    return valid;
            }

            throw new Exception(string.Format("Entry \"{}\" cannot convert in a valid move", entry));
        }
    }
}
