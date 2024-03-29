﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Jeffparty.Interfaces;

namespace Jeffparty.Client
{
    public class CategoryViewModel : Notifier
    {
        private static readonly HashSet<string> UsedPaths = new();

        public string CategoryHeader { get; set; } = string.Empty;

        public List<QuestionViewModel> CategoryQuestions { get; set; } = new();

        public static CategoryViewModel GenerateNonsense()
        {
            var result = new CategoryViewModel();
            result.CategoryHeader = "that aint real";
            result.CategoryQuestions = Enumerable.Range(1, 5).Select(a => new QuestionViewModel()).ToList();
            return result;
        }

        public static CategoryViewModel? CreateRandom()
        {
            try
            {
                var rootDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Categories");
                var files = Directory.EnumerateFiles(rootDirectory)
                    .Where(fileName => fileName.Contains("category")).ToList();
                var rand = new Random();
                CategoryViewModel? result = null;
                bool shouldLoop;
                do
                {
                    var path = files[rand.Next(0, files.Count)];
                    shouldLoop = !UsedPaths.Add(path) || !ValidatePath(path, out result) || result == null;
                    if (UsedPaths.Count == files.Count)
                    {
                        shouldLoop = false;
                    }
                } while (shouldLoop);

                foreach (var path in UsedPaths)
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch (Exception)
                    {
                    }
                }
                return result;
            }
            catch
            {
                return null;
            }
        }

        public static string CleanUpString(string input)
        {
            return Regex.Replace(HttpUtility.HtmlDecode(input), "<[^>]*>", "");
        }

        private static bool ValidatePath(string path, out CategoryViewModel? result)
        {
            result = null;
            if (!path.Contains("-"))
            {
                return false;
            }

            try
            {
                var pathRoot = path.Substring(0, path.LastIndexOf("-", StringComparison.Ordinal));
                var questionsPath = pathRoot + "-questions.txt";
                var answersPath = pathRoot + "-answers.txt";
                var questionText = File.ReadAllLines(questionsPath);
                var answerText = File.ReadAllLines(answersPath);
                Debug.Assert(questionText[0] == answerText[0]);
                var res = new CategoryViewModel {CategoryHeader = CleanUpString(questionText[0])};
                for (var i = 1; i < questionText.Length; i++)
                {
                    if (questionText[i].Contains("<"))
                    {
                        return false;
                    }

                    res.CategoryQuestions.Add(new QuestionViewModel
                    {
                        QuestionText = CleanUpString(questionText[i]),
                        PointValue = i * 200,
                        AnswerText = CleanUpString(answerText[i])
                    });
                }

                result = res;
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}