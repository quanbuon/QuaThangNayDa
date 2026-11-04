# Baseline đã duyệt — 04/11/2026

## Phạm vi

- Windows offline; không tài khoản/multiplayer/dịch vụ trả phí.
- Một buổi sáng 25–35 phút không tính hướng dẫn/tạm dừng.
- Phòng trọ, phố nhỏ, lớp, quán; khoảng ba NPC có vai trò.
- Năm chỉ số, lịch cấu hình và khóa trong lượt; UI nhập tuần để sau.
- Thức dậy → ăn → xem lịch → đi bộ/xe → học/làm → cam kết → tổng kết.
- Tình huống đầu: ở lớp học tiếp hoặc tới quán làm thay. Cả hai phải thực hiện đủ phút.
- Chọn chưa nhận thưởng. Hậu quả lịch và cam kết tách riêng trong nhật ký/tổng kết.
- Hướng dẫn an toàn; đọc tình huống, pause, settings dừng giờ. Điện thoại thường chạy giờ.
- Tiến nhanh xử lý mọi mốc và dừng tại tình huống; báo trước thời lượng/nguy cơ lỡ hẹn.
- Xe arcade quán tính vừa, first person, ít rung, va chạm nhẹ có chống phạt liên tiếp, gỡ kẹt.
- Autosave + ba ô thủ công, bản dự phòng; lưu giữa hoạt động/cam kết; không tính thời gian ngoài đời.
- Kết thúc lượt không ghi đè autosave an toàn.

## Kiến trúc được duyệt

Core C# độc lập Unity → Runtime adapter → cảnh/UI. Cấu hình ScriptableObject tách trạng thái
runtime và DTO lưu. ID ổn định; giao dịch/thưởng/phạt có trạng thái đã chốt.
Bootstrap + StudentQuarter + ArtPreview; một khu liền mạch, chưa streaming.
CharacterController đi bộ; Rigidbody arcade cho xe; camera trạng thái, Input System.
Luật thời gian độc lập vật lý; các bước tiến giờ dùng chung một cơ chế và thứ tự xử lý xác định.

## Mốc

M0 nền công cụ/build/Git/concept; M1 đi bộ/xe; M2 sinh hoạt/lịch;
M3 buổi sáng hai nhánh; M4 save/recovery; M5 khu mỹ thuật mẫu; M6 hoàn thiện và cân bằng.
Không coi việc viết xong script là nghiệm thu; cần chạy được trong Unity và build.

## Để sau

Tuần đầy đủ, nhập lịch cả tuần, bệnh viện, Mini Mart, bóng rổ, giao thông, hỏng/sửa xe,
tay cầm, nền tảng khác. Không tự mở rộng phạm vi.

## Hiệu năng và môi trường

Máy đã đọc: Windows 10 x64, Ryzen 7 3750H (4C/8T), RAM khoảng 16GB, GTX1650 4GB,
Vega10 tích hợp. Editor.log xác nhận NVIDIA GTX1650/DX12. Unity 6000.6.8f1, URP17.6.0.
Mục tiêu thiết kế 1080p Medium hướng tới 60FPS, có Low/30FPS; chưa là kết quả đo gameplay
hay cấu hình tối thiểu. Giữ phiên bản hiện có.

## Giả định cần cân bằng bằng playtest

Giờ buổi sáng, tốc độ đồng hồ, thời lượng học/làm, hạn cam kết, thưởng/phạt, ngưỡng kết thúc.
Không mặc định các con số của demo web. Bảng hình cần người dùng duyệt trước cảnh chính thức.
