using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using iText.Kernel;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using iText.IO;
using iText.Forms;
using iText.Forms.Fields;
using iText.Forms.Util;


namespace PDFCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            Conf conf = new Conf(args);
            if (conf.show_help)
            {
                help();
                return;
            }
            if (!conf.IsValid)
            {
                conf.messageBag.PrintErrors();
                return;
            }
            if (conf.discover)
            {
                discover(conf);
                return;
            }
            InputFile filein = new InputFile(conf.fields_path);
            if (conf.fields_path != null && filein.fields.Count == 0)
            {
                new MessageBag("error", "No input fields provided in fields file").PrintErrors();
                return;
            }
            
            try
            {
                PdfReader pdfReader = new PdfReader(conf.source_path);
                PdfWriter pdfWriter = new PdfWriter(conf.dest_path);
                PdfDocument doc = new PdfDocument(pdfReader, pdfWriter);
                PdfAcroForm form = PdfAcroForm.GetAcroForm(doc, true);
                IDictionary<string, PdfFormField> fields = form.GetFormFields();
                if (conf.fields_path != null)
                {
                    foreach (KeyValuePair<string, string> kvp in filein.fields)
                    {
                        string field_name = Regex.Replace(kvp.Key, @"^(circle|check|square|star|cross)-", "", RegexOptions.IgnoreCase);
                        if (fields.ContainsKey(field_name))
                        {
                            Console.Write(field_name);
                            PdfFormField field;
                            fields.TryGetValue(kvp.Key, out field);
                            field.SetValue(kvp.Value);
                        }
                    }
                }
                foreach (KeyValuePair<string, string> kvp in conf.custom_fields)
                {
                    string field_name = Regex.Replace(kvp.Key, @"^(circle|check|square|star|cross)-", "", RegexOptions.IgnoreCase);
                    if (fields.ContainsKey(field_name))
                    {
                        string t = form.GetField(field_name).GetFormType().ToString();
                        switch (t)
                        {
                            case "/Btn":
                                if(Regex.IsMatch(kvp.Value, @"^(yes|on|1|true)$", RegexOptions.IgnoreCase))
                                {
                                    if (Regex.IsMatch(kvp.Key,@"^(circle|check|square|star|cross)-.*",RegexOptions.IgnoreCase))
                                    {
                                        int checktype = PdfFormField.TYPE_CROSS;
                                        if (kvp.Key.StartsWith("circle")) checktype = PdfFormField.TYPE_CIRCLE;
                                        if (kvp.Key.StartsWith("check")) checktype = PdfFormField.TYPE_CHECK;
                                        if (kvp.Key.StartsWith("square")) checktype = PdfFormField.TYPE_SQUARE;
                                        if (kvp.Key.StartsWith("star")) checktype = PdfFormField.TYPE_STAR;
                                        form.GetField(field_name).SetCheckType(checktype).SetValue(kvp.Value, true);
                                    }
                                }
                                break;
                            default:
                                form.GetField(field_name).SetValue(kvp.Value);
                                break;
                        }                                      
                    }
                }
                if (!conf.preserve) form.FlattenFields();
                doc.Close();
                if (File.Exists(conf.dest_path))
                {
                    new MessageBag("message", conf.dest_path).PrintMessages();
                }
                else
                {
                    new MessageBag("error","Process failed").PrintErrors();
                }
            }
            catch (Exception e)
            {
                string message = conf.debug ? e.Message + Environment.NewLine + e.StackTrace + e.TargetSite : "Error";
                new MessageBag("error", message).PrintErrors();
                return;
            }


        }

        private static void discover(Conf conf)
        {
            try
            {
                PdfReader pdfReader = new PdfReader(conf.source_path);
                PdfDocument doc = new PdfDocument(pdfReader);
                PdfAcroForm form = PdfAcroForm.GetAcroForm(doc, false);
                if(form == null)
                {
                    Console.WriteLine("Document has no form");
                    return;
                }
                Console.WriteLine("Type\tName\t\t\tValue");
                Console.WriteLine("------\t-----\t\t\t------");
                foreach (KeyValuePair<string, PdfFormField> kvp in form.GetFormFields())
                {
                    if (kvp.Value != null && kvp.Value.GetFormType() != null)
                    {
                        string val = kvp.Value.GetValueAsString();
                        string fieldtype = kvp.Value.GetFormType().ToString() ?? "";
                        Console.WriteLine("{0}\t{1}\t\t\t{2}", fieldtype, kvp.Key, val);
                    }
                }
            }
            catch (Exception e)
            {
                string message = conf.debug ? e.Message + Environment.NewLine + e.StackTrace : "Error discovering";
                new MessageBag("error", message).PrintErrors();
            }
        }

        private static void help()
        {
            Console.WriteLine(Environment.NewLine + " PDF Form Filler. Joshua R. Thomas 2018"+Environment.NewLine);
            string text =  " Usage: "+ System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.ToUpper() + " [/?] [/d] [/data[-{s}]-{s}={s}] [/p] source destination"
                +Environment.NewLine + Environment.NewLine
                + " Args:" + Environment.NewLine
                + " /? [/help] \t\t\t\t\t Show this help" + Environment.NewLine
                + " /d [/discover] \t\t\t\t Show form fields in PDF file" + Environment.NewLine
                + " /data-{fieldname}={string} \t\t\t Fill field with value specified (overrides duplicate field file value)" + Environment.NewLine
                + " /data-{checktype}-{fieldname}={string} \t Set checkbox value and type (circle, square, check, star, cross)" + Environment.NewLine
                + " /p [/preserve ]\t\t\t\t Preserve fields, do not flatten" + Environment.NewLine;
            Console.WriteLine(text);
        }
    }
}
