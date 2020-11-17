using System;
using System.Collections.Generic;
using System.Text;

namespace BooleanSearcher
{
	/// <summary>
	/// Class that takes care of splitting document text into tokens.
	/// </summary>
    public class Tokenizer
    {

		/// <summary>
		/// Function that takes a char and returns a bool. Determines if tokenizer should split off a token when seeing a certain character.
		/// </summary>
		private Func<char, bool> tokenSplittingRule;
		/// <summary>
		/// Functiona that determines if a character should be included in tokens. Here I am using this to remove whitespace.
		/// </summary>
		private Func<char, bool> characterErasingRule;
		public Tokenizer(Func<char, bool> tokenSplittingRule, Func<char, bool> characterErasingRule)
        {
			this.tokenSplittingRule = tokenSplittingRule;
			this.characterErasingRule = characterErasingRule;
		}

		/// <summary>
		/// Splits text into separate tokens.
		/// </summary>
		/// <param name="str">Text to tokenize</param>
		/// <returns>A list of tokens (not yet normalized)</returns>
		public List<string> Tokenize(string str)
		{
			var tokens = new List<string>();

			int lastSplit = 0;
			for (int i = 0; i < str.Length; i++)
			{
				char c = str[i];
				if (ShouldSplitOnCharacter(c))
				{
					var token = str.Substring(lastSplit, i - lastSplit);
					tokens.Add(token);
					if (ShouldRemoveCharacter(c))
					{
						i++;
					}
					lastSplit = i;
				}
			}
			var lastToken = str.Substring(lastSplit);
			tokens.Add(lastToken);

			return tokens;
		}

		private bool ShouldSplitOnCharacter(char c)
		{
			return tokenSplittingRule.Invoke(c);
		}

		private bool ShouldRemoveCharacter(char c)
		{
			return characterErasingRule.Invoke(c);
		}
	}
}
