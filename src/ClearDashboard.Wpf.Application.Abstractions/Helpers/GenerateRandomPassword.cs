using System;
using System.Text;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public static class GenerateRandomPassword
    {
        // Generate a random number between two numbers
        public static int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        // Generate a random string with a given size and case.
        // If second parameter is true, the return string is lowercase
        private static string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            for (int i = 0; i < size; i++)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }

        // shuffle around a string
        private static string Shuffle(string str)
        {
            char[] array = str.ToCharArray();
            Random rng = new Random();
            int n = array.Length;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var value = array[k];
                array[k] = array[n];
                array[n] = value;
            }
            return new string(array);
        }

        // Generate a random password
        public static string RandomPassword(int passwordSize = 12)
        {
            var builder = new StringBuilder();
            builder.Append(RandomString(10, true));
            builder.Append(RandomNumber(1000, 9999));
            builder.Append(RandomString(10, false));

            var shuffledText = Shuffle(builder.ToString());

            shuffledText = shuffledText.Substring(0, passwordSize);
            return shuffledText;
        }
    }
}
