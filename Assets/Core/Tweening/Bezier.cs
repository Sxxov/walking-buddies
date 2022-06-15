using System;

namespace WalkingBuddies.Core.Tweening
{
	public class Bezier
	{
		private const int newtonIterations = 4;
		private const double newtonMinSlope = 0.001;
		private const double subdivisionPrecision = 0.0000001;
		private const int subdivisionMaxIterations = 10;
		private const int splineTableSize = 11;
		private const double sampleStepSize = 0.1;

		private const int finalSampleIndex = splineTableSize - 1;

		private readonly double[] lut = new double[splineTableSize];

		private readonly double x1;
		private readonly double x2;
		private readonly double y1;
		private readonly double y2;

		public Bezier(double x1, double y1, double x2, double y2)
		{
			this.x1 = x1;
			this.y1 = y1;
			this.x2 = x2;
			this.y2 = y2;

			// calculate sample values
			for (int i = 0, l = splineTableSize; i < l; ++i)
			{
				lut[i] = CalcBezier(i * sampleStepSize, x1, x2);
			}
		}

		public double At(double v) => CalcBezier(GetTforX(v), y1, y2);

		private double GetTforX(double aX)
		{
			var intervalStart = 0d;
			var currentSampleIndex = 1;

			while (
				currentSampleIndex != finalSampleIndex
				&& lut[currentSampleIndex] <= aX
			)
			{
				intervalStart += sampleStepSize;
				++currentSampleIndex;
			}

			--currentSampleIndex;

			var currentSample = lut[currentSampleIndex];
			var nextSample = lut[currentSampleIndex + 1];

			// interpolate to provide an initial guess for t
			var dist = (aX - currentSample) / (nextSample - currentSample);
			var guessForT = intervalStart + dist * sampleStepSize;
			var initialSlope = GetSlope(guessForT, x1, x2);

			if (initialSlope >= newtonMinSlope)
			{
				return NewtonRaphsonIterate(aX, guessForT, x1, x2);
			}

			if (initialSlope == 0)
			{
				return guessForT;
			}

			return BinarySubdivide(
				aX,
				intervalStart,
				intervalStart + sampleStepSize,
				x1,
				x2
			);
		}

		private static double A(double aA1, double aA2)
		{
			return 1 - 3 * aA2 + 3 * aA1;
		}

		private static double B(double aA1, double aA2)
		{
			return 3 * aA2 - 6 * aA1;
		}

		private static double C(double aA1)
		{
			return 3 * aA1;
		}

		// Returns x(t) given t, x1, and x2, or y(t) given t, y1, and y2.
		private static double CalcBezier(double aT, double aA1, double aA2)
		{
			return ((A(aA1, aA2) * aT + B(aA1, aA2)) * aT + C(aA1)) * aT;
		}

		// Returns dx/dt given t, x1, and x2, or dy/dt given t, y1, and y2.
		private static double GetSlope(double aT, double aA1, double aA2)
		{
			return 3 * A(aA1, aA2) * aT * aT + (2 * B(aA1, aA2) * aT + C(aA1));
		}

		private static double BinarySubdivide(
			double aX,
			double aA,
			double aB,
			double x1,
			double x2
		)
		{
			double currentX;
			double currentT;
			var i = 0;

			do
			{
				currentT = aA + (aB - aA) / 2;
				currentX = CalcBezier(currentT, x1, x2) - aX;

				if (currentX > 0)
				{
					aB = currentT;
				}
				else
				{
					aA = currentT;
				}
			} while (
				Math.Abs(currentX) > subdivisionPrecision
				&& ++i < subdivisionMaxIterations
			);

			return currentT;
		}

		private static double NewtonRaphsonIterate(
			double aX,
			double aGuessT,
			double x1,
			double x2
		)
		{
			for (int i = 0, l = newtonIterations; i < l; ++i)
			{
				var currentSlope = GetSlope(aGuessT, x1, x2);

				if (currentSlope == 0)
				{
					return aGuessT;
				}

				var currentX = CalcBezier(aGuessT, x1, x2) - aX;

				aGuessT -= currentX / currentSlope;
			}

			return aGuessT;
		}
	}
}
