# DR_Base Architecture & Conventions

> **Mục đích**: File này giúp AI (Claude, Cursor, Codex...) hiểu nhanh cấu trúc project mà không cần scan lại toàn bộ codebase.

---

## 1. Project Structure

```
DR_Base/
├── Runner/
│   ├── Common/          → DR.Common     (.NET 4.8)  [Networking + OperationHandler]
│   ├── Define/          → DR.Define     (.NET 4.8)  [DTO + Enums cơ bản]
│   ├── Dto/             → DR.Dto        (.NET 4.8)  [DTO + MessagePack SourceGen]
│   ├── Enum/            → DR.Enum       (.NET 4.8)  [Enum chung]
│   ├── Share/           → DR.Share      (.NET 4.8)  [Shared utilities]
│   ├── DR_Sever/        → DR_Game       (Plugin)    [DarkRift Server Plugin]
│   ├── DR_TestClient/   → Test console  (Client)    [Test client]
│   └── Hotfix/          → Hotfix system
│
├── Deploy Server/
│   └── Lib/             → Nơi copy DLL sau build
│
├── build-libs.bat       → Build 4 DLL shared (Define+Dto+Enum+Share)
├── build-msgpack.bat    → Chỉ build MessagePack (DR.Dto)
└── ARCHITECTURE.md      → File này
```

---

## 2. Naming Conventions (BẮT BUỘC)

### DTO (Request / Response)

| Loại | Quy tắc | Ví dụ |
|------|---------|-------|
| Client → Server | `XxxRequestDto` | `UserLoginRequestDto`, `DemoPingRequestDto` |
| Server → Client | `XxxResponseDto` | `UserLoginResponseDto`, `DemoPingResponseDto` |

**Lưu ý**:
- Tất cả DTO nằm trong `DR.Dto`
- Không đặt DTO trong `DR.Define` nữa
- MessagePack Source Generator **chỉ build ở `DR.Dto`**

### Project Prefix

- `DR.Common.*`
- `DR.Define.*`
- `DR.Dto.*`
- `DR.Enum.*`
- `DR.Share.*`

---

## 3. MessagePack Strategy

- **Chỉ** `DR.Dto` có `MessagePack.SourceGenerator`
- `DR.Define` chỉ chứa enum + type cơ bản (không có DTO)
- Client và Server đều reference `DR.Dto.dll` để dùng chung formatter

---

## 4. Build Scripts

| File | Mục đích |
|------|----------|
| `build-libs.bat` | Build 4 DLL: Define, Dto, Enum, Share |
| `build-msgpack.bat` | Chỉ build DR.Dto (MessagePack) |

Sau khi build, copy DLL vào `Deploy Server\Lib\`

---

## 5. Quick Reference cho AI

Khi cần thêm DTO mới:

1. Tạo class trong `DR.Dto` theo quy tắc `XxxRequestDto` / `XxxResponseDto`
2. Chạy `build-msgpack.bat` để generate formatter
3. Copy DLL vào `Deploy Server\Lib\`

Khi cần sửa logic network:

- `DR.Common` → Networking + OperationHandler
- `DR.Dto` → DTO + MessagePack

---

**Last updated**: 2026-06-09
