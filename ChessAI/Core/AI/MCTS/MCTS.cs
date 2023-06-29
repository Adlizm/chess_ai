using System;
using System.Collections.Generic;

using Keras.Models;

using Core.Chess;
using Core.AI.NeuralNet;

namespace Core.AI.MCTS {
	public struct DecisionConfig {
		public enum DecisionPolicy {
			Deterministc, Stochastic
		}

		public DecisionPolicy Policy;
		public float Temparature;
		public int Episodes;

		public DecisionConfig(DecisionPolicy policy, float temparature = 1.0f, int episodes = 600) {
			Policy = policy;
			Temparature = temparature;
			Episodes = episodes;
		}
	}

	class MCTS {

		public static class DecisionPolicys {
			public static Random random = new Random();
			public static Move Deterministc(Game game, MCTree[] childs) {
				uint index = 0, max = 0;
				for(uint i = 0; i < childs.Length; i++) {
					if(childs[i].N > max) {
						max = childs[i].N;
						index = i;
					}
				}
				return childs[index].Move;
			}

			public static Move Stochastic(Game game, MCTree[] childs, double T = 1.0) {
				double sum = 0;
				double[] ps = new double[childs.Length];
				for(int i = 0; i < ps.Length; i++) {
					double p = Math.Pow(childs[i].N, 1.0 / T);
					ps[i] = p;
					sum += p;
				}

				double random = DecisionPolicys.random.NextDouble() * sum;
				for(int i = 0; i < ps.Length; i++) {
					random -= ps[i];
					if(random < 0)
						return childs[i].Move;
				}
				return childs[childs.Length - 1].Move;
			}

		}
		private static class SelectionPolicy {
			public static float GetQU(MCTree node) {
				float C = MathF.Log2((1 + node.GetParent.N + 19652.0f) / 19652.0f) + 1.25f;
				float Q = node.Q;
				float U = C * node.P * (MathF.Sqrt(node.GetParent.N) / (node.N + 1));

				return Q + U;
			}
		}


		public static (Move, uint[]) Decision(DecisionConfig config, Game game, BaseModel model) {
			if(game.CurrentState != Game.GameState.Playing)
				throw new Exception("Unable to make a decision in a finshed game");
			
			MCTree T = new MCTree(new Node(move: 0));
			int episode = 0;

			while(episode < config.Episodes) {
				var gameSimulated = game.Copy();
				var nodeSelected = Selection(gameSimulated, T);
				var evaluatedValue = EvaluationExpansion(gameSimulated, nodeSelected, T, model);
				BackPropagation(evaluatedValue, nodeSelected, gameSimulated.TimeOf);

				episode++;
			}

			Move a = config.Policy == DecisionConfig.DecisionPolicy.Deterministc ? 
				DecisionPolicys.Deterministc(game, T.Childs) :
				DecisionPolicys.Stochastic(game, T.Childs, config.Temparature);

			return (a, T.GetChildsVisits());
		}

		private static MCTree Selection(Game simulatedGame, MCTree T) {
			MCTree nodeSelected = T;

			while(simulatedGame.CurrentState == Game.GameState.Playing && nodeSelected.N > 0) {
				float maxQU = float.MinValue;

				MCTree nextSelection = null;
				foreach(MCTree child in nodeSelected.Childs) {
					float QU = SelectionPolicy.GetQU(child);
					if(QU > maxQU) {
						maxQU = QU;
						nextSelection = child;
					}
				}
				if(nextSelection == null)
					throw new Exception("Selected a non-terminal node that was not expanded");

				nodeSelected = nextSelection;
				simulatedGame.TryMakeMove(nodeSelected.Move);
			}

			return nodeSelected;
		}

		private static float EvaluationExpansion(Game simulatedGame, MCTree nodeSelected, MCTree T, BaseModel model) {
			if(simulatedGame.CurrentState != Game.GameState.Playing) 
				return OutputFormat.ExpectedValue(simulatedGame);

			if(nodeSelected.N >= 1) 
				return nodeSelected.Q;
			
			var input = InputFormat.GameToInput(simulatedGame);
			var output = model.PredictMultipleOutputs(input);
			var (p, v) = OutputFormat.OutputValues(output);

			List<Move> moves = simulatedGame.GetAllValidMoves();
			Node[] nodes = new Node[moves.Count];
			for(int i = 0; i < moves.Count; i++) {
				int index = OutputFormat.MoveToIndex(moves[i]);

				nodes[i] = new Node(move: moves[i].ToShort(), p: p[index]);
			}
			nodeSelected.AppendChilds(nodes);
			return v;
		}

		private static void BackPropagation(float q, MCTree nodeSelected, byte colorTime) {
			do {
				//Console.WriteLine("evaluated: {0}", q);
				//Console.Write("(Move: {0}, N: {1}, W: {2}, Q: {3}, P: {4}) -> ", nodeSelected.Move, nodeSelected.N, nodeSelected.W, nodeSelected.Q, nodeSelected.P);

				nodeSelected.N = nodeSelected.N + 1;
				nodeSelected.W = nodeSelected.W +  (colorTime == Piece.White ? -q : q);
				nodeSelected.Q = nodeSelected.W / nodeSelected.N;

				//Console.Write("(Move: {0}, N: {1}, W: {2}, Q: {3}, P: {4})\n", nodeSelected.Move, nodeSelected.N, nodeSelected.W, nodeSelected.Q, nodeSelected.P);

				colorTime = colorTime == Piece.White ? Piece.Black : Piece.White;
				nodeSelected = nodeSelected.GetParent;
			} while(nodeSelected != null);
		}
	}
}
