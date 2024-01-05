using iTextSharp.text.pdf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PDFFiller;

internal class Form
{
    public string? HereAfter { get; set; }
    public string? TwoZeroTwo { get; set; }
    public string? DateCompany { get; set; }
    public string? Name { get; set; }
    public string? Title { get; set; }
    public string? CompanyName { get; set; }
    public string? DateMarketer { get; set; }


    public void FillForm(Form form, HttpListenerContext context)
    {

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

        using (var pdfReader = new PdfReader(pdfPath))
        {
            using (var pdfStamper = new PdfStamper(pdfReader, new System.IO.FileStream(modifiedPdfPath, System.IO.FileMode.Create)))
            {
                AcroFields fields = pdfStamper.AcroFields;

                // Set field values for both fields
                SetFormFieldValue(fields, fieldNames[0], form.HereAfter);
                SetFormFieldValue(fields, fieldNames[1], form.TwoZeroTwo);
                SetFormFieldValue(fields, fieldNames[3], form.DateCompany);
                SetFormFieldValue(fields, fieldNames[5], form.Name);
                SetFormFieldValue(fields, fieldNames[6], form.Title);
                SetFormFieldValue(fields, fieldNames[7], form.CompanyName);
                SetFormFieldValue(fields, fieldNames[8], form.DateMarketer);

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

        // Send a response
        HttpListenerResponse response = context.Response;
        string responseString = "{\"Location\": \"stuff done\"}";
        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.Close();

        Console.ReadKey();
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