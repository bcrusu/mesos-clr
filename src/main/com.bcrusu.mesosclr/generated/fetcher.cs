//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: mesos/fetcher/fetcher.proto
// Note: requires additional types generated from: mesos/mesos.proto
namespace mesos.fetcher
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"FetcherInfo")]
  public partial class FetcherInfo : global::ProtoBuf.IExtensible
  {
    public FetcherInfo() {}
    
    private string _sandbox_directory;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"sandbox_directory", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string sandbox_directory
    {
      get { return _sandbox_directory; }
      set { _sandbox_directory = value; }
    }
    private string _cache_directory = "";
    [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name=@"cache_directory", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string cache_directory
    {
      get { return _cache_directory; }
      set { _cache_directory = value; }
    }
    private readonly global::System.Collections.Generic.List<mesos.fetcher.FetcherInfo.Item> _items = new global::System.Collections.Generic.List<mesos.fetcher.FetcherInfo.Item>();
    [global::ProtoBuf.ProtoMember(3, Name=@"items", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<mesos.fetcher.FetcherInfo.Item> items
    {
      get { return _items; }
    }
  
    private string _user = "";
    [global::ProtoBuf.ProtoMember(4, IsRequired = false, Name=@"user", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string user
    {
      get { return _user; }
      set { _user = value; }
    }
    private string _frameworks_home = "";
    [global::ProtoBuf.ProtoMember(5, IsRequired = false, Name=@"frameworks_home", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string frameworks_home
    {
      get { return _frameworks_home; }
      set { _frameworks_home = value; }
    }
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"Item")]
  public partial class Item : global::ProtoBuf.IExtensible
  {
    public Item() {}
    
    private mesos.CommandInfo.URI _uri;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"uri", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public mesos.CommandInfo.URI uri
    {
      get { return _uri; }
      set { _uri = value; }
    }
    private mesos.fetcher.FetcherInfo.Item.Action _action;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"action", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public mesos.fetcher.FetcherInfo.Item.Action action
    {
      get { return _action; }
      set { _action = value; }
    }
    private string _cache_filename = "";
    [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name=@"cache_filename", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string cache_filename
    {
      get { return _cache_filename; }
      set { _cache_filename = value; }
    }
    [global::ProtoBuf.ProtoContract(Name=@"Action")]
    public enum Action
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"BYPASS_CACHE", Value=0)]
      BYPASS_CACHE = 0,
            
      [global::ProtoBuf.ProtoEnum(Name=@"DOWNLOAD_AND_CACHE", Value=1)]
      DOWNLOAD_AND_CACHE = 1,
            
      [global::ProtoBuf.ProtoEnum(Name=@"RETRIEVE_FROM_CACHE", Value=2)]
      RETRIEVE_FROM_CACHE = 2
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}