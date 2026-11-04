# M0 — Nhật ký kiểm chứng

## Đã xác nhận trước triển khai

- Unity 6000.6.8f1 và Windows build support đã cài.
- Project dùng URP17.6.0, Input System1.20.1, uGUI2.6.0.
- VS Code có Unity extension1.3.2, C#2.160.4 và C# Dev Kit3.40.215.
- Cấu hình Attach to Unity có sẵn. Chưa chứng minh breakpoint hoạt động.
- Log Editor xác nhận GTX1650, DX12, VRAM khoảng3962MB.
- Scene mẫu ban đầu giữ nguyên.

## Kiểm tra cần hoàn tất trong M0

- [x] Unity compile Core/Runtime/Editor không lỗi.
- [x] Build Windows tạo thành công từ scene Bootstrap.
- [ ] Bản build chạy có hình; ghi nhận GPU, độ phân giải, scene.
- [ ] Nút DEBUG PROBE phản hồi.
- [ ] VS Code attach và breakpoint thực sự dừng.
- [x] Concept được tạo và gửi người dùng duyệt (chưa nhận xác nhận phong cách).

## Kết quả chạy ngày 04/11/2026

- Build báo `Succeeded; errors=0; warnings=0`, batch Unity thoát mã 0.
- Đã chạy Windows Player trên GTX1650, 1920×1080, scene Bootstrap; cả DX12
  và lần đối chiếu DX11 đều ghi được báo cáo runtime rồi tự kết thúc.
- DX12 báo `Failed to capture screen shot`; DX11 xuất được PNG nhưng ảnh toàn đen.
  Hai lần khởi chạy đều dùng cửa sổ ẩn. Chưa xác định do chế độ cửa sổ/chụp ảnh
  hay render của scene; không đánh dấu đạt kiểm tra hình ảnh.
- Báo cáo và log cục bộ nằm trong `Logs/M0` (không đưa vào Git).
- Chưa kiểm tra được nút DEBUG PROBE hoặc breakpoint bằng thao tác thực tế.

### Kiểm tra thủ công còn cần

1. Mở `Build/Windows-M0/QuaThangNayDa-M0.exe` bằng Explorer, không chạy ẩn.
2. Xác nhận có nền xanh, sàn/cube và bảng M0; nếu đen hãy báo lại hoặc gửi ảnh.
3. Bấm DEBUG PROBE: số đếm phải tăng mỗi lần bấm.
4. Trong Unity mở menu M0, Play; VS Code Attach to Unity và đặt breakpoint
   trong `DebugProbe` theo README. Xác nhận debugger dừng khi bấm nút.

M0 chưa nghiệm thu đầy đủ cho tới khi các kiểm tra còn lại đạt.

## Giới hạn công cụ

Computer Use tìm thấy cửa sổ Unity và VS Code nhưng lần chụp gặp timeout;
lần phục hồi báo hết thời gian chờ quyền truy cập ứng dụng. Chưa điều khiển được Editor/VS Code.
Không cài cầu nối bên ngoài. Công cụ Editor M0 là menu cục bộ và entry point batch,
không có mạng/listener hay tự chạy khi import.

Build kiểm tra dùng bản sao project trong .m0-validation để không đụng scene đang mở.
Development build và scene chẩn đoán không chứng minh chất lượng đồ họa hay FPS gameplay.
