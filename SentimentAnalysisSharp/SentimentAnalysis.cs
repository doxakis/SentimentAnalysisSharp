using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SentimentAnalysis
{
    public class SentimentAnalysis
    {
		private readonly Dictionary<string, int> _words;
		public IDictionary<string, int> Words { get { return _words; } }
		public int WordsCount { get { return _words.Count; } }

		public SentimentAnalysis()
		{
			_words = new Dictionary<string, int>();
		}

		public void Load(string[] lines)
		{
			foreach (var line in lines)
			{
				var bits = line.Split('\t');
				if (!_words.ContainsKey(bits[0]))
				{
					_words.Add(bits[0], int.Parse(bits[1]));
				}
			}
		}

		private static string CleanUpInput(string input)
		{
			// Padding for emoticon
			Regex regex = new Regex(@"(\:\w+\:|\<[\/\\]?3|[\(\)\\\D|\*\$][\-\^]?[\:\;\=]|[\:\;\=B8][\-\^]?[3DOPp\@\$\*\\\)\(\/\|])(?=\s|[\!\.\?]|$)");
			input = regex.Replace(input, m =>
			{
				return ReplaceNamedGroup(input, 1, m);
			});
			// Trim extra spaces.
			input = Regex.Replace(input, @"\s+", " ");
			input = input.ToLower();
			return " " + input + " ";
		}

		private static string ReplaceNamedGroup(string input, int groupNum, Match m)
		{
			string result = m.Value;
			foreach (Capture cap in m.Groups[groupNum]?.Captures)
			{
				result = result.Remove(cap.Index - m.Index, cap.Length);
				result = result.Insert(cap.Index - m.Index, " " + cap.Value + " ");
			}
			return result;
		}

		/// <summary>
		/// Calculates sentiment score of a sentence
		/// </summary>
		/// <param name="input">Sentence</param>
		/// <returns>Score object</returns>
		public Score GetScore(string input)
		{
			var cleanInput = CleanUpInput(input);
			var score = new Score { Tokens = new List<string>() };
			foreach (var token in _words)
			{
				int count = 0, n = 0;
				if (token.Key != "")
				{
					var pattern = " " + token.Key + " ";
					while ((n = cleanInput.IndexOf(pattern, n, StringComparison.InvariantCulture)) != -1)
					{
						n += token.Key.Length;
						++count;
					}
				}

				if (count == 0) continue;

				var item = _words[token.Key];
				score.Tokens.Add(token.Key);
				score.Words.Add(token.Key);

				if (item > 0) score.Positive.Add(token.Key);
				if (item < 0) score.Negative.Add(token.Key);

				score.Sentiment += item * count;
			}

			return score;
		}
		
		public class Score
		{
			/// <summary>
			/// Tokens which were scored
			/// </summary>
			public List<string> Tokens { get; set; }

			/// <summary>
			/// Total sentiment score of the tokens
			/// </summary>
			public int Sentiment { get; set; }
			
			/// <summary>
			/// Average sentiment score Sentiment/Tokens.Count
			/// </summary>
			public double AverageSentimentTokens {
				get {
					if (Words.Count() == 0)
					{
						return 0;
					}
					return (double)Sentiment / Tokens.Count();
				}
			}
			
			/// <summary>
			/// Average sentiment score Sentiment/Words.Count
			/// </summary>
			public double AverageSentimentWords {
				get {
					if (Words.Count() == 0)
					{
						return 0;
					}
					return (double)Sentiment / Words.Count();
				}
			}
			
			/// <summary>
			/// Words that were used from AFINN
			/// </summary>
			public IList<string> Words { get; set; }
			
			/// <summary>
			/// Words that had negative sentiment
			/// </summary>
			public IList<string> Negative { get; set; }
			
			/// <summary>
			/// Words that had positive sentiment
			/// </summary>
			public IList<string> Positive { get; set; }

			public Score()
			{
				Words = new List<string>();
				Negative = new List<string>();
				Positive = new List<string>();
			}
		}
	}
}
