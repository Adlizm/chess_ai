using System;
using System.Collections.Generic;

using Numpy;

using Core.Chess;
using Others.DataTranslate;

namespace Core.AI.NeuralNet {
    public static class InputFormat {
        public const int RECENT_POSITIONS = 8;
        public const int TOTAL_8X8_BLOCKS = RECENT_POSITIONS + 4;

        public static float PieceToInputValue(Piece piece) {
            float value = (float) piece.Type;
            return piece.Color == Piece.White ? value : -value;
        }

        public static byte[] GameToInputBytes(Game game) {
            var S = new byte[8 * 8 * TOTAL_8X8_BLOCKS];
            int index = 0;
            for(int i = game.TotalPositions - 1, j = 0; i >= 0 && j < RECENT_POSITIONS; i--, j++) {
                Board position = game.GetPosition(i);
                for(int s = 0; s < 64; s++, index++)
                    S[index] = position[s].Byte;
            }

            byte rooks = (byte) game.Rooks;
            byte enpassant = (byte) (game.EnPassant.valid ? game.EnPassant.column + 1 : 0);
            byte last = (byte) game.LastCaptureOrPawnMove;

            index = 8 * 8 * RECENT_POSITIONS - 1;
            for(int s = 0; s < 64; s++, index++) {
                S[index] = game.TimeOf;
                S[index + 64] = rooks;
                S[index + 128] = enpassant;
                S[index + 192] = last;
            }
            return S;
        }
        public static NDarray GameToInput(Game game) {
            float[,,] input = new float[TOTAL_8X8_BLOCKS, 8, 8];

            for(int i = game.TotalPositions - 1, j = 0; i >= 0 && j < RECENT_POSITIONS; i--, j++) {
                Board position = game.GetPosition(i);
                for(int row = 0; row < 8; row++)
                    for(int col = 0; col < 8; col++) 
                        input[j, row, col] = PieceToInputValue(position[row*8 + col]);
            }
            
            float TimeOf = game.TimeOf == Piece.White ? 1.0f : -1.0f;
            float Rooks = (float) game.Rooks;
            float EnPassant = (float) (game.EnPassant.valid ? game.EnPassant.column + 1 : 0);
            float LastCaptureOrPawnMove = (float) game.LastCaptureOrPawnMove;
            for(int row = 0; row < 8; row++){
                for(int col = 0; col < 8; col++){
                    input[RECENT_POSITIONS    , row, col] = TimeOf;
                    input[RECENT_POSITIONS + 1, row, col] = Rooks;
                    input[RECENT_POSITIONS + 2, row, col] = EnPassant;
                    input[RECENT_POSITIONS + 3, row, col] = LastCaptureOrPawnMove;
                }
            }
            
            return np.array(input).reshape(1, TOTAL_8X8_BLOCKS, 8, 8);
        }

        public static NDarray InputsFromTriples(List<Triple> triples) {
            float[,,,] input = new float[triples.Count, TOTAL_8X8_BLOCKS, 8, 8];

            for(int i = 0; i < triples.Count; i++) {
                var triple = triples[i];

                int index = 0;
                for(int j = 0; j < RECENT_POSITIONS; j++) {
                    for(int row = 0; row < 8; row++)
                        for(int col = 0; col < 8; col++)
                            input[i, j, row, col] = PieceToInputValue(new Piece(triple.S[index++]));
                }

                float TimeOf = triple.S[index] == Piece.White ? 1.0f : -1.0f;
                float Rooks  = triple.S[index + 64];
                float EnPassant = triple.S[index + 128];
                float LastCaptureOrPawnMove = triple.S[index + 192];

                for(int row = 0; row < 8; row++) {
                    for(int col = 0; col < 8; col++) {
                        input[i, RECENT_POSITIONS, row, col] = TimeOf;
                        input[i, RECENT_POSITIONS + 1, row, col] = Rooks;
                        input[i, RECENT_POSITIONS + 2, row, col] = EnPassant;
                        input[i, RECENT_POSITIONS + 3, row, col] = LastCaptureOrPawnMove;
                    }
                }
            }

            /*
            for(int i = 0; i < triples.Count; i++) {
                for(int j = 0; j < TOTAL_8X8_BLOCKS; j++) {
                    for(int row = 0; row < 8; row++) {
                        for(int col = 0; col < 8; col++) {
                            var value = input[i, RECENT_POSITIONS + 3, row, col];
                            if(float.IsNaN(value) || float.IsInfinity(value)) {
                                Console.WriteLine("Invalid Value ({0},{1},{2},{3}): {4}", i, j, row, col, value);   
                            }
                        }
                    }
                }
            }
            */

            return np.array(input);
		}
	}
}
