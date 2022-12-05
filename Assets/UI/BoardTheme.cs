using UnityEngine;

namespace UI {
	[CreateAssetMenu(menuName = "Theme/Board")]
	public class BoardTheme : ScriptableObject {
		public Color WhiteSquares;
		public Color BlackSquares;

		public Color LegalMoves;
		public Color Selected;
	}
}