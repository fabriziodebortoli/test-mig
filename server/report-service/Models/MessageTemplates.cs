
using Newtonsoft.Json;
using System;

namespace Microarea.RSWeb.Models
{
    public class AskDialogElement
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class HotlinkDescr
    {
        public string ns { get; set; }
        public string filter { get; set; }
        public string selection_type { get; set; }
        public string name { get; set; }
    }

    public class Snapshot
    {
        public Info info;
        public object[] pages;
    }

    public class Info
    {
        public string report_title { get; set; }
        public string name_snapshot { get; set; }
        public string date_snapshot { get; set; }
    }

}