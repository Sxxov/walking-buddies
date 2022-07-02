using System;

namespace WalkingBuddies.Core.Card
{
	public abstract class AbstractParseException : Exception
	{
		public CardKinds[] input;
		public int i;
		public CardKinds? fault
		{
			get => input[i];
		}

		public AbstractParseException(CardKinds[] input, int i)
			: this("", input, i) { }

		public AbstractParseException(string message, CardKinds[] input, int i)
			: base(message)
		{
			this.input = input;
			this.i = i;
		}
	}
}
