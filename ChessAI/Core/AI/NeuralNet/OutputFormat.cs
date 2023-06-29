using System;
using System.Collections.Generic;

using Numpy;

using Core.Chess;
using Others.DataTranslate;

namespace Core.AI.NeuralNet {
    public static class OutputFormat {
        public const int TOTAL_MOVES = 1880;
        public const int TOTAL_TOWER_MOVES = 896;
        public const int TOTAL_BISHOP_MOVES = 560;
        public const int TOTAL_KNIGHT_MOVES = 336;

        private const float P_EQUALS_PROBABILITY = 1.0f / TOTAL_MOVES; 

        private static readonly int[] ACCUMULATE_INDEXS_BISHOP_MOVES = new int[64] {
            0,   7,  14,  21,  28,  35,  42,  49,
            56,  63,  72,  81,  90,  99, 108, 117,
            124, 131, 140, 151, 162, 173, 184, 193,
            200, 207, 216, 227, 240, 253, 264, 273,
            280, 287, 296, 307, 320, 333, 344, 353,
            360, 367, 376, 387, 398, 409, 420, 429,
            436, 443, 452, 461, 470, 479, 488, 497,
            504, 511, 518, 525, 532, 539, 546, 553
        };
        private static readonly int[] ACCUMULATE_INDEXS_KNIGHT_MOVES = new int[64] {
            0,   2,   5,   9,  13,  17,  21,  24,
            26,  29,  33,  39,  45,  51,  57,  61,
            64,  68,  74,  82,  90,  98, 106, 112,
            116, 120, 126, 134, 142, 150, 158, 164,
            168, 172, 178, 186, 194, 202, 210, 216,
            220, 224, 230, 238, 246, 254, 262, 268,
            272, 275, 279, 285, 291, 297, 303, 307,
            310, 312, 315, 319, 323, 327, 331, 334
        };

        private static readonly int[] ACCUMULATE_INDEXS_PROMOTION_MOVES = new int[64]{
             0,  1, -1, -1, -1, -1, -1, -1,
             2,  3,  4, -1, -1, -1, -1, -1,
            -1,  5,  6,  7, -1, -1, -1, -1,
            -1, -1,  8,  9, 10, -1, -1, -1,
            -1, -1, -1, 11, 12, 13, -1, -1,
            -1, -1, -1, -1, 14, 15, 16, -1,
            -1, -1, -1, -1, -1, 17, 18, 19,
            -1, -1, -1, -1, -1, -1, 20, 21,
        };

        public static int MoveToIndex(Move move) {
            // TOTAL: 1876
            if(move.type >= Move.NORMAL && move.type <= Move.LIT_ROOK) {
                // others moves - TOTAL: 1792
                int LeftSelect = move.selected % 8;
                int RightSelect = 7 - LeftSelect;

                int TopSelect = move.selected / 8;
                int BottomSelect = 7 - TopSelect;

                int LeftTarget = move.target % 8;
                int TopTarget = move.target / 8;

                int dx = Math.Abs(LeftSelect - LeftTarget);
                int dy = Math.Abs(TopSelect - TopTarget);
                if(dx == 0 || dy == 0) {
                    //tower moves - TOTAL: 896
                    int[] movesOffSets = new int[4] { -1, 1, -8, 8 };
                    int[] steps = new int[4] { LeftSelect, RightSelect, TopSelect, BottomSelect };
                    int accIndex = 14 * move.selected;

                    for(int i = 0; i < 4; i++)
                        for(int j = 1; j <= steps[i]; j++, accIndex++)
                            if(move.selected + j * movesOffSets[i] == move.target)
                                return accIndex;
                }
                if(dx == dy) {
                    //bishop moves - TOTAL: 560
                    int[] movesOffSets = new int[4] { -9, -7, 7, 9 };
                    int[] steps = new int[4] { Math.Min(LeftSelect, TopSelect),   Math.Min(RightSelect, TopSelect),
                                            Math.Min(LeftSelect, BottomSelect),Math.Min(RightSelect, BottomSelect) };

                    int accIndex = ACCUMULATE_INDEXS_BISHOP_MOVES[move.selected] + TOTAL_TOWER_MOVES;
                    for(int i = 0; i < 4; i++)
                        for(int j = 1; j <= steps[i]; j++, accIndex++)
                            if(move.selected + j * movesOffSets[i] == move.target)
                                return accIndex;
                }
                if(dx == 2 && dy == 1 || dx == 1 && dy == 2) {
                    //knight moves - TOTAL: 336
                    int[] offsetRow = new int[8] { 2,  2, 1,  1, -1, -1, -2, -2 };
                    int[] offsetCol = new int[8] { 1, -1, 2, -2,  2, -2,  1, -1 };

                    int accIndex = ACCUMULATE_INDEXS_KNIGHT_MOVES[move.selected] + TOTAL_TOWER_MOVES + TOTAL_BISHOP_MOVES;
                    for(int i = 0; i < 8; i++) {
                        int col = LeftSelect + offsetCol[i];
                        int row = TopSelect + offsetRow[i];
                        if(col >= 0 && col < 8 && row >= 0 && row < 8) {
                            if(row * 8 + col == move.target)
                                return accIndex;
                            accIndex++;
                        }
                    }
                }
            }
            if(move.type >= Move.PROMOTION_BISHOP && move.type <= Move.PROMOTION_QUEEN) {
                // promotions - TOTAL: 88
                int colSelect = move.selected % 8;
                int colTarget = move.target % 8;

                if(Math.Abs(colSelect - colTarget) > 1)
                    return -1;

                int index = ACCUMULATE_INDEXS_PROMOTION_MOVES[colSelect * 8 + colTarget] + (move.type - Move.PROMOTION_BISHOP) * 22;
                return index + TOTAL_TOWER_MOVES + TOTAL_BISHOP_MOVES + TOTAL_KNIGHT_MOVES;
            }
            return -1;
        }

		public static int[] ValidOutputIndexs(Game game) {
            List<Move> moves = game.GetAllValidMoves();
            int[] indexs = new int[moves.Count];

            for(int i = 0; i < moves.Count; i++) {
                indexs[i] = MoveToIndex(moves[i]);
            }

            return indexs;
        }

        public static (float[], float) OutputValues(NDarray[] output) {
            float[] p = output[0][0].GetData<float>();
            float   v = output[1][0].GetData<float>()[0];
            return (p, v);
        }


        public static NDarray[] ExpectedFromTriples(List<Triple> triples) {
            float[,] eP = new float[triples.Count, TOTAL_MOVES];
            float[,] eV = new float[triples.Count, 1];

            for(int i = 0; i < triples.Count; i++) {
                Triple triple = triples[i];

                var p = ExpectedPolicyVector(triple);
                for(int j = 0; j < TOTAL_MOVES; j++) 
                    eP[i, j] = p[j];
                

                eV[i, 0] = ExpectedValue(triple);
            }

            return new NDarray[] { np.array(eP), np.array(eV) };
        }
        

        public static float[] ExpectedPolicyVector(Triple triple) {
            float[] expectedP = new float[TOTAL_MOVES];

            float totalVisits = 0.0f;
            for(int i = 0; i < triple.MoveN.Length; i++)
                totalVisits += triple.MoveN[i].Item2;

            if(totalVisits != 0) {
                for(int i = 0; i < triple.MoveN.Length; i++) {
                    int index = MoveToIndex(Move.FromShort(triple.MoveN[i].Item1));
                    if(index != -1)
                        expectedP[index] = triple.MoveN[i].Item2 / totalVisits;
                }
            }
            return expectedP;
        }


        public static float ExpectedValue(Game game) {
            switch(game.CurrentState) {
                case Game.GameState.WhiteWin:
                    return 1.0f;
                case Game.GameState.BlackWin:
                    return -1.0f;
            }
            return 0.0f;
        }
        public static float ExpectedValue(Game.GameState gameResult) {
            switch(gameResult) {
                case Game.GameState.WhiteWin:
                    return 1.0f;
                case Game.GameState.BlackWin:
                    return -1.0f;
            }
            return 0.0f;
        }
        public static float ExpectedValue(Triple triple) {
            return triple.R;
        }
    }
    
}
