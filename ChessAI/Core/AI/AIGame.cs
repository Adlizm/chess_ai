using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Keras.Models;

using Core.Chess;
using Core.AI.MCTS;
using Core.AI.NeuralNet;

namespace Core.AI {
	class AIGame {
		public class GameHistory {
			public List<Move> moves;
			public Game.GameState result;

			public GameHistory(List<Move> moves, Game.GameState result) {
				this.moves = moves;
				this.result = result;
			}
		
		}

		private DecisionConfig DecisionConfig;

		public AIGame(DecisionConfig decisionConfig) {
			this.DecisionConfig = decisionConfig;
		}

		public GameHistory GetGameBetweenAIs(BaseModel model) {
			var config = this.DecisionConfig;
			var game = Game.CreateInitialPosition();
			var moves = new List<Move>();

			while(game.CurrentState == Game.GameState.Playing) {
				if(game.Rounds == 15)
					config.Policy = DecisionConfig.DecisionPolicy.Deterministc;

				var (move, _) = MCTS.MCTS.Decision(config, game, model);

				if(game.TryMakeMove(move)) {
					moves.Add(move);
					Console.WriteLine("Move: " + move.ToString());
				}

				if(game.Rounds == 60) {
					return new GameHistory(moves, Game.GameState.Draw);
				}
			}

			return new GameHistory(moves, game.CurrentState);
		}
	}
	
}
