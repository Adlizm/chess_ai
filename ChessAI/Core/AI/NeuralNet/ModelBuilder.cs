using System.Collections.Generic;
using Keras;
using Keras.Models;
using Keras.Layers;

namespace Core.AI.NeuralNet {
	class ModelBuilder {
		private Shape InputShape;
		private int ResidualBlocks { get; set; }


		private static Conv2D Convolution(int filters = 32, int kernelSize = 3, int strides = 1, string padding = "same", bool useBias = true) {
			var Kernel = new System.Tuple<int, int>(kernelSize, kernelSize);
			var Strides = new System.Tuple<int, int>(strides, strides);

			return new Conv2D(filters, Kernel, strides: Strides, padding: padding , use_bias: useBias);
		}
		private static Dense Dense(int units, bool useBias = true, string activation = "") {
			return new Dense(units, use_bias: useBias, activation: activation);
		}


		public ModelBuilder() {
			InputShape = new Shape(InputFormat.TOTAL_8X8_BLOCKS, 8, 8);
			ResidualBlocks = 8;
		}

		private BaseLayer ResidualBlock(BaseLayer input) {
			var residual = Convolution().Set(input);
			residual = new BatchNormalization().Set(residual);
			residual = new Activation("relu").Set(residual);

			residual = Convolution().Set(input);
			residual = new BatchNormalization().Set(residual);

			residual = new Add(residual, input);
			residual = new Activation("relu").Set(residual);
			return residual;
		}

		private BaseLayer PolicyHead(BaseLayer input) {
			var policy = Convolution(2, 1).Set(input);
			policy = new BatchNormalization().Set(policy);
			policy = new Flatten().Set(policy);
			policy = Dense(OutputFormat.TOTAL_MOVES, activation: "sigmoid").Set(policy);
			return policy;
		}

		private BaseLayer ValueHead(BaseLayer input) {
			var value = Convolution(1, 1).Set(input);
			value = new BatchNormalization().Set(value);
			value = new Flatten().Set(value);
			value = Dense(256, activation: "relu").Set(value);
			value = Dense(1, activation: "tanh").Set(value);
			return value;
		}

		public Model Build() {
			var inputs = new Input(shape: InputShape);

			var body = Convolution().Set(inputs);

			for(int i = 0; i < ResidualBlocks; i++)
				body = ResidualBlock(body);

			var policy = PolicyHead(body);
			var value = ValueHead(body);

			var model = new Model(new BaseLayer[] { inputs }, new BaseLayer[] { policy, value });
			model.Compile("sgd", new string[] { "mean_squared_error", "mean_squared_error" });
			
			return model;
		}

		public static BaseModel Load(string filepath) {
			BaseModel model = BaseModel.LoadModel(filepath);
			return model;
		}
		public static void Save(string filepath, BaseModel model) {
			model.Save(filepath);
		}
		public static BaseModel Clone(BaseModel model) {
			var clone = new ModelBuilder().Build();
			var weigths = new List<Numpy.NDarray>();

			model.GetWeights().ForEach(arr => weigths.Add(arr.copy()));

			clone.SetWeights(weigths);

			return clone;
		}
	}

}
