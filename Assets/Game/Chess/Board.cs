using UnityEngine;

using System;
using System.Collections.Generic;

namespace Game.Chess {
    public class Board {
        public enum BoardState {
            Playing, WhiteWin, BlackWin, Draw
        }

        // String de configuração segue a notação de Forsyth-Edwards
        public const string DefaultBoard = "tnbqkbnt/pppppppp/8/8/8/8/PPPPPPPP/TNBQKBNT w kKqQ - 1 0";

        private Piece[] data;

        private byte timeOf;
        private bool[] biggerRook;
        private bool[] littlerRook;
        private OnPassan onPassan;
        private int lastCaptureOrPawnMove;
        private int rounds;

        public Board() {
            this.data = new Piece[64];
            this.timeOf = 0;
            this.biggerRook = new bool[2] { false, false }; // [Black, Withe]
            this.littlerRook = new bool[2] { false, false }; // [Black, Withe]
            this.onPassan = new OnPassan();
            this.lastCaptureOrPawnMove = 0;
            this.rounds = 1;
        }

        public Board copy() {
            Board copy = new Board();
            copy.timeOf = this.timeOf;
			for(int i = 0; i < 64; i++) {
                copy.data[i] = this.data[i];
			}
            copy.biggerRook[0] = this.biggerRook[0];
            copy.biggerRook[1] = this.biggerRook[1];

            copy.littlerRook[0] = this.littlerRook[0];
            copy.littlerRook[1] = this.littlerRook[1];

            copy.lastCaptureOrPawnMove = this.lastCaptureOrPawnMove;
            copy.rounds = this.rounds;

            copy.onPassan = this.onPassan;
            return copy;
        }
        public Piece this[int index] {
            get => this.data[index];
        }
        public byte TimeOf {
            get => this.timeOf;
        }

        static public Board CreateInitialPosition() {
            return CreateBoard(DefaultBoard);
        }
        static public Board CreateBoard(string displayBoard) {
            Dictionary<char, byte> pieceTypeFromLetter = new Dictionary<char, byte>() {
                { 'p', Piece.Pawn }, { 'b', Piece.Bishop }, { 'n', Piece.Knight },
                { 't', Piece.Tower }, { 'q', Piece.Queen }, { 'k', Piece.King }
            };

            Board myBoard = new Board();

            int nBars = 0;
            int offSet = 0;
            int index = 0;
			foreach(char letter in displayBoard) {
                index++;
                if(nBars == 7 && offSet == 8 && letter == ' ') 
                    break;

                if(letter == '/' && offSet == 8) {
                    nBars++;
                    offSet = 0;
                } else {
                    if(char.IsDigit(letter)) {
                        int plus = (int) (letter - '0');
                        offSet += plus;
                    } else {
                        byte colorPiece = char.IsUpper(letter) ? Piece.White : Piece.Black;
                        byte pieceType = pieceTypeFromLetter[char.ToLower(letter)];

                        if(pieceType == 0) {
                            throw new FormatException("Character found is not compatible with a Chess Piece");
                        }
                        myBoard.data[nBars * 8 + offSet] = new Piece((byte) (colorPiece | pieceType) );
                        offSet++;
                    }
                }
                if(offSet > 8) {
                    throw new FormatException("String is incompatible with the columns on a chessboard");
                }
            }
            String configBoard = displayBoard.Substring(index);
            String[] configs = configBoard.Split(' ');

            if(configs.Length != 5)
                throw new FormatException("String not contain five configs params");

            if(!configs[0].Equals("w") && configs[0].Equals("b"))
                throw new FormatException("String does not contain a valid character to indicate a turn");

            myBoard.timeOf = configs[0].Equals("w") ? Piece.White : Piece.Black;

            if(configs[1].Length > 4 || (configs[1].Length == 1 && configs[1].Equals("-")) )
                throw new FormatException("String Format not contain a valid params to indicate the Rooks moviments");

            foreach(char letter in configs[1]) {
                switch(letter) {
                    case 'k':
                        myBoard.littlerRook[0] = true; 
                        break;
                    case 'K':
                        myBoard.littlerRook[1] = true;
                        break;
                    case 'q':
                        myBoard.biggerRook[0] = true;
                        break;
                    case 'Q':
                        myBoard.biggerRook[1] = true;
                        break;
                    case '-':
                        break;
                    default:
                        throw new FormatException("String Format not contain a valid params to indicate the Rooks moviments");
                }
            }

            if(configs[2].Length != 1 || (configs[2][0] != '-' && (configs[2][0] < '0' || configs[2][0] > '8') ))
                throw new FormatException("String Format not contain a valid params to indicate the EnPassant moviment");

            myBoard.onPassan.valid = configs[2][0] != '-';
            myBoard.onPassan.column = (byte) (configs[2][0] - '0');

            if( (myBoard.lastCaptureOrPawnMove = int.Parse(configs[3])) < 0 )
                throw new FormatException("String Format not contain a valid params to indicate the last capture");

            if( (myBoard.rounds = int.Parse(configs[4])) < 0 )
                throw new FormatException("String Format not contain a valid params to indicate the current round");
           
            return myBoard;
        }

        public BoardState GetBoardState() {
            if(this.lastCaptureOrPawnMove >= 50)
                return BoardState.Draw;

            byte enemyColor = this.timeOf == Piece.White ? Piece.Black : Piece.White;
            if(this.GetAllValidMoves().Count == 0) {
                int kingLocal = this.getKingLocal(this.timeOf);
                if(this.existsAttackInLocal(kingLocal, enemyColor))
                    return enemyColor == Piece.White ? BoardState.WhiteWin : BoardState.BlackWin;

                return BoardState.Draw;
            }

            int[] countBishopOrKinght = new int[2]; //[Black, White]
            for(int i = 0; i < 63; i++) {
                Piece piece = this.data[i];
                byte type = piece.Type;

                switch(type) {
                    case Piece.None:
                    case Piece.King:
                        break;

                    case Piece.Pawn:
                    case Piece.Tower:
                    case Piece.Queen:
                        return BoardState.Playing;

                    case Piece.Knight:
                    case Piece.Bishop:
                        int index = piece.IsWhite ? 1 : 0;
                        countBishopOrKinght[index]++;
                        break;
                }
            }
            return countBishopOrKinght[0] <= 1 && countBishopOrKinght[1] <= 1 ? BoardState.Draw : BoardState.Playing;
        }

        public List<Move> GetAllValidMoves() {
            List<Move> moves = new List<Move>();

            for(byte i = 0; i < 64; i++) {
                List<Move> pieceMoves = this.getPieceMoves(i);
                moves.AddRange(pieceMoves);
            }
            return filterInvalidMoves(moves);
        }
        public List<Move> GetValidPieceMoves(byte localPiece) {
            List<Move> moves = new List<Move>();
                
            moves = this.getPieceMoves(localPiece);
            return this.filterInvalidMoves(moves);
        }
        
        private List<Move> getPieceMoves(byte localPiece) {
            Piece piece = this.data[localPiece];
            byte type = piece.Type;

            List<Move> moves = new List<Move>();
            if(!piece.IsPiece || !piece.isSameColor(this.timeOf))
                return moves;

            switch(type) {
                case Piece.Pawn:
                    return this.getPawnMoves(localPiece);
                case Piece.Bishop:
                    return this.getBishopMoves(localPiece);
                case Piece.Knight:
                    return this.getKnightMoves(localPiece);
                case Piece.Tower:
                    return this.getTowerMoves(localPiece);
                case Piece.Queen:
                    return this.getQueenMoves(localPiece);
                case Piece.King:
                    return this.getKingMoves(localPiece);
            }
            return moves;
        }
        private List<Move> filterInvalidMoves(List<Move> moves) {
            byte enemyColor = this.timeOf == Piece.White ? Piece.Black : Piece.White;

            for(int i = moves.Count - 1; i >= 0 ; i--) {
                Board copy = this.copy();
                Move move = moves[i];
                switch(move.type) {
                    case Move.BIG_ROOK:
                        copy.timeOf = enemyColor;
                        if(copy.existsAttackInLocal(move.selected, enemyColor) ||
                           copy.existsAttackInLocal(move.selected - 1, enemyColor) ||
                           copy.existsAttackInLocal(move.selected - 2, enemyColor)) {
                            moves.RemoveAt(i);
                        }
                        break;
                    case Move.LIT_ROOK:
                        copy.timeOf = enemyColor;
                        if(copy.existsAttackInLocal(move.selected, enemyColor) ||
                           copy.existsAttackInLocal(move.selected + 1, enemyColor) ||
                           copy.existsAttackInLocal(move.selected + 2, enemyColor)) {
                            moves.RemoveAt(i);
                        }
                        break;
                    default:
                        copy.makeMove(moves[i]);
                        byte localKing = copy.getKingLocal(this.timeOf);
                        if(copy.existsAttackInLocal(localKing, enemyColor)) {
                            moves.RemoveAt(i);
                        }
                        break;
                }
            }
            
            return moves;
        }
        
        public bool TryMakeMove(Move mymove) {
            Piece piece = this.data[mymove.selected];
            byte enemyColor = this.timeOf == Piece.White ? Piece.Black : Piece.White;

            if(!piece.isSameColor(this.timeOf)) {
                Console.WriteLine("Treid move a enemy piece");
                return false;
            }

            List<Move> pieceMove = this.GetValidPieceMoves(mymove.selected);
            foreach(Move move in pieceMove) {
                if(mymove == move) {
                    this.makeMove(mymove);
                    return true;
                }
            }
            return false;
        }
        private void makeMove(Move move) {
            Piece piece = this.data[move.selected];
            byte colorPiece = piece.Color;

            int indexRook = colorPiece == Piece.White ? 1 : 0;
            int row = move.selected / 8;
            int col = move.selected % 8;

            this.lastCaptureOrPawnMove++;

            Piece targetPiece = this.data[move.target];
            if(piece.IsPawn || targetPiece.IsPiece)
                this.lastCaptureOrPawnMove = 0;
            if(targetPiece.IsTower) {
                if(move.target % 8 == 7) {
                    if(move.target / 8 == 0)
                        this.littlerRook[0] = false;
                    else if(move.target / 8 == 7)
                        this.littlerRook[1] = false;
                }else if(move.target % 8 == 0) {
                    if(move.target / 8 == 0)
                        this.biggerRook[0] = false;
                    else if(move.target / 8 == 7)
                        this.biggerRook[1] = false;
                }
            }

            if(this.timeOf == Piece.Black)
                this.rounds++;

            this.timeOf = this.timeOf == Piece.White ? Piece.Black : Piece.White;
            this.data[move.target] = piece;
            this.data[move.selected] = new Piece(Piece.None);
            this.onPassan.valid = false;

            switch(move.type) {
                case Move.NORMAL:
                    if(piece.IsKing) {
                        this.biggerRook[indexRook] = false;
                        this.littlerRook[indexRook] = false;
                    }else if(piece.IsTower && col == 0) {
                        this.biggerRook[indexRook] = false;
                    }else if(piece.IsTower && col == 7) {
                        this.littlerRook[indexRook] = false;
                    }
                    break;
                case Move.DOUBLE_STEP_PAWN:
                    this.onPassan.valid = true;
                    this.onPassan.column = (byte) (move.selected % 8);
                    break;
                case Move.EN_PASSANT:
                    int rowPassan = piece.IsWhite ? 3 : 4;
                    this.data[rowPassan * 8 + this.onPassan.column] = new Piece(Piece.None);
                    break;
                case Move.BIG_ROOK:
                    this.biggerRook[indexRook] = false;
                    this.littlerRook[indexRook] = false;
                    this.data[row * 8] = new Piece(Piece.None);
                    this.data[row * 8 + 3] = new Piece((byte) (Piece.Tower | colorPiece));
                    break;
                case Move.LIT_ROOK:
                    this.biggerRook[indexRook] = false;
                    this.littlerRook[indexRook] = false;
                    this.data[row * 8 + 7] = new Piece(Piece.None);
                    this.data[row * 8 + 5] = new Piece((byte) (Piece.Tower | colorPiece));
                    break;
                case Move.PROMOTION_BISHOP:
                    this.data[move.target] = new Piece((byte) (Piece.Bishop | colorPiece));
                    break;
                case Move.PROMOTION_KNIGHT:
                    this.data[move.target] = new Piece((byte) (Piece.Knight | colorPiece));
                    break;
                case Move.PROMOTION_TOWER:
                    this.data[move.target] = new Piece((byte) (Piece.Tower | colorPiece));
                    break;
                case Move.PROMOTION_QUEEN:
                    this.data[move.target] = new Piece((byte) (Piece.Queen | colorPiece));
                    break;
            }
        }

        private List<Move> getPawnMoves(byte localPiece) {
            List<Move> moves = new List<Move>();
            Piece piece = this.data[localPiece];

            byte offsetTopBottom = (byte) (piece.IsWhite ? -1 : 1);
            byte col = (byte) (localPiece % 8);
            byte row = (byte) (localPiece / 8);
            int rowOnPassan = (byte) (piece.IsWhite ? 3 : 4);

            byte targetLocal = (byte) (localPiece + offsetTopBottom * 8);
            byte rowTarget = (byte) (targetLocal / 8);
            bool doubleStep = row == (piece.IsWhite ? 6 : 1);
            bool promotion = rowTarget == 0 || rowTarget == 7;


            Piece pieceForward = this.data[targetLocal];
            if(!pieceForward.IsPiece) {
                if(promotion) {
                    moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_BISHOP));
                    moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_KNIGHT));
                    moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_TOWER));
                    moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_QUEEN));
                }else {
                    moves.Add(new Move(localPiece, targetLocal, Move.NORMAL));
                }

                if(doubleStep) {
                    byte targetDouble = (byte) (localPiece + offsetTopBottom * 8 * 2);
                    pieceForward = this.data[targetDouble];
                    if(!pieceForward.IsPiece) {
                        if(piece.IsWhite && row == 6) {

                            moves.Add(new Move(localPiece, targetDouble, Move.DOUBLE_STEP_PAWN));
                        } else if(row == 1) {
                            moves.Add(new Move(localPiece, targetDouble, Move.DOUBLE_STEP_PAWN));
                        }
                    }
                }
            }

            targetLocal = (byte) (localPiece + offsetTopBottom * 8 - 1);
            if(col > 0) {
                pieceForward = this.data[targetLocal];
                if(piece.isEnemy(pieceForward)) {
                    if(promotion) {
                        moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_BISHOP));
                        moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_KNIGHT));
                        moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_TOWER));
                        moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_QUEEN));
                    }else {
                        moves.Add(new Move(localPiece, targetLocal, Move.NORMAL));
                    }
                }

                if(this.onPassan.valid && this.onPassan.column == col - 1 && row == rowOnPassan)
                    moves.Add(new Move(localPiece, targetLocal, Move.EN_PASSANT)); 
            }

            targetLocal = (byte) (localPiece + offsetTopBottom * 8 + 1);
            if(col < 7) {
                pieceForward = this.data[targetLocal];
                if(piece.isEnemy(pieceForward)) {
                    if(promotion) {
                        moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_BISHOP));
                        moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_KNIGHT));
                        moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_TOWER));
                        moves.Add(new Move(localPiece, targetLocal, Move.PROMOTION_QUEEN));
                    }else {
                        moves.Add(new Move(localPiece, targetLocal, Move.NORMAL));
                    }
                }

                if(this.onPassan.valid && this.onPassan.column == col + 1 && row == rowOnPassan)
                    moves.Add(new Move(localPiece, targetLocal, Move.EN_PASSANT));
            }

            return moves;
        }
        private List<Move> getBishopMoves(byte localPiece) {
            List<Move> moves = new List<Move>();
            Piece piece = this.data[localPiece];

            int left = localPiece % 8, right = 7 - left;
            int top = localPiece / 8, bottom = 7 - top;

            int[] movesOffSets = new int[4] { -9, -7, 7, 9 };
            int[] steps = new int[4] { Math.Min(left, top),   Math.Min(right, top),
                                       Math.Min(left, bottom),Math.Min(right, bottom) };


            for(int j = 0; j < 4; j++) {
                int moveOffSet = movesOffSets[j];
                int step = steps[j];

                for(int i = 1; i <= step; i++) {
                    byte target = (byte) (localPiece + i * moveOffSet);
                    Piece pieceTarget = this.data[target];
                    if(pieceTarget.IsPiece) {
                        if(piece.isEnemy(pieceTarget)) {
                            moves.Add(new Move(localPiece, target, Move.NORMAL));
                        }
                        break;
                    }
                    moves.Add(new Move(localPiece, target, Move.NORMAL));
                }
            }

            return moves;
        }
        private List<Move> getKnightMoves(byte localPiece) {
            List<Move> moves = new List<Move>();
            Piece piece = this.data[localPiece];

            int row = localPiece / 8, col = localPiece % 8;

            int[] offsetRow = new int[8] { 2, 2, 1, 1, -1, -1, -2, -2 };
            int[] offsetCol = new int[8] { 1, -1, 2, -2, 2, -2, 1, -1 };

            for(int i = 0; i < 8; i++) {
                int targetRow = row + offsetRow[i];
                int targetCol = col + offsetCol[i];
                if(targetRow >= 0 && targetRow < 8 && targetCol >= 0 && targetCol < 8) {
                    byte targetLocal = (byte) (targetRow * 8 + targetCol);
                    Piece targetPiece = this.data[targetLocal];
                    if(targetPiece.IsPiece && piece.isAlly(targetPiece))
                        continue;
                    moves.Add(new Move(localPiece, targetLocal, Move.NORMAL));
                }
            }
            return moves;
        }
        private List<Move> getTowerMoves(byte localPiece) {
            List<Move> moves = new List<Move>();
            Piece piece = this.data[localPiece];

            int left = localPiece % 8, right = 7 - left;
            int top = localPiece / 8, bottom = 7 - top;

            int[] movesOffSets = new int[4] { -1, 1, -8, 8 };
            int[] steps = new int[4] { left, right, top, bottom };


            for(int j = 0; j < 4; j++) {
                int moveOffSet = movesOffSets[j];
                int step = steps[j];

                for(int i = 1; i <= step; i++) {
                    byte target = (byte) (localPiece + i * moveOffSet);
                    Piece pieceTarget = this.data[target];
                    if(pieceTarget.IsPiece) {
                        if(piece.isEnemy(pieceTarget)) {
                            moves.Add(new Move(localPiece, target, Move.NORMAL));
                        }
                        break;
                    }
                    moves.Add(new Move(localPiece, target, Move.NORMAL));
                }
            }

            return moves;
        }
        private List<Move> getQueenMoves(byte localPiece) {
            List<Move> moves = this.getTowerMoves(localPiece);
            moves.AddRange(this.getBishopMoves(localPiece));
            return moves;
        }
        private List<Move> getKingMoves(byte localPiece) {
            List<Move> moves = new List<Move>();
            Piece piece = this.data[localPiece];

            byte enemyColor = piece.IsWhite ? Piece.Black : Piece.White;
            int rowRook = piece.IsWhite ? 7 : 0;
            int row = localPiece / 8; 
            int col = localPiece % 8;
            int indexRook = piece.IsWhite ? 1 : 0;


            if(col == 4 && row == rowRook) {
                if(this.biggerRook[indexRook]) {
                    bool ok = !this.data[localPiece - 1].IsPiece && !this.data[localPiece - 2].IsPiece && 
                        !this.data[localPiece - 3].IsPiece;
                    if(ok && this.data[localPiece - 4].IsTower && piece.isAlly(this.data[localPiece - 4])) 
                        moves.Add(new Move(localPiece, (byte) (localPiece - 2), Move.BIG_ROOK));
                }
                if(this.littlerRook[indexRook]) {
                    bool ok = !this.data[localPiece + 1].IsPiece && !this.data[localPiece + 2].IsPiece;
                    if(ok && this.data[localPiece + 3].IsTower && piece.isAlly(this.data[localPiece + 3]))
                        moves.Add(new Move(localPiece, (byte) (localPiece + 2), Move.LIT_ROOK));
                }
            }

            for(int i = Math.Max(0, row - 1); i <= Math.Min(7, row + 1); i++) {
                for(int j = Math.Max(0, col - 1); j <= Math.Min(7, col + 1); j++) {
                    if(i == row && j == col)
                        continue;

                    byte targetLocal =  (byte) (i * 8 + j);
                    Piece pieceTarget = this.data[targetLocal];
                    if(!piece.isAlly(pieceTarget)) {
                        moves.Add(new Move(localPiece, targetLocal, Move.NORMAL));
                    }
                }
            }

            return moves;
        }

        public bool KingInCheck {
            get {
                byte localKing = this.getKingLocal(this.timeOf);
                byte enemyColor = this.timeOf == Piece.White ? Piece.Black : Piece.White;
                return existsAttackInLocal(localKing, enemyColor);
            }
        }
        private byte getKingLocal(byte kingColor) {
            for(byte i = 0; i < 64; i++) {
                Piece piece = this.data[i];
                if(piece.IsKing && piece.isSameColor(kingColor))
                    return i;
            }
            string colorString = kingColor == Piece.White ? "White" : "Black";
            throw new Exception("This board hasn't a king with " + colorString + " color");
        }
        private bool existsAttackInLocal(int local, byte colorAttack) {
            for(byte i = 0; i < 64; i++) {
                Piece piece = this.data[i];
                if(piece.IsPiece && piece.isSameColor(colorAttack)){
                    List<Move> moves = this.getPieceMoves(i);
                    foreach(Move move in moves) {
                        if(move.target == local) 
                            return true;
                        
                    }
                }
            }
            return false;
        }
    }

}
