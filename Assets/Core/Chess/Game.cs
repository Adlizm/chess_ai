using UnityEngine;

using System;
using System.Collections.Generic;

namespace Core.Chess {
    public struct GameConfig {
        public byte TimeOf;
        public uint Rooks;
        public uint LastCaptureOrPawnMove;
        public uint Rounds;
        public EnPassant EnPassant;

        public bool GetRook(int index) {
            return (this.Rooks & (1u << index)) != 0;
        }
        public void SetRook(int index, bool isValid) {
            if(isValid)
                this.Rooks |= (1u << index);
            else
                this.Rooks &= ~(1u << index);
        }
    }

    public class Game {
        public enum GameState {
            Draw, WhiteWin, BlackWin, Playing
        }

        // String de configura��o segue a nota��o de Forsyth-Edwards
        public const string DefaultPosition = "tnbqkbnt/pppppppp/8/8/8/8/PPPPPPPP/TNBQKBNT w kKqQ - 1 0";

        private Board currentPosition;
        private List<Board> positions;

        private GameConfig gameConfig;
        private GameState gameState; 

        public Game() {
            this.currentPosition = new Board();
            this.positions = new List<Board>();
            this.gameConfig = new GameConfig();
            this.gameState = GameState.Playing;
        }
        public Game Copy() {
            Game copy = new Game();
            copy.currentPosition = this.currentPosition.Copy();
            copy.gameConfig = this.gameConfig;
            copy.gameState = this.gameState;

            return copy;
        }

        static public Game CreateInitialPosition() {
            return CreateGame(DefaultPosition);
        }
        static public Game CreateGame(string displayGame) {
            Dictionary<char, byte> pieceTypeFromLetter = new Dictionary<char, byte>() {
                { 'p', Piece.Pawn }, { 'b', Piece.Bishop }, { 'n', Piece.Knight },
                { 't', Piece.Tower }, { 'q', Piece.Queen }, { 'k', Piece.King }
            };

            Game game = new Game();

            int nBars = 0;
            int offSet = 0;
            int index = 0;
			foreach(char letter in displayGame) {
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
                        game.currentPosition[nBars * 8 + offSet] = new Piece((byte) (colorPiece | pieceType) );
                        offSet++;
                    }
                }
                if(offSet > 8) {
                    throw new FormatException("String is incompatible with the columns on a chessboard");
                }
            }
            String configBoard = displayGame.Substring(index);
            String[] configs = configBoard.Split(' ');

            if(configs.Length != 5)
                throw new FormatException("String not contain five configs params");

            if(!configs[0].Equals("w") && configs[0].Equals("b"))
                throw new FormatException("String does not contain a valid character to indicate a turn");

            game.gameConfig.TimeOf = configs[0].Equals("w") ? Piece.White : Piece.Black;

            if(configs[1].Length > 4 || (configs[1].Length == 1 && configs[1].Equals("-")) )
                throw new FormatException("String Format not contain a valid params to indicate the Rooks moviments");

            game.gameConfig.Rooks = 0b00000000;
            foreach(char letter in configs[1]) {
                switch(letter) {
                    case 'K':
                        game.gameConfig.SetRook(0, true); 
                        break;
                    case 'Q':
                        game.gameConfig.SetRook(1, true); 
                        break;
                    case 'k':
                        game.gameConfig.SetRook(2, true); 
                        break;
                    case 'q':
                        game.gameConfig.SetRook(3, true); 
                        break;
                    case '-':
                        break;
                    default:
                        throw new FormatException("String Format not contain a valid params to indicate the Rooks moviments");
                }
            }

            if(configs[2].Length != 1 || (configs[2][0] != '-' && (configs[2][0] < '0' || configs[2][0] > '8') ))
                throw new FormatException("String Format not contain a valid params to indicate the EnPassant moviment");

            game.gameConfig.EnPassant.valid = configs[2][0] != '-';
            game.gameConfig.EnPassant.column = (byte) (configs[2][0] - '0');

            if( (game.gameConfig.LastCaptureOrPawnMove = uint.Parse(configs[3])) < 0 )
                throw new FormatException("String Format not contain a valid params to indicate the last capture");

            if( (game.gameConfig.Rounds = uint.Parse(configs[4])) < 0 )
                throw new FormatException("String Format not contain a valid params to indicate the current round");

            game.gameState = game.getGameState();
            game.positions.Add(game.currentPosition.Copy());
            return game;
        }

        public byte TimeOf {
            get => this.gameConfig.TimeOf;
        }
        public uint LastCaptureOrPawnMove {
            get => this.gameConfig.LastCaptureOrPawnMove;
        }
        public uint Rounds {
            get => this.gameConfig.Rounds;
        }
        public int TotalPositions {
            get => this.positions.Count;
        }
        public Piece this[int index] {
            get =>this.currentPosition[index]; 
        }
        public GameState CurrentState {
            get => this.gameState;
            set { this.gameState = value; }
        }
        public Board GetPosition(int index) {
            index = Math.Max(0, Math.Min(this.positions.Count - 1, index));
            return this.positions[index].Copy();
        }
        public bool KingInCheck {
            get {
                byte localKing = MoveGenerator.GetKingLocal(this.currentPosition, this.gameConfig.TimeOf);
                byte enemyColor = this.gameConfig.TimeOf == Piece.White ? Piece.Black : Piece.White;
                bool ok = MoveGenerator.ExistsAttackInLocal(this.currentPosition, this.gameConfig, localKing, enemyColor);
                Debug.Log(localKing + " " + enemyColor + " " + ok);
                return ok;
            }
        }
        
        public List<Move> GetAllValidMoves() {
            List<Move> moves = new List<Move>();

            for(byte i = 0; i < 64; i++) {
                List<Move> pieceMoves = MoveGenerator.GetPieceMoves(this.currentPosition, this.gameConfig, i);
                moves.AddRange(pieceMoves);
            }
            return MoveGenerator.FilterInvalidMoves(this.currentPosition, this.gameConfig, moves);
        }
        public List<Move> GetValidPieceMoves(byte localPiece) {
            List<Move> moves = MoveGenerator.GetPieceMoves(this.currentPosition, this.gameConfig, localPiece);
            return MoveGenerator.FilterInvalidMoves(this.currentPosition, this.gameConfig, moves);
        }
        public bool TryMakeMove(Move mymove) {
            Piece piece = this.currentPosition[mymove.selected];
            if(!piece.isSameColor(this.gameConfig.TimeOf)) {
                Console.WriteLine("Treid move a enemy piece");
                return false;
            }

            List<Move> pieceMoves = this.GetValidPieceMoves(mymove.selected);
            foreach(Move move in pieceMoves) {
                if(mymove == move) {
                    this.gameConfig = MoveGenerator.MakeMove(this.currentPosition, this.gameConfig, mymove);
                    this.positions.Add(this.currentPosition.Copy());
                    this.gameState = this.getGameState(); 
                    
                    return true;
                }
            }
            return false;
        }

        private GameState getGameState() {
            if(this.gameConfig.LastCaptureOrPawnMove >= 50)
                return GameState.Draw;

            byte enemyColor = this.gameConfig.TimeOf == Piece.White ? Piece.Black : Piece.White;
            if(this.GetAllValidMoves().Count == 0) {
                if(this.KingInCheck)
                    return enemyColor == Piece.White ? GameState.WhiteWin : GameState.BlackWin;

                return GameState.Draw;
            }

            int[] countBishopOrKinght = new int[2]; //[Black, White]
            for(int i = 0; i < 63; i++) {
                Piece piece = this.currentPosition[i];
                byte type = piece.Type;

                switch(type) {
                    case Piece.None:
                    case Piece.King:
                        break;

                    case Piece.Pawn:
                    case Piece.Tower:
                    case Piece.Queen:
                        return GameState.Playing;

                    case Piece.Knight:
                    case Piece.Bishop:
                        int index = piece.IsWhite ? 1 : 0;
                        countBishopOrKinght[index]++;
                        break;
                }
            }
            return countBishopOrKinght[0] <= 1 && countBishopOrKinght[1] <= 1 ? GameState.Draw : GameState.Playing;
        }
    }

}
