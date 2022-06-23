using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Game.Chess;
using Game.Utils;

namespace UI {
	public class BoardUI : MonoBehaviour {

		public BoardTheme boardTheme;
		public PiecesTheme piecesTheme;

		public bool ShowLegalMoves = true;
		public bool WhitePointOfView = true;

		private Board board;
		private MeshRenderer[,] squareRenderers;
		private SpriteRenderer[,] squarePieceRenderers;

		const float PieceDepth = -0.1f;
		const float PieceDragDepth = -0.2f;

		private MeshRenderer[] squaresPromotions;
		private SpriteRenderer[] spritesPromotions;

		void Awake() {
			this.board = Board.CreateInitialPosition();
			CreatePromotionUIBox();
			CreateBoardUI();

			ClosePromotions();
		}

		public void ShowPromotions() {
			for(int i = 0; i < 4; i++)
				squaresPromotions[i].transform.localScale = new Vector3(0.3106097f, 0.3382822f, 0.0f);	

			byte color = this.board.TimeOf;
			spritesPromotions[3].sprite = piecesTheme.GetPieceSprite(new Piece((byte) (Piece.Queen | color)));
			spritesPromotions[2].sprite = piecesTheme.GetPieceSprite(new Piece((byte) (Piece.Tower | color)));
			spritesPromotions[1].sprite = piecesTheme.GetPieceSprite(new Piece((byte) (Piece.Bishop | color)));
			spritesPromotions[0].sprite = piecesTheme.GetPieceSprite(new Piece((byte) (Piece.Knight | color)));
		}

		public void ClosePromotions() {
			for(int i = 0; i < 4; i++)
				squaresPromotions[i].transform.localScale = Vector3.zero;
		}

		public bool TryPromotion(Vector2 mouseWorld, out byte promotionType) {
			promotionType = (byte) 0;

			if(mouseWorld.y >= -2.0f && mouseWorld.y < 2.0f) {
				if(mouseWorld.x >= -5.5f && mouseWorld.x < -4.5f) {
					int type = (int) Mathf.Floor(mouseWorld.y + 2);
					switch(type) {
						case 3: 
							promotionType = Move.PROMOTION_QUEEN; 
							break;
						case 2:
							promotionType = Move.PROMOTION_TOWER;
							break;
						case 1:
							promotionType = Move.PROMOTION_BISHOP;
							break;
						case 0:
							promotionType = Move.PROMOTION_KNIGHT;
							break;
					}
					return true;
				}
			}
			return false;
		}

		public void HighlightLegalMoves(Coord fromSquare) {
			if(this.ShowLegalMoves) {
				byte squareIndex = fromSquare.toBoardIndex();
				List<Move> moves = this.board.GetValidPieceMoves(squareIndex);

				foreach(Move move in moves) {
					Coord coord = Coord.CoordToBoardIndex(move.target);
					SetSquareColour(coord, boardTheme.LegalMoves);
				}
			}
		}

		public void DragPiece(Coord pieceCoord, Vector2 mousePos) {
			Vector3 pos = new Vector3(mousePos.x, mousePos.y, PieceDragDepth);
			squarePieceRenderers[pieceCoord.row, pieceCoord.col].transform.position = pos;
		}

		public void ResetPiecePosition(Coord pieceCoord) {
			Vector3 pos = PositionFromCoord(pieceCoord.row, pieceCoord.col, PieceDepth);
			squarePieceRenderers[pieceCoord.row, pieceCoord.col].transform.position = pos;
		}

		public void SelectSquare(Coord coord) {
			SetSquareColour(coord, boardTheme.Selected);
		}

		public void DeselectSquare(Coord coord) {
			bool white = coord.isWhiteSquare();
			white  = WhitePointOfView ? !white : white;
			Color squareColor = white ? boardTheme.WhiteSquares : boardTheme.BlackSquares;
			SetSquareColour(coord, squareColor);
		}

		public bool TryGetSquareUnderMouse(Vector2 mouseWorld, out Coord selectedCoord) {
			int row = (int) (4 - mouseWorld.y);
			int col = (int) (mouseWorld.x + 4);
			if(!WhitePointOfView) {
				row = 7 - row;
				col = 7 - col;
			}
			selectedCoord = new Coord(row, col);
			return row >= 0 && row < 8 && col >= 0 && col < 8;
		}

		public void SetNewPosition(Board board) {
			this.board = board;
			UpdatePosition();
		}

		public void UpdatePosition() {
			for(int row = 0; row < 8; row++) {
				for(int col = 0; col < 8; col++) {
					Coord coord = new Coord(row, col);
					Piece piece = this.board[coord.toBoardIndex()];
					squarePieceRenderers[row, col].sprite = piecesTheme.GetPieceSprite(piece);
					squarePieceRenderers[row, col].transform.position = PositionFromCoord(row, col, PieceDepth);
				}
			}

		}

		public void MoveMade() {
			UpdatePosition();
			ResetSquareColours();
		}

		void CreatePromotionUIBox() {
			Shader squareShader = Shader.Find("Unlit/Color");

			squaresPromotions = new MeshRenderer[4];
			spritesPromotions = new SpriteRenderer[4];
			for(int i = 0; i < 4; i++) {
				Transform square = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
				square.parent = this.transform;
				square.name = "Promotion Piece";
				square.position = new Vector3(-5.0f, -1.5f + i);

				squaresPromotions[i] = square.GetComponent<MeshRenderer>();
				squaresPromotions[i].material = new Material(squareShader);

				spritesPromotions[i] = new GameObject("Promotion Type").AddComponent<SpriteRenderer>();
				spritesPromotions[i].transform.parent = square;
				spritesPromotions[i].transform.position = new Vector3(-5.0f, -1.5f + i, PieceDepth);
				spritesPromotions[i].transform.localScale = Vector3.one * (100 / 180f);
			}
		}

		void CreateBoardUI() {
			Shader squareShader = Shader.Find("Unlit/Color");
			squareRenderers = new MeshRenderer[8, 8];
			squarePieceRenderers = new SpriteRenderer[8, 8];

			for(int row = 0; row < 8; row++) {
				for(int col = 0; col < 8; col++) {
					Transform square = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;

					square.parent = this.transform;
					square.name = row + " " + col;
					square.position = PositionFromCoord(row, col);

					squareRenderers[row, col] = square.GetComponent<MeshRenderer>();
					squareRenderers[row, col].material = new Material(squareShader);

					SpriteRenderer pieceRenderer = new GameObject("Piece").AddComponent<SpriteRenderer>();
					pieceRenderer.transform.parent = square;
					pieceRenderer.transform.position = PositionFromCoord(row, col, PieceDepth);
					pieceRenderer.transform.localScale = Vector3.one * (100 / 180f);
					squarePieceRenderers[row, col] = pieceRenderer;
				}
			}

			ResetSquareColours();
			UpdatePosition();
		}

		void ResetSquarePositions() {
			for(int rank = 0; rank < 8; rank++) {
				for(int file = 0; file < 8; file++) {;
					squareRenderers[file, rank].transform.position = PositionFromCoord(file, rank);
					squarePieceRenderers[file, rank].transform.position = PositionFromCoord(file, rank, PieceDepth);
				}
			}
		}

		public void SetPerspective(bool whitePointOfView) {
			WhitePointOfView = whitePointOfView;
			ResetSquarePositions();
		}

		public void ResetSquareColours() {
			bool whiteColor = !WhitePointOfView;
			
			for(int row = 0; row < 8; row++) {
				for(int col = 0; col < 8; col++) {
					Color squareColor = whiteColor ? boardTheme.WhiteSquares : boardTheme.BlackSquares;
					SetSquareColour(new Coord(row, col), squareColor);
					whiteColor = !whiteColor;
				}
				whiteColor = !whiteColor;
			}

			for(int i = 0; i < 4; i++) {
				Color squareColor = (i % 2) == 0 ? boardTheme.WhiteSquares : boardTheme.BlackSquares;
				squaresPromotions[i].material.color = squareColor;
			}
		}

		private void SetSquareColour(Coord square, Color color) {
			squareRenderers[square.row, square.col].material.color = color;
		}

		private Vector3 PositionFromCoord(int row, int col, float depth = 0) {
			if(WhitePointOfView) {
				return new Vector3(col - 3.5f, 3.5f - row, depth);
			}
			return new Vector3(3.5f - col, row - 3.5f, depth);
		}

		private Vector3 PositionFromCoord(Coord coord, float depth = 0) {
			return PositionFromCoord(coord.row, coord.col, depth);
		}

	}
}