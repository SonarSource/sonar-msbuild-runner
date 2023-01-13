﻿/*
 * SonarScanner for .NET
 * Copyright (C) 2016-2023 SonarSource SA
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
using System.Linq;

namespace SonarScanner.MSBuild.Common
{
    public class EmptyPropertyProvider : IAnalysisPropertyProvider
    {
        public static readonly IAnalysisPropertyProvider Instance = new EmptyPropertyProvider();

        private EmptyPropertyProvider()
        {
        }

        #region IAnalysisPropertyProvider interface

        public IEnumerable<Property> GetAllProperties()
        {
            return Enumerable.Empty<Property>();
        }

        public bool TryGetProperty(string key, out Property property)
        {
            property = null;
            return false;
        }

        #endregion IAnalysisPropertyProvider interface
    }
}
