# M4 — lưu/tải offline, autosave và ba ô thủ công

## Cách mở và thử

Tắt Play → **Qua Thang Nay Da → M4 → Create or Open Saved Morning** → Play.
Scene SavedMorning riêng, giữ nguyên cảnh M0–M3. Build bằng task
**M4: Build Windows (isolated)**; bản chạy ở `Build/Windows-M4/QuaThangNayDa-M4.exe`.

1. Thức dậy, ăn hoặc học vài phút; nhớ giờ, chỉ số và số phút đã học/làm.
2. P mở điện thoại. Trong phần LƯU/TẢI, chọn Lưu ô 1 rồi Xác nhận.
3. Đóng điện thoại, chơi tiếp để giờ/chỉ số thay đổi.
4. P → Tải ô 1 → Xác nhận. Game tải về trạng thái cũ và **tạm dừng**.
5. Bấm Esc hoặc Tiếp tục để chạy lại. Hoạt động đang làm tiếp tục, không bắt đầu lại từ đầu.
6. Thử tương tự khi đang làm thay 9/20 phút, sau khi hoàn tất cam kết và sau khi đã nhận lương.
   Tải lại không được trả thưởng/phạt lại. Có thể lưu cả khi tin cốt truyện chưa chọn.
7. Thử Stop/Play hoặc đóng/mở EXE rồi tải từ điện thoại; không cộng thời gian ngoài đời.
8. Thử đang trên xe, sau đó tải lại: đúng vị trí xe, trạng thái ngồi xe và hướng nhìn;
   game vẫn đứng yên cho tới khi bạn chủ động tiếp tục.

Màn hình tải luôn yêu cầu xác nhận vì thay thế tiến trình hiện tại. Lưu thủ công cũng
yêu cầu xác nhận ghi đè. Điện thoại thường vẫn chạy giờ khi bạn thao tác, trừ khi Esc
hoặc tình huống quan trọng đã dừng mô phỏng.

## Dữ liệu và tính an toàn

- Một autosave (ô 0), ba ô thủ công, mỗi ô có `.bak` gần nhất; định dạng JSON version 1.
- Windows: `%USERPROFILE%/AppData/LocalLow/DefaultCompany/QuaThangNayda/Saves-v1`.
  Dùng Application.persistentDataPath, không lưu trong thư mục EXE hay Git.
- Snapshot chứa giờ/phần phút lẻ, tốc độ giờ, lịch đã khóa, chỉ số, số phút tham gia,
  hoạt động/phần thời lượng còn lại, cờ chốt lịch, cam kết/tiến độ/hạn/trạng thái,
  nhật ký/hộp thư và số tin đã đọc; vị trí/hướng người chơi, ngồi xe, vị trí/hướng/vận tốc xe,
  số va chạm và phần cooldown còn lại. Không dùng thời điểm ngoài đời để tính tiến trình.
- Tải kiểm tra phiên bản, thế giới, giá trị và checksum trước khi áp dụng. Nếu tệp chính
  lỗi thì thử `.bak`; nếu cả hai lỗi, giữ nguyên tiến trình đang chơi và báo lỗi.
- Ghi tệp tạm, flush, đọc kiểm chứng rồi thay tệp chính bằng File.Replace trên Windows.
  Không lấy tệp chính lỗi để ghi đè bản dự phòng tốt. `.tmp` còn sót không được tự tải.
- Checksum chống lỗi dữ liệu vô ý, không phải cơ chế chống người chơi sửa save.
- Autosave sau thay đổi nhật ký ở trạng thái đủ an toàn (xe gần dừng, không đang chọn
  cốt truyện, người chơi không rơi khỏi map), hoặc sau 15 phút game; giới hạn thử mỗi
  5 giây thực. Chỉ bắt đầu sau khi thức dậy; không tự ghi đè ngay khi mở scene/tutorial.
- Không autosave trạng thái kết thúc. Lưu thủ công trạng thái kết thúc vẫn được, để xem tổng kết.
- Snapshot lưu cấu hình của lượt; chỉnh asset cấu hình không sửa lịch trong bản đã lưu.
- Chưa có migration save phiên bản cũ, cloud, tài khoản hoặc mã hóa.

## Kiểm chứng tự động

Build ngày 04/11/2026 thành công: 0 lỗi, 0 cảnh báo build, Unity thoát mã 0.
Log `Logs/M4/build-20261104-201122.log`: các kiểm tra M2, M3 và M4 đều PASS.

SaveChecks chạy cùng MorningChecks và CommitmentChecks trước build M4.
Kiểm tra JSON round-trip giữa bữa ăn, trước chọn nhánh, giữa cam kết, sau hoàn tất,
giữa ca làm, sau trả lương và sau thất bại cam kết. So sánh nhánh được tải với nhánh
chạy liên tục; kiểm tra cờ chốt, phiên bản và snapshot sai bị từ chối.
Kiểm tra ghi thực trên đĩa trong thư mục Logs/M4Checks của project validation riêng:
ghi đè, giữ phút lẻ, cố ý làm hỏng tệp thử, fallback backup, giữ backup tốt khi sửa tệp chính,
bốn ô độc lập và từ chối ghi autosave khi lượt kết thúc. Không sửa tệp lưu người dùng.

## Chờ nghiệm thu trong bản chạy

### Editor bỏ sót script khi import

Editor đang mở báo CS0246 với SaveCoordinator; response file biên dịch có SaveGame,
SaveStore nhưng thiếu SaveCoordinator dù file tồn tại và GUID không trùng. Refresh/
Reimport và cập nhật nội dung chưa giải quyết theo người dùng. Đã chuyển adapter
SaveCoordinator sang Runtime/Activities, giữ GUID và bổ sung metadata MonoImporter;
scene SavedMorning vẫn tham chiếu GUID cũ, không thay dữ liệu lưu. Cần khởi động lại
Editor để xác minh đường dẫn mới được đưa vào danh sách biên dịch. Chưa kết luận
nguyên nhân sâu của việc AssetDatabase bỏ sót file.

- [ ] Bố cục điện thoại/ô lưu phù hợp màn hình, xác nhận/hủy không bấm nhầm.
- [ ] Lưu giữa học/làm/cam kết, tải chính xác và tiếp tục được.
- [ ] Vị trí người chơi/xe, camera, khóa chuột và pause được khôi phục đúng.
- [ ] Tin chưa đọc và tin cốt truyện chưa chọn hiển thị đúng sau tải.
- [ ] Đóng mở EXE vẫn tải được; autosave không mất ở màn kết thúc.
- [ ] Thử phục hồi thực tế từ backup với bản sao save thử, không làm hỏng save thật.

Kiểm tra luật/tệp và build không thay thế các kiểm tra chơi thử này. M3 UI cũng chưa
được người dùng xác nhận toàn bộ; không tuyên bố M3/M4 nghiệm thu đầy đủ.
