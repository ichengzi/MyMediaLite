// Copyright (C) 2010, 2011 Zeno Gantner
// Copyright (C) 2011 Artus Krohn-Grimberghe
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

using System;
using System.Data;
using System.Globalization;
using System.IO;
using MyMediaLite.Data;
using MyMediaLite.Util;

namespace MyMediaLite.IO
{
	/// <summary>Class that offers static methods for reading in MovieLens 1M and 10M rating data</summary>
	public class MovieLensRatingData
	{
		/// <summary>Read in rating data from a file</summary>
		/// <param name="filename">the name of the file to read from, "-" if STDIN</param>
		/// <param name="min_rating">the lowest possible rating value, warn on out of range ratings</param>
		/// <param name="max_rating">the highest possible rating value, warn on out of range ratings</param>
		/// <param name="user_mapping">mapping object for user IDs</param>
		/// <param name="item_mapping">mapping object for item IDs</param>
		/// <returns>the rating data</returns>
		static public RatingData Read(string filename, double min_rating, double max_rating, EntityMapping user_mapping, EntityMapping item_mapping)
		{
			if (filename.Equals("-"))
				return Read(Console.In, min_rating, max_rating, user_mapping, item_mapping);
			else
				using ( var reader = new StreamReader(filename) )
					return Read(reader, min_rating, max_rating, user_mapping, item_mapping);
		}

		/// <summary>Read in rating data from a TextReader</summary>
		/// <param name="reader">the <see cref="TextReader"/> to read from</param>
		/// <param name="min_rating">the lowest possible rating value, warn on out of range ratings</param>
		/// <param name="max_rating">the highest possible rating value, warn on out of range ratings</param>
		/// <param name="user_mapping">mapping object for user IDs</param>
		/// <param name="item_mapping">mapping object for item IDs</param>
		/// <returns>the rating data</returns>
		static public RatingData
			Read(TextReader reader,	double min_rating, double max_rating, EntityMapping user_mapping, EntityMapping item_mapping)
		{
			var ratings = new RatingData();

			bool out_of_range_warning_issued = false;
			var ni = new NumberFormatInfo(); ni.NumberDecimalDigits = '.';
			string line;

			while ( (line = reader.ReadLine()) != null )
			{
				if (line.Trim().Equals(string.Empty))
					continue;

				string[] tokens = Utils.Split(line, "::", 4);

				if (tokens.Length < 3)
					throw new IOException("Expected at least three columns: " + line);

				var rating = new RatingEvent();
				rating.user_id = user_mapping.ToInternalID(int.Parse(tokens[0]));
				rating.item_id = item_mapping.ToInternalID(int.Parse(tokens[1]));
				rating.rating = double.Parse(tokens[2], ni);

				if (!out_of_range_warning_issued)
					if (rating.rating > max_rating || rating.rating < min_rating)
					{
						Console.Error.WriteLine("WARNING: rating value out of range [{0}, {1}]: {2} for user {3}, item {4}",
												min_rating, max_rating, rating.rating, rating.user_id, rating.item_id);
						out_of_range_warning_issued = true;
					}

				ratings.AddRating(rating);
			}
			return ratings;
		}
	}
}