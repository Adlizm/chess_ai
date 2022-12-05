using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Core.Chess;
using UI;
using Others;

namespace UI {
    public class GameViewer : MonoBehaviour {
        [Multiline]
        public string pgn;
    
        private BoardUI boardUI;
        private Game game;

        private List<Move> moves;
        private int moveIndex;

        void Start() {
            try {
                Game.GameState result;
                PGNLoader.Load(pgn, out moves, out result);

                this.game = Game.CreateInitialPosition();
                moveIndex = 0;

                boardUI.SetNewPosition(game.GetPosition(0));
            } catch(Exception e){
                Console.WriteLine(e.Message);
			}
        }

        void Update() {
            if(Input.GetKeyDown(KeyCode.RightArrow)) {
                if(moveIndex < moves.Count) {
                    if(moveIndex >= game.TotalPositions - 1)
                        if (game.TryMakeMove(moves[moveIndex]))
                            moveIndex++;
                    else
                        moveIndex++;

                    boardUI.SetNewPosition(game.GetPosition(moveIndex));
                    boardUI.OnMoveMade(moves[moveIndex], true);
                }
            }
            if(Input.GetKeyDown(KeyCode.LeftArrow)) {
                if(moveIndex > 0) {
                    moveIndex--;
                    this.boardUI.SetNewPosition(game.GetPosition(moveIndex));
                }
            }
        }
    }
}
