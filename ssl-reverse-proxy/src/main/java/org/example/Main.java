package org.example;

import org.eclipse.jetty.proxy.ProxyServlet;
import org.eclipse.jetty.server.Server;
import org.eclipse.jetty.server.ServerConnector;
import org.eclipse.jetty.servlet.ServletContextHandler;
import org.eclipse.jetty.servlet.ServletHolder;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class Main {
  private static final Logger logger = LoggerFactory.getLogger(Main.class);

  public static void main(String[] args) throws Exception {
    Server server = new Server();

    // Create an HTTP connector
    ServerConnector connector = new ServerConnector(server);
    connector.setPort(8080); // Port for the proxy server
    server.addConnector(connector);

    // Set up a servlet context
    ServletContextHandler context = new ServletContextHandler(ServletContextHandler.SESSIONS);
    context.setContextPath("/");
    server.setHandler(context);

    // Add a ProxyServlet to forward requests to the target service
    ServletHolder proxyServlet = new ServletHolder(ProxyServlet.class);
    proxyServlet.setInitParameter("proxyTo", "http://localhost:9000"); // Target service URL
    proxyServlet.setInitParameter("prefix", "/");

    context.addServlet(proxyServlet, "/*");

    // Start the server
    server.start();
    server.join();
  }
}
