# SignalR Architecture - Hướng dẫn từ A-Z

## 1. SignalR là gì?

SignalR là một thư viện của ASP.NET Core cho phép giao tiếp **hai chiều** (bidirectional) giữa server và client **real-time**. Khác với AJAX truyền thống (client request -> server response), SignalR cho phép server chủ động gửi dữ liệu đến client mà không cần client request trước.

### Sự khác nhau giữa SignalR và AJAX:

| Đặc điểm | AJAX | SignalR |
|---------|------|---------|
| Hướng giao tiếp | Một chiều (Client → Server) | Hai chiều (Client ↔ Server) |
| Real-time | Client phải polling | Tự động cập nhật ngay |
| Hiệu suất | Nhiều request → chậm | Ít request → nhanh |
| Setup phức tạp | Đơn giản | Cần config Hub |

## 2. Kiến trúc SignalR trong dự án

### A. Server-side (Backend)

#### ProductHub.cs - Trung tâm giao tiếp
```csharp
public class ProductHub : Hub
{
    public async Task SendProductNotify(int productId, string productName, string notificationType)
    {
        // Gửi thông báo đến TẤT CẢ clients đang kết nối
        await Clients.All.SendAsync("ReceiveNotification", notification);
    }
}
```

**Cách hoạt động:**
1. Client kết nối đến Hub thông qua connection `/productHub`
2. Client gọi method `SendProductNotify()` trên server
3. Server xử lý và gầi sự kiện `ReceiveNotification` đến ALL clients

#### NotificationStore - Bộ nhớ lưu thông báo
```csharp
public static class NotificationStore
{
    private static List<ProductNotification> _notifications = new();
    
    public static List<ProductNotification> GetNotifications()
    {
        return _notifications.OrderByDescending(x => x.Timestamp).ToList();
    }
}
```

**Tác dụng:**
- Lưu tất cả thông báo vào bộ nhớ (in-memory cache)
- Khi client vừa kết nối, có thể load lịch sử thông báo ngay lập tức
- Sử dụng lock để tránh race condition (thread-safe)

#### Program.cs - Cấu hình
```csharp
builder.Services.AddSignalR();  // Đăng ký SignalR service
app.MapHub<ProductHub>("/productHub");  // Route đến Hub
```

### B. Client-side (Frontend JavaScript)

#### 1. Kết nối đến Hub
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/productHub")                    // Đường dẫn Hub
    .withAutomaticReconnect()                  // Tự động reconnect khi mất kết nối
    .build();
```

**Giải thích:**
- `HubConnectionBuilder()`: Xây dựng connection object
- `withUrl()`: URL của Hub server
- `withAutomaticReconnect()`: Nếu kết nối bị mất, tự động kết nối lại

#### 2. Lắng nghe sự kiện (Listening)
```javascript
connection.on("ReceiveNotification", (notification) => {
    console.log('Nhận được thông báo:', notification);
    // notification = { productId, productName, notificationType, timestamp }
});
```

**Cách hoạt động:**
- Server gọi `Clients.All.SendAsync("ReceiveNotification", ...)`
- Client JavaScript lắng nghe sự kiện này thông qua `.on()`
- Callback function được gọi ngay khi nhận dữ liệu

#### 3. Khởi động kết nối
```javascript
connection.start().catch(err => console.error(err));
```

**Tác dụng:**
- Thiết lập WebSocket connection với server
- Nếu lỗi, log lỗi ra console

## 3. Luồng hoạt động CRUD với SignalR

### Create (Thêm sản phẩm)

```
1. User click "Create Product" → Trang Create.cshtml
2. User điền dữ liệu, click "Create"
3. Form submit POST → Create.cshtml.cs
4. Backend:
   - Validate dữ liệu (asp-validation)
   - Lưu vào database
   - Gọi: await _hubContext.Clients.All.SendAsync("ReceiveNotification", {...})
5. Tất cả clients kết nối:
   - Nhận sự kiện thông qua connection.on("ReceiveNotification")
   - Cập nhật giao diện (reload danh sách sản phẩm)
   - Hiển thị thông báo "Product added: Laptop"
```

### Update (Cập nhật sản phẩm)

```
1. User click "Edit" → Trang Edit.cshtml
2. Trang load dữ liệu sản phẩm hiện tại
3. User thay đổi, click "Save"
4. Form submit POST → Edit.cshtml.cs
5. Backend:
   - Validate (asp-validation)
   - Update database
   - Gọi SendAsync("ReceiveNotification") với notificationType = "Updated"
6. Tất cả clients:
   - Nhận thông báo update
   - Reload danh sách
```

### Delete (Xóa sản phẩm)

```
1. User click "Delete" → Xác nhận dialog
2. JavaScript gọi fetch('/Products?handler=Delete&id=123')
3. Backend:
   - Xóa khỏi database
   - Gọi SendAsync("ReceiveNotification") với notificationType = "Deleted"
4. Danh sách tự động cập nhật (không cần reload trang)
5. Thông báo hiển thị: "Deleted: ProductName"
```

## 4. Quy trình truyền dữ liệu chi tiết

### Scenario: Thêm sản phẩm mới

```
CLIENT A                           SERVER                        CLIENT B
(Browser 1)                                                    (Browser 2)
    |                                |                              |
    |--1. POST /Products/Create----->|                              |
    |   (ProductName: "Laptop")       |                              |
    |                                |--2. Validate & Save          |
    |                                |   to Database                 |
    |                                |                              |
    |                                |--3. AddNotification()         |
    |                                |   (Save to NotificationStore)|
    |                                |                              |
    |                                |--4. Clients.All.SendAsync()--|
    |<--5. ReceiveNotification--------|-4b. ReceiveNotification------|
    |   {notificationType: "Added"}   |                              |
    |                                |                              |
    |--6. loadProducts()------------->|                              |
    |   fetch GetProducts             |                              |
    |<--7. JSON [...]                 |                              |
    |                                |                              |
    |--8. renderProducts()            |--9. renderProducts()---------|
    |   Update DOM table              |   (ClientB auto-update)      |
```

## 5. Các Class và Method quan trọng

### ProductHub Methods

```csharp
await Clients.All.SendAsync(...)           // Gửi đến TẤT CẢ clients
await Clients.Caller.SendAsync(...)        // Gửi chỉ client gọi
await Clients.Others.SendAsync(...)        // Gửi đến clients khác (không gồi người gọi)
await Clients.Group("GroupName").SendAsync(...) // Gửi đến group cụ thể
```

### JavaScript Connection Methods

```javascript
connection.on("EventName", (data) => {...})   // Lắng nghe sự kiện
connection.start()                            // Khởi động connection
connection.stop()                             // Dừng connection
connection.invoke("MethodName", param)        // Gọi method server từ client
```

## 6. NotificationStore - Lưu lịch sử thông báo

```csharp
public static class NotificationStore
{
    private static List<ProductNotification> _notifications = new();
    private static object _lock = new object();  // Thread-safe
    
    // Lấy tất cả thông báo, sắp xếp mới nhất trước
    public static List<ProductNotification> GetNotifications()
    {
        lock (_lock)
        {
            return _notifications.OrderByDescending(x => x.Timestamp).ToList();
        }
    }
    
    // Thêm thông báo mới, giữ max 100 thông báo
    public static void AddNotification(ProductNotification notification)
    {
        lock (_lock)
        {
            _notifications.Add(notification);
            if (_notifications.Count > 100)
                _notifications.RemoveAt(0);  // Xóa cũ nhất
        }
    }
}
```

## 7. Validation (asp-validation)

### Server-side (Create.cshtml.cs / Edit.cshtml.cs)

```csharp
public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid)  // Kiểm tra validate
        return Page();        // Trả về form với lỗi
    
    // Lưu dữ liệu...
    return RedirectToPage("Index");
}
```

### Client-side (Create.cshtml / Edit.cshtml)

```html
<!-- Hiển thị lỗi chung -->
<div asp-validation-summary="ModelOnly" style="color:red;"></div>

<!-- Hiển thị lỗi từng field -->
<input asp-for="Product.ProductName" />
<span asp-validation-for="Product.ProductName" style="color:red;"></span>
```

**Tác dụng:**
- `asp-validation-summary`: Liệt kê tất cả lỗi
- `asp-validation-for`: Hiển thị lỗi từng property

## 8. Notifications Page - Trang xem tất cả sự kiện

**File:** `/Pages/Notifications/Index.cshtml`

```html
<table>
    <tr>
        <th>Time</th>
        <th>Product ID</th>
        <th>Product Name</th>
        <th>Type</th>
    </tr>
    <tbody id="notificationsTable"></tbody>
</table>
```

**JavaScript hoạt động:**
1. Load lịch sử thông báo: `fetch('/Notifications?handler=GetNotifications')`
2. Lắng nghe sự kiện mới: `connection.on("ReceiveNotification")`
3. Cập nhật bảng real-time: `renderNotifications()`

## 9. So sánh SignalR vs Blazor vs AJAX

| Công nghệ | Mô tả | Dùng khi nào |
|-----------|-------|------------|
| **AJAX** | Request/Response truyền thống | Trang tĩnh, ít cập nhật |
| **SignalR** | Real-time 2 chiều, JavaScript | Cần real-time, muốn tận dụng Razor Pages |
| **Blazor** | Viết client logic bằng C#, không cần JS | Team không quen JS, cần logic phức tạp |

**Dự án này:** Dùng **SignalR + Razor Pages** vì:
- Cần real-time (CRUD notifications)
- Razor Pages đã là nền tảng
- JavaScript đơn giản (không logic phức tạp)
- Không cần Blazor (overhead lớn)

## 10. Tổng kết

**SignalR = Real-time Server-to-Client Communication**

1. **Server** tạo **Hub** để gửi sự kiện
2. **Client** JavaScript kết nối đến Hub
3. Khi CRUD xảy ra, server gửi sự kiện cho **TẤT CẢ clients**
4. Giao diện **tự động cập nhật** mà không reload trang
5. **NotificationStore** lưu lịch sử -> trang Notifications hiển thị

**Advantage:**
- ✅ Tự động cập nhật real-time
- ✅ Tốn ít resource (1 connection)
- ✅ Hiệu ứng UX tốt
- ✅ Đơn giản và native với ASP.NET

**Disadvantage:**
- ❌ Phức tạp hơn AJAX
- ❌ Cần JavaScript
- ❌ Khó scale với 1000+ users (session memory)

