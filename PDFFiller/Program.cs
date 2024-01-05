using System;
using System.Reflection;
using System.Reflection.PortableExecutable;
using iTextSharp.text.pdf;
using System.IO;
using System.Reflection;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PDFFiller;

class Program
{
    static void Main()
    {
        // Define the base URL and route
        string baseUrl = "http://localhost:5000/";
        string route = "doStuff";

        // Create a listener
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add(baseUrl);

        try
        {
            // Start the listener
            listener.Start();
            Console.WriteLine($"Listening on {baseUrl}");

            // Handle requests in a separate thread
            ThreadPool.QueueUserWorkItem((o) =>
            {
                while (listener.IsListening)
                {
                    try
                    {
                        // Wait for a request
                        HttpListenerContext context = listener.GetContext();
                        HttpListenerRequest request = context.Request;

                        if (request.HttpMethod == "POST" && request.Url.LocalPath == $"/{route}")
                        {
                            // Read the request body
                            using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
                            {
                                string requestBody = reader.ReadToEnd();
                                try
                                {

                                    JObject jsonBody = JObject.Parse(requestBody);

                                    var form = new Form()
                                    {
                                        HereAfter = (string)jsonBody["hereAfter"],
                                        TwoZeroTwo = (string)jsonBody["202"],
                                        DateCompany = (string)jsonBody["dateCompany"],
                                        Name = (string)jsonBody["name"],
                                        Title = (string)jsonBody["title"],
                                        CompanyName = (string)jsonBody["companyName"],
                                        DateMarketer = (string)jsonBody["dateMarketer"]
                                    };

                                    form.FillForm(form, context);

                                }
                                catch (JsonException ex)
                                {
                                    Console.WriteLine($"Error parsing JSON: {ex.Message}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            });

            // Keep the console application running
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            // Stop the listener when done
            listener.Stop();
        }

    }
}