package org.example;

import org.eclipse.jetty.ee10.servlet.ServletContextHandler;
import org.eclipse.jetty.ee10.servlet.ServletHolder;
import org.eclipse.jetty.proxy.ProxyHandler;
import org.eclipse.jetty.server.Server;
import org.eclipse.jetty.server.ServerConnector;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class Main {
  public static void main(String[] args) throws Exception {
    Server server = new Server();

    // Create an HTTP connector
    ServerConnector connector = new ServerConnector(server);
    connector.setPort(8080); // Port for the proxy server
    server.addConnector(connector);

    // Create a ProxyHandler
    ProxyHandler proxyHandler = new ProxyHandler.Reverse("http://localhost:8080", "http://localhost:9000");
    server.setHandler(proxyHandler);

    // Start the server
    server.start();
    server.join();
  }
}
