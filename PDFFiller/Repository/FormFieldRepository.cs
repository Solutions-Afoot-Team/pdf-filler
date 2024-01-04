using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFFiller.Repository;

internal class FormFieldRepository
{
    public static void SetFormFieldValue(AcroFields fields, string fieldName, string newValue)
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

    public static string GetFormFieldValue(string pdfPath, string fieldName)
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
