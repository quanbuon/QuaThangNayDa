# M3 — điện thoại và cam kết làm thay

## Mở bản thử

Tắt Play, chọn **Qua Thang Nay Da → M3 → Create or Open Phone Story**, rồi Play.
Scene StoryMorning và cấu hình StoryMorningConfig riêng; không ghi đè M2.
Task **M3: Build Windows (isolated)** xuất `Build/Windows-M3/QuaThangNayDa-M3.exe`.

- P mở/đóng điện thoại; biểu tượng hình điện thoại góc trên phải có số tin chưa đọc.
- Khi chuột đang tự do có thể bấm biểu tượng. Khi chuột khóa dùng P.
- Hộp thư lưu thông báo hoạt động, thưởng/phạt, lịch và cam kết; tin mới nhất ở trên.
- Mở điện thoại thường không dừng đồng hồ. Khi đang lái, mở điện thoại khóa điều khiển
  và phanh xe, không cho vừa đọc vừa tăng ga. Esc vẫn tạm dừng game.
- Tin tình huống quan trọng tự mở điện thoại và khóa giờ cho tới khi xác nhận nhánh.
  P không đóng được tin này để tránh bỏ qua quyết định; Esc vẫn dùng được.
- Đây là UI chẩn đoán IMGUI, biểu tượng tạo từ hình chữ nhật trong code, không dùng
  ảnh tải về, tài nguyên trả phí hoặc dịch vụ thông báo ngoài game. Chưa phải UI cuối.

## Đường chơi kiểm tra

1. Bắt đầu buổi sáng, ăn/nghỉ và tới LOP HOC lúc 08:00.
2. Học 10 phút, có thể bấm tiến nhanh: phải dừng chính xác 08:10 và mở tin quản lý.
3. Để nguyên màn hình vài giây: giờ và phút chuyên cần không tăng trong lúc đọc.
4. Chọn một nhánh. Không có tiền/chỉ số thưởng chỉ vì chọn.
   - Ở lại: tiếp tục học thêm 20 phút sau lựa chọn, hoàn thành trước 09:00.
     Cam kết hoàn tất làm quan hệ giảm 2; học tập vẫn tính độc lập theo lịch.
   - Làm thay: rời hoạt động học, tự tới nhân vật capsule QUAN LY gần quán (4,1,12),
     nhìn và nhấn F, xác nhận làm thay, thực hiện đủ 20 phút. Hạn là 45 phút game
     kể từ xác nhận lựa chọn, gồm 25 phút dự phòng di chuyển nếu làm liên tục.
     Hoàn thành +20.000đ/+3 quan hệ. Đây là khoản cam kết, không cộng lần nữa vào ca thường.
5. Thử không thực hiện hoặc dừng giữa chừng: tới hạn, chưa đủ phút thì -4 quan hệ,
   không thưởng. Phút đã học/làm trong lịch vẫn được tính riêng.
6. P xem tiến độ, hạn và trạng thái cam kết, kết quả trong hộp thư.
7. Chơi lại từ Stop/Play để thử nhánh kia. Chưa có lưu/tải (M4).

## Giả định thử và giới hạn

Thời lượng 10/20/45 phút và thưởng/phạt là giá trị thử để cân bằng, chưa phải thiết kế cuối.
Tin chỉ xuất hiện khi đang học và có ít nhất 20 phút còn lại trước kết thúc lớp; nếu vào
quá muộn/bỏ học, báo không kích hoạt khi kết thúc giờ lớp. Chưa có tình huống thay thế.
Quản lý là capsule có tương tác, không phải model/hoạt ảnh/hội thoại nhân vật hoàn chỉnh.
Chỉ một tình huống; chưa có tài khoản, mạng, thông báo hệ điều hành, âm thanh điện thoại hoặc save.
Hộp thư hiện phản chiếu nhật ký, đánh dấu các tin hiện có đã đọc khi mở điện thoại.
Hoạt động học/café vẫn dùng điểm tương tác mẫu, chưa mô phỏng thao tác làm đồ uống.

## Kiểm chứng

Build 04/11/2026: thành công, 0 lỗi và 0 cảnh báo build; Unity thoát mã 0.
Log cục bộ: `Logs/M3/build-20261104-164248.log`. MorningChecks và CommitmentChecks đều PASS.

CommitmentChecks chạy trước build, cùng MorningChecks để kiểm tra không phá luật M2:
ngắt tiến nhanh ở tin quan trọng; đọc tin không trôi giờ; chọn không nhận thưởng;
không đổi nhánh sau xác nhận; đủ phút học mới hoàn thành; phút di chuyển không tính;
hủy/nối việc chỉ cộng phút thực hiện; hạn cam kết; hoàn thành đúng phút cuối;
hết hạn không cho nhận việc; tiền không cộng vào ca thường; thưởng/phạt một lần;
hậu quả bỏ lớp vẫn áp dụng tách biệt.

Chờ chơi thử trong Editor và EXE: icon/P/con trỏ, bố cục ở màn hình nhỏ, tin quan trọng,
cả hai nhánh, gặp quản lý, thông báo kết quả, pause và đóng/mở điện thoại trên xe.
Kiểm tra luật/build không thay thế xác minh UI hoặc thao tác thực tế.
