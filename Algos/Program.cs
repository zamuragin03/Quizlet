using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Quizlet;


namespace Quizlet
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(LevinshteinDist.LevenshteinDistance("Привет     ".Trim().ToLower(), "првет".Trim().ToLower()));
        }
    }
    public static class LevinshteinDist
    {
        public static int Minimum(int a, int b, int c) => (a = a < b ? a : b) < c ? a : c;
        public static int LevenshteinDistance(string firstWord, string secondWord)
        {
            var n = firstWord.Length + 1;
            var m = secondWord.Length + 1;
            var matrixD = new int[n, m];

            const int deletionCost = 1;
            const int insertionCost = 1;

            for (var i = 0; i < n; i++)
            {
                matrixD[i, 0] = i;
            }

            for (var j = 0; j < m; j++)
            {
                matrixD[0, j] = j;
            }

            for (var i = 1; i < n; i++)
            {
                for (var j = 1; j < m; j++)
                {
                    var substitutionCost = firstWord[i - 1] == secondWord[j - 1] ? 0 : 1;

                    matrixD[i, j] = Minimum(matrixD[i - 1, j] + deletionCost,          // удаление
                        matrixD[i, j - 1] + insertionCost,         // вставка
                        matrixD[i - 1, j - 1] + substitutionCost); // замена
                }
            }

            return matrixD[n - 1, m - 1];
        }
    }

}
