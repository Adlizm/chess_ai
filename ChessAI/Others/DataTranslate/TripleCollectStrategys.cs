using System;

namespace Others.DataTranslate {
	class TripleCollectStrategys {
		public interface TripleCollectStrategy {
			public bool CanCollectThis(DataTree tree);
			public bool CanCollectMore(DataTree tree);
			public void Reset();
			public void Deep();
			public void Back();
		}

		public class AnyStrategy : TripleCollectStrategy {
			public AnyStrategy() {}

			public void Back() {}

			public bool CanCollectMore(DataTree tree) {
				return true;
			}

			public bool CanCollectThis(DataTree tree) {
				return true;
			}

			public void Deep() {}

			public void Reset() {}
		}
		public class DeepLimitStrategy : TripleCollectStrategy {
			private int MaxDeep { get; }
			private int currentDeep;

			public DeepLimitStrategy(int maxDeep) {
				this.MaxDeep = maxDeep;
			}

			public void Back() {
				this.currentDeep--;
			}

			public bool CanCollectMore(DataTree tree) {
				return this.currentDeep < this.MaxDeep;
			}

			public bool CanCollectThis(DataTree tree) {
				return this.currentDeep < this.MaxDeep;
			}

			public void Deep() {
				this.currentDeep++;
			}

			public void Reset() {
				this.currentDeep = 0;
			}
		}

		public class MinVisitsStrategy : TripleCollectStrategy {
			private int MinVisits {
				get;
			}

			public MinVisitsStrategy(int MinVisits) {
				this.MinVisits = MinVisits;
			}

			public void Back() {}

			public bool CanCollectMore(DataTree tree) {
				return tree.N >= this.MinVisits;
			}

			public bool CanCollectThis(DataTree tree) {
				return tree.N >= this.MinVisits;
			}

			public void Deep() {}
			public void Reset() {}
		}

		public class RandomStrategy : TripleCollectStrategy {
			private double Probability { get; }
			private int Seed { get; }
			private Random random;

			public RandomStrategy(double proobability, int seed = 1) {
				Probability = proobability;
				Seed = seed;
			}
			public void Back() {}

			public bool CanCollectMore(DataTree tree) {
				return true;
			}

			public bool CanCollectThis(DataTree tree) {
				return random.NextDouble() < Probability;
			}

			public void Deep() {}

			public void Reset() {
				random = new Random(this.Seed);
			}
		}

		public class HybridStrategy : TripleCollectStrategy {
			private int MaxDeep {
				get;
			}
			private int MinVisits {
				get;
			}
			private double Probability {
				get;
			}
			private int Seed {
				get;
			}
			
			
			private Random random;
			private int currentDeep;

			public HybridStrategy(int maxDeep, int minVisits, double probability, int seed = 1) {
				MaxDeep = maxDeep;
				MinVisits = minVisits;
				Probability = probability;
			}

			public void Back() {
				currentDeep--;
			}

			public bool CanCollectMore(DataTree tree) {
				return true;
			}

			public bool CanCollectThis(DataTree tree) {
				return tree.N >= MinVisits || currentDeep < this.MaxDeep || random.NextDouble() < Probability;
			}

			public void Deep() {
				currentDeep++;
			}

			public void Reset() {
				random = new Random(Seed);
				currentDeep = 0;
			}
		}
	}
}
