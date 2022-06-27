using System.IO;

namespace Others {
    class PGNExport {

        public void SavePGNs(string filepath, string[] pgns) {
            using(StreamWriter file = new StreamWriter(filepath)) {
                foreach(string pgn in pgns) {
                    file.Write(pgn + '\n');
                }
            }
        }
    }
}
