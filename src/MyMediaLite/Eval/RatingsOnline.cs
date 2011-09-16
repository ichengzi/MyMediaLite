// Copyright (C) 2011 Zeno Gantner
//
// This file is part of MyMediaLite.
//
// MyMediaLite is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MyMediaLite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using MyMediaLite.Data;
using MyMediaLite.RatingPrediction;

namespace MyMediaLite.Eval
{
	/// <summary>Online evaluation for rating prediction</summary>
	public static class RatingsOnline
	{
		/// <summary>Online evaluation for rating prediction</summary>
		/// <remarks>
		/// Every rating that is tested is added to the training set afterwards.
		/// </remarks>
		/// <param name="recommender">rating predictor</param>
		/// <param name="ratings">Test cases</param>
		/// <returns>a Dictionary containing the evaluation results</returns>
		static public Dictionary<string, double> EvaluateOnline(IIncrementalRatingPredictor recommender, IRatings ratings)
		{
			double rmse = 0;
			double mae  = 0;

			if (recommender == null)
				throw new ArgumentNullException("recommender");
			if (ratings == null)
				throw new ArgumentNullException("ratings");

			// iterate in random order    // TODO also support chronological order
			foreach (int index in ratings.RandomIndex)
			{
				double error = (recommender.Predict(ratings.Users[index], ratings.Items[index]) - ratings[index]);

				rmse += error * error;
				mae  += Math.Abs(error);

				recommender.AddRating(ratings.Users[index], ratings.Items[index], ratings[index]);
			}
			mae  = mae / ratings.Count;
			rmse = Math.Sqrt(rmse / ratings.Count);

			var result = new Dictionary<string, double>();
			result["RMSE"] = rmse;
			result["MAE"]  = mae;
			result["NMAE"] = mae / (recommender.MaxRating - recommender.MinRating);
			return result;
		}

	}
}
