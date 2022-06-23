using System;
using System.Collections.Generic;
using UnityEngine;

using Game.Chess;
using Game.Utils;

namespace Game.Players {
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

        override public void Update() {
            Vector2 mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);

            switch(inputState) {
                case InputState.None:
                    if(Input.GetMouseButtonDown(0) && boardUI.TryGetSquareUnderMouse(mousePosition, out squareSelected)) {
                        this.mouseDown = true;

                        inputState = InputState.Selected;
                        this.boardUI.SelectSquare(squareSelected);
                        this.boardUI.HighlightLegalMoves(squareSelected);
                    } else {
                        this.mouseDown = false;
                    }
                    break;
                case InputState.Selected:
                    if(Input.GetMouseButtonUp(0)) {
                        this.mouseDown = false;
                        inputState = InputState.None;
                        this.boardUI.ResetSquareColours();
                    } else if(this.mouseDown) {
                        inputState = InputState.Dragging;
                    } else {
                        this.inputState = InputState.None;
                    }
                    break;
                case InputState.Dragging:
                    if(Input.GetMouseButtonUp(0)) {
                        this.draggingToMouseUp(mousePosition);
                        this.boardUI.ResetSquareColours();
                        this.mouseDown = false;
                    } else if(this.mouseDown) {
                        this.boardUI.DragPiece(squareSelected, mousePosition);
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
                        this.boardUI.TryPromotion(mousePosition, out promotionType);

                    if(promotionType != 0) {
                        byte selectIndex = squareSelected.toBoardIndex();
                        byte targetIndex = squareTarget.toBoardIndex();
                        this.choseMove(new Move(selectIndex, targetIndex, promotionType));
                        this.boardUI.MoveMade();

                        this.boardUI.ClosePromotions();
                        inputState = InputState.None;
                    }
                    break;
            }

        }

        private void draggingToMouseUp(Vector2 mousePosition) {
            if(boardUI.TryGetSquareUnderMouse(mousePosition, out squareTarget)) {
                byte selectIndex = squareSelected.toBoardIndex();
                byte targetIndex = squareTarget.toBoardIndex();

                Piece piece = this.board[selectIndex];
                if(piece.IsPawn || piece.IsKing) {
                    List<Move> moves = this.board.GetValidPieceMoves(selectIndex);

                    foreach(Move move in moves) {
                        if(move.target == targetIndex) {
                            if(piece.IsPawn && move.type != Move.NORMAL &&
                                move.type != Move.DOUBLE_STEP_PAWN && move.type != Move.EN_PASSANT) {
                                inputState = InputState.Promotion;
                                this.boardUI.ShowPromotions();
                            } else {
                                this.inputState = InputState.None;
                                this.choseMove(move);
                            }
                            break;
                        }
                    }
                } else {
                    this.inputState = InputState.None;
                    this.choseMove(new Move(selectIndex, targetIndex, Move.NORMAL));
                }
                this.boardUI.MoveMade();
            } else {
                this.boardUI.ResetPiecePosition(squareSelected);
            }
        }

        override public void NotifyTurnToMove() {
            
        }

        override public void NotifyResult(Board.BoardState result) {
            string resultString = "";
            resultString = result == Board.BoardState.Draw ? "Draw" : "";
            resultString = result == Board.BoardState.WhiteWin ? "White Win!" : "";
            resultString = result == Board.BoardState.BlackWin ? "Black Win!" : "";
            Console.WriteLine(resultString);
        }
    }
}