﻿/*
 * SonarScanner for .NET
 * Copyright (C) 2016-2025 SonarSource SA
 * mailto: info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System.Collections.Generic;

namespace SonarScanner.MSBuild.Common.CommandLine;

public static class CommandLineFlagPrefix
{
    private static readonly char[] Prefixes = { '-', '/' };

    public static string[] GetPrefixedFlags(params string[] flags)
    {
        var flagPrefixed = new List<string>();
        foreach (var flag in flags)
        {
            foreach (var prefix in Prefixes)
            {
                flagPrefixed.Add($"{prefix}{flag}");
            }
        }

        return flagPrefixed.ToArray();
    }
}
