# Qua Tháng Này Đã — Unity

Chuyển thể theo hướng B từ demo web 0.14. Windows offline, góc nhìn thứ nhất,
stylized bán hiện thực. Bản đầu là một buổi sáng 25–35 phút, có xe máy và hai nhánh cam kết.

## Hiện trạng

M0 đã build Windows thành công và chạy được kiểm tra thông số GPU/scene ở 1080p.
Chưa nghiệm thu hình ảnh: chụp tự động trong cửa sổ ẩn bị lỗi/ảnh đen; cần mở bản chạy
trực tiếp để kiểm tra. Nút tương tác và breakpoint cũng chưa được xác minh.
Chưa có gameplay, lưu tiến trình hay cảnh mỹ thuật chính thức.
Không sử dụng model từ ZIP tham khảo khi chưa kiểm tra nguồn và quyền sử dụng.

## Mở và chạy M0

1. Mở project bằng Unity **6000.6.8f1**.
2. Chờ compile, chọn **Qua Thang Nay Da → M0 → Create or Open Foundation Scene**.
3. Bấm Play. Bảng M0 cho biết GPU/API/độ phân giải, có nút DEBUG PROBE.
4. Để debug: mở [FoundationDiagnostics.cs](Assets/_Project/Scripts/Runtime/Bootstrap/FoundationDiagnostics.cs),
   đặt breakpoint trong `DebugProbe`, chọn **Attach to Unity** ở VS Code rồi bấm nút trong game.

Nền/cube chỉ dùng kiểm tra render, không phải cảnh chính thức. SampleScene được giữ nguyên.
M0 dùng IMGUI cho bảng chẩn đoán; HUD gameplay về sau dùng uGUI/TextMeshPro.

## Build Windows

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

