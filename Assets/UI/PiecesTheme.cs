using UnityEngine;
using Game.Chess;

namespace UI {
	[CreateAssetMenu(menuName = "Theme/Pieces")]
	public class PiecesTheme : ScriptableObject {

		public PieceSprites WhiteSprites;
		public PieceSprites BlackSprites;

		public Sprite GetPieceSprite(Piece piece) {
			PieceSprites sprites = piece.IsWhite ? WhiteSprites : BlackSprites;

			switch(piece.Type) {
				case Piece.Pawn:
					return sprites.Pawn;
				case Piece.Tower:
					return sprites.Tower;
				case Piece.Knight:
					return sprites.Knight;
				case Piece.Bishop:
					return sprites.Bishop;
				case Piece.Queen:
					return sprites.Queen;
				case Piece.King:
					return sprites.King;
				default:
					return null;
			}
		}

		[System.Serializable]
		public class PieceSprites {
			public Sprite Pawn, Tower, Knight, Bishop, Queen, King;
			public Sprite this[int i] {
				get {
					return new Sprite[] { Pawn, Tower, Knight, Bishop, Queen, King }[i];
				}
			}
		}
	}
}