using System;
using System.Collections;
using System.Collections.Generic;

using Core.Chess;

namespace Others {
    public class PGNLoader {

        static public void Load(string pgn, out List<Move> moves, out Game.GameState result) {
            string pgnClear = RemoveCommentsAndLabels(pgn);
            string[] words = pgnClear.Replace("\n", " ").Split(' ');

            Game game = null;
            List<string> entrys = new List<string>();

            result = Game.GameState.Playing;
            for(int i = 0; i < words.Length; i++) {
                string entry = words[i].Trim();

                if(entry.Contains(".") && !entry.Contains("e.p")) {
                    continue;
                }
                if(entry.Contains("1-0")) {
                    result = Game.GameState.WhiteWin;
                    break;
                } else if(entry.Contains("0-1")) {
                    result = Game.GameState.BlackWin;
                    break;
                } else if (entry.Contains("1/2-1/2")) {
                    result = Game.GameState.Draw;
                    break;
                }

                if(!string.IsNullOrEmpty(entry)) {
                    entrys.Add(entry);
                }
            }

            moves = EntrysToMoves(entrys, out game);
        }

        static private List<Move> EntrysToMoves(List<string> entrys, out Game game) {
            List<Move> moves = new List<Move>();    
            game = Game.CreateInitialPosition();

            foreach(string entry in entrys) {
                Move move = EntryToMove(game, entry);
                if(game.TryMakeMove(move)) {
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

        static public Move EntryToMove(Game game, string entry) {
            List<Move> valids = game.GetAllValidMoves();

            foreach(Move valid in valids) {
                string msn = PGNCreator.MoveToStringNotation(game, valid);
                if(msn.Equals(entry))
                    return valid;
            }

            throw new Exception(string.Format("Entry \"{}\" cannot convert in a valid move", entry));
        }
    }
}
