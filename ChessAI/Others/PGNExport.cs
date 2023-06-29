using System.IO;

namespace Others {
    class PGNExport {

        public static void SavePGNs(string filepath, string[] pgns) {
            var stream = new FileStream(filepath, FileMode.Append);
            using(StreamWriter file = new StreamWriter(stream)) {
                foreach(string pgn in pgns) {
                    file.Write(pgn + '\n');
                }
                file.Close();
            }

            stream.Close();
        }

        public static void SavePGN(string filepath, string pgn) {
            var stream = new FileStream(filepath, FileMode.Append);

            using(StreamWriter file = new StreamWriter(stream)) {
                file.Write(pgn + '\n');
                file.Close();
            }
            stream.Close();
        }
    }
}
