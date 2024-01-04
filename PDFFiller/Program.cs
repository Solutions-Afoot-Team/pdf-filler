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
                                    // Access the specific property ("Email Primary") and print its value
                                    //string emailPrimary = (string)jsonBody["Name"];
                                    //Console.WriteLine($"Name Primary value: {emailPrimary}");

                                    // Path to your PDF file
                                    string pdfPath = "myPDF.pdf";
                                    string modifiedPdfPath = "myPDF_modified.pdf";

                                    // Specify the field names you want to set a value for
                                    string[] fieldNames = {
                                        "hereinafter referred to as the Company and MARKETER hereinafter referred to as the",
                                        "202 by and between Solutions Afoot LLC",
                                        "Signature",
                                        "Date",
                                        "Signature_2",
                                        "Typed or Printed Name",
                                        "Title",
                                        "Company Name",
                                        "Date_2"
                                    };

                                    JObject jsonBody = JObject.Parse(requestBody);

                                    var hereAfter = (string)jsonBody["hereAfter"];
                                    var twoZeroTwo = (string)jsonBody["202"];

                                    Dictionary<string, string> myValues = new Dictionary<string, string>();
                                    myValues.Add("hereAfter", hereAfter);
                                    myValues.Add("202", twoZeroTwo);
                                    //myValues.Add("Signature", "");
                                    myValues.Add("Date", "date");
                                    //myValues.Add("Signature_2", "");
                                    myValues.Add("Name", "John Doe");
                                    myValues.Add("Title", "Manager");
                                    myValues.Add("Company Name", "John LLC");
                                    myValues.Add("Date_2", "date 2");

                                    using (var pdfReader = new PdfReader(pdfPath))
                                    {
                                        using (var pdfStamper = new PdfStamper(pdfReader, new System.IO.FileStream(modifiedPdfPath, System.IO.FileMode.Create)))
                                        {
                                            AcroFields fields = pdfStamper.AcroFields;

                                            // Set field values for both fields
                                            SetFormFieldValue(fields, fieldNames[0], myValues["hereAfter"]);
                                            SetFormFieldValue(fields, fieldNames[1], myValues["202"]);
                                            SetFormFieldValue(fields, fieldNames[3], myValues["Date"]);
                                            SetFormFieldValue(fields, fieldNames[5], myValues["Name"]);
                                            SetFormFieldValue(fields, fieldNames[6], myValues["Title"]);
                                            SetFormFieldValue(fields, fieldNames[7], myValues["Company Name"]);
                                            SetFormFieldValue(fields, fieldNames[8], myValues["Date_2"]);

                                            // Close the PdfStamper to save changes
                                            pdfStamper.Close();
                                        }
                                    }

                                    // Execute the function
                                    Console.WriteLine("Field Values after setting: ");
                                    foreach (var fieldName in fieldNames)
                                    {
                                        string fieldValue = GetFormFieldValue(modifiedPdfPath, fieldName);
                                        Console.WriteLine($"Field '{fieldName}': {fieldValue}");
                                    }

                                    Console.ReadKey();
                                }
                                catch (JsonException ex)
                                {
                                    Console.WriteLine($"Error parsing JSON: {ex.Message}");
                                }
                                //Console.WriteLine($"Received POST data: {requestBodyJSON}");
                            }

                            // Send a response
                            HttpListenerResponse response = context.Response;
                            string responseString = "{\"Location\": \"stuff done\"}";
                            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                            response.ContentLength64 = buffer.Length;
                            response.OutputStream.Write(buffer, 0, buffer.Length);
                            response.Close();
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

    static void SetFormFieldValue(AcroFields fields, string fieldName, string newValue)
    {
        // Check if the form field exists
        if (fields.Fields.ContainsKey(fieldName))
        {
            // Set the new value for the field
            fields.SetField(fieldName, newValue);
        }
        else
        {
            Console.WriteLine($"Field '{fieldName}' not found.");
        }
    }

    static string GetFormFieldValue(string pdfPath, string fieldName)
    {
        using (var pdfReader = new PdfReader(pdfPath))
        {
            AcroFields fields = pdfReader.AcroFields;

            // Check if the form field exists
            if (fields.Fields.ContainsKey(fieldName))
            {
                return fields.GetField(fieldName);
            }
            else
            {
                Console.WriteLine($"Field '{fieldName}' not found.");
                return null;
            }
        }
    }
}