using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Others.DataTranslate {
	/* Classe responsavel por coletar os dados de treinamento pertencetes a uma epoca
	 * de treinamento, isto é, coletar os dados dos jogos de treinamento
	 * realizados por uma rede neural e seu clone, e salva-la em um arquivo.
	 * 
	 * Esta Classe, será ainda, responsável por disponibilizar uma interface para obter os
	 * dados de treinamento de posicões expecíficas dentre os jogos realizados.
	 * 
	 * 
	 * Estrutura do arquivo
	 * FILE = (N_COLLECTIONS, [COLLECTION])
	 *	 COLLECTION = (OFFSET_COLLECTION, N_TRIPLES, [TRIPLE])
	 *	   TRIPLE = (OFFSET_TRIPLE, DATA_TRIPLE)
	 */
	public class DataCollector {
		private FileStream fs;

		private uint totalCollections;
		private List<ushort> triplesInCollections;

		private List<ushort> offsetTriples;
		private List<uint> offsetCollections;

		private long lastCollectionCursor;
		private bool collectionStarted;
		
		private DataCollector() {
			totalCollections = 0;
			triplesInCollections = new List<ushort>();

			offsetCollections = new List<uint>();
			offsetTriples = new List<ushort>();

			lastCollectionCursor = 0;
			collectionStarted = false;
			fs = null;
		}
		
		public int TotalCollections {
			get => (int) totalCollections;
		}
		public int TotalTriples {
			get {
				int total = 0;
				foreach(var n in triplesInCollections)
					total += n;
				return total; 
			}
		}
		public int TriplesBetweenCollections(int init, int end) {
			int total = 0;
			for(int i = init; i < end && i < totalCollections; i++)
				total += triplesInCollections[i];
			return total;
		}

		public static DataCollector Create(string path) {
			DataCollector collect = new DataCollector();
			if(File.Exists(path))
				throw new Exception("Cannot create a data file in a exist file");

			collect.fs = File.Create(path, 4096, FileOptions.RandomAccess);

			collect.fs.Seek(0, SeekOrigin.Begin);
			collect.fs.Write(BitConverter.GetBytes(collect.totalCollections), 0, sizeof(uint));

			collect.lastCollectionCursor = collect.fs.Position;
			return collect;
		}
		public static DataCollector Load(string path) {
			DataCollector collect = new DataCollector();
			byte[] buffer = new byte[4];

			collect.fs = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
			
			collect.fs.Seek(0, SeekOrigin.Begin);
			collect.fs.Read(buffer, 0, sizeof(uint));			//Read TotalCollections
			collect.totalCollections = BitConverter.ToUInt32(buffer, 0);
			
			for(int i = 0; i < collect.totalCollections; i++) {
				collect.fs.Read(buffer, 0, sizeof(uint));     //Read CollectionOffset
				uint collectionOffset = BitConverter.ToUInt32(buffer, 0);

				collect.fs.Read(buffer, 0, sizeof(ushort));     //Read TriplesInCollection
				ushort triplesInCollection = BitConverter.ToUInt16(buffer, 0);

				collect.offsetCollections.Add(collectionOffset);
				collect.triplesInCollections.Add(triplesInCollection);
				for(int j = 0; j < triplesInCollection; j++) {
					collect.fs.Read(buffer, 0, sizeof(ushort)); //Read TriplesOffset
					ushort tripleOffset = BitConverter.ToUInt16(buffer, 0);

					collect.offsetTriples.Add(tripleOffset);
					collect.fs.Seek(tripleOffset, SeekOrigin.Current);
				}
			}
			collect.lastCollectionCursor = collect.fs.Position;
			collect.collectionStarted = false;
			return collect;
		}

		public void InitCollection() {
			if(collectionStarted)
				EndCollection();

			fs.Seek(0, SeekOrigin.End);
			fs.Write(BitConverter.GetBytes((uint) 0));		// OFFSET_COLLECTION
			fs.Write(BitConverter.GetBytes((ushort) 0));	// TRIPLES_IN_COLLECTION

			totalCollections++;
			offsetCollections.Add(0);
			triplesInCollections.Add(0);

			lastCollectionCursor = fs.Position;
			collectionStarted = true;
		}
		public void EndCollection() {
			if(collectionStarted) {
				long end = fs.Seek(0, SeekOrigin.End);
				if(totalCollections > 0)
					offsetCollections[(int)(totalCollections - 1)] = (uint) (end - lastCollectionCursor);
			}
			collectionStarted = false;
		}
		public void AddTriple(Triple triple) {
			if(totalCollections == 0 || !collectionStarted)
				InitCollection();

			ushort length = (ushort) triple.Length;

			fs.Seek(0, SeekOrigin.End);
			fs.Write(BitConverter.GetBytes(length), 0, sizeof(ushort)); // Write tripleOffset
			fs.Write(triple.ToBytes(), 0, length);						// Write triple data

			triplesInCollections[(int) (totalCollections - 1)]++;
			offsetTriples.Add(length);
		}
		
		public void Save() {
			if(collectionStarted)
				EndCollection();
			
			fs.Seek(0, SeekOrigin.Begin);
			fs.Write(BitConverter.GetBytes(totalCollections), 0, sizeof(uint));         //Write TotalCollections

			for(int i = 0; i < totalCollections; i++) {
				fs.Write(BitConverter.GetBytes(offsetCollections[i]), 0, sizeof(uint));			//Write OffsetCollection
				fs.Write(BitConverter.GetBytes(triplesInCollections[i]), 0, sizeof(ushort));    //Write nTriples
				
				fs.Seek(offsetCollections[i], SeekOrigin.Current);
			}
		}
		public void Close() {
			fs.Close();
		}


		public List<Triple> GetTriples(int collectionIndexInit, int collectionIndexEnd) {
			List<Triple> triples = new List<Triple>();

			long currentCursor = sizeof(uint); // Incress bytes N_COLLECTIONS
			
			int collectionIndex = collectionIndexInit;
			int initialTripleIndex = 0;
			for(int i = 0; i < collectionIndexInit; i++) {
				currentCursor += sizeof(uint) + sizeof(ushort) + offsetCollections[i];  //Set cursor to next Collection
				initialTripleIndex += triplesInCollections[i];
			}
			int currentTripleIndex = initialTripleIndex;
			int accTripleIndex = initialTripleIndex;

			currentCursor += sizeof(uint) + sizeof(ushort);


			int triplesBetweenCollections = TriplesBetweenCollections(collectionIndexInit, collectionIndexEnd);
			for(int index = 0; index < triplesBetweenCollections; index++) {
				if(currentTripleIndex >= accTripleIndex + triplesInCollections[collectionIndex]) {   //Find a position from other collection
					accTripleIndex += triplesInCollections[collectionIndex];
					collectionIndex++;

					currentCursor += sizeof(uint) + sizeof(ushort);     // Incress bytes from OFFSET_COLLECTION and N_TRIPLES
				}

				currentCursor += sizeof(ushort);
				fs.Seek(currentCursor, SeekOrigin.Begin);

				int tripleLength = offsetTriples[currentTripleIndex];
				byte[] buffer = new byte[tripleLength];

				fs.Read(buffer, 0, tripleLength);
				triples.Add(Triple.FromBytes(buffer));

				currentCursor += tripleLength; //Incress bytes from TRIPLE_OFFSET and TRIPLE
				currentTripleIndex++;
			}

			return triples;
		}
		public List<Triple> GetTriples(int collectionIndexInit, int collectionIndexEnd, int[] triplesIndexs) {
			int positionsBetween = TriplesBetweenCollections(collectionIndexInit, collectionIndexEnd);

			List<int> indexs = triplesIndexs.Where(i => i < positionsBetween).ToList();
			indexs.Sort();

			List<Triple> triples = new List<Triple>();

			long currentCursor = sizeof(uint); // Incress bytes N_COLLECTIONS
			int collectionIndex = collectionIndexInit;
			int initialTripleIndex = 0;
			for(int i = 0; i < collectionIndexInit; i++) {
				currentCursor += sizeof(uint) + sizeof(ushort) + offsetCollections[i];	//Set cursor to next Collection
				initialTripleIndex += triplesInCollections[i];
			}
			int currentTripleIndex = initialTripleIndex;
			int accTripleIndex = initialTripleIndex;


			currentCursor += sizeof(uint) + sizeof(ushort);		// Incress bytes from OFFSET_COLLECTION and N_TRIPLES
			foreach(int index in indexs) {
				//Set currentCursor to init of triple
				int deltaIndex = (initialTripleIndex + index) - currentTripleIndex;
				for(int c = 0; c < deltaIndex; c++) {
					if(currentTripleIndex >= accTripleIndex + triplesInCollections[collectionIndex]) {   //Find a position from other collection
						accTripleIndex += triplesInCollections[collectionIndex];
						collectionIndex++;

						currentCursor += sizeof(uint) + sizeof(ushort);     // Incress bytes from OFFSET_GAME and N_TRIPLES
					}
					currentCursor += sizeof(ushort) + offsetTriples[currentTripleIndex]; //Incress bytes from TRIPLE_OFFSET and TRIPLE
					currentTripleIndex++;
				}
				int tripleLength = offsetTriples[currentTripleIndex];
				byte[] buffer = new byte[tripleLength];

				fs.Seek(currentCursor + sizeof(ushort), SeekOrigin.Begin);
				fs.Read(buffer, 0, tripleLength);
				triples.Add(Triple.FromBytes(buffer));
			}
			return triples;
		}

	}
}
