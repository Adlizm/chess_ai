using System;
using System.Collections.Generic;

using Numpy;
using Keras.Models;

using Core.Chess;
using Core.AI.NeuralNet;
using Core.AI.MCTS;
using Others.DataTranslate;

namespace Core.AI.Train {
	public class Training {
		public struct TrainConfig {
			public readonly string DATA_FILE;
			public readonly string MODEL_FILE;

			public readonly int EPOCHS;
			public readonly int DATA_COLLECT_GAMES;

			public readonly bool HAS_VALIDATION_GAMES;
			public readonly int TRAIN_VALIDATION_GAMES;

			public readonly int RECENT_GAMES;
			public readonly int RAFFLED_TRIPLES;

			public readonly int THREADS;

			public DecisionConfig MCTS;
		}

		private TrainConfig config;
		private BaseModel model;
		private DataCollector data;

		public Training(TrainConfig config) {
			this.config = config;
			this.data = DataCollector.Load(config.DATA_FILE);
			this.model = ModelBuilder.Load(config.MODEL_FILE);
		}

		public void Run() {
			for(int i = 0; i < config.EPOCHS; i++) {
				Collect();
				Train();
			}
			data.Save();
			data.Close();

			ModelBuilder.Save(config.MODEL_FILE, model);
		}
		public void Collect() {
			for(int i = 0; i < config.DATA_COLLECT_GAMES; i++) {
				data.InitCollection();

				var triples = CollectGame(model);
				foreach(Triple triple in triples)
					data.AddTriple(triple);

				data.EndCollection();
			}

		}
		public List<Triple> CollectGame(BaseModel model) {
			config.MCTS.Policy = DecisionConfig.DecisionPolicy.Stochastic;

			Game game = Game.CreateInitialPosition();
			List<Triple> triples = new List<Triple>();

			while(game.CurrentState == Game.GameState.Playing) {
				if(game.Rounds == 15)
					config.MCTS.Policy = DecisionConfig.DecisionPolicy.Deterministc;

				var (move, N) = MCTS.MCTS.Decision(config.MCTS, game, model);

				Triple triple = new Triple(game, N);
				triples.Add(triple);

				game.TryMakeMove(move);
			}

			byte R = 0;
			switch(game.CurrentState) {
				case Game.GameState.WhiteWin: R = 1; break;
				case Game.GameState.BlackWin: R = 2; break;
			}
			foreach(Triple triple in triples)
				triple.R = R;

			return triples;
		}

		public void Train() {
			Random r = new Random();
			int init = Math.Max(0, data.TotalCollections - config.RECENT_GAMES);
			int end = data.TotalCollections;

			int totalTriples = data.TriplesBetweenCollections(init, end);
			int[] indexs = new int[config.RAFFLED_TRIPLES];
			for(int i = 0; i < indexs.Length; i++)
				indexs[i] = r.Next(totalTriples);

			//Raffled triples
			List<Triple> triples = data.GetTriples(init, end, indexs);

			//Create Clone
			BaseModel clone = ModelBuilder.Clone(model);

			NDarray inputs = InputFormat.InputsFromTriples(triples);
			NDarray expected = OutputFormat.ExpectedFromTriples(triples);

			clone.Fit(inputs, expected);

			if(!config.HAS_VALIDATION_GAMES) {
				model = clone;
				return;
			}

			int cloneResult = 0;
			bool cloneWhite = true;
			for(int i = 0; i < config.TRAIN_VALIDATION_GAMES; i++) {
				cloneResult += ValidationGame(model, clone, cloneWhite);
				cloneWhite = !cloneWhite;
			}

			if(cloneResult > 0)
				model = clone;
		}
		public int ValidationGame(BaseModel original, BaseModel clone, bool cloneWhite) { //Return clone result (-1, 0, 1)
			var white = cloneWhite ? clone : original;
			var black = cloneWhite ? original : clone;

			config.MCTS.Policy = DecisionConfig.DecisionPolicy.Stochastic;

			var game = Game.CreateInitialPosition();
			while(game.CurrentState == Game.GameState.Playing) {
				var player = game.TimeOf == Piece.White ? white : black;
				if(game.Rounds == 15)
					config.MCTS.Policy = DecisionConfig.DecisionPolicy.Deterministc;

				var (move, _) = MCTS.MCTS.Decision(config.MCTS, game, player);
				game.TryMakeMove(move);
			}

			switch(game.CurrentState) {
				case Game.GameState.WhiteWin:
					return cloneWhite ? 1 : -1;
				case Game.GameState.BlackWin:
					return cloneWhite ? -1 : 1;
			}
			return 0;
		}
		
	}
}
