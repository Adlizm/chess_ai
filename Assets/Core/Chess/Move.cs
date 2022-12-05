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
	}

	public struct EnPassant {
		public bool valid;
		public byte column;
	}

}
