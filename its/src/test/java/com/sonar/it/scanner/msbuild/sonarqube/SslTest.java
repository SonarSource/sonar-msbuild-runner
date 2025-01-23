/*
 * SonarScanner for .NET
 * Copyright (C) 2016-2024 SonarSource SA
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
package com.sonar.it.scanner.msbuild.sonarqube;

import com.eclipsesource.json.Json;
import com.sonar.it.scanner.msbuild.utils.AzureDevOpsUtils;
import com.sonar.it.scanner.msbuild.utils.EnvironmentVariable;
import com.sonar.it.scanner.msbuild.utils.ProxyAuthenticator;
import com.sonar.it.scanner.msbuild.utils.ScannerClassifier;
import com.sonar.it.scanner.msbuild.utils.TestUtils;
import com.sonar.orchestrator.build.BuildResult;
import com.sonar.orchestrator.build.ScannerForMSBuild;
import com.sonar.orchestrator.http.HttpException;
import com.sonar.orchestrator.locator.FileLocation;
import com.sonar.orchestrator.util.Command;
import com.sonar.orchestrator.util.CommandExecutor;
import com.sonar.orchestrator.util.NetworkUtils;
import com.sonar.orchestrator.version.Version;
import jakarta.servlet.ServletException;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.LinkOption;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.time.Duration;
import java.util.Arrays;
import java.util.Collections;
import java.util.List;
import java.util.concurrent.ConcurrentLinkedDeque;
import java.util.function.Function;
import java.util.stream.Collectors;
import org.apache.commons.io.FileUtils;
import org.apache.commons.lang.StringUtils;
import org.eclipse.jetty.client.api.Request;
import org.eclipse.jetty.proxy.ProxyServlet;
import org.eclipse.jetty.security.ConstraintMapping;
import org.eclipse.jetty.security.ConstraintSecurityHandler;
import org.eclipse.jetty.security.HashLoginService;
import org.eclipse.jetty.security.SecurityHandler;
import org.eclipse.jetty.security.UserStore;
import org.eclipse.jetty.server.Handler;
import org.eclipse.jetty.server.HttpConfiguration;
import org.eclipse.jetty.server.HttpConnectionFactory;
import org.eclipse.jetty.server.Server;
import org.eclipse.jetty.server.ServerConnector;
import org.eclipse.jetty.server.SslConnectionFactory;
import org.eclipse.jetty.server.handler.DefaultHandler;
import org.eclipse.jetty.server.handler.HandlerCollection;
import org.eclipse.jetty.servlet.ServletContextHandler;
import org.eclipse.jetty.servlet.ServletHandler;
import org.eclipse.jetty.servlet.ServletHolder;
import org.eclipse.jetty.util.security.Constraint;
import org.eclipse.jetty.util.security.Credential;
import org.eclipse.jetty.util.ssl.SslContextFactory;
import org.eclipse.jetty.util.thread.QueuedThreadPool;
import org.junit.jupiter.api.AfterEach;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.api.io.TempDir;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonarqube.ws.Ce;
import org.sonarqube.ws.Components;
import org.sonarqube.ws.Issues.Issue;
import org.sonarqube.ws.client.WsClient;
import org.sonarqube.ws.client.components.ShowRequest;

import static com.sonar.it.scanner.msbuild.sonarqube.Tests.ORCHESTRATOR;
import static com.sonar.it.scanner.msbuild.utils.AzureDevOpsUtils.getEnvBuildDirectory;
import static com.sonar.it.scanner.msbuild.utils.AzureDevOpsUtils.getSourcesDirectory;
import static com.sonar.it.scanner.msbuild.utils.AzureDevOpsUtils.isRunningUnderAzureDevOps;
import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.tuple;
import static org.awaitility.Awaitility.await;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assumptions.assumeFalse;
import static org.junit.jupiter.api.Assumptions.assumeTrue;

@ExtendWith(Tests.class)
class SslTest {
  final static Logger LOG = LoggerFactory.getLogger(SslTest.class);

  private static final String PROJECT_KEY = "my.project";
  private static Server server;

  private static final ConcurrentLinkedDeque<String> seenByProxy = new ConcurrentLinkedDeque<>();

  @TempDir
  public Path basePath;

  @BeforeEach
  public void setUp() {
    jettyServerSetup();
    TestUtils.reset(ORCHESTRATOR);
    seenByProxy.clear();
  }

  @AfterEach
  public void stopProxy() throws Exception {
    if (server != null && server.isStarted()) {
      server.stop();
    }
  }

  @Test
  void testSample() throws Exception {
    String localProjectKey = PROJECT_KEY + ".2";
    ORCHESTRATOR.getServer().restoreProfile(FileLocation.of("projects/ProjectUnderTest/TestQualityProfile.xml"));
    ORCHESTRATOR.getServer().provisionProject(localProjectKey, "sample");
    ORCHESTRATOR.getServer().associateProjectToQualityProfile(localProjectKey, "cs", "ProfileForTest");

    String token = TestUtils.getNewToken(ORCHESTRATOR);

    Path projectDir = TestUtils.projectDir(basePath, "ProjectUnderTest");
    ORCHESTRATOR.executeBuild(TestUtils.newScanner(ORCHESTRATOR, projectDir, token)
      .addArgument("begin")
      .setProjectKey(localProjectKey)
      .setProperty("sonar.host.url", "https://localhost:8443")
      .setProjectName("sample")
      .setProperty("sonar.projectBaseDir", Paths.get(projectDir.toAbsolutePath().toString(), "ProjectUnderTest").toString())
      .setProjectVersion("1.0"));

    TestUtils.buildMSBuild(ORCHESTRATOR, projectDir);

    BuildResult result = TestUtils.executeEndStepAndDumpResults(ORCHESTRATOR, projectDir, localProjectKey, token);
    assertTrue(result.isSuccess());
    List<Issue> issues = TestUtils.allIssues(ORCHESTRATOR);
    // 1 * csharpsquid:S1134 (line 34)
    assertThat(issues).hasSize(1);
  }

  void jettyServerSetup() {
    server = new Server();

    // Configure SSL
    SslContextFactory.Server sslContextFactory = new SslContextFactory.Server();
    sslContextFactory.setKeyStorePath("src/test/resources/keystore.p12");
    sslContextFactory.setKeyStorePassword("keystore_password");
    sslContextFactory.setKeyManagerPassword("your_password");
    sslContextFactory.setKeyStoreType("PKCS12");

    // Create an HTTPS connector
    ServerConnector sslConnector = new ServerConnector(server,
      new SslConnectionFactory(sslContextFactory, "http/1.1"));
    sslConnector.setPort(8443); // HTTPS port

    // Add the connector to the server
    server.addConnector(sslConnector);

    // Set up a servlet context
    ServletContextHandler context = new ServletContextHandler(ServletContextHandler.SESSIONS);
    context.setContextPath("/");
    server.setHandler(context);

    // Add a ProxyServlet to forward requests to the orchestrator
    ServletHolder proxyServlet = new ServletHolder(ProxyServlet.class);
    proxyServlet.setInitParameter("proxyTo", ORCHESTRATOR.getServer().getUrl());
    proxyServlet.setInitParameter("prefix", "/");

    context.addServlet(proxyServlet, "/*");
  }
}
