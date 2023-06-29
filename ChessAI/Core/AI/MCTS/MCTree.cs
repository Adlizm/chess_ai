using Core.Chess;

namespace Core.AI.MCTS {
	public struct Node {
		public uint N;      //Number of visits on node
		public float P;     //Node Priority
		public float Q;     //Node Evaluation
		public float W;

		public readonly ushort move; //Move performed to reach that node

		public Node(ushort move, uint n = 0, float p = 0, float q = 0, float w = 0) {
			this.N = n;
			this.P = p;
			this.Q = q;
			this.W = w;

			this.move = move;
		}
	}

	public class MCTree {
		private MCTree parent;
		private Node data;
		private MCTree[] childs;

		public MCTree(Node root, MCTree parent = null) {
			this.parent = parent;
			this.childs = null;
			this.data = root;
		}

		public uint N {
			get => this.data.N;
			set => this.data.N = value;
		}
		public float P {
			get => this.data.P;
			set => this.data.P = value;
		}
		public float Q {
			get => this.data.Q;
			set => this.data.Q = value;
		}
		public float W {
			get => this.data.W;
			set => this.data.W = value;
		}
		public Move Move => Move.FromShort(this.data.move);


		public bool HasParent {
			get => this.parent != null;
		}
		public MCTree GetParent {
			get => this.parent;
		}

		public MCTree[] Childs {
			get => this.childs;
		}
		public void AppendChilds(Node[] nodes) {
			this.childs = new MCTree[nodes.Length];
			for(int i = 0; i < nodes.Length; i++) 
				this.childs[i] = new MCTree(nodes[i], parent: this);
		}
		
		public uint[] GetChildsVisits() {
			if(this.childs == null)
				return null;

			uint[] Ns = new uint[this.childs.Length];
			for(int i = 0; i < this.childs.Length; i++) 
				Ns[i] = this.childs[i].data.N;

			return Ns;
		}
	}
}

