using System.Collections.Generic;
using System.IO;

namespace Others {
    class PGNImport {

        public IEnumerator<string> PGNsFromFile(string filepath) {
            string pgn = "";
            foreach(string line in System.IO.File.ReadLines(filepath)) {
                pgn += line.Replace('\n', ' ').Trim();

                if(line.Contains("1-0") || line.Contains("0-1") || line.Contains("1/2-1/2")) {
                    if(!string.IsNullOrEmpty(pgn)) {
                        yield return pgn;
                    }
                    pgn = "";
                }
            }
        }
    }
}
