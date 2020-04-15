using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Verse;

namespace RimWorld_LanguageWorker_French
{
	// Log statistics on methods call stack
	public class StatsLogger : IDisposable
	{
		// Collect caller method stats
		private class CallerStats
		{
			static int next_id = 0;
			int id;
			string name;
			int count = 0;
			double time = 0.0;

			// record the calling graph
			List<CallerStats> calledMethods = new List<CallerStats>();

			public CallerStats(string aname)
			{
				name = aname;
				// unique id
				id = next_id;
				next_id++;
			}

			public int Count { get => count; set => count = value; }
			public double Time { get => time; set => time = value; }
			public string Name { get => name; set => name = value; }
			public List<CallerStats> CalledMethods { get => calledMethods; set => calledMethods = value; }
			public double Time_ms() { return time / 1e3; }
			public string NodeId() { return "Node" + id; }

			public void Increment(double elapsed, ref CallerStats called)
			{
				count++;
				time += elapsed;
				if (called != null)
				{
					string called_name = called.Name;
					if (!CalledMethods.Exists(p => p.Name == called_name))
						CalledMethods.Add(called);
				}
			}

			public string ToStringCount()
			{
				return string.Format("{0}, {1:##.000} ms", Count, Time_ms());
			}

			// Graphviz dot string
			private const string NodeFormat = "{0} [shape=record label=\"{1}|{{{2} hits|{3:##.000} ms}}\"]";
			public string ToGraphvizNode()
			{
				return string.Format(NodeFormat, NodeId(),
					Name.Replace("(", "(\\l  ").Replace(", ","\\l  ").Replace(")", "\\l)\\l ").ReplaceFirst(" ","\\l"),
					Count, Time_ms());
			}

			private const string EdgeFormat = "{0} -> {1}";
			public string ToGraphvizEdge(int index)
			{
				if (index < CalledMethods.Count)
				{
					CallerStats called = CalledMethods[index];
					return string.Format(EdgeFormat, NodeId(), called.NodeId());
				}
				// else
				return null;
			}

			public string ToGraphvizEdges()
			{
				string result = "";
				for (int i = 0; i < CalledMethods.Count; i++)
				{
					result += ToGraphvizEdge(i) + "\n";
				}
				return result;
			}
		}

		// Collect processed string stats
		private class StringStats
		{
			private static uint next_index = 1;

			string translated;
			uint hits = 0;
			bool processed = false;
			double totalTime = 0.0;
			// record call history
			uint index;                   // serial id of this unique string
			uint history_hitCount;        // number of calls
			uint history_hitProcessed;    // number of calls with processed strings
			uint history_hitNotProcessed; // number of calls with not processed strings

			List<double> processTime = new List<double>();

			public StringStats(string atranslated, uint ahit, double time, bool aprocessed)
			{
				translated = atranslated;
				hits = ahit;
				processTime.Add(time);
				totalTime = time;
				processed = aprocessed;
				// string call history (static counters)
				index = next_index;
				next_index++;
				history_hitCount = hitCount;
				history_hitProcessed = hitProcessed;
				history_hitNotProcessed = hitNotProcessed;
			}

			public string Translated { get => translated; set => translated = value; }
			public uint Hits { get => hits; set => hits = value; }
			public List<double> ProcessTime { get => processTime; set => processTime = value; }
			public double TotalTime { get => totalTime; set => totalTime = value; }
			public bool Processed { get => processed; set => processed = value; }

			public void Increment(double time)
			{
				hits += 1;
				processTime.Add(time);
				totalTime += time;
			}

			private const string ToStringFormat = "{0}\nchanged({1}):length({2}):hit({3}):time:min {4:##.000} µs, avg {5:##.000} µs, max {6:##.000} µs, total {7:##.000} µs";
			public override string ToString()
			{
				// dump readable string with the string translated
				return string.Format(
					ToStringFormat,
					translated, processed, translated.Length, hits,
					processTime.Min(), processTime.Average(), processTime.Max(), totalTime
				//string.Join(",", processTime)
				);
			}

			public static string ToGraphHeaderString()
			{
				// dump a line of data heaeder in csv format
				return "index,hitCount,hitProcessed,hitNotProcessed," +
					"changed,length,hits,minTime,avgTime,maxTime,totalTime";
			}

			private const string ToGraphStringFormat = "{0},{1},{2},{3},{4},{5},{6},{7:##.000},{8:##.000},{9:##.000},{10:##.000}";
			public string ToGraphString()
			{
				// dump a line of data in csv format
				return string.Format(
					ToGraphStringFormat,
					index, history_hitCount, history_hitProcessed, history_hitNotProcessed,
					processed, translated.Length, hits,
					processTime.Min(), processTime.Average(), processTime.Max(), totalTime
				);
			}
		}

		private static Dictionary<string, CallerStats> callers = new Dictionary<string, CallerStats>();

		private static List<KeyValuePair<string, StringStats>> loggedKeys = new List<KeyValuePair<string, StringStats>>();

		// Log the processed strings into files
		private static Logger logNotProcessed = new Logger("PostProcessed_no.txt");
		private static Logger logInProcessed = new Logger("PostProcessed_in.txt");
		private static Logger logOutProcessed = new Logger("PostProcessed_out.txt");
		private static Logger logStats = new Logger("PostProcessed_stats.txt");

		public static Logger LogNotProcessed { get => logNotProcessed; set => logNotProcessed = value; }
		public static Logger LogInProcessed { get => logInProcessed; set => logInProcessed = value; }
		public static Logger LogOutProcessed { get => logOutProcessed; set => logOutProcessed = value; }
		public static Logger LogStats { get => logStats; set => logStats = value; }

		// Log the translated strings only once, count the hits
		private static uint hitCount = 0;       // number of calls
		private static uint hitProcessed = 0;   // number of calls with processed strings
		private static uint hitNotProcessed = 0;// number of calls with not processed strings
		private static double overallTime = 0.0;// total process time

		// Unreliable time measure
		Stopwatch stopwatch;
		StackTrace currentCallStack;

		internal static double GetMicroseconds(Stopwatch stopwatch, int numberofDigits = 1)
		{
			double time = stopwatch.ElapsedTicks / (double)Stopwatch.Frequency;
			return Math.Round(1e6 * time, numberofDigits);
		}

		internal void LogProcessedString(string original, string processed_str, double elapsed)
		{
			// Count all calls
			hitCount++;
			overallTime += elapsed;

			// Log all PostProcessed strings
			if (!loggedKeys.Exists(p => p.Key == original))
			{
				// Add new StringStats
				StringStats value = new StringStats(processed_str, 1, elapsed, processed_str != original);
				loggedKeys.Add(new KeyValuePair<string, StringStats>(original, value));
				try
				{
					if (processed_str != original)
					{
						hitProcessed++;
						LogInProcessed.Message(string.Format("PostProcessed_str:{0,6:##.0} µs:{1}", elapsed, original));
						LogOutProcessed.Message(string.Format("PostProcessed_str:{0,6:##.0} µs:{1}", elapsed, processed_str));
					}
					else
					{
						hitNotProcessed++;
						LogNotProcessed.Message(string.Format("PostProcessed_no({0}):{1,6:##.0} µs:{2}", hitCount.ToString(), elapsed, original));
					}
				}
				catch (MissingMethodException e)
				{
					// Unit test does not initialize Verse.Log for some reason
					Console.WriteLine("Log.Message: {0}", e.Message);
				}
			}
			else
			{
				// Update StringStats
				int hitIndex = loggedKeys.FirstIndexOf(p => p.Key == original);
				KeyValuePair<string, StringStats> currentStats = loggedKeys[hitIndex];
				currentStats.Value.Increment(elapsed);
				loggedKeys[hitIndex] = currentStats;
			}
		}

		internal void LogCallStack(string original, StackTrace callStack, double elapsed)
		{
			int hitIndex;
			if (!loggedKeys.Exists(p => p.Key == original))
			{
				// Use loggedKeys.Count as index, string stats will be updated later
				hitIndex = loggedKeys.Count;
				LogStats.Message(string.Format("#{0}:{1:##.0} µs", hitIndex, elapsed));
				CallerStats previous = null;
				// Log up to two levels
				for (int i = 0; i < callStack.FrameCount; i++)
				{
					StackFrame frame = callStack.GetFrame(i);
					MethodBase method = frame.GetMethod();
					// TODO: modify key for the dot graph
					string key = method.ToString();
					CallerStats stats;

					callers.TryGetValue(key, out stats);
					if (stats == null)
						stats = new CallerStats(method.ToString());
					stats.Increment(elapsed, ref previous);
					callers.SetOrAdd(key, stats);
					LogStats.Message("[" + i + "]" + key);

					// record the calling graph
					previous = stats;
				}
				LogStats.Message("----");
			}
		}

		internal void StartLogging(StackTrace callStack)
		{
			currentCallStack = callStack;
			stopwatch = Stopwatch.StartNew();
		}

		internal void StopLogging(string original, string processed_str)
		{
			stopwatch.Stop();
			double elapsed = GetMicroseconds(stopwatch);
			// Log call stacks for this method
			LogCallStack(original, currentCallStack, elapsed);
			// Log all PostProcessed strings
			LogProcessedString(original, processed_str, elapsed);
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// Dispose managed state (managed objects).
				}

				// Free unmanaged resources (unmanaged objects) and override a finalizer below.
				// Dump all call stats into the file
				Logger logStatsSummary = new Logger("PostProcessed_callstats_summary.txt");
				logStatsSummary.Message("----");
				logStatsSummary.Message("Overall call stack stats ordered by totalTime:");
				logStatsSummary.Message("Format: [#call stack level]Name(...):hitCount, totalTime");
				logStatsSummary.Message(string.Format("hitCount: {0}, overall {1} ms", hitCount, overallTime / 1e3));
				logStatsSummary.Message("----");

				List<KeyValuePair<string, CallerStats>> sortedcalls = callers.ToList();
				sortedcalls.Sort((x, y) => y.Value.Time.CompareTo(x.Value.Time));
				foreach (KeyValuePair<string, CallerStats> pairs in sortedcalls)
				{
					logStatsSummary.Message(pairs.Key + ": " + pairs.Value.ToStringCount());
				}

				// Dump all call graphs
				Logger logCallGraph = new Logger("PostProcessed_callgraph.dot");
				logCallGraph.Message("// dot example.dot - Tpdf - o test.pdf");
				logCallGraph.Message("digraph PostProcessed {");
				foreach (KeyValuePair<string, CallerStats> pairs in callers)
				{
					logCallGraph.Message(pairs.Value.ToGraphvizNode());
					logCallGraph.Message(pairs.Value.ToGraphvizEdges());
				}
				logCallGraph.Message("}");

				// Dump all string stats
				Logger logStrStatsSummary = new Logger("PostProcessed_strstats_summary.txt");
				logStrStatsSummary.Message("----");
				logStrStatsSummary.Message("Strings stats:");
				logStrStatsSummary.Message("----");

				// Dump all string stats for graphs
				Logger logGraphSummary = new Logger("PostProcessed_graph.csv");
				logGraphSummary.Message(StringStats.ToGraphHeaderString());

				int i = 0;
				foreach (KeyValuePair<string, StringStats> pairs in loggedKeys)
				{
					logStrStatsSummary.Message("[" + i + "]:" + pairs.Value.ToString());

					logGraphSummary.Message(pairs.Value.ToGraphString());
					i++;
				}
				disposedValue = true;
			}
		}

		~StatsLogger()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(false);
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}