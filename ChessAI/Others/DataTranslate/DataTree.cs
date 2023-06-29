using System;
using System.IO;
using System.Collections.Generic;

using Core.Chess;
using Core.AI.NeuralNet;

namespace Others.DataTranslate {
	
	public struct DataNode {
		static public readonly int SIZEOF = 10;

		public uint N;
		public float W;
		public Move move;

		public DataNode(uint N, float W, Move move) {
			this.N = N;
			this.W = W;
			this.move = move;
		}

		public float Q {
			get => W / N;
		}

		public void ToBytes(byte[] bytes) {
			BitConverter.TryWriteBytes(bytes, N);
			BitConverter.TryWriteBytes(bytes.AsSpan(4), W);
			BitConverter.TryWriteBytes(bytes.AsSpan(8), move.ToShort());
		}
		static public DataNode FromBytes(byte[] bytes) {
			uint N = BitConverter.ToUInt32(bytes, 0);
			float W = BitConverter.ToSingle(bytes, 4);
			ushort move = BitConverter.ToUInt16(bytes, 8);

			return new DataNode(N, W, Move.FromShort(move));
		}
	}

	class DataTree {

		private DataTree parent;
		private DataNode node;
		private List<DataTree> children;

		public DataTree() {
			parent = null;
			node = new DataNode {};
			children = new List<DataTree>();
		}


		public int InsertBook(string dirpath) {
			int count = 0;
			try {
				foreach(var filepath in Directory.GetFiles(dirpath, "*.pgn"))
					count += InsertFile(filepath);
				Console.WriteLine("Total pgns files: " + count);
					
			} catch(Exception e) {
				Console.WriteLine("Error: " + e.Message);
			}
			return count;
		}
		public int InsertFile(string filepath) {
			var count = 0;
			try {
				Console.WriteLine("\nReading file: {0} ", filepath);
				foreach(var pgn in PGNImport.PGNsFromFile(filepath)) {
					Console.Write("\rPGNs read: {0}", count);
					count += InsertPGN(pgn);
				}
				Console.Write("\rPGNs read: {0}", count);
			} catch(Exception e) {
				Console.WriteLine("Error: " + e.Message);
			}
			return count;
		}
		public int InsertPGN(string pgn) {
			try {
				var (moves, result) = PGNLoader.Load(pgn);
				InsertGame(moves, result);
				return 1;

			}catch(Exception e) {
				Console.WriteLine("\n" + e.Message);
				Console.WriteLine("PGN cannot be converted");
				Console.WriteLine(pgn + "\n");
			}
			return 0;
		}
		public int InsertGame(List<Move> moves, Game.GameState result) {
			var resultValue = OutputFormat.ExpectedValue(result);

			DataTree currentNode = this;
			currentNode.node.N++;
			currentNode.node.W += resultValue;
			foreach(var move in moves) {
				var next = currentNode.children.Find(child => child.node.move == move);
				if(next == null) {
					next = new DataTree();
					next.node.move = move;
					next.parent = currentNode;
					currentNode.children.Add(next);
				}
				currentNode = next;
				currentNode.node.N++;
				currentNode.node.W += resultValue;
			}
			return 0;
		}

		public uint N {
			get => this.node.N;
		}
		public float W {
			get => this.node.W;
		}
		public float Q {
			get => this.node.Q;
		}

		public int TotalTriples() {
			return TotalTriples(new TripleCollectStrategys.AnyStrategy());
		}
		public int TotalTriples(TripleCollectStrategys.TripleCollectStrategy strategy) {
			strategy.Reset();

			var total = 0;
			var currentTree = this;

			Stack<int> stackCurrentChildIndex = new Stack<int>();
			stackCurrentChildIndex.Push(0);

			while(currentTree != null) {
				var currentChildIndex = stackCurrentChildIndex.Peek();

				if(currentChildIndex == currentTree.children.Count || !strategy.CanCollectMore(currentTree)) {  // Back
					currentTree = currentTree.parent;
					stackCurrentChildIndex.Pop();
					strategy.Back();
					continue;
				}

				if(currentTree.children.Count != 0) {
					if(currentChildIndex == 0 && strategy.CanCollectThis(currentTree)) 
						total++;

					stackCurrentChildIndex.Pop();
					stackCurrentChildIndex.Push(currentChildIndex + 1); // Incress index to next child;


					stackCurrentChildIndex.Push(0);
					currentTree = currentTree.children[currentChildIndex];
					strategy.Deep();
				}
			}
			return total;
		}

		public IEnumerable<Triple> GetTriplesStack() {
			return GetTriplesStack(new TripleCollectStrategys.AnyStrategy());
		}
		public IEnumerable<Triple> GetTriplesStack(TripleCollectStrategys.TripleCollectStrategy strategy) {
			strategy.Reset();

			Stack<Game> stackCurrentGame = new Stack<Game>();
			Stack<int> stackCurrentChildIndex = new Stack<int>();

			var currentTree = this;
			stackCurrentGame.Push(Game.CreateInitialPosition());
			stackCurrentChildIndex.Push(0);

			while(currentTree != null) {
				var currentGame = stackCurrentGame.Peek();
				var currentChildIndex = stackCurrentChildIndex.Peek();

				if(currentChildIndex == currentTree.children.Count || !strategy.CanCollectMore(currentTree)) {  // Back
					currentTree = currentTree.parent;
					stackCurrentChildIndex.Pop();
					stackCurrentGame.Pop();
					strategy.Back();
					continue;
				}

				if(currentTree.children.Count != 0) {

					if(currentChildIndex == 0 && strategy.CanCollectThis(currentTree)) {
						yield return new Triple(
							currentGame,
							currentTree.Q,
							currentTree.children.ConvertAll(child => (child.node.move.ToShort(), child.N)).ToArray()
						);
					}


					var copy = currentGame.Copy();
					var currentChild = currentTree.children[currentChildIndex];

					stackCurrentChildIndex.Pop();
					stackCurrentChildIndex.Push(currentChildIndex + 1); // Incress index to next child;

					if(copy.TryMakeMove(currentChild.node.move)) { // Deep
						stackCurrentGame.Push(copy);
						stackCurrentChildIndex.Push(0);
						currentTree = currentChild;
						strategy.Deep();
					}
				}
			}
		}


		public IEnumerable<Triple> GetTriplesRecursion() {
			return DeepGetTriples(this, Game.CreateInitialPosition(), new TripleCollectStrategys.AnyStrategy());
		}
		public  IEnumerable<Triple> GetTriplesRecursion(TripleCollectStrategys.TripleCollectStrategy strategy) {
			strategy.Reset();
			return DeepGetTriples(this, Game.CreateInitialPosition(), strategy);
		}
		private IEnumerable<Triple> DeepGetTriples(DataTree tree, Game game, TripleCollectStrategys.TripleCollectStrategy strategy) {
			if(tree.children.Count != 0 ) {
				if(strategy.CanCollectThis(tree)) {
					yield return new Triple(
						game,
						tree.Q,
						tree.children.ConvertAll(child => (child.node.move.ToShort(), child.N)).ToArray()
					);
				}

				if(strategy.CanCollectMore(tree)) {
					strategy.Deep();

					foreach(var child in tree.children) {
						var copy = game.Copy();
						if(copy.TryMakeMove(child.node.move)) {
							foreach(var triple in DeepGetTriples(child, copy, strategy))
								yield return triple;

						}
					}
				}
			}
			strategy.Back();
		}


		/* Função responsavel por gerar um arquivo binário a partir da arvoré atual gerada.
		 * TREE = (DATA_NODE, N_CHILDS, [TREE])
		 *		DATA_NODE = (N, W, MOVE)
		 */
		static public DataTree Load(string filepath) {
			var tree = new DataTree();
			FileStream fs = File.OpenRead(filepath);

			DeepLoad(tree, fs, new byte[DataNode.SIZEOF]);

			fs.Close();
			return tree;
		}
		static private void DeepLoad(DataTree tree, FileStream fs, byte[] buffer) {
			if(fs.Read(buffer, 0, DataNode.SIZEOF) != DataNode.SIZEOF)
				throw new Exception("File does not represent a Data Tree");
			tree.node = DataNode.FromBytes(buffer);

			if(fs.Read(buffer, 0, sizeof(int)) != sizeof(int))
				throw new Exception("File does not represent a Data Tree");

			var nChlids = BitConverter.ToInt32(buffer);
			for(int i = 0; i < nChlids; i++) {
				var child = new DataTree();
				child.parent = tree;

				DeepLoad(child, fs, buffer);
				tree.children.Add(child);
			}
		}


		static public void Save(DataTree tree, string filepath) {
			if(tree == null)
				return;
			if(File.Exists(filepath))
				throw new Exception("Cannot save a data tree file in a exist file");

			FileStream fs = File.Create(filepath, 4096, FileOptions.RandomAccess);

			DeepSave(tree, fs, new byte[DataNode.SIZEOF]);
			fs.Close();
		}
		static private void DeepSave(DataTree tree, FileStream fs, byte[] buffer) {
			if(tree == null)
				return;

			tree.node.ToBytes(buffer);
			fs.Write(buffer, 0, DataNode.SIZEOF);

			BitConverter.TryWriteBytes(buffer, tree.children.Count);
			fs.Write(buffer, 0, sizeof(int));

			foreach(var child in tree.children)
				DeepSave(child, fs, buffer);
		}


		static public bool Equals(DataTree a, DataTree b) {
			if( a.node.N == b.node.N && 
				a.node.W == b.node.W && 
				a.node.move == b.node.move && 
				a.children.Count == b.children.Count ) {

				for(int i = 0; i < a.children.Count; i++) 
					if(!Equals(a.children[i], b.children[i]))
						return false;

				return true;
			}
			return false;
		}

		public DataCollector ToDataCollector(string filepath, int collectionSize = 4096) {
			return ToDataCollector(filepath, new TripleCollectStrategys.AnyStrategy(), collectionSize);
		}
		public DataCollector ToDataCollector(string filepath, TripleCollectStrategys.TripleCollectStrategy strategy, int collectionSize = 4096) {
			DataCollector collector = DataCollector.Create(filepath);

			int currentCollectionSize = 0;
			foreach(var triple in GetTriplesStack(strategy)) {
				if(currentCollectionSize == collectionSize) {
					collector.InitCollection();
					currentCollectionSize = 0;
				}

				collector.AddTriple(triple);
				currentCollectionSize++;
			}

			collector.Save();
			return collector;
		}
	}
}
