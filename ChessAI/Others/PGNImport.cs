using System.Collections.Generic;
using System.IO;

namespace Others {
    class PGNImport {

        public static IEnumerable<string> PGNsFromFile(string filepath) {
            bool isReadingPGN = false;

            string pgn = "";
            foreach(string line in File.ReadLines(filepath)) {
                if(line.Contains("[")) {
                    if(isReadingPGN) {
                        isReadingPGN = false;
                        pgn = pgn.Trim();
                        if(!string.IsNullOrEmpty(pgn)) {
                            yield return pgn;
                        }
                        pgn = "";
                    }
                } else {
                    isReadingPGN = true;
                }
                pgn += line + " ";
            }
            if(!string.IsNullOrEmpty(pgn)) {
                yield return pgn;
            }
        }
    }
}
