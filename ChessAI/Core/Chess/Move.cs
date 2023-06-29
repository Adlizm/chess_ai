namespace Core.Chess {
	public struct Move{
		public const byte NORMAL = 0;
		public const byte DOUBLE_STEP_PAWN = 1;
		public const byte EN_PASSANT = 2;
		public const byte BIG_ROOK = 3;
		public const byte LIT_ROOK = 4;
		public const byte PROMOTION_BISHOP = 5;
		public const byte PROMOTION_KNIGHT = 6;
		public const byte PROMOTION_TOWER = 7;
		public const byte PROMOTION_QUEEN = 8;

		public byte selected, target;
		public byte type;

		public Move(byte selected, byte target, byte type) {
			this.selected = selected;
			this.target = target;
			this.type = type;
		}

		public ushort ToShort() {
			return (ushort) (type | ((target & 0b0011_1111) << 4) | ((selected & 0b0011_1111) << 10));
		}
		public static Move FromShort(ushort value) {
			byte type = (byte) (value & 0b0000_0000_0000_1111);
			byte target = (byte) ((value & 0b0000_0011_1111_0000) >> 4);
			byte selected = (byte) (value >> 10);

			return new Move(selected, target, type);
		}

		public static bool operator ==(Move move, Move other) {
			return move.selected == other.selected &&
				   move.target == other.target &&
				   move.type == other.type;
		}

		public static bool operator !=(Move move, Move other) {
			return !(move == other);
		}

		public override bool Equals(object obj) {
			return base.Equals(obj);
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public override string ToString() {
			var srow = (char) ((7 - selected / 8) + '1');
			var scol = (char) ((selected % 8) + 'a');
			var trow = (char) ((7 - target / 8) + '1');
			var tcol = (char) ((target % 8) + 'a');


			return string.Format("({0}, {1}, {2})",
				scol.ToString() + srow.ToString(),
				tcol.ToString() + trow.ToString(),
				type);
		}
	}

	public struct EnPassant {
		public bool valid;
		public byte column;
	}

}
