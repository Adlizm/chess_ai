namespace Core.Chess {
    public struct Piece {
        public const byte None = 0;
        public const byte Pawn = 1;
        public const byte Bishop = 2;
        public const byte Knight = 3;
        public const byte Tower = 4;
        public const byte Queen = 5;
        public const byte King = 6;

        public const byte White = 8;
        public const byte Black = 0;

        private byte value;
        public Piece(byte value) {
            this.value = value;
        }

        public byte Byte {
            get => value;
        }

        public bool IsPiece {
            get => IsBlack || IsWhite;
        }
        public bool IsBlack {
            get => value >= 1 && value <= 6;
        }
        public bool IsWhite {
            get => value >= 9 && value <= 14;
        }
        public byte Type {
            get => (byte) (value & 7);
        }
        public byte Color {
            get => (byte) (value & 8);
        }

        public bool isEnemy(Piece other) {
            return this.IsWhite && other.IsBlack || this.IsBlack && other.IsWhite;
        }
        public bool isAlly(Piece other) {
            return this.IsWhite && other.IsWhite || this.IsBlack && other.IsBlack;
        }
        public bool isSameColor(byte color) {
            return this.Color == color;
        }

        public bool IsNone {
            get => value == None;
        }
        public bool IsPawn {
            get => value == Pawn || value == (Pawn | White);
        }
        public bool IsBishop {
            get => value == Bishop || value == (Bishop | White);
        }
        public bool IsKnight {
            get => value == Knight || value == (Knight | White);
        }
        public bool IsTower {
            get => value == Tower || value == (Tower | White);
        }
        public bool IsQueen {
            get => value == Queen || value == (Queen | White);
        }
        public bool IsKing {
            get => value == King || value == (King | White);
        }

        public static bool operator ==(Piece piece, Piece other) {
            return piece.value == other.value;
        }
        public static bool operator !=(Piece piece, Piece other) {
            return piece.value != other.value;
        }

        public override bool Equals(object obj) {
            return base.Equals(obj);
        }
        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
