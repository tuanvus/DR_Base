# SerializationBench

Muc dich:

- So sanh nhanh `MessagePack` va `Nino` tren cung mot bo DTO mau.
- Khong cham vao flow `DR_Sever` / `DR_TestClient` net48 hien tai.

Chay:

```powershell
dotnet run --project .\SerializationBench\SerializationBench.csproj -c Release
dotnet run --project .\SerializationBench\SerializationBench.csproj -c Release -- messagepack
dotnet run --project .\SerializationBench\SerializationBench.csproj -c Release -- messagepack-sg
dotnet run --project .\SerializationBench\SerializationBench.csproj -c Release -- messagepack-noalloc
dotnet run --project .\SerializationBench\SerializationBench.csproj -c Release -- nino
```

Neu muon ep ro Tiered PGO ngay trong session benchmark:

```powershell
$env:DOTNET_TieredCompilation = "1"
$env:DOTNET_TieredPGO = "1"
dotnet run --project .\SerializationBench\SerializationBench.csproj -c Release
```

Luu y:

- `DR_Sever` va `DR_TestClient` dang target `.NET Framework 4.8`.
- `Nino` hien tai tren NuGet di theo `Nino.Serialization` va target `netstandard2.1` / `.NET 6+`, nen khong cam truc tiep vao 2 project net48 do.
- Vi vay project nay dung `.NET 10` de benchmark serializer, con wire protocol DarkRift hien tai van giu `MessagePack`.
- `messagepack` = `MessagePackSerializerOptions.Standard` + `Lz4BlockArray`.
- `messagepack-sg` = uu tien formatter tu `SourceGeneratedFormatterResolver`, sau do moi roi sang builtin/attribute resolver can thiet cho primitive.
- `messagepack-noalloc` = duong serialize `MessagePack` ghi vao `ArrayBufferWriter<byte>` de tranh tao `byte[]` moi moi vong benchmark.
