using SentimentAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditCommentExample
{
	class Program
	{
		static void Main(string[] args)
		{
			var projectDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
			var pathFiles = Directory.EnumerateFiles(projectDir + @"\Samples").ToList();

			SentimentAnalysis.SentimentAnalysis sentimenAnalysis = new SentimentAnalysis.SentimentAnalysis();
			sentimenAnalysis.Load(File.ReadAllLines("AFINN-emoticon-8.txt"));
			sentimenAnalysis.Load(File.ReadAllLines("AFINN-en-165.txt"));
			sentimenAnalysis.Load(File.ReadAllLines("AFINN-fr-165.txt"));

			Dictionary<string, List<double>> scorePerCategory = new Dictionary<string, List<double>>();
			foreach (var path in pathFiles)
			{
				var fileName = Path.GetFileNameWithoutExtension(path);
				var category = fileName.Split('-')[0].Trim();

				var content = File.ReadAllText(path);
				SentimentAnalysis.SentimentAnalysis.Score score = sentimenAnalysis.GetScore(content);

				var averageSentiment = score.AverageSentimentWords;
				
				if (!scorePerCategory.ContainsKey(category))
				{
					scorePerCategory.Add(category, new List<double>());
				}
				scorePerCategory[category].Add(averageSentiment);
			}

			var scores = scorePerCategory
				.Select(m => new KeyValuePair<string, double>(m.Key, m.Value.Average()))
				.OrderBy(m => m.Value);

			foreach (var category in scores)
			{
				Console.WriteLine(category.Key + ": " + category.Value);
			}
			Console.ReadLine();

			Console.WriteLine("Press any key to continue...");
			Console.ReadLine();
		}
	}
}
