/*
 * SonarScanner for .NET
 * Copyright (C) 2016-2022 SonarSource SA
 * mailto:info AT sonarsource DOT com
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
package com.sonar.it.scanner.msbuild;

public class VstsUtils {

  final static String ENV_BUILD_DIRECTORY = "AGENT_BUILDDIRECTORY";
  final static String ENV_SOURCES_DIRECTORY = "BUILD_SOURCESDIRECTORY";

  static Boolean isRunningUnderVsts(){
    return System.getenv(ENV_BUILD_DIRECTORY) != null;
  }

  static String getSourcesDirectory(){
    return GetVstsEnvironmentVariable(ENV_SOURCES_DIRECTORY);
  }

  static String getEnvBuildDirectory(){
    return GetVstsEnvironmentVariable(ENV_BUILD_DIRECTORY);
  }

  private static String GetVstsEnvironmentVariable(String name){
    String value = System.getenv(name);
    if (name == null){
      throw new IllegalStateException("Unable to find VSTS environment variable: " + name);
    }
    return value;
  }
}
