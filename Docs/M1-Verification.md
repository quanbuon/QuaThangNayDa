# M1 — khu thử đi bộ và xe máy

## Cách mở

Ngắt debugger nếu đang dừng, tắt Play Mode. Trong Unity chọn
**Qua Thang Nay Da → M1 → Create or Open Travel Test**, rồi Play.
Menu tạo scene riêng nếu chưa có, không ghi đè scene đã có hoặc SampleScene/Bootstrap.

WASD di chuyển; chuột nhìn; E lên/xuống xe khi cách dưới 3m;
Space phanh; R về điểm an toàn; Esc tạm dừng/thả chuột.
Khi cửa sổ mất focus, game tạm dừng; nhấn Tiếp tục để khóa chuột lại.

Xe và nhà là hình khối thử nghiệm, không phải model chính thức. Có lùi chậm bằng S.
Camera không lắc/roll. R hiện dùng điểm cố định, chưa chọn điểm gần nhất trên đường.
Va chạm hiển thị phản hồi và đếm với cooldown 2 giây; chưa tác động chỉ số gameplay.

## Trạng thái kiểm chứng

- [x] Unity compile/build Windows thành công: 04/11/2026, errors=0, warnings=0, Unity thoát mã 0.
- [ ] Đi bộ và nhìn được, không xuyên sàn/tường, di chuyển chéo không nhanh hơn.
- [ ] E lên xe gần, không lên xuyên tường; camera chuyển về ghế và nhìn được tay lái.
- [ ] W tăng tốc, S lùi chậm, Space phanh, A/D rẽ; cảm giác điều khiển dễ làm quen.
- [ ] Không xuống khi xe đang chạy; xuống trái/phải chỉ khi không bị vật cản.
- [ ] Va chạm có phản hồi, không đếm dồn dập; R đưa xe về vị trí an toàn.
- [ ] Esc/mất focus dừng xe, tiếp tục chạy bình thường; rời Play không để timeScale bằng 0.
- [ ] Bản EXE có hình và điều khiển tương đương Editor.

Chưa nghiệm thu tương tác bằng việc chỉ viết xong C#. Cần chơi thử trong Editor
và standalone; chưa đo hiệu năng gameplay hoặc khẳng định đạt mục tiêu 60 FPS.
Build nằm ở `Build/Windows-M1/QuaThangNayDa-M1.exe`; log cục bộ:
`Logs/M1/build-20261104-151612.log`. Chưa xác minh hình ảnh hoặc cảm giác lái M1.

## Tổ chức

- TravelInput: Input System actions, tách binding khỏi logic di chuyển.
- TravelController: CharacterController, camera góc nhìn thứ nhất và chuyển trạng thái lên/xuống xe.
- ArcadeScooter: Rigidbody, gia tốc/phanh/rẽ, va chạm và hồi phục.
- TravelTools: menu dựng scene thử riêng và entry point build.

HUD đang là IMGUI chẩn đoán để kiểm tra điều khiển, sẽ thay bằng UI gameplay.
Lịch, chỉ số, nhiệm vụ và lưu game chưa nằm trong M1.
