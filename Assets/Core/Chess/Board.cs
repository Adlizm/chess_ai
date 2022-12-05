using UnityEngine;

using System;
using System.Collections.Generic;

namespace Core.Chess {
    public class Board {
        private Piece[] data;

        public Board() {
            this.data = new Piece[64];
        }

        public Board Copy() {
            Board copy = new Board();
			for(int i = 0; i < 64; i++) 
                copy.data[i] = this.data[i];
            return copy;
        }
        public Piece this[int index] {
            get { return this.data[index]; }
            set { this.data[index] = value; }
        }
        
        public static bool operator ==(Board board, Board other) {
			for(int i = 0; i < 64; i++)
                if(board[i] != other[i])
                    return false;
            return true;
		}
		public static bool operator !=(Board board, Board other) {
			return !(board == other);
		}

		public override bool Equals(object obj) {
			return base.Equals(obj);
		}
		public override int GetHashCode() {
			return base.GetHashCode();
		}
    }

}
