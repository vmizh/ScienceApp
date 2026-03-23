using System;
using System.Diagnostics;
using System.Windows;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WPFCore.Layout;

[DebuggerDisplay("'{_id}' Name")]
public class Layout
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid _id { get; set; }

    public double FormHeight { set; get; }
    public double FormWidth { set; get; }
    public double FormLeft { set; get; }
    public double FormTop { set; get; }

    public WindowStartupLocation FormStartLocation { set; get; }
    public WindowState FormState { set; get; }

    public string? LayoutString { set; get; }
}
