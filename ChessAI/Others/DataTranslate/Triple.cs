using System;

using Core.Chess;
using Core.AI.NeuralNet;

namespace Others.DataTranslate {
	public class Triple {
		public byte[] S;
		public float R;
		public (ushort, uint)[] MoveN;

		public Triple(Game game, uint[] N) : this(game, OutputFormat.ExpectedValue(game), N) { }
		public Triple(Game game, float R, uint[] N) {
			this.S = InputFormat.GameToInputBytes(game);
			this.R = R;

			var index = 0;
			this.MoveN = new (ushort, uint)[N.Length];
			foreach(var move in game.GetAllValidMoves()) {
				this.MoveN[index] = (move.ToShort(), N[index]);
				index++;
			}
		}
		public Triple(Game game, float R, (ushort, uint)[] MoveN) {
			this.S = InputFormat.GameToInputBytes(game);
			this.R = R;
			this.MoveN = MoveN;
		}
		
		public Triple(byte[] S, float R, (ushort, uint)[] MoveN) {
			this.S = S;
			this.R = R;
			this.MoveN = MoveN;
		}


		public int Length {
			get => ((InputFormat.TOTAL_8X8_BLOCKS * 8 * 8) + MoveN.Length * (sizeof(ushort) + sizeof(uint)) + sizeof(float));
		}

		public byte[] ToBytes() {
			byte[] buffer = new byte[Length];

			int index = 0;
			for(int i = 0; i < S.Length; i++)
				buffer[index++] = S[i];

			BitConverter.TryWriteBytes(buffer.AsSpan(index), R);
			index += 4;

			for(int i = 0; i < MoveN.Length; i++) {
				BitConverter.TryWriteBytes(buffer.AsSpan(index), MoveN[i].Item1);
				index += 2;

				BitConverter.TryWriteBytes(buffer.AsSpan(index), MoveN[i].Item2);
				index += 4;
			}
			return buffer;
		}
		public static Triple FromBytes(byte[] buffer) {
			int index = 0;

			byte[] S = new byte[(InputFormat.TOTAL_8X8_BLOCKS * 8 * 8)];
			for(int i = 0; i < S.Length; i++)
				S[i] = buffer[index++];

			float R = BitConverter.ToSingle(buffer, index);
			index += sizeof(float);


			int moveLength = sizeof(ushort) + sizeof(uint);
			int moves = (buffer.Length - index) / moveLength;

			(ushort, uint)[] MoveN = new (ushort, uint)[moves];
			for(int i = 0; i < moves; i++) {
				ushort move = BitConverter.ToUInt16(buffer, index);
				uint n = BitConverter.ToUInt32(buffer, index + sizeof(ushort));
				MoveN[i] = (move, n);

				index += moveLength;
			}

			return new Triple(S, R, MoveN);
		}


		public override bool Equals(object obj) {
			if(obj == null || !typeof(Triple).IsInstanceOfType(obj))
				return false;

			Triple other = (Triple) obj;
			if(R != other.R)
				return false;
			if(S.Length != other.S.Length)
				return false;
			if(MoveN.Length != other.MoveN.Length)
				return false;

			for(int i = 0; i < S.Length; i++)
				if(S[i] != other.S[i])
					return false;

			for(int i = 0; i < MoveN.Length; i++)
				if(MoveN[i] != other.MoveN[i])
					return false;
			return true;
		}
		public override int GetHashCode() {
			return base.GetHashCode();
		}
	}
}

