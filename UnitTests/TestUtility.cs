using System.IO;
using NUnit.Framework;

namespace UnitTests
{
    /// <summary>
    /// Utility class for unit test
    /// </summary>
    internal static class TestUtility
    {
        private const string TestFileFolder = "TestData";

        /// <summary>
        /// Function to get an absolute file path to a file named <paramref name="testFilename"/>.
        /// </summary>
        /// <param name="testFilename">The name of the test-file</param>
        /// <returns>The absolute path to <paramref name="testFilename"/>.</returns>
        public static string GetPathToTestFile(string testFilename)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, TestFileFolder, testFilename);
        }
    }
}
