using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace PDFCmd
{
    class Conf
    {
        public bool show_help = false;
        public string source_path;
        public string dest_path;
        public string fields_path;
        public bool discover = false;
        public bool preserve = false;
        public bool debug = false;
        //protected string regex_fields = @"^\/f(ields)?=";
        protected string regex_discover = @"^\/d(iscover)?$";
        protected string regex_preserve = @"^\/p(reserve)?$";
        protected string regex_custom_field= @"^\/data-[-A-Z0-9\]\[_.]{1,50}=.+$";
        public Dictionary<string, string> custom_fields = new Dictionary<string, string>();
        public MessageBag messageBag = new MessageBag();

        public Conf(string[] args)
        {
            if (args.Count() == 0) show_help = true;
            foreach (string arg in args)
            {
                if(arg == "/?" || arg == "/help") show_help = true;
            }
            if (!show_help)
            {
                foreach (string arg in args)
                {

                    if (arg.StartsWith("/"))
                    {
                        if (Regex.IsMatch(arg, regex_discover))
                        {
                            discover = true;
                        }
                        else if (Regex.IsMatch(arg, regex_preserve))
                        {
                            preserve = true;
                        }
                        else if (arg == "/debug")
                        {
                            debug = true;
                        }
                        /**
                        else if (Regex.IsMatch(arg, regex_fields))
                        {
                            string fields = Regex.Replace(arg, regex_fields, "");
                            if (fields_path != null)
                            {
                                messageBag.AddError("Only one fields file may be specfied");
                            }
                            else
                            {
                                fields_path = fields;
                                if (!fields_path.EndsWith(".txt")) messageBag.AddError("Invalid fields file type");
                                if (!File.Exists(fields_path)) messageBag.AddError("Fields file not found");
                            }
                        }*/
                        else if (Regex.IsMatch(arg, regex_custom_field, RegexOptions.IgnoreCase))
                        {
                            string keyval = arg.Trim().Substring(6);
                            string[] f = keyval.Trim().Split('=');
                            string val = f[1].Replace("\"", "").Trim();
                            if (!custom_fields.ContainsKey(f[0])) custom_fields.Add(f[0], val);
                        }
                        else
                        {
                            messageBag.AddError("Invalid Option: " + arg);
                        }
                    }
                    else
                    {
                        if (source_path == null)
                        {
                            validateSource(arg);
                        }
                        else if (dest_path == null)
                        {
                            validateDest(arg);
                        }
                        else
                        {
                            messageBag.AddError("Invalid Option: " + arg);
                        }

                    }
                }
                if (source_path == null) messageBag.AddError("Source PDF is required");
                if (!discover)
                {
                    if (dest_path == null) messageBag.AddError("Destination PDF path is required");
                    if (fields_path == null && custom_fields.Count == 0) messageBag.AddError("Fields args or file is required");
                }
            }

        }

        public bool IsValid
        {
            get {
                if (discover)
                {
                    return messageBag.errors.Count == 0 && source_path != null ? true : false;
                }
                return messageBag.errors.Count == 0 && (fields_path != null || custom_fields.Count > 0) && source_path != null ? true : false;
            }
            set { }
        }

        private void validateSource(string path)
        {
            if (source_path != null)
            {
                messageBag.AddError("Only one source file may be specfied");
            }
            else
            {
                source_path = path.Trim();
                if (!source_path.EndsWith(".pdf")) messageBag.AddError("Invalid source file type");
                if (!File.Exists(source_path)) messageBag.AddError("Source file not found");
            }
        }

        private void validateDest(string path)
        {
            if (dest_path != null)
            {
                messageBag.AddError("Only one destination file may be specfied");
            }
            else
            {
                dest_path = path.Trim();
                if (!dest_path.EndsWith(".pdf")) messageBag.AddError("Invalid destination file name");
                if (File.Exists(dest_path)) messageBag.AddError("Destination file already exists");
            }  
        }

    
    }
}
