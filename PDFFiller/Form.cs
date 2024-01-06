using BoldSign.Api;
using BoldSign.Model;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Net;
using System.Text;

namespace PDFFiller;

internal class Form
{
    public string? HereAfter { get; set; }
    public string? TwoZeroTwo { get; set; }
    public string? DateCompany { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Title { get; set; }
    public string? CompanyName { get; set; }
    public string? DateMarketer { get; set; }


    public void FillPDFAndSendForSign(Form form, HttpListenerContext context)
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
                //var font = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.WINANSI, BaseFont.NOT_EMBEDDED);

                foreach (string fieldName in fieldNames)
                {
                    fields.SetFieldProperty(fieldName, "textsize", 11f, null);
                    //fields.SetFieldProperty(fieldName, "textfont", font, null);
                }

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

        Console.WriteLine("New PDF created");
        //Console.WriteLine("Field Values after setting: ");
        //foreach (var fieldName in fieldNames)
        //{
        //    string fieldValue = GetFormFieldValue(modifiedPdfPath, fieldName);
        //    Console.WriteLine($"Field '{fieldName}': {fieldValue}");
        //}

        // Send a response
        HttpListenerResponse response = context.Response;
        string responseString = "{\"Location\": \"stuff done\"}";
        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.Close();

        // use boldsign api to create/send document for signing
        SendDocumentForSigning(form);
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

    public void SendDocumentForSigning(Form form)
    {
        var apiClient = new ApiClient("https://api.boldsign.com", "*** api key ***");
        var documentClient = new DocumentClient(apiClient);

        var documentFilePath = new DocumentFilePath
        {
            ContentType = "application/pdf",
            FilePath = "myPDF_modified.pdf",
        };

        var filesToUpload = new List<IDocumentFile>
        {
            documentFilePath,
        };

        var signatureField = new FormField(
           id: "sign",
           isRequired: true,
           type: FieldType.Signature,
           pageNumber: 1,
           bounds: new BoldSign.Model.Rectangle(x: 100, y: 8890, width: 350, height: 30));

        var formFieldCollections = new List<FormField>()
        {
            signatureField
        };

        var signer = new DocumentSigner(
          signerName: form.Name,
          signerType: SignerType.Signer,
          signerEmail: form.Email,
          formFields: formFieldCollections,
          locale: Locales.EN);

        var documentSigners = new List<DocumentSigner>()
        {
            signer
        };

        var sendForSign = new SendForSign()
        {
            Message = "please sign this",
            Title = "Affiliate Marketing Agreement",
            HideDocumentId = false,
            Signers = documentSigners,
            Files = filesToUpload,

        };

        try
        {
            var documentCreated = documentClient.SendDocument(sendForSign);
            Console.WriteLine($"BoldSign DocumentCreated response: {documentCreated}");
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"API Exception: {ex.Message}");
        }

        Console.ReadLine();

    }
}