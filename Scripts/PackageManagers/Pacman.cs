using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BubblePack.Scripts;

public static class Pacman
{
	public static Dictionary<string, PackageData> GetInstalledPackages()
	{
		// One call, many packages.
		string output = LPU.RunBash("pacman -Qi");

		var packages = new Dictionary<string, PackageData>(StringComparer.Ordinal);

		PackageData current = default;
		bool hasCurrent = false;

		string currentField = string.Empty;

		void FinalizeCurrent()
		{
			if (!hasCurrent)
			{
				return;
			}

			if (current.Dependencies == null)
			{
				current.Dependencies = new List<string>();
			}

			if (!string.IsNullOrWhiteSpace(current.Name))
			{
				packages[current.Name] = current;
			}

			current = default;
			hasCurrent = false;
			currentField = string.Empty;
		}

		using var reader = new StringReader(output);

		string line;
		while ((line = reader.ReadLine()) != null)
		{
			if (string.IsNullOrWhiteSpace(line))
			{
				// Blank line = end of a package block.
				FinalizeCurrent();
				continue;
			}

			// Continuation lines are indented. pacman uses these e.g. for Depends On and Optional Deps.
			// We only care about Depends On for now.
			if (char.IsWhiteSpace(line[0]) && !string.IsNullOrEmpty(currentField))
			{
				if (currentField == "Depends On")
				{
					EnsureInitialized(ref current, ref hasCurrent);
					ParseDependsTokensInto(ref current, line.Trim());
				}

				continue;
			}

			int sep = line.IndexOf(" : ", StringComparison.Ordinal);
			if (sep < 0)
			{
				continue;
			}

			string key = line.Substring(0, sep).Trim();
			string value = line.Substring(sep + 3).Trim();
			currentField = key;

			EnsureInitialized(ref current, ref hasCurrent);

			switch (key)
			{
				case "Name":
					current.Name = value;
					break;

				case "Version":
					current.Version = value;
					break;

				case "Description":
					current.Description = value;
					break;

				case "Packager":
					// "Packager" is the closest stable field pacman provides to an "author/maintainer".
					current.Author = value;
					break;

				case "Installed Size":
					current.SizeInMB = ParseInstalledSizeToMB(value);
					break;

				case "Depends On":
					ParseDependsTokensInto(ref current, value);
					break;
			}
		}

		// EOF might not end in a blank line.
		FinalizeCurrent();

		return packages;
	}

	private static void EnsureInitialized(ref PackageData pkg, ref bool hasCurrent)
	{
		if (!hasCurrent)
		{
			pkg = new PackageData
			{
				Dependencies = new List<string>(),
				Description = string.Empty,
				Version = string.Empty,
				Author = string.Empty,
				SizeInMB = 0.0f
			};
			hasCurrent = true;
		}
		else if (pkg.Dependencies == null)
		{
			pkg.Dependencies = new List<string>();
		}
	}

	private static void ParseDependsTokensInto(ref PackageData pkg, string value)
	{
		if (string.IsNullOrWhiteSpace(value) || value == "None")
		{
			return;
		}

		// pacman Depends On is whitespace-separated list of deps, which may include version constraints
		// e.g. "glibc>=2.38 zlib"
		string[] tokens = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

		foreach (string token in tokens)
		{
			string depName = NormalizeDependencyName(token);
			if (string.IsNullOrWhiteSpace(depName))
			{
				continue;
			}

			if (!pkg.Dependencies.Contains(depName))
			{
				pkg.Dependencies.Add(depName);
			}
		}
	}

	private static string NormalizeDependencyName(string raw)
	{
		if (string.IsNullOrWhiteSpace(raw))
		{
			return string.Empty;
		}

		raw = raw.Trim();

		// Drop version constraints: >=, <=, =, >, < (pacman uses these in dep tokens)
		int opIndex = raw.IndexOfAny(new[] { '<', '>', '=' });
		if (opIndex > 0)
		{
			raw = raw.Substring(0, opIndex);
		}

		// Drop any arch qualifiers if they appear for some reason (more common in apt, but harmless here)
		int colonIndex = raw.IndexOf(':');
		if (colonIndex > 0)
		{
			raw = raw.Substring(0, colonIndex);
		}

		return raw.Trim();
	}

	private static float ParseInstalledSizeToMB(string value)
	{
		// Examples:
		// "123.45 MiB"
		// "900.00 KiB"
		// "1.23 GiB"
		if (string.IsNullOrWhiteSpace(value))
		{
			return 0.0f;
		}

		string[] parts = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length < 2)
		{
			return 0.0f;
		}

		if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float amount))
		{
			// Fallback in case locale uses comma decimals.
			if (!float.TryParse(parts[0], out amount))
			{
				return 0.0f;
			}
		}

		string unit = parts[1];

		return unit switch
		{
			"KiB" => amount / 1024.0f,
			"MiB" => amount,
			"GiB" => amount * 1024.0f,
			_ => amount
		};
	}
}