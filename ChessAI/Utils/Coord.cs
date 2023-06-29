namespace Utils {
    public struct Coord {
        public int row;
        public int col;

        public Coord(int row, int col) {
            this.row = row;
            this.col = col;
        }

        public byte ToBoardIndex() {
            return (byte) (8 * row + col);
        }
        public bool IsWhiteSquare() {
            return (row + col) % 2 == 0;
        }
        public static Coord CoordToBoardIndex(byte index) {
            return new Coord { row = index / 8, col = index % 8 };
        }
    }
}
