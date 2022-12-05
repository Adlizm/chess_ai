using System;
using System.Collections.Generic;
using UnityEngine;

using Core.Chess;
using Utils;
using UI;

namespace Core.Players {
    public class HumanPlayer : Player {
        private enum InputState{
            None, Selected, Dragging, Promotion
        }

        private Coord squareSelected;
        private Coord squareTarget;

        private Camera cam;
        private InputState inputState;
        private bool mouseDown, mouseUp;

        public HumanPlayer(byte color, Player.ChoseMove chose, Player.RequestDraw draw, Player.AdmitDefeat defeat)
            : base(color, chose, draw, defeat) {
            
            this.cam = Camera.main;
            this.inputState = InputState.None;
        }

        override public void Update(BoardUI boardUI) {
            Vector2 mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);

            switch(inputState) {
                case InputState.None:
                    if(Input.GetMouseButtonDown(0) && boardUI.TryGetSquareUnderMouse(mousePosition, out squareSelected)) {
                        this.mouseDown = true;

                        inputState = InputState.Selected;
                        boardUI.SelectSquare(squareSelected);
                        boardUI.HighlightMoves(this.game.GetValidPieceMoves(squareSelected.ToBoardIndex()));
                    } else {
                        this.mouseDown = false;
                    }
                    break;
                case InputState.Selected:
                    if(Input.GetMouseButtonUp(0)) {
                        this.mouseDown = false;
                        inputState = InputState.None;
                        boardUI.ResetSquareColours();
                    } else if(this.mouseDown) {
                        inputState = InputState.Dragging;
                    } else {
                        this.inputState = InputState.None;
                    }
                    break;
                case InputState.Dragging:
                    if(Input.GetMouseButtonUp(0)) {
                        this.DraggingToMouseUp(mousePosition, boardUI);
                        boardUI.ResetSquareColours();
                        this.mouseDown = false;
                    } else if(this.mouseDown) {
                        boardUI.DragPiece(squareSelected, mousePosition);
                    } else {
                        this.inputState = InputState.None;
                    }
                    break;
                case InputState.Promotion:
                    byte promotionType = 0;
                    if(Input.GetKeyDown(KeyCode.Alpha4))
                        promotionType = Move.PROMOTION_KNIGHT;
                    if(Input.GetKeyDown(KeyCode.Alpha3))
                        promotionType = Move.PROMOTION_BISHOP;
                    if(Input.GetKeyDown(KeyCode.Alpha2))
                        promotionType = Move.PROMOTION_TOWER;
                    if(Input.GetKeyDown(KeyCode.Alpha1))
                        promotionType = Move.PROMOTION_QUEEN;

                    if(Input.GetMouseButtonDown(0))
                        boardUI.TryPromotion(mousePosition, out promotionType);

                    if(promotionType != 0) {
                        byte selectIndex = squareSelected.ToBoardIndex();
                        byte targetIndex = squareTarget.ToBoardIndex();
                        Move move = new Move(selectIndex, targetIndex, promotionType);

                        this.choseMove(move);
                        boardUI.OnMoveMade(move);

                        boardUI.ClosePromotions();
                        inputState = InputState.None;
                    }
                    break;
            }

        }

        private void DraggingToMouseUp(Vector2 mousePosition, BoardUI boardUI) {
            if(boardUI.TryGetSquareUnderMouse(mousePosition, out squareTarget)) {
                byte selectIndex = squareSelected.ToBoardIndex();
                byte targetIndex = squareTarget.ToBoardIndex();

                Move moveSelected = new Move(0, 0, 0);

                Piece piece = this.game[selectIndex];
                if(piece.IsPawn || piece.IsKing) {
                    List<Move> moves = this.game.GetValidPieceMoves(selectIndex);
                    

                    foreach(Move move in moves) {
                        if(move.target == targetIndex) {
                            if(piece.IsPawn && move.type != Move.NORMAL &&
                                move.type != Move.DOUBLE_STEP_PAWN && move.type != Move.EN_PASSANT) {
                                inputState = InputState.Promotion;
                                boardUI.ShowPromotions(this.Color);
                            } else {
                                this.inputState = InputState.None;
                                moveSelected = move;
                                this.choseMove(moveSelected);
                                
                            }
                            break;
                        }
                    }
                } else {
                    this.inputState = InputState.None;
                    moveSelected = new Move(selectIndex, targetIndex, Move.NORMAL);
                    this.choseMove(moveSelected);
                }
                boardUI.OnMoveMade(moveSelected);
            } else {
                boardUI.ResetPiecePosition(squareSelected);
            }
        }

        override public void NotifyTurnToMove() {
            
        }

        override public void NotifyResult(Game.GameState result) {
            string resultString = "";
            resultString = result == Game.GameState.Draw ? "Draw" : resultString;
            resultString = result == Game.GameState.WhiteWin ? "White Win!" : resultString;
            resultString = result == Game.GameState.BlackWin ? "Black Win!" : resultString;
            Debug.Log(resultString);
        }
    }
}