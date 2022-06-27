using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Game.Chess;
using UI;

namespace Others {
    public class GameViewer : MonoBehaviour {
        [Multiline]
        public string pgn;
    
        private BoardUI boardUI;

        private List<Board> boards;
        private List<Move> moves;

        private Board.BoardState result;

        private int moveIndex;

        void Start() {
            try {
                PGNLoader.Load(pgn, out moves, out result);
                boards = new List<Board>();
                boards.Add(Board.CreateInitialPosition());
                moveIndex = 0;

                boardUI.SetNewPosition(boards[0]);
            } catch(Exception e){
                Console.WriteLine(e.Message);
			}
        }

        void Update() {
            if(Input.GetKeyDown(KeyCode.RightArrow)) {
                if(moveIndex < moves.Count) {
                    Board copy = moveIndex >= boards.Count ? this.boards[moveIndex].copy() : boards[moveIndex];
                    
                    boardUI.SetNewPosition(copy);
                    copy.TryMakeMove(moves[moveIndex]);

                    boardUI.OnMoveMade(moves[moveIndex], true);

                    if(moveIndex >= boards.Count)
                        this.boards.Add(copy);
                    moveIndex++;
                }
            }
            if(Input.GetKeyDown(KeyCode.LeftArrow)) {
                if(moveIndex > 0) {
                    moveIndex--;
                    this.boardUI.SetNewPosition(this.boards[moveIndex]);
                }
            }
        }
    }
}
