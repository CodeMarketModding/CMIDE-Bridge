using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JavaMarket.App;
using UnityEngine;

namespace CodeMarket.LiveIDE
{
    internal static class ReferenceOperations
    {
        private static readonly FieldInfo NewsField = typeof(GameApp).GetField("referenceNews", BindingFlags.Instance | BindingFlags.NonPublic);
        private static ReferenceNews News(GameApp app) => NewsField == null ? null : NewsField.GetValue(app) as ReferenceNews;
        private static Response Missing(Response r) { r.ok=false; r.error="The in-game reference is not ready. Load a world first.";return r; }
        public static Response Index(Response r,GameApp app)
        {
            var progress=app.ReferenceProgress;
            if(progress == null)return Missing(r);
            var news=News(app);
            var items=new List<string>();
            foreach(var section in ReferenceCatalog.Sections)
                foreach(var entry in section.Entries)
                    if(entry.IsOpen(progress))
                        items.Add(JsonUtility.ToJson(new ReferenceItem { section=section.Title, note=section.Note, id=entry.Id, chip=entry.Chip, title=entry.Title, kind=entry.Kind.ToString(), highlight=entry.Highlight.ToString(), unread=news != null && news.IsUnread(entry.Id) }));
            r.referenceItems=items.ToArray();return r;
        }
        public static Response Page(Request req,Response r,GameApp app)
        {
            var progress=app.ReferenceProgress;
            if(progress == null)return Missing(r);
            var entry=ReferenceCatalog.Find(req.path);
            if(entry == null || !entry.IsOpen(progress)) { r.ok=false;r.error="That reference entry is unavailable in this world.";return r; }
            r.path=entry.Id;r.referenceTitle=entry.Title;r.referenceSubtitle=entry.Subtitle;r.referenceText=entry.TextIn(progress) ?? "";
            r.related=(entry.SeeAlso ?? new string[0]).Select(ReferenceCatalog.Find).Where(x=>x != null && x.IsOpen(progress)).Select(x=>x.Id).ToArray();
            News(app)?.MarkRead(entry.Id);
            return r;
        }
    }
}
