package org.example;

import org.eclipse.jetty.http.HttpVersion;
import org.eclipse.jetty.proxy.ProxyHandler;
import org.eclipse.jetty.server.HttpConfiguration;
import org.eclipse.jetty.server.HttpConnectionFactory;
import org.eclipse.jetty.server.SecureRequestCustomizer;
import org.eclipse.jetty.server.Server;
import org.eclipse.jetty.server.ServerConnector;
import org.eclipse.jetty.server.SslConnectionFactory;
import org.eclipse.jetty.util.ssl.SslContextFactory;

public class Main {
  public static void main(String[] args) throws Exception {
    Server server = new Server();
    int httpsPort = 9443;

    HttpConfiguration httpConfig = new HttpConfiguration();
    httpConfig.setSecureScheme("https");
    httpConfig.setSecurePort(httpsPort);

    // SSL Context Factory
    SslContextFactory.Server sslContextFactory = new SslContextFactory.Server();
    sslContextFactory.setKeyStorePath("src/main/resources/keystore.p12");
    sslContextFactory.setKeyStorePassword("your_password");
    sslContextFactory.setKeyStoreType("PKCS12");

    // SSL HTTP Configuration
    HttpConfiguration httpsConfig = new HttpConfiguration(httpConfig);
    httpsConfig.addCustomizer(new SecureRequestCustomizer()); // so that servlets can see the
    // encryption details

    // SSL Connector
    ServerConnector sslConnector = new ServerConnector(server,
      new SslConnectionFactory(sslContextFactory, HttpVersion.HTTP_1_1.asString()),
      new HttpConnectionFactory(httpsConfig)); // <-- the argument you were missing
    sslConnector.setPort(httpsPort);
    server.addConnector(sslConnector);

    // Create a ProxyHandler
    ProxyHandler proxyHandler = new ProxyHandler.Reverse("https://localhost:9443", "http://localhost:9000");
    server.setHandler(proxyHandler);

    // Start the server
    server.start();
    server.join();
  }
}
