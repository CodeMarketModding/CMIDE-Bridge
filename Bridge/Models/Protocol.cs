using System;
namespace CodeMarket.LiveIDE
{
    [Serializable] public class Request { public string command; public string session; public int world; public string path; public string name; public string text; public string expected; public int caret; public bool forced; }
    [Serializable] public class Item { public string label; public string insert; public string detail; public string doc; public int caret; }
    [Serializable] public class Response { public bool ok; public string error; public string session; public int world; public string language; public string[] files; public string[] folders; public string path; public string text; public int from; public string[] items; public string[] referenceItems; public string referenceTitle; public string referenceSubtitle; public string referenceText; public string[] related; }
    [Serializable] public class ReferenceItem { public string section; public string note; public string id; public string chip; public string title; public string kind; public string highlight; public bool unread; }

}
