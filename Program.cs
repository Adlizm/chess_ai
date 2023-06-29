using System;
using System.Diagnostics;
using System.Collections.Generic;

using Numpy;
using Keras.Models;

using Core.AI.MCTS;
using Core.AI.NeuralNet;
using Core.AI;
using Core.Chess;

using Others;
using Others.DataTranslate;


namespace chess_ai {
	class Program {
        public static void MakeDataTrain() {
            try {
                Console.WriteLine("Creating Data Tree by PGN directory");
                var tree = new DataTree();
                var total = tree.InsertBook(@"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Data\pgns\");
                Console.WriteLine("Total Triples = " + total);


                Console.WriteLine("Saving tree created");
                DataTree.Save(tree, @"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Data\datatree.dt");
                Console.WriteLine("Tree Saved");


                Console.WriteLine("Creating Data Collector by tree with selected strategy");
                var strategy = new TripleCollectStrategys.HybridStrategy(25, 4, 0.1);
                DataCollector collector = tree.ToDataCollector(@"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Data\datacolecttor.dc", strategy);
                collector.Save();
                Console.WriteLine("Data Collector created");

            } catch(Exception e) {
                Console.WriteLine("");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        public static void TrainStep(int step, DataCollector collector, BaseModel model) {
            try {
                Console.WriteLine("\n\nIniting Step {0} of Training", step);

                var Epochs = 10;
                var CollectionSize = 2048;
                var CollectionsTrainSize = 100;

                var CollectinoStart = step * CollectionsTrainSize;
                var CollectionEnd = Math.Min(collector.TotalCollections, CollectinoStart + CollectionsTrainSize);

                Console.WriteLine("\nLoading Data Train");
                var triples = collector.GetTriples(CollectinoStart, CollectionEnd);
                Console.WriteLine("Data Train Loaded");

                Console.WriteLine("\nCreating Data Input and Outputs");
                var inputs = InputFormat.InputsFromTriples(triples);
                var outputs = OutputFormat.ExpectedFromTriples(triples);
                Console.WriteLine("Input and Outputs Created");


                Console.WriteLine("\nIniting Fit");
                model.Fit(inputs, outputs, batch_size: CollectionSize, epochs: Epochs);
                Console.WriteLine("\nFit Complete");

                Console.WriteLine("\nSaving Model");
                model.Save(@"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Core\AI\NeuralNet\model" + step);
                Console.WriteLine("\nModel Saved");

                Console.ReadLine(); //Pause
            } catch(Exception e) {
                Console.WriteLine("");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        public static void Train() {
            try {
                Console.WriteLine("Loading Data Collector");
                DataCollector collector = DataCollector.Load(@"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Data\datacolecttor.dc");
                Console.WriteLine("Data Collector Loaded");

                Console.WriteLine("\nLoading Model");
                //var model = ModelBuilder.Load(@"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Core\AI\NeuralNet\model");
                var model = new ModelBuilder().Build();
                Console.WriteLine("Model Loaded");

                //Total Steps = 13
                TrainStep(0, collector, model); 
                TrainStep(1, collector, model);
                TrainStep(2, collector, model);
                TrainStep(3, collector, model);
                TrainStep(4, collector, model);
                TrainStep(5, collector, model);
                TrainStep(6, collector, model);
                TrainStep(7, collector, model);
                TrainStep(8, collector, model);
                TrainStep(9, collector, model);
                TrainStep(10, collector, model);
                TrainStep(11, collector, model);
                TrainStep(12, collector, model);
               
                /* -- OK -- 
                */
            } catch(Exception e) {
                Console.WriteLine("");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            Console.ReadLine(); //Pause
        }

        public static void Result(string filepath) {
            int gamesSaved = 0;
            var config = new DecisionConfig(DecisionConfig.DecisionPolicy.Stochastic, 1.0f, 400);
            var gamesGenerators = new AIGame(config);

            var totalGames = 5;
            var pgns = new string[totalGames];
            try {
                int seed = (filepath.GetHashCode() + DateTime.Now.Ticks.GetHashCode()).GetHashCode();
                MCTS.DecisionPolicys.random = new Random(seed);

                Console.WriteLine("Seed: " + seed);

                Console.WriteLine("\nLoading Model");
                var model = ModelBuilder.Load(@"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Core\AI\NeuralNet\model12");
                Console.WriteLine("Model Loaded");

                for(int i = 0; i < totalGames; i++) {
                    Console.WriteLine("Initing a game");
                    var gameHistory = gamesGenerators.GetGameBetweenAIs(model);
                    Console.WriteLine("Game Created");

                    Console.WriteLine("Creating PGN");
                    var pgn = PGNCreator.Create(gameHistory.moves.ToArray(), gameHistory.result);
                    pgns[i] = pgn;
                    Console.WriteLine("PGN Created");

                    Console.WriteLine("Saving PGN");
                    PGNExport.SavePGN(filepath, pgn);
                    Console.WriteLine("PGN Saved");

                    gamesSaved++;
                }
            } catch (Exception e) {
                Console.WriteLine("");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

            Console.WriteLine("Games Saved: " + gamesSaved);
            foreach(var pgn in pgns)
                Console.WriteLine(pgn + '\n');
                
            Console.ReadLine(); //Pause
        }

        public static void Main(string[] args) {
            Result(args[0]);
            //Result(@"C:\Users\PICHAU\source\repos\chess_ai\results.pgn");
        }

        public static void Test16() {
            try {
                Console.WriteLine("Creating Model!");
                var model = new ModelBuilder().Build();
                Console.WriteLine("Creating Clone!");
                var clone = ModelBuilder.Clone(model);

                var game = Game.CreateInitialPosition();
                NDarray inputs = InputFormat.GameToInput(game);

                NDarray[] outputModel = model.PredictMultipleOutputs(inputs);
                NDarray[] outputModelClone = clone.PredictMultipleOutputs(inputs);

                Console.WriteLine("\n\nComparing outputs with model created and clone of initial position");
                for(int i = 0; i < outputModel.Length; i++) {
                    var outM = outputModel[i];
                    var outL = outputModelClone[i];

                    if(!outM.equals(outL).all())
                        Console.WriteLine("Different!");
                }
                Console.WriteLine("Equals!");
            } catch(Exception e) {
                Console.WriteLine("");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
        public static void Test15() {
            try {
                Console.WriteLine("Creating Model!");
                var model = new ModelBuilder().Build();
                model.Save(@"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Core\AI\NeuralNet\model");
                Console.WriteLine("Created and Saved Model!");

                Console.WriteLine("\n\nLoading Model!");
                var modelLoaded = ModelBuilder.Load(@"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Core\AI\NeuralNet\model");
                Console.WriteLine("Model loaded!");

                var game = Game.CreateInitialPosition();
                NDarray inputs = InputFormat.GameToInput(game);
                
                NDarray[] outputModel = model.PredictMultipleOutputs(inputs);
                NDarray[] outputModelLoad = modelLoaded.PredictMultipleOutputs(inputs);

                Console.WriteLine("\n\nComparing outputs with model created and loaded of initial position");
                for(int i = 0; i < outputModel.Length; i++) {
                    var outM = outputModel[i];
                    var outL = outputModelLoad[i];

                    if(!outM.equals(outL).all())
                        Console.WriteLine("Different!");
                }
                Console.WriteLine("Equals!");
            } catch(Exception e){
                Console.WriteLine("");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
        public static void Test14() {
            try {
                Console.WriteLine("Loading Tree Data...");
                var tree = DataTree.Load(@"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Data\carlsen.dt");
                Console.WriteLine("Tree Loaded!");


                Console.WriteLine("\n");

                Console.WriteLine("Tree Triples Tota = {0}", tree.TotalTriples() );
                Console.WriteLine("Tree Triples (DeepLimit 25, MinVisists 4, Probability = 0.1) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(25, 4, 0.1)));

                int count = 0;
                foreach(var triple in tree.GetTriplesStack(new TripleCollectStrategys.HybridStrategy(25, 4, 0.1)))
                    count++;
                
                Console.WriteLine("Triples Loaded with Stretegy selected = {0}", count);

            } catch(Exception e) {
                Console.WriteLine("");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
        public static void Test13() {
            try {
                Console.WriteLine("Loading Tree Data...");
                var tree = DataTree.Load(@"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Data\datatree.dt");
                Console.WriteLine("Tree Loaded!");

                Console.WriteLine("\n");
                Console.WriteLine("Tree Triples All= {0}", tree.TotalTriples(new TripleCollectStrategys.AnyStrategy()));


                Console.WriteLine("\n\n");
                Console.WriteLine("Tree Triples MinVisists 2 = {0}", tree.TotalTriples(new TripleCollectStrategys.MinVisitsStrategy(2)));

                Console.WriteLine("\n\n");
                Console.WriteLine("Tree Triples DeepLimit 20 = {0}", tree.TotalTriples(new TripleCollectStrategys.DeepLimitStrategy(20)));
                Console.WriteLine("Tree Triples DeepLimit 30 = {0}", tree.TotalTriples(new TripleCollectStrategys.DeepLimitStrategy(30)));
                Console.WriteLine("Tree Triples DeepLimit 40 = {0}", tree.TotalTriples(new TripleCollectStrategys.DeepLimitStrategy(40)));
                Console.WriteLine("Tree Triples DeepLimit 50 = {0}", tree.TotalTriples(new TripleCollectStrategys.DeepLimitStrategy(50)));


                Console.WriteLine("\n\n");
                Console.WriteLine("Tree Triples (DeepLimit 20, MinVisists 2, Probability = 0) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(20, 2, 0.0)));
                Console.WriteLine("Tree Triples (DeepLimit 20, MinVisists 2, Probability = 0.05) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(20, 2, 0.05)));
                Console.WriteLine("Tree Triples (DeepLimit 20, MinVisists 2, Probability = 0.1) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(20, 2, 0.1)));
                Console.WriteLine("Tree Triples (DeepLimit 20, MinVisists 2, Probability = 0.15) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(20, 2, 0.15)));

                Console.WriteLine("\n\n");
                Console.WriteLine("Tree Triples (DeepLimit 30, MinVisists 2, Probability = 0) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(30, 2, 0.0)));
                Console.WriteLine("Tree Triples (DeepLimit 30, MinVisists 2, Probability = 0.05) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(30, 2, 0.05)));
                Console.WriteLine("Tree Triples (DeepLimit 30, MinVisists 2, Probability = 0.1) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(30, 2, 0.1)));
                Console.WriteLine("Tree Triples (DeepLimit 30, MinVisists 2, Probability = 0.15) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(30, 2, 0.15)));


                Console.WriteLine("\n\n");
                Console.WriteLine("Tree Triples (DeepLimit 30, MinVisists 3, Probability = 0) = {0}", 
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(30, 3, 0.0)) );
                Console.WriteLine("Tree Triples (DeepLimit 30, MinVisists 3, Probability = 0.05) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(30, 3, 0.05)));
                Console.WriteLine("Tree Triples (DeepLimit 30, MinVisists 3, Probability = 0.1) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(30, 3, 0.1)));
                Console.WriteLine("Tree Triples (DeepLimit 30, MinVisists 3, Probability = 0.15) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(30, 3, 0.15)));
                
                Console.WriteLine("\n\n");
                Console.WriteLine("Tree Triples (DeepLimit 20, MinVisists 3, Probability = 0) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(20, 3, 0.0)));
                Console.WriteLine("Tree Triples (DeepLimit 20, MinVisists 3, Probability = 0.05) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(20, 3, 0.05)));
                Console.WriteLine("Tree Triples (DeepLimit 20, MinVisists 3, Probability = 0.1) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(20, 3, 0.1)));
                Console.WriteLine("Tree Triples (DeepLimit 20, MinVisists 3, Probability = 0.15) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(20, 3, 0.15)));


                Console.WriteLine("\n\n");
                Console.WriteLine("Tree Triples (DeepLimit 30, MinVisists 4, Probability = 0) = {0}",
                   tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(30, 4, 0.0)));
                Console.WriteLine("Tree Triples (DeepLimit 30, MinVisists 4, Probability = 0.05) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(30, 4, 0.05)));
                Console.WriteLine("Tree Triples (DeepLimit 30, MinVisists 4, Probability = 0.1) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(30, 4, 0.1)));
                Console.WriteLine("Tree Triples (DeepLimit 30, MinVisists 4, Probability = 0.15) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(30, 4, 0.15)));
                

                Console.WriteLine("\n\n");
                Console.WriteLine("Tree Triples (DeepLimit 25, MinVisists 4, Probability = 0) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(25, 4, 0.0)));
                Console.WriteLine("Tree Triples (DeepLimit 25, MinVisists 4, Probability = 0.05) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(25, 4, 0.05)));
                Console.WriteLine("Tree Triples (DeepLimit 25, MinVisists 4, Probability = 0.1) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(25, 4, 0.1)));
                Console.WriteLine("Tree Triples (DeepLimit 25, MinVisists 4, Probability = 0.15) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(25, 4, 0.15)));
                

                Console.WriteLine("\n\n");
                Console.WriteLine("Tree Triples (DeepLimit 20, MinVisists 4, Probability = 0) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(20, 4, 0.0)));
                Console.WriteLine("Tree Triples (DeepLimit 20, MinVisists 4, Probability = 0.05) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(20, 4, 0.05)));
                Console.WriteLine("Tree Triples (DeepLimit 20, MinVisists 4, Probability = 0.1) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(20, 4, 0.1)));
                Console.WriteLine("Tree Triples (DeepLimit 20, MinVisists 4, Probability = 0.15) = {0}",
                    tree.TotalTriples(new TripleCollectStrategys.HybridStrategy(20, 4, 0.15)));


                Console.WriteLine("\n");
            } catch(Exception e) {
                Console.WriteLine("");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
        public static void Test12() {
            try {
                var collector = DataCollector.Load(@"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Data\carlsen.dc");
                Console.WriteLine("Collector Loaded!");

                Stopwatch stopwatch = new Stopwatch();
                Console.WriteLine("Calculating to time to load triples from collector");
                stopwatch.Start();

                for(int i = 0; i < collector.TotalCollections; i++) {
                    var triples = collector.GetTriples(i, i + 1);
                    //Console.WriteLine("Load a collection. Size = {0}",  triples.Count );
                }
                    
                stopwatch.Stop();
                Console.WriteLine("Triples loaded = {0} ", collector.TotalTriples);
                Console.WriteLine("Time to load triples = {0} ms", stopwatch.ElapsedMilliseconds);
                Console.WriteLine("Time Estimeted to load 10000 triples = {0} ms", 10_000 * (stopwatch.ElapsedMilliseconds / (float) collector.TotalTriples) );
            } catch(Exception e) {
                Console.WriteLine("");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
        public static void Test11() {
            try {
                Console.WriteLine("Loading Tree Data...");
                var tree = DataTree.Load(@"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Data\carlsen.dt");
                Console.WriteLine("Tree Loaded!");

                Console.WriteLine("Loading Collector by tree...");
                var collector = DataCollector.Load(@"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Data\carlsen.dc");
                collector.Save();
                Console.WriteLine("Collector loaded!");


                Console.WriteLine("Testing id collector triples equals tree triples.");
                var currentCollection = 0;
                var triplesInCurrentCollection = collector.TriplesBetweenCollections(currentCollection, currentCollection + 1);

                List<Triple> triplesOfTree = new List<Triple>();
                List<Triple> triplesOfCollector;

                foreach(var triple in tree.GetTriplesStack()) {
                    if(triplesOfTree.Count == triplesInCurrentCollection) {
                        triplesOfCollector = collector.GetTriples(currentCollection, currentCollection + 1);

                        for(int i = 0; i < triplesInCurrentCollection; i++) {
                            if( !triplesOfTree[i].Equals(triplesOfCollector[i]) ) {
                                Console.WriteLine("Differents Triples");
                                return;
                            }
                        }
                       

                        currentCollection++;
                        triplesInCurrentCollection = collector.TriplesBetweenCollections(currentCollection, currentCollection + 1);
                        triplesOfTree.Clear();
                    }
                    triplesOfTree.Add(triple);
                }

                triplesOfCollector = collector.GetTriples(currentCollection, currentCollection + 1);
                for(int i = 0; i < triplesInCurrentCollection; i++) {
                    if(!triplesOfTree[i].Equals(triplesOfCollector[i])) {
                        Console.WriteLine("Differents Triples");
                        return;
                    }
                }
                Console.WriteLine("Differents Equals");
            } catch(Exception e) {
                Console.WriteLine("");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
        public static void Test10() {
            var game = Game.CreateInitialPosition();

            uint n = 0;
            var moveN = game.GetAllValidMoves().ConvertAll(move => { n++; return (move.ToShort(), n); }).ToArray();
            Triple triple = new Triple(game, 0.5f, moveN);

            var bytes = triple.ToBytes();

            Triple other = Triple.FromBytes(bytes);

            Console.WriteLine("Triples Equals = {0}", triple.Equals(other));
        }
        public static void Test09() {
            try {
                Stopwatch stopwatch = new Stopwatch();

                Console.WriteLine("Calculating to time to load collector");
                stopwatch.Start();

                var collector = DataCollector.Load(@"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Data\datacolecttor.dc");

                stopwatch.Stop();

                Console.WriteLine("Collector Loaded!");
                Console.WriteLine("Time to load collector = {0} ms", stopwatch.ElapsedMilliseconds);

                stopwatch.Reset();

                Console.WriteLine("Total Collections = {0}", collector.TotalCollections);
                for(int i = 0; i < collector.TotalCollections; i++)
                    Console.WriteLine("Triples in Collection {0} = {1}", i, collector.TriplesBetweenCollections(i, i + 1));

            } catch(Exception e) {
                Console.WriteLine("");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
        public static void Test08() {
            try {
                Console.WriteLine("Loading Tree Data...");
                var tree = DataTree.Load(@"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Data\carlsen.dt");
                Console.WriteLine("Tree Loaded!");

                Console.WriteLine("Createing Collector by tree...");
                var collector = tree.ToDataCollector(@"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Data\carlsen.dc", 4096);
                collector.Save();
                Console.WriteLine("Collector created!");

            } catch(Exception e) {
                Console.WriteLine("");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
        public static void Test07() {
            try {
                var tree = DataTree.Load(@"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Data\carlsen.dt");

                var totalTriples = tree.TotalTriples();

                Stopwatch stopwatch = new Stopwatch();
                
                Console.WriteLine("Calculating to load triples with recursion...");
                stopwatch.Start();

                int countTriplesRecursion = 0;
                foreach(var triple in tree.GetTriplesRecursion())
                    countTriplesRecursion++;


                stopwatch.Stop();
                Console.WriteLine("Total Triples = {0}", totalTriples);
                Console.WriteLine("Triples Load with recursion = {0}", countTriplesRecursion);
                Console.WriteLine("Time from load triples with recursion = {0} ms", stopwatch.ElapsedMilliseconds);


                stopwatch.Reset();


                Console.WriteLine("\n\n-----------------------------------\n\n");

                Console.WriteLine("Calculating to load triples with stack...");
                stopwatch.Start();

                int countTriplesStack = 0;
                foreach(var triple in tree.GetTriplesStack())
                    countTriplesStack++;


                stopwatch.Stop();
                Console.WriteLine("Total Triples = {0}", totalTriples);
                Console.WriteLine("Triples Load with stacks = {0}", countTriplesStack);
                Console.WriteLine("Time from load triples with stacks = {0} ms", stopwatch.ElapsedMilliseconds);

            } catch(Exception e) {
                Console.WriteLine("");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
        public static void Test06() {
            BaseModel model = new ModelBuilder().Build();

            Console.WriteLine("Loading tree saved...");
            var tree = DataTree.Load(@"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Data\datatree.dt");
            Console.WriteLine("Tree Loaded");

            int TRIPLES_TO_LOAD = 10000;
            Stopwatch stopwatch = new Stopwatch();

            Console.WriteLine("Calculating to load triples...");
            stopwatch.Start();

            var triples = new List<Triple>();
            foreach(var triple in tree.GetTriplesRecursion()) {
                triples.Add(triple);
                if(triples.Count == TRIPLES_TO_LOAD)
                    break;
            }

            stopwatch.Stop();
            Console.WriteLine("Time from load Triples = {0} ms", stopwatch.ElapsedMilliseconds);


            stopwatch.Reset();


            Console.WriteLine("Calculating to Fit time from triples...");
            stopwatch.Start();

            var inputs = InputFormat.InputsFromTriples(triples);
            Console.WriteLine("Input Shape: " + inputs.shape);

            var outputs = OutputFormat.ExpectedFromTriples(triples);
            Console.WriteLine("Outputs Shapes: {0} {1}", outputs[0].shape, outputs[1].shape);
            model.Fit(inputs, outputs, batch_size: TRIPLES_TO_LOAD);

            stopwatch.Stop();
            Console.WriteLine("Time to fit triples loaded = {0} ms", stopwatch.ElapsedMilliseconds);
        }
        public static void Test05() {
            try {
                Console.WriteLine("Creating Test Tree");
                var tree = new DataTree();

                Console.WriteLine("\n\n--------------------------");
                tree.InsertPGN("e4 e5 Nf3 1-0");
                Console.WriteLine("tree.N = " + tree.N);
                Console.WriteLine("tree.W = " + tree.W);
                Console.WriteLine("tree.Q = " + tree.Q);

                Console.WriteLine("\n\n--------------------------");
                tree.InsertPGN("e4 e5 Nc3 1-0");
                Console.WriteLine("tree.N = " + tree.N);
                Console.WriteLine("tree.W = " + tree.W);
                Console.WriteLine("tree.Q = " + tree.Q);

                Console.WriteLine("\n\n--------------------------");
                tree.InsertPGN("e4 e5 Nc3 Nc6 0-1");
                Console.WriteLine("tree.N = " + tree.N);
                Console.WriteLine("tree.W = " + tree.W);
                Console.WriteLine("tree.Q = " + tree.Q);
            } catch(Exception e) {
                Console.WriteLine("Error: " + e.Message);
            }
        }
        public static void Test04() {
            try {
                Console.WriteLine("Loading tree saved...");
                var tree = DataTree.Load(@"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Data\datatree.dt");
                Console.WriteLine("Tree Loaded");

                Console.WriteLine("tree.N() = " + tree.N);
            } catch(Exception e) {
                Console.WriteLine("Error: " + e.Message);
            }
        }
        public static void Test03() {
            try {
                Console.WriteLine("Loading tree saved...");
                var tree = DataTree.Load(@"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Data\datatree.dt");
                Console.WriteLine("Tree Loaded");

                Console.WriteLine("tree.TotalTriples() = " + tree.TotalTriples());

                var count = 0;
                foreach(var triple in tree.GetTriplesRecursion()) {
                    count++;
                    Console.Write("\r Count = " + count);
                }

                Console.WriteLine("\nTriples = " + count);
            } catch(Exception e) {
                Console.WriteLine("Error: " + e.Message);
			}
        }
        public static void Test02() {
            var tree = new DataTree();
            try {
                tree.InsertFile(@"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Data\pgns\Carlsen.pgn");

            } catch(Exception e) {
                Console.WriteLine("");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
			}

            try {
                Console.WriteLine("");
                Console.WriteLine("Saving tree created");
                DataTree.Save(tree, @"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Data\datatree.dt");

                Console.WriteLine("Loading tree saved");
                var copy = DataTree.Load(@"C:\Users\PICHAU\source\repos\chess_ai\ChessAI\Data\datatree.dt");

                Console.WriteLine("Comparing tree created with tree load");
                var result = DataTree.Equals(tree, copy);

                Console.WriteLine("Result: " + (result ? "Equal" : "Different"));
            } catch(Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
        public static void Test01() {
            Game game = Game.CreateInitialPosition();
            BaseModel model = new ModelBuilder().Build();

            NDarray inputs = InputFormat.GameToInput(game);
            NDarray[] output = model.PredictMultipleOutputs(inputs);

            Console.WriteLine("Output from predict: ");

            var (p, v) = OutputFormat.OutputValues(output);
            Console.WriteLine("p: " + p);
            Console.WriteLine("v: " + v);


            DecisionConfig config = new DecisionConfig();
            config.Episodes = 100;
            config.Policy = DecisionConfig.DecisionPolicy.Deterministc;
            config.Temparature = 1.0f;

            Console.WriteLine("Made decision... ");
            var (move, N) = MCTS.Decision(config, game, model);

            var triples = new List<Triple>() { new Triple(game, N) };
            var expect = OutputFormat.ExpectedFromTriples(triples);
            Console.WriteLine("Expect Values: ");
            Console.WriteLine(expect);

            Console.WriteLine("Training...");
            model.Fit(inputs, expect);
            
        }
    }
}
