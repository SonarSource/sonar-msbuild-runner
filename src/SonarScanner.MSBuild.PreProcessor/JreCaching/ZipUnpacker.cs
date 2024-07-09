﻿/*
 * SonarScanner for .NET
 * Copyright (C) 2016-2024 SonarSource SA
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

using System.IO;
using System.IO.Compression;

namespace SonarScanner.MSBuild.PreProcessor.JreCaching;

public class ZipUnpacker : IUnpacker
{
    public void Unpack(Stream archive, string destinationDirectory)
    {
        // Zip unpack ignores the wrapper and uses ExtractToDirectory directly.
        // This avoids problems with zip-slip attacks and file permission setting.
        // As a downside, the tests are a relying on direct disk operations.
        using var zipArchive = new ZipArchive(archive, ZipArchiveMode.Read);
        zipArchive.ExtractToDirectory(destinationDirectory);
    }
}