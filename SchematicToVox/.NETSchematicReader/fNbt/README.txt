Named Binary Tag (NBT) is a structured binary file format used by Minecraft.
fNbt is a small library, written in C# for .NET 3.5+. It provides functionality
to create, load, traverse, modify, and save NBT files and streams.

Current released version is 0.6.3 (12 April 2015).

fNbt is based in part on Erik Davidson's (aphistic's) original LibNbt library,
now completely rewritten by Matvei Stefarov (fragmer).

Note that fNbt.Test.dll and nunit.framework.dll do NOT need to be bundled with
applications that use fNbt; they are only used for testing.


==== FEATURES =================================================================
- Load and save uncompressed, GZip-, and ZLib-compressed files/streams.
- Easily create, traverse, and modify NBT documents.
- Simple indexer-based syntax for accessing compound, list, and nested tags.
- Shortcut properties to access tags' values without unnecessary type casts.
- Compound tags implement ICollection<T> and List tags implement IList<T>, for
    easy traversal and LINQ integration.
- Good performance and low memory overhead.
- Built-in pretty-printing of individual tags or whole files.
- Every class and method are fully documented, annotated, and unit-tested.
- Can work with both big-endian and little-endian NBT data and systems.
- Optional high-performance reader/writer for working with streams directly.


==== DOWNLOAD =================================================================
Latest version of fNbt requires .NET Framework 3.5+ (client or full profile).

 Package @ NuGet:  https://www.nuget.org/packages/fNbt/

 Compiled binary:  https://fcraft.net/fnbt/fNbt_v0.6.3.zip
                   SHA1: c743b4e8af649af75d7ab097dfec2bc897a70689

 Amalgamation (single source file):
    Non-annotated: https://fcraft.net/fnbt/fNbt_v0.6.3.cs
                   SHA1: 2a2cd74bee0bab765d9647e0cc048927248d0142
        Annotated: https://fcraft.net/fnbt/fNbt_v0.6.3_Annotated.cs
                   SHA1: 66ab908708678272a58d9e71f1615f68caba79d7
                   (using JetBrains.Annotations, for ReSharper)


==== EXAMPLES =================================================================
- Loading a gzipped file:
    var myFile = new NbtFile();
    myFile.LoadFromFile("somefile.nbt.gz");
    var myCompoundTag = myFile.RootTag;

- Accessing tags (long/strongly-typed style):
    int intVal = myCompoundTag.Get<NbtInt>("intTagsName").Value;
    string listItem = myStringList.Get<NbtString>(0).Value;
    byte nestedVal = myCompTag.Get<NbtCompound>("nestedTag")
                              .Get<NbtByte>("someByteTag")
                              .Value;

- Accessing tags (shortcut style):
    int intVal = myCompoundTag["intTagsName"].IntValue;
    string listItem = myStringList[0].StringValue;
    byte nestedVal = myCompTag["nestedTag"]["someByteTag"].ByteValue;

- Iterating over all tags in a compound/list:
    foreach( NbtTag tag in myCompoundTag.Values ){
        Console.WriteLine( tag.Name + " = " + tag.TagType );
    }
    foreach( string tagName in myCompoundTag.Names ){
        Console.WriteLine( tagName );
    }
    for( int i=0; i<myListTag.Count; i++ ){
        Console.WriteLine( myListTag[i] );
    }
    foreach( NbtInt intListItem in myIntList.ToArray<NbtInt>() ){
        Console.WriteLine( listIntItem.Value );
    }

- Constructing a new document
    var serverInfo = new NbtCompound("Server");
    serverInfo.Add( new NbtString("Name", "BestServerEver") );
    serverInfo.Add( new NbtInt("Players", 15) );
    serverInfo.Add( new NbtInt("MaxPlayers", 20) );
    var serverFile = new NbtFile(serverInfo);
    serverFile.SaveToFile( "server.nbt", NbtCompression.None );

- Constructing using collection initializer notation:
    var compound = new NbtCompound("root"){
        new NbtInt("someInt", 123),
        new NbtList("byteList") {
            new NbtByte(1),
            new NbtByte(2),
            new NbtByte(3)
        },
        new NbtCompound("nestedCompound") {
            new NbtDouble("pi", 3.14)
        }
    };

- Pretty-printing file structure:
    Console.WriteLine( myFile.ToString("\t") );
    Console.WriteLine( myRandomTag.ToString("    ") );

- Check out unit tests in fNbt.Test for more examples.


==== API REFERENCE ============================================================
Online reference can be found at http://www.fcraft.net/fnbt/v0.6.3/


==== LICENSING ================================================================
fNbt v0.5.0+ is licensed under 3-Clause BSD license. See ./docs/LICENSE
LibNbt2012 up to and including v0.4.1 kept LibNbt's original license (LGPLv3).
fNbt makes use of the NUnit testing framework (www.nunit.org)


==== VERSION HISTORY ==========================================================
See ./docs/Changelog


==== OLD VERSIONS =============================================================
If you need .NET 2.0 support, stick to using fNbt version 0.5.1.
Note that this 0.5.x branch of fNbt is no longer supported or updated.

 Compiled binary:  http://fcraft.net/fnbt/fNbt_v0.5.1.zip

 Amalgamation (single source file):
    Non-annotated: http://fcraft.net/fnbt/fNbt_v0.5.1.cs
        Annotated: http://fcraft.net/fnbt/fNbt_v0.5.1_Annotated.cs
                   (using JetBrains.Annotations, for ReSharper)
