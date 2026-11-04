# Qua Tháng Này Đã — Unity

Chuyển thể theo hướng B từ demo web 0.14. Windows offline, góc nhìn thứ nhất,
stylized bán hiện thực. Bản đầu là một buổi sáng 25–35 phút, có xe máy và hai nhánh cam kết.

## Hiện trạng

M0: người dùng đã xác nhận render, nút thử và breakpoint trong Editor.
M1: đi bộ/lên xe đã được thử, sửa lỗi ma sát xe và camera; người dùng phản hồi đã mượt.
M2: đã build lát cắt đồng hồ/lịch/chỉ số/ăn/nghỉ/học/làm; kiểm tra luật tự động đạt,
chờ chơi thử tương tác và hình ảnh. Chưa có tình huống, lưu tiến trình hoặc mỹ thuật chính thức.
Không sử dụng model từ ZIP tham khảo khi chưa kiểm tra nguồn và quyền sử dụng.

## Mở và chạy M0

1. Mở project bằng Unity **6000.6.8f1**.
2. Chờ compile, chọn **Qua Thang Nay Da → M0 → Create or Open Foundation Scene**.
3. Bấm Play. Bảng M0 cho biết GPU/API/độ phân giải, có nút DEBUG PROBE.
4. Để debug: mở [FoundationDiagnostics.cs](Assets/_Project/Scripts/Runtime/Bootstrap/FoundationDiagnostics.cs),
   đặt breakpoint trong `DebugProbe`, chọn **Attach to Unity** ở VS Code rồi bấm nút trong game.

Nền/cube chỉ dùng kiểm tra render, không phải cảnh chính thức. SampleScene được giữ nguyên.
M0 dùng IMGUI cho bảng chẩn đoán; HUD gameplay về sau dùng uGUI/TextMeshPro.

## Buổi sáng thử M2

Chọn **Qua Thang Nay Da → M2 → Create or Open Morning Test** (ngoài Play Mode).
Đọc [cách chơi và giới hạn M2](Docs/M2-Verification.md). F tương tác bảng, Tab lịch/nhật ký,
Esc tạm dừng. Chưa có tình huống M3 hoặc save/load M4.
Build riêng bằng task **M2: Build Windows (isolated)**.

## Khu thử M1 (điều khiển)

Tắt Play Mode rồi chọn **Qua Thang Nay Da → M1 → Create or Open Travel Test**.
Chơi thử theo [hướng dẫn và checklist M1](Docs/M1-Verification.md).
Task **M1: Build Windows (isolated)** tái sử dụng cache validation của M0,
xuất riêng `Build/Windows-M1/QuaThangNayDa-M1.exe`, không ghi đè bản M0.

## Build M0

Chạy VS Code task **M0: Build Windows (isolated)** hoặc:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Build-M0.ps1
```

Script sao chép Assets/Packages/ProjectSettings vào `.m0-validation`, gọi Unity batch,
không đóng Editor đang mở, không sửa scene chưa lưu. Log ở `Logs/M0`, build ở
`Build/Windows-M0/QuaThangNayDa-M0.exe`. Gửi **cả thư mục build**, không chỉ EXE.
Đây là Development Build để kiểm tra nền, không dùng để công bố hiệu năng gameplay.

Không tự nâng Unity/package, không có dịch vụ mạng hoặc công cụ lắng nghe từ xa được thêm vào.

## Tài liệu

- [Yêu cầu và kiến trúc đã duyệt](Docs/Design/ApprovedBaseline.md)
- [Hướng mỹ thuật](Docs/ArtDirection/README.md)
- [Mốc M0 và bằng chứng kiểm tra](Docs/M0-Verification.md)

